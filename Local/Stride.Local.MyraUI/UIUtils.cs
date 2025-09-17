using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Stride.Local.MyraUI;

public static class UIUtils
{
  public static HorizontalProgressBar CreateHealthBar(int top, string filler)
  {
    var healthBar = new HorizontalProgressBar
    {
      HorizontalAlignment = HorizontalAlignment.Left,
      VerticalAlignment = VerticalAlignment.Bottom,

      Value = 100,
      Minimum = 0,
      Maximum = 100,

      Top = top,
      Left = 20,

      Width = 300,
      Height = 20,
      Background = new SolidBrush("#202020FF"),
      Filler = new SolidBrush(filler),
      Margin = new Myra.Graphics2D.Thickness(10)
    };

    return healthBar;
  }
}
