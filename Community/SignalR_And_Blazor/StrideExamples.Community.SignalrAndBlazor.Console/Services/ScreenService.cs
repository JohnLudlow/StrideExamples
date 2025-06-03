using Microsoft.AspNetCore.SignalR.Client;
using StrideExamples.SignalrAndBlazor.Console.Core;
using StrideExamples.Community.SignalrAndBlazor.Core.Interfaces;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.Community.SignalrAndBlazor.Console.Services
{
    public class ScreenService
    {
        public HubConnection HubConnection { get; private init; }

        public ScreenService()
        {
            HubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/screen1")
                .Build();

            HubConnection.Closed += async (error) =>
            {
                System.Console.WriteLine("Connection closed. Attempting to reconnect...");
                await Task.Delay(new Random().Next(0, 5) * 1000); // Wait before reconnecting
                await HubConnection.StartAsync();
            };

            HubConnection.On<CountDto>(nameof(IScreenClient.ReceiveMessageAsync), async (dto) =>
            {
                System.Console.WriteLine($"Received count: {dto.Count} of type {dto.Type}");
                GlobalEvents.CountReceivedEventKey.Broadcast(dto);
            });

            HubConnection.On<MessageDto>(nameof(IScreenClient.ReceiveMessageAsync), async (dto) =>
            {
                System.Console.WriteLine($"Received message: {dto.Text} of type {dto.Type}");
                GlobalEvents.MessageReceivedEventKey.Broadcast(dto);
            });
        }
    }
}