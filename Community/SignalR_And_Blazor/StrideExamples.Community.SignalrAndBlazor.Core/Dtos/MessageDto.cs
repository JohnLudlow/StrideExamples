using StrideExamples.Community.SignalrAndBlazor.Core.Core;

namespace StrideExamples.Community.SignalrAndBlazor.Core.Dtos;

public class MessageDto
{
    public EntityType Type { get; set; }
    public required string Text { get; set; }
}
