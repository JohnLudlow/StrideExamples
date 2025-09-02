using Stride.CommunityToolkit.Engine;
using Stride.Engine;
using StrideExamples.Community.CubeClicker;

using var game = new Game();

var cubeStacker = new CubeStacker(game);
game.Run(start: cubeStacker.Start, update: cubeStacker.Update);