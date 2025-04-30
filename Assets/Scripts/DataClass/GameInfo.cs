using System.Collections.Generic;
using BaseClass;
using UnityEngine;

namespace DataClass
{
    public static class GameInfo
    {
        // 游戏月份
        public static byte month = 1;
        // 游戏年份
        public static short years = 251;
        // 前回合势力ID
        public static int curTurnIndex;
        // 现在回合势力ID
        public static byte curTurnCountryId;
        // 现在回合势力命令数量
        public static byte curTurnCountryOrderNum;
        // 玩家势力ID
        public static byte playerCountryId;
        // 玩家势力命令数量
        public static byte playerOrderNum;
        // 游戏难度
        public static byte difficult;
        // 记录信息数组
        public static string[] recordInfo = new string[4];
        // 最大保留列表
        public static List<General> customGeneralList = new List<General>();
        // 选择的武将名称
        public static string chooseGeneralName;
        // 选择的剧情
        public static string chapter;
        // 势力灭亡提示
        public static byte countryDieTips;
        // 是否观看模式
        public static bool isWatch = false;
        // 显示的信息
        public static string ShowInfo;

        public static byte targetCountryId;
        public static byte attackCount;
        public static byte doCityId;


        public static byte targetCityId;
        public static GameState PlayingState;
        public static TaskType Task;
        public static List<short> targetGeneralIds= new List<short>();
        public static List<short> optionalGeneralIds = new List<short>(); // 可选的将领ID列表
        public static List<short> doGeneralIds = new List<short>();   // 选中的将领ID列表

        /// <summary>
        /// 获取玩家命令数量
        /// </summary>
        /// <returns></returns>
        public static byte GetPlayerOrderNum()
        {
            // 获取人类势力拥有的城市数量
            byte cityNum = CountryListCache.GetCountryByCountryId(playerCountryId).GetHaveCityNum();
        
            // 根据城市数量调整命令数量
            if (cityNum < 3)
            {
                cityNum = 3;
            }
            else if (cityNum >= 3 && cityNum <= 4)
            {
                cityNum = 4;
            }
            else if (cityNum >= 5 && cityNum <= 7)
            {
                cityNum = 6;
            }
            else if (cityNum >= 8 && cityNum <= 10)
            {
                cityNum = 7;
            }
            else if (cityNum >= 11)
            {
                cityNum = 8;
            }

            return cityNum;
        }
    
        /// <summary>
        /// 减少玩家命令，如果减少到0则返回true表示回合结束
        /// </summary>
        /// <returns>如果玩家命令数为0，则返回true；否则返回false</returns>
        public static bool SubPlayerOrder()
        {
            // 检查当前playerOrderNum是否在有效范围内（虽然初始化为8，但这里做范围检查是个好习惯）
            if (playerOrderNum < 0 || playerOrderNum > 8)
            {
                Debug.LogError("玩家命令数超出有效范围:" + playerOrderNum);
                playerOrderNum = 0; // 重置为0
                return false; // 或者抛出异常等
            }

            // 如果playerOrderNum大于0，则减1
            if (playerOrderNum > 0)
            {
                playerOrderNum--;
                Debug.Log($"减少后玩家命令数量：{playerOrderNum}");

                // 如果减少到0，则返回true表示回合结束
                if (playerOrderNum == 0)
                {
                    Debug.Log("玩家命令用尽，回合结束");
                    return true;
                }
            }

            // 默认情况下返回false，表示回合未结束
            return false;
        }

        public static void SetGeneralOption(IEnumerable<short> generalIds)
        {
            optionalGeneralIds.Clear();
            optionalGeneralIds.AddRange(generalIds);
        }

        public static void SetTargetGeneral(IEnumerable<short> generalIds)
        {
            targetGeneralIds.Clear();
            targetGeneralIds.AddRange(generalIds);
        }

        public static void SetTargetGeneral(short generalId)
        {
            targetGeneralIds.Clear();
            targetGeneralIds.Add(generalId);
        }
    



  

 

    }

    public enum GameState
    {
        None,
        GameStart,
        Playing,
        PlayerTurn,
        AITurn,
        GameOver,
        GameWin,
        
        Inherit,
        Truce,
        Bribe,

        PlayervsAI,
        AIvsPlayer,
        Occupy,
        AIAttack,
        AIWin,
        AILose,
        
        Tax,
        Harvest,
        Rebel,

       
        Drought,
        Flood,
        Plague,
        LocustPlague,
        Turmoil,
        Plunder,

        /*Famine,
        Tsunami,
        Earthquake,
        Meteor,
        Snow,
        Storm*/
    }

    public enum TaskType
    {
        None,           // 无
        Move,           // 移动
        MoveDeny,       // 移动禁止
        OverMove,       // 移动过多
        MovePrefect,    // 移动太守
        Attack,         // 攻城
        AttackDeny,     // 不可攻城
        OverAttack,     // 攻城过多
        Conscript,      // 征兵
        Assign,         // 分配
        Transport,      // 输送
        TransportDeny,  // 无法输送
        End,            // 结束
        Search,         // 搜索
        SearchMoney,    // 搜索金钱
        SearchFood,     // 搜索粮食
        SearchTreasure, // 搜索宝物
        SearchGeneral,  // 搜索武将
        SearchNothing,  // 搜索失败
        Employ,         // 登用
        EmploySuccess,  // 登用成功
        EmployDeny,     // 登用失败
        EmployNothing,  // 野无遗贤
        OverEmploy,     // 置吏已满
        Reward,         // 奖赏
        RewardDeny,     // 奖赏失败
        Appoint,        // 任命
        AppointDeny,    // 任命失败
        Lack,           // 缺钱 
        Reclaim,        // 开垦
        OverReclaim,    // 农业饱和
        Mercantile,     // 劝商
        OverMercantile, // 商业饱和
        Tame,           // 治水
        OverTame,       // 治水饱和
        Patrol,         // 巡查
        OverPatrol,     // 巡查饱和
        Truce,          // 停战
        OverTruce,      // 无敌
        TruceSelect,    // 议和选择
        Alienate,       // 离间
        Intelligence,   // 情报
        Bribe,          // 招揽
        Shop,           // 金粮
        LackShop,       // 缺少粮店
        Smithy,         // 武器
        LackSmithy,     // 缺少工匠
        School,         // 学院
        LackSchool,     // 缺少书院
        SchoolDeny,     // 书院否决
        Hospital,       // 医馆
        LackHospital,   // 缺少医馆
        HospitalDeny,   // 医馆否决
        Inherit,        // 选择继承
        SelfBuild,      // 自建势力
        SelfRemove,     // 自建武将移除
        Save,           // 存储
        Load,           // 读取
        Settings,       // 设置
        Exit,           // 退出      

    }
}