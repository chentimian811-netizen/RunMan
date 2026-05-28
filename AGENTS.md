# RunMan — AGENTS.md

A Unity 2022.3.62f2c1 multiplayer 3D action game using Mirror networking.

## Build & Run

- Open `RunMan.sln` in Unity Editor, not standalone IDE.
- Two scenes in build: `Assets/Scenes/Login.unity` (→ `Demo6.unity`)
- No tests, lint, or CI/CD configured.

## Architecture

- **Networking**: Mirror (embedded `Assets/Mirror/`). No NetworkManager subclass — uses vanilla `NetworkManager` component with `RoomAuthenticator` for password auth.
- **Entry flow**: Login → Store account in `PlayerPrefs` → Load `Demo6` scene. `MenuController` handles Host (password popup) / Join (IP+password popup) / pause menu.
- **Global singletons**: `GameManager.instance`, `MenuController.instance`, `ResultUI.instance`, `RoomAuthenticator.instance`, `Uimanager.instance`, `CameraShake.instance`.
- **Server-authoritative**: Game logic gated by `isServer` (enemy AI, collectibles, extraction, damage, spawn). Clients send `[Command]` for actions.

## Key Scripts

| Script | Role |
|--------|------|
| `GameManager` | Singleton `NetworkBehaviour`. Server runs round timer via `SyncVar timeRemaining`, calls `PlayerExtracted()` or `OnTimeUp()` |
| `PlayerController` | `NetworkBehaviour`. `CharacterController` movement, jump buffer, attack (`OverlapSphere`), `SyncVar health` |
| `EnemyContorller` | `NetworkBehaviour`. State machine (Patrol/Chase/Attack). `OnAttackHit()` called from `AnimationEventRelay` |
| `EnemySpawner` | `NetworkBehaviour`. Spawns enemies on `OnStartServer` |
| `RoomAuthenticator` | `NetworkAuthenticator` subclass. Password via `AuthRequestMessage`/`AuthResponseMessage` |
| `ThirdPersonCameraController` | Unparents from player, mouse orbit with collision push |

## Known Issues & Gotchas

- **`SpeedArea.cs`** calls `GameObject.FindGameObjectWithTag("Player")` — will find arbitrary player in multiplayer. Should use cached reference or validate `isLocalPlayer`.
- **`GameManager.Start()`** sets `timeRemaining` on all clients without `isServer` check — SyncVar from host will overwrite, but clients briefly see wrong value.
- **`DeadArea.cs`** teleports player without `isServer` check — runs on all clients.
- **`JumpArea.cs`** is a dead stub (empty `OnTriggerEnter`).
- **Spelling**: `Contorller`, `Piont`, `Falsh`, `SavePostion`, `ReSpeed`, `falshDuration` are consistent typos across codebase.
- All interaction scripts use `OnTriggerEnter` on `"Player"` tag — ensure colliders are triggers.

## Style Notes

- Chinese comments mixed in (`//玩家旋转`, `//敌人缩小死亡效果`).
- `[SerializeField]` for serialized fields, no `[Tooltip]` on most.
- Mirror `[Command]`/`[ClientRpc]` patterns: `Cmd` prefix on commands, `Rpc` on clientRpc.
- No `.editorconfig`, no `.gitignore` beyond defaults.
