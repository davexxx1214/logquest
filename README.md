# LogQuest 放置冒险原型

放置类冒险游戏。当前版本是一个纯静态本地网页原型，不需要构建工具。

## 运行

直接用浏览器打开 `index.html` 即可游玩。

Godot 迁移版位于 `godot/`。当前目标是 Godot 4.6.2 .NET 的 Windows 桌面端，不做 Web 导出。

```powershell
cd godot
dotnet build .\LogQuest.csproj
```

也可以用 Godot 4.6.2 .NET 版打开 `godot/project.godot`。

## 当前内容

- 6 个地点：晨曦草原、旧矿山、沉没神殿、黑森林、魔法塔、边境要塞
- 6 个职业：战士、法师、僧侣、猎人、骑士、德鲁伊
- 最多 3 人出击
- 最多 2 支小队同时探索，忙碌英雄不能重复派遣
- 每个地点 1 个固定层数迷宫
- 探索画面：横版行走、随机事件、自动战斗、伤害数字和血条
- 英雄动画使用朝右的 `heroes-16bit-v2.png`，怪物战斗动画使用 `monsters-16bit-v2.png`
- 自动战斗、只读日志、现实时间结算
- 成功/失败、经验、升级、随机装备掉落
- 装备词缀、自动换装、职业技能解锁
- 地图进度、职业解锁、地区解锁、迷宫层数推进
- 快速模拟 100 场，用于测试当前队伍与层数强度

## Godot 迁移进度

- 已创建 `godot/project.godot` 与 Godot .NET C# 项目
- 已迁移网页原型的职业、技能、地区、迷宫、怪物、装备、随机事件配置到 `godot/Data/*.json`
- 已迁移核心 tile sheet 到 `godot/Assets/`
- 已实现第一版 Godot 主界面骨架：大地图、出击计划、探索画面、英雄队伍、怪物图鉴、装备面板
- 已接入 Godot 运行时状态、`user://logquest-save.json` 本地存档、英雄选择、最多两支小队出击、探索倒计时、自动战斗模拟和结算解锁
- 下一步：把探索画面拆成独立场景，补齐攻击动画、血条插值、伤害飘字和装备自动换装

## 调数值入口

主要数据都在 `app.js` 顶部：

- `classData`：职业基础属性、成长、定位
- `classSkills`：职业技能、解锁等级、战斗触发点
- `regions`：地点、迷宫、层数、掉落表
- `regions[].rewards`：完成指定层数后解锁职业、地点或路线事件
- `monsters`：怪物种类与基础属性
- `itemCatalog` / `affixPool`：装备槽位、基础属性、随机词缀
- `explorationEvents`：探索过程中的随机事件和事件奖励
- `createExplorationPlan`：生成行走、事件、战斗的探索时间轴
- `simulateBattle`：自动战斗主循环

## 当前推进线

新存档从战士、僧侣和晨曦草原开始。

- 晨曦草原：解锁猎人、旧矿山
- 旧矿山：解锁骑士、沉没神殿
- 沉没神殿：解锁法师、黑森林
- 黑森林：解锁德鲁伊、魔法塔
- 魔法塔：解锁边境要塞
- 边境要塞：当前中期目标

## 美术资产接入

见 `assets/README.md`。当前版本已经接入 GPT Image 2 生成的 16-bit tile sheet，包括英雄、怪物和装备图标。
