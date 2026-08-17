# Progress Summary / 进度总结

## Completed / 已完成

### 1. Single-player Test / 单人全链路测试

- 6 scenes (Setup->Practice->Lobby->LoadTrial->Experiment->End) all run successfully / 6 个场景全部跑通
- Tracking data written to results/ folder with correct format / 追踪数据正确写入 results/
- 3 targets x 1 repetition = 3 trials per participant / 3 个目标球 x 1 次重复 = 每人 3 个试次

### 2. Multiplayer Networking / 多人网络

- Ubiq RoomServer connected (127.0.0.1:8009) / 房间服务器已连接
- Fixed room GUID "4b5e1f8a..." so all clients join same room / 固定房间号保证所有客户端进入同一房间
- RoomClient timeout disabled to prevent disconnects / 禁用自动重连防止掉线
- Both Editor and Build can join the same lobby / Editor 和 Build 可以进入同一大厅

### 3. Bug Fixes / Bug 修复

- Mouse look: playerBody was pointing to Body mesh instead of root / 视角无法左右转——playerBody 错误指向胶囊体
- Canvas renderMode: WorldSpace -> Screen Space Overlay / Canvas 渲染模式修正
- Cursor: MouseTracker.Start() locked cursor after TrialOverview unlocked it / 光标锁定顺序冲突
- Build errors: Fixed 3 compile errors (corrupt JoinAllRoomClients, Editor API references) / 修复 3 个编译错误
- PlayerMovement groundCheck auto-find / 自动查找地面检测点
- Lobby room joining was missing (LobbyManager now calls roomClient.Join directly) / 大厅加入房间逻辑缺失

### 4. New Scripts / 新增脚本

- PeerManager.cs: spawns/destroys remote avatars on peer join/leave / 远端玩家进出时生成/销毁替身
- RemoteAvatar.prefab: lightweight remote player with disabled local input / 轻量级远端玩家预置体

### 5. Flow Improvements / 流程优化

- TrialOverview: optional skip between-task panels (showPanelBetweenTasks=false) / 支持跳过任务间面板
- AutoStartNextTrial() continues task loop without panel / 自动续接下一个任务

---

## Server Config / 服务器配置

| Service / 服务 | Address / 地址 | Port / 端口 |
|---------------|---------|------|
| Data collection Flask / 数据收集 | localhost | 8080 |
| Ubiq RoomServer / 多人同步 | 127.0.0.1 | 8009 |

Start commands / 启动命令：

```bash
python server.py                                    # Data collection / 数据收集
npx @ucl-vr/ubiq-server                             # Ubiq room server / 多人同步
```

---

## Key Files Changed / 关键改动文件

| File / 文件 | Change / 改动 |
|------------|-------------|
| MouseTracker.cs | isLocalPlayer guard + playerBody fix / 本地玩家检测 + 旋转轴修正 |
| TrialOverview.cs | showPanelBetweenTasks + AutoStartNextTrial / 任务自动续接 |
| TrialSyncManager.cs | Database.EndTrial() + panel skip / 结束试次 + 跳过面板 |
| Target.cs | showPanelBetweenTasks check / 自动模式判断 |
| LobbyManager.cs | roomClient.Join(fixedGUID) + timeoutBehaviour=None / 加入固定房间 + 防掉线 |
| PeerManager.cs | timeoutBehaviour=None / 防掉线 |
| PlayerMovement.cs | groundCheck auto-find / 自动查找地面检测点 |
| JoinAllRoomClients.cs | Fixed corrupt first line / 修复损坏的第一行 |
| CaptureWalkthrough.cs | UNITY_EDITOR guard / 编辑器 API 守卫 |
| POIMarkerScript.cs | UNITY_EDITOR guard / 编辑器 API 守卫 |
