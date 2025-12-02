using Myra.Graphics2D.UI;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;

namespace StrideExamples.Local.OcclusionTest.UI;

public class SceneEditorView : Panel
{
  private readonly Window _sceneHierarchyWindow;
  private readonly Window _propertiesWindow;
  private readonly VerticalStackPanel _sceneTreePanel;
  private readonly VerticalStackPanel _propertiesPanel;
  private readonly Dictionary<Button, Entity> _buttonToEntity = new();
  private readonly Dictionary<Entity, EntityTreeNode> _entityToNode = new();
  
  private Scene? _scene;
  private Entity? _selectedEntity;
  private Game? _game;

  public bool IsInitialized { get; private set; }

  public Entity? SelectedEntity
  {
    get => _selectedEntity;
    set
    {
      if (_selectedEntity != value)
      {
        _selectedEntity = value;
        RefreshProperties();
        UpdateSelectionHighlight();
      }
    }
  }

  private class EntityTreeNode
  {
    public Entity Entity { get; set; } = null!;
    public Button Button { get; set; } = null!;
    public Button ExpandButton { get; set; } = null!;
    public VerticalStackPanel ChildrenPanel { get; set; } = null!;
    public bool IsExpanded { get; set; } = true;
  }

  public SceneEditorView()
  {
    Console.WriteLine("SceneEditorView: Constructor started");

    // Add a simple test label to verify Myra is working
    var testLabel = new Label
    {
      Text = "Scene Editor UI - Collapsible Tree",
      Left = 10,
      Top = 520
    };
    Widgets.Add(testLabel);

    // Create scene hierarchy tree panel
    _sceneTreePanel = new VerticalStackPanel
    {
      Spacing = 0,
      HorizontalAlignment = HorizontalAlignment.Stretch
    };

    var hierarchyScroll = new ScrollViewer
    {
      Content = _sceneTreePanel,
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch
    };

    _sceneHierarchyWindow = new Window
    {
      Title = "Scene Hierarchy",
      Left = 10,
      Top = 10,
      Width = 300,
      Height = 500,
      Content = hierarchyScroll
    };

    // Create properties window
    _propertiesPanel = new VerticalStackPanel
    {
      Spacing = 8,
      Padding = new Myra.Graphics2D.Thickness(5)
    };

    var propertiesScroll = new ScrollViewer
    {
      ShowVerticalScrollBar = true,
      Content = _propertiesPanel,
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch
    };

    _propertiesWindow = new Window
    {
      Title = "Properties",
      Left = 320,
      Top = 10,
      Width = 450,
      Height = 500,
      Content = propertiesScroll
    };

    // Add windows to panel
    Widgets.Add(_sceneHierarchyWindow);
    Widgets.Add(_propertiesWindow);

    // Add initial placeholder content
    _propertiesPanel.Widgets.Add(new Label { Text = "Initializing..." });

    Console.WriteLine("SceneEditorView: Constructor completed. Windows added to panel.");
    Console.WriteLine($"SceneEditorView: Widgets count = {Widgets.Count}");
  }

  public void Initialize(Scene scene, Game game)
  {
    if (IsInitialized) return;
    
    Console.WriteLine("SceneEditorView: Initialize called");
    _scene = scene;
    _game = game;
    RefreshSceneTree();
    
    Console.WriteLine($"SceneEditorView: Scene has {_scene.Entities.Count} entities");

    // Select the first root entity by default
    if (_entityToNode.Count > 0)
    {
      var firstEntity = _entityToNode.Values.First().Entity;
      SelectedEntity = firstEntity;
      Console.WriteLine($"SceneEditorView: Selected entity = {firstEntity.Name}");
    }
    
    IsInitialized = true;
  }

  public void RefreshSceneTree()
  {
    if (_scene == null) return;

    _sceneTreePanel.Widgets.Clear();
    _buttonToEntity.Clear();
    _entityToNode.Clear();

    // Add only root entities (those without parents)
    foreach (var entity in _scene.Entities)
    {
      if (entity.Transform.Parent == null)
      {
        AddEntityToTree(entity, _sceneTreePanel, 0);
      }
    }
  }

  private void AddEntityToTree(Entity entity, VerticalStackPanel parentPanel, int depth)
  {
    var nodePanel = new HorizontalStackPanel
    {
      Spacing = 2,
      HorizontalAlignment = HorizontalAlignment.Stretch
    };

    // Add indentation
    if (depth > 0)
    {
      nodePanel.Widgets.Add(new Label { Width = depth * 20 });
    }

    // Create expand/collapse button
    var hasChildren = entity.Transform.Children.Count > 0;
    var expandButton = new Button
    {
      Content = new Label { Text = hasChildren ? "▼" : "  " },
      Width = 20,
      Height = 20,
      Padding = new Myra.Graphics2D.Thickness(0)
    };

    // Create entity name button
    var nameButton = new Button
    {
      Content = new Label { Text = entity.Name ?? "Unnamed Entity" },
      HorizontalAlignment = HorizontalAlignment.Stretch,
      Padding = new Myra.Graphics2D.Thickness(4, 2, 4, 2)
    };

    var childrenPanel = new VerticalStackPanel
    {
      Spacing = 0,
      HorizontalAlignment = HorizontalAlignment.Stretch
    };

    var node = new EntityTreeNode
    {
      Entity = entity,
      Button = nameButton,
      ExpandButton = expandButton,
      ChildrenPanel = childrenPanel,
      IsExpanded = true
    };

    _entityToNode[entity] = node;
    _buttonToEntity[nameButton] = entity;

    // Wire up click events
    nameButton.Click += (s, e) => SelectedEntity = entity;
    
    if (hasChildren)
    {
      expandButton.Click += (s, e) =>
      {
        node.IsExpanded = !node.IsExpanded;
        childrenPanel.Visible = node.IsExpanded;
        ((Label)expandButton.Content).Text = node.IsExpanded ? "▼" : "▶";
      };
    }

    nodePanel.Widgets.Add(expandButton);
    nodePanel.Widgets.Add(nameButton);

    parentPanel.Widgets.Add(nodePanel);

    // Add children
    foreach (var child in entity.Transform.Children)
    {
      AddEntityToTree(child.Entity, childrenPanel, depth + 1);
    }

    if (hasChildren)
    {
      parentPanel.Widgets.Add(childrenPanel);
    }
  }

  private void UpdateSelectionHighlight()
  {
    // Reset all button backgrounds
    foreach (var kvp in _buttonToEntity)
    {
      var button = kvp.Key;
      button.Background = null;
    }

    // Highlight selected entity with background
    if (_selectedEntity != null && _entityToNode.TryGetValue(_selectedEntity, out var node))
    {
      node.Button.Background = new Myra.Graphics2D.Brushes.SolidBrush("#4080FF80");
    }
  }

  private void RefreshProperties()
  {
    _propertiesPanel.Widgets.Clear();

    if (_selectedEntity == null)
    {
      _propertiesPanel.Widgets.Add(new Label { Text = "No entity selected" });
      return;
    }

    _propertiesPanel.Widgets.Add(new Label 
    { 
      Text = $"Entity: {_selectedEntity.Name ?? "Unnamed"}"
    });

    AddTransformProperties(_selectedEntity.Transform);
    AddComponentsSection(_selectedEntity);
  }

  private void AddTransformProperties(TransformComponent transform)
  {
    _propertiesPanel.Widgets.Add(new Label());
    _propertiesPanel.Widgets.Add(new Label 
    { 
      Text = "=== Transform ==="
    });

    AddVector3Property("Position", transform.Position, value => transform.Position = value);
    AddVector3Property("Rotation (Deg)", GetRotationDegrees(transform.Rotation), value => SetRotationDegrees(transform, value));
    AddVector3Property("Scale", transform.Scale, value => transform.Scale = value);
  }

  private Vector3 GetRotationDegrees(Quaternion rotation)
  {
    var euler = QuaternionToEulerAngles(rotation);
    return new Vector3(
      MathUtil.RadiansToDegrees(euler.X),
      MathUtil.RadiansToDegrees(euler.Y),
      MathUtil.RadiansToDegrees(euler.Z)
    );
  }

  private void SetRotationDegrees(TransformComponent transform, Vector3 degrees)
  {
    var radians = new Vector3(
      MathUtil.DegreesToRadians(degrees.X),
      MathUtil.DegreesToRadians(degrees.Y),
      MathUtil.DegreesToRadians(degrees.Z)
    );
    transform.Rotation = EulerAnglesToQuaternion(radians);
  }

  private static Vector3 QuaternionToEulerAngles(Quaternion q)
  {
    var eulerAngles = new Vector3();

    var sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
    var cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
    eulerAngles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

    var sinp = 2 * (q.W * q.Y - q.Z * q.X);
    if (Math.Abs(sinp) >= 1)
      eulerAngles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
    else
      eulerAngles.Y = (float)Math.Asin(sinp);

    var siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
    var cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
    eulerAngles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

    return eulerAngles;
  }

  private static Quaternion EulerAnglesToQuaternion(Vector3 euler)
  {
    var cy = (float)Math.Cos(euler.Z * 0.5);
    var sy = (float)Math.Sin(euler.Z * 0.5);
    var cp = (float)Math.Cos(euler.Y * 0.5);
    var sp = (float)Math.Sin(euler.Y * 0.5);
    var cr = (float)Math.Cos(euler.X * 0.5);
    var sr = (float)Math.Sin(euler.X * 0.5);

    return new Quaternion
    {
      W = cr * cp * cy + sr * sp * sy,
      X = sr * cp * cy - cr * sp * sy,
      Y = cr * sp * cy + sr * cp * sy,
      Z = cr * cp * sy - sr * sp * cy
    };
  }

  private void AddVector3Property(string name, Vector3 value, Action<Vector3> onChanged)
  {
    var panel = new VerticalStackPanel { Spacing = 2 };
    panel.Widgets.Add(new Label { Text = $"{name}:" });

    var hPanel = new HorizontalStackPanel { Spacing = 4 };
    
    var xText = CreateFloatTextBox(value.X, v => 
    {
      value.X = v;
      onChanged(value);
    });
    var yText = CreateFloatTextBox(value.Y, v => 
    {
      value.Y = v;
      onChanged(value);
    });
    var zText = CreateFloatTextBox(value.Z, v => 
    {
      value.Z = v;
      onChanged(value);
    });

    hPanel.Widgets.Add(new Label { Text = "X:", Width = 20 });
    hPanel.Widgets.Add(xText);
    hPanel.Widgets.Add(new Label { Text = "Y:", Width = 20 });
    hPanel.Widgets.Add(yText);
    hPanel.Widgets.Add(new Label { Text = "Z:", Width = 20 });
    hPanel.Widgets.Add(zText);

    panel.Widgets.Add(hPanel);
    _propertiesPanel.Widgets.Add(panel);
  }

  private TextBox CreateFloatTextBox(float initialValue, Action<float> onChanged)
  {
    var textBox = new TextBox
    {
      Text = initialValue.ToString("F2"),
      Width = 70
    };

    textBox.TextChanged += (sender, args) =>
    {
      if (float.TryParse(textBox.Text, out var value))
      {
        onChanged(value);
      }
    };

    return textBox;
  }

  private void AddComponentsSection(Entity entity)
  {
    _propertiesPanel.Widgets.Add(new Label());
    _propertiesPanel.Widgets.Add(new Label 
    { 
      Text = "=== Components ==="
    });

    foreach (var component in entity.Components)
    {
      var componentType = component.GetType();
      _propertiesPanel.Widgets.Add(new Label 
      { 
        Text = $"  • {componentType.Name}"
      });
    }
  }

  public void Update(Game game)
  {
    if (_game == null || _scene == null) return;

    var input = game.Input;
    
    if (input.IsMouseButtonPressed(MouseButton.Left))
    {
      var mousePosition = input.MousePosition;
      var cameraEntity = _scene.Entities.FirstOrDefault(e => e.Get<CameraComponent>() != null);
      
      if (cameraEntity != null)
      {
        var camera = cameraEntity.Get<CameraComponent>();
        
        var simulation = game.Services.GetService<Stride.BepuPhysics.BepuSimulation>();
        if (simulation != null && camera != null)
        {
          var viewMatrix = camera.ViewMatrix;
          var projectionMatrix = camera.ProjectionMatrix;
          
          var viewport = new Stride.Graphics.Viewport(0, 0, game.GraphicsDevice.Presenter.BackBuffer.Width, game.GraphicsDevice.Presenter.BackBuffer.Height);
          
          var nearPosition = viewport.Unproject(
            new Vector3(mousePosition.X, mousePosition.Y, 0f),
            projectionMatrix,
            viewMatrix,
            Matrix.Identity);
          
          var farPosition = viewport.Unproject(
            new Vector3(mousePosition.X, mousePosition.Y, 1f),
            projectionMatrix,
            viewMatrix,
            Matrix.Identity);
          
          var direction = Vector3.Normalize(farPosition - nearPosition);
          
          var hit = simulation.RayCast(nearPosition, direction, float.MaxValue, out var hitInfo);
          if (hit && hitInfo.Collidable?.Entity is Entity hitEntity)
          {
            SelectedEntity = hitEntity;
          }
        }
      }
    }
  }
}
