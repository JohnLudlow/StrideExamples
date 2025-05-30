using Microsoft.AspNetCore.SignalR.Client;
using StrideExamples.SignalrAndBlazor.Console.Core;
using StrideExamples.SignalrAndBlazor.Core.Dtos;
using StrideExamples.SignalrAndBlazor.Core.Interfaces;

namespace StrideExamples.SignalrAndBlazor.Console.Services
{
    public class ScreenService
    {
        private HubConnection HubConnection { get; set; }

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