using SharpDX.Direct3D11;
using Stride.BepuPhysics;
using Stride.BepuPhysics.Definitions.Contacts;
using StrideExamples.Community.BlazorAndSignalR.Server.Core;
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Scripts;

public class ContactTriggerHandler : IContactEventHandler
{
  public bool NoContactResponse => false;

  void IContactEventHandler.OnStartedTouching<TManifold>(
    CollidableComponent eventSource,
    CollidableComponent other,
    ref TManifold contactManifold,
    bool flippedManifold,
    int workerIndex,
    BepuSimulation bepuSimulation
  )
  {
    if (eventSource?.Entity is null || other?.Entity is null) return;

    var sourceRobot = eventSource.Entity.Get<RobotComponent>();
    var otherRobot = other.Entity.Get<RobotComponent>();

    if (sourceRobot is null || other is null) return;
    if (sourceRobot.IsDeleted || otherRobot.IsDeleted) return;
    if (sourceRobot.Type == EntityType.Destroyer && otherRobot.Type == EntityType.Destroyer) return;
    if (sourceRobot.Type == EntityType.Destroyer || otherRobot.Type == EntityType.Destroyer)
    {
      MarkForRemoval(sourceRobot);
      MarkForRemoval(otherRobot);
    }
  }

  public static void MarkForRemoval(RobotComponent? robotComponent)
  {
    if (robotComponent is null) return;
    robotComponent.IsDeleted = true;    
  }
}