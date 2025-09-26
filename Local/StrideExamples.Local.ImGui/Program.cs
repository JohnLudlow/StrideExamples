// See https://aka.ms/new-console-template for more information
using Stride.Engine;
using Stride.CommunityToolkit.Engine;
using Stride.Games;
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Skyboxes;
using StrideExamples.Local.Common;
using Stride.Core.Mathematics;

Console.WriteLine("Hello, World!");

var game = new Game();

game.Run(start: Start, update: Update);

void Start(Scene rootScene)
{
  game.SetupBase3DScene();
  game.Add3DCamera();
  game.AddSkybox();
  game.AddAllDirectionLighting();

  var greenCube = game.CreateCube(rootScene, "GreenCube", new(-5, 1, 0), Color.Green);
  var blueCube = game.CreateCube(rootScene, "BlueCube", new(5, 1, 0), Color.Blue);
}

void Update(Scene rootScene, GameTime gameTime)
{
  // Your code here
}