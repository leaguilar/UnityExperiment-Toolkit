# Progress Summary (Part 2) / 进度总结（第二部分）

## Build Fixes / Build 修复

### Lobby -> Experiment transition broken / 大厅不跳转

Root cause: `context.Send()` threw NullRef in Build because `NetworkedBehaviour.context` was null. This silently killed `StartCountdown()`.
Root cause: Build 中 `context.Send()` 抛 NullRef 异常，导致 `StartCountdown()` 被跳过。

Fix: wrapped `context.Send()` in try-catch so countdown always runs even if network send fails.
Fix: 加 try-catch 包住 `context.Send()`，即使发送失败倒计时也继续。

### Config issues / 配置文件问题

- LoadTrial missing from Build Settings / LoadTrial 未加入 Build
- Removed LoadTrial dependency, Lobby now transitions directly to ExperimentalZhuo / 改成直接跳转

---

## New 8-Step Medical Workflow / 新 8 步医疗流程

### Target.cs Rewrite / 重写

| Feature / 功能 | Detail / 说明 |
|---------------|-------------|
| `requireInteraction` | true = press E to complete, false = walk-in / true=按E完成, false=走到就完成 |
| `keepMeshVisible` | true = don't hide mesh when not active target (for patient targets) / 非当前目标时也不隐藏（病人用） |
| `onTargetCompleted` | UnityEvent fired on completion, connects to PatientAgent.Interact() / 完成时触发事件，连到病人交互 |

### TrialOverview Changes

| Feature / 功能 | Detail / 说明 |
|---------------|-------------|
| `randomizeOrder` | false = execute targets in Number order (1,2,3...) / 按编号顺序执行 |
| HUD hint | Shows `Step X/8 - Description [Press E to complete]` during trials / 试次中显示步骤提示 |

### PatientAgent.cs (New) / 新脚本

Standalone patient NPC with: Name, Age, Condition, Bed attributes. Shows info panel on E-interact, then walks to destination via NavMeshAgent.
独立病人 NPC：带姓名、年龄、病情、床位属性。按 E 显示信息面板，然后通过 NavMesh 走向目标点。

### Scene Setup / 场景结构

```
Patient_NPC                       <- PatientAgent + Capsule + NavMeshAgent + Cap Collider
  +-- Capsule (body)               <- 视觉模型
  +-- TriggerSphere                <- SphereCollider(isTrigger) + Target(Number, requireInteraction, keepMeshVisible)
                                      Target.onTargetCompleted -> Patient_NPC.PatientAgent.Interact()
```

Steps 1/5/7 use this structure. Steps 2/3/4/6/8 use plain Target spheres (walk-to or E-interact at location).
步骤 1/5/7 用此结构，步骤 2/3/4/6/8 用普通目标球。

### Bug Fixes / Bug 修复

- HideInteractionPrompt was called AFTER AutoStartNextTrial, hiding the new hint / 先更新提示再隐藏它，顺序反了
- Cursor/speed separation fixed by removing static RemoteAvatar from scene / 删掉场景里静态 RemoteAvatar 解决视角分离
- PlayerMovement groundCheck auto-find added / 自动查找地面检测点
- layerMask set to Everything to prevent sinking / layerMask 设 Everything 防止下沉

---

## Server Config (unchanged) / 服务器配置（不变）

| Service / 服务 | Address | Port |
|---------------|---------|------|
| Data collection / 数据收集 | localhost | 8080 |
| Ubiq RoomServer / 多人同步 | 127.0.0.1 | 8009 |
