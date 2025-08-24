using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Engine;
using Stride.Games;
using StrideExamples.Community.BlazorAndSignalR.Server.Scripts;
using StrideExamples.Community.BlazorAndSignalR.Shared.Dtos;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Core;

public class RobotBuilder
{
  private readonly IGame _game;
  private readonly MaterialManager _materialManager;
  private readonly ContactTriggerHandler _triggerScript = new();
  private int _count = 1;

  public RobotBuilder(IGame game, MaterialManager materialManager)
  {
    _game = game;
    _materialManager = materialManager;
  }

  public void CreatePrimitives(CountDto countDto, Scene scene)
  {
    for (var i = 0; i < countDto.Count; i++)
    {
      var entity = _game.Create3DPrimitive(
        PrimitiveModelType.Cube,
        new()
        {
          EntityName = $"Entity {_count++} - {countDto.Type}",
          Material = _materialManager.GetMaterial(countDto.Type),
          Size = countDto.Type == Shared.Core.EntityType.Destroyer ? new(0.5f, 0.5f, 0.5f) : new(1, 1, 1)
        }
      );

      entity.Add(new RobotComponent { Type = countDto.Type });
      entity.Add(new RemoveEntityScript());
    }
  }
}
