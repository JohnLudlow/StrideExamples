using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;

using var game = new Game();

game.Run(start: Start);

void Start(Scene rootScene)
{
    game.SetupBase3DScene();
    game.AddGroundGizmo(showAxisName: true);
    game.AddSkybox();

    var sphereEntity1 = CreateSphereEntity(game, Color.Blue);
    sphereEntity1.Transform.Position = new (0, 8, 0);
    sphereEntity1.AddChild(CreateLineEntity(game));

    var sphereEntity2 = CreateSphereEntity(game, Color.Yellow);
    sphereEntity2.Transform.Position = new (-.01f, 9, -.01f);

    sphereEntity1.Scene = rootScene;
    sphereEntity2.Scene = rootScene;
}

Entity CreateSphereEntity(Game game, Color color)
{
    return game.Create3DPrimitive(PrimitiveModelType.Sphere, new Primitive3DCreationOptions { Material = game.CreateMaterial(color) });
}

Entity CreateLineEntity(Game game)
{
    var vertices = new [] { Vector3.Zero, new (1, 1, -1) };
    var vertexBuffer = Stride.Graphics.Buffer.New(game.GraphicsDevice, vertices, BufferFlags.VertexBuffer);

    var indices = new short[] {0, 1};
    var indexBuffer = Stride.Graphics.Buffer.New(game.GraphicsDevice, indices, BufferFlags.IndexBuffer);

    return [
        new ModelComponent(
            [
                new Mesh { 
                    Draw = new MeshDraw {
                        PrimitiveType = PrimitiveType.LineList,
                        VertexBuffers = [new VertexBufferBinding(vertexBuffer, new VertexDeclaration(VertexElement.Position<Vector3>()), vertices.Length)],
                        IndexBuffer = new IndexBufferBinding(indexBuffer, false, indices.Length),
                        DrawCount = indices.Length
                    } 
                },
                game.CreateMaterial(Color.Red)
            ]
        )
    ];
}