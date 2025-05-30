using StrideExamples.SignalrAndBlazor.Core.Core;

namespace StrideExamples.SignalrAndBlazor.Core.Dtos;

public class MessageDto
{
    public EntityType Type { get; set; }
    public required string Text { get; set; }
}
