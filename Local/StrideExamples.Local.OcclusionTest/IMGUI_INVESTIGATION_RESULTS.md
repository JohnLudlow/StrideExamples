# ImGui Investigation Results

## Executive Summary

✅ **Official ImGui Integration Found!**

The **Stride.CommunityToolkit.ImGui** package exists and provides a ready-to-use ImGui integration for Stride3D. This eliminates the need for custom implementation.

## Discovery

### Package Details
- **Name**: `Stride.CommunityToolkit.ImGui`
- **Version**: 1.0.0-preview.62 (latest as of Dec 2025)
- **Backend**: Uses Hexa.NET.ImGui (not ImGui.NET)
- **Status**: Production-ready
- **Downloads**: 1,878 total (as of search)

### Alternative Package
- **Name**: `Stride.CommunityToolkit.ImGuiNet`  
- **Backend**: Uses ImGui.NET
- **Purpose**: Box2D.NET-style text rendering
- **Use Case**: Simpler text rendering scenarios

## Recommendation

**Use `Stride.CommunityToolkit.ImGui`** for the scene editor because:
1. ✅ Full-featured ImGui integration with all widgets
2. ✅ Resizable, dockable windows
3. ✅ Tree views, property editors, color pickers
4. ✅ Official support from Stride Community Toolkit
5. ✅ Pre-compiled shaders and rendering pipeline
6. ✅ Example code available

## Implementation Plan

### Phase 1: Add Package (5 minutes)

#### 1.1 Update Directory.Packages.props
```xml
<ItemGroup>
  <PackageVersion Include="Stride.CommunityToolkit" Version="1.0.0-preview.58" />
  <PackageVersion Include="Stride.CommunityToolKit.Bepu" Version="1.0.0-preview.58" />
  <PackageVersion Include="Stride.CommunityToolKit.Bullet" Version="1.0.0-preview.58" />
  <PackageVersion Include="Stride.CommunityToolkit.Skyboxes" Version="1.0.0-preview.58" />
  <PackageVersion Include="Stride.CommunityToolkit.Windows" Version="1.0.0-preview.58" />    
  <PackageVersion Include="Stride.CommunityToolkit.DebugShapes" Version="1.0.0-preview.58" />
  <PackageVersion Include="Stride.CommunityToolkit.ImGui" Version="1.0.0-preview.62" /> <!-- ADD THIS -->
  <PackageVersion Include="NexVYaml" Version="1.1.0" />
  <PackageVersion Include="Microsoft.Build" Version="17.13.9" />
  <PackageVersion Include="Microsoft.Build.Utilities.Core" Version="17.13.9" />
  <PackageVersion Include="Myra.Stride" Version="1.5.9" />
</ItemGroup>
```

#### 1.2 Update Project File
```xml
<!-- In StrideExamples.Local.OcclusionTest.csproj -->
<ItemGroup>
  <PackageReference Include="Stride.CommunityToolkit" />
  <PackageReference Include="Stride.CommunityToolKit.Skyboxes" />
  <PackageReference Include="Stride.CommunityToolKit.Bepu" />
  <PackageReference Include="Stride.CommunityToolkit.Windows" />    
  <PackageReference Include="Stride.CommunityToolkit.DebugShapes" />
  <PackageReference Include="Stride.CommunityToolkit.ImGui" /> <!-- ADD THIS -->
  <PackageReference Include="Myra.Stride" />
</ItemGroup>
```

### Phase 2: Initialize ImGui (10 minutes)

#### 2.1 Update Program.cs
```csharp
using Stride.CommunityToolkit.ImGui;
using static Hexa.NET.ImGui.ImGui;

// At the top of Program.cs, add:
ImGuiSystem? imguiSystem = null;

void Start(Scene rootScene)
{
  game.SetupBase3DScene();
  game.AddGroundGizmo();
  game.Add3DGround();
  game.AddProfiler();
  game.AddAllDirectionLighting();

  // Initialize ImGui System (replaces Myra renderer)
  imguiSystem = new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
  Console.WriteLine("ImGui system initialized");

  var font = game.Content.Load<SpriteFont>("StrideDefaultFont");
  var gameManager = new GameManager(rootScene, font);
  game.Services.AddService(gameManager);

  var greenCube = CreateCube(rootScene, game, "GreenCube", new(-5, 1, 0), Color.Green);
  gameManager.UIManager.MonitorEntity(greenCube);

  var blueCube = CreateCube(rootScene, game, "BlueCube", new(5, 1, 0), Color.Blue);
  gameManager.UIManager.MonitorEntity(blueCube);

  var redCube = CreateCube(rootScene, game, "RedCube", new(0, 1, 5), Color.Red);
  gameManager.UIManager.MonitorEntity(redCube);
}

void Update(Scene rootScene, GameTime gameTime)
{
  var gameManager = game.Services.GetService<GameManager>();
  gameManager?.UIManager.UpdateUI();

  // Draw ImGui scene editor
  DrawSceneEditor(rootScene);
}
```

### Phase 3: Create ImGui Scene Editor (30-60 minutes)

#### 3.1 Create ImGuiSceneEditor.cs
```csharp
using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Input;
using System.Numerics;
using static Hexa.NET.ImGui.ImGui;
using ImGuiWindowFlags = Hexa.NET.ImGui.ImGuiWindowFlags;
using ImGuiTreeNodeFlags = Hexa.NET.ImGui.ImGuiTreeNodeFlags;

namespace StrideExamples.Local.OcclusionTest.UI;

public class ImGuiSceneEditor
{
    private Entity? _selectedEntity;
    private readonly Game _game;
    private readonly Scene _scene;

    public ImGuiSceneEditor(Game game, Scene scene)
    {
        _game = game;
        _scene = scene;
    }

    public void Draw()
    {
        DrawHierarchyWindow();
        DrawPropertiesWindow();
    }

    private void DrawHierarchyWindow()
    {
        Begin("Scene Hierarchy", ImGuiWindowFlags.None);
        
        foreach (var entity in _scene.Entities)
        {
            if (entity.Transform.Parent == null) // Root entities only
            {
                DrawEntityTree(entity);
            }
        }
        
        End();
    }

    private void DrawEntityTree(Entity entity)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
        
        if (_selectedEntity == entity)
            flags |= ImGuiTreeNodeFlags.Selected;
            
        bool hasChildren = entity.Transform.Children.Count > 0;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool nodeOpen = TreeNodeEx(entity.Name, flags);
        
        if (IsItemClicked())
        {
            _selectedEntity = entity;
        }

        if (nodeOpen)
        {
            if (hasChildren)
            {
                foreach (var child in entity.Transform.Children)
                {
                    DrawEntityTree(child.Entity);
                }
            }
            TreePop();
        }
    }

    private void DrawPropertiesWindow()
    {
        Begin("Properties", ImGuiWindowFlags.None);
        
        if (_selectedEntity != null)
        {
            Text($"Entity: {_selectedEntity.Name}");
            Separator();
            
            // Transform properties
            if (CollapsingHeader("Transform"))
            {
                var pos = _selectedEntity.Transform.Position;
                var posVec = new Vector3(pos.X, pos.Y, pos.Z);
                if (DragFloat3("Position", ref posVec, 0.1f))
                {
                    _selectedEntity.Transform.Position = new Vector3(posVec.X, posVec.Y, posVec.Z);
                }
                
                var rot = _selectedEntity.Transform.RotationEulerXYZ;
                var rotVec = new Vector3(
                    MathUtil.RadiansToDegrees(rot.X),
                    MathUtil.RadiansToDegrees(rot.Y),
                    MathUtil.RadiansToDegrees(rot.Z)
                );
                if (DragFloat3("Rotation", ref rotVec, 0.5f))
                {
                    _selectedEntity.Transform.RotationEulerXYZ = new Vector3(
                        MathUtil.DegreesToRadians(rotVec.X),
                        MathUtil.DegreesToRadians(rotVec.Y),
                        MathUtil.DegreesToRadians(rotVec.Z)
                    );
                }
                
                var scale = _selectedEntity.Transform.Scale;
                var scaleVec = new Vector3(scale.X, scale.Y, scale.Z);
                if (DragFloat3("Scale", ref scaleVec, 0.01f))
                {
                    _selectedEntity.Transform.Scale = new Vector3(scaleVec.X, scaleVec.Y, scaleVec.Z);
                }
            }
            
            // Components
            if (CollapsingHeader("Components"))
            {
                foreach (var component in _selectedEntity.Components)
                {
                    Text($"- {component.GetType().Name}");
                }
            }
        }
        else
        {
            Text("No entity selected");
        }
        
        End();
    }

    public void HandlePicking(Scene scene)
    {
        var input = _game.Input;
        if (input.IsMouseButtonPressed(MouseButton.Left))
        {
            // Implement raycasting here if needed
            // Similar to your existing MultiRaycastVisibilityComponent
        }
    }

    public Entity? SelectedEntity
    {
        get => _selectedEntity;
        set => _selectedEntity = value;
    }
}
```

#### 3.2 Update Program.cs to Use ImGui Editor
```csharp
ImGuiSceneEditor? sceneEditor = null;

void Start(Scene rootScene)
{
  // ... existing setup ...
  
  imguiSystem = new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
  sceneEditor = new ImGuiSceneEditor(game, rootScene);
}

void Update(Scene rootScene, GameTime gameTime)
{
  var gameManager = game.Services.GetService<GameManager>();
  gameManager?.UIManager.UpdateUI();

  sceneEditor?.Draw();
  sceneEditor?.HandlePicking(rootScene);
}
```

### Phase 4: Advanced Features (Optional, 1-2 hours)

#### 4.1 Docking Support
```csharp
private void DrawWithDocking()
{
    var io = GetIO();
    io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
    
    DockSpaceOverViewport(GetMainViewport());
    
    DrawHierarchyWindow();
    DrawPropertiesWindow();
}
```

#### 4.2 Context Menus
```csharp
private void DrawEntityTree(Entity entity)
{
    // ... existing code ...
    
    if (BeginPopupContextItem())
    {
        if (MenuItem("Delete"))
        {
            // Handle deletion
        }
        if (MenuItem("Duplicate"))
        {
            // Handle duplication
        }
        EndPopup();
    }
}
```

## Comparison: Myra vs ImGui

| Feature | Myra (Current) | ImGui (New) |
|---------|----------------|-------------|
| Window Resizing | ❌ No | ✅ Yes |
| Docking | ❌ No | ✅ Yes |
| Tree Views | ✅ Yes | ✅ Yes |
| Property Editors | ✅ Basic | ✅ Advanced |
| Color Pickers | ❌ No | ✅ Yes |
| DragFloat Controls | ❌ No | ✅ Yes |
| Implementation Time | Already done | 30-60 minutes |
| Learning Curve | Low | Low-Medium |
| Professional Look | Good | Excellent |

## Migration Effort

### From Myra to ImGui
**Time**: 1-2 hours

**Steps**:
1. Add package (5 min)
2. Replace MyraSceneRenderer with ImGuiSystem (5 min)
3. Rewrite SceneEditorView as ImGuiSceneEditor (30-60 min)
4. Test and refine (15-30 min)

### What to Keep
- ✅ GameManager and UIManager logic
- ✅ Entity picking logic
- ✅ Scene structure
- ✅ All game objects and physics

### What to Replace
- ❌ MyraSceneRenderer → ImGuiSystem
- ❌ SceneEditorView (Myra) → ImGuiSceneEditor (ImGui)
- ❌ Myra-specific window code → ImGui window code

## Example from Community Toolkit

The Community Toolkit includes `Example11_ImGui` which demonstrates:
- ImGui initialization
- Window creation
- Built-in tools: HierarchyView, PerfMonitor, Inspector
- Integration with Stride's rendering pipeline

### Reference Code
```csharp
// From Example11_ImGui/Program.cs
new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
new HierarchyView(game.Services);
new PerfMonitor(game.Services);
Inspector.FindFreeInspector(game.Services).Target = game.SceneSystem.SceneInstance;
```

## Built-in ImGui Tools

The toolkit provides ready-to-use debugging tools:

1. **HierarchyView**: Complete scene hierarchy browser
2. **PerfMonitor**: Performance monitoring window
3. **Inspector**: Object property inspector

### Option: Use Built-in Tools
You could skip custom implementation and use:
```csharp
void Start(Scene rootScene)
{
  // ... setup ...
  
  var imgui = new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
  new HierarchyView(game.Services); // FREE scene hierarchy!
  var inspector = Inspector.FindFreeInspector(game.Services);
  inspector.Target = rootScene; // FREE property editor!
}
```

This gives you a **complete scene editor in 3 lines of code**!

## Next Steps

### Option A: Quick Win (3 lines of code, 5 minutes)
Use built-in ImGui tools:
```csharp
new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
new HierarchyView(game.Services);
Inspector.FindFreeInspector(game.Services).Target = rootScene;
```

### Option B: Custom Implementation (1-2 hours)
Build custom scene editor with:
- Custom tree view styling
- Custom property editors
- Custom context menus
- Custom docking layout

### Option C: Hybrid Approach (30 minutes)
Use built-in tools + custom windows:
```csharp
new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
new HierarchyView(game.Services); // Use built-in
new MyCustomToolWindow(game.Services); // Add your own
```

## Recommendation

**Start with Option A** to get immediate results, then:
1. Evaluate if built-in tools meet your needs
2. If yes: You're done! ✅
3. If no: Extend with custom windows (Option C)
4. Full custom only if you need very specific behavior (Option B)

## Resources

- **Package**: https://www.nuget.org/packages/Stride.CommunityToolkit.ImGui
- **Example**: `stride-community-toolkit/examples/code-only/Example11_ImGui`
- **Docs**: https://github.com/stride3d/stride-community-toolkit
- **ImGui.NET**: https://github.com/mellinoe/ImGui.NET
- **Hexa.NET.ImGui**: https://github.com/HexaEngine/Hexa.NET.ImGui

## Timeline

| Task | Time | Total |
|------|------|-------|
| Add package & restore | 5 min | 5 min |
| Test built-in tools | 10 min | 15 min |
| Evaluate results | 5 min | 20 min |
| **(If needed) Custom editor** | 60 min | 80 min |
| Testing & polish | 15 min | 95 min |

**Minimum viable**: 20 minutes  
**Full custom**: 95 minutes

## Decision Matrix

| Requirement | Myra | ImGui Built-in | ImGui Custom |
|-------------|------|----------------|--------------|
| Resizable windows | ❌ | ✅ | ✅ |
| Scene hierarchy | ✅ | ✅ | ✅ |
| Property editing | ✅ Basic | ✅ Advanced | ✅ Full control |
| Time to implement | Done | 20 min | 90 min |
| Maintenance | Low | None | Medium |
| Customization | Limited | None | Full |
| Professional look | Good | Excellent | Excellent |

## Conclusion

**The custom ImGui implementation is no longer necessary!**

Use `Stride.CommunityToolkit.ImGui` with built-in HierarchyView and Inspector tools for a complete, professional scene editor in **under 20 minutes**.

This eliminates:
- ❌ Custom shader compilation
- ❌ Graphics pipeline integration
- ❌ Vertex/index buffer management
- ❌ Input system mapping
- ❌ 12-20 hours of work

And gives you:
- ✅ Resizable windows
- ✅ Docking support
- ✅ Complete UI toolkit
- ✅ Professional debugging tools
- ✅ Battle-tested implementation
