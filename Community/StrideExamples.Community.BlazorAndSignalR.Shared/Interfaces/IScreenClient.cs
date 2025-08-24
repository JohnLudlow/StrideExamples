#pragma warning disable CA1716
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Shared.Interfaces;
#pragma warning restore CA1716

public interface IScreenClient
{
    Task ReceiveMessageAsync(MessageDto dto);
    Task ReceiveCountAsync(CountDto dto);
    Task ReceiveUnitsRemovedAsync(CountDto units);
}