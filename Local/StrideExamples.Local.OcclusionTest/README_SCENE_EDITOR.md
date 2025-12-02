# Scene Editor UI for OcclusionTest

This example demonstrates a functional scene editor UI built with Myra for the Stride game engine.

## Features

### Scene Hierarchy
- **Location**: Left window panel titled "Scene Hierarchy"
- **Functionality**: 
  - Displays all entities in the scene in a hierarchical list
  - Shows parent-child relationships with indentation
  - Click on any entity name to select it

### Properties Inspector
- **Location**: Right window panel titled "Properties"
- **Functionality**:
  - Displays properties of the currently selected entity
  - Shows entity name
  - **Transform Properties** (editable):
    - Position (X, Y, Z)
    - Rotation in degrees (X, Y, Z)
    - Scale (X, Y, Z)
  - **Components List**: Shows all components attached to the entity

### Entity Selection
You can select entities in two ways:
1. **From Scene Hierarchy**: Click on an entity name in the left panel
2. **From 3D World**: Click on an object in the 3D viewport (uses physics raycasting)

### Property Editing
- Click on any X, Y, or Z value text box to edit it
- Type a new value and press Enter or click elsewhere
- Changes are applied in real-time to the 3D world
- Watch the object transform as you adjust values

## Usage Example

1. Run the application
2. You'll see two windows on the left side of the screen
3. The Scene Hierarchy shows all entities: GreenCube, BlueCube, RedCube, Camera, Ground, etc.
4. Click on "GreenCube" in the hierarchy
5. The Properties panel updates to show GreenCube's transform values
6. Edit the Position Y value to move the cube up or down
7. Try clicking on a cube in the 3D world to select it
8. Modify its rotation to see it rotate in real-time

## Technical Implementation

### Architecture
- **MyraSceneRenderer**: Custom scene renderer that integrates Myra UI into Stride's rendering pipeline
- **SceneEditorView**: Main UI panel containing the hierarchy and properties panels
- **Integration**: Uses Stride's graphics compositor with a clean UI stage

### Key Components
- ListBox for hierarchical entity display
- TextBox controls for editable float values
- Real-time property updates using delegates
- Physics-based 3D object picking with raycasting

## Code Structure

```
Local/StrideExamples.Local.OcclusionTest/
├── UI/
│   ├── MyraSceneRenderer.cs      # Myra integration with Stride
│   └── SceneEditorView.cs        # Main editor UI
├── Program.cs                     # Application entry point
└── Components/
    └── MultiRaycastVisibilityComponent.cs  # Visibility testing
```

## Customization

You can extend the scene editor by:
- Adding more property types (colors, materials, etc.)
- Implementing component-specific property editors
- Adding entity creation/deletion functionality
- Implementing drag-and-drop hierarchy rearrangement
- Adding undo/redo functionality

## Notes

- The editor windows are **draggable** - click and drag the title bar to reposition them
- Windows have fixed sizes in this version of Myra (300x500 for hierarchy, 450x500 for properties)
- Property changes are immediate and affect the live scene
- The selection synchronizes between the hierarchy list and 3D world clicks
- Mouse clicks in the 3D viewport are intercepted for entity selection
- Use the expand/collapse arrows (▼/▶) to manage the hierarchy tree
