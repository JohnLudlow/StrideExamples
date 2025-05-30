using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Engine;
using StrideExamples.SignalrAndBlazor.Console.Services;

using var game = new Game();

game.Services.AddService(new ScreenService());

game.Run(start: (Scene rootScene) =>
{
    game.SetupBase3DScene();
    game.AddSkybox();
    game.AddProfiler();

    var screenManager = new Entity("")
    {
        // TODO: new ScreenManagerScript()
    };
});