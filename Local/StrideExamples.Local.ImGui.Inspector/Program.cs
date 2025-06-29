using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.ImGui;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Engine;
var game = new Game();

game.Run(start: Start);


void Start(Scene scene)
{
    game.SetupBase3DScene();
    game.AddEntityDebugSceneRenderer(
        new()
        {
            ShowFontBackground = true,
        }
    );

    game.AddSkybox();
    game.AddProfiler();

    _ = new ImGuiSystem(game.Services, game.GraphicsDeviceManager);
    _ = new HierarchyView(game.Services);
    _ = new PerfMonitor(game.Services); 

    Inspector.FindFreeInspector(game.Services).Target = game.SceneSystem.SceneInstance;

    game.SetMaxFPS(60);
}