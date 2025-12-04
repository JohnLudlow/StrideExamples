using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Games;
using Stride.CommunityToolkit.ImGui;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.DebugShapes.Code;
using Stride.Rendering;
using ImGuiVector3 = System.Numerics.Vector3;
using StrideVector3 = Stride.Core.Mathematics.Vector3;
using static Hexa.NET.ImGui.ImGui;
using ImGuiWindowFlags = Hexa.NET.ImGui.ImGuiWindowFlags;
using ImGuiTreeNodeFlags = Hexa.NET.ImGui.ImGuiTreeNodeFlags;

namespace StrideExamples.Local.OcclusionTest.UI;

/// <summary>
/// ImGui-based scene editor with resizable windows, tree hierarchy, and property editing.
/// </summary>
public class ImGuiSceneEditor : GameSystem
{
    private Entity? _selectedEntity;
    private readonly Game _game;
    private readonly Scene _scene;
    private ImmediateDebugRenderSystem? _debugDraw;
    private CameraComponent? _camera;

    public ImGuiSceneEditor(Game game, Scene scene) : base(game.Services)
    {
        _game = game;
        _scene = scene;
        
        // Ensure this runs after ImGuiSystem has called NewFrame
        Enabled = true;
        UpdateOrder = int.MaxValue - 1;
        
        // Add to game systems
        game.GameSystems.Add(this);
    }

    public override void Update(GameTime gameTime)
    {
        // Initialize services if needed
        _debugDraw ??= Services.GetService<ImmediateDebugRenderSystem>();
        _camera ??= _scene.Entities.FirstOrDefault(e => e.Get<CameraComponent>() != null)?.Get<CameraComponent>();

        // Handle 3D picking
        HandlePicking();
        
        // Draw highlight for selected entity
        if (_selectedEntity != null)
        {
            DrawSelectionHighlight(_selectedEntity);
        }

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
                // Position
                var pos = _selectedEntity.Transform.Position;
                var posX = pos.X;
                var posY = pos.Y;
                var posZ = pos.Z;
                if (DragFloat("Position X", ref posX, 0.1f) |
                    DragFloat("Position Y", ref posY, 0.1f) |
                    DragFloat("Position Z", ref posZ, 0.1f))
                {
                    _selectedEntity.Transform.Position = new StrideVector3(posX, posY, posZ);
                }
                
                // Rotation (convert to degrees for editing)
                var rot = _selectedEntity.Transform.RotationEulerXYZ;
                var rotX = MathUtil.RadiansToDegrees(rot.X);
                var rotY = MathUtil.RadiansToDegrees(rot.Y);
                var rotZ = MathUtil.RadiansToDegrees(rot.Z);
                if (DragFloat("Rotation X", ref rotX, 0.5f) |
                    DragFloat("Rotation Y", ref rotY, 0.5f) |
                    DragFloat("Rotation Z", ref rotZ, 0.5f))
                {
                    _selectedEntity.Transform.RotationEulerXYZ = new StrideVector3(
                        MathUtil.DegreesToRadians(rotX),
                        MathUtil.DegreesToRadians(rotY),
                        MathUtil.DegreesToRadians(rotZ)
                    );
                }
                
                // Scale
                var scale = _selectedEntity.Transform.Scale;
                var scaleX = scale.X;
                var scaleY = scale.Y;
                var scaleZ = scale.Z;
                if (DragFloat("Scale X", ref scaleX, 0.01f) |
                    DragFloat("Scale Y", ref scaleY, 0.01f) |
                    DragFloat("Scale Z", ref scaleZ, 0.01f))
                {
                    _selectedEntity.Transform.Scale = new StrideVector3(scaleX, scaleY, scaleZ);
                }
            }
            
            // Components
            if (CollapsingHeader("Components"))
            {
                foreach (var component in _selectedEntity.Components)
                {
                    BulletText(component.GetType().Name);
                }
            }
        }
        else
        {
            Text("No entity selected");
        }
        
        End();
    }

    private void HandlePicking()
    {
        var input = _game.Input;
        
        // Only handle clicks if not over ImGui windows
        var io = GetIO();
        if (_camera != null && input.IsMouseButtonPressed(MouseButton.Left) && !io.WantCaptureMouse)
        {
            // Perform raycast from mouse position
            if (_camera.Raycast(input.MousePosition, 1000f, out var hitInfo))
            {
                if (hitInfo.Collidable?.Entity != null)
                {
                    _selectedEntity = hitInfo.Collidable.Entity;
                }
            }
        }
    }

    private void DrawSelectionHighlight(Entity entity)
    {
        if (_debugDraw == null)
        {
            Console.WriteLine("DEBUG: _debugDraw is null");
            return;
        }

        // Ensure debug draw is enabled and visible
        _debugDraw.Enabled = true;
        _debugDraw.Visible = true;

        Console.WriteLine($"DEBUG: Drawing highlight for {entity.Name}");

        // Get entity's model component to determine bounds
        var modelComponent = entity.Get<ModelComponent>();
        if (modelComponent?.Model != null)
        {
            // Get world-space bounding box
            var worldMatrix = entity.Transform.WorldMatrix;
            var localBoundingBox = modelComponent.Model.BoundingBox;
            BoundingBox.Transform(ref localBoundingBox, ref worldMatrix, out var worldBoundingBox);

            // Draw wireframe box around the entity
            var center = worldBoundingBox.Center;
            var extent = worldBoundingBox.Extent;
            var size = extent * 2.0f;

            Console.WriteLine($"DEBUG: Center={center}, Size={size}");

            // Extract rotation from world matrix
            worldMatrix.Decompose(out StrideVector3 _, out Quaternion rotation, out StrideVector3 _);

            // Draw multiple wireframe cubes at slightly different scales for a thicker, more visible outline
            _debugDraw.DrawCube(center, size * 1.00f, rotation, Color.Yellow, depthTest: false, solid: false);
            _debugDraw.DrawCube(center, size * 1.02f, rotation, Color.Yellow, depthTest: false, solid: false);
            _debugDraw.DrawCube(center, size * 1.04f, rotation, Color.Yellow, depthTest: false, solid: false);
        }
        else
        {
            Console.WriteLine($"DEBUG: No model component, drawing at position {entity.Transform.Position}");
            // Fallback: draw a small cube at entity position if no model
            var position = entity.Transform.Position;
            var rotation = entity.Transform.Rotation;
            _debugDraw.DrawCube(position, new StrideVector3(0.5f), rotation, Color.Yellow, depthTest: false, solid: false);
            _debugDraw.DrawCube(position, new StrideVector3(0.52f), rotation, Color.Yellow, depthTest: false, solid: false);
        }
    }

    public Entity? SelectedEntity
    {
        get => _selectedEntity;
        set => _selectedEntity = value;
    }
}
