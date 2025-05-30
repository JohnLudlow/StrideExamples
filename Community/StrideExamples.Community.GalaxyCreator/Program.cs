using Stride.CommunityToolkit.Bullet;
using Stride.CommunityToolkit.Engine;
using Stride.Engine;
using StrideExamples.GalaxyCreator;

var game = new Game();
game.Run();

void Start(Scene rootScene)
{
    game.AddDirectionalLight();
    game.Add3DGround();
    game.Add3DCamera().Add3DCameraController();

    var galaxy = new Entity{ new GalaxyCreator() };

    galaxy.Scene = rootScene;
}