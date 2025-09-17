using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Renderers;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
namespace StrideExamples.Local.Common;

public static class GameExtensions
{
  public static Material CreateMaterial(this Game game, Color? color = null, float specular = 1.0f, float microSurface = 0.65f)
  {
    var lightmapMaterial = new MaterialDescriptor
    {
      Attributes =
      {
        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)),
        // DiffuseModel = new MaterialLightmapModelFeature()
        // {
        //   Intensity = 20,
        //   LightMap = new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)
        // },
        Specular =  new MaterialMetalnessMapFeature(new ComputeFloat(specular)),
        SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(microSurface))
      }
    };

    return Material.New(game.GraphicsDevice, lightmapMaterial);
  }

  public static Entity CreateCube(this Game game, Scene rootScene, string name, Vector3 position, Color color)
  {
    var cube = game.Create3DPrimitive(
      PrimitiveModelType.Cube,
      new Bepu3DPhysicsOptions
      {
        Material = game.CreateMaterial(color),
        Size = new Vector3(2)
      }
    );

    cube.Name = name;
    cube.Transform.Position = position;
    cube.Scene = rootScene;

    return cube;
  }
}
