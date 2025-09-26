using System.Collections;

namespace StrideExamples.Local.ImGui.UI;

public enum DisposableTypes
{
  Menu,
  MenuBar,
  Child,
  Window,
  Tooltip,
  Columns,
  Combo,
  ID
}

public struct DisposableImGuiIndent : IDisposable
{
  readonly float _size;

  public DisposableImGuiIndent(float size = 0f)
  {
      _size = size;
      Hexa.NET.ImGui.ImGui.Indent(size);
  }

  public readonly void Dispose()
  {
      Hexa.NET.ImGui.ImGui.Unindent(_size);
  }
}

public struct DisposableImGui(bool dispose, DisposableTypes type) : IDisposable
{

  public void Dispose()
  {
    if (!dispose) return;

    switch (type)
    {
      case DisposableTypes.Menu: Hexa.NET.ImGui.ImGui.EndMenu(); return;
      case DisposableTypes.MenuBar: Hexa.NET.ImGui.ImGui.EndMenuBar(); return;
      case DisposableTypes.Child: Hexa.NET.ImGui.ImGui.EndChild(); return;
      case DisposableTypes.Window: Hexa.NET.ImGui.ImGui.End(); return;
      case DisposableTypes.Tooltip: Hexa.NET.ImGui.ImGui.EndTooltip(); return;
      case DisposableTypes.Columns: Hexa.NET.ImGui.ImGui.Columns(1); return;
      case DisposableTypes.Combo: Hexa.NET.ImGui.ImGui.EndCombo(); return;
      case DisposableTypes.ID: Hexa.NET.ImGui.ImGui.PopID(); return;
      default: throw new ArgumentOutOfRangeException();
    }
  }
}
