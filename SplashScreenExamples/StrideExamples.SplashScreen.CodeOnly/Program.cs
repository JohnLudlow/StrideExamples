// See https://aka.ms/new-console-template for more information
using Stride.Engine;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Rendering;
using Stride.Graphics;
using Stride.CommunityToolkit.Rendering.Utilities;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;

Console.WriteLine("Hello, World!");

var game = new Game();

game.Run(start: (Scene rootScene) =>
{
    // Set up the base 3D scene
    game.SetupBase3DScene();
    game.AddDirectionalLight();    
    game.AddSkybox();

    var camera = rootScene.Entities.First(entity => entity.Get<CameraComponent>() != null).Get<CameraComponent>();
    var planeEntity = CreateMeshEntity(game.GraphicsDevice, rootScene, new Vector3(0, 8, 0), CreatePlaneMesh);

    planeEntity.Transform.Scale *= 10;
    planeEntity.Scene = rootScene;
});

void CreatePlaneMesh(MeshBuilder meshBuilder)
{
    meshBuilder.WithIndexType(IndexingType.Int16);
    meshBuilder.WithPrimitiveType(PrimitiveType.TriangleList);

    var position = meshBuilder.WithPosition<Vector3>();
    var color = meshBuilder.WithColor<Color>();

    meshBuilder.AddVertex();
    meshBuilder.SetElement(position, new Vector3(0, 0, 0));
    meshBuilder.SetElement(color, Color.Red);

    meshBuilder.AddVertex();
    meshBuilder.SetElement(position, new Vector3(0, 1, 0));
    meshBuilder.SetElement(color, Color.Green);

    meshBuilder.AddVertex();
    meshBuilder.SetElement(position, new Vector3(1, 1, 0));
    meshBuilder.SetElement(color, Color.Blue);

    meshBuilder.AddVertex();
    meshBuilder.SetElement(position, new Vector3(1, 0, 0));
    meshBuilder.SetElement(color, Color.Yellow);

    meshBuilder.AddIndex(0);
    meshBuilder.AddIndex(1);
    meshBuilder.AddIndex(2);

    meshBuilder.AddIndex(0);
    meshBuilder.AddIndex(2);
    meshBuilder.AddIndex(3);

}

Entity CreateMeshEntity(GraphicsDevice graphicsDevice, Scene rootScene, Vector3 position, Action<MeshBuilder> build)
{
    using var meshBuilder = new MeshBuilder();
    build(meshBuilder);

    var entity = new Entity { Scene = rootScene, Transform = { Position = position } };
    var model = new Model
    {
        new MaterialInstance { Material = CreateMaterial(graphicsDevice)},
        new Mesh {
            Draw = meshBuilder.ToMeshDraw(graphicsDevice),
            MaterialIndex = 0
        }
    };

    entity.Add(new ModelComponent { Model = model });

    return entity;
}

Material CreateMaterial(GraphicsDevice graphicsDevice) => Material.New(graphicsDevice, new MaterialDescriptor 
{
    Attributes = new MaterialAttributes
    {
        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
        Diffuse = new MaterialDiffuseMapFeature { DiffuseMap = new ComputeVertexStreamColor() },
    }
});
