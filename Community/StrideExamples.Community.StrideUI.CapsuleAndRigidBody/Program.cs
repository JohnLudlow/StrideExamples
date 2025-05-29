// See https://aka.ms/new-console-template for more information

using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

SpriteFont _spriteFont;

using var game = new Game();

game.Run(start: Start);

void Start(Scene scene)
{
    game.SetupBase3DScene();
    game.AddSkybox();

    AddCapsule(scene);

    LoadFont();

    AddWindow(scene);
}

void AddCapsule(Scene scene)
{
    var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);
    entity.Transform.Position = new (0, 8, 0);
    entity.Scene = scene;
}

void LoadFont()
{
    _spriteFont = game.Content.Load<SpriteFont>("StrideDefaultFont");
}

void AddWindow(Scene scene)
{
    var uiEntity = CreateUIEntity();
    uiEntity.Scene = scene;
}

Entity CreateUIEntity()
{
    return [
        new UIComponent
        {
            Page = new UIPage { RootElement = CreateCanvas() },
            RenderGroup = RenderGroup.Group31
        }
    ];
}

Canvas CreateCanvas()
{
    var canvas = new Canvas { Width = 300, Height = 100, BackgroundColor = new Color(248, 177, 149, 100) };
    canvas.Children.Add(CreateTextBlock(_spriteFont));
    return canvas;
}

TextBlock CreateTextBlock(SpriteFont spriteFont)
{
    ArgumentNullException.ThrowIfNull(spriteFont);

    return new TextBlock { 
        Text = "Hello world",
        TextColor = Color.White,
        TextSize = 20,
        Margin = new Thickness(3, 3, 3, 0),
        Font = spriteFont
    };
}