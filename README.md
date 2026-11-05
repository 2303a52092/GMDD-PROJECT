# Deep Forest – Zombie Apocalypse

Unity 6 endless-runner game project for Windows. The playable scene is generated at `Assets/Scenes/DeepForest.unity`; runtime systems provide lane movement, jump/slide/sprint, stamina, health, pickups, score, timer, increasing difficulty, streaming forest chunks, zombie pursuit, menus, pause/restart, settings, and a foggy forest-at-dusk atmosphere. Procedural scenery now mixes layered conifers and broadleaf trees over a textured forest floor.

## Important art and testing limits

This source package does **not** contain realistic licensed human/zombie models, humanoid rigs, animation clips, scanned forest assets, recorded sound, or a volumetric-rendering setup. The procedural survivor, zombies, vegetation, and props are fallback geometry and are not AAA assets. The project is intended to run without downloads; use licensed, rigged assets in the `GameBootstrap` prefab/audio fields to approach the visual target. Zombie navigation is local steering around nearby colliders, not navmesh pathfinding.

Unity compiled the runtime and editor scripts and built a Windows standalone player. The player launched and remained responsive during a short smoke run; its startup log had no common runtime exceptions. Full input-by-input gameplay QA has not been completed.

## Open and play

1. Open this folder in Unity 6 (or a compatible recent Unity LTS) with Windows Build Support.
2. Open `Assets/Scenes/DeepForest.unity`.
3. Ensure **Project Settings > Player > Active Input Handling** is **Input Manager (Old)** or **Both**.
4. Press **Play**, then select **START RUN**.
5. To recreate the bootstrap scene, use **Deep Forest > Create Playable Scene**.

The scene is in Build Settings. In Unity 6, use **File > Build Profiles** to select a Windows player and build.

## Controls

- **A / Left Arrow**, **D / Right Arrow**: move between lanes
- **Space / Up Arrow**: jump
- **S / Down Arrow**: slide
- **Left Shift**: sprint while stamina remains
- **Escape**: pause/resume
- On-screen controls: start, pause/resume, restart, main menu, settings

The survivor runs forward automatically. Collect coins and health kits, avoid lane obstacles, manage stamina and avoid the pursuing zombies. The HUD shows distance, score, coins, health, elapsed time, stamina, and zombie threat.

## Optional assets

Assign assets on the `GameBootstrap` component in `DeepForest.unity`:

- Survivor prefab: detailed model with root at the feet; optional Animator parameters are `Speed` (float), `Grounded` (bool), `Jump` (trigger), and `Slide` (bool).
- Zombie prefab: model with root at feet; `ZombieAI` and a CharacterController are added when missing.
- Tree and bush prefabs: keep colliders appropriately sized and outside the running lanes.
- AudioClips: forest ambience, horror music, player footsteps, collision, and zombie growl.

Free asset sources to review (verify current licenses and distribution rights): Unity Asset Store free packs; Mixamo character/animation downloads; Poly Haven and ambientCG CC0 materials/HDRIs; Sonniss GDC bundles; and Freesound assets filtered by appropriate license.

## Systems and tuning

- `PlayerController`: lane movement, jump, slide, stamina sprint, collision handling, fallback character motion.
- `GroundSpawner`, `ForestGenerator`, `ObstacleSpawner`: deterministic streamed tile generation and props.
- `ZombieSpawner`, `ZombieAI`: capped pursuing enemies and obstacle steering.
- `GameManager`, `HealthSystem`, `CoinManager`: run state, distance/time/score, damage, pickups, restart.
- `UIManager`: HUD, pause, settings/volume/quality, menu and game-over screens.
- `ForestAtmosphere`: run-time-driven daylight-to-dusk lighting/fog.

Runtime-generated objects can be inspected during Play mode; their inspector changes are not persisted.

## Validation checklist

1. The source has been compiled and a Windows player has been built; still check the Unity Console when making changes.
2. Start a run; test all movement keys, sprint depletion/recovery, lane obstacles, pickups, and health.
3. Verify zombie pursuit and the danger meter.
4. Verify pause/resume, volume/quality settings, restart, and game-over/menu flow.
5. A standalone player is available at `Build/Windows/DeepForest.exe`. Test it on the target Windows hardware; the smoke run is not a substitute for complete gameplay QA.
