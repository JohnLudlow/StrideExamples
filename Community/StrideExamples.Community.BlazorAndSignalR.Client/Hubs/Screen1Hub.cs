using Microsoft.AspNetCore.SignalR;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;
using StrideExamples.Community.BlazorAndSignalR.Shared.Interfaces;

namespace StrideExamples.Community.BlazorAndSignalR.Client.Hubs;

public class Screen1Hub : Hub<IScreenClient>
{
  public async Task SendMessage(MessageDto dto) => await Clients.All.ReceiveMessageAsync(dto);
  public async Task SendCount(CountDto dto) => await Clients.All.ReceiveCountAsync(dto);
  public async Task SendUnitsRemoved(CountDto dto) => await Clients.All.ReceiveUnitsRemovedAsync(dto);
}