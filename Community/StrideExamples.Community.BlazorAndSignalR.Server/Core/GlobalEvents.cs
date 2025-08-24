using Stride.Engine.Events;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Core;

public static class GlobalEvents
{
  private const string Category = "Global";
  public static readonly EventKey<CountDto> CountReceievedEventKey = new(Category, "CountReceived");
  public static readonly EventKey<MessageDto> MessageReceivedEventKey = new(Category, "MessageReceived");
  public static readonly EventKey<CountDto> RemoveRequestEventKey = new(Category, "RemoveRequest");
}