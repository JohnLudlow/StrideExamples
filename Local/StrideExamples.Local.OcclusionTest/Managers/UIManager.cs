using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using Stride.UI.Panels;
using StrideExamples.Local.OcclusionTest.Components;

namespace StrideExamples.Local.OcclusionTest.Managers;

public class UIManager
{
  private const string EntityName = "GameUI";
  private readonly SpriteFont _font;
  private readonly Color _gridBackgroundColor = Color.LightGray;
  private readonly Grid _grid;

  private readonly StackPanel _panel;

  private List<Entity> _monitoredEntities = [];

  public UIManager(Scene rootScene, SpriteFont spriteFont)
  {
    _font = spriteFont;
    _grid = CreateGrid();
    _panel = CreatePanel(_grid);

    var ui = CreateUI(_grid);
    ui.Scene = rootScene;
  }

  internal Entity CreateUI(Grid grid) => new(EntityName)
    {
        new UIComponent
        {
            Page = new UIPage { RootElement = grid },
            RenderGroup = RenderGroup.Group31
        }
    };

  internal void UpdateUI()
  {
    _panel.Children.Clear();
    foreach (var entity in _monitoredEntities)
    {
      var visibility = entity.Get<MultiRaycastVisibilityComponent>();

      if (visibility is not null)
      {
        _panel.Children.Add(CreateTextBlock($"Entity: {entity.Name} ({visibility.LastVisibilityResult.VisibilityPercentage:P}% visible, {visibility.LastVisibilityResult.RaysHit} / {visibility.LastVisibilityResult.TotalRays} rays hit)", 12, TextAlignment.Left));
      }
    }
  }

  public void MonitorEntity(Entity entity)
  {
    if (!_monitoredEntities.Contains(entity))
    {
      _monitoredEntities.Add(entity);
    }
  }

  private StackPanel CreatePanel(Grid grid)
  {
    var panel = CreateVisibilityPanel();
    panel.SetGridColumn(2);
    panel.SetGridRow(1);
    grid.Children.Add(panel);

    return panel;
  }  

  private static Grid CreateGrid() => new()
  {
    VerticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment = HorizontalAlignment.Stretch,
    RowDefinitions = {
          new StripDefinition() { Type = StripType.Auto },
          new StripDefinition() { Type = StripType.Auto },
          new StripDefinition() { Type = StripType.Auto },
          new StripDefinition() { Type = StripType.Auto }
      },
    ColumnDefinitions =
      {
          new StripDefinition(StripType.Star, 1),
          new StripDefinition(StripType.Star, 1),
          new StripDefinition(StripType.Star, 1)
      }
  };

  private StackPanel CreateVisibilityPanel()
  {
    var panel = new StackPanel
    {
      Orientation = Orientation.Vertical,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      BackgroundColor = _gridBackgroundColor,
      Margin = new Thickness(0, 3, 0, 3),
    };

    panel.Children.Add(CreateTextBlock("Visibility:", 14, TextAlignment.Center));
    panel.Children.Add(CreateSlider());

    return panel;
  }
  

  private Slider CreateSlider()
  {
    var slider = new Slider
    {
      Minimum = 0,
      Maximum = 100,
      Value = 50,
      Width = 200,
      Margin = new Thickness(3, 3, 3, 3),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      BackgroundColor = Color.Black,
      MinimumWidth = 100,
      Name = "VisibilitySlider"
    };

    return slider;
  }

  private TextBlock CreateTextBlock(string? text = null, float textSize = 20, TextAlignment textAlignment = TextAlignment.Left)
    => new()
    {
      Text = text,
      TextColor = Color.Black,
      Margin = new Thickness(3, 0, 3, 0),
      TextSize = textSize,
      Font = _font,
      TextAlignment = textAlignment,
      VerticalAlignment = VerticalAlignment.Center
    };
}