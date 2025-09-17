using Stride.Engine;
using Stride.Games;
using Stride.Graphics;

namespace StrideExamples.Local.OcclusionTest.Managers;

public class GameManager
{
  private readonly MultiRaycastVisibility _visibility;

  public UIManager UIManager { get; }

  public GameManager(Scene rootScene, SpriteFont font)
  {
    UIManager = new UIManager(rootScene, font);
    _visibility = new MultiRaycastVisibility();
  }
}