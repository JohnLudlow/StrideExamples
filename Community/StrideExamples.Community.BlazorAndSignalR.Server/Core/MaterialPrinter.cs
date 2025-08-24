using System.Linq;
using Stride.Profiling;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Core;

public class MessagePrinter
{
  private readonly FixedSizeQueue _messageQueue = new(10);
  private readonly DebugTextSystem _debugTextSystem;

  public MessagePrinter(DebugTextSystem debugTextSystem)
  {
    _debugTextSystem = debugTextSystem;
  }

  public void PrintMessage()
  {
    if (_messageQueue.Count == 0) return;
    var messages = _messageQueue.AsSpan();

    for (var i = 0; i < messages.Length; i++)
    {
      if (messages[i] is MessageDto message)
        _debugTextSystem.Print(message.Text, new(55, 30 + i * 18), Colours.ColourTypes[message.Type]);
    }
  }

  public void Enqueue(MessageDto? message)
  {
    if (message is null) return;

    _messageQueue.Enqueue(message);
  }
}