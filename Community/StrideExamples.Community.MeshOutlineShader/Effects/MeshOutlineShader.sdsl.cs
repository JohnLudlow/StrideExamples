
using Stride.Core.Mathematics;
using Stride.Rendering;

namespace Stride.Rendering
{
  public static partial class MeshOutlineShaderKeys
  {
    public static readonly ValueParameterKey<Vector4> Color = ParameterKeys.NewValue<Vector4>();
    public static readonly ValueParameterKey<float> Intensity = ParameterKeys.NewValue<float>();
    public static readonly ValueParameterKey<Vector4> Viewport = ParameterKeys.NewValue<Vector4>();
  }
}