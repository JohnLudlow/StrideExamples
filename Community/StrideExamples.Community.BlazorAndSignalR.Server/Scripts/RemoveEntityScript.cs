using Stride.Engine;
using StrideExamples.Community.BlazorAndSignalR.Server.Core;
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Scripts;

public class RemoveEntityScript : AsyncScript
{
  private bool _isBeingRemoved;
  public override async Task Execute()
  {
    var robotComponent = Entity.Get<RobotComponent>();
    if (robotComponent is null) return;

    while (Game.IsRunning)
    {
      if (robotComponent.IsDeleted && !_isBeingRemoved)
      {
        _isBeingRemoved = true;
        Console.WriteLine($"Removing entity: {Entity.Name}");
        if (robotComponent.Type != EntityType.Destroyer)
        {
          var message = new CountDto(robotComponent.Type, 1);

          GlobalEvents.RemoveRequestEventKey.Broadcast(message);

          Entity.Scene = null;
        }
      }

      await Script.NextFrame();
    }
  }
}
