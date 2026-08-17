# Development Plan / 开发计划 (Target: Wednesday Meeting / 目标：周三汇报)

## Done / 已完成

- [x] Ubiq RoomServer connected (127.0.0.1:8009) / Ubiq 房间服务器已连接
- [x] RoomServerConnection asset created / 连接配置文件已创建
- [x] PeerManager.cs: spawn/destroy remote avatars / 远端替身自动生成/销毁
- [x] RemoteAvatar prefab (isLocalPlayer=false, camera disabled) / 远端化身预制体
- [x] MouseTracker: isLocalPlayer guard + playerBody fix / 鼠标旋转修复
- [x] TrialOverview: showPanelBetweenTasks + AutoStartNextTrial / 任务自动续接
- [x] Canvas Render Mode -> Screen Space Overlay / 画布模式修复
- [x] LobbyManager: requiredParticipants=1 / 大厅单人模式
- [x] Data server dependency (Flask + flask-cors installed) / 数据服务器依赖已装
- [x] Documentation (experimental_scene_setup_log.md) / 文档已生成

---

## Mon / 周一 (Today)

### Core / 核心

- [ ] 1. Single-player full runthrough (Setup -> End) / 单人全链路测试
- [ ] 2. Verify data written to results/ folder / 验证数据写入
- [ ] 3. Fix any crash/error found in runthrough / 修复测试中的报错

### Practice Scene / 练习场景

- [ ] 4. Drag FPSControllerZhuo prefab into PracticeZhuo scene / 拖入 FPSController 预制体
- [ ] 5. Wire PracticeScenePage.FPSController -> PlayerMovement / 连线 FPSController 引用
- [ ] 6. Wire TestMouseMovement.Camera -> FieldOfView Camera / 连线鼠标测试相机
- [ ] 7. Wire FindGoalTest: Trigger -> ProximityZone, Hint -> GoalHint / 连线寻球测试
- [ ] 8. Wire RegisterTest: patient -> Patient_NPC, computerScript -> OpenKISIM / 连线注册测试
- [ ] 9. Test all practice tasks sequentially / 测试全部练习任务流程
- [ ] 10. Test AutoSceneTransition -> Lobby countdown / 验证自动跳转大厅

### Experiment Scene Polish / 实验场景完善

- [ ] 11. Review all Target descriptions & positions / 检查 3 个目标的描述和位置
- [ ] 12. Test HUD HintText visibility during trials / 测试任务提示文字显示
- [ ] 13. Test Spawnpoint reset after each trial / 测试每轮回复位点
- [ ] 14. Verify TargetMaterials (6 mats) color matching / 验证材质颜色匹配

---

## Tue / 周二

### Multiplayer / 多人测试

- [ ] 15. Launch two Unity Editor instances / 启动两个 Editor 实例
- [ ] 16. Connect both to same RoomServer / 确认两人加入同一房间
- [ ] 17. Verify remote avatar spawns (Body + name tag) / 验证远端替身出现
- [ ] 18. Verify remote avatar position tracking (walk test) / 验证替身位置跟随
- [ ] 19. Verify remote avatar head tracking (look test) / 验证替身头部跟随
- [ ] 20. Verify trial sync: one clicks Start -> both start / 验证试次同步开始
- [ ] 21. Verify trial sync: one reaches target -> both advance / 验证试次同步结束

### Edge Cases / 边界情况

- [ ] 22. Test participant disconnect/reconnect during trial / 测试掉线重连
- [ ] 23. Test RoomServer restart mid-experiment / 测试房间服务器重启
- [ ] 24. Test with 0 targets in scene (graceful exit) / 测试空目标场景
- [ ] 25. Test admin backdoor (6-4-8-5-9-9-7-2) / 测试管理员后门

### Build / 打包

- [ ] 26. Verify all Zhuo scenes in Build Settings / 检查 Build Settings 场景列表
- [ ] 27. Build WebGL (test compilation) / 尝试 WebGL 打包
- [ ] 28. Test local deployment with serve_experiment.go / 本地部署测试
- [ ] 29. Verify URL parameters parsing (group, ExpID) / 验证 URL 参数解析

---

## Wed / 周三 (Morning / 早上)

### Final Checks / 最终检查

- [ ] 30. Final full single-player runthrough / 最终单人全链测试
- [ ] 31. Final multiplayer test (if ready) / 最终多人测试
- [ ] 32. Check all Console logs clean (no red errors) / 清理所有红字报错
- [ ] 33. Review action plan & update checklist / 回顾计划更新清单

### Meeting Prep / 会议准备

- [ ] 34. Prepare demo: live single-player run / 准备演示：单人流程
- [ ] 35. Prepare demo: 2-player walkthrough (if ready) / 准备演示：双人流程
- [ ] 36. Prepare screenshots of data output / 准备数据输出截图
- [ ] 37. Note remaining work for next sprint / 整理下阶段待办

---

## Quick Reference / 速查

| Item / 项目 | Command/Location / 命令/位置 |
|------------|---------------------------|
| Data server / 数据服务器 | `python server.py` in data_collection_backend/python/ |
| Ubiq RoomServer / 房间服务器 | `npx @ucl-vr/ubiq-server` |
| Config file / 配置文件 | StreamingAssets/experiment_1_config.json |
| Editor log / 编辑器日志 | Console window in Unity |
| Data output / 数据输出 | data_collection_backend/python/results/ |
