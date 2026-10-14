# CRAZE

> 一个基于 Unity 的第三人称 3D 动作冒险 Demo，角色控制基于 Invector Third Person Controller。

在线试玩：[unregisterd.github.io/CRAZE](https://unregisterd.github.io/CRAZE/)（WebGL，浏览器直接运行）

## 简介

CRAZE 是一个小型 3D 动作冒险项目，包含完整的**任务、对话、存档、交互**等系统。项目重点在于**系统设计与代码架构**，而非美术表现，适合作为 Unity 客户端方向的求职 Demo。

## 环境要求

| 项目 | 版本 |
|------|------|
| Unity | 2022 LTS |
| 渲染管线 | URP |
| 角色控制器 | Invector Third Person Controller v2.6.5 |
| 对话 UI | TextMeshPro |

## 功能特性

### 事件系统
- 泛型事件中心 `EventManager<T>`，类型安全、无装箱
- `EventRegistry` 统一管理所有泛型实例的清理
- `DebugPrintListeners` 调试工具，可查看某事件的所有监听者
- 按需监听，注册 / 注销成对出现

### 交互系统
- `IInteractable` 接口定义契约
- `InteractableBase` 抽象基类，提供公共实现
- `PlayerInteractor` 使用 `Physics.OverlapSphere` 检测最近的可交互物体
- 支持多种交互物：NPC、猫、宝箱、拾取物

### 对话系统
- JSON 数据驱动，改台词不改代码
- 支持线性对话、选项分支、条件节点
- 对话中锁定玩家、禁用交互
- 对话开始 / 结束广播事件

### 任务系统
- 目标制设计：任务由一组 `QuestObjective` 组成
- 支持线性（带前置依赖）与网状（无前置）任务
- 三种目标类型：`Dialogue`（对话触发）、`Collect`（背包查询）、`Interact`（交互触发）
- 任务级前置、目标级前置
- HUD 任务追踪器 + 任务面板
- NPC 头顶任务图标（可接 / 进行中 / 可交付）
- 基于 `ScriptableObject` 的任务配置表

### 存档系统
- JSON 多槽位存档
- `ISaveable` 接口驱动，扩展内容不改 `SaveManager`
- 删除二次确认
- 异步加载界面
- 主菜单读档 + 跨场景传参（`PendingLoadFlag`）

### 其他
- **对象池**：`ObjectPool` + `PoolManager` + `PooledObject`
- **背包数据层**：`ItemInventory`，实现 `ISaveable`

## 项目结构

```text
Assets/
├── Invector-3rdPersonController/    (第三方，未修改)
└── _MyProject/
    ├── Scripts/
    │   ├── Core/           (事件系统、UIState)
    │   ├── Interaction/    (交互系统)
    │   ├── Dialogue/       (对话系统)
    │   ├── Quest/          (任务系统)
    │   ├── Inventory/      (背包数据层)
    │   ├── Save/           (存档系统)
    │   ├── Pooling/        (对象池)
    │   ├── UI/             (所有 UI)
    │   └── NPC/            (NPC 行为)
    ├── Quests/             (QuestDefinition asset)
    ├── Prefabs/
    └── Resources/
        └── Dialogues/      (对话 JSON)
```

## 架构设计

### 事件驱动

所有系统通过 `EventManager` 通信，不直接相互引用：

```text
游戏逻辑 → 广播事件 → 事件中心 → 分发 → 订阅者
```

例如拾取物品：

```text
PickupItem.Interact()
  → ItemInventory.Add("泳圈")
    → 广播 ItemAdded
      → QuestManager 监听 → 推进 Collect 目标
      → HUD 监听 → 刷新显示
```

### 数据与逻辑分离

| 数据层 | 逻辑层 | 表现层 |
|--------|--------|--------|
| SaveData / QuestDefinition / DialogueData / ItemInventory | SaveManager / QuestManager / DialogueManager | UI 面板 / 动画 |

### 接口驱动
- `IInteractable`：任何可交互物体实现它
- `ISaveable`：任何需要存读的对象实现它
- 添加新内容不需要修改核心管理器

## 快速上手

**1. 打开项目**
使用 Unity 2022 LTS 打开，等待编译完成。

**2. 打开主菜单场景**
`Assets/_MyProject/Scenes/MainMenu.unity`

**3. 运行**
- 点击「新游戏」进入 Demo 场景
- 点击「继续」从存档槽读取

**4. 操作**

| 按键 | 功能 |
|------|------|
| `WASD` | 移动 |
| `Shift` | 奔跑 |
| `Space` | 跳跃 |
| `F` | 交互 |
| `Esc` | 系统菜单 |
| `J` | 任务面板 |
| `B` | 背包（UI 未实现） |

## 关键系统讲解

### 任务系统：从步骤到目标

- **旧设计**：任务是一串有序的步骤，进度存"第几步"。
- **新设计**：任务是一组目标，每个目标独立记录进度。

| 概念 | 作用 |
|------|------|
| `QuestObjective` | 一件可独立完成的事 |
| `prerequisiteObjectiveIds` | 目标的前置依赖，形成线性 |
| `requiredCount` | 需要几次 |
| `required` | 是否必须完成 |

线性任务 = 每个目标都有前置，形成链；网状任务 = 目标无前置，任意顺序。

三种目标类型：
- `Dialogue`：和 NPC 对话触发
- `Collect`：实时查背包（物品丢失会回退进度）
- `Interact`：和物体交互触发

### 存档系统：接口驱动

```csharp
public interface ISaveable
{
    void OnSave(SaveData data);
    void OnLoad(SaveData data);
}
```

`SaveManager` 维护所有 `ISaveable`，统一收集 / 分发数据。

跨场景读档使用 `PendingLoadFlag`：
- 主菜单只记录要读的槽位
- 游戏场景加载完成后由 `SceneLoadHandler` 执行读档

### 对话系统：数据驱动

对话使用 JSON 配置：

```json
{
    "npcName": "玛丽",
    "startNodeId": "intro",
    "nodes": [
        {
            "id": "intro",
            "text": "我的三只小猫跑丢了，你能帮我找找吗？",
            "options": [
                {
                    "text": "我帮你找",
                    "next": "accept",
                    "action": "AcceptQuest",
                    "actionParam": "FindCatQuest"
                },
                {
                    "text": "我现在没空",
                    "next": "decline"
                }
            ]
        }
    ]
}
```

选项可以携带 `action`，通过 `DialogueActionHandler` 分发到任务、背包等系统。

## 技术亮点
- **泛型事件中心**：类型安全、避免装箱，`EventRegistry` 处理泛型实例清理
- **目标制任务系统**：同一套结构支持线性与网状任务，加新任务不改代码
- **接口驱动存档**：`ISaveable` 让存档内容可扩展，`SaveManager` 零改动
- **数据驱动对话**：JSON 配置，改台词不改代码
- **事件解耦**：系统间零直接引用，通过事件系统通信

## 已知限制
- 背包 UI 未实现（仅有数据层）
- 存档缩略图未实现
- 暂未支持多语言

## 可拓展方向
- 背包 UI + 对象池格子
- `ItemDatabase`（物品定义表）
- 存档缩略图 + 加密

## 第三方资源
- Invector Third Person Controller
- Stylized3D 场景包
- 其他美术资源

## 作者
高雁晰

## 许可
仅用于个人求职展示，第三方资源版权归原作者所有。
