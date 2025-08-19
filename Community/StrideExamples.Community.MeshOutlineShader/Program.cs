
// See https://aka.ms/new-console-template for more information
using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;
using StrideExamples.Community.MeshOutlineShader;

Console.WriteLine("Hello, World!");

using var game = new Game();

game.Run(start: Start);

void Start(Scene rootScene)
{
  game.SetupBase3DScene();
  game.AddSkybox();
  game.AddRootRenderFeature(
    new MeshOutlineRenderFeature
    {
      RenderGroupMask = RenderGroupMask.Group5,
      ScaleAdjust = 0.03f
    }
  );

  CreateOutlinePrimitive(rootScene, PrimitiveModelType.Sphere, Color.Cyan    , new Vector3( 2, .5f, -2));
  CreateOutlinePrimitive(rootScene, PrimitiveModelType.Capsule, Color.Yellow , new Vector3(-1, .5f, -2));
  CreateOutlinePrimitive(rootScene, PrimitiveModelType.Sphere, Color.Red     , new Vector3(-1, .5f, 4));
  CreateOutlinePrimitive(rootScene, PrimitiveModelType.Sphere, Color.Green   , new Vector3( 2, .5f, 1));
  CreateOutlinePrimitive(rootScene, PrimitiveModelType.Sphere, Color.Blue    , new Vector3( 1, .5f, 1));
}     

void CreateOutlinePrimitive(Scene rootScene, PrimitiveModelType modelType, Color4 color, Vector3 position)
{
  var entity = game.Create3DPrimitive(modelType, options: new()
  {
    RenderGroup = RenderGroup.Group5,
  }
  );

  entity.Transform.Position = position;
  entity.Add(
    new MeshOutlineComponent
    {
      Enabled = true,
      Color = color,
      Intensity = 100f
    }
  );

  entity.Scene = rootScene;
}