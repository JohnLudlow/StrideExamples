using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.Community.SignalrAndBlazor.Core.Interfaces;

public interface IScreenClient
{
    Task ReceiveMessageAsync(MessageDto dto);
    Task ReceiveCountAsync(CountDto dto);
    Task ReceiveUnitsRemovedAsync(CountDto units);
}