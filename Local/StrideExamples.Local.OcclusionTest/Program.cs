using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.DebugShapes.Code;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Renderers;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Compositing;
using StrideExamples.Local.OcclusionTest.Components;
using StrideExamples.Local.OcclusionTest.Managers;
using StrideExamples.Local.OcclusionTest.UI;


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

  // Add Myra renderer to the graphics compositor
  var compositor = game.SceneSystem.GraphicsCompositor;
  if (compositor != null)
  {
    var gameCompositor = compositor.Game as SceneRendererCollection;
    if (gameCompositor != null)
    {
      var myraRenderer = new MyraSceneRenderer();
      gameCompositor.Children.Add(myraRenderer);
      Console.WriteLine("Myra renderer added to compositor children");
    }
    else
    {
      Console.WriteLine("WARNING: compositor.Game is not a SceneRendererCollection");
    }
  }
  else
  {
    Console.WriteLine("WARNING: GraphicsCompositor is null!");
  }

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

  var sceneEditor = game.Services.GetService<SceneEditorView>();
  
  // Initialize scene editor on first update when it's available
  if (sceneEditor != null && sceneEditor.IsInitialized == false)
  {
    sceneEditor.Initialize(rootScene, game);
    Console.WriteLine("SceneEditor initialized in Update");
  }
  
  sceneEditor?.Update(game);
}

static Material CreateMaterial(Game game, Color? color = null, float specular = 1.0f, float microSurface = 0.65f)
{
  var lightmapMaterial = new MaterialDescriptor
  {
    Attributes =
    {
      Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)),
      Specular =  new MaterialMetalnessMapFeature(new ComputeFloat(specular)),
      SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
      MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(microSurface))
    }
  };

  return Material.New(game.GraphicsDevice, lightmapMaterial);
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

  cube.Add(new MultiRaycastVisibilityComponent
  {
    Game = game,
    Target = cube,
    Camera = rootScene.GetCamera() ?? throw new InvalidOperationException("No camera found in scene"),
    Simulation = cube.GetSimulation()
  });

  return cube;
}