using Stride.CommunityToolkit.ImGui;
using Stride.Core;
using HImGui = Hexa.NET.ImGui;

namespace StrideExamples.Local.ImGui.UI;

public class TestWindow(IServiceRegistry service) : BaseWindow(service)
{
  protected override void OnDestroy()
  {
    throw new NotImplementedException();
  }

  protected override void OnDraw(bool collapsed)
  {
    HImGui.ImGui.Text("Hello from TestWindow!");
  }
}