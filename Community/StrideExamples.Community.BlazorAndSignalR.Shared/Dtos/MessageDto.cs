#pragma warning disable CA1716
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;

namespace StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;
#pragma warning restore CA1716

public class MessageDto
{
    public EntityType Type { get; set; }
    public required string Text { get; set; }
}