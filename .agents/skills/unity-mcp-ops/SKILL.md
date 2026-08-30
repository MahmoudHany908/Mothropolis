---
name: unity-mcp-ops
description: >-
  Operations guide for using the AI Game Developer MCP tools to inspect, query,
  debug, and configure Unity scenes, game objects, components, and console logs live.
---

# Unity MCP Operations Skill

This skill documents how to interact with the live Unity Editor session using the `ai-game-developer` MCP server tools.

## Key Tool Capabilities

### 1. Scene & Hierarchy Inspection
- **`scene-list-opened`**: Discover currently opened scenes, root count, and dirty status.
- **`scene-get-data`**: Retrieve the full hierarchy of a scene with root GameObject IDs and transform details.
- **`gameobject-find`**: Search for specific GameObjects by name or tag in the active scene.

### 2. Component Inspection & Modification
- **`gameobject-component-list-all`**: Enumerate all components attached to a GameObject.
- **`gameobject-component-get`**: Inspect serialized fields, public properties, and references on any component.
- **`gameobject-component-modify`**: Update field values or object references on components directly in the Editor.
- **`gameobject-component-add` / `gameobject-component-destroy`**: Add or remove components dynamically.

### 3. Debugging & Error Checking
- **`console-get-logs`**: Fetch live console logs, warnings, and compilation errors from the Unity Editor without requiring manual copy-pasting.

### 4. Scene & Prefab Authoring
- **`gameobject-create`**: Create empty or primitive GameObjects in the scene.
- **`gameobject-set-parent`**: Reorganize hierarchy (e.g. parenting visuals or origins).
- **`assets-prefab-instantiate`**: Spawn prefabs into the scene at specified coordinates.
- **`scene-save`**: Save dirty scenes after applying hierarchy or component modifications.
- **`tests-run`**: Execute EditMode or PlayMode tests.

## Workflow Best Practices
1. **Always Check Logs on Startup / Reload**: When scripts change, call `console-get-logs` to proactively verify that no compilation or runtime errors occurred.
2. **Verify Inspector Bindings**: After adding new serialized fields (`Transform`, `Animator`, `SpriteRenderer`), use `gameobject-component-get` to verify if they are linked.
3. **Safe Modifications**: Check if the scene is dirty with `scene-list-opened` and save with `scene-save` when applying structural changes.
