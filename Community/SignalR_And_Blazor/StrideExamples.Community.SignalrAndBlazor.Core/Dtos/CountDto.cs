using StrideExamples.SignalrAndBlazor.Core.Core;

namespace StrideExamples.SignalrAndBlazor.Core.Dtos;

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