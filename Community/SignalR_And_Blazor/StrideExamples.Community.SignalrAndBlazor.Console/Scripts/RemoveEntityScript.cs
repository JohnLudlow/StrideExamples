using Stride.Engine;
using StrideExamples.Community.SignalrAndBlazor.Core.Core;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;
using StrideExamples.SignalrAndBlazor.Console.Core;

namespace StrideExamples.Community.SignalrAndBlazor.Console.Scripts;

public class RemoveEntityScript : AsyncScript
{
    private bool _isBeingRemoved;

    public override async Task Execute()
    {
        var robotComponent = Entity.Get<RobotComponent>();

        if (robotComponent == null) return;

        while (Game.IsRunning)
        {
            if (robotComponent.IsDeleted && !_isBeingRemoved)
            {
                _isBeingRemoved = true;

                System.Console.WriteLine($"Removing Entity {Entity.Name}");

                if (robotComponent.Type != EntityType.Destroyer)
                {
                    GlobalEvents.RemoveRequestEventKey.Broadcast(new CountDto(robotComponent.Type, 1));
                }
            }
        }

        await Script.NextFrame();
    }
}
