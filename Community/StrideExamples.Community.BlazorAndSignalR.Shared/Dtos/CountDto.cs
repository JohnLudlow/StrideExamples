#pragma warning disable CA1716
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;

namespace StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;
#pragma warning restore CA1716

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
