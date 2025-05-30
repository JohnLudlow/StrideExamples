using Stride.Engine.Events;
using StrideExamples.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.SignalrAndBlazor.Console.Core;

public static class GlobalEvents
{
    private const string Category = "Global";

    public readonly static EventKey<CountDto> CountReceivedEventKey = new(Category, "CountReceived");
    public readonly static EventKey<MessageDto> MessageReceivedEventKey = new(Category, "MessageReceived");
    public readonly static EventKey<CountDto> RemoveRequestEventKey = new(Category, "RemoveRequest");
}