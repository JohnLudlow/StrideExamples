using System.Reflection;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Extensions;
using Stride.CommunityToolkit.Rendering.Utilities;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

using var game = new Game();
game.Run(start: Start);

static void Start(Game game)
{
    game.Window.SetSize(new Int2(1000, 1000));
    game.SetupBase();

    var font = game.Content.Load<SpriteFont>("StrideDefaultFont");
    var directory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) ?? throw new InvalidOperationException("Unable to find assembly path");
    var filePath = Path.Combine(directory, "Assets/Images/input.png");

    using var input = File.Open(filePath, FileMode.Open);
    using var texture = Texture.Load(game.GraphicsDevice, input);

    var grid = new UniformGrid{
        Width = 1024,
        Height = 1024,

        Columns = 9,
        Rows = 9,

        Margin = new (2, 2, 2, 2)        
    };

    grid.Children.Add(CreateCard(texture, font));

    foreach (var anchor in Enum.GetValues<TextureCanvas.Anchor>())
    {
        foreach(var stretch in Enum.GetValues<TextureCanvas.Stretch>())
        {
            using (var canvas = game.CreateTextureCanvas(new (1024, 1024)))
            {
                canvas.DrawTexture(
                    sourceTexture   : texture,
                    sourceRect      : new (0, 128, 256, 256),
                    destinationRect : new (128, 256, 768, 512),
                    colorMultiplier : null,
                    stretch         : stretch,
                    anchor          : anchor,
                    samplingPattern : Stride.Rendering.Images.SamplingPattern.Expanded
                );

                var card = CreateCard(canvas.ToTexture(), font, anchor, stretch);
                card.SetGridColumn((int)anchor);
                card.SetGridRow((int)stretch * 2 + 1);
                grid.Children.Add(card);
            }

            using (var canvas = game.CreateTextureCanvas(new (1024, 1024)))
            {
                canvas.DrawTexture(
                    sourceTexture   : texture,
                    sourceRect      : new (0, 128, 256, 256),
                    destinationRect : new (128, 256, 512, 768),
                    colorMultiplier : null,
                    stretch         : stretch,
                    anchor          : anchor
                );

                var card = CreateCard(canvas.ToTexture(), font, anchor, stretch);
                card.SetGridColumn((int)anchor);
                card.SetGridRow((int)stretch * 2 + 2);
                grid.Children.Add(card);
            }            
        }
    }

    var entity = new Entity { Scene = game.SceneSystem.SceneInstance.RootScene };
    entity.Add(new UIComponent { Page = new UIPage { RootElement = grid} });
}

static Border CreateCard(Texture texture, SpriteFont spriteFont, TextureCanvas.Anchor? anchor = null, TextureCanvas.Stretch? stretch = null)
=> new() {
    BorderColor = new(25, 25, 25),
    BorderThickness = new (2, 2, 2, 2),

    BackgroundColor = new(120, 120, 120),

    Padding = new (2, 2, 2, 2),
    Margin = new (2, 2, 2, 2),

    Content = new StackPanel
    {
        Orientation = Orientation.Vertical,
        Height = 100,
        Children =
            {
                new TextBlock { 
                    TextAlignment       = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Top,
                    TextSize            = 12,
                    Font                = spriteFont,
                    TextColor           = Color.White,
                    Text                = $"{anchor?.ToString() ?? "DEFAULT"}"
                },

                new TextBlock { 
                    TextAlignment       = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Top,
                    TextSize            = 12,
                    Font                = spriteFont,
                    TextColor           = Color.White,
                    Text                = $"{stretch.ToString() ?? "DEFAULT"}"
                },

                new ImageElement
                {
                    Source = new SpriteFromTexture { Texture = texture }
                }
            }
    }
};