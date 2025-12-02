# ImGui.NET Implementation Guide for Stride

## Executive Summary

This document outlines the complete requirements for implementing ImGui.NET in Stride. After investigation, **implementing ImGui.NET requires 12-20 hours of specialized work** involving:

1. Custom HLSL shader compilation
2. Low-level graphics API integration
3. Complex vertex/index buffer management
4. Input system mapping
5. Font texture atlas handling

## Current Status

✅ **Working Myra Implementation**
- Draggable windows
- Collapsible tree hierarchy
- Property editors with real-time updates
- 3D world entity picking
- Fixed-size windows (not resizable)

## Why ImGui is Complex for Stride

Unlike game engines like Unity or Unreal that have established ImGui integrations, Stride requires:

### 1. Custom Shader Compilation
```hlsl
// Required: ImGui vertex/pixel shader in Stride's .sdsl format
shader ImGuiShader : ShaderBase
{
    cbuffer PerFrame
    {
        float4x4 ProjectionMatrix;
    };

    stage stream float4 Position : POSITION;
    stage stream float2 TexCoord : TEXCOORD0;
    stage stream float4 Color : COLOR;

    stage override void VSMain()
    {
        streams.ShadingPosition = mul(ProjectionMatrix, streams.Position);
    }

    stage override void PSMain()
    {
        streams.ColorTarget = Texture0.Sample(Sampler0, streams.TexCoord) * streams.Color;
    }
};
```

**Complexity**: Requires Stride's shader compilation pipeline, not standard HLSL

### 2. Graphics Pipeline Integration

```csharp
public class ImGuiRenderer
{
    // Required components:
    - VertexBufferBinding with custom vertex layout
    - IndexBuffer management  
    - DynamicBuffer allocation (resize on demand)
    - PipelineState configuration
    - BlendState for alpha blending
    - RasterizerState with scissor test
    - DepthStencilState (disabled)
    - Texture atlas for fonts
    - Sampler state
    - Constant buffer for projection matrix
}
```

**Complexity**: 500+ lines of low-level graphics code

### 3. Input System Mapping

ImGui expects specific input format:
```csharp
- Mouse position in screen coordinates
- Mouse button states (5 buttons)
- Mouse wheel delta
- Keyboard key states (512 keys)
- Keyboard modifiers (Ctrl, Shift, Alt)
- Text input events (UTF-16 characters)
- Key mapping for ImGui navigation
```

**Complexity**: Medium - mostly straightforward but tedious

### 4. Font Texture Atlas

```csharp
- Load ImGui's default font
- Generate RGBA32 texture atlas
- Upload to GPU
- Handle font scaling
- Support custom fonts
```

**Complexity**: Low-Medium

## Alternative Solutions

### Solution 1: Use Existing ImGui.NET.Stride Package (RECOMMENDED)

**Status**: Check NuGet for `ImGui.NET.Stride` or similar packages

```bash
dotnet search ImGui Stride
```

If found, integration becomes:
```csharp
// 1. Add package
<PackageReference Include="ImGui.NET.Stride" Version="X.X.X" />

// 2. Use in code  
var imguiRenderer = new StrideImGuiRenderer(GraphicsDevice);
// Done!
```

**Time Required**: 30 minutes  
**Success Rate**: High (if package exists)

### Solution 2: Fork Myra and Add Resize (RECOMMENDED if no package)

**Effort**: 4-8 hours  
**Benefits**:
- Keep working C# codebase
- Add exactly the features you need
- Contribute back to open source

**Steps**:
1. Fork Myra repository
2. Add resize handle widget
3. Implement mouse drag resize logic
4. Test with Stride
5. Submit PR to Myra project

### Solution 3: Enlarge Myra Windows (QUICK WIN)

**Effort**: 2 minutes  
**Benefits**:
- Works immediately
- No new dependencies
- Same familiar UI

```csharp
_sceneHierarchyWindow = new Window
{
    Title = "Scene Hierarchy",
    Left = 10,
    Top = 10,
    Width = 600,  // Was 300
    Height = 800, // Was 500
    Content = hierarchyScroll
};
```

### Solution 4: Full ImGui.NET Implementation (ADVANCED)

**Effort**: 12-20 hours  
**Requirements**:
- Deep Stride graphics API knowledge
- HLSL shader expertise  
- Understanding of immediate-mode GUI paradigm
- Patience for debugging graphics issues

**Implementation Checklist**:

#### Phase 1: Setup (2-3 hours)
- [ ] Add ImGui.NET package to Directory.Packages.props
- [ ] Create ImGuiRenderer class structure
- [ ] Initialize ImGui context
- [ ] Set up basic font texture

#### Phase 2: Shader Development (4-6 hours)
- [ ] Write ImGui vertex shader (.sdsl)
- [ ] Write ImGui pixel shader (.sdsl)
- [ ] Compile shaders using Stride's EffectCompiler
- [ ] Create EffectBytecode
- [ ] Set up PipelineState with all render states

#### Phase 3: Rendering (3-5 hours)
- [ ] Implement vertex buffer management
- [ ] Implement index buffer management
- [ ] Handle dynamic buffer resizing
- [ ] Implement draw call batching
- [ ] Set up scissor rectangle handling
- [ ] Implement texture binding

#### Phase 4: Input (2-3 hours)
- [ ] Map Stride keyboard to ImGui
- [ ] Map Stride mouse to ImGui
- [ ] Handle text input events
- [ ] Implement clipboard support (optional)

#### Phase 5: Testing & Debug (2-3 hours)
- [ ] Test with simple windows
- [ ] Test with complex UI (trees, tables)
- [ ] Debug rendering artifacts
- [ ] Optimize performance

## Recommended Action Plan

### Option A: Quick Solution (2 minutes)
1. Enlarge Myra windows to 600x800
2. Continue development
3. Revisit later if truly needed

### Option B: Community Package (30 mins - 2 hours)
1. Search for existing ImGui.NET.Stride integration
2. If found, integrate it
3. If not found, use Option A or C

### Option C: Long-term Solution (4-8 hours)
1. Fork Myra
2. Implement resize handles
3. Test thoroughly
4. Use in project

### Option D: Full Custom (12-20 hours)
1. Only if you need ImGui-specific features
2. Budget significant development time
3. Be prepared for graphics debugging

## Code Skeleton for Full ImGui Implementation

If you choose Option D, here's the structure you'll need:

```csharp
// File: ImGuiRenderer.cs (500-700 lines)
public class ImGuiRenderer : IDisposable
{
    // Core fields
    private IntPtr _context;
    private GraphicsDevice _device;
    private Effect _effect;
    private PipelineState _pipeline;
    private Buffer _vertexBuffer;
    private Buffer _indexBuffer;
    private Texture _fontTexture;
    
    // Methods to implement
    public void Initialize();
    public void Update(float deltaTime);
    public void Render(CommandList cmdList);
    public void UpdateInput(InputManager input);
    private void CreateFontTexture();
    private void CreateShaders();
    private void CreatePipeline();
    private void RenderDrawData(ImDrawDataPtr data);
    public void Dispose();
}

// File: ImGuiSceneRenderer.cs (50-100 lines)
public class ImGuiSceneRenderer : SceneRendererBase
{
    private ImGuiRenderer _renderer;
    
    protected override void InitializeCore();
    protected override void DrawCore(RenderContext ctx, RenderDrawContext drawCtx);
}

// File: ImGuiShader.sdsl (50-100 lines)
shader ImGuiShader : ShaderBase
{
    // Vertex/pixel shader implementation
}
```

## Conclusion

**Bottom Line**: ImGui.NET integration is technically possible but requires significant specialized effort.

**Recommendation**: 
1. **Short term**: Enlarge Myra windows (2 min fix)
2. **Medium term**: Check for community ImGui package
3. **Long term**: Fork Myra and add resize feature

The current Myra implementation is **95% there** - it just lacks window resizing. Adding that one feature to Myra is **much easier** than implementing all of ImGui from scratch.

## Questions to Consider

Before committing to ImGui:

1. **Do you really need resizable windows?**
   - Or would larger fixed windows work?

2. **Do you need docking?**
   - Myra doesn't support it
   - ImGui does

3. **Do you need ImGui-specific widgets?**
   - Color pickers, plots, etc.
   - Myra has most common controls

4. **Is this a learning project?**
   - ImGui implementation = great learning
   - Myra = get work done faster

5. **Timeline?**
   - Need it now? → Enlarge Myra
   - Have 20 hours? → Implement ImGui
   - Have 8 hours? → Fork Myra

## Next Steps

Please indicate which path you'd like to take:

1. ✅ **Enlarge Myra windows** (2 min) - I can do this now
2. 🔍 **Search for ImGui package** (help you search)
3. 🔧 **Fork Myra guide** (detailed instructions)
4. 💪 **Full ImGui implementation** (commit to 12-20 hours, I'll guide you)

I recommend **Option 1** to unblock you immediately, with Option 3 as a follow-up project.
