# AutoPlay

[![Game Version](https://img.shields.io/badge/Beat%20Saber-1.40.8-blue.svg)](https://beatsaber.com/)
[![BSIPA](https://img.shields.io/badge/BSIPA-^4.3.0-orange.svg)](https://github.com/bsmg/BeatSaber-IPA-Reloaded)
[![BeatLeader](https://img.shields.io/badge/BeatLeader-^0.9.33-red.svg)](https://beatleader.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**AutoReplay** 是一个为 **Beat Saber** 开发的开源模组，能够为任意选中的谱面一键生成超拟真、**BeatLeader 回放（`.bsor`）**，并直接在游戏内自动唤起回放器进行观赏。

---

### 📕 未来发展
这个项目未来没有维护计划

目前的已知问题
- 躲炸弹效果不是很好

如果你有任何问题想马上联系作者 https://t.me/haha_2ha

想要什么模组都可以告诉我，如果我想做的话我就会做

我也欢迎各种形式的打赏

---

### 🚀 安装方法

1. 前往本仓库的 [Releases](../../releases) 页面，下载最新的 `auto_play.dll`。
2. 将 `auto_play.dll` 复制到你的 Beat Saber 安装目录下的 `Plugins` 文件夹中：
   ```text
   你的BeatSaber路径/Plugins/auto_play.dll
   ```
   如果你不知道的话可以去 steam 按 “浏览本地目录” 查看
3. 启动游戏即可生效。

---

### 🎮 使用说明

1. 启动 Beat Saber，进入Solo模式
2. 选择你想要的曲子和难度，你也可以加选Mod和左手模式，身高永远150厘米不可以更改
3. 你会在歌单和排行榜中间看到一个PEPE蛙按钮，按一下就会生成对应的 .bsor 回放
4. 接着模组会自动使用Beatleader的回放播放器播放生成的回放
5. 好好欣赏

---

### 📂 代码结构

```
AutoPlay/
├── auto_replay.csproj        # 项目文件
├── AutoReplayGenerator.cs    # 回放数据生成核心
├── AutoReplayManager.cs      # 自动化调度中枢，获取选定谱面及修饰符并驱动生成与播放流程
├── AutoReplayMath.cs         # 数学与运动学工具库
├── LeaderboardPatches.cs     # 按钮控件
├── NoteEventBuilder.cs       # 音符击打事件构建器
├── NoteSwingPlanner.cs       # 剑挥砍的算法，几乎都完美了
├── PepeButtonController.cs   # PEPE蛙按钮
├── Plugin.cs                 # 负责 BSIPA 初始化与 Harmony 补丁管理
├── PoseCalculator.cs         # 躲掩体的控件，其中包含头显蹲下和探头
├── ReplayFileSaver.cs        # 保存回放至本地
└── manifest.json             # BSIPA 模组元数据清单与依赖声明
```

---

### ⚖️ 免责声明

- 本模组仅用于**谱面演示、回放效果观赏、算法学习与研究**。
- 请勿将本工具用于任何破坏天梯与多人排位公平性的行为，尊重 BeatLeader 与 BeatSaber 社区规范。

---

### 📄 开源协议

本项目基于 [MIT License](LICENSE) 开源。


