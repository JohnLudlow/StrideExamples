using System.Collections.Concurrent;
using Stride.Engine;
using StrideExamples.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.SignalrAndBlazor.Console.Scripts;

public class ScreenManagerScript : AsyncScript
{
    private readonly ConcurrentQueue<CountDto> _primitiveCreationQueue = new();
    private readonly ConcurrentQueue<CountDto> _primitiveRemovalQueue = new();
    // TODO: new RobotBuilder

    public override Task Execute()
    {
        throw new NotImplementedException();
    }
}