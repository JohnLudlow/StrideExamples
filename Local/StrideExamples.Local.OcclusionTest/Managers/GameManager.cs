using Stride.Engine;
using Stride.Graphics;

namespace StrideExamples.Local.OcclusionTest.Managers;

public class GameManager
{
  private readonly UIManager _uiManager;

  public GameManager(SpriteFont font)
  {
    _uiManager = new UIManager();
  }

  internal Entity CreateUI() => _uiManager.CreateUI();
}