using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Physics;


Console.WriteLine("Hello, World!");

using var game = new Game();
var visibility = new MultiRaycastVisibility();
var gameStart = TimeSpan.FromMilliseconds(0);
game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
  game.SetupBase3DScene();
  game.AddProfiler();

  var greenCube = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DEntityOptions { Material = game.CreateMaterial(Color.Green), Size = new(2, 2, 2) });
  greenCube.Name = "GreenCube";
  greenCube.Transform.Position = new(-5, 1, 0);
  greenCube.Scene = rootScene;

  var blueCube = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DEntityOptions { Material = game.CreateMaterial(Color.Blue), Size = new(3) });
  blueCube.Name = "BlueCube";
  blueCube.Transform.Position = new(+5, 1, 0);
  blueCube.Scene = rootScene;
}

void Update(Scene rootScene, GameTime gameTime)
{
  if ((gameTime.Elapsed - gameStart).TotalSeconds > 5) return;
  gameStart = gameTime.Elapsed;

  var camera = rootScene.Entities.First(e => e.Get<CameraComponent>() != null).Get<CameraComponent>();

  var greenCube = rootScene.Entities.First(e => e.Name == "GreenCube");
  var greenCubeModel = greenCube.Get<ModelComponent>();
  Console.WriteLine($"Green cube: {visibility.CheckVisibility(greenCube, camera, camera.Entity.GetSimulation())}");

  var blueCube = rootScene.Entities.First(e => e.Name == "BlueCube");
  var blueCubeModel = blueCube.Get<ModelComponent>();
  Console.WriteLine($"Blue cube: {visibility.CheckVisibility(blueCube, camera, camera.Entity.GetSimulation())}");
}

public class MultiRaycastVisibility
{
  public record struct VisibilityResult
  {
    public bool IsVisible;
    public float VisibilityPercentage; // 0.0 to 1.0
    public int RaysHit;
    public int TotalRays;
  }

  public VisibilityResult CheckVisibility(Entity target, CameraComponent camera, Stride.BepuPhysics.BepuSimulation simulation)
  {
    var cameraPos = camera.Entity.Transform.WorldMatrix.TranslationVector;
    
    // Get target bounding box points
    var targetTransform = target.Transform;
    var targetModel = target.Get<ModelComponent>();
    var worldMatrix = targetTransform.WorldMatrix;
    var localBoundingBox = targetModel.Model.BoundingBox;
    BoundingBox.Transform(localBoundingBox, worldMatrix, out var worldBoundingBox);
    
    var testPoints = GenerateTestPoints(worldBoundingBox);
    var visibleRays = 0;
    
    foreach (var point in testPoints)
    {
        var direction = point - cameraPos;      
        
        // Use Bepu's raycast method
        var hit = simulation.RayCast(
            cameraPos, 
            Vector3.Normalize(direction),
            float.MaxValue,
            out var hitInfo
        );

      if (hit && hitInfo.Collidable?.Entity is not null)
      {
        if (hitInfo.Collidable.Entity.Name == "Ground") continue;

        Console.WriteLine($"Ray hit entity: {hitInfo.Collidable.Entity.Name}");

        if (hit && hitInfo.Collidable?.Entity == target)
        {
          visibleRays++; // Ray hit the target
        }
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
}