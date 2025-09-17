using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Engine;
using Stride.Games;
using StrideExamples.Local.MyraUI;

using var game = new Game();

var healthBarVisible = false;

game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
  game.AddGraphicsCompositor()
      .AddCleanUIStage()
      .AddSceneRenderer(new MyraSceneRenderer());

  game.Add3DCamera().Add3DCameraController();
  game.AddDirectionalLight();
  game.AddSkybox();
  game.Add3DGround();
}

void Update(Scene rootScene, GameTime gameTime)
{
  if (healthBarVisible) return;

  var mainView = game.Services.GetService<MainView>();

  if (mainView is null) return;

  mainView.Widgets.Add(Stride.Local.MyraUI.UIUtils.CreateHealthBar(-50, "#FFD961FF"));

  healthBarVisible = true;
}
