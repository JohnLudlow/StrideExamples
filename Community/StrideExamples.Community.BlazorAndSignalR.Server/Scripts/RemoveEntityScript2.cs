using Microsoft.AspNetCore.SignalR.Client;
using SharpFont.MultipleMasters;
using Stride.Engine;
using StrideExamples.Community.BlazorAndSignalR.Server.Core;
using StrideExamples.Community.BlazorAndSignalR.Server.Services;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Scripts
{
  public class RemoveEntityScript2 : AsyncScript
  {
    private bool _isBeingRemoved;

    public override async Task Execute()
    {
      var robotComponent = Entity.Get<RobotComponent>();
      var screenService = Services.GetService<ScreenService>();

      if (screenService is null || robotComponent is null) return;

      while (Game.IsRunning)
      {
        if (robotComponent.IsDeleted && !_isBeingRemoved)
        {
          _isBeingRemoved = true;

          Console.WriteLine($"Removing Entity {Entity.Name}");
          var message = new CountDto(robotComponent.Type, 1);
          await screenService.Connection.SendAsync("SendUnitsRemoved", message);

          Entity.Scene = null;
        }

        await Script.NextFrame();
      }
    }
  }
}