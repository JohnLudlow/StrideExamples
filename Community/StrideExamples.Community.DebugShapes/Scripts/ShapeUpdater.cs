using Stride.CommunityToolkit.DebugShapes.Code;
using Stride.Core;
using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Core.Threading;
using Stride.Engine;
using Stride.Physics;

namespace StrideExamples.DebugShapes.Scripts;

public class ShapeUpdater : SyncScript
{
    private enum CurRenderMode : byte
    {
        All = 0,
        Quad,
        Circle,
        Sphere,
        HalfSphere,
        Cube,
        Capsule,
        Cylinder,
        Cone,
        Ray,
        Arrow,
        None
    }

    const int _changePerSecond = 8192 + 2048;
    const int _initialNumPrimitives = 1024 * 1024;
    const int _areaSize = 1024;

    [DataMemberIgnore]
    private ImmediateDebugRenderSystem? _debugDraw; // this is here to make it look like it should when properly integrated

    int _minNumberOfPrimitives;
    int _maxNumberOfPrimitives = 1024 * 1024 * 1024;
    int _currentNumPrimitives = _initialNumPrimitives;
    CurRenderMode _mode = CurRenderMode.All;
    bool _useDepthTesting = true;
    bool _useWireframe = true;
    bool _running = true;
    
    [Obsolete]
    FastList<Vector3> _primitivePositions = new FastList<Vector3>(_initialNumPrimitives);
    [Obsolete]
    FastList<Quaternion> _primitiveRotations = new FastList<Quaternion>(_initialNumPrimitives);
    [Obsolete]
    FastList<Vector3> _primitiveVelocities = new FastList<Vector3>(_initialNumPrimitives);
    [Obsolete]
    FastList<Vector3> _primitiveRotVelocities = new FastList<Vector3>(_initialNumPrimitives);
    [Obsolete]
    FastList<Color> _primitiveColors = new FastList<Color>(_initialNumPrimitives);

    private CameraComponent? _currentCamera;

    [Obsolete]
    private void InitializePrimitives(int from, int to)
    {
        var random = new Random();

        _primitivePositions.Resize(to, true);
        _primitiveRotations.Resize(to, true);
        _primitiveVelocities.Resize(to, true);
        _primitiveRotVelocities.Resize(to, true);
        _primitiveColors.Resize(to, true);

        for (var i = from; i < to; ++i)
        {
            // initialize boxes

            var randX = random.Next(-_areaSize, _areaSize);
            var randY = random.Next(-_areaSize, _areaSize);
            var randZ = random.Next(-_areaSize, _areaSize);

            var velX = random.NextDouble() * 4.0;
            var velY = random.NextDouble() * 4.0;
            var velZ = random.NextDouble() * 4.0;
            var ballVel = new Vector3((float)velX, (float)velY, (float)velZ);

            var rotVelX = random.NextDouble();
            var rotVelY = random.NextDouble();
            var rotVelZ = random.NextDouble();
            var ballRotVel = new Vector3((float)rotVelX, (float)rotVelY, (float)rotVelZ);

            _primitivePositions.Items[i] = new Vector3(randX, randY, randZ);
            _primitiveRotations.Items[i] = Quaternion.Identity;
            _primitiveVelocities.Items[i] = ballVel;
            _primitiveRotVelocities.Items[i] = ballRotVel;

            ref var color = ref _primitiveColors.Items[i];
            color.R = (byte)((_primitivePositions[i].X / _areaSize + 1f) / 2.0f * 255.0f);
            color.G = (byte)((_primitivePositions[i].Y / _areaSize + 1f) / 2.0f * 255.0f);
            color.B = (byte)((_primitivePositions[i].Z / _areaSize + 1f) / 2.0f * 255.0f);
            color.A = 255;
        }
    }

    public override void Start()
    {
        _currentCamera = SceneSystem.SceneInstance.RootScene.Entities.First(e => e.Get<CameraComponent>() != null).Get<CameraComponent>();

        // Gets added in the program.cs class by the AddDebugShapes() extension.
        _debugDraw = Services.GetService<ImmediateDebugRenderSystem>() ?? throw new InvalidOperationException($"Failed to get {nameof(ImmediateDebugRenderSystem)}.");

        _debugDraw.PrimitiveColor = Color.Green;
        _debugDraw.MaxPrimitives = _currentNumPrimitives * 2 + 8;
        _debugDraw.MaxPrimitivesWithLifetime = _currentNumPrimitives * 2 + 8;

        // keep DebugDraw visible in release builds too
        _debugDraw.Visible = true;

        // keep DebugText visible in release builds too
        DebugText.Visible = true;

        InitializePrimitives(0, _currentNumPrimitives);
    }

    private static int Clamp(int v, int min, int max)
    {
        if (v < min)
        {
            return min;
        }
        else if (v > max)
        {
            return max;
        }
        else
        {
            return v;
        }
    }

    [Obsolete]
    public override void Update()
    {
        if (_debugDraw is null) throw new InvalidOperationException($"{_debugDraw} cannot be null");
        var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;

        var speedyDelta = Input.IsKeyDown(Stride.Input.Keys.LeftShift) ? 100.0f : 1.0f;
        var newAmountOfBoxes = Clamp(_currentNumPrimitives + (int)(Input.MouseWheelDelta * _changePerSecond * speedyDelta * dt), _minNumberOfPrimitives, _maxNumberOfPrimitives);

        if (Input.IsKeyPressed(Stride.Input.Keys.LeftAlt))
        {
            _mode = (CurRenderMode)(((int)_mode + 1) % ((int)CurRenderMode.None + 1));
        }

        if (Input.IsKeyPressed(Stride.Input.Keys.LeftCtrl))
        {
            _useDepthTesting = !_useDepthTesting;
        }

        if (Input.IsKeyPressed(Stride.Input.Keys.Tab))
        {
            _useWireframe = !_useWireframe;
        }

        if (Input.IsKeyPressed(Stride.Input.Keys.Space))
        {
            _running = !_running;
        }

        if (newAmountOfBoxes > _currentNumPrimitives)
        {
            InitializePrimitives(_currentNumPrimitives, newAmountOfBoxes);
            _debugDraw.MaxPrimitivesWithLifetime = newAmountOfBoxes * 2 + 8;
            _debugDraw.MaxPrimitives = newAmountOfBoxes * 2 + 8;
            _currentNumPrimitives = newAmountOfBoxes;
        }
        else
        {
            _currentNumPrimitives = newAmountOfBoxes;
        }

        var textPositionX = (int)Input.Mouse.SurfaceSize.X - 800;
        DebugText.Print($"Primitive Count: {_currentNumPrimitives} (scrollwheel to adjust)", new Int2(textPositionX, 32));
        DebugText.Print($" - Render Mode: {_mode} (left alt to switch)", new Int2(textPositionX, 48));
        DebugText.Print($" - Depth Testing: {(_useDepthTesting ? "On " : "Off")} (left ctrl to toggle)", new Int2(textPositionX, 64));
        DebugText.Print($" - Fillmode: {(_useWireframe ? "Wireframe" : "Solid")} (tab to toggle)", new Int2(textPositionX, 80));
        DebugText.Print($" - State: {(_running ? "Simulating" : "Paused")} (space to toggle)", new Int2(textPositionX, 96));

        if (_running)
        {
            Dispatcher.For(0, _currentNumPrimitives, i =>
            {

                ref var position = ref _primitivePositions.Items[i];
                ref var velocity = ref _primitiveVelocities.Items[i];
                ref var rotVelocity = ref _primitiveRotVelocities.Items[i];
                ref var rotation = ref _primitiveRotations.Items[i];
                ref var color = ref _primitiveColors.Items[i];

                if (position.X > _areaSize || position.X < -_areaSize)
                {
                    velocity.X = -velocity.X;
                }

                if (position.Y > _areaSize || position.Y < -_areaSize)
                {
                    velocity.Y = -velocity.Y;
                }

                if (position.Z > _areaSize || position.Z < -_areaSize)
                {
                    velocity.Z = -velocity.Z;
                }

                position += velocity * dt;

                rotation *=
                    Quaternion.RotationX(rotVelocity.X * dt) *
                    Quaternion.RotationY(rotVelocity.Y * dt) *
                    Quaternion.RotationZ(rotVelocity.Z * dt);

                color.R = (byte)((position.X / _areaSize + 1f) / 2.0f * 255.0f);
                color.G = (byte)((position.Y / _areaSize + 1f) / 2.0f * 255.0f);
                color.B = (byte)((position.Z / _areaSize + 1f) / 2.0f * 255.0f);
                color.A = 255;

            });
        }

        var currentShape = 0;

        for (var i = 0; i < _currentNumPrimitives; ++i)
        {
            ref var position = ref _primitivePositions.Items[i];
            ref var rotation = ref _primitiveRotations.Items[i];
            ref var velocity = ref _primitiveVelocities.Items[i];
            ref var rotVelocity = ref _primitiveRotVelocities.Items[i];
            ref var color = ref _primitiveColors.Items[i];

            switch (_mode)
            {
                case CurRenderMode.All:
                    switch (currentShape++)
                    {
                        case 0: // sphere
                            _debugDraw.DrawSphere(position, 0.5f, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 1: // cube
                            _debugDraw.DrawCube(position, new Vector3(1, 1, 1), rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 2: // capsule
                            _debugDraw.DrawCapsule(position, 2.0f, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 3: // cylinder
                            _debugDraw.DrawCylinder(position, 2.0f, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 4: // cone
                            _debugDraw.DrawCone(position, 1.0f, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 5: // ray
                            _debugDraw.DrawRay(position, velocity, color, depthTest: _useDepthTesting);
                            break;
                        case 6: // quad
                            _debugDraw.DrawQuad(position, new Vector2(1.0f), rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 7: // circle
                            _debugDraw.DrawCircle(position, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                            break;
                        case 8: // half sphere
                            _debugDraw.DrawHalfSphere(position, 0.5f, color, rotation, depthTest: _useDepthTesting, solid: !_useWireframe);
                            currentShape = 0;
                            break;
                        default:
                            break;
                    }
                    break;
                case CurRenderMode.Quad:
                    _debugDraw.DrawQuad(position, new Vector2(1.0f), rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.Circle:
                    _debugDraw.DrawCircle(position, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.Sphere:
                    _debugDraw.DrawSphere(position, 0.5f, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.HalfSphere:
                    _debugDraw.DrawHalfSphere(position, 0.5f, color, rotation, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.Cube:
                    _debugDraw.DrawCube(position, new Vector3(1, 1, 1), rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe && i % 2 == 0);
                    break;
                case CurRenderMode.Capsule:
                    _debugDraw.DrawCapsule(position, 2.0f, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.Cylinder:
                    _debugDraw.DrawCylinder(position, 2.0f, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.Cone:
                    _debugDraw.DrawCone(position, 1.0f, 0.5f, rotation, color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.Ray:
                    _debugDraw.DrawRay(position, velocity, color, depthTest: _useDepthTesting);
                    break;
                case CurRenderMode.Arrow:
                    _debugDraw.DrawArrow(position, position + velocity, color: color, depthTest: _useDepthTesting, solid: !_useWireframe);
                    break;
                case CurRenderMode.None:
                    break;
            }
        }

        // CUBE OF ORIGIN!!
        _debugDraw.DrawCube(new Vector3(0, 0, 0), new Vector3(1.0f, 1.0f, 1.0f), color: Color.White);
        _debugDraw.DrawBounds(new Vector3(-5, 0, -5), new Vector3(5, 5, 5), color: Color.White);
        _debugDraw.DrawBounds(new Vector3(-_areaSize), new Vector3(_areaSize), color: Color.HotPink);

        if (Input.IsMouseButtonPressed(Stride.Input.MouseButton.Left) && _currentCamera != null)
        {
            var clickPos = Input.MousePosition;
            var result = ScreenPositionToWorldPositionRaycast(clickPos, _currentCamera, this.GetSimulation());
            if (result.Succeeded)
            {
                var cameraWorldPos = _currentCamera.Entity.Transform.WorldMatrix.TranslationVector;
                var cameraWorldUp = _currentCamera.Entity.Transform.WorldMatrix.Up;
                var cameraWorldNormal = Vector3.Normalize(result.Point - cameraWorldPos);
                _debugDraw.DrawLine(cameraWorldPos + cameraWorldNormal * -2.0f + cameraWorldUp * (-0.125f / 4.0f), result.Point, color: Color.HotPink, duration: 5.0f);
                _debugDraw.DrawArrow(result.Point, result.Point + result.Normal, color: Color.HotPink, duration: 5.0f);
                _debugDraw.DrawArrow(result.Point, result.Point + Vector3.Reflect(result.Point - cameraWorldPos, result.Normal), color: Color.LimeGreen, duration: 5.0f);
            }
        }

        // draw 16 cubes in a line, for testing depth testing disabled stuff
        var startPos = new Vector3(0, 5, 0);
        for (var i = -(16 / 2); i < 16 / 2; ++i)
        {
            _debugDraw.DrawCube(startPos + new Vector3(i, 0, 0), Vector3.One, depthTest: _useDepthTesting, solid: true);
            _debugDraw.DrawCube(startPos + new Vector3(i, 0, 0), Vector3.One, depthTest: _useDepthTesting, solid: false, color: Color.White);
        }

        // draw every primitive to see where they're put
        var testPos = new Vector3(0.0f, 0.0f, -5.0f);
        _debugDraw.PrimitiveColor = Color.Red;
        _debugDraw.DrawQuad(testPos + new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f), solid: !_useWireframe);
        _debugDraw.DrawCircle(testPos + new Vector3(2.0f, 0.0f, 0.0f), 0.5f, solid: !_useWireframe);
        _debugDraw.DrawSphere(testPos + new Vector3(3.0f, 0.0f, 0.0f), 0.5f, solid: !_useWireframe);
        _debugDraw.DrawCube(testPos + new Vector3(4.0f, 0.0f, 0.0f), new Vector3(1.0f), solid: !_useWireframe);
        _debugDraw.DrawCapsule(testPos + new Vector3(5.0f, 0.0f, 0.0f), 1.0f, 0.5f, solid: !_useWireframe);
        _debugDraw.DrawCylinder(testPos + new Vector3(6.0f, 0.0f, 0.0f), 1.0f, 0.5f, solid: !_useWireframe);
        _debugDraw.DrawCone(testPos + new Vector3(7.0f, 0.0f, 0.0f), 1.0f, 0.5f, solid: !_useWireframe);
        _debugDraw.DrawHalfSphere(testPos + new Vector3(8.0f, 0.0f, 0.0f), 0.5f, solid: !_useWireframe);
        _debugDraw.DrawHalfSphere(testPos + new Vector3(9.0f, 0.0f, 0.0f), 0.5f, rotation: Quaternion.RotationX((float)Math.PI), solid: !_useWireframe);

        // center cone thing yes
        _debugDraw.DrawCone(new Vector3(0, 0.5f, 0), 2.0f, 0.5f, color: Color.HotPink);

    }

    // ToDo, this method is already in CameraComponentExtensions?
    public static HitResult ScreenPositionToWorldPositionRaycast(Vector2 screenPos, CameraComponent camera, Simulation simulation)
    {
        var invertedViewProjection = Matrix.Invert(camera.ViewProjectionMatrix);

        // Reconstruct the projection-space position in the (-1, +1) range.
        // Don't forget that Y is down in screen coordinates, but up in projection space
        Vector3 sPos;
        sPos.X = screenPos.X * 2f - 1f;
        sPos.Y = 1f - screenPos.Y * 2f;

        // Compute the near (start) point for the raycast
        // It's assumed to have the same projection space (x,y) coordinates and z = 0 (lying on the near plane)
        // We need to unproject it to world space
        sPos.Z = 0f;
        var vectorNear = Vector3.Transform(sPos, invertedViewProjection);
        vectorNear /= vectorNear.W;

        // Compute the far (end) point for the raycast
        // It's assumed to have the same projection space (x,y) coordinates and z = 1 (lying on the far plane)
        // We need to unproject it to world space
        sPos.Z = 1f;
        var vectorFar = Vector3.Transform(sPos, invertedViewProjection);
        vectorFar /= vectorFar.W;

        // Raycast from the point on the near plane to the point on the far plane and get the collision result
        var result = simulation.Raycast(vectorNear.XYZ(), vectorFar.XYZ());

        return result;
    }
}