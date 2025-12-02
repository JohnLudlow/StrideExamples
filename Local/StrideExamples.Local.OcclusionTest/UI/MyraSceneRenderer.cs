using Myra;
using Myra.Graphics2D.UI;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Compositing;

namespace StrideExamples.Local.OcclusionTest.UI;

public class MyraSceneRenderer : SceneRendererBase
{
  private Desktop? _desktop;
  private SceneEditorView? _sceneEditorView;

  protected override void InitializeCore()
  {
    base.InitializeCore();

    Console.WriteLine("MyraSceneRenderer: InitializeCore called");

    MyraEnvironment.Game = Services.GetService<IGame>() as Game ?? throw new InvalidOperationException("Failed to get Game service");

    Console.WriteLine($"MyraEnvironment.Game set: {MyraEnvironment.Game != null}");

    InitializeSceneEditorView();
    InitializeDesktop();

    Console.WriteLine("MyraSceneRenderer: Initialization complete");
  }

  private void InitializeSceneEditorView()
  {
    _sceneEditorView = new SceneEditorView();
    Services.AddService(_sceneEditorView);
    Console.WriteLine($"MyraSceneRenderer: SceneEditorView created and added to services");
  }

  private void InitializeDesktop()
  {
    _desktop = new Desktop
    {
      Root = _sceneEditorView
    };
    Console.WriteLine($"MyraSceneRenderer: Desktop created with root = {_desktop.Root != null}");
  }

  protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
  {
    drawContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
    _desktop?.Render();
  }
}
