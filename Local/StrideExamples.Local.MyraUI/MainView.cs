using Myra;
using Myra.Graphics2D.UI;
using SharpDX.Direct3D12;
using Stride.Engine;
using Stride.Local.MyraUI;

namespace StrideExamples.Local.MyraUI;

internal sealed class MainView : Panel
{
  // TODO: Add UI elements and logic here

  public HorizontalProgressBar? HealthBar { get; private init; }

  public Window ExampleWindow { get; private init; }

  /// <summary>
  /// Initializes a new instance of the <see cref="MainView"/> class.
  /// </summary>
  public MainView(Scene rootScene)
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


    var tree = new Myra.Graphics2D.UI.TreeView();

    PopulateTree(tree, rootScene.Entities);
    var treeWindow = new Window
    {
      Title = "Scene Tree",
      Left = 590,
      Top = 200,
      Content = tree
    };

    Widgets.Add(treeWindow);

    Widgets.Add(ExampleWindow);
  }

  private static void PopulateTree(TreeView node, IEnumerable<Entity> entities)
  {
    foreach (var entity in entities)
    {
      var childNode = node.AddSubNode( new Label { Text = entity.Name } );
      PopulateTree(childNode, entity.GetChildren());
    }
  }

  private static void PopulateTree(TreeViewNode node, IEnumerable<Entity> entities)
  {
    foreach (var entity in entities)
    {
      var childNode = node.AddSubNode(new Label(entity.Name));
      PopulateTree(childNode, entity.GetChildren());
    }
  }
}