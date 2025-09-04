using System;
using Stride.BepuPhysics;
using Stride.BepuPhysics.Constraints;
using Stride.BepuPhysics.Definitions.Colliders;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Local.BlockGridGenerator.Components;
using Stride.Local.BlockGridGenerator.Scripts;
using Stride.Rendering;
using Stride.Rendering.Colors;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using StrideExamples.Local.BlockGridGenerator.Common;

namespace Stride.Local.BlockGridGenerator;

public class BlockGrid(Game game)
{
  private const int Seed = 1;
  private Vector3 _startPosition = new(-4, 1, -4);
  private readonly Game _game = game;
  private readonly Random _random = new(Seed);
  private readonly Dictionary<Color, Material> _materials = [];
  private double _elapsedTime;
  private int _layer = 1;
  private Scene? _scene;

  public void Start(Scene scene)
  {
    _game.Window.AllowUserResizing = true;
    _game.AddGraphicsCompositor().AddCleanUIStage();
    _game.Add3DCamera().Add3DCameraController();
    _game.Add3DGround();
    _game.AddProfiler();

    _scene = scene;

    AddGizmo(scene);
    AddAllDirectionLighting(intensity: 5);
    AddMaterials();
    AddNewFirstLayer(_startPosition);
    CreateCubeLayer(.5f);

  }

  public void Update(Scene scene, GameTime time)
  {

  }

  private void AddGameManagerEntity()
  {
    var entity = new Entity("GameManager")
    {
      new RaycastInteractionScript()
    };
    entity.Scene = _scene;
  }

  private void AddGizmo(Scene scene)
  {
    var entity = new Entity("MyGizmo")
    {
      Scene = scene
    };

    entity.Transform.Position = new Vector3(-7.5f, 1, -7.5f);
    entity.AddGizmo(_game.GraphicsDevice, showAxisName: true);
  }

  private void AddMaterials()
  {
    foreach (var color in Constants.Colours)
    {
      _materials.Add(color, CreateMaterial(color, specular: 0));
    }
  }

  public Material CreateMaterial(Color? color = null, float specular = 1f, float microSurface = .65f)
  {
    var lightmapMaterial = new MaterialDescriptor
    {
      Attributes =
      {
        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)),
        DiffuseModel = new MaterialLightmapModelFeature(),
        Specular = new MaterialMetalnessMapFeature(new ComputeFloat(specular)),
        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(microSurface))
      }
    };

    return Material.New(_game.GraphicsDevice, lightmapMaterial);
  }

  public void AddAllDirectionLighting(float intensity, bool showLightGizmo = true)
  {
    var position = new Vector3(7f, 2f, 0);
    var rotations = new[] { 180, 270, 90, 0 };

    foreach (var rotation in rotations)
    {
      var lightEntity = new Entity
      {
        new LightComponent
        {
          Intensity = intensity,
          Type = new LightDirectional() { Color = new ColorRgbProvider(Color.White) }
        }
      };

      lightEntity.Transform.Position = position;
      lightEntity.Transform.Rotation = Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(rotation));
      lightEntity.Scene = _scene;

      if (showLightGizmo) lightEntity.AddLightDirectionalGizmo(_game.GraphicsDevice);
    }
  }

  private List<Entity> CreateCubeLayer(float y)
  {
    var entities = new List<Entity>(Constants.Rows * Constants.Rows);

    for (var x = 0; x < Constants.Rows; x++)
    {
      for (var z = 0; z < Constants.Rows; z++)
      {
        var entity = CreateCube(Constants.CubeSize);
        entity.Scene = _scene;
        entity.Transform.Position = new Vector3(x, y, z) * Constants.CubeSize;

        entities.Add(entity);
      }
    }

    return entities;
  }

  private Entity CreateCube(Vector3 size)
  {
    var color = Constants.Colours[_random.Next(0, Constants.Colours.Count)];
    var entity = _game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DEntityOptions()
      {
        Size = size,
        Material = _materials[color],
        EntityName = "Cube",
      }
    );

    entity.Add(new CubeComponent(color));
    return entity;
  }

  private void AddNewFirstLayer(Vector3 startPosition)
  {
    var cube = _game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
      EntityName = "Cube1",
      Size = Constants.CubeSize
    });

    cube.Transform.Position = startPosition;
    cube.Scene = _scene;
  }
}
