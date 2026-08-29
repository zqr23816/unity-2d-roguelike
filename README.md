# 随机地牢：Unity 2D Roguelike

一个面向游戏开发岗位作品集的原创 Unity 2D Roguelike MVP，使用 Unity 2022.3.62f3c1 开发。

## 已实现

- 基于随机种子的房间与 L 形走廊生成
- 确保所有房间连通的地牢结构
- 玩家八方向移动、近战攻击、受伤与无敌帧
- 敌人游荡、追踪、攻击、受击、死亡有限状态机
- 经验球掉落与自动吸附
- 升级三选一：生命、伤害、移速、攻速、范围、恢复
- 中文 HUD、胜利/失败结算和随机种子重开
- Windows 64 位构建
- 0x72 像素角色逐帧动画：骑士主角、哥布林、兽人战士与大型僵尸
- NPC 基础交互模块预留（当前版本不包含剧情）
- 中文主菜单：开始游戏、储存/读取、退出游戏
- Inspector 数值配置：玩家、普通敌人、Boss 与武器参数集中管理
- 初始劈刀、手持武器表现、关底 Boss 武器掉落与确认替换
- F5 保存角色成长、武器和随机种子，Esc 返回菜单
- 连续闯关：通关后继承当前生命、等级、经验、成长属性与武器
- 敌人使用四方向网格 A* 寻路，并随层数和玩家等级提升生命、攻击与防御
- 武器通过玩家子物体 `Hand Point` 挂载，攻击表现由武器挥砍完成
- 玩家、敌人、经验球和武器掉落均由 Prefab 实例化
- `GameManager` 已拆分为会话、实体工厂、关卡遭遇和成长四个独立系统

## 操作

- `WASD`：移动
- `J` 或鼠标左键：攻击
- `1/2/3`：选择升级
- `R`：胜利或失败后生成新地牢

## 项目结构

- `DungeonGenerator`：程序化地牢生成
- `PlayerController`：玩家移动、战斗、属性成长
- `EnemyController`：敌人有限状态机
- `GameManager`：单局流程、刷怪、升级与结算
- `RunSession`：随机种子、存档和楼层推进
- `EncounterDirector`：敌人波次、经验掉落、Boss 奖励和通关条件
- `RunProgressionSystem`：升级候选生成与属性应用
- `RoguelikeEntityFactory`：统一从 Prefab 创建玩家、敌人和掉落物
- `RoguelikePrefabCatalog`：ScriptableObject 形式的 Prefab 引用目录
- `RoguelikeHUD`：中文界面
- `RoguelikeProjectBuilder`：场景生成和自动构建
- `GameBalanceSettings`：在 Unity Inspector 中调节 HP、速度、攻击距离等数值
- `WeaponController` / `WeaponPickup`：装备显示、武器数值与 Boss 掉落替换

## 在 Unity 中调节数值

点击 `Assets/Resources/Settings/GameBalanceSettings.asset`，在 Inspector 中修改玩家、敌人、Boss 和每种武器的参数。`Main` 场景在编辑状态下保持精简是正常现象：地牢、玩家和敌人会在点击 Play 后根据随机种子生成。

如果菜单、Prefab 或配置资产不存在，使用 Unity 顶部菜单 `Roguelike > 生成菜单、Prefab 与主场景` 重新生成。生成后的实体资源位于 `Assets/Prefabs`，引用目录位于 `Assets/Resources/Settings/RoguelikePrefabCatalog.asset`。

## 美术素材与署名

角色与敌人素材使用 0x72 创作的 **Dungeon Tileset II**，依据 CC0 1.0 许可使用。CC0 不强制署名，但本项目主动感谢并标注原作者：

- 作者：0x72
- 素材主页：https://0x72.itch.io/dungeontileset-ii
- 许可：https://creativecommons.org/publicdomain/zero/1.0/

完整第三方素材说明见 `THIRD_PARTY_NOTICES.md`。森林角色与 Fantasy RPG NPC 素材包本轮未接入战斗场景；NPC 代码模块已预留，但没有设计剧情。

本机 Windows 黑体仅用于本地开发验证，已从公开仓库排除。克隆项目后如未配置中文字体，IMGUI 会回退到 Unity 默认字体；正式发布版本应接入允许再分发的开源中文字体。
