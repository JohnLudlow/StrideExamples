using BulletSharp.SoftBody;
using Stride.BepuPhysics;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Helpers;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Engine;
using Stride.Games;
using StrideExamples.Community.SignalrAndBlazor.Console.Scripts;
using StrideExamples.Community.SignalrAndBlazor.Core.Core;
using StrideExamples.Community.SignalrAndBlazor.Core.Dtos;
using StrideExamples.SignalrAndBlazor.Console.Core;
using StrideExamples.SignalrAndBlazor.Console.Scripts;

namespace StrideExamples.Community.SignalrAndBlazor.Console.Core;

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
            var entity = _game.Create3DPrimitive(PrimitiveModelType.Cube, new()
            {
                EntityName = $"Entity {_count++} - {countDto.Type}",
                Material = _materialManager.GetMaterial(countDto.Type),
                Size = countDto.Type == EntityType.Destroyer ? new(.5f, .5f, .5f) : new(1, 1, 1)
            });

            entity.Add(new RobotComponent
            {
                Type = countDto.Type
            });

            entity.Add(new RemoveEntityScript());
            entity.Transform.Position = VectorHelper.RandomVector3([-5, 5], [5, 10], [-5, 5]);

            var collider = entity.Get<BodyComponent>();
            if (collider != null)
            {
                collider.ContactEventHandler = _triggerScript;
            }

            entity.Scene = scene;
        }
    }
}