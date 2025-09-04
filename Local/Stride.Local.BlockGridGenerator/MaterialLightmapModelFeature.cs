using System;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Shaders;

namespace Stride.Local.BlockGridGenerator;

[DataContract(nameof(MaterialLightmapModelFeature))]
[Display("LightMap")]
public class MaterialLightmapModelFeature : MaterialFeature, IMaterialDiffuseModelFeature
{
  private static readonly ObjectParameterKey<Texture> Map = ParameterKeys.NewObject<Texture>();
  private static readonly ValueParameterKey<Color4> Value = ParameterKeys.NewValue<Color4>();

  [DataMember(10)]
  [Display(nameof(LightMap))]
  [NotNull, System.Diagnostics.CodeAnalysis.NotNull]
  public IComputeColor LightMap { get; set; } = new ComputeTextureColor();

  [DataMember(20)]
  [Display(nameof(Intensity))]
  [NotNull, System.Diagnostics.CodeAnalysis.NotNull]
  public float Intensity { get; set; } = 1f;

  public override void GenerateShader(MaterialGeneratorContext context)
  {
    var shaderSource = new ShaderMixinSource();
    shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingLightmap", Intensity));

    if (LightMap is not null)
    {
      shaderSource.AddComposition(nameof(LightMap), LightMap.GenerateShaderSource(context, new MaterialComputeColorKeys(Map, Value, Color.White)));
    }

    var shaderBuilder = context.AddShading(this);
    shaderBuilder.LightDependentSurface = shaderSource;
  }

  public bool Equals(MaterialLightmapModelFeature? other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    return LightMap.Equals(other.LightMap) && Intensity.Equals(other.Intensity);
  }

  public bool Equals(IMaterialShadingModelFeature? other)
    => other is MaterialLightmapModelFeature otherFeature && Equals(otherFeature);

  public override int GetHashCode() => HashCode.Combine(LightMap, Intensity);
}
