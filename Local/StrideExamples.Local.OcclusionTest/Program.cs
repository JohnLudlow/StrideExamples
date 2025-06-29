// See https://aka.ms/new-console-template for more information
using SharpDX.WIC;
using Stride.BepuPhysics;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Graphics;
using Stride.CommunityToolkit.ImGui;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Input;
using Stride.Profiling;

Console.WriteLine("Hello, World!");
var _mainCamera = default(CameraComponent);

var _cube1Entity = default(Entity);
var _cube1Body = default(BodyComponent);
var _cube2Entity = default(Entity);
var _cube2Body = default(Entity);

var game = new Game();
game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
    game.SetupBase3DScene();
    game.AddSkybox();
    game.AddProfiler();
    game.AddGroundGizmo(new(-5, 0, -5), showAxisName: true);

    _cube1Entity = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions { Material = game.CreateMaterial(Color.Green) });
    _cube1Entity.Transform.Position = new Vector3(3, 5, 0);
    _cube1Entity.Scene = rootScene;

    _cube2Entity = game.Create3DPrimitive(PrimitiveModelType.Cube, new Primitive3DCreationOptions { Material = game.CreateMaterial(Color.Blue) });
    _cube2Entity.Transform.Position = new Vector3(-3, 5, 0);
    _cube2Entity.Scene = rootScene;

    _mainCamera = rootScene.GetCamera();
    _mainCamera.Entity.Transform.Position = new Vector3(5, 1, 0);
    
    game.SetMaxFPS(60);
}

void Update(Scene rootScene, GameTime gameTime)
{
    var centre = game.GraphicsDevice.GetWindowSize() / 2f;

    if (_mainCamera is not null && _mainCamera.Raycast(new Vector2(centre.X, centre.Y), 100, out var hitInfo))
    {
        if (hitInfo.Collidable.Entity == _cube1Entity)
        {
            game.DebugTextSystem.Print("Cube 1 is visible", new Int2(50, 50));
        }
    }
}