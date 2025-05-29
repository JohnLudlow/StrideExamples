using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Engine;
using Stride.Graphics;

using NexVYaml;

using StrideExamples.StrideUI.Grid.Managers;
using StrideExamples.StrideUI.Grid.Scripts;
using System.Reflection;
using System.Text;

using var game = new Game();

NexYamlSerializerRegistry.Init();

game.Run(start: Start);

void Start(Scene rootScene)
{
    game.SetupBase3DScene();
    game.AddSkybox();
    game.AddGroundGizmo();
    game.Add3DGround();
    game.AddProfiler();
    game.AddEntityDebugSceneRenderer();

    CreateAndRegisterGameManagerUI(rootScene);
}

void CreateAndRegisterGameManagerUI(Scene rootScene)
{
    var font = game.Content.Load<SpriteFont>("StrideDefaultFont");
    var gameManager = new GameManager(font);
    game.Services.AddService(gameManager);

    var uiEntity = gameManager.CreateUI();
    uiEntity.Add(new ClickHandlerComponent());
    uiEntity.Scene = rootScene;
}
