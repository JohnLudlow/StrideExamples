using Hexa.NET.ImGui;
using SharpFont.MultipleMasters;
using Stride.Core;
using Stride.Engine;

namespace StrideExamples.Local.ImGui.UI;

public class HierarchyView(IServiceRegistry registry) : BaseWindow(registry)
{
  private string _searchTerm = string.Empty;
  private readonly HashSet<Guid> _recursingThrough = [];
  private readonly List<IIdentifiable> _searchResult = [];
  const float _dummyWidth = 19;
  const float _indentaton2 = _dummyWidth * 8;

  protected override void OnDraw(bool collapsed)
  {
    if (collapsed) return;

    if (Hexa.NET.ImGui.ImGui.InputText("Search", ref _searchTerm, 64))
    {
      _searchResult.Clear();
      if (string.IsNullOrWhiteSpace(_searchTerm))
      {
        RecursiveSearch(_searchResult, _searchTerm, Game.SceneSystem.SceneInstance.RootScene);
      }
    }

    using (UserInterfaceExtension.Child())
    {
      foreach (var identifiable in _searchResult)
      {
        RecursiveDrawing(identifiable);
      }

      if (_searchResult.Count > 0)
      {
        Hexa.NET.ImGui.ImGui.Spacing();
        Hexa.NET.ImGui.ImGui.Spacing();
      }

      foreach (var child in EnumerateChildren(Game.SceneSystem.SceneInstance.RootScene))
        RecursiveDrawing(child);
    }
  }

  protected override void OnDestroy()
  {
  }

  private static void RecursiveSearch(List<IIdentifiable> result, string term, IIdentifiable source)
  {
    if (source is null) return;

    foreach (var child in EnumerateChildren(source))
    {
      RecursiveSearch(result, term, child);
    }

    var lower = source switch
    {
      Entity e => e.Name.ToLowerInvariant(),
      Scene s => s.Name.ToLowerInvariant(),
      _ => string.Empty
    };

    if (
      term.Contains(lower, StringComparison.InvariantCultureIgnoreCase) ||
      lower.Contains(term, StringComparison.InvariantCultureIgnoreCase)
    )
    {
      result.Add(source);
    }
  }

  private void RecursiveDrawing(IIdentifiable source)
  {
    if (source is null) return;

    // TODO: Refactor!
    string? label;
    bool canRecurse;
    {
      if (source is Entity entity)
      {
        label = entity.Name;
        canRecurse = entity.Transform.Children.Count > 0;
      }
      else if (source is Scene scene)
      {
        label = scene.Name;
        canRecurse = scene.Children.Count > 0 || scene.Entities.Count > 0;
      }
      else return;
    }

    using (UserInterfaceExtension.ID(source.Id.GetHashCode()))
    {
      var recursingThrough = _recursingThrough.Contains(source.Id);
      var recurse = canRecurse && recursingThrough;

      if (canRecurse)
      {
        if (Hexa.NET.ImGui.ImGui.ArrowButton("", recurse ? ImGuiDir.Down : ImGuiDir.Right))
        {
          if (recurse)
            _recursingThrough.Remove(source.Id);
          else
            _recursingThrough.Add(source.Id);
        }
      }
      else Hexa.NET.ImGui.ImGui.Dummy(new(_dummyWidth, 1));
      Hexa.NET.ImGui.ImGui.SameLine();

      if (Hexa.NET.ImGui.ImGui.Button(label))
      {
        // TODO: implement
      }

      using (UserInterfaceExtension.UIndent(_indentaton2))
      {
        if (recurse)
        {
          foreach (var child in EnumerateChildren(source))
          {
            RecursiveDrawing(child);
          }
        }
      }
    }
  }

  private static IEnumerable<IIdentifiable> EnumerateChildren(IIdentifiable source)
  {
    if (source is Entity entity)
    {
      foreach (var child in entity.Transform.Children)
      {
        yield return child.Entity;
      }
    }
    else if (source is Scene scene)
    {
      foreach (var childEntity in scene.Entities)
        yield return childEntity;

      foreach (var childScene in scene.Children)
        yield return childScene;
    }
  }  
}