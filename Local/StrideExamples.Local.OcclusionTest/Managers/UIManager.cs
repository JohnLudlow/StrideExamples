using Stride.Engine;

namespace StrideExamples.Local.OcclusionTest.Managers;

public class UIManager
{
  public Entity CreateUI()
  {
    var entity = new Entity("UI")
    {
      new UIComponent
      {
        Page = new UIPage { RootElement = new Stride.UI.Controls.TextBlock { Text = "Occlusion Test" } },
        RenderGroup = Stride.Rendering.RenderGroup.Group31
      }
    };

    return entity;
  }
}