// See https://aka.ms/new-console-template for more information
using Stride.CommunityToolkit.Engine;
using Stride.Engine;
using Stride.Local.BlockGridGenerator;

Console.WriteLine("Hello, World!");

var game = new Game();

var blockGrid = new BlockGrid(game);

game.Run(start: blockGrid.Start, update: blockGrid.Update);