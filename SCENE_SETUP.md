# WebClash — Scene Setup Guide

Follow these steps to assemble the prototype scene in Unity 2022 LTS or newer.

## 1. Project prerequisites

1. Open this folder as a Unity project (or copy `Assets/` and `Packages/` into an existing project).
2. When prompted, allow Unity to install **Input System** and **TextMeshPro** from `Packages/manifest.json`.
3. In **Edit → Project Settings → Player → Other Settings**, set **Active Input Handling** to **Input System Package** (or **Both** for editor testing with mouse).

## 2. Tags and layers

Create these tags in **Edit → Project Settings → Tags and Layers**:

| Tag        | Used by                          |
|------------|----------------------------------|
| `Player`   | Player root object               |
| `Building` | Building prefab colliders        |

Optional: create a **Building** layer and assign it to building prefabs, then set the **Web Target Layers** mask on `WebShooter` to that layer only.

## 3. Scene hierarchy

Create a new scene named `WebClashMain` and build this hierarchy:

```
WebClashMain
├── GameManager          (GameManager.cs)
├── ObjectPooler         (ObjectPooler.cs)
├── Directional Light
├── Main Camera
├── Environment
│   └── LavaFloor        (Plane, Y = 0, FloorIsLava.cs)
├── Player               (tag: Player)
│   └── WebOrigin        (empty child, chest-height offset)
└── Canvas               (Screen Space - Overlay)
    ├── ScoreText        (TextMeshProUGUI)
    └── GameOverPanel    (disabled by default)
        ├── FinalScoreText
        └── TryAgainButton
```

Attach **MobileUIManager.cs** to the `Canvas` object and wire its serialized fields.

## 4. Lava floor (Y = 0)

1. Create **3D Object → Plane**, rename to `LavaFloor`.
2. Set **Transform → Position** to `(0, 0, 0)` and **Scale** to `(10, 1, 50)` (adjust as needed).
3. Add **FloorIsLava.cs**.
4. Ensure the **Box Collider** has **Is Trigger** checked (the script sets this on Reset).
5. Assign a bright lava-colored material for visual feedback.

## 5. Player setup

1. Create a **Capsule** (or cube) at `(0, 8, 0)`, tag it **Player**.
2. Add **Rigidbody**:
   - Mass: `1`
   - Use Gravity: ✓
   - Interpolate: Interpolate
   - Constraints: Freeze Rotation X/Y/Z
3. Add **Capsule Collider** (non-trigger).
4. Add **PlayerController.cs** and **WebShooter.cs**.
5. Create an empty child `WebOrigin` at roughly `(0, 0.8, 0.3)` and assign it to **Web Origin** on `WebShooter`.
6. Tune **Critical Y Threshold** on `PlayerController` to `1.5` (just above the lava plane).

### WebShooter tuning (starting values)

| Field            | Value   |
|------------------|---------|
| Max Web Distance | 40      |
| Cone Half Angle  | 22.5    |
| Spring           | 120     |
| Damper           | 8       |
| Min Distance     | 2       |
| Max Distance     | 30      |

Assign a simple white **Material** to **Web Line Material** (Unlit works well for mobile).

## 6. Prefabs

### Building prefab

1. Create a **Cube**, rename `Building`.
2. Tag: **Building**.
3. Add a **Box Collider** (non-trigger).
4. Apply a neutral building material.
5. Drag to `Assets/Prefabs/Building.prefab`.

The pooler randomizes scale at spawn time for variety.

### Booster prefabs

Create two small cube/sphere prefabs:

| Prefab          | Components                         | Pool tag       |
|-----------------|------------------------------------|----------------|
| BoosterSpeed    | BoosterPack (SpeedBoost), trigger  | `BoosterSpeed` |
| BoosterJump     | BoosterPack (DoubleJump), trigger  | `BoosterJump`  |

Scale roughly `(0.8, 0.8, 0.8)` and place at origin before saving as prefabs.

## 7. ObjectPooler configuration

Select `ObjectPooler` and configure:

**Player**: drag the Player transform.

**Pools** (three entries):

| Tag            | Prefab        | Initial Size |
|----------------|---------------|--------------|
| Building       | Building      | 20           |
| BoosterSpeed   | BoosterSpeed  | 5            |
| BoosterJump    | BoosterJump   | 5            |

**Spawn settings** (defaults are fine to start):

- Spawn Ahead Distance: `80`
- Despawn Behind Distance: `30`
- Spawn Interval: `1.5`
- Lane Width: `12`
- Booster Spawn Chance: `0.35`

## 8. Mobile UI

1. Create **UI → Canvas** (Screen Space - Overlay). Add **Canvas Scaler** → Scale With Screen Size, reference `1080 × 1920`.
2. Add **ScoreText** (top-center TMP): `"0 m"`, large font, white with shadow.
3. Add **GameOverPanel** (full-screen semi-transparent Image), disabled on start.
   - **FinalScoreText**: `"Distance: 0 m"`
   - **TryAgainButton**: label `"Try Again"`
4. On `Canvas`, add **MobileUIManager.cs** and wire:
   - Score Text → `ScoreText`
   - Game Over Panel → `GameOverPanel`
   - Final Score Text → `FinalScoreText`
   - Try Again Button → `TryAgainButton`

## 9. Camera

1. Position **Main Camera** behind and above the player, e.g. `(0, 6, -10)`.
2. Add a simple follow script or parent the camera to an empty that tracks the player on X/Y while the player moves forward on Z.

Minimal follow (optional script or animate in editor):

```csharp
// Optional: attach to Main Camera
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -10f);

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
```

## 10. GameManager

1. Create empty `GameManager` object.
2. Add **GameManager.cs**.
3. Assign **Score Reference** to the Player transform.

## 11. Build settings

1. Add `WebClashMain` to **File → Build Settings → Scenes In Build**.
2. For mobile testing:
   - Switch platform to **Android** or **iOS**.
   - Set **Player → Default Orientation** to **Portrait** (recommended for one-thumb swinging).
   - Enable **Internet Access** only if needed later.

## 12. Playtest checklist

- [ ] Tap/hold attaches a web to a building; release detaches and preserves momentum.
- [ ] Falling below Y threshold triggers Game Over.
- [ ] Touching the lava trigger triggers Game Over.
- [ ] Distance score increases as the player moves forward on Z.
- [ ] Booster packs apply speed or jump boost and disappear.
- [ ] Buildings and boosters recycle behind the player without frame spikes.
- [ ] **Try Again** reloads the scene.

## Architecture notes

- **GameManager** is the single authority for state and score; other systems subscribe via events or call `TriggerGameOver()` / `RestartScene()`.
- **No `FindObjectOfType` in `Update`** — references are serialized or cached once in `Awake`/`Start`.
- **ObjectPooler** owns spawn/despawn lifecycle; **BoosterPack** returns instances through the pooler reference set at spawn time.
- **WebShooter** and **PlayerController** share touch input: hold to swing, light drag for air control.
