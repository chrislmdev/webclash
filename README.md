# WebClash

Endless mobile web-swinging game proof of concept for Unity. The floor is lava — traverse the city by shooting webs at buildings, collect booster packs, and survive as long as you can.

## Quick start

1. Open this repository as a Unity 2022 LTS+ project.
2. Install dependencies when prompted (Input System, TextMeshPro).
3. Follow **[SCENE_SETUP.md](SCENE_SETUP.md)** to build the prototype scene.

## Scripts

| Script | Role |
|--------|------|
| `GameManager.cs` | Singleton; game state, distance score, restart |
| `PlayerController.cs` | Rigidbody movement, death checks, boost effects |
| `WebShooter.cs` | Touch swing via raycast, SpringJoint, LineRenderer |
| `FloorIsLava.cs` | Lava plane kill trigger |
| `BoosterPack.cs` | Collectible speed / double-jump boost |
| `ObjectPooler.cs` | Spawns and recycles buildings and boosters |
| `MobileUIManager.cs` | HUD score and Game Over UI |
| `CameraFollow.cs` | Optional follow camera |

All gameplay scripts live in `Assets/Scripts/`.

## Controls

- **Tap and hold** — shoot web, swing from buildings
- **Release** — detach web, carry momentum forward
- **Drag while airborne** — light air control

## License

Proof of concept — use freely for iteration and prototyping.
