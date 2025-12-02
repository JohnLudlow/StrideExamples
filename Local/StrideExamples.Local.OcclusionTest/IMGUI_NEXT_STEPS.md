# Scene Editor with ImGui.NET - Implementation Plan

## Current Status (Session End)

### ✅ Completed
1. **Working Myra Scene Editor** - Fully functional with:
   - Draggable windows (Scene Hierarchy + Properties)
   - Collapsible tree view for entity hierarchy
   - Real-time property editing (Position, Rotation, Scale)
   - 3D world entity picking via raycasting
   - Component list display
   - Fixed-size windows (not resizable)

2. **Documentation Created**:
   - `UI_FRAMEWORK_COMPARISON.md` - Comparison of UI frameworks
   - `IMGUI_IMPLEMENTATION_GUIDE.md` - Complete ImGui implementation guide
   - `README_SCENE_EDITOR.md` - Scene editor user guide

3. **Investigation Completed**:
   - Researched ImGui.NET integration requirements
   - Analyzed complexity: 12-20 hours of specialized work
   - Identified key challenges (shader compilation, graphics pipeline)
   - Explored alternative solutions

### 📂 Project Structure

```
Local/StrideExamples.Local.OcclusionTest/
├── Program.cs                          # Main entry point (using Myra)
├── UI/
│   ├── MyraSceneRenderer.cs           # Myra renderer integration
│   ├── SceneEditorView.cs             # Scene editor UI (Myra-based)
│   └── (ImGui files will go here)
├── Components/
│   └── MultiRaycastVisibilityComponent.cs
├── Managers/
│   ├── GameManager.cs
│   └── UIManager.cs
├── README_SCENE_EDITOR.md             # User guide
├── UI_FRAMEWORK_COMPARISON.md         # Framework analysis
└── IMGUI_IMPLEMENTATION_GUIDE.md      # This implementation plan
```

## Next Steps: ImGui.NET Implementation

### Phase 1: Setup & Preparation (1-2 hours)

#### 1.1 Add ImGui.NET Package
Edit `Directory.Packages.props` in repository root:
```xml
<ItemGroup>
  <PackageVersion Include="ImGui.NET" Version="1.90.0" />
  <!-- Add these if needed for rendering -->
  <PackageVersion Include="System.Runtime.CompilerServices.Unsafe" Version="6.0.0" />
</ItemGroup>
```

Then update `StrideExamples.Local.OcclusionTest.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="ImGui.NET" />
  <PackageReference Include="System.Runtime.CompilerServices.Unsafe" />
</ItemGroup>
```

Enable unsafe code:
```xml
<PropertyGroup>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

#### 1.2 Search for Existing Packages
Before implementing from scratch, search for:
```bash
# Check NuGet
dotnet search ImGui Stride

# Check GitHub
# Search: "ImGui.NET Stride" or "ImGui Stride integration"
```

**If package found**: Skip to Phase 6, use existing integration  
**If not found**: Continue with custom implementation

### Phase 2: Shader Development (4-6 hours)

#### 2.1 Create ImGui Shader (.sdsl file)
File: `Local/StrideExamples.Local.OcclusionTest/Effects/ImGuiShader.sdsl`

```hlsl
shader ImGuiShader : ShaderBase, Texturing
{
    // Constant buffer for projection matrix
    cbuffer PerFrame
    {
        stage float4x4 ProjectionMatrix;
    };

    // Vertex shader streams
    stage stream float4 Position : POSITION;
    stage stream float2 TexCoord : TEXCOORD0;
    stage stream float4 Color : COLOR;

    stage override void VSMain()
    {
        streams.ShadingPosition = mul(float4(streams.Position.xy, 0, 1), ProjectionMatrix);
        streams.TexCoord = streams.TexCoord;
        streams.Color = streams.Color;
    }

    stage override void PSMain()
    {
        float4 texColor = Texture0.Sample(Sampler0, streams.TexCoord);
        streams.ColorTarget = texColor * streams.Color;
    }
};
```

#### 2.2 Compile Shader
- Use Stride Game Studio to compile shader
- OR use EffectCompilerApp.exe from Stride SDK
- Generate .sdeffect bytecode file

**Reference**: Look at existing shader compilation in `MeshOutlineShader.sdsl` in Community examples

### Phase 3: ImGui Renderer Implementation (3-5 hours)

#### 3.1 Create ImGuiRenderer.cs
File: `Local/StrideExamples.Local.OcclusionTest/UI/ImGuiRenderer.cs`

**Key Components**:
```csharp
public class ImGuiRenderer : IDisposable
{
    private IntPtr _context;
    private GraphicsDevice _graphicsDevice;
    private InputManager _inputManager;
    
    // Graphics resources
    private Effect _effect;
    private PipelineState _pipelineState;
    private Buffer _vertexBuffer;
    private Buffer _indexBuffer;
    private Texture _fontTexture;
    private SamplerState _samplerState;
    
    // Buffer sizes (grow as needed)
    private int _vertexBufferSize = 10000;
    private int _indexBufferSize = 20000;
    
    // Methods to implement
    public ImGuiRenderer(GraphicsDevice device, InputManager input);
    public void Update(float deltaTime);
    public void Render(CommandList commandList);
    private void CreateFontTexture();
    private void CreateRenderResources();
    private void UpdateInput();
    private unsafe void RenderDrawData(ImDrawDataPtr drawData);
    public void Dispose();
}
```

**Critical Implementation Details**:

1. **Vertex Format**:
```csharp
[StructLayout(LayoutKind.Sequential)]
struct ImGuiVertex
{
    public Vector2 Position;
    public Vector2 UV;
    public uint Color;
}
```

2. **Pipeline State**:
```csharp
var pipelineDesc = new PipelineStateDescription
{
    BlendState = BlendStates.AlphaBlend,
    RasterizerState = new RasterizerStateDescription 
    { 
        CullMode = CullMode.None,
        ScissorTestEnable = true 
    },
    DepthStencilState = DepthStencilStates.None,
    InputElements = new VertexElement[]
    {
        VertexElement.Position<Vector2>(),
        VertexElement.TextureCoordinate<Vector2>(),
        VertexElement.Color<Color>()
    },
    PrimitiveType = PrimitiveType.TriangleList,
    EffectBytecode = _effect.Bytecode,
    RootSignature = _effect.RootSignature,
    Output = new RenderOutputDescription(PixelFormat.R8G8B8A8_UNorm)
};
```

3. **Font Texture Creation**:
```csharp
var io = ImGui.GetIO();
io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height);

_fontTexture = Texture.New2D(
    _graphicsDevice, 
    width, 
    height, 
    PixelFormat.R8G8B8A8_UNorm,
    TextureFlags.ShaderResource);
    
// Copy pixel data
var pixelData = new byte[width * height * 4];
Marshal.Copy(pixels, pixelData, 0, pixelData.Length);
_fontTexture.SetData(_graphicsDevice.ImmediateCommandList, pixelData);

io.Fonts.SetTexID((IntPtr)1);
io.Fonts.ClearTexData();
```

### Phase 4: Scene Renderer Integration (1-2 hours)

#### 4.1 Create ImGuiSceneRenderer.cs
File: `Local/StrideExamples.Local.OcclusionTest/UI/ImGuiSceneRenderer.cs`

```csharp
public class ImGuiSceneRenderer : SceneRendererBase
{
    private ImGuiRenderer? _imguiRenderer;
    private ImGuiSceneEditorView? _sceneEditorView;

    protected override void InitializeCore()
    {
        base.InitializeCore();
        
        var game = Services.GetService<IGame>() as Game;
        _imguiRenderer = new ImGuiRenderer(GraphicsDevice, game.Input);
        _sceneEditorView = new ImGuiSceneEditorView();
        
        Services.AddService(_sceneEditorView);
    }

    protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
    {
        var game = Services.GetService<IGame>() as Game;
        _imguiRenderer?.Update((float)game.UpdateTime.Elapsed.TotalSeconds);
        
        _sceneEditorView?.Draw();
        
        _imguiRenderer?.Render(drawContext.CommandList);
    }

    protected override void Destroy()
    {
        _imguiRenderer?.Dispose();
        base.Destroy();
    }
}
```

### Phase 5: Scene Editor UI (2-3 hours)

#### 5.1 Create ImGuiSceneEditorView.cs
File: `Local/StrideExamples.Local.OcclusionTest/UI/ImGuiSceneEditorView.cs`

**Key Features**:
```csharp
public class ImGuiSceneEditorView
{
    private Scene? _scene;
    private Entity? _selectedEntity;
    
    public void Draw()
    {
        DrawSceneHierarchy();
        DrawPropertiesPanel();
    }
    
    private void DrawSceneHierarchy()
    {
        // ✅ Resizable window!
        ImGui.Begin("Scene Hierarchy");
        
        // Tree with proper expand/collapse
        foreach (var entity in _scene.Entities)
        {
            if (entity.Transform.Parent == null)
                DrawEntityNode(entity);
        }
        
        ImGui.End();
    }
    
    private void DrawEntityNode(Entity entity)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (_selectedEntity == entity)
            flags |= ImGuiTreeNodeFlags.Selected;
            
        var hasChildren = entity.Transform.Children.Count > 0;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;
            
        var isOpen = ImGui.TreeNodeEx(entity.Name, flags);
        
        if (ImGui.IsItemClicked())
            _selectedEntity = entity;
            
        if (isOpen && hasChildren)
        {
            foreach (var child in entity.Transform.Children)
                DrawEntityNode(child.Entity);
            ImGui.TreePop();
        }
    }
    
    private void DrawPropertiesPanel()
    {
        ImGui.Begin("Properties");
        
        if (_selectedEntity == null)
        {
            ImGui.Text("No entity selected");
        }
        else
        {
            ImGui.Text($"Entity: {_selectedEntity.Name}");
            
            // Transform properties with DragFloat
            var pos = _selectedEntity.Transform.Position;
            var posVec = new Vector3(pos.X, pos.Y, pos.Z);
            
            if (ImGui.DragFloat3("Position", ref posVec, 0.1f))
            {
                _selectedEntity.Transform.Position = 
                    new Stride.Core.Mathematics.Vector3(posVec.X, posVec.Y, posVec.Z);
            }
            
            // Similar for rotation, scale
        }
        
        ImGui.End();
    }
}
```

### Phase 6: Integration & Testing (2-3 hours)

#### 6.1 Update Program.cs
Replace Myra with ImGui:
```csharp
// In Start() method
var compositor = game.SceneSystem.GraphicsCompositor;
if (compositor != null)
{
    var gameCompositor = compositor.Game as SceneRendererCollection;
    if (gameCompositor != null)
    {
        var imguiRenderer = new ImGuiSceneRenderer(); // Changed from MyraSceneRenderer
        gameCompositor.Children.Add(imguiRenderer);
    }
}

// In Update() method
var sceneEditor = game.Services.GetService<ImGuiSceneEditorView>(); // Changed type
if (sceneEditor != null && !sceneEditor.IsInitialized)
{
    sceneEditor.Initialize(rootScene, game);
}
sceneEditor?.Update(game);
```

#### 6.2 Testing Checklist
- [ ] ImGui context initializes
- [ ] Font texture loads correctly
- [ ] Windows appear on screen
- [ ] Windows are resizable
- [ ] Scene hierarchy populates
- [ ] Tree nodes expand/collapse
- [ ] Entity selection works
- [ ] Properties panel updates
- [ ] DragFloat controls work
- [ ] Values update in real-time
- [ ] 3D picking still works
- [ ] No memory leaks
- [ ] Performance is acceptable

### Phase 7: Polish & Features (2-4 hours)

#### 7.1 Additional Features
- [ ] Docking support (ImGui.DockSpace)
- [ ] Context menus (right-click entity)
- [ ] Entity creation/deletion
- [ ] Component add/remove
- [ ] Color pickers for materials
- [ ] Save/load window layouts
- [ ] Keyboard shortcuts
- [ ] Search/filter entities

#### 7.2 Optimization
- [ ] Batch similar entities in hierarchy
- [ ] Lazy-load deep hierarchies
- [ ] Cache expanded state
- [ ] Optimize property refresh

## Troubleshooting Guide

### Common Issues

#### 1. Shader Compilation Fails
**Solution**: Use Stride Game Studio to compile .sdsl files, or check EffectCompiler output

#### 2. Nothing Renders
**Checklist**:
- ImGui context created?
- Font texture uploaded?
- Vertex/index buffers allocated?
- Pipeline state configured?
- Draw data has vertices?

#### 3. Input Not Working
**Check**:
- Key mapping correct?
- Mouse position scaled properly?
- Text input events wired up?

#### 4. Performance Issues
**Optimize**:
- Reduce draw calls
- Batch UI elements
- Cache frequently accessed data
- Profile with Stride profiler

## Resources

### Reference Projects
1. **MeshOutlineShader** (in Community folder)
   - Example of custom .sdsl shader
   - Shows shader compilation process

2. **MyraUI** (in Local folder)
   - Current working Myra implementation
   - UI renderer pattern

### Documentation
- ImGui.NET: https://github.com/mellinoe/ImGui.NET
- Dear ImGui: https://github.com/ocornut/imgui
- Stride Docs: https://doc.stride3d.net/latest/en/

### Getting Help
- Stride Discord: https://discord.gg/f6aerfE
- Stride Forums: https://forums.stride3d.net/
- Stack Overflow: Tag `stride3d`

## Success Criteria

### Minimum Viable Product
- ✅ Resizable windows
- ✅ Scene hierarchy tree
- ✅ Property editing (Position, Rotation, Scale)
- ✅ Entity selection

### Full Feature Set
- ✅ All MVP features
- ✅ Docking support
- ✅ Context menus
- ✅ Component management
- ✅ Search/filter
- ✅ Keyboard shortcuts

## Time Estimate

| Phase | Optimistic | Realistic | Pessimistic |
|-------|-----------|-----------|-------------|
| Setup | 1h | 2h | 3h |
| Shaders | 3h | 5h | 8h |
| Renderer | 2h | 4h | 6h |
| Integration | 1h | 2h | 3h |
| Scene Editor | 2h | 3h | 4h |
| Testing | 1h | 2h | 4h |
| Polish | 2h | 3h | 5h |
| **Total** | **12h** | **21h** | **33h** |

## Decision Point

Before starting, consider:

1. **Do you need resizable windows badly enough** to invest 12-21 hours?
2. **Could you live with larger fixed windows** (2-minute fix)?
3. **Is there a Stride ImGui package** you haven't found yet?
4. **Would forking Myra** (4-8 hours) be better than custom ImGui?

## Next Session Checklist

When you resume:

1. ✅ Branch `scene_editor` checked out
2. ✅ Read this README
3. ✅ Review `IMGUI_IMPLEMENTATION_GUIDE.md`
4. ✅ Check for ImGui.NET.Stride package first
5. ⏸️ If no package, start Phase 1: Setup

Good luck! 🚀
