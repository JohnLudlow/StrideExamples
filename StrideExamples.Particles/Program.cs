﻿using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles;
using Stride.Particles.Components;
using Stride.Particles.Initializers;
using Stride.Particles.Materials;
using Stride.Particles.Modules;
using Stride.Particles.ShapeBuilders;
using Stride.Particles.Spawners;
using Stride.Rendering.Materials.ComputeColors;

using var game = new Game();
game.Run(start: (Scene rootScene) => {
    game.AddGraphicsCompositor();
    game.Add3DCamera().Add3DCameraController();
    game.AddDirectionalLight();
    game.AddSkybox();
    game.Add3DGround();
    game.AddParticleRenderer();

    game.AddProfiler();
    game.SetMaxFPS(60);

    CreateParticleEffect();
});

void CreateParticleEffect()
{
    var emitter = new ParticleEmitter()
    {
        ParticleLifetime = new Vector2(.5f, .5f),
        SimulationSpace = EmitterSimulationSpace.World,
        RandomSeedMethod = EmitterRandomSeedMethod.Time,
        ShapeBuilder = new ShapeBuilderBillboard(),
        Material = new ParticleMaterialComputeColor()
        {
            ComputeColor = new ComputeColor
            {
                Value = new Color4(0, 0, 1, 1)
            }
        },
    };

    emitter.Spawners.Add(
        new SpawnerPerSecond 
        { 
            LoopCondition = SpawnerLoopCondition.Looping,
            Delay = new Vector2(),
            Duration = new Vector2(1, 1),
            SpawnCount = 50
        }
    );

    var sizeInit = new InitialSizeSeed
    {
        ScaleUniform = .3f,
        RandomSize = new Vector2(.1f, .5f)
    };

    var positionInitializer = new InitialPositionSeed()
    {
        PositionMin = new Vector3(-0.03f, -0.03f, -0.03f),
        PositionMax = new Vector3(0.03f, 0.03f, 0.03f),
    };

    var velocityInitialzer = new InitialVelocitySeed()
    {
        VelocityMin = new Vector3(0, 3, 0),
        VelocityMax = new Vector3(3, 4, 3),
    };

    emitter.Initializers.Add(sizeInit);
    emitter.Initializers.Add(positionInitializer);
    emitter.Initializers.Add(velocityInitialzer);

    emitter.Updaters.Add(new UpdaterGravity() { GravitationalAcceleration = new Vector3(0, -9.8f, 0) });

    var particleSettings = new ParticleSystemSettings
    {
        WarmupTime = 0,
    };

    var particles = new ParticleSystemComponent()
    {
        Color = Color.White,
        RenderGroup = Stride.Rendering.RenderGroup.Group0,
        Speed = 1,
    };
    particles.ParticleSystem.Emitters.Add(emitter);
    particles.ParticleSystem.Settings = particleSettings;

    var entity = new Entity
    {
        particles
    };
    entity.Name = "Particles";
    entity.Scene = game.SceneSystem.SceneInstance.RootScene;
}