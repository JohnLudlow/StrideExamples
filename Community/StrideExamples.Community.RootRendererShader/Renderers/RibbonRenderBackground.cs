using System.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Rendering;

namespace StrideExamples.Community.RootRendererShader.Renderers;

internal sealed class RibbonRenderBackground : RenderObject
{
  public float Intensity { get; set; }
  public float Frequency { get; set; }
  public float Amplitude { get; set; }
  public float Speed { get; set; }
  public Vector3 Top { get; set; }  
  public Vector3 Bottom { get; set; }  
  public float WidthFactor { get; set; }
}