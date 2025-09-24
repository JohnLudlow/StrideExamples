using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;

namespace StrideExamples.Local.OcclusionTest.Components;

public class MultiRaycastVisibilityComponent : SyncScript
{
  public required Entity Target { get; init; }
  public required CameraComponent Camera { get; init; }
  public required Stride.BepuPhysics.BepuSimulation Simulation { get; init; }

  public VisibilityResult LastVisibilityResult { get; private set; }

  [DataMemberIgnore]
  private Stride.CommunityToolkit.DebugShapes.Code.ImmediateDebugRenderSystem? _debugDraw;

  public record struct VisibilityResult
  {
    public bool IsVisible;
    public float VisibilityPercentage; // 0.0 to 1.0
    public int RaysHit;
    public int TotalRays;
  }

  public VisibilityResult CheckVisibility(IGame game, Entity target, CameraComponent camera, Stride.BepuPhysics.BepuSimulation simulation)
  {
    _debugDraw ??= game.Services.GetService<Stride.CommunityToolkit.DebugShapes.Code.ImmediateDebugRenderSystem>();
    if (_debugDraw is not null)
    {
      _debugDraw.Enabled = true;
      _debugDraw.Visible = true;
    }

    var cameraPos = camera.Entity.Transform.WorldMatrix.TranslationVector;

    // Get target bounding box points
    var targetTransform = target.Transform;
    var targetModel = target.Get<ModelComponent>();
    var worldMatrix = targetTransform.WorldMatrix;
    var localBoundingBox = targetModel.Model.BoundingBox;
    BoundingBox.Transform(ref localBoundingBox, ref worldMatrix, out var worldBoundingBox);

    var testPoints = GenerateTestPoints(worldBoundingBox);
    var visibleRays = 0;

    foreach (var point in testPoints)
    {
      var direction = point - cameraPos;

      direction.Normalize();
      _debugDraw?.DrawRay(cameraPos, direction*100, Color.Yellow, 0, false);

      // Use Bepu's raycast method
      var hit = simulation.RayCast(
          cameraPos,
          direction,
          float.MaxValue,
          out var hitInfo
      );

      if (hit && hitInfo.Collidable?.Entity == target)
      {
        visibleRays++; // Ray hit the target
      }
    }

    var visibilityPercentage = (float)visibleRays / testPoints.Length;

    return new VisibilityResult
    {
      IsVisible = visibilityPercentage > 0.0f,
      VisibilityPercentage = visibilityPercentage,
      RaysHit = visibleRays,
      TotalRays = testPoints.Length
    };
  }

  private Vector3[] GenerateTestPoints(BoundingBox boundingBox)
  {
    var min = boundingBox.Minimum;
    var max = boundingBox.Maximum;
    var center = (min + max) * 0.5f;

    return
    [
      // 8 corners of the bounding box
      new(min.X, min.Y, min.Z), // 0: min corner
    new(max.X, min.Y, min.Z), // 1: +X
    new(min.X, max.Y, min.Z), // 2: +Y  
    new(min.X, min.Y, max.Z), // 3: +Z
    new(max.X, max.Y, min.Z), // 4: +XY
    new(max.X, min.Y, max.Z), // 5: +XZ
    new(min.X, max.Y, max.Z), // 6: +YZ
    new(max.X, max.Y, max.Z), // 7: max corner
    
    // Face centers
    center, // Center
    new(center.X, center.Y, min.Z), // Front face center
    new(center.X, center.Y, max.Z), // Back face center
    new(min.X, center.Y, center.Z), // Left face center
    new(max.X, center.Y, center.Z), // Right face center
    new(center.X, min.Y, center.Z), // Bottom face center
    new(center.X, max.Y, center.Z), // Top face center
  ];
  }

  public override void Update()
  {
    LastVisibilityResult = CheckVisibility(Game, Target, Camera, Simulation);
  }
}