using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.DebugShapes.Code;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Renderers;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using StrideExamples.Local.Common;
using StrideExamples.Local.OcclusionTest.Components;
using StrideExamples.Local.OcclusionTest.Managers;


Console.WriteLine("Hello, World!");

using var game = new Game();
game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
  game.SetupBase3DScene();
  game.AddGroundGizmo();
  game.Add3DGround();
  game.AddProfiler();
  game.AddAllDirectionLighting();
  // game.AddEntityDebugSceneRenderer();
  // game.AddDebugShapes();

  var font = game.Content.Load<SpriteFont>("StrideDefaultFont");
  var gameManager = new GameManager(rootScene, font);
  game.Services.AddService(gameManager);

  var greenCube = CreateCube(game, rootScene, "GreenCube", new(-5, 1, 0), Color.Green);
  gameManager.UIManager.MonitorEntity(greenCube);

  var blueCube = CreateCube(game, rootScene, "BlueCube", new(5, 1, 0), Color.Blue);
  gameManager.UIManager.MonitorEntity(blueCube);
}

void Update(Scene rootScene, GameTime gameTime)
{
  var gameManager = game.Services.GetService<GameManager>();
  gameManager?.UIManager.UpdateUI();
}

static Entity CreateCube(Game game, Scene rootScene, string name, Vector3 position, Color color)
{
  var cube = game.CreateCube(rootScene, name, position, color);
 
  cube.Add(new MultiRaycastVisibilityComponent
  {
    Game = game,
    Target = cube,
    Camera = rootScene.GetCamera() ?? throw new InvalidOperationException("No camera found in scene"),
    Simulation = cube.GetSimulation()
  });
 
  return cube;
}