using Stride.Input;

namespace StrideExamples.StrideUI.Grid.Core;

public interface IClickable
{
    string Prefix { get; init; }
    int Count { get; set; }
    MouseButton Type { get; }
    void HandleClick();
}