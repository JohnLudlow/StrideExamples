using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
using Stride.Graphics;

namespace StrideExamples.Local.ImGui.UI;

// TODO: refactorings
// [ ] unit tests
// [ ] deal with warnings
// [ ] convert to OOP controls with dispose pattern

public partial class UserInterfaceExtension
{
  private static readonly List<Texture> _textureRegistry = [];

  /// <summary>
  /// Adds a texture to the registry and returns its index
  /// </summary>
  /// <param name="texture">The texture to add</param>
  /// <returns>The index of the added texture</returns>
  internal static ulong GetTextureKey(Texture texture)
  {
    _textureRegistry.Add(texture);
    return (ulong)_textureRegistry.Count;
  }

  /// <summary>
  /// Attempts to convert a pointer to a texture if its in the <see cref="_textureRegistry"/>
  /// </summary>
  /// <param name="key"></param>
  /// <param name="texture"></param>
  /// <returns></returns>
  internal static bool TryGetTexture(ulong key, out Texture texture)
  {
    var index = (int)key - 1;

    if (index >= 0 && index < _textureRegistry.Count)
    {
      texture = _textureRegistry[index];
      return true;
    }

    texture = null;
    return false;
  }

  /// <summary>
  /// Clears the dictionaries that contain the mappings between textures and their reference ids:
  /// <see cref="_textureRegistry"/> <see cref="_pointerRegistry"/>
  /// </summary>
  internal static void ClearTextures()
  {
    _textureRegistry.Clear();
  }

  public static DisposableImGui ID(string id)
  {
    Hexa.NET.ImGui.ImGui.PushID(id);
    return new DisposableImGui(true, DisposableTypes.ID);
  }

  public static DisposableImGui ID(int id)
  {
    Hexa.NET.ImGui.ImGui.PushID(id);
    return new DisposableImGui(true, DisposableTypes.ID);
  }

  public static DisposableImGui UCombo(string label, string previewValue, out bool open, ImGuiComboFlags flags = ImGuiComboFlags.None)
  {
    return new DisposableImGui(
      open = Hexa.NET.ImGui.ImGui.BeginCombo(label, previewValue, flags),
      DisposableTypes.ID
    );
  }

  public static DisposableImGui ToolTip()
  {
    Hexa.NET.ImGui.ImGui.BeginTooltip();
    return new DisposableImGui(true, DisposableTypes.Tooltip);
  }

  public static DisposableImGuiIndent UIndent(float size = 0f) => new(size);

  public static DisposableImGui UColumns(int count, string id = null, bool border = false)
  {
    Hexa.NET.ImGui.ImGui.Columns(count, id, border);
    return new DisposableImGui(true, DisposableTypes.Columns);
  }

  public static DisposableImGui Window(string name, ref bool open, out bool collapsed, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
  {
    collapsed = !Hexa.NET.ImGui.ImGui.Begin(name, ref open, flags);
    return new DisposableImGui(true, DisposableTypes.Window);
  }

  public static unsafe DisposableImGui Child(
    [CallerLineNumber] int line = 0,
    Vector2 size = default,
    ImGuiChildFlags childFlags = ImGuiChildFlags.None,
    ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None
  )
  {
    Hexa.NET.ImGui.ImGui.BeginChild(line.ToString(CultureInfo.InvariantCulture), size, childFlags, windowFlags);
    return new DisposableImGui(true, DisposableTypes.Child);
  }

  public static bool ColorPicker3(string label, ref Stride.Core.Mathematics.Color3 color)
  {
    var lightColorVector = new Vector3(color.R, color.G, color.B);
    var changed = Hexa.NET.ImGui.ImGui.ColorPicker3(label, ref lightColorVector);
    if (changed)
    {
      color.R = lightColorVector.X;
      color.G = lightColorVector.Y;
      color.B = lightColorVector.Z;
    }

    return changed;
  }

  public static void Image(Texture texture)
  {
    Hexa.NET.ImGui.ImGui.Image(GetTextureKey(texture), new Vector2(texture.Width, texture.Height));
  }

  public static void Image(Texture texture, int width, int height)
  {
    Hexa.NET.ImGui.ImGui.Image(GetTextureKey(texture), new Vector2(width, height));
  }

  public static bool ImageButton(string text, Texture texture)
  {
    return Hexa.NET.ImGui.ImGui.ImageButton(text, GetTextureKey(texture), new Vector2(texture.Width, texture.Height));
  }

  public static DisposableImGui MenuBar(out bool open) => new(open = Hexa.NET.ImGui.ImGui.BeginMenuBar(), DisposableTypes.MenuBar);

  public static DisposableImGui Menu(string label, out bool open, bool enabled = true) => new(open = Hexa.NET.ImGui.ImGui.BeginMenu(label, enabled), DisposableTypes.MenuBar);

  public static void PlotLines
  (
    string label,
    ref float values,
    int count,
    int offset = 0,
    string overlay = null,
    float valueMin = float.MinValue,
    float valueMax = float.MaxValue,
    Vector2 size = default,
    int stride = 4
  )
  {
    Hexa.NET.ImGui.ImGui.PlotLines(label, ref values, count, offset, overlay, valueMin, valueMax, size, stride);
  }
}