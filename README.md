# TwoPeople — 双人协作 2D PVE 游戏

> 答辩项目 · Unity + Photon 双人联机闯关游戏

## 🎮 项目简介

TwoPeople 是一款 **2D 横版双人协作 PVE 动作游戏**。两名玩家通过网络联机，共同对抗波次进攻的敌人，需要双方配合使用技能与走位来完成关卡挑战。

## 🎬 演示视频

[观看演示视频](https://pan.quark.cn/s/0141e7356b4c)

## 🏗️ 技术架构

| 层级 | 技术 |
|------|------|
| 游戏引擎 | Unity（2D） |
| 网络框架 | Photon PUN 2 + 自建 Photon Socket Server |
| 服务端 | C# Photon Socket Server（ApplicationBase） |
| 数据持久化 | NHibernate + MySQL |
| 资源热更新 | XLua + MD5 校验实现图片替换 |
| 通信协议 | 自定义 OperationCode / EventCode / ParameterCode |

## 📁 项目结构

```
TwoPeople/
├── MyServerDemo/
│   ├── 2DPVEDemo/               # Unity 客户端项目
│   │   └── Assets/
│   │       ├── Scripts/          # 游戏逻辑脚本
│   │       │   ├── GameManager.cs         # 游戏主控（GameOver、双方结算检测）
│   │       │   ├── PlayerManager.cs       # 玩家控制（移动/攻击/技能/同步）
│   │       │   ├── PunTwoManager.cs       # Photon 网络配置
│   │       │   ├── MapManager.cs          # 地图/关卡选择与结算
│   │       │   ├── LocalUserData.cs       # 本地用户数据
│   │       │   ├── CameraManager.cs       # 相机管理
│   │       │   ├── RemotePlayer.cs        # 远端玩家
│   │       │   ├── PlayerItem.cs          # 玩家条目
│   │       │   ├── Projectile.cs          # 弹射物
│   │       │   ├── SpriteAnimEffect.cs    # 精灵动画特效
│   │       │   ├── Chat/                  # 聊天系统
│   │       │   │   ├── ChatManager.cs     # 聊天管理器
│   │       │   │   ├── ChatMessageItem.cs # 聊天消息条目
│   │       │   │   ├── ChatPanel.cs       # 聊天面板基类
│   │       │   │   ├── PublicChatPanel.cs # 公频面板
│   │       │   │   └── PrivateChatPanel.cs# 私聊面板
│   │       │   ├── EnemyScripts/          # 敌人脚本
│   │       │   │   ├── Enemy.cs           # 敌人基类（FSM 状态机）
│   │       │   │   ├── SwordEnemy.cs      # 剑士敌人
│   │       │   │   ├── ShieldEnemy.cs     # 盾兵敌人
│   │       │   │   ├── HalberdEnemy.cs    # 戟兵敌人
│   │       │   │   └── CrossbowlEnemy.cs  # 弩兵敌人
│   │       │   ├── Wave/                  # 波次系统
│   │       │   │   ├── WaveManager.cs     # 波次管理器
│   │       │   │   ├── WaveSO.cs          # 波次配置（ScriptableObject）
│   │       │   │   ├── WaveTriggerData.cs # 波次触发数据
│   │       │   │   └── ExitGameTrigger.cs # 退出触发器
│   │       │   └── Ui/                    # UI 系统
│   │       │       ├── UiManager.cs       # UI 管理器
│   │       │       ├── LoginCanvas.cs     # 登录/注册界面
│   │       │       ├── UserDataPanel.cs   # 用户数据面板
│   │       │       ├── RoomUI.cs          # 房间界面
│   │       │       ├── CreatRoom.cs       # 创建房间
│   │       │       ├── InRoom.cs          # 房间内界面
│   │       │       ├── GameOverPanel.cs   # 游戏结束面板
│   │       │       ├── HpBar.cs           # 血条 UI
│   │       │       ├── SkillCdUI.cs       # 技能冷却 UI
│   │       │       ├── PulseEffect.cs     # 脉冲特效
│   │       │       └── TypewriterEffect.cs# 打字机效果
│   │       └── Photon/            # Photon SDK
│   │
│   └── 2dPveDemoSever/            # 服务端项目
│       ├── 2dPveDemoSever/
│       │   ├── MyServer.cs         # 服务器入口（ApplicationBase）
│       │   ├── MyClient.cs         # 客户端连接处理
│       │   ├── GameController.cs   # 游戏消息中继（位置/攻击/技能/波次）
│       │   ├── ChatController.cs   # 聊天消息处理（公频/私聊）
│       │   └── DOA/                # 数据访问层
│       │       ├── GameData.cs     # 游戏数据实体
│       │       ├── DataManager.cs  # 数据管理器
│       │       └── MyServer.DOA.cs # DOA 服务
│       └── MyCommon/               # 公共协议库
│           ├── OperationCode.cs    # 操作码枚举
│           ├── EventCode.cs        # 事件码枚举
│           ├── ParameterCode.cs    # 参数码枚举
│           └── ReturnCode.cs       # 返回码枚举
│
└── Video/
    └── 2d.mp4                      # 演示视频
```

## 🎯 核心功能

### 👤 用户系统
- 登录 / 注册（手机号 + 密码 + 用户名）
- 用户数据持久化（MySQL）
- 改名 / 注销账户

### 🌐 联机对战
- Photon 自建服务器，支持双人房间
- **Master Client 权威模式**：房主控制敌人 AI 与波次
- 所有游戏操作通过自定义 Operation/Event 协议同步

### 🔥 资源热更新（XLua + MD5）
- **XLua 脚本驱动**：通过 XLua 在运行时动态加载 Lua 脚本，实现不重新打包即可更新游戏逻辑和资源
- **MD5 校验**：服务端存储资源文件的 MD5 码，客户端启动时对比本地 MD5，判断是否需要拉取新资源
- **图片热替换**：通过 MD5 校验发现变更的图片资源后，从服务器下载并替换本地资源，无需重新安装

### ⚔️ 战斗系统
- **普通攻击**：三段连击（Combo 机制 + 预输入缓冲）
- **技能系统**：
  | 按键 | 技能 | 冷却 |
  |------|------|------|
  | U | AOE 范围攻击 | 3s |
  | I | 远程弹射物 | 4s |
  | O | 自我治疗 (+30HP) | 5s |
  | L | 终极技能（全屏） | 60s |

### 🎯 敌人系统
- 4 种敌人类型：**剑士、盾兵、戟兵、弩兵**
- 有限状态机 AI（Idle → Chase → Attack → Die）
- 自动寻敌（追逐最近的玩家）
- 敌人状态通过自建服务器同步

### 🌊 波次系统
- ScriptableObject 配置（WaveSO）：敌人种类、数量、生成点
- 逐波推进，清完一波自动开启下一波
- 每波清除墙壁障碍推进场景

### 💬 聊天系统
- **公频聊天**：所有在线玩家可见
- **私聊**：指定玩家发送
- 在线用户列表实时刷新

### 📊 结算系统
- 记录通关结果、用时、得分
- 双方完成 → 自动展示对比结算面板
- 数据写入 MySQL 数据库

## 🎮 操作说明

| 按键 | 操作 |
|------|------|
| A/D 或 ← → | 左右移动 |
| Space / K | 跳跃 |
| J | 普通攻击（三段连击） |
| U | AOE 范围攻击 |
| I | 远程弹射物 |
| O | 自我治疗 |
| L | 终极技能 |

## 🚀 运行方式

### 服务端
1. 打开 `MyServerDemo/2dPveDemoSever/2dPveDemoSever.sln`
2. 配置 `hibernate.cfg.xml` 中的 MySQL 连接字符串
3. 启动 `2dPveDemoSever`（Photon Socket Server）

### 客户端
1. 使用 Unity 打开 `MyServerDemo/2DPVEDemo/`
2. 配置 Photon Server 地址指向自建服务器
3. 运行 `SampleScene` 场景
4. 登录 → 创建/加入房间 → 开始游戏

### 双人测试
1. 启动服务端
2. 打开两个 Unity 客户端（或 Build 后双开）
3. 分别登录不同账号
4. 一方创建房间，另一方加入
5. 房主选择关卡开始游戏

## 📦 依赖

- Unity 版本：见 `ProjectSettings/ProjectVersion.txt`
- [Photon PUN 2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922)
- [Photon Socket Server SDK](https://www.photonengine.com/en-us/server-sdks)
- [XLua](https://github.com/Tencent/xLua)
- NHibernate
- MySQL
