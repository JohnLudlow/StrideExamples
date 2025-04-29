using System;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Core;
using Stride.Rendering;
using Stride.Core.Annotations;
using Stride.Rendering.ProceduralModels;
using System.Globalization;

namespace StrideExamples.GalaxyCreator;

public class GalaxyCreator : StartupScript
{
    [DataMember]
    [DataMemberRange(10, 40, 1, 15, 0)]
    public int SphereRadiusFactor { get; set; } = 20; //10~30
    [DataMember]
    public int StarsCount { get; set; } = 8000;

    [DataMember]
    public int ArmsCount { get; set; } = 2;
    [DataMember]
    [DataMemberRange(0, 5, 0.5, 1, 1)]
    public float TwistValue { get; set; } = 4f;
    [DataMember]
    public List<Material> MaterialsList { get; set; } = [];
    [DataMember]
    public bool MaterialInstancePerObject { get; set; } = false;
    private Vector2[] _points; // All objects will place in one plane, we use only two coordinates
    private Material[] _materials; // Array for random using materials
    private static Vector2[] GenerateGalaxy(int numOfStars, int numOfArms, float spin, double armSpread, double starsAtCenterRatio)
    {
        List<Vector2> result = new(numOfStars);
        for (var i = 0; i < numOfArms; i++)
        {
            result.AddRange(ArmsCreator(numOfStars / numOfArms, i / numOfArms, spin, armSpread, starsAtCenterRatio));
        }
        return [.. result];
    }

    private static Vector2[] ArmsCreator(int numOfStars, float rotation, float spin, double armSpread, double starsAtCenterRatio)
    {
        var result = new Vector2[numOfStars];
        Random r = new();

        for (var i = 0; i < numOfStars; i++)
        {
            double part = i / numOfStars;
            part = Math.Pow(part, starsAtCenterRatio);

            var distanceFromCenter = (float)part;
            var position = (part * spin + rotation) * Math.PI * 2;

            var xBeating = (Dispersal(r.NextDouble()) - Dispersal(r.NextDouble())) * armSpread;
            var yBeating = (Dispersal(r.NextDouble()) - Dispersal(r.NextDouble())) * armSpread;

            var resultX = (float)Math.Cos(position) * distanceFromCenter / 2 + 0.5f + (float)xBeating;
            var resultY = (float)Math.Sin(position) * distanceFromCenter / 2 + 0.5f + (float)yBeating;

            result[i] = new Vector2(resultX, resultY);
        }

        return result;
    }

    private static double Dispersal(double x)
    {
        var value = Math.Pow(x - 0.5, 3) * 4 + 0.5d;
        return Math.Max(Math.Min(1, value), 0);
    }

    private Model MModel()
    {
        var sphere = new SphereProceduralModel() { Radius = 0.02f / SphereRadiusFactor };
        var model = sphere.Generate(Services);
        return model;
    }

    public override void Start()
    {
        var materialInstance = default(MaterialInstance);
        // Initialization of the script.
        if (MaterialsList.Count != 0) { _materials = [.. MaterialsList]; }
        else { return; } // If no materials skip generation

        if (!MaterialInstancePerObject)
        {
            materialInstance = new()
            {
                Material = _materials[new Random().Next(_materials.Length)]
            };
        }
        _points = GenerateGalaxy(StarsCount, ArmsCount, TwistValue, 0.1d, 1);
        // fine looking, but need more VRAM -  GenerateGalaxy(80000, 2, 3f, 0.1d, 3); 
        for (var i = 0; i < _points.Length; i++)
        {
            Entity e = new(name: i.ToString(CultureInfo.InvariantCulture), new Vector3(new Vector2(_points[i].X, _points[i].Y), 0.0f), rotation: null, scale: null);
            //Log.Debug(e.Name);
            SceneSystem.SceneInstance.RootScene.Entities.Add(e);
            //Entity.Scene.Entities.Add(e);
            if (MaterialInstancePerObject)
            {
                materialInstance = new()
                {
                    Material = _materials[new Random().Next(_materials.Length)]
                };
            }
            var Mats = MModel();
            Mats.Materials.Insert(0, materialInstance);
            //MModel().Materials.Add(materialInstance); 
            e.GetOrCreate<ModelComponent>().Model = Mats;
        }
    }
}