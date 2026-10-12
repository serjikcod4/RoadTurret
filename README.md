# Road Turret

Core mechanic for the **Factura Unity Developer** test task. A car drives forward along the road and the player controls the turret mounted on it. Stickmen idle at random spots, run at the car when it gets close and smash into it. The goal is to reach the finish line without losing all of the car's HP, while killing as many enemies as possible.

▶ **Gameplay video (57 s, win + lose):** [watch / download](https://github.com/serjikcod4/RoadTurret/raw/main/Media/RoadTurret_Gameplay.mp4) (`Media/RoadTurret_Gameplay.mp4`)

## How to run

1. Open the project in **Unity 6000.6.0f1** with URP. The task lists 6000.0 / 6000.3; the project was made in the newer 6000.6 release of Unity 6.
2. Open the scene `Assets/_Project/Scenes/Game.unity`.
3. Set the Game view to a portrait resolution, for example 1080×1920, and press **Play**.

Packages restore automatically. VContainer and UniTask come from OpenUPM, and the scoped registry is already in `Packages/manifest.json`.

### Controls

| Action | Mouse | Touch |
| --- | --- | --- |
| Start / restart | Click | Tap |
| Aim the turret | Move the cursor, no button needed | Drag a finger across the screen |

The turret turns toward the point under the cursor or finger, limited to a 120° arc in front of the car, and fires automatically while the car is driving.

## Gameplay

- **Game loop.** The camera starts behind the stationary car. A tap starts the drive and the camera follows. At the finish line the car stops and **You win** is shown. If the car's HP reaches 0, **You lose** is shown. The result screen shows the kill count. Another tap resets the level: the car waits for the next tap, and so on.
- **Enemies.** 30 stickmen are spread evenly along the 260 m road at random positions across its width. They idle until the car comes within 20 m, then run at it. An enemy that reaches the car crashes into it: it deals damage, flies off to the side and dies. There is no attack animation, as in the reference. Bullets kill an enemy in one or two hits (15 HP, 10–20 damage per bullet). Only kills made by the turret are counted.
- **Turret.** Aims at the cursor or finger point on the ground, with light smoothing. Its rotation is limited to ±60°. It fires 2.4 shots per second with a small spread and has recoil, a muzzle flash and an aim line.
- **Feel:**
  - When hit, the car flashes white (turret included), pops slightly in scale, throws sparks and shakes the camera.
  - The car explodes when destroyed.
  - Enemies flash on hit, show floating damage numbers and splash blood that collides with the ground.
  - Corpses slide back with knockback and sink into the ground.
- **UI:**
  - HUD with a kill counter and a vertical progress bar showing the remaining distance.
  - Start and result overlays with fades.
  - World-space health bars. The car's bar sits on its rear bumper and has a color gradient, an HP number, 10 segments, a delayed damage trail and a punch on hit. Enemies have compact red bars.

All numbers live in ScriptableObject configs in `Assets/_Project/Configs` and can be tuned without touching code: HP, speeds, damage, aggro radius, fire rate, level length and enemy count.

## Architecture

Dependency injection uses **VContainer**, async flow uses **UniTask**, and input uses the **Input System** with actions defined in code.

```
GameLifetimeScope (VContainer)
├── Configs            CarConfig, TurretConfig, EnemyConfig, LevelConfig (ScriptableObjects)
├── Scene components   Car, Turret, Track, Camera, CameraFollow, HudView, OverlayView
├── Services           EffectsPlayer, PrefabPool<Bullet | Enemy | DamageNumber>, DamageNumbers, EnemySpawner
└── Entry points       PointerPlayerInput (IPlayerInput), TurretAimController, GameFlow
```

- **`GameFlow`** (`IAsyncStartable`) is the whole game loop written as plain sequential async code: prepare round → wait for a tap → drive until the finish or until the car is destroyed → show the result → wait for a tap. There are no state-machine classes or screen events.
- **`IPlayerInput` / `PointerPlayerInput`** hide the input device. Mouse and touch share the same `<Pointer>` bindings.
- **`TurretAimController`** (`ITickable`) projects the pointer onto the ground plane and passes the point to `Turret.AimAt`. The turret itself knows nothing about input.
- **`IDamageable` + `Health`.** `Car` and `Enemy` both implement `IDamageable`, and a bullet damages anything that implements it. HP logic is a small plain C# class with `Changed` and `Died` events, which the health bars subscribe to.
- **`PrefabPool<T>`** wraps `UnityEngine.Pool.ObjectPool` with prewarming. Instances are created through `IObjectResolver`, so pooled prefabs receive `[Inject]` dependencies. Bullets, enemies and damage numbers are pooled.
- **`EffectsPlayer`** keeps one pool per effect prefab. `PooledEffect` returns itself to its pool when its particle system stops.
- **`EnemySpawner`** places enemies one per road segment with random side offsets and a minimum spacing, so pressure stays even along the level. It also counts kills and returns enemies left far behind to the pool.
- **`Track`** tiles the ground model along the level length at startup. It provides the start pose, the distance along the road and the finish check.
- **`CameraFollow`** follows the car with smoothing and adds trauma-based shake when the car takes damage.
- Small view components handle hit feedback: `HitFlash` (emission through `MaterialPropertyBlock`, no material instances), `CarHitReaction`, `WorldHealthBar`, `DamageNumber` and `PulseScale`.

The code is split into one assembly, `RoadTurret.Runtime`, with a folder and namespace per feature:

```
Assets/_Project/Scripts/Runtime
├── Core           GameLifetimeScope, GameFlow, GameResult
├── Configs        ScriptableObject configs
├── Common         Health, IDamageable, PrefabPool<T>
├── Input          IPlayerInput, PointerPlayerInput
├── Vehicle        Car, CarHitReaction
├── Weapons        Turret, TurretAimController, Bullet
├── Enemies        Enemy, EnemySpawner
├── Effects        EffectsPlayer, PooledEffect, HitFlash
├── Level          Track
├── CameraSystem   CameraFollow
└── UI             HudView, OverlayView, WorldHealthBar, DamageNumber(s), PulseScale
```

## Scene

```
Game
├── GameLifetimeScope
├── Environment     Directional Light, Global Volume
├── Track           StartPoint, Ground (tiles are created at startup), FinishLine
├── Car             Body (Model, TurretMount/Turret), ExhaustPoint, HealthBar
├── Main Camera
├── Pools           Bullets, Enemies, Effects, DamageNumbers
└── UI              Hud, Overlay
```

**Physics.** Bullets have no Rigidbody. They move in code and use a `SphereCast` from the previous position each frame against the `Enemy` layer, so fast bullets never tunnel. Enemies have a kinematic Rigidbody and a CapsuleCollider. Collisions for the `Enemy` layer are disabled in the layer matrix, because the colliders are used only for queries. Trigger hits are disabled for queries. The ground has a collider on the `Ground` layer, used only for blood particle collisions. Enemies move toward the closest point on the car's box collider, so the crash happens exactly at the bodywork.

**Lighting and rendering.** URP with a realtime directional light and soft shadows, no baked GI, trilight ambient and linear fog matching the background color. Global Volume:
- Neutral tonemapping;
- Bloom, so bullets and muzzle flashes glow;
- Color Adjustments;
- Vignette.

## Assets

- **From the task archive:** models of the car, turret, ground and stickman. All models use one palette texture, and the car uses its own texture. The ground model is copied along the level length and stretched sideways for a wider road.
- **Mixamo:** the stickman rig and the Idle, Run and Death animations, set up as a Humanoid with a single Animator Controller.
- **Font:** Lilita One, licensed under the SIL Open Font License (`Assets/_Project/Art/Fonts/LilitaOne-OFL.txt`).
- **Made in the project:** bullets, particle effects, UI sprites and the finish line.

```
Assets/_Project
├── Art         Models, Animations, Textures, Materials, Fonts, UI
├── Configs     ScriptableObject configs
├── Prefabs     Vehicle, Weapons, Enemies, Environment, Effects, UI
├── Rendering   Volume profile, lighting settings
├── Scenes      Game.unity
└── Scripts     Runtime
```
