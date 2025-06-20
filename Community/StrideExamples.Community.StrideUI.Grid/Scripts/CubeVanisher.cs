using Stride.CommunityToolkit.Engine;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace StrideExamples.StrideUI.Grid.Scripts;

public class CubeVanisher : AsyncScript
{
    private const float _totalTime = .5f;
    private const float _rotationSpeed = 900;

    public override async Task Execute()
    {
        var elapsedTime = 0f;
        var collider = Entity.GetComponent<RigidbodyComponent>();

        while (elapsedTime < _totalTime)
        {
            elapsedTime += (float)Game.UpdateTime.Elapsed.TotalSeconds;

            Entity.Transform.Scale = new Vector3(1 - elapsedTime / _totalTime);
            Entity.Transform.Rotation = Quaternion.RotationY(MathUtil.DegreesToRadians(_rotationSpeed * elapsedTime));

            collider?.UpdatePhysicsTransformation();

            await Script.NextFrame();
        }

        Entity.Remove();
        Console.WriteLine("Entity removed");
    }
}