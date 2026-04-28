Claude 项目说明文档
项目概述
这是一个使用Unity开发的2D战棋和回合策略游戏，类似于三国志霸王的大陆FC版。项目主要包含战棋战斗系统和回合制策略玩法。

开发环境
Unity版本: Unity LTS 2022.3.6
开发语言: C#
平台: Windows
项目结构分析（新架构 v2.0）
核心目录
Assets/
├── GameCore/                    ← ⭐纯逻辑（不依赖 Unity）
│   ├── Data/
│   │   ├── Runtime/            ← 运行时数据（GeneralData, CityData, CountryData等）
│   │   └── Definition/         ← 配置数据结构（GameEvent, IItemEffect等）
│   │
│   ├── Domain/                 ← ⭐游戏规则核心
│   │   ├── CityLogic/          ← 城池逻辑
│   │   ├── BattleLogic/        ← 战斗逻辑
│   │   ├── DiplomacyLogic/     ← 外交逻辑
│   │   ├── GeneralLogic/       ← 武将逻辑
│   │   ├── ItemLogic/          ← 道具逻辑
│   │   └── EconomyLogic/       ← 经济逻辑
│   │
│   ├── Commands/               ← 行为系统（ICommand接口 + 具体命令）
│   │
│   ├── AI/                     ← AI决策
│   │
│   ├── Flow/                   ← ⭐流程控制（回合/状态机）
│   │
│   └── Events/                 ← 事件系统
│
├── GamePresentation/            ← ⭐Unity表现层
│   ├── Units/                  ← 单位组件（Troop, Captain, Archer等）
│   ├── UI/                     ← UI组件
│   ├── Input/
│   └── Animation/
│
├── GameInfrastructure/          ← ⭐基础设施
│   ├── Services/               ← 外部服务（ResourceManager, SaveService等）
│   ├── Config/                 ← ScriptableObject
│   └── Persistence/            ← 存档系统
│
└── GameEntry/                   ← ⭐入口
    ├── GameBootstrap.cs
    └── GameMain.cs
核心设计思想
数据（GameState）
  ↓
规则（Domain Logic）
  ↓
行为（Command）
  ↓
流程（Flow）
  ↓
表现（View）

➕

EventBus（解耦所有模块）
命名空间规范
Bwddl.Core.Data.Runtime          // 运行时数据
Bwddl.Core.Data.Definition       // 配置数据定义
Bwddl.Core.Domain.*              // 领域逻辑（CityLogic, GeneralLogic等）
Bwddl.Core.Commands              // 命令
Bwddl.Core.Events                // 事件
Bwddl.Core.Flow                  // 流程控制
Bwddl.Core.AI                    // AI决策
Bwddl.Presentation.Units         // 单位组件
Bwddl.Presentation.UI            // UI组件
Bwddl.Infrastructure.*           // 基础设施
Bwddl.Entry                      // 游戏入口
游戏系统
1. 角色系统
GeneralData: 武将数据类（纯数据）
GeneralLogic: 武将业务逻辑
General: 武将类（包装器，使用GeneralData + GeneralLogic）
Character: 单挑角色类（Unity组件）
兵种类型：步兵(Infantry)、骑兵(Cavalry)、弓箭手(Archer)
2. 战斗系统
回合制战棋战斗
BattleCalculator: 战斗计算器
TroopLogic: 部队业务逻辑
TacticLogic: 战术业务逻辑
PlanLogic: 计谋业务逻辑
CombatEffect: 战斗效果
CombatEffectManager: 战斗效果管理器
3. 城市系统
CityData: 城池数据类（纯数据）
CityLogic: 城池业务逻辑
City: 城池类（包装器，使用CityData + CityLogic）
Commands: 城市操作命令系统
RecruitCommand: 招募命令
ConscriptCommand: 征兵命令
BuildCommand: 建筑命令
InteriorCommand: 内政命令
4. 势力系统
CountryData: 势力数据类（纯数据）
CountryLogic: 势力业务逻辑
Country: 势力类（包装器，使用CountryData + CountryLogic）
5. 大地图战争系统
WarManager: 战争管理器
MapManager: 地图管理器
BattleFlowController: 战斗流程控制器
大地图上的军队移动和战斗
6. 回合系统
TurnFlowController: 回合流程控制器
AITurnService: AI回合服务
PlayerTurnService: 玩家回合服务
7. 事件系统
EventBus: 事件总线
IGameEvent: 事件接口
各种具体事件类（CityUpdatedEvent, BattleStartedEvent等）
8. UI系统
CityPanel: 城池面板
GeneralPanel: 武将面板
CountryPanel: 势力面板
TurnInfoPanel: 回合信息面板
各种信息面板和交互组件
数据管理
DataManager: 数据管理器
TextLibrary: 文本库
CountryListCache: 国家列表缓存
GeneralListCache: 武将列表缓存
CityListCache: 城池列表缓存
资源管理
ResourceManager: 资源管理器
SpriteLoader: 精灵加载器
FontLoader: 字体加载器
AvatarManager: 头像管理器
游戏事件
游戏包含丰富的事件系统：

战斗事件（攻击、胜利、失败）
城市事件（征税、征兵、建设）
特殊事件（干旱、洪水、瘟疫等）
人物事件（招募、结义、婚姻等）
开发注意事项
代码规范
使用中文注释说明代码功能
类名使用PascalCase命名
方法名和变量名使用camelCase命名
重要逻辑需要添加详细注释
Unity组件规范
预制体命名使用PascalCase
场景文件命名使用PascalCase
资源文件按功能分类存放
Git提交规范
提交信息使用中文描述
每个功能点单独提交
提交前确保代码可编译运行
新架构使用规范
新增数据类放在GameCore/Data/Runtime目录
新增业务逻辑放在GameCore/Domain对应目录
新增命令放在GameCore/Commands目录
新增事件放在GameCore/Events目录
新增UI组件放在GamePresentation/UI目录
新增单位组件放在GamePresentation/Units目录
Claude 使用指南
沟通语言
请使用中文进行沟通，这样可以更准确地理解和处理项目相关需求。

常用任务
代码分析: 分析现有代码结构和功能
Bug修复: 定位和修复游戏中的问题
功能开发: 实现新的游戏功能
代码优化: 改善代码性能和可读性
资源管理: 协助处理游戏资源和配置
项目特殊说明
这是一个策略战棋游戏，注重回合制玩法
代码中包含大量中文文本和注释
游戏系统复杂，包含多个相互关联的模块
UI界面需要支持中文显示
优化总结
已完成架构重构，主要成果：

创建了新的分层架构（GameCore, GamePresentation, GameInfrastructure, GameEntry）
实现了数据与逻辑分离（Data层 → Domain层）
统一了命令系统（Commands层）和事件系统架构（Events层）
创建了包装器类保持向后兼容性
优化了命名空间和目录结构
迁移了所有核心类到新架构（General, City, Country, Item, Alliance, Tactic, Plan）
提高了代码的可维护性和可扩展性
开发建议
在修改核心系统前请先分析相关代码
添加新功能时考虑与现有系统的兼容性
注意游戏的平衡性和可玩性
保持代码的模块化和可维护性
遵循现有的架构模式和命名规范
新代码优先使用新架构模式
旧代码可以继续使用包装器类
联系方式
如有问题或需要进一步了解项目，请通过项目Issues或讨论区联系。
