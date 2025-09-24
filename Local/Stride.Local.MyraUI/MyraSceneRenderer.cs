using Myra;
using Myra.Graphics2D.UI;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering;
using Stride.Rendering.Compositing;

namespace Stride.Local.MyraUI;

public class MyraSceneRenderer : SceneRendererBase
{
  private Desktop? _desktop;
  private MainView? _mainView;

  protected override void InitializeCore()
  {
    base.InitializeCore();

    MyraEnvironment.Game = Services.GetService<IGame>() as Game ?? throw new InvalidOperationException("Failed to get Game service");

    InitializeMainView();
    InitializeDesktop();
  }

  /// <summary>
  /// Initializes the main view and adds it to the Stride services.
  /// </summary>
  private void InitializeMainView()
  {
    _mainView = new MainView(MyraEnvironment.Game.SceneSystem.SceneInstance.RootScene);

    Services.AddService(_mainView);
  }

  /// <summary>
  /// Initializes the desktop and sets the root view.
  /// </summary>
  private void InitializeDesktop()
  {
    _desktop = new Desktop
    {
      Root = _mainView
    };
  }

  protected override void DrawCore(Rendering.RenderContext context, RenderDrawContext drawContext)
  {
    // TODO: Implement Myra UI rendering here
    drawContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, Graphics.DepthStencilClearOptions.DepthBuffer);
    _desktop?.Render();
  }
}
