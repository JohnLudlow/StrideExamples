using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Input;
using StrideExamples.Community.SignalrAndBlazor.Console.Core;
using StrideExamples.Community.SignalrAndBlazor.Console.Services;
using StrideExamples.Community.SignalrAndBlazor.Core.Core;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;
using StrideExamples.SignalrAndBlazor.Console.Core;

namespace StrideExamples.SignalrAndBlazor.Console.Scripts;

public class ScreenManagerScript : AsyncScript
{
    private readonly ConcurrentQueue<CountDto> _primitiveCreationQueue = new();
    private readonly ConcurrentQueue<CountDto> _primitiveRemovalQueue = new();
    private RobotBuilder? _primitiveBuilder;
    private MessagePrinter? _messagePrinter;
    private ScreenService? _screenService;
    private bool _isCreatingPrimitives;

    public override async Task Execute()
    {
        _screenService = Services.GetService<ScreenService>();

        if (_screenService is null) return;

        try
        {
            await _screenService.HubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error starting connection: {ex.Message}");
        }

        var materialManager = new MaterialManager(new MaterialBuilder(Game.GraphicsDevice));

        var countReceiver = new EventReceiver<CountDto>(GlobalEvents.CountReceivedEventKey);
        var messageReceiver = new EventReceiver<MessageDto>(GlobalEvents.MessageReceivedEventKey);
        var removeRequestReceiver = new EventReceiver<CountDto>(GlobalEvents.RemoveRequestEventKey);

        while (Game.IsRunning)
        {
            if (countReceiver.TryReceive(out var countDto))
            {
                QueuePrimitiveCreation(countDto);
            }

            if (removeRequestReceiver.TryReceive(out var countDto2))
            {
                System.Console.WriteLine($"Broadcast received");
                QueuePrimitiveRemoval(countDto2);
            }

            if (messageReceiver.TryReceive(out var messageDto))
            {
                _messagePrinter?.Enqueue(messageDto);
            }

            _messagePrinter?.Enqueue(messageDto);

            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                System.Console.WriteLine("------------------------------------------");
                QueuePrimitiveCreation(new CountDto
                {
                    Type = EntityType.Destroyer,
                    Count = 10
                });
            }

            ProcessPrimitiveQueue();
            await ProcessRemoveQueue();
            await Script.NextFrame();
        }
    }

    public void QueuePrimitiveCreation(CountDto countDto) => _primitiveCreationQueue.Enqueue(countDto);
    public void QueuePrimitiveRemoval(CountDto countDto) => _primitiveRemovalQueue.Enqueue(countDto);

    public void ProcessPrimitiveQueue()
    {
        if (_isCreatingPrimitives) return;

        if (_primitiveCreationQueue.TryDequeue(out var nextBatch))
        {
            if (nextBatch is null) return;

            _isCreatingPrimitives = true;
            _primitiveBuilder!.CreatePrimitives(nextBatch, Entity.Scene);
            _isCreatingPrimitives = false;
        }
    }

    public async Task ProcessRemoveQueue()
    {
        if (_primitiveRemovalQueue.TryDequeue(out var countDto))
        {
            if (countDto is null)
            {
                await _screenService!.HubConnection.SendAsync("SendUnitsRemoved", countDto);
            }
        }
    }
}