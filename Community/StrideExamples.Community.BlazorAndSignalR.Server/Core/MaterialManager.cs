using Stride.Rendering;
using StrideExamples.Community.BlazorAndSignalR.Shared.Core;

namespace StrideExamples.Community.BlazorAndSignalR.Server.Core;

public class MaterialManager
{
  private readonly Dictionary<EntityType, Material> _materials = [];
  private readonly MaterialBuilder _materialBuilder;

  public MaterialManager(MaterialBuilder materialBuilder)
  {
    _materialBuilder = materialBuilder;

    AddMaterials();
  }

  private void AddMaterials()
  {
    foreach (var colorType in Colours.ColourTypes)
    {
      var material = _materialBuilder.CreateMaterial(colorType.Value);
      _materials.Add(colorType.Key, material);
    }
  }

  public Material GetMaterial(EntityType entityType)
  {
    if (_materials.TryGetValue(entityType, out var material))
    {
      return material;
    }

    throw new ArgumentException($"Material for {entityType} not found");
  }
}