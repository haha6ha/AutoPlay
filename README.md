# AutoPlay
Generate full-combo .bsor replays in Beat Saber
# AutoReplay 🐸

[![Game Version](https://img.shields.io/badge/Beat%20Saber-1.40.8-blue.svg)](https://beatsaber.com/)
[![BSIPA](https://img.shields.io/badge/BSIPA-^4.3.0-orange.svg)](https://github.com/bsmg/BeatSaber-IPA-Reloaded)
[![BeatLeader](https://img.shields.io/badge/BeatLeader-^0.9.33-red.svg)](https://beatleader.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**AutoReplay** 是一个为 **Beat Saber** 开发的开源模组，能够为任意选中的谱面一键生成超拟真、高精度的理论满分（115分全连 / AP）**BeatLeader 回放（`.bsor`）**，并直接在游戏内自动唤起回放器进行观赏。

---

### 📦 前置依赖

运行该 Mod 需要在 Beat Saber 中安装以下依赖：

- **Beat Saber** `1.40.8` (或兼容版本)
- **BSIPA** `^4.3.0`
- **BeatLeader** `^0.9.33`
- **BSML (BeatSaberMarkupLanguage)**

---

### 🚀 安装方法

1. 前往本仓库的 [Releases](../../releases) 页面，下载最新的 `auto_replay.dll`。
2. 将 `auto_replay.dll` 复制到你的 Beat Saber 安装目录下的 `Plugins` 文件夹中：
   ```text
   Beat Saber/Plugins/auto_replay.dll
   ```
3. 启动游戏即可生效。

---

### 🎮 使用说明

1. 启动 Beat Saber，进入单人模式（Solo）。
2. 在歌曲列表中选择你想观赏的任意关卡与难度。
3. （可选）勾选你需要测试的 Modifiers（例如 Faster Song / No Arrows 等）或左手模式。
4. 在歌曲详情面板与排行榜面板之间，你会看到一个绿色的 **Pepe 图标按钮**。
5. 挥动光剑对准并点击 Pepe 按钮：
   - Mod 将在后台高速生成该谱面的 115 满分回放；
   - 随后将自动切换并启动 BeatLeader 回放器，尽情欣赏！

---

### 📂 代码结构

```
AutoPlay/
├── AutoReplayGenerator.cs    # 回放数据生成核心，组装 BeatLeader Replay 对象与 90 FPS 运动帧
├── AutoReplayManager.cs      # 自动化调度中枢，获取选定谱面及修饰符并驱动生成与播放流程
├── AutoReplayMath.cs         # 数学与运动学工具库，包含罗德里格旋转、角度换算与避障判定
├── LeaderboardPatches.cs     # Harmony 补丁类，挂钩选歌界面并在激活时注入 Pepe 按钮
├── NoteEventBuilder.cs       # 音符击打事件构建器，构造满分 115 NoteCutInfo 与 ID 映射
├── NoteSwingPlanner.cs       # 核心挥刀规划器，负责连刀（滑块/叠块）合并判定与挥动关键帧轨迹
├── PepeButtonController.cs   # UI 交互控制器，构建浮动 Pepe 按钮及处理 GIF 下载与点击响应
├── Plugin.cs                 # 插件生命周期入口点，负责 BSIPA 初始化与 Harmony 补丁管理
├── PoseCalculator.cs         # 姿态计算器，计算手腕姿态与障碍墙（下蹲/偏头）避让动作
├── ReplayFileSaver.cs        # 回放文件持久化工具，将回放数据编码为 .bsor 文件并保存到本地
├── auto_replay.csproj        # 项目工程文件与编译引用配置
└── manifest.json             # BSIPA 模组元数据清单与依赖声明
```

---

### ⚖️ 免责声明

- 本模组仅用于**谱面演示、回放效果观赏、算法学习与研究**。
- 请勿将本工具用于任何破坏天梯与多人排位公平性的行为，尊重 BeatLeader 与 Beat Saber 社区规范。

---

### 📄 开源协议

本项目基于 [MIT License](LICENSE) 开源。


