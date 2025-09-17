using Myra.Graphics2D.UI;

namespace Stride.Local.MyraUI;

internal sealed class MainView : Panel
{
  // TODO: Add UI elements and logic here

  public HorizontalProgressBar? HealthBar { get; private init; }

  public Window ExampleWindow { get; private init; }

  /// <summary>
  /// Initializes a new instance of the <see cref="MainView"/> class.
  /// </summary>
  public MainView()
  {
    Widgets.Add(UIUtils.CreateHealthBar(-20, "#4BD961FF"));

    var label = new Label
    {
      VerticalSpacing = 10,
      Text = "This is a Test! Hello from Myra! This is a draggable window and below two progress bars."
    };

    ExampleWindow = new Window
    {
      Title = "Hello From Myra",
      Left = 590,
      Top = 200,
      Content = label
    };

    Widgets.Add(ExampleWindow);
  }
}