using System;
using System.Collections.Concurrent;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.Community.SignalrAndBlazor.Console.Core;

public class FixedSizeQueue(int maxSize)
{
    private readonly ConcurrentQueue<MessageDto> _queue = new();
    public int Count => _queue.Count;

    public void Enqueue(MessageDto? item)
    {
        if (item is null) return;

        if (_queue.Count == maxSize && !_queue.IsEmpty)
        {
            _queue.TryDequeue(out _);
        }

        _queue.Enqueue(item);
    }

    public ReadOnlySpan<MessageDto> AsSpan() => new([.. _queue]);
    public List<MessageDto> ToList() => [.. _queue];
}
