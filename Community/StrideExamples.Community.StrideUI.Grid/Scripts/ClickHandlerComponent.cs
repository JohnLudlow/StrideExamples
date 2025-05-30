using Stride.CommunityToolkit.Bepu;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Rendering;
using StrideExamples.StrideUI.Grid.Managers;
using System.Diagnostics;

namespace StrideExamples.StrideUI.Grid.Scripts;

public class ClickHandlerComponent : AsyncScript
{
    private const string _hitEntityName = "Cube";
    private readonly Vector3 _defaultCubePosition = new (0, 8, 0);
    private readonly Random _random = new();

    private GameManager? _gameManager;
    private CameraComponent? _cameraComponent;
    private Material? _material;

    public override async Task Execute()
    {
        _cameraComponent = Entity.Scene.Entities.FirstOrDefault(x => x.Get<CameraComponent>() != null)?.Get<CameraComponent>();
        _gameManager = Game.Services.GetService<GameManager>();
        
        Debug.Assert(_cameraComponent is not null);
        Debug.Assert(_gameManager is not null);

        _material = Game.CreateMaterial(Color.Yellow, 0, .1f);

        await _gameManager.LoadClickDataAsync();
        await LoadCubeDataAsync();

        while(Game.IsRunning)
        {
            if (!Input.HasMouse)
            {
                await Script.NextFrame();
                continue;
            }

            if (_gameManager.ReloadCubes)
            {
                await LoadCubeDataAsync();
            }

            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                ProcessRaycast(MouseButton.Left);
            }
            else if (Input.IsMouseButtonPressed(MouseButton.Right))
            {
                ProcessRaycast(MouseButton.Right);
            }

            await Script.NextFrame();
        }
    }

    private async Task LoadCubeDataAsync()
    {
        _gameManager!.ReloadCubes = false;
        foreach (var cube in GetCubeEntities())
        {
            cube.Remove();
        }

        var loadedCubes = await _gameManager.LoadCubeDataAsync();

        if (loadedCubes.Count == 0)
        {
            CreateCube();
        }
        else
        {
            foreach (var cube in loadedCubes)
            {
                CreateCube(cube);
            }
        }
    }

    private void ProcessRaycast(MouseButton mouseButton)
    {
        var hitSucceeded = _cameraComponent!.RaycastMouse(this, 1f, out var hitInfo);
        if (hitSucceeded && hitInfo.Collidable.Entity.Name == _hitEntityName)
        {
            if (mouseButton == MouseButton.Left)
            {
                AddNewEntity(hitInfo.Collidable.Entity);
            }
            else if (mouseButton == MouseButton.Right)
            {
                RemoveEntity(hitInfo.Collidable.Entity);
            }

            _gameManager?.HandleClick(mouseButton, GetCubeEntities().ConvertAll(s => s.Transform.Position));
        }
    }


    private List<Entity> GetCubeEntities() 
        => Entity.Scene.Entities
        .Where(e => e.Name == _hitEntityName && e.Get<CubeVanisher>() is null)
        .ToList();

    private void AddNewEntity(Entity entity)
    {
        ChangeColor(entity);
        Console.WriteLine("Adding entity");
        CreateCube(new Vector3(_random.Next(-4, 4), 8, _random.Next(-4, 4)));
    }

    private static void RemoveEntity(Entity entity)
    {
        Console.WriteLine("Removing entity");
        entity.Add(new CubeVanisher());
    }

    private void ChangeColor(Entity entity)
    {
        var model = entity.GetComponent<ModelComponent>();

        if (model is null) return;

        if (model.Materials.Count > 0)
        {
            model.Model.Materials[0] = _material;
        }
        else
        {
            model.Model.Materials.Add(_material);
        }
    }

    private void CreateCube(Vector3? position = null)
    {
        var entity = Game.Create3DPrimitive(PrimitiveModelType.Cube, new () { EntityName = _hitEntityName });
        entity.Transform.Position = position ?? _defaultCubePosition;
        entity.Add(new CubeGrower());
        entity.Scene = Entity.Scene;
    }
}