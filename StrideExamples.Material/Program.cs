using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Scripts.Utilities;
using Stride.CommunityToolkit.Skyboxes;
using Stride.CommunityToolkit.DebugShapes.Code;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;

const float _intensityChanegStep = .5f;
float _skyboxIntensity = 0;
DebugTextPrinter? _instructions = null;
LightComponent? _lightComponent = null;

using var game = new Game();
game.Run(start: Start, update: Update);

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

void Start(Scene scene)
{
    game.SetupBase3DScene();
    var skybox = game.AddSkybox();

    _lightComponent = skybox.GetComponent<LightComponent>();
    _skyboxIntensity = _lightComponent?.Intensity ?? 1;

    Create3DPrimitive(scene, new Vector3(-5f, 0.5f, -1f), game.CreateMaterial(Color.Green));
    Create3DPrimitive(scene, new Vector3(-5f, 0.5f, -3f), game.CreateMaterial(Color.Green, 0.1f, 0.1f));
    Create3DPrimitive(scene, new Vector3(-5f, 0.5f, -5f), game.CreateMaterial(Color.Green, 4f, 0.75f));
    Create3DPrimitive(scene, new Vector3(-1f, 0.5f, -1f), GetMaterial1());
    Create3DPrimitive(scene, new Vector3(1f, 0.5f, -1f), GetMaterial2());
    Create3DPrimitive(scene, new Vector3(0f, 0.5f, 1f), GetMaterial3());

    InitializeDebugTextPrinter();    
}

void Create3DPrimitive(Scene scene, Vector3 position, Material material)
{
    var entity = game.Create3DPrimitive(PrimitiveModelType.Cube, new () { Material = material });
    entity.Transform.Position = position;
    entity.Scene = scene;
}

void Update(Scene scene, GameTime time)
{
    if (_lightComponent == null) return;

    if (game.Input.IsKeyPressed(Stride.Input.Keys.Z))
    {
        _skyboxIntensity -= _intensityChanegStep;
        _lightComponent.Intensity = _skyboxIntensity;
    }

    if (game.Input.IsKeyPressed(Stride.Input.Keys.X))
    {
        _skyboxIntensity += _intensityChanegStep;
        _lightComponent.Intensity = _skyboxIntensity;
    }

    DisplayInstructions();
}

Material GetMaterial1()
{
    return Material.New(game.GraphicsDevice, new()
    {
        Attributes = new()
        {
            MicroSurface = new MaterialGlossinessMapFeature
            {
                GlossinessMap = new ComputeFloat(0.1f)
            },
            Diffuse = new MaterialDiffuseMapFeature
            {
                DiffuseMap = new ComputeColor(new Color4(1, 0.3f, 0.5f, 1))
            },
            DiffuseModel = new MaterialDiffuseLambertModelFeature(),
            Specular = new MaterialMetalnessMapFeature
            {
                MetalnessMap = new ComputeFloat(0.9f)
            },
            SpecularModel = new MaterialSpecularMicrofacetModelFeature
            {
                Environment = new MaterialSpecularMicrofacetEnvironmentGGXPolynomial()
            },
        }
    });
}

Material GetMaterial2()
{
    return Material.New(game.GraphicsDevice, new()
    {
        Attributes = new()
        {
            MicroSurface = new MaterialGlossinessMapFeature
            {
                GlossinessMap = new ComputeFloat(0.9f)
            },
            Diffuse = new MaterialDiffuseMapFeature
            {
                DiffuseMap = new ComputeColor(Color.Blue)
            },
            DiffuseModel = new MaterialDiffuseLambertModelFeature(),
            Specular = new MaterialMetalnessMapFeature
            {
                MetalnessMap = new ComputeFloat(0.0f)
            },
            SpecularModel = new MaterialSpecularMicrofacetModelFeature
            {
                Environment = new MaterialSpecularMicrofacetEnvironmentGGXPolynomial()
            },
        }
    });
}

Material GetMaterial3()
{
    return Material.New(game.GraphicsDevice, new()
    {
        Attributes = new()
        {
            MicroSurface = new MaterialGlossinessMapFeature
            {
                GlossinessMap = new ComputeFloat(0.1f)
            },
            Diffuse = new MaterialDiffuseMapFeature
            {
                DiffuseMap = new ComputeColor(Color.Black)
            },
            DiffuseModel = new MaterialDiffuseLambertModelFeature(),
            Specular = new MaterialMetalnessMapFeature
            {
                MetalnessMap = new ComputeFloat(0.8f)
            },
            SpecularModel = new MaterialSpecularMicrofacetModelFeature
            {
                Environment = new MaterialSpecularMicrofacetEnvironmentGGXPolynomial()
            },
        }
    });
}

void DisplayInstructions()
{

    _instructions.Instructions.Clear();
    _instructions.Instructions.AddRange(GenerateInstructions(_skyboxIntensity));
}

void InitializeDebugTextPrinter()
{
    var screenSize = new Int2(game.GraphicsDevice.Presenter.BackBuffer.Width, game.GraphicsDevice.Presenter.BackBuffer.Height);
    
    _instructions = new DebugTextPrinter()
    {
        DebugTextSystem = game.DebugTextSystem,
        TextSize = new(205, 17 * 4),
        ScreenSize = screenSize,
        Instructions = GenerateInstructions(_skyboxIntensity)        
    };

    _instructions.Initialize();
}

static List<TextElement> GenerateInstructions(float skyBoxLightIntensity)
    => [
            new("GAME INSTRUCTIONS"),
            //new("Click the golden sphere and drag to move it (Y-axis locked)"),
            new("Hold Z to decrease, X to increase Skybox light intensity", Color.Yellow),
            new($"Intensity: {skyBoxLightIntensity}", Color.Yellow),
        ];