using Stride.Engine;
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Core;

public sealed class RobotComponent : EntityComponent
{
  public EntityType Type { get; set; }
  public bool IsDeleted { get; set; }
}
