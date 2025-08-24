using System.Collections.Concurrent;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Core;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class FixedSizeQueue
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
  public int Count => _queue.Count;
  private readonly ConcurrentQueue<MessageDto> _queue = new();
  private readonly int _maxSize;
  public FixedSizeQueue(int maxSize)
  {
    _maxSize = maxSize;
  }

  public void Enqueue(MessageDto? item)
  {
    if (item == null) return;
    if (_queue.Count == _maxSize && !_queue.IsEmpty)
    {
      _queue.TryDequeue(out _);
    }

    _queue.Enqueue(item);
  }

  public ReadOnlySpan<MessageDto> AsSpan() => new([.. _queue]);
  public List<MessageDto> ToList() => [.. _queue];
}