using Stride.CommunityToolkit.ImGui;
using Stride.Core.Mathematics;
using Stride.Core;
using Stride.Engine;
using Stride.Games;
using StrideExamples.Local.ImGui.UI;

public abstract class BaseWindow : GameSystem
{
  private Lock _lock = new();
  private static Dictionary<string, uint> _windowId = [];
  private bool _open = true;
  private readonly uint _id;

  private ImGuiSystem _imgui;
  private readonly string _uniqueName;
  protected bool Open => _open;
  protected uint Id => _id;
  protected virtual Hexa.NET.ImGui.ImGuiWindowFlags WindowFlags => Hexa.NET.ImGui.ImGuiWindowFlags.None;
  protected virtual Vector2? WindowPosition => null;
  protected virtual Vector2? WindowSize => null;

  public float Scale => _imgui.Scale;

  protected BaseWindow(IServiceRegistry registry) : base(registry)
  {
    Game.GameSystems.Add(this);
    Enabled = true;

    var n = GetType().Name;
    lock (_lock)
    {
      // TODO: make this be like the sample if there are issues

      if (!_windowId.TryGetValue(n, out var _id))
      {
        _id = 1;
        _windowId.Add(n, Id);
      }

      _windowId[n] = Id + 1;
    }

    _uniqueName = Id == 1 ? n : $"{n}({Id})";
    _imgui ??= Services?.GetService<ImGuiSystem>()!;
  }

  public override void Update(GameTime gameTime)
  {
    _imgui ??= Services?.GetService<ImGuiSystem>()!;
    if (_imgui is null) return;

    if (UpdateOrder <= _imgui.UpdateOrder)
    {
      UpdateOrder = _imgui.UpdateOrder + 1;
      return;
    }

    if (WindowPosition is not null)
      Hexa.NET.ImGui.ImGui.SetNextWindowPos(WindowPosition.Value);

    if (WindowSize is not null)
      Hexa.NET.ImGui.ImGui.SetNextWindowSize(WindowSize.Value);

    using (UserInterfaceExtension.Window(_uniqueName, ref _open, out var collapsed, WindowFlags))
    {
      OnDraw(collapsed);
    }

    if (!Open)
    {
      Enabled = false;
      Dispose();
    }
  }

  protected abstract void OnDraw(bool collapsed);
  protected abstract void OnDestroy();
  protected override void Destroy()
  {
    Game.GameSystems.Remove(this);
    OnDestroy();
    base.Destroy();
  }
}