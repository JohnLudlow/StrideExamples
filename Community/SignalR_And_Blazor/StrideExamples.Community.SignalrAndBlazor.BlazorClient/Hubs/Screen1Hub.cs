using System;
using Microsoft.AspNetCore.SignalR;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;
using StrideExamples.Community.SignalrAndBlazor.Core.Interfaces;

namespace StrideExamples.Community.SignalrAndBlazor.BlazorClient.Hubs;

public class Screen1Hub : Hub<IScreenClient>
{
    public Task SendMessage(MessageDto dto) => Clients.All.ReceiveMessageAsync(dto);
    public Task SendCount(CountDto dto) => Clients.All.ReceiveCountAsync(dto);
    public Task SendUnitsRemoved(CountDto dto) => Clients.All.ReceiveUnitsRemovedAsync(dto);
}
