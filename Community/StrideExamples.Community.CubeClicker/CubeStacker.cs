using BepuPhysics;
using Stride.BepuPhysics;
using Stride.BepuPhysics.Constraints;
using Stride.BepuPhysics.Definitions.Colliders;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Graphics;
using Stride.CommunityToolkit.Renderers;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Colors;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using StrideExamples.Community.CubeClicker.Common;
using StrideExamples.Community.CubeClicker.Components;
using StrideExamples.Community.CubeClicker.Scripts;

namespace StrideExamples.Community.CubeClicker;

public class CubeStacker
{
  private const int Seed = 1;
  private Vector3 _startPosition = new(-4, 1, -4);

  private readonly Game _game;
  private readonly Random _random = new(Seed);
  private readonly Dictionary<Color, Material> _materials = [];
  private double _elapsedTime;
  private int _layer = 1;
  private bool _layersCreated;
  private Scene? _scene;

  public CubeStacker(Game game) => _game = game;

  public void Start(Scene scene)
  {
    // _game.SetupBase3DScene();
    _game.Window.AllowUserResizing = true;
    _game.AddGraphicsCompositor().AddCleanUIStage();
    _game.Add3DCamera().Add3DCameraController();
    //_game.AddEntityDebugSceneRenderer(new EntityDebugSceneRendererOptions {  });
    _game.AddSceneRenderer(new EntityTextRenderer());
    _game.AddDirectionalLight();
    _game.Add3DGround();
    _game.AddProfiler();
    _scene = scene;

    AddMaterials();
    AddGizmo(scene);

    //_translationGizmo = new TranslationGizmo(_game.GraphicsDevice);
    //var gizmoEntity = _translationGizmo.Create(scene);
    //gizmoEntity.Transform.Position = new Vector3(-10, 0, 0);

    AddAllDirectionLighting(intensity: 5f);
    AddNewFirstLayer(_startPosition);
    AddFirstLayer(.5f);
    AddGameManagerEntity();
    AddTotalScoreEntity();

    var camera = _scene.GetCamera();
    camera?.Entity.Add(new CameraRotationScript());
    // _simulation = camera?.Entity.GetSimulation();
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

  private void AddGameManagerEntity()
  {
    var entity = new Entity("GameManager")
    {
      new RaycastInteractionScript()
    };
    entity.Scene = _scene;
  }

  private void AddTotalScoreEntity()
  {
    var entity = new Entity(Constants.TotalScore)
    {
      new EntityTextComponent()
      {
        Text = "Total Score: 0",
        FontSize = 20,
        Position = new Vector2(0, 20),
        TextColor = new Color(255, 255, 255),
      }
    };

    entity.Scene = _scene;
  }

  public void Update(Scene scene, GameTime time)
  {
    _elapsedTime += time.Elapsed.TotalSeconds;

    if (_elapsedTime >= Constants.Interval && _layer <= Constants.MaxLayers - 1)
    {
      _elapsedTime = 0;

      var entities = CreateCubeLayer(_layer + .5f);
      // AddColliders(entities)
      _layer++;
    }

    if (!_layersCreated && _layer == Constants.MaxLayers)
    {
      _layersCreated = true;

      foreach (var cube in scene.Entities.Where(e => e.Name == "Cube"))
      {
        if (cube.Get<BodyComponent>() is BodyComponent body)
          body.Kinematic = false;
      }
    }

    // foreach (var cube in scene.Entities.Where(e => e.Name == "Cube"))    
    // {
    //   NOTE: Entirely commented out in sample
    // }
  }

  private void AddMaterials()
  {
    foreach (var color in Constants.Colours)
    {
      var material = CreateMaterial(color, specular: 0);

      _materials.Add(color, material);
    }
  }

  public Material CreateMaterial(Color? color = null, float specular = 1.0f, float microSurface = 0.65f)
  {
    var lightmapMaterial = new MaterialDescriptor
    {
      Attributes =
      {
        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)),
        DiffuseModel = new MaterialLightmapModelFeature()
        {
          Intensity = 20,
          LightMap = new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)
        },
        Specular =  new MaterialMetalnessMapFeature(new ComputeFloat(specular)),
        SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(microSurface))
      }
    };

    return Material.New(_game.GraphicsDevice, lightmapMaterial);
  }

  public Material CreateMaterial2(Color? color = null, float specular = 1, float microSurface = .65f)
  {
    var materialDescription = new MaterialDescriptor
    {
      Attributes =
      {
        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color ?? GameDefaults.DefaultMaterialColor)),
        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
        Specular = new MaterialMetalnessMapFeature(new ComputeFloat(0)),
        SpecularModel = new MaterialSpecularMicrofacetModelFeature()
        {
          Fresnel = new MaterialSpecularMicrofacetFresnelSchlick(),
          Visibility = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX(),
          NormalDistribution = new MaterialSpecularMicrofacetNormalDistributionGGX(),
          Environment = new MaterialSpecularMicrofacetEnvironmentGGXLUT()
        },
        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(0))
      }
    };

    var windowSize = _game.GraphicsDevice.GetWindowSize();
    var whiteTexture = _game.GraphicsDevice.GetSharedWhiteTexture();
    return Material.New(_game.GraphicsDevice, materialDescription);
  }

  private void AddNewFirstLayer(Vector3 startPosition)
  {
    var cube = _game.Create3DPrimitive(PrimitiveModelType.Cube, new()
    {
      EntityName = "Cube1",
      Material = _materials[Constants.Colours[0]],
      Size = Constants.CubeSize
    });
    cube.Transform.Position = startPosition;
    cube.Scene = _scene;

    // var entities = CreateCubeLayer(y, scene);
    // AddColliders(entities)
  }

  private void AddFirstLayer(float y)
  {
    var entities = CreateCubeLayer(y);

    // AddColliders(entities);
  }

  private List<Entity> CreateCubeLayer(float y)
  {
    var entities = new List<Entity>();

    for (var x = 0; x < Constants.Rows; x++)
    {
      for (var z = 0; z < Constants.Rows; z++)
      {
        var entity = CreateCube(Constants.CubeSize);
        entity.Transform.Position = new Vector3(x, y, z) * Constants.CubeSize;
        AddCollider(entity);
        entity.Scene = _scene;

        // entity.AddGizmo(_game.GraphicsDevice);
        // entity.Transform.Children.Add(_translationGizmo.Create(scene).Transform);

        entities.Add(entity);
      }
    }

    return entities;
  }

  private static void AddColliders(List<Entity> entities)
  {
    foreach (var entity in entities)
    {
      AddCollider(entity);
    }
  }

  private static void AddCollider(Entity entity)
  {
    // NOTE: Lots of commented and unused out stuff in this method
    var compoundCollider = new CompoundCollider();
    var boxCollider = new BoxCollider
    {
      Size = Constants.CubeSize,
      Mass = 1000000000
    };

    compoundCollider.Colliders.Add(boxCollider);

    var body = new BodyComponent
    {
      Collider = compoundCollider,
      Kinematic = true,
      BodyInertia = new BodyInertia
      {
        InverseMass = 1 / boxCollider.Mass,
        InverseInertiaTensor = default
      }
    };

    entity.Add(body);

    var linearConstraint = new OneBodyLinearServoConstraintComponent
    {
      ServoMaximumSpeed = float.MaxValue,
      ServoBaseSpeed = 0,
      ServoMaximumForce = 1000,

      A = body,
      Enabled = false
    };

    var angularConstraint = new OneBodyAngularServoConstraintComponent
    {
      ServoMaximumSpeed = float.MaxValue,
      ServoBaseSpeed = 0,
      ServoMaximumForce = 1000,

      A = body,
      Enabled = false
    };
  }

  private Entity CreateCube(Vector3 size)
  {
    var color = Constants.Colours[_random.Next(0, Constants.Colours.Count)];

    var entity = _game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DEntityOptions()
    {
      EntityName = "Cube",
      Material = _materials[color],
      Size = size
    });

    entity.Add(new CubeComponent(color));
    return entity;
  }

  public void AddAllDirectionLighting(float intensity, bool showLightGizmo = true)
  {
    var position = new Vector3(7f, 2f, 0);

    CreateLightEntity(GetLight(), intensity, position);
    CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(180)));
    CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(270)));
    CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(90)));
    CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(270)));

    LightDirectional GetLight() => new() { Color = GetColor(Color.White) };
    static ColorRgbProvider GetColor(Color color) => new(color);
    void CreateLightEntity(ILight light, float intensity, Vector3 position, Quaternion? rotation = null)
    {
      var entity = new Entity
      {
        new LightComponent
        {
          Intensity = intensity,
          Type = light
        }
      };

      entity.Transform.Position = position;
      entity.Transform.Rotation = rotation ?? Quaternion.Identity;
      entity.Scene = _scene;

      if (showLightGizmo)
        entity.AddLightDirectionalGizmo(_game.GraphicsDevice);
    }
  }
}
