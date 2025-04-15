using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.Utilities;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;

const float _cylinderRadius = 0.3f;
const float _torusAngle = 270.0f;
const float _bendRadius = 1.0f;
const int _circumferenceStepsCount = 20;
const int _bendSegmentSteps = 40;

using var game = new Game();

game.Run(start: rootScene =>
{
    game.SetupBase3DScene();
    game.AddSkybox();

    CreateMeshEntity(
        game.GraphicsDevice,
        rootScene,
        new Vector3(0, 1, 0),
        b => BuildPartialTorusMesh(b, _cylinderRadius, _torusAngle, _bendRadius, _circumferenceStepsCount, _bendSegmentSteps)
    );
});

Entity CreateMeshEntity(GraphicsDevice graphicsDevice, Scene scene, Vector3 position, Action<MeshBuilder> build)
{
    using var meshBuilder = new MeshBuilder();
    build(meshBuilder);

    var entity = new Entity {
        Scene = scene, 
        Transform = { Position = position}
    };

    var model = new Model {
        new MaterialInstance { Material = CreateMaterial(game) },
        new Mesh
        {
            Draw = meshBuilder.ToMeshDraw(graphicsDevice),
            MaterialIndex = 0
        }
    };

    entity.Add(new ModelComponent { Model = model });

    return entity;
}

void BuildPartialTorusMesh(MeshBuilder meshBuilder, float cylinderRadius, float torusAngle, float bendRadius, float circumferenceStepsCount, float bendSegmentSteps)
{
        // for partial torus up to 360 degrees (torusAngle in degrees)
    meshBuilder.WithIndexType(IndexingType.Int16);
    meshBuilder.WithPrimitiveType(PrimitiveType.TriangleList);

    var position = meshBuilder.WithPosition<Vector3>();
    var normal = meshBuilder.WithNormal<Vector3>();

    // vertices
    for (var j = 0; j <= bendSegmentSteps; j++)
    {
        // Torus angle position Phi starts at 0 in line with Z-axis and increases to Pi/2 at X-axis
        var tPhi = j * torusAngle / bendSegmentSteps * Math.PI / 180.0;
        var xc = bendRadius * Math.Sin(tPhi);
        var zc = bendRadius * Math.Cos(tPhi);

        for (var i = 0; i < circumferenceStepsCount; i++)
        {
            // Circumference angle tTheta
            var tTheta = i * Math.Tau / circumferenceStepsCount;
            var yr = cylinderRadius * Math.Sin(tTheta);
            var xr = cylinderRadius * Math.Cos(tTheta) * Math.Sin(tPhi);
            var zr = cylinderRadius * Math.Cos(tTheta) * Math.Cos(tPhi);

            var tNorm = new Vector3((float)xr, (float)yr, (float)zr);
            var tPos = new Vector3((float)(xc + xr), (float)yr, (float)(zc + zr));

            meshBuilder.AddVertex();
            meshBuilder.SetElement(position, tPos);
            meshBuilder.SetElement(normal, tNorm);
        }
    }

    // Triangle indices
    for (var j = 0; j < bendSegmentSteps; j++)
    {
        for (var i = 0; i < circumferenceStepsCount; i++)
        {
            var i_tot = (int)(i + j * circumferenceStepsCount);
            var i_next = (int)((i + 1) % circumferenceStepsCount + j * circumferenceStepsCount);

            // Triangle 1
            meshBuilder.AddIndex(i_tot);
            meshBuilder.AddIndex(i_next + (int)circumferenceStepsCount);
            meshBuilder.AddIndex(i_tot + (int)circumferenceStepsCount);

            // Triangle 2
            meshBuilder.AddIndex(i_tot);
            meshBuilder.AddIndex(i_next);
            meshBuilder.AddIndex(i_next + (int)circumferenceStepsCount);
        }
    }
}

Material CreateMaterial(Game game)
{
    return Material.New(
        game.GraphicsDevice, 
        new MaterialDescriptor 
        { 
            Attributes = new MaterialAttributes {
                MicroSurface = new MaterialGlossinessMapFeature
                {
                    GlossinessMap = new ComputeFloat(.9f)
                },
                Diffuse = new MaterialDiffuseMapFeature {
                    DiffuseMap = new ComputeColor(new Color4(1f, .3f, .5f, 1.0f))
                },
                DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                Specular = new MaterialMetalnessMapFeature
                {
                    MetalnessMap = new ComputeFloat(0)
                },
                SpecularModel = new MaterialSpecularMicrofacetModelFeature
                {
                    Environment = new MaterialSpecularMicrofacetEnvironmentGGXPolynomial()
                }
            }
        }
    );
}