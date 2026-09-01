# WebClash

Endless mobile web-swinging game proof of concept for Unity. The floor is lava — traverse the city by shooting webs at buildings, collect booster packs, and survive as long as you can.

## Quick start

1. Install **Unity Hub** and **Unity 2022.3 LTS** (or newer).
2. In Unity Hub: **Add → Add project from disk**.
3. Select the repo root folder (`webclash` — the one that contains `Assets` and `ProjectSettings`).
4. Open the project. Unity will import packages and generate a `Library/` folder on first launch.
5. Follow **[SCENE_SETUP.md](SCENE_SETUP.md)** to build the prototype scene.

> **Note:** Unity Hub requires `ProjectSettings/ProjectVersion.txt` to recognize a folder as a project. This repo includes that file. If Hub still says "No Unity project found", make sure you selected `j:\git\webclash` and not a parent folder.

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
