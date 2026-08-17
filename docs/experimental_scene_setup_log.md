# ExperimentZhuo Setup Log / 实验场景搭建日志

## Scene Flow / 场景流程

```
SetupZhuo -> PracticeZhuo -> LobbyZhuo -> LoadTrial -> ExperimentalZhuo -> EndZhuo
```

| Scene | Purpose / 用途 |
|-------|---------------|
| SetupZhuo | Participant info, read URL params (group/ExpID/sessionId) / 收集参与者信息 |
| PracticeZhuo | Mouse/keyboard/find-goal practice / 操作练习 |
| LobbyZhuo | Wait for all peers, countdown / 多人等待大厅 |
| LoadTrial | Load additive scene by ParticipantGroup / 按组加载试验场景 |
| ExperimentalZhuo | Loop all Targets, record tracking data / 遍历目标球，记录数据 |
| EndZhuo | Thank-you + redeem code / 结束页 |

---

## Server Config / 服务器配置

| Server / 服务 | Address | Port | Role / 作用 |
|---------------|---------|------|-------------|
| Data collection (Flask) / 数据收集 | localhost | 8080 | Receive experiment data via HTTP POST / 接收 Unity 上传的追踪数据 |
| Ubiq RoomServer / 多人同步 | 127.0.0.1 | 8009 | Relay peer positions in real-time / 实时广播所有玩家位置 |
| WebGL deployment / 线上部署 | eth-cog.s3... | — | Public experiment URL / 参与者访问的网页 |

**Start commands / 启动命令：**

```bash
cd vr_experiments\data_collection_backend\python
python server.py                       # Data server / 数据收集

npx @ucl-vr/ubiq-server                # Ubiq room server / 多人同步
```

**Configure Ubiq in Unity / Unity 配置 Ubiq：**
1. Project -> Create -> Ubiq -> Connection Definition
2. Set: Type=TcpClient, SendToIp=127.0.0.1, SendToPort=8009
3. Drag into RoomClient -> Servers array

---

## New Files / 新增文件

| File / 文件 | Description / 说明 |
|------------|-------------------|
| `PeerManager.cs` | Spawns/destroys remote avatar prefabs on peer join/leave / 远端加入时创建替身，离开时销毁 |
| `RemoteAvatar.prefab` | Lightweight remote player: UbiqNetworkedPlayer+PlayerMovement+MouseTracker (all isLocalPlayer=false) / 远端玩家轻量化身 |

---

## Code Changes / 代码改动

### MouseTracker.cs

| Change / 改动 | Why / 原因 |
|-------------|-----------|
| Added `public bool isLocalPlayer = true` | Prevent remote avatars from reading local mouse / 防止远端被本地鼠标操控 |
| Added `if (!isLocalPlayer) return;` in Update() | Same as above / 同上 |

### TrialOverview.cs

| Change / 改动 | Why / 原因 |
|-------------|-----------|
| Added `showPanelBetweenTasks = false` | Control whether panel shows between tasks / 控制任务间是否弹面板 |
| Added `AutoStartNextTrial()` | Auto-start next task without showing panel / 任务完成后自动续接 |

### TrialSyncManager.cs

| Change / 改动 | Why / 原因 |
|-------------|-----------|
| Added `Database.EndTrial()` in ApplyTrialEnd() | EndTrial was missing in multiplayer path / 多人模式下未正确结束试次 |
| Check `showPanelBetweenTasks` | Skip panel in auto-advance mode / 自动模式下跳过面板 |

### Target.cs

| Change / 改动 | Why / 原因 |
|-------------|-----------|
| Check `showPanelBetweenTasks` in single-player path | Skip panel between tasks / 单机模式下任务间不弹面板 |

---

## Bugs Fixed / Bug 修复

### 1. Head cannot yaw (left/right) / 头无法左右转

**Cause / 根因：** MouseTracker.playerBody was pointing to the "Body" mesh node instead of the root. Rotating Body only spins the capsule, not the camera.

**Fix / 修复：** Drag FPSControllerZhuo root into MouseTracker's Player Body field in Inspector.

### 2. TrialOverview panel invisible / 面板不显示

**Cause / 根因：** Canvas Render Mode was "World Space" instead of "Screen Space - Overlay".

**Fix / 修复：** Change Canvas render mode to Screen Space - Overlay.

### 3. Cursor permanently locked / 鼠标永久锁定

**Cause / 根因：** OnEnable() runs before Start(). TrialOverview.OnEnable() unlocks cursor, then MouseTracker.Start() locks it again.

**Fix / 修复：** Add isLocalPlayer guard in MouseTracker.Start().

### 4. GUID conflict / GUID 冲突

**Cause / 根因：** Manually created .meta files with fake GUIDs didn't match Unity's auto-generated ones.

**Fix / 修复：** Delete fake .meta files, let Unity regenerate them.

---

## Knowledge / 知识点

### FPS Hierarchy / 层级结构

```
FPSControllerZhuo            <- Root / 根 (CharacterController, movement)
+-- FirstPersonCharacter 1   <- Body pivot / 旋转轴 (empty Transform)
    +-- FieldOfView           <- Camera / 相机 (MouseTracker)
    +-- Body                  <- Visual mesh / 胶囊体
    +-- groundCheck
```

**playerBody rule / 选择规则：** Drag in a node that is an ancestor of the camera. Rotating an ancestor rotates all descendants.
拖入的节点必须是相机的祖先——旋转祖先，子孙跟着转。

| playerBody | Effect / 效果 |
|-----------|---------------|
| FPSControllerZhuo (Root) | Look+walk same direction / "看哪走哪" |
| FirstPersonCharacter 1 (Middle) | Look freely, W always north / "头四处看，W 永远向北" |

### Server concepts / 服务器概念

- `dataAssemblyUrl` is like a phone number — `server.py` is the person who answers / dataAssemblyUrl 是"电话号码"，server.py 是"接电话的人"
- Flask stores data; Ubiq relays positions; WebGL serves the page / Flask 存数据，Ubiq 传位置，WebGL 提供页面
- All three run locally during development / 开发阶段全跑在本地

### .meta files and GUID / .meta 文件与 GUID

- .meta = Unity's "ID card" for each asset / .meta 是 Unity 给每个资源发的"身份证"
- Internally referenced by GUID, not filename / 内部用 GUID 引用，不靠文件名
- Never create .meta files manually — let Unity auto-generate / 不要手动创建，让 Unity 自动生成

---

## Scene Checklist / 场景清单

- [x] Canvas — Screen Space - Overlay
- [x] TrialOverviewPanel — wired to FPSController, SyncManager, Spawnpoint
- [x] FPSControllerZhuo — PlayerMovement + UbiqNetworkedPlayer(isLocalPlayer=true) + CharacterController
- [x] FieldOfView — Camera + MouseTracker(playerBody=FPSControllerZhuo)
- [x] Targets (3 spheres) — Target component with Number + Description
- [x] Spawnpoint / Recorder / TrialSync / PeerManagerObject / HUD
- [x] Ubiq Network Scene — RoomClient configured with RoomServerConnection
- [x] EventSystem
- [x] TargetMaterials (6 mats in Resources/TargetMaterials/)
