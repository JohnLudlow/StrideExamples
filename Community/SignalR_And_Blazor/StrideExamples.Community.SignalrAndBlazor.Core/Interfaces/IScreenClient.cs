using StrideExamples.SignalrAndBlazor.Core.Dtos;

namespace StrideExamples.SignalrAndBlazor.Core.Interfaces;

public interface IScreenClient
{
    Task ReceiveMessageAsync(MessageDto dto);
    Task ReceiveCountAsync(CountDto dto);
    Task ReceiveUnitsRemovedAsync(CountDto units);
}