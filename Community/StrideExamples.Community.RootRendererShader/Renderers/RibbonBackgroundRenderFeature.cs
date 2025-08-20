using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Streaming;

namespace StrideExamples.Community.RootRendererShader.Renderers;

public class RibbonBackgroundRenderFeature : RootRenderFeature
{
  private SpriteBatch? _spriteBatch;
  private DynamicEffectInstance? _background2DEffect;
  private SamplerState? _samplerState;
  private Texture? _texture;

  public override Type SupportedRenderObjectType { get; } = typeof(RibbonRenderBackground);

  public RibbonBackgroundRenderFeature()
  {
    // Background should render after most objects (to take advantage of early z depth test)
    SortKey = 192;
  }

  public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
  {
    for (var index = startIndex; index < endIndex; index++)
    {
      var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
      var renderNode = GetRenderNode(renderNodeReference);
      var renderBackground = (RibbonRenderBackground)renderNode.RenderObject;

      if (_texture == null)
        continue;

      Draw2D(context, renderBackground);
    }
  }

    protected override void InitializeCore()
    {
        // Prepare texture information
        var width = 1920;// Context.GraphicsDevice.Presenter.BackBuffer.Width;
        var height = 1080;// Context.GraphicsDevice.Presenter.BackBuffer.Height;
        var maxMipMapCount = 2;
        var mipMapCount = Math.Min(Texture.CountMips(Math.Max(width, height)), maxMipMapCount + 1);
        var textureDescription = TextureDescription.New2D(width, height, mipMapCount, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Dynamic);
        _texture = Texture.New(Context.GraphicsDevice, textureDescription);
        Context.StreamingManager?.StreamResources(_texture, StreamingOptions.LoadAtOnce);

        // load shader
        _background2DEffect = new DynamicEffectInstance("RibbonBackgroundShader");
        _background2DEffect.Initialize(Context.Services);

        _spriteBatch = new SpriteBatch(RenderSystem.GraphicsDevice) { VirtualResolution = new Vector3(1) };

        // set fixed parameters once
        _background2DEffect.Parameters.Set(TexturingKeys.Sampler, _samplerState);

        // NOTE: Linear-Wrap sampling is not available for non-square non-power-of-two textures on opengl es 2.0
        _samplerState = SamplerState.New(Context.GraphicsDevice, new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Clamp));
    }

  private void Draw2D([NotNull] RenderDrawContext context, [NotNull] RibbonRenderBackground renderBackground)
  {
    if (_texture is null) throw new InvalidOperationException($"{nameof(Draw2D)} cannot be called when {_texture} is null");
    if (_background2DEffect is null) throw new InvalidOperationException($"{nameof(Draw2D)} cannot be called when {_background2DEffect} is null");
    if (_spriteBatch is null) throw new InvalidOperationException($"{nameof(Draw2D)} cannot be called when {_spriteBatch} is null");

    var target = context.CommandList.RenderTarget;
    var destination = new RectangleF(0, 0, 1, 1);

    var textureIsLoading = _texture.ViewType == ViewType.Full && _texture.FullQualitySize.Width != _texture.Width;
    var textureSize = textureIsLoading ? _texture.FullQualitySize : new Size3(_texture.ViewWidth, _texture.ViewHeight, _texture.ViewDepth);
    var imageBufferMinRatio = Math.Min(textureSize.Width / (float)target.ViewWidth, textureSize.Height / (float)target.ViewHeight);
    var sourceSize = new Vector2(target.ViewWidth * imageBufferMinRatio, target.ViewHeight * imageBufferMinRatio);
    var source = new RectangleF((textureSize.Width - sourceSize.X) / 2, (textureSize.Height - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

    if (textureIsLoading)
    {
      var horizontalRatio = _texture.ViewWidth / (float)_texture.Width;
      var verticalRatio = _texture.ViewHeight / (float)_texture.Height;

      source.X *= horizontalRatio;
      source.Width *= horizontalRatio;

      source.Y *= verticalRatio;
      source.Width *= verticalRatio;
    }

    if (_texture.ViewDimension == TextureDimension.Texture2D)
    {
      _background2DEffect.UpdateEffect(context.GraphicsDevice);
      _spriteBatch.Begin(
        graphicsContext: context.GraphicsContext,
        sortMode: SpriteSortMode.FrontToBack,
        blendState: BlendStates.Opaque,
        samplerState: context.GraphicsDevice.SamplerStates.LinearClamp,
        depthStencilState: DepthStencilStates.DepthRead,
        rasterizerState: null,
        effect: _background2DEffect
      );

      try
      {
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.DeltaTime, (float)context.RenderContext.Time.Total.TotalSeconds);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.Intensity, renderBackground.Intensity);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.Frequency, renderBackground.Frequency);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.Amplitude, renderBackground.Amplitude);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.Speed, renderBackground.Speed);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.Top, renderBackground.Top);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.Bottom, renderBackground.Bottom);
        _spriteBatch.Parameters.Set(RibbonBackgroundShaderKeys.WidthFactor, renderBackground.WidthFactor);

        _spriteBatch.Draw(_texture, destination, source, Color.White, 0, Vector2.Zero, layerDepth: -.5f);
      }
      finally
      {
        _spriteBatch.End();
      }
    }
    else
    {
      return;
    }

  }
}
