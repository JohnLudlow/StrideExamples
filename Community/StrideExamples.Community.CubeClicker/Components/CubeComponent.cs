using Stride.Core.Mathematics;
using Stride.Engine;

namespace StrideExamples.Community.CubeClicker.Components;

public class CubeComponent : EntityComponent
{
  public Color Color { get; set; }
  public CubeComponent() => Color = Color.Gray;
  public CubeComponent(Color color) => Color = color;
}