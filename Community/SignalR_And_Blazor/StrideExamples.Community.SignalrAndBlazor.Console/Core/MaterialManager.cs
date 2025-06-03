
using Stride.Rendering;
using StrideExamples.Community.SignalrAndBlazor.Core.Core;

namespace StrideExamples.Community.SignalrAndBlazor.Console.Core;

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
            _materials.Add(colorType.Key, _materialBuilder.CreateMaterial(colorType.Value));
        }
    }

    public Material GetMaterial(EntityType entityType)
    {
        if (_materials.TryGetValue(entityType, out var material))
        {
            return material;
        }

        throw new ArgumentException($"Material for {entityType} does not exist.");
    }
}