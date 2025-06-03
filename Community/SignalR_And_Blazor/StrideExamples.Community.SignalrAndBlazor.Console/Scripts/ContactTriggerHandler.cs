using Stride.BepuPhysics;
using Stride.BepuPhysics.Definitions.Contacts;
using StrideExamples.Community.SignalrAndBlazor.Core.Core;
using StrideExamples.SignalrAndBlazor.Console.Core;

namespace StrideExamples.SignalrAndBlazor.Console.Scripts;

public class ContactTriggerHandler : IContactEventHandler
{
    public bool NoContactResponse => false;

    void IContactEventHandler.OnTouching<TManifold>(
        CollidableComponent eventSource,
        CollidableComponent other,
        ref TManifold contactManifold,
        bool flippedManifold,
        int workerIndex,
        BepuSimulation bepuSimulation
    )
    {
        if (eventSource?.Entity is null || other?.Entity is null)
            return;

        var sourceRobot = eventSource.Entity.Get<RobotComponent>();
        var otherRobot = other.Entity.Get<RobotComponent>();

        if (sourceRobot is null || otherRobot is null)
            return;

        if (sourceRobot.IsDeleted || otherRobot.IsDeleted)
            return;

        if (sourceRobot.Type == EntityType.Destroyer && sourceRobot.Type == EntityType.Destroyer)
            return;

        if (sourceRobot.Type == EntityType.Destroyer || otherRobot.Type == EntityType.Destroyer)
        {
            MarkForRemoval(sourceRobot);
            MarkForRemoval(otherRobot);
        }
    }

    public static void MarkForRemoval(RobotComponent? entityComponent)
    {
        if (entityComponent is RobotComponent robotComponent)
        {
            robotComponent.IsDeleted = true;
            robotComponent.Entity.Scene = null;
        }
    }
}