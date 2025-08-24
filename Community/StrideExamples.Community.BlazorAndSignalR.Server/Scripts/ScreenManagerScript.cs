using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Stride.Engine;
using Stride.Engine.Events;
using StrideExamples.Community.BlazorAndSignalR.Server.Core;
using StrideExamples.Community.BlazorAndSignalR.Server.Services;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Scripts;

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
      await _screenService.Connection.StartAsync();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error starting connection: {ex.Message}");
    }

    var materialManager = new MaterialManager(new MaterialBuilder(Game.GraphicsDevice));
    _primitiveBuilder = new RobotBuilder(Game, materialManager);
    _messagePrinter = new MessagePrinter(DebugText);

    var counterReceiver = new EventReceiver<CountDto>(GlobalEvents.CountReceievedEventKey);
    var messageReceiver = new EventReceiver<MessageDto>(GlobalEvents.MessageReceivedEventKey);
    var removeRequestReceiver = new EventReceiver<CountDto>(GlobalEvents.RemoveRequestEventKey);

    while (Game.IsRunning)
    {
      if (counterReceiver.TryReceive(out var countDto))
      {
      }
    }
  }

  private void QueuePrimitiveCreation(CountDto countDto) => _primitiveCreationQueue.Enqueue(countDto);
  private void QueuePrimitiveRemoval(CountDto countDto) => _primitiveRemovalQueue.Enqueue(countDto);
  private void ProcessPrimitiveCreationQueue()
  {
    if (_primitiveBuilder is null) throw new InvalidOperationException($"{nameof(ProcessPrimitiveCreationQueue)} cannot be called when {_primitiveBuilder} is null");

    if (_isCreatingPrimitives) return;
    if (_primitiveCreationQueue.TryDequeue(out var nextBatch))
    {
      if (nextBatch is null) return;
      _isCreatingPrimitives = true;
      _primitiveBuilder.CreatePrimitives(nextBatch, Entity.Scene);
      _isCreatingPrimitives = false;
    }
  }

  private async Task ProcessPrimitiveRemovalQueue()
  {
    if (_screenService is null) throw new InvalidOperationException($"{nameof(ProcessPrimitiveRemovalQueue)} cannot be called when {_screenService} is null");

    if (_primitiveRemovalQueue.TryDequeue(out var nextBatch))
    {
      if (nextBatch is null) return;
      await _screenService.Connection.SendAsync("SendUnitsRemoved", nextBatch);
    }
  }
}