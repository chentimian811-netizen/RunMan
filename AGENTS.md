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
| `MenuController` | Singleton `MonoBehaviour`. Main menu / host-join popups / pause menu. Controls `gameHUDPanel` visibility: hidden at start, shown on Host/Join confirm, hidden on exit |
| `PlayerController` | `NetworkBehaviour`. `CharacterController` movement, jump buffer, attack (`OverlapSphere`), `SyncVar health` |
| `EnemyContorller` | `NetworkBehaviour`. State machine split into `UpdateState()` (transitions) + `FixedUpdate()` (movement). Rotation uses `Quaternion.RotateTowards` for smooth interpolation. Animation driven by `currenteState` SyncVar (Patrol=0.6, Chase=1.0, Attack=0.0) |
| `EnemySpawner` | `NetworkBehaviour`. Spawns enemies on `OnStartServer` |
| `RoomAuthenticator` | `NetworkAuthenticator` subclass. Password via `AuthRequestMessage`/`AuthResponseMessage` |
| `ThirdPersonCameraController` | Unparents from player, mouse orbit with collision push |
| `GameHUD` | `MonoBehaviour`. Displays round timer and collectible count. Visibility controlled by `MenuController` |
| `ResultUI` | Singleton `MonoBehaviour`. Win/lose result panel, shown via `RpcShowResult` |
| `HealthBarController` | `MonoBehaviour`. Smooth-lerped health bar with color gradient (green→yellow→red) |

## Known Issues & Gotchas

- **`SpeedArea.cs`** calls `GameObject.FindGameObjectWithTag("Player")` — will find arbitrary player in multiplayer. Should use cached reference or validate `isLocalPlayer`.
- **`GameManager.Start()`** sets `timeRemaining` on all clients without `isServer` check — SyncVar from host will overwrite, but clients briefly see wrong value.
- **`DeadArea.cs`** teleports player without `isServer` check — runs on all clients.
- **`JumpArea.cs`** is a dead stub (empty `OnTriggerEnter`).
- **Spelling**: `Contorller`, `Piont`, `Falsh`, `SavePostion`, `ReSpeed`, `falshDuration` are consistent typos across codebase.
- All interaction scripts use `OnTriggerEnter` on `"Player"` tag — ensure colliders are triggers.

## Network Transform Settings

Both Player and Enemy prefabs use `NetworkTransformHybrid` (Mirror). Key settings that must be preserved:

| Setting | Value | Reason |
|---------|-------|--------|
| `onlySyncOnChange` | `true` | Enemy/Player静止时不发包；设为false会每帧发位置更新，造成join客户端严重卡顿 |
| `syncInterval` | `0.05` | 20Hz发送上限，降低网络压力 |
| `updateMethod` | `Update` (0) | 与 `LateUpdate` 的 `SetDirty` 对齐，确保采样稳定 |
| `syncDirection` | `ServerToClient` (Enemy) / `ClientToServer` (Player) | 敌人服务端权威，玩家客户端权威 |

> ⚠️ **不要**给敌人加 `NetworkAnimator` 或额外的 `SyncVar` 来同步动画参数 — 这会产生大量额外网络流量，导致join客户端卡顿。动画由 `currenteState` SyncVar 在客户端本地驱动。

## Style Notes

- Chinese comments mixed in (`//玩家旋转`, `//敌人缩小死亡效果`).
- `[SerializeField]` for serialized fields, no `[Tooltip]` on most.
- Mirror `[Command]`/`[ClientRpc]` patterns: `Cmd` prefix on commands, `Rpc` on clientRpc.
- No `.editorconfig`, no `.gitignore` beyond defaults.
