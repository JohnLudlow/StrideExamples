using StrideExamples.Community.SignalrAndBlazor.Core.Core;

namespace StrideExamples.Community.SignalrAndBlazor.Core.Dtos;

public class CountDto
{
    public EntityType Type { get; set; }
    public int Count { get; set; }

    public CountDto() { }

    public CountDto(EntityType type, int count)
    {
        Count = count;
        Type = type;
    }
}