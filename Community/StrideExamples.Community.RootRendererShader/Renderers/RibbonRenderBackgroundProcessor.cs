using System.Runtime.CompilerServices;
using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Rendering;

namespace StrideExamples.Community.RootRendererShader.Renderers;

internal sealed class RibbonRenderBackgroundProcessor : EntityProcessor<RibbonBackgroundComponent, RibbonRenderBackground>, IEntityComponentRenderProcessor
{
  public VisibilityGroup? VisibilityGroup { get; set; }

  public RibbonRenderBackground? ActiveBackground { get; private set; }

  protected override void OnSystemRemove()
  {
    if (ActiveBackground != null)
    {
      VisibilityGroup?.RenderObjects.Remove(ActiveBackground);
      ActiveBackground = null;
    }

    base.OnSystemRemove();
  }

  protected override RibbonRenderBackground GenerateComponentData([NotNull] Entity entity, [NotNull] RibbonBackgroundComponent component)
  {
    return new RibbonRenderBackground { Source = component, RenderGroup = component.RenderGroup };
  }

  protected override bool IsAssociatedDataValid([NotNull] Entity entity, [NotNull] RibbonBackgroundComponent component, [NotNull] RibbonRenderBackground associatedData)
  {
    return component == associatedData.Source && component.RenderGroup == associatedData.RenderGroup;
  }

  public override void Draw(RenderContext context)
  {
    var previousBackground = ActiveBackground;
    ActiveBackground = null;

    foreach (var entityKvp in ComponentDatas)
    {
      var backgroundComponent = entityKvp.Key;
      var renderBackground = entityKvp.Value;

      if (backgroundComponent.Enabled)
      {
        renderBackground.Intensity = backgroundComponent.Intensity;
        renderBackground.RenderGroup = backgroundComponent.RenderGroup;
        renderBackground.Speed = backgroundComponent.Speed;
        renderBackground.Frequency = backgroundComponent.Frequency;
        renderBackground.Amplitude = backgroundComponent.Amplitude;
        renderBackground.Top = backgroundComponent.Top.ToVector3();
        renderBackground.Bottom = backgroundComponent.Bottom.ToVector3();
        renderBackground.WidthFactor = backgroundComponent.WidthFactor;

        ActiveBackground = renderBackground;
        break;
      }
    }

    if (ActiveBackground != previousBackground)
    {
      if (previousBackground != null) VisibilityGroup?.RenderObjects.Remove(previousBackground);
      if (ActiveBackground != null) VisibilityGroup?.RenderObjects.Add(ActiveBackground);
    }
  }
}