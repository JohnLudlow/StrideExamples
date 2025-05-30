using System;
using Stride.Engine;
using StrideExamples.SignalrAndBlazor.Core.Core;

namespace StrideExamples.SignalrAndBlazor.Console.Core;

public class RobotComponent : EntityComponent
{
    public EntityType Type { get; set; }
    public bool IsDeleted { get; set; }
}
