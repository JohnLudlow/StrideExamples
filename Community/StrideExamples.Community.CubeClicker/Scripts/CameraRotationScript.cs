using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Scripts.Utilities;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace StrideExamples.Community.CubeClicker.Scripts;

public class CameraRotationScript : SyncScript
{
  private const string GroundEntityName = "Ground";
  private readonly float _rotationSpeed = 45f;
  private Vector3 _rotationCentre;
  private DebugTextPrinter? _instructionsPrinter;

  public override void Start()
  {
    var ground = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(e => e.Name == GroundEntityName);
    if (ground is null) return;

    _rotationCentre = ground.Transform.Position;

    InitializeDebugTextPrinter();
  }

  public override void Update()
  {
    DisplayInstructions();

    var deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
    var deltaRotation = 0f;

    if (Input.IsKeyDown(Stride.Input.Keys.Z))
    {
      deltaRotation = -_rotationSpeed * deltaTime;
    }
    else if (Input.IsKeyDown(Stride.Input.Keys.C))
    {
      deltaRotation = +_rotationSpeed * deltaTime;
    }

    if (Math.Abs(deltaRotation) > .001f)
    {
      RotateAroundCentre(deltaRotation);
    }
  }

  private void RotateAroundCentre(float angleDegrees)
  {
    var offset = Entity.Transform.Position - _rotationCentre;
    var yawQuat = Quaternion.RotationY(MathUtil.DegreesToRadians(angleDegrees));
    var rotatedOffset = Vector3.Transform(offset, yawQuat);

    Entity.Transform.Position = _rotationCentre + rotatedOffset;
    Entity.Transform.LookAt(_rotationCentre, Vector3.UnitY);
  }

  private void DisplayInstructions()
  {
    _instructionsPrinter?.Print(GenerateInstructions(Entity.Transform.Position));
  }

  private void InitializeDebugTextPrinter()
  {
      var screenSize = new Int2(Game.GraphicsDevice.Presenter.BackBuffer.Width, Game.GraphicsDevice.Presenter.BackBuffer.Height);
      var instructions = GenerateInstructions(Entity.Transform.Position);

      _instructionsPrinter = new DebugTextPrinter()
      {
        DebugTextSystem = DebugText,
        TextSize = new(205, 17 * instructions.Count),
        ScreenSize = screenSize,
        Instructions = instructions,          
      };

      _instructionsPrinter.Initialize(DisplayPosition.BottomLeft);
  }

  private static List<TextElement> GenerateInstructions(Vector3 cameraPosition)
    => [
          new("GAME INSTRUCTIONS"),
          //new("Click the golden sphere and drag to move it (Y-axis locked)"),
          new("Click a cube", Color.Yellow),
          new("Hold Shift: Left mouse button down", Color.Yellow),
          new("Z/C orbit around the ground", Color.Yellow),
          new($"Camera Position: {cameraPosition}", Color.Yellow),
      ];
}