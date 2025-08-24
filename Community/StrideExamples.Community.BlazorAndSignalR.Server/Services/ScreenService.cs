// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;
using StrideExamples.Community.BlazorAndSignalR.Server.Core;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;
using StrideExamples.Community.BlazorAndSignalR.Shared.Interfaces;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Services;

public class ScreenService
{
  public HubConnection Connection { get; private init; }

  public ScreenService()
  {
    Connection = new HubConnectionBuilder()
      .WithUrl("https://localhost/44369/screen1")
      .Build();

    Connection.Closed += async (error) =>
    {
      await Task.Delay(new Random().Next(0, 5) * 1000);
      await Connection.StartAsync();
    };

    Connection.On<MessageDto>(nameof(IScreenClient.ReceiveMessageAsync), dto =>
    {
      GlobalEvents.MessageReceivedEventKey.Broadcast(dto);
      var encodedMsg = $"From Hub: {dto.Type}: {dto.Text}";

      Console.WriteLine(encodedMsg);
    });

    Connection.On<CountDto>(nameof(IScreenClient.ReceiveCountAsync), dto =>
    {
      GlobalEvents.CountReceievedEventKey.Broadcast(dto);
      var encodedMsg = $"From Hub: {dto.Type}: {dto.Count}";

      Console.WriteLine(encodedMsg);
    });
  }
}