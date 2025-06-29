using Stride.BepuPhysics;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.Gizmos;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;

// Constants for the impulse strength and sphere properties
const float _impulseStrength = 0.5f;
const float _sphereRadius = 0.5f;

// Game entities and components
var _mainCamera = default(CameraComponent?);
var _sphereEntity = default(Entity?);
var _sphereBody = default(BodyComponent?);
var _vertexBuffer = default(Stride.Graphics.Buffer?);

// Line vertices to visualize the impulse direction (start and end points)
var lineVertices = new Vector3[2];

// Initialize the game instance
using var game = new Game();

game.Run(start: Start, update: Update);

void Start(Scene rootScene) 
{
    game.SetupBase3DScene();
    game.AddSkybox();
    game.AddProfiler();
    game.AddGroundGizmo(new(-5, 0, -5), showAxisName: true);

    _sphereEntity = game.Create3DPrimitive(PrimitiveModelType.Sphere);
    _sphereEntity.Transform.Position = new Vector3(0, 8, 0);

    _sphereBody = _sphereEntity.Get<BodyComponent>();

    _sphereEntity.Scene = rootScene;
    _mainCamera = rootScene.GetCamera();

    var lineEntity = CreateLineEntity(game);
    _sphereEntity.AddChild(lineEntity);
}

void Update(Scene scene, GameTime gameTime) 
{
    if (_mainCamera == null) return;

    DisplayInstructions(game);

    if (game.Input.IsMouseButtonPressed(Stride.Input.MouseButton.Left))
    {
        ProcessMouseClick();
    }
}

void ProcessMouseClick()
{
    if (_mainCamera.Raycast(game.Input.MousePosition, 100, out var hitInfo))
    {
        if (_sphereBody is null || _sphereEntity is null) return;
        
        Console.WriteLine($"Hit entity {hitInfo.Collidable.Entity.Name}");
        
        if (hitInfo.Collidable.Entity == _sphereEntity)
        {
            _sphereBody.LinearVelocity = Vector3.Zero;
            _sphereBody.AngularVelocity = Vector3.Zero;
        }
        else
        {
            UpdateLineVisualization(hitInfo.Point);
            ApplyImpulseToSphere(hitInfo.Point);
        }
    }
    else
    {
        Console.WriteLine("No hit");
    }
}

void UpdateLineVisualization(Vector3 hitPointWorld)
{
    if (_sphereEntity == null || _vertexBuffer == null) return;

    var localHitPoint = Vector3.Transform(hitPointWorld, Matrix.Invert(_sphereEntity.Transform.WorldMatrix));

    lineVertices[1] = localHitPoint.XYZ();

    _vertexBuffer.SetData(game.GraphicsContext.CommandList, lineVertices);
}

void ApplyImpulseToSphere(Vector3 hitPointWorld)
{
    if (_sphereEntity == null || _vertexBuffer == null) return;

    var sphereCenter = _sphereEntity.Transform.WorldMatrix.TranslationVector;
    var direction = hitPointWorld - sphereCenter;

    direction.Normalize();

    var impulse = direction * _impulseStrength;
    var offset = direction * _sphereRadius;

    _sphereBody.ApplyImpulse(impulse, offset);
    _sphereBody.Awake = true;
}

// Creates a line entity to visualize the direction of the applied impulse
Entity CreateLineEntity(Game game)
{
    // Initialize the line vertices.
    // The start point is at the origin; the endpoint is set arbitrarily
    lineVertices[0] = Vector3.Zero;
    lineVertices[1] = new(-1, 1, 1);

    // Create a vertex buffer for the line, with start and end points
    _vertexBuffer = Stride.Graphics.Buffer.New(game.GraphicsDevice, lineVertices, BufferFlags.VertexBuffer);

    // Create an index buffer defining the line's two endpoints
    var indices = new ushort[] { 0, 1 };
    var indexBuffer = Stride.Graphics.Buffer.New(game.GraphicsDevice, indices, BufferFlags.IndexBuffer);

    // Set up the mesh draw parameters for a line list
    var meshDraw = new MeshDraw
    {
        PrimitiveType = PrimitiveType.LineList,
        VertexBuffers = [new VertexBufferBinding(_vertexBuffer, new VertexDeclaration(VertexElement.Position<Vector3>()), lineVertices.Length)],
        IndexBuffer = new IndexBufferBinding(indexBuffer, is32Bit: false, indices.Length),
        DrawCount = indices.Length
    };

    // Create the mesh
    var mesh = new Mesh { Draw = meshDraw };

    // The model is built from the mesh and a gizmo material, an emissive material for clear visualization
    var lineModelComponent = new ModelComponent { Model = [mesh, GizmoEmissiveColorMaterial.Create(game.GraphicsDevice, Color.DarkMagenta)] };

    // Return a new entity that contains the line model component
    return [lineModelComponent];
}

// Displays on-screen instructions to guide the user
static void DisplayInstructions(Game game)
{
    game.DebugTextSystem.Print("Click the ground to apply a direction impulse", new(5, 30));
    game.DebugTextSystem.Print("Click the sphere to stop moving", new(5, 50));
}