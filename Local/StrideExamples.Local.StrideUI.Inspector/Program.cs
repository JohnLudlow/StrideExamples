using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

var game = new Game();
game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
    game.SetupBase3DScene();
    game.AddDirectionalLight();
    game.AddSkybox();
    var spriteFont = game.Content.Load<SpriteFont>("StrideDefaultFont");

    AddCapsule(rootScene, game);
    AddWindow(rootScene, spriteFont);
}

void Update(Scene rootScene, GameTime gameTime)
{

}

static void AddCapsule(Scene rootScene, Game game)
{
    var entity = game.Create3DPrimitive(PrimitiveModelType.Capsule);
    entity.Transform.Position = new(0, 8, 0);
    entity.Scene = rootScene;
}

Entity AddWindow(Scene scene, SpriteFont spriteFont)
{
    var entity = new Entity
    {
        Scene = scene
    };

    entity.Add(new UIComponent
    {
        
        Page = new UIPage
        {
            RootElement = CreateCanvas(spriteFont),
        }
    });

    return entity;
}

Canvas CreateCanvas(SpriteFont spriteFont)
{
    ArgumentNullException.ThrowIfNull(spriteFont);

    var canvas = new Canvas
    {
        Width = 300,
        Height = 100,
        BackgroundColor = Color.SlateBlue
    };
    canvas.Children.Add(CreateTextBlock(spriteFont));
    return canvas;
}

TextBlock CreateTextBlock(SpriteFont spriteFont)
{
    ArgumentNullException.ThrowIfNull(spriteFont);

    return new TextBlock
    {
        Text = "Hello world",
        TextColor = Color.LightGray,
        TextSize = 20,
        Margin = new Thickness(3, 3, 3, 0),
        Font = spriteFont
    };
}