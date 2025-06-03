using System;
using Stride.Profiling;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.Community.SignalrAndBlazor.Console.Core;

public class MessagePrinter(DebugTextSystem debugTextSystem)
{
    private readonly FixedSizeQueue _messageQueue = new(10);

    public void PrintMessage()
    {
        if (_messageQueue.Count == 0) return;

        var messages = _messageQueue.AsSpan();

        for (var i = 0; i < messages.Length; i++)
        {
            var message = messages[i];
            if (message is null) continue;
            debugTextSystem.Print(message.Text, new(5, 30 * i + 18), Colours.ColourTypes[message.Type]);
        }
    }

    public void Enqueue(MessageDto? message)
    {
        if (message is null) return;
        _messageQueue.Enqueue(message);
    }
}
