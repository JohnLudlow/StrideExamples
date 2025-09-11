using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using StrideExamples.Local.OcclusionTest.Managers;


Console.WriteLine("Hello, World!");

using var game = new Game();
var visibility = new MultiRaycastVisibility();
game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
  game.SetupBase3DScene();
  game.AddGroundGizmo();
  game.Add3DGround();
  game.AddProfiler();
  // game.AddEntityDebugSceneRenderer();

  var font = game.Content.Load<SpriteFont>("StrideDefaultFont");
  var gameManager = new GameManager(font);
  game.Services.AddService(gameManager);

  var uiEntity = gameManager.CreateUI();
  uiEntity.Scene = rootScene;

  CreateCube(rootScene, game, "GreenCube", new(-5, 1, 0), Color.Green);
  CreateCube(rootScene, game, "BlueCube", new(5, 1, 0), Color.Blue);
}

void Update(Scene rootScene, GameTime gameTime)
{
  if (!game.Input.IsKeyPressed(Stride.Input.Keys.Space))
  {
    return;
  }

  var camera = rootScene.Entities.First(e => e.Get<CameraComponent>() != null).Get<CameraComponent>();

  var greenCube = rootScene.Entities.First(e => e.Name == "GreenCube");
  var greenVisibility = visibility.CheckVisibility(game, greenCube, camera, camera.Entity.GetSimulation());
  Console.WriteLine($"Green cube: {visibility.CheckVisibility(game, greenCube, camera, camera.Entity.GetSimulation())}");

  var blueCube = rootScene.Entities.First(e => e.Name == "BlueCube");
  var blueVisibility = visibility.CheckVisibility(game, blueCube, camera, camera.Entity.GetSimulation());
  Console.WriteLine($"Blue cube: {visibility.CheckVisibility(game, blueCube, camera, camera.Entity.GetSimulation())}");
}

static Entity CreateCube(Scene rootScene, Game game, string name, Vector3 position, Color color)
{
  var cube = game.Create3DPrimitive(
    PrimitiveModelType.Cube,
    new Bepu3DPhysicsOptions
    {
      Material = game.CreateMaterial(color),
      Size = new(2)
    }
  );

  cube.Name = name;
  cube.Transform.Position = position;
  cube.Scene = rootScene;

  return cube;
}