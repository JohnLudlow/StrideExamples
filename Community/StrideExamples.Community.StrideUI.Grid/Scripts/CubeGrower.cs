using Stride.CommunityToolkit.Engine;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace StrideExamples.StrideUI.Grid.Scripts;

public class CubeGrower : AsyncScript
{
    private const float _growDuration = 1.0f;
    public override async Task Execute()
    {
        var elapsedTime = 0f;
        Entity.Transform.Scale = Vector3.Zero;
        var collider = Entity.GetComponent<RigidbodyComponent>();

        while (elapsedTime < _growDuration)
        {
            elapsedTime += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            Entity.Transform.Scale = new Vector3((float)(elapsedTime / _growDuration));
            
            collider?.UpdatePhysicsTransformation();

            await Script.NextFrame();
        }

        Entity.Transform.Scale = Vector3.One;
        Entity.Remove<CubeGrower>();
        Console.WriteLine("Entity grown");
    }
}