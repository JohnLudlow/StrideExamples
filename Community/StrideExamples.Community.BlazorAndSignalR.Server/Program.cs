// See https://aka.ms/new-console-template for more information
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using StrideExamples.Community.BlazorAndSignalR.Server.Scripts;
using StrideExamples.Community.BlazorAndSignalR.Server.Services;

using var game = new Game();

game.Services.AddService(new ScreenService());
game.Run(start: (Scene rootScene) =>
{
  game.SetupBase3DScene();
  game.AddSkybox();
  game.AddProfiler();

  var screenManager = new Entity("ScreenManager")
  {
    new ScreenManagerScript()
  };
  screenManager.Scene = rootScene;

  var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);
  entity.Transform.Position = new Vector3(0, 8, 0);
  entity.Scene = rootScene;
});