# Mothropolis Development Guidelines & Workflow

## 1. Project Context & Architecture
- **Game**: *Mothropolis* — 2D Evolutionary Platformer/Ecosystem Simulator built with Unity 6 (URP 2D).
- **Core Loop**:
  - **Day Phase**: Shop / Upgrades, UI, Evolution Report review.
  - **Night Phase**: Single-screen contained arena, Moth hunting via tongue / grapple mechanics, Light attraction/repulsion, Owl predator stealth/avoidance.
  - **Evolution Step**: Off-screen genetic reproduction (`MothGenome`, `ReproductionEngine`) shifting population traits (color, speed, light preference) across generations.

## 2. Process & Verification Workflow
- **Granular Sub-Phases**: Always break down multi-step tasks into clear, isolated sub-phases. Provide a concrete verification step after each one.
- **Source of Truth**: The Implementation Plan (`implementation_plan.md`) is the single source of truth for architectural decisions.
- **No Uncheckpointed Bulk Dumps**: Never modify dozens of interconnected systems in a single blind pass without intermediate checkpoints.

## 3. Unity & C# Coding Standards
- **Input System**: Use Unity's new Input System (`UnityEngine.InputSystem`).
- **Decoupled Events**: Use `Mothropolis.Core.GameEvents` static action hooks for cross-system communication (e.g., `OnMothCaught`, `OnDawnReached`, `OnExposureChanged`, `OnTongueAttack`).
- **Animation Ownership**: Keep playback timing inside the Animator Controller (`Animator`) with parameters/triggers rather than hardcoding animation timers or frame arrays in MonoBehaviour scripts.
- **Physics 2D**:
  - Use non-allocating physics queries with `ContactFilter2D` (e.g., `Physics2D.OverlapCircle(..., ContactFilter2D, Collider2D[])`).
  - Keep Moth interactions isolated to the `Moth` LayerMask.
  - Respect level bounds and single-screen camera frustum constraints (steer-away / bounce, no toroidal screen-wrapping).

## 4. AI Game Developer (MCP) Tool Usage
- Use the connected `ai-game-developer` MCP tools to proactively inspect open scenes (`scene-get-data`, `gameobject-find`), verify GameObject hierarchies and components (`gameobject-component-get`), check console errors (`console-get-logs`), and validate scene integrity after code changes.
