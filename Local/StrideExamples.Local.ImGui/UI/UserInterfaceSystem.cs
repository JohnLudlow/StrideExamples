using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Games;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;

namespace StrideExamples.Local.ImGui.UI;

public class UserInterfaceSystem : GameSystemBase
{
  const int initialVertexBufferSize = 128;
  const int initialIndexBufferSize = 128;

  private float _scale = 1;
  private ImGuiIOPtr _io;
  private ImGuiPlatformIOPtr _platform;

  #region dependencies
  private readonly InputManager? _input;
  private readonly GraphicsDevice? _graphicsDevice;
  private readonly GraphicsDeviceManager? _graphicsDeviceManager;
  private readonly GraphicsContext? _graphicsContext;
  private readonly EffectSystem? _effectSystem;
  private CommandList? _commandList;
  #endregion

  #region device objects
  private PipelineState? _imPipeline;
  private VertexDeclaration? _imVertLayout;
  private VertexBufferBinding _vertexBufferBinding;
  private IndexBufferBinding? _indexBufferBinding;
  private EffectInstance? _imShader;
  private Texture _fontTexture;
  #endregion

  private readonly Dictionary<Keys, ImGuiKey> _keys = [];
  private bool _isFirstFrame = true;

  public UserInterfaceSystem(
    [NotNull] IServiceRegistry registry,
    [NotNull] GraphicsDeviceManager graphicsDeviceManager,
    InputManager? inputManager = null
  ) : base(registry)
  {
    _input = inputManager ?? Services.GetService<InputManager>();
    Debug.Assert(_input is not null, $"[{nameof(UserInterfaceSystem)}] InputManager must be available!");

    _graphicsDeviceManager = graphicsDeviceManager;
    Debug.Assert(_graphicsDeviceManager is not null, $"[{nameof(UserInterfaceSystem)}] GraphicsDeviceManager must be available!");

    _graphicsDevice = _graphicsDeviceManager.GraphicsDevice;
    Debug.Assert(_graphicsDevice is not null, $"[{nameof(UserInterfaceSystem)}] GraphicsDevice must be available!");

    _graphicsContext = Services.GetService<GraphicsContext>();
    Debug.Assert(_graphicsContext is not null, $"[{nameof(UserInterfaceSystem)}] GraphicsContext must be available!");

    _effectSystem = Services.GetService<EffectSystem>();
    Debug.Assert(_effectSystem is not null, $"[{nameof(UserInterfaceSystem)}] EffectSystem must be available!");

    ImGuiContext = Hexa.NET.ImGui.ImGui.CreateContext();
    Hexa.NET.ImGui.ImGui.SetCurrentContext(ImGuiContext);

    _io = Hexa.NET.ImGui.ImGui.GetIO();
    SetupInput();

    CreateDeviceObjects();
    CreateFontTexture();

    Enabled = true;
    Visible = true;
    UpdateOrder = 1;

    Services.AddService(this);
    Game.GameSystems.Add(this);
  }

  public float Scale
  {
    get => _scale;
    set
    {
      _scale = value;
      CreateFontTexture();
    }
  }

  public ImGuiContextPtr ImGuiContext { get; }

  protected override void Destroy()
  {
    Hexa.NET.ImGui.ImGui.DestroyContext(ImGuiContext);
    base.Destroy();
  }

  [FixedAddressValueType]
  static SetClipboardDelegate _setClipboardDelegate;

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate void SetClipboardDelegate(IntPtr data);

  [FixedAddressValueType]
  static GetClipboardDelegate _getClipboardDelegate;

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate IntPtr GetClipboardDelegate();

  void SetClipboard(IntPtr data)
  {

  }

  unsafe IntPtr GetClipboard()
  {
    return (nint)_platform.PlatformClipboardUserData;
  }

  private unsafe void SetupInput()
  {
    _io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

    _keys.Add(Keys.Tab, ImGuiKey.Tab);
    _keys.Add(Keys.Left, ImGuiKey.LeftArrow);
    _keys.Add(Keys.Right, ImGuiKey.RightArrow);
    _keys.Add(Keys.Up, ImGuiKey.UpArrow);
    _keys.Add(Keys.Down, ImGuiKey.DownArrow);
    _keys.Add(Keys.PageUp, ImGuiKey.PageUp);
    _keys.Add(Keys.PageDown, ImGuiKey.PageDown);
    _keys.Add(Keys.Home, ImGuiKey.Home);
    _keys.Add(Keys.End, ImGuiKey.End);
    _keys.Add(Keys.Delete, ImGuiKey.Delete);
    _keys.Add(Keys.Back, ImGuiKey.Backspace);
    _keys.Add(Keys.Enter, ImGuiKey.Enter);
    _keys.Add(Keys.Escape, ImGuiKey.Escape);
    _keys.Add(Keys.Space, ImGuiKey.Space);
    _keys.Add(Keys.A, ImGuiKey.A);
    _keys.Add(Keys.C, ImGuiKey.C);
    _keys.Add(Keys.V, ImGuiKey.V);
    _keys.Add(Keys.X, ImGuiKey.X);
    _keys.Add(Keys.Y, ImGuiKey.Y);
    _keys.Add(Keys.Z, ImGuiKey.Z);

    _setClipboardDelegate = SetClipboard;
    _getClipboardDelegate = GetClipboard;

    _platform.PlatformSetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(_setClipboardDelegate);
    _platform.PlatformGetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(_getClipboardDelegate);

    throw new NotImplementedException();
  }

  private void CreateDeviceObjects()
  {
    _commandList = _graphicsContext?.CommandList;
    _imShader = new EffectInstance(_effectSystem.LoadEffect("ImGuiShader").WaitForResult());
    _imShader.UpdateEffect(_graphicsDevice);

    _imVertLayout = new VertexDeclaration(
      VertexElement.Position<Vector2>(),
      VertexElement.TextureCoordinate<Vector2>(),
      VertexElement.Color(PixelFormat.R8G8B8A8_UNorm)
    );

    var pipeline = new PipelineStateDescription
    {
      BlendState = BlendStates.NonPremultiplied,
      RasterizerState = new RasterizerStateDescription
      {
        CullMode = CullMode.None,
        DepthBias = 0,
        FillMode = FillMode.Solid,
        MultisampleAntiAliasLine = false,
        ScissorTestEnable = true,
        SlopeScaleDepthBias = 0
      },
      PrimitiveType = PrimitiveType.TriangleList,
      InputElements = _imVertLayout.CreateInputElements(),
      DepthStencilState = DepthStencilStates.Default,
      EffectBytecode = _imShader.Effect.Bytecode,
      RootSignature = _imShader.RootSignature,
      Output = new RenderOutputDescription(Stride.Graphics.PixelFormat.R8G8B8A8_UNorm)
    };

    var pipelineState = PipelineState.New(_graphicsDevice, ref pipeline);
    _imPipeline = pipelineState;

    var is32bits = false;

    var indexBuffer = Stride.Graphics.Buffer.Index.New(_graphicsDevice, initialIndexBufferSize * sizeof(ushort), GraphicsResourceUsage.Dynamic);
    _indexBufferBinding = new IndexBufferBinding(indexBuffer, is32bits, 0);

    var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(_graphicsDevice, initialVertexBufferSize * _imVertLayout.CalculateSize(), GraphicsResourceUsage.Dynamic);
    _vertexBufferBinding = new VertexBufferBinding(vertexBuffer, _imVertLayout, 0);
  }

  private unsafe void CreateFontTexture()
  {
    _io.Fonts.Clear();
    var text = _io.Fonts.AddFontDefault();
    text.Scale = Scale;
    var pixelData = default(byte*);
    var width = 0;
    var height = 0;
    var bytesPerPixel = 0;

    _io.Fonts.GetTexDataAsRGBA32(&pixelData, &width, &height, &bytesPerPixel);

    var newFontTexture = Texture.New2D(_graphicsDevice, width, height, Stride.Graphics.PixelFormat.R8G8B8A8_UNorm);

    // TODO: use the Span<T> overload instead
    newFontTexture.SetData(_commandList, new DataPointer(pixelData, (width * height) * bytesPerPixel));

    _fontTexture = newFontTexture;
  }

  public override void Update(GameTime gameTime)
  {
    var deltaTime = (float)gameTime.Elapsed.TotalSeconds;

    if (_isFirstFrame)
    {
      _isFirstFrame = false;
      deltaTime = 1 / 60f;
    }

    var surfaceSize = Game.Window.ClientBounds;
    _io.DisplaySize = new System.Numerics.Vector2(surfaceSize.Width, surfaceSize.Height);
    _io.DeltaTime = deltaTime;

    if (_input.HasMouse == false || _input.IsMousePositionLocked == false)
    {
      var mousePos = _input.AbsoluteMousePosition;
      _io.MousePos = new(mousePos.X, mousePos.Y);

      if (_io.WantTextInput)
      {
        _input.TextInput.EnabledTextInput();
      }
      else
      {
        _input.TextInput.DisableTextInput();
      }

      foreach (var ev in _input.Events)
      {
        switch (ev)
        {
          case Stride.Input.TextInputEvent tev:
            if (tev.Text.Equals("\t", StringComparison.Ordinal))
              continue;
            break;
          case KeyEvent kev:
            if (_keys.TryGetValue(kev.Key, out var imGuiKey))
              _io.AddKeyEvent(imGuiKey, _input.IsKeyDown(kev.Key));
            break;
          case Stride.Input.MouseWheelEvent mev:
            _io.MouseWheel += mev.WheelDelta;
            break;
        }
      }

      var mouseDown = _io.MouseDown;
      mouseDown[0] = _input.IsMouseButtonDown(MouseButton.Left);
      mouseDown[1] = _input.IsMouseButtonDown(MouseButton.Right);
      mouseDown[2] = _input.IsMouseButtonDown(MouseButton.Left);

      _io.KeyAlt = _input.IsKeyDown(Keys.LeftAlt) || _input.IsKeyDown(Keys.RightAlt);
      _io.KeyShift = _input.IsKeyDown(Keys.LeftShift) || _input.IsKeyDown(Keys.RightShift);
      _io.KeyCtrl = _input.IsKeyDown(Keys.LeftCtrl) || _input.IsKeyDown(Keys.RightCtrl);
      _io.KeySuper = _input.IsKeyDown(Keys.LeftWin) || _input.IsKeyDown(Keys.RightWin);
    }

    Hexa.NET.ImGui.ImGui.NewFrame();
  }

  public override void EndDraw()
  {
    Hexa.NET.ImGui.ImGui.Render();
    RenderDrawLists(Hexa.NET.ImGui.ImGui.GetDrawData());
    UserInterfaceExtension.ClearTextures();
  }

  private void CheckBuffers(ImDrawDataPtr drawData)
  {
    var totalVBOSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
    if (totalVBOSize > _vertexBufferBinding.Buffer.SizeInBytes)
    {
      var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(_graphicsDevice, (int)(totalVBOSize * 1.5f));
      _vertexBufferBinding = new VertexBufferBinding(vertexBuffer, _imVertLayout, 0);
    }

    var totalIBOSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
    if (totalIBOSize > _vertexBufferBinding.Buffer.SizeInBytes)
    {
      var is32bits = false;
      var indexBuffer = Stride.Graphics.Buffer.Index.New(_graphicsDevice, (int)(totalVBOSize * 1.5f));
      _indexBufferBinding = new IndexBufferBinding(indexBuffer, is32bits, 0);
    }
  }

  private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
  {
    var vtxOffsetBytes = 0;
    var idxOffsetBytes = 0;

    for (var i = 0; i < drawData.CmdListsCount; i++)
    {
      var currentCommandList = drawData.CmdLists[i];
      _vertexBufferBinding.Buffer.SetData(
        _commandList,
        new DataPointer(
          currentCommandList.VtxBuffer.Data,
          currentCommandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()
        ),
        vtxOffsetBytes
      );

      _indexBufferBinding.Buffer.SetData(
        _commandList,
        new DataPointer(
          currentCommandList.IdxBuffer.Data,
          currentCommandList.IdxBuffer.Size * sizeof(ushort)
        ),
        idxOffsetBytes
      );

      vtxOffsetBytes += currentCommandList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
      idxOffsetBytes += currentCommandList.IdxBuffer.Size * sizeof(ushort);
    }
  }

  void RenderDrawLists(ImDrawDataPtr drawData)
  {
    var surfaceSize = Game.Window.ClientBounds;
    var projMatrix = Matrix.OrthoRH(surfaceSize.Width, -surfaceSize.Height, -1, 1);

    CheckBuffers(drawData);
    UpdateBuffers(drawData);

    var is32bits = false;
    _commandList.SetPipelineState(_imPipeline);
    _commandList.SetVertexBuffer(0, _vertexBufferBinding.Buffer, 0, Unsafe.SizeOf<ImDrawVert>());
    _commandList.SetIndexBuffer(_indexBufferBinding.Buffer, 0, is32bits);
    _imShader.Parameters.Set(ImGuiShaderKeys.tex, _fontTexture);

    var vtxOffset = 0;
    var idxOffset = 0;

    for (var i = 0; i < drawData.CmdListsCount; i++)
    {
      var currentCommandList = drawData.CmdLists[i];

      for (var j = 0; j < currentCommandList.CmdBuffer.Size; j++)
      {
        var currentCommand = currentCommandList.CmdBuffer[j];
        if (currentCommand.TextureId != IntPtr.Zero)
        {
          if (UserInterfaceExtension.TryGetTexture(currentCommand.TextureId.Handle, out var texture))
          {
            _imShader.Parameters.Set(ImGuiShaderKeys.tex, texture);
          }
        }
        else
        {
          _imShader.Parameters.Set(ImGuiShaderKeys.tex, _fontTexture);
        }

        _commandList.SetScissorRectangle(
          new Rectangle(
            (int)currentCommand.ClipRect.X,
            (int)currentCommand.ClipRect.Y,
            (int)(currentCommand.ClipRect.Z - currentCommand.ClipRect.X),
            (int)(currentCommand.ClipRect.W - currentCommand.ClipRect.Y)
          )
        );

        _imShader.Parameters.Set(ImGuiShaderKeys.proj, ref projMatrix);
        _imShader.Apply(_graphicsContext);

        _commandList.DrawIndexed((int)currentCommand.ElemCount, idxOffset, vtxOffset);
        idxOffset += (int)currentCommand.ElemCount;
      }
      vtxOffset = currentCommandList.VtxBuffer.Size;
    }
  }
}
