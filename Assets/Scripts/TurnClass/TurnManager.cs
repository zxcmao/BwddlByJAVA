using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DataClass.GameInfo;
using Random = UnityEngine.Random;

namespace TurnClass
{

    public class TurnManager : MonoBehaviour
    {
        // 私有静态实例，用于实现单例模式
        private static TurnManager _instance;

        // 私有构造函数，防止外部实例化
        private TurnManager()
        {
            // 可以在这里进行必要的初始化
        }

        // 公共静态方法，用于获取类的唯一实例
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<TurnManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("TurnManager为空!");
                    }
                }
                return _instance;
            }
        }
        
        
        private byte _disasterCount;
        private List<byte> _disasterCity;
        public UIGlobe uiGlobe;
        public AITurnStateMachine.AITurnStateMachine aiTurnStateMachine;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this; // 确保在 Awake 中完成初始化
                DontDestroyOnLoad(gameObject); // 回合管理器在场景切换时不被销毁
            }
            else
            {
                Destroy(gameObject); // 防止多个实例
            }
        }
        
        /*void Start()
        {
            if (Task == TaskType.SelfBuild) return;
            if (PlayingState == GameState.PlayerTurn)//处于玩家操作回合
            {
                if (playerOrderNum == 0)
                {
                    Debug.Log("玩家命令用尽");
                    PlayingState = GameState.AITurn;
                    StartMonth();
                }
            }
            else if (PlayingState == GameState.AIvsPlayer || PlayingState == GameState.PlayervsAI)//战争后返回
            {
                StartCoroutine(BackFromWar());
            }
            else
            {
                GameStart();
            }
        }*/

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (SceneManager.GetActiveScene().name == "GlobalScene")
            {
                uiGlobe = GameObject.Find("Canvas").GetComponent<UIGlobe>();
               
                if (Task == TaskType.SelfBuild) return;
                if (PlayingState == GameState.GameStart)
                {
                    GameStart();
                }
                else if (PlayingState == GameState.PlayerTurn)//处于玩家操作回合
                {
                    if (playerOrderNum == 0)
                    {
                        Debug.Log("玩家命令用尽");
                        PlayingState = GameState.AITurn;
                        //StartCoroutine(ExecuteMouth());
                    }
                }
                else if (PlayingState == GameState.AITurn)//处于玩家操作回合
                {
                    if (aiTurnStateMachine.IsFinished)
                    {
                        Debug.Log("AI命令用尽");
                        PlayingState = GameState.PlayerTurn;
                        //StartCoroutine(ExecuteMouth());
                    }
                }
                else if (PlayingState == GameState.AIvsPlayer)//AI发起战争后返回
                {
                    StartCoroutine(BackFromWar());
                }
                else if (PlayingState == GameState.PlayervsAI)//玩家发起战争后返回
                {
                    SubPlayerOrder();
                    StartCoroutine(BackFromWar());
                }
                
            }
        }

        public void GameStart()
        {
            ReadMapData();
            curTurnIndex = -1;  // 初始化变量
            curTurnCountryId = 0;
            PlayingState = GameState.Playing;
            CountryListCache.TurnSort();  // 执行国家顺序排序
            Debug.Log(String.Join(", ", CountryListCache.countrySequence));

            StartTurn();
        }
        public static void ReadMapData()
        {
            foreach (var cityID in CityListCache.cityDictionary.Keys)
            {
                Instance.StartCoroutine(DataManagement.LoadMapAsync(cityID, (map) =>
                {
                    // 读取地图数据
                    if (map != null)
                    {
                        Debug.Log(cityID +"地图加载成功！");
                        // 使用 warMap 进行逻辑处理
                        DataManagement.maps.TryAdd(cityID, map);
                    }
                    else
                    {
                        Debug.LogError(cityID +"地图加载失败！");
                    }
                }));
            }
        }

        void StartTurn()
        {
            if (PlayingState == GameState.GameOver)
            {
                Debug.Log("游戏失败，退出");
                return ;
            }
            if (PlayingState == GameState.Playing)
            {
                Debug.Log("开始新回合");
                curTurnCountryId = CountryListCache.GetCurrentExecutionCountryId();
                Debug.Log($"获取当前执行国家ID: {curTurnCountryId},下一个序号为{curTurnIndex}");

                attackCount = 0;  // 重置计数器
                Debug.Log("重置攻击计数器");
            }
            StartCoroutine(ExecuteMouth());
        }

        IEnumerator BackFromWar()
        {
            if (countryDieTips != 0)
            {
                if (countryDieTips == 1)
                {
                    yield return uiGlobe.tips.ShowTurnTips(ShowInfo, GameState.AIFail);  // 切换AI势力灭亡状态并处理
                }
                else if (countryDieTips == 2)
                {
                    Task = TaskType.Inherit;  // 调用继承逻辑
                    uiGlobe.UpdateTurnInfo("请选择继任者所在的城池");
                    yield break;
                }
                else if (countryDieTips == 3)
                {
                    playerOrderNum = 1;  // 设置用户指令
                    yield return uiGlobe.tips.ShowTurnTips(ShowInfo, GameState.GameOver);  // 切换玩家势力灭亡状态
                }
                else if (countryDieTips == 4)
                {
                    yield return uiGlobe.tips.ShowTurnTips(ShowInfo, GameState.AIInherit);  // 切换AI继位状态
                }
                else
                {
                    yield return uiGlobe.tips.ShowTurnTips(ShowInfo, GameState.PlayerInherit); // 玩家继承完成
                }
                countryDieTips = 0;  // 重置国家灭亡提示
            }

            if (playerOrderNum == 0)
            {
                PlayingState = GameState.Playing;
            }
           
            yield return ExecuteMouth();
        }
        
        IEnumerator ExecuteMouth()
        {
            //根据势力ID开始执行回合

            if (curTurnCountryId == playerCountryId)
            {
                yield return HandlePlayerTurn();  // 处理玩家回合
                if (PlayingState == GameState.GameOver)
                {
                    Debug.Log("游戏失败，退出");
                    yield break;  // 如果游戏失败，结束方法
                }
            }
            else
            {
                yield return HandleAITurn(curTurnCountryId);  // 处理AI回合
            }

            if (CountryListCache.countrySequence[^1] == curTurnCountryId)
            {
                yield return MonthlyEvent();
                EndMonth();
            }
            
            StartTurn();
        }
        
        
        // 增加月份的方法
        void EndMonth()
        {
            month++; // 增加月份
            if (month > 12) // 如果月份超过12
            {
                month = 1; // 重置月份为1
                years++; // 增加年份
                GeneralListCache.DebutByYears(years);// 更新武将出道
            }
            uiGlobe.UpdateTimeTitle();  // 更新时间标题

            if (isWatch)
                TestGeneral();
        }
        

        private IEnumerator HandlePlayerTurn()
        {
            Debug.Log("玩家回合开始");
            // 用户操作代码，例如 userDoSomething()
            Country playerCountry = CountryListCache.GetCountryByCountryId(playerCountryId);

            if (isWatch)  // 如果是观察状态，直接返回
                yield break;

            if (PlayingState == GameState.Playing)  // 初始状态
            {
                playerOrderNum = GetPlayerOrderNum();  // 获取用户指令编号
                doCityId = playerCountry.FindKingCity();  // 执行某个操作
                SceneManager.LoadScene("CityScene");
                PlayingState = GameState.PlayerTurn;
            }
            else if (PlayingState == GameState.PlayervsAI)
            {
                //ToDo
                //战争打AI
                CountryDieAfterWar();  // 执行战斗后的结算
                
            }
            
            yield return new WaitUntil(() => PlayingState == GameState.AITurn);
            yield return new WaitForSeconds(2f);  // 等待玩家操作
            PlayingState = GameState.Playing;  // 重置状态
        }
        
        private IEnumerator HandleAITurn(byte countryId)
        {
            uiGlobe.UpdateTurnInfo(CountryListCache.GetCountryByCountryId(countryId).KingName() + " 战略中...");
            yield return new WaitForSeconds(1f);  // 等待1s

            if (aiTurnStateMachine == null)
            {
                aiTurnStateMachine = new AITurnStateMachine.AITurnStateMachine(); // 假设这个函数根据国家ID或创建AI状态管理器
                aiTurnStateMachine.Init(countryId);
            }
            else
            {
                aiTurnStateMachine.SetTurn(countryId);
            }
            
            yield return new WaitUntil(() => aiTurnStateMachine.IsFinished);  // 更新AI结束状态
            yield return new WaitForSeconds(1f);  // 等待1s
            PlayingState = GameState.Playing;  // 重置状态
        }

        private void Update()
        {
            aiTurnStateMachine?.UpdateState();
        }

        // 处理战后判断国家是否灭亡逻辑
        public void CountryDieAfterWar()
        {
            if (countryDieTips == 1)
            {
                AfterInheritNextCountryTurn(ShowInfo);  // 切换AI势力灭亡状态并处理
            }
            else if (countryDieTips == 2)
            {
                PlayingState = GameState.PlayerInherit;
                Task = TaskType.Inherit;  // 调用继承逻辑
                ShowInfo = ShowInfo + "新君主" + (GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(playerCountryId)).countryKingId)).generalName + " 继位!";  // 更新信息
                AfterInheritNextCountryTurn("新君主" + (GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(playerCountryId)).countryKingId)).generalName + " 继位!");  // 切换玩家势力继位状态并处理
            }
            else if (countryDieTips == 3)
            {
                playerOrderNum = 1;  // 设置用户指令
                PlayingState = GameState.GameOver;
                AfterInheritNextCountryTurn("游戏结束");  // 切换玩家势力灭亡状态
                return;
            }
            else if (countryDieTips == 4)
            {
                PlayingState = GameState.AIInherit;
                AfterInheritNextCountryTurn("AI新君主继位");  // 切换AI继位状态
            }
            countryDieTips = 0;  // 重置国家灭亡提示
            PlayingState = GameState.PlayerUseOrder;  // 更新状态为4
        }


    
        /// <summary>
        /// 势力继位后轮到下一个势力回合处理
        /// </summary>
        void AfterInheritNextCountryTurn(string text)
        {
            StartCoroutine(uiGlobe.tips.ShowTurnTips(text, GameState.AIInherit));
            StartCoroutine(ExecuteMouth());
            PlayingState = GameState.Playing; // 设置标志位
        }
        
    
        /// <summary>
        /// 检查所有城市是否都属于玩家国家的国王
        /// </summary>
        /// <returns></returns>
        private bool PlayerHaveAllCity()
        {
            byte count = CountryListCache.GetCountryByCountryId(playerCountryId).GetHaveCityNum();
            byte cityNum = CityListCache.GetCityNum(); // 总城池数量
            return count == cityNum; // 所有城市都属于玩家国家的国王，返回true
        }

        /// <summary>
        /// 检查玩家国家是否没有城市
        /// </summary>
        /// <returns></returns>
        private bool PlayerHaveNoneCity()
        {
            Country userCountry = CountryListCache.GetCountryByCountryId(playerCountryId);
            // 如果玩家国家存在且拥有的城市数量不为0，则返回false，否则返回true
            return !(userCountry != null && userCountry.GetHaveCityNum() != 0);
        }


        /// <summary> 
        /// 处理游戏中的月度事件
        /// </summary>
        private IEnumerator MonthlyEvent()
        {
            Debug.Log("月度事件处理");
            _disasterCount = 0; // 初始化灾难计数为 0
            _disasterCity = new List<byte>(); // 初始化灾难城市列表
            // 检查玩家是否没有城市
            if (PlayerHaveNoneCity())
            {
                PlayingState = GameState.GameOver;
                uiGlobe.UpdateTurnInfo(String.Empty);
                uiGlobe.GameEnd();
                yield break; // 结束方法
            }

            // 检查玩家是否拥有所有城市
            if (PlayerHaveAllCity())
            {
                PlayingState = GameState.GameSuccess;
                uiGlobe.UpdateTurnInfo(String.Empty);
                uiGlobe.GameEnd();
                yield break; // 结束方法
            }

            yield return Uprising();
            // 检查是否发生旱灾、洪灾和蝗灾
            CheckForDisasters(DisasterRate_i);

            // 如果没有以上灾难，检查是否发生瘟疫
            if (_disasterCount < 1)
            {
                CheckForDisasters(PlagueRate);
            }
            else
            {
                yield return DisastersType();// 处理三灾类型
            }

            if (_disasterCount > 0)
            {
                // 对发生灾害的的城市 ID 处理瘟疫
                for (short i = 0; i < _disasterCount; i++)
                    yield return HandlePlague(_disasterCity[i]);
            }

            // 检查百姓叛乱
            HandleTurmoil();

            // 自动治理所有城市
            AutoInteriorAllCity();

            // 处理城市中的将军
            HandleCityGenerals();

            // 按照月份处理定期事件
            HandleMonthlyEvents();

            // 处理回合技能事件
            HandleTurnSkills(); 

            // 更新势力联盟
            UpdateAlliance();

            // 在野武将移动
            TalentGenMove();
        }


        /// <summary>
        /// 检查指定灾难发生的城市
        /// </summary>
        /// <param name="disasterRateFunc">灾难率检查函数</param>
        private void CheckForDisasters(Func<bool> disasterRateFunc)
        {
            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId++)
            {
                if (disasterRateFunc())
                {
                    _disasterCount++;
                    _disasterCity.Add (cityId);
                }
            }
        }



        // 判断是否为特定事件
        bool DisasterRate_i()
        {
            // 判断当前月份是否在3月到11月之间
            if (month < 11 && month > 2)
            {
                // 随机生成一个0到499之间的整数，并判断是否小于4
                return (Random.Range(0, 500) < 5);
            }
            return false;
        }

        // 判断是否为特定事件
        bool PlagueRate()
        {
            // 随机生成一个0到499之间的整数，并判断是否小于等于1
            return (Random.Range(0, 500) <= 2);
        }

        /// <summary>
        /// 起义
        /// </summary>
        /// <returns></returns>
        private IEnumerator Uprising()
        {
            foreach (var city in CityListCache.cityDictionary.Values)
            {
                if (city.IsRebel())
                {
                    short prefectId = city.prefectId;  // 获取城市太守ID
                    General general = GeneralListCache.GetGeneral(prefectId);  // 获取将领对象
                    Country oldCountry = CountryListCache.GetCountryByKingId(city.cityBelongKing);  // 获取旧国家
                    oldCountry.RemoveCity(city.cityId);  // 从旧国家移除该城市
                    Country newCountry = new Country();  // 创建新国家
                    newCountry.countryId = (byte)(CountryListCache.GetCountrySize() + 1);  // 设置新国家ID
                    newCountry.countryKingId = general.generalId;  // 设置新国家的国王ID
                    city.prefectId = general.generalId;  // 设置城市的太守为该将领
                    newCountry.AddCity(city.cityId);  // 新国家添加城市
                    CountryListCache.AddCountry(newCountry);  // 将新国家添加到国家缓存
                    CountryListCache.countrySequence.Insert(0, newCountry.countryId);  // 插入势力顺序
                    string text = general.generalName + "在" + city.cityName + "起义！";
                    yield return uiGlobe.tips.ShowTurnTips(text, GameState.Rebel);
                    Debug.Log(text);  // 输出起义日志
                    yield break;
                }
            }
            
        }

        /// <summary>
        /// 处理旱灾、洪灾、蝗灾三种灾难事件种类
        /// </summary>
        private IEnumerator DisastersType()
        {
            for (short i = 0; i < _disasterCount; i++)
            {
                Disaster(_disasterCity[i]); // 处理每个城市的洪水灾难


                string text = string.Empty;
                int disasterKind = Random.Range(0, 3); // 设置随机事件 ID
                switch (disasterKind)
                {
                    case 0:
                        text = CityListCache.GetCityByCityId(_disasterCity[i]).cityName + "发生旱灾";
                        yield return uiGlobe.tips.ShowTurnTips(text, GameState.Drought);
                        break;
                    case 1:
                        text = CityListCache.GetCityByCityId(_disasterCity[i]).cityName + "发生洪涝";
                        yield return uiGlobe.tips.ShowTurnTips(text, GameState.Flood);
                        break;
                    case 2:
                        text = CityListCache.GetCityByCityId(_disasterCity[i]).cityName + "发生蝗灾";
                        yield return uiGlobe.tips.ShowTurnTips(text, GameState.LocustPlague);
                        break;
                }
            }
            _disasterCount = 0; // 重置灾难计数
        }

        /// <summary>
        /// 处理城市的瘟疫灾害
        /// </summary>
        /// <param name="cityId"></param>
        private IEnumerator HandlePlague(byte cityId)
        {
            // 获取指定ID的城市
            City city = CityListCache.GetCityByCityId(cityId);
            // 获取城市中的将军ID数组
            short[] officeGeneralIdArray = city.GetOfficerIds();

            // 如果城市的洪水控制小于90
            if (city.floodControl < 90)
            {
                // 遍历城市中的将军，减少将军的士兵数量
                for (byte byte1 = 0; byte1 < city.GetCityOfficerNum(); byte1 = (byte)(byte1 + 1))
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[byte1]);
                    general.generalSoldier = (short)(general.generalSoldier - DisasterLoss(general.generalSoldier, city.floodControl, 90));
                }
                // 减少城市储备士兵数量和统治力
                city.cityReserveSoldier -= DisasterLoss(city.cityReserveSoldier, city.floodControl, 90);
                city.rule = (byte)(city.rule - DisasterLoss(city.rule, city.floodControl, 90));
            }

            // 如果城市的洪水控制小于99
            if (city.floodControl < 99)
                // 减少城市人口
                city.population -= DisasterLoss(city.population, city.floodControl, 99);

            // 如果城市的洪水控制大于0
            if (city.floodControl > 0)
                // 减少洪水控制值，每次减少1/10加1
                city.floodControl = (byte)(city.floodControl - city.floodControl / 10 + 1);

            string text = CityListCache.GetCityByCityId(_disasterCity[0]).cityName + "发生瘟疫";
            yield return uiGlobe.tips.ShowTurnTips(text, GameState.Plague);
        }


        // 计算某值
        int DisasterLoss(int i1, byte byte0, int j1)
        {
            i1 /= 2;
            return byte0 * i1 / j1;
        }


        // 处理城市的洪水灾难
        private void Disaster(byte byte0)
        {
            // 获取指定ID的城市
            City city = CityListCache.GetCityByCityId(byte0);

            // 如果城市的洪水控制小于90
            if (city.floodControl < 90)
            {
                // 减少城市的金钱、食物和统治力
                city.SubGold((short)DisasterLoss(city.GetMoney(), city.floodControl, 90));
                city.SubFood((short)DisasterLoss(city.GetFood(), city.floodControl, 90));
                city.rule = (byte)(city.rule - DisasterLoss(city.rule, city.floodControl, 90));
            }

            // 如果城市的洪水控制小于99
            if (city.floodControl < 99)
            {
                // 减少城市的贸易和农业
                city.trade = (short)(city.trade - DisasterLoss(city.trade, city.floodControl, 99));
                city.agro = (short)(city.agro - DisasterLoss(city.agro, city.floodControl, 99));
            }

            // 如果城市的洪水控制大于0
            if (city.floodControl > 0)
                // 减少洪水控制值，每次减少1/10加1
                city.floodControl = (byte)(city.floodControl - city.floodControl / 10 + 1);
        }


        /// <summary>
        /// 处理灾难和骚乱
        /// </summary>
        private void HandleTurmoil()
        {
            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId++)
            {
                City city = CityListCache.GetCityByCityId(cityId);
                if (city.cityBelongKing > 0 && IsTurmoil(cityId))
                {
                    _disasterCount++;
                    Debug.Log(_disasterCount);
                    this._disasterCity[_disasterCount] = cityId;
                }
            }

            if (_disasterCount > 0)
            {
                for (short i = 0; i < _disasterCount; i++)
                    TurmoilLowRule(this._disasterCity[i]);

                PlayingState = GameState.Turmoil; // 设置事件 ID
                _disasterCount = 0; // 重置灾难计数
                AIStateMachine.AIRebuildCity(); // Ai自动灾后重建所有城市
            }
        }

        /// <summary>
        /// 检查是否有骚乱的可能性
        /// </summary>
        /// <param name="cityId">城市 ID</param>
        /// <returns>是否叛乱</returns>
        private bool IsTurmoil(byte cityId)
        {
            City city = CityListCache.GetCityByCityId(cityId);
            if (city.rule < 15)
            {
                // 计算叛乱发生的概率
                int i1 = city.rule * 10 / 15 + 80;
                if (Random.Range(0, 101) >= i1)
                    return true;
            }
            else if (city.rule < 30)
            {
                int j1 = (city.rule - 15) / 3 + 90;
                if (Random.Range(0, 101) >= j1)
                    return true;
            }
            else if (city.rule < 40)
            {
                int k1 = (city.rule - 30) / 5 + 98;
                if (Random.Range(0, 101) >= k1)
                    return true;
            }
            else if (city.rule < 60)
            {
                int l1 = (city.rule - 40) / 5 + 998;
                if (Random.Range(0, 1001) >= l1)
                    return true;
            }
            return false;
        }



        /// <summary>
        /// 处理城市的叛乱灾难
        /// </summary>
        /// <param name="cityId">城市 ID</param>
        private void TurmoilLowRule(byte cityId)
        {
            int i1 = 0;
            // 获取指定ID的城市
            City city = CityListCache.GetCityByCityId(cityId);
            // 获取城市中的将军ID数组
            short[] officeGeneralIdArray = city.GetOfficerIds();

            // 根据城市的统治度来处理不同的情况
            if (city.rule < 15)
            {
                // 统治度小于15，城市的各种资源减少
                city.population /= 3;
                city.agro = (short)(city.agro / 3);
                city.trade = (short)(city.trade / 3);
                city.SetMoney((short)(city.GetMoney() / 2));
                city.SetFood((short)(city.GetFood() / 2));
                city.rule = (byte)((city.rule - Random.Range(0,15)));
                city.cityReserveSoldier -= Random.Range(city.cityReserveSoldier * 1 / 3,city.cityReserveSoldier* 2 / 3);
                for (byte byte1 = 0; byte1 < city.GetCityOfficerNum(); byte1 = (byte)(byte1 + 1))
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[byte1]);
                    general.generalSoldier -= (short)Random.Range(general.generalSoldier *1/3,general.generalSoldier* 2/3);
                    i1 += general.generalSoldier;
                }
            }
            else if (city.rule < 30)
            {
                // 统治度在15到30之间，城市的各种资源减少
                city.population /= 2;
                city.agro = (short)(city.agro / 2);
                city.trade = (short)(city.trade / 2);
                city.SetMoney((short)(city.GetMoney() * 3 / 7));
                city.SetFood((short)(city.GetFood() * 3 / 7));
                city.rule = (byte)((city.rule - Random.Range(10, 20)));
                city.cityReserveSoldier -= Random.Range(city.cityReserveSoldier * 1 / 4, city.cityReserveSoldier * 1 / 2);
                for (byte byte2 = 0; byte2 < city.GetCityOfficerNum(); byte2 = (byte)(byte2 + 1))
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[byte2]);
                    general.generalSoldier -= (short)Random.Range(general.generalSoldier * 1 / 4, general.generalSoldier * 1 / 2);
                    i1 += general.generalSoldier;
                }
            }
            else
            {
                // 统治度在30以上，城市的各种资源减少
                city.population = city.population * 2 / 3;
                city.agro = (short)(city.agro * 2 / 3);
                city.trade = (short)(city.trade * 2 / 3);
                city.SetMoney((short)(city.GetMoney() * 1 / 7));
                city.SetFood((short)(city.GetFood() * 1 / 7));
                city.rule = (byte)((city.rule - Random.Range(15, 25)));
                city.cityReserveSoldier -= Random.Range(city.cityReserveSoldier * 1 / 5, city.cityReserveSoldier * 1 / 4);
                for (byte byte3 = 0; byte3 < city.GetCityOfficerNum(); byte3 = (byte)(byte3 + 1))
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[byte3]);
                    general.generalSoldier -= (short)Random.Range(general.generalSoldier * 1 / 5, general.generalSoldier * 1 / 4);
                    i1 += general.generalSoldier;
                }
            }
        }

    




        /// <summary>
        /// 处理城市中的将军逻辑
        /// </summary>
        private void HandleCityGenerals()
        {
            short userKingId = CountryListCache.GetCountryByCountryId(playerCountryId).countryKingId; // 获取玩家国王 ID

            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId++)
            {
                City city = CityListCache.GetCityByCityId(cityId);
                if (city.cityBelongKing > 0)
                {
                    HandleGeneralAssignments(city, userKingId); // 处理将军任命
                    city.SoldierEatFood(); // 士兵吃粮食
                    city.PaySalaries(); // 支付工资
                    BaseGeneralTreat(city); // 更新将军状态
                }
            }
        }

        /// <summary>
        /// 处理AI将军任命和随机搜索事件
        /// </summary>
        /// <param name="city">城市实例</param>
        /// <param name="userKingId">用户国王 ID</param>
        private void HandleGeneralAssignments(City city, short userKingId)
        {
            if (city.cityBelongKing != userKingId)
            {
                city.AppointmentPrefect(); // 任命城市长
                RandomlyAddGeneral(city); // 随机添加将军
            }
        }

        /// <summary>
        /// 随机添加在野将军
        /// </summary>
        /// <param name="city">城市实例</param>
        private void RandomlyAddGeneral(City city)
        {
            if (Random.Range(0, 6) < 1)
            {
                byte notFoundGeneralNum = city.GetCityNotFoundGeneralNum();
                if (notFoundGeneralNum > 0)
                {
                    int index = Random.Range(0, notFoundGeneralNum);
                    short generalId = city.GetNotFoundGeneralId((byte)index);
                    city.RemoveNotFoundGeneralId(generalId);
                    if (generalId > 0)
                        city.AddReservedGeneralId(generalId); // 添加将军
                }
            }
        }

        /// <summary>
        /// 基本回复受伤将军的状态
        /// </summary>
        /// <param name="city">城市实例</param>
        private void BaseGeneralTreat(City city)
        {
            short[] officeGeneralIdArray = city.GetOfficerIds();

            foreach (short id in officeGeneralIdArray)
            {
                General general = GeneralListCache.GetGeneral(id);
                if (general.GetCurPhysical() < general.maxPhysical)
                {
                    byte addPhysical = (byte)(1 + Random.Range(0, 3));
                    general.AddCurPhysical(addPhysical);
                }

            }
        }


        /// <summary>
        /// 处理按月的定期事件
        /// </summary>
        private void HandleMonthlyEvents()
        {
            if (month == 4 || month == 8 || month == 12)
            {
                // 遍历所有城市 ID
                for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
                {
                    // 如果城市的国王 ID 大于 0
                    if ((CityListCache.GetCityByCityId(cityId)).cityBelongKing > 0)
                        RegularTaxMoney(cityId); // 调用 RegularTaxMoney 方法
                }
                PlayingState=GameState.MoneyTax;
            }
            else if (month == 5 || month == 10)
            {
                // 遍历所有城市 ID
                for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
                {
                    // 如果城市的国王 ID 大于 0
                    if ((CityListCache.GetCityByCityId(cityId)).cityBelongKing > 0)
                        RegularTaxFood(cityId); // 调用 RegularTaxFood 方法
                }
                PlayingState=GameState.FoodTax;
            }

            if (month == 3 || month == 6 || month == 9 || month == 12)
            {
                HandleGeneralLoyaltyDecay(); // 处理将军忠诚度衰减
            }

        }

    



        /// <summary>
        /// 定期征收金钱
        /// </summary>
        /// <param name="cityId"></param>
        private void RegularTaxMoney(byte cityId)
        {
            // 获取指定ID的城市
            City city = CityListCache.GetCityByCityId(cityId);
            // 计算金钱收入
            int income = city.MoneyIncome();
            // 如果城市不属于玩家国家，则金钱收入增加20%
            if (city.cityBelongKing != (CountryListCache.GetCountryByCountryId(playerCountryId)).countryKingId)
                income = (int)(income * 1.2f);
            // 处理风水技能
            bool fengShui = false;
            foreach (var generalId in city.GetOfficerIds())
            {
                if (GeneralListCache.GetGeneral(generalId).HasSkill(4, 5)) fengShui = true;
            }
            if (fengShui) income += income /2;
            // 添加金钱到城市
            city.AddGold((short)income);
        }


        /// <summary>
        /// 定期征收食物
        /// </summary>
        /// <param name="cityId"></param>
        private void RegularTaxFood(byte cityId)
        {
            // 获取指定ID的城市
            City city = CityListCache.GetCityByCityId(cityId);
            // 计算食物产量
            int income = city.FoodIncome();
            // 如果城市不属于玩家国家，则食物产量增加20%
            if (city.cityBelongKing != (CountryListCache.GetCountryByCountryId(playerCountryId)).countryKingId)
                income = (int)(income * 1.2D);
            // 处理风水技能
            bool fengShui = false;
            foreach (var generalId in city.GetOfficerIds())
            {
                if (GeneralListCache.GetGeneral(generalId).HasSkill(4, 5)) fengShui = true;
            }
            if (fengShui) income += income /2;
            // 添加食物到城市
            city.AddFood((short)income);
        }

        private void HandleGeneralLoyaltyDecay()
        {
            for (byte b = 1; b < CityListCache.CITY_NUM; b = (byte)(b + 1))
            {
                City city = CityListCache.GetCityByCityId(b);
                short[] officeGeneralIdArray = city.GetOfficerIds();

                // 遍历城市中的将军
                for (int j = 0; j < city.GetCityOfficerNum(); j++)
                {
                    short generalId = officeGeneralIdArray[j];
                    short kingId = city.cityBelongKing;
                    General general = GeneralListCache.GetGeneral(generalId);

                    // 计算将军与城市国王的阶段差
                    int d = GeneralListCache.GetdPhase(general.phase, (GeneralListCache.GetGeneral(kingId)).phase);

                    // 如果阶段差大于 10 且将军的忠诚度不等于 100
                    if (d > 10 && general.GetLoyalty() != 100)
                    {
                        // 如果随机数小于阶段差75
                        if (Random.Range(0,75) < d)
                        {
                            int val = d / 10;
                            val = Mathf.Max(0, general.GetLoyalty() - val);
                            general.DecreaseLoyalty((byte)val); // 减少将军的忠诚度
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 处理回合技能效果
        /// </summary>
        private void HandleTurnSkills()
        {
            // 遍历所有城市 ID
            for (byte i = 1; i < CityListCache.CITY_NUM; i++)
            {
                City city = CityListCache.GetCityByCityId(i);
                short[] officeGeneralIdArray = city.GetOfficerIds();

                // 遍历城市中的将军
                for (int j = 0; j < city.GetCityOfficerNum(); j++)
                {
                    short id = officeGeneralIdArray[j];
                    General general = GeneralListCache.GetGeneral(id);
                    
                    // 处理义军技能
                    if (general.HasSkill(4, 6) && city.cityReserveSoldier <= 10000 && city.population >= 20000) HandleYiJunSkill(city, general);
                    // 处理内助神医技能
                    if (general.HasSkill(4, 7)) HandleNeiZhuSkill(officeGeneralIdArray);
                    // 处理仁义技能
                    if (general.HasSkill(4, 8)) HandleRenYiSkill(officeGeneralIdArray);
                    // 处理掠夺技能
                    if (general.HasSkill(5, 1) && general.IQ >= Random.Range(0,120)) ExecuteLueDuo(city, general);
                    // 处理能吏技能
                    if (general.HasSkill(5, 5)) HandleNengLiSkill(city, general);
                    // 处理练兵技能
                    if (general.HasSkill(5, 6)) HandleLianBingSkill(officeGeneralIdArray, general);
                    // 处理言教技能
                    if (general.HasSkill(5, 7)) HandleYanJiaoSkill(officeGeneralIdArray, general);
                }
            }
        }

        // 义军技能效果
        private void HandleYiJunSkill(City city, General general)
        {
            int addReserveSoldier = general.moral + city.population / 1000 + Random.Range(0, 200) - 100;
            if (addReserveSoldier >= 0)
            {
                city.cityReserveSoldier += addReserveSoldier;
                city.population -= addReserveSoldier;
            }
        }

        // 神医内助技能效果[10,20]
        private void HandleNeiZhuSkill(short[] officeGeneralIdArray)
        {
            foreach (short generalId in officeGeneralIdArray)
            {
                General general = GeneralListCache.GetGeneral(generalId);
                if (general.GetCurPhysical() < general.maxPhysical)
                {
                    byte addPhysical = (byte)Random.Range(10, 20);// 加上基础回复总范围[10,20]
                    general.AddCurPhysical(addPhysical);
                }
            }
        }


        // 仁义技能效果
        private void HandleRenYiSkill(short[] officeGeneralIdArray)
        {
            foreach (short generalId in officeGeneralIdArray)
            {
                General general = GeneralListCache.GetGeneral(generalId);
                if (general.GetLoyalty() < 90)
                {
                    byte x = (byte)Random.Range(5, 12);
                    if (general.GetLoyalty() + x >= 90) x = (byte)(90 - general.GetLoyalty());
                    general.AddLoyalty(x); // 增加忠诚度
                }
            }
        }




        // 掠夺技能效果
        private void ExecuteLueDuo(City city, General general)
        {
            byte[] enemyCityIds = CountryListCache.getEnemyCityIdArray_new(city.cityId);

            foreach (byte enemyCityId in enemyCityIds)
            {
                City enemyCity = CityListCache.GetCityByCityId(enemyCityId);
                General prefectGeneral = GeneralListCache.GetGeneral(enemyCity.prefectId);
                byte forceDifference = (byte)(general.force - prefectGeneral.force);

                if (GetLueDuoByForceD(forceDifference) >= Random.Range(0, 70))
                {
                    LueDuoNum(city, enemyCity);
                    break; // 一旦成功掠夺，退出循环
                }
            }
        }

        private IEnumerator LueDuoNum(City city, City enemyCity)
        {
            short food = (short)LueDuoRate(enemyCity.GetFood(), city.GetFood());
            short money = (short)LueDuoRate(enemyCity.GetMoney(), city.GetMoney());
            int population = LueDuoRate(enemyCity.population, city.population);
            string text = $"{city.cityName}掠夺了{enemyCity.cityName}粮食：{food} 金：{money} 人口：{population}";

            // 减少敌方城市资源
            enemyCity.SubFood(food);
            enemyCity.SubGold(money);
            enemyCity.SubPopulation(population);

            // 打印掠夺信息
            yield return uiGlobe.tips.ShowTurnTips(text, GameState.Plunder);
            Debug.Log(text);

            // 增加本城市资源
            city.AddFood(food);
            city.AddGold(money);
            city.AddPopulation(population);
        }

        private int LueDuoRate(int enemyValue, int cityValue)
        {
            short result = (short)(enemyValue * 0.04D - cityValue * 0.01D);
            return (short)Math.Max((int)result, 0);
        }

        private int GetLueDuoByForceD(byte forceDifference)
        {
            if (forceDifference >= 50) return 60;
            if (forceDifference >= 40) return 55;
            if (forceDifference >= 30) return 50;
            if (forceDifference >= 20) return 45;
            if (forceDifference >= 10) return 40;
            return 35; // forceDifference < 10
        }



        //能吏技能效果
        private void HandleNengLiSkill(City city, General general)
        {
            if (general.political < Random.Range(0, 100))
                return;

            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0:
                    city.agro = (short)Math.Min(city.agro + general.political / 10, 999);
                    break;
                case 1:
                    city.trade = (short)Math.Min(city.trade + general.political / 10, 999);
                    break;
                case 2:
                    city.population += general.political * 30;
                    city.population = Math.Min(city.population, 990000);
                    break;
                case 3:
                    city.floodControl = (byte)Math.Min(city.floodControl + Random.Range(0, 4), 99);
                    break;
            }
        }


        // 练兵技能效果
        private void HandleLianBingSkill(short[] officeGeneralIdArray, General general)
        {
            foreach (short otherGeneralId in officeGeneralIdArray)
            {
                General otherGeneral = GeneralListCache.GetGeneral(otherGeneralId);
                otherGeneral.Addexperience(Random.Range(0, general.force));
            }
        }
    

        // 
        private void HandleYanJiaoSkill(short[] officeGeneralIdArray, General general)
        {
            foreach (short otherGeneralId in officeGeneralIdArray)
            {
                General otherGeneral = GeneralListCache.GetGeneral(otherGeneralId);
                otherGeneral.AddIqExp((byte)Random.Range(1, general.IQ / 10));
            }
        }



    

        /// <summary>
        /// 处理自动治理所有城市的逻辑
        /// </summary>
        private void AutoInteriorAllCity()
        {
            AIStateMachine.AutoInteriorAllCity();
        }

    
        private void UpdateAlliance()
        {
            // 遍历所有国家
            foreach (var countryPair in CountryListCache.countryDictionary)
            {
                Country country = countryPair.Value;
                List<Alliance> allianceList = country.allianceList;

                // 遍历联盟列表
                foreach (var alliance in allianceList)
                {
                    alliance.Months = (byte)(alliance.Months - 1);

                    // 如果联盟持续时间小于等于 0
                    if (alliance.Months <= 0)
                    {
                        bool isRemoveAlliance = country.RemoveAlliance(alliance.countryId);

                        // 如果联盟被移除
                        if (isRemoveAlliance)
                        {
                            //PlayingState = GameState.AllianceEnd; // 设置事件 ID 为 14
                            Debug.Log($"移除联盟：{country.countryId} - {alliance.countryId}");
                        }
                    }
                }
            }
        }

        //TODO
        /// <summary>
        /// 处理已经发掘的人才将领的移动逻辑
        /// </summary>
        void TalentGenMove()
        {
            List<short> vector = new List<short>(); // 创建一个列表来存储将领ID

            foreach (var city in CityListCache.cityDictionary.Values)
            {
                List<short> talentIds = city.GetTalentIds();
                foreach (var id in talentIds)
                {
                    if (id > 0)
                    {
                        if (vector.Contains(id))
                        {
                            // 如果列表中已包含该将领ID，则跳过当前循环
                            continue;
                        }
                        vector.Add(id); // 添加将领ID到列表

                        bool ev = false;
                        byte evCountryId = 0;
                        General general = GeneralListCache.GetGeneral(id);
                        // 查找符合条件的国家
                        foreach (var countryPair in CountryListCache.countryDictionary)
                        {
                            if ((GeneralListCache.GetGeneral(countryPair.Value.countryKingId)).phase == general.phase)
                            {
                                ev = true;
                                evCountryId = countryPair.Key;
                            }
                        }

                        if (city.cityBelongKing > 0)
                        {
                            General cityKing = GeneralListCache.GetGeneral(city.cityBelongKing);
                            short kingPhase = cityKing.phase;

                            if (ev)
                            {
                                // 如果找到符合条件的国家
                                Country evCountry = CountryListCache.GetCountryByCountryId(evCountryId);
                                if (evCountry != null && evCountry.countryKingId != city.cityBelongKing)
                                {
                                    // 遍历符合条件的城市，并进行在野将领移动
                                    foreach (var cityID in evCountry.cityIDs)
                                    {
                                        City otherCity = CityListCache.GetCityByCityId(cityID);
                                        if (CityListCache.GetCityByCityId(cityID).GetOfficerIds()[0] == evCountry.countryKingId)
                                        {
                                            TalentGenMove(id, city, otherCity);
                                        }
                                    }
                                }
                            }
                            else if (GeneralListCache.GetPhaseDifference(cityKing, general) > 5 || city.GetCityOfficerNum() >= 10)
                            {
                                // 如果将领的阶段差距大于5，或者城市将领数量大于等于10
                                City moveCity = GetTalentMoveTargetCity(city);
                                if (moveCity != null)
                                {
                                    TalentGenMove(id, city, moveCity);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 获取在野将领可以移动到的目标城市
        /// </summary>
        /// <param name="curCity"></param>
        /// <returns></returns>
        City GetTalentMoveTargetCity(City curCity)
        {
            City tarCity = curCity;
            byte[] connectionCityIds = curCity.connectCityId;
            City[] canMoveCityIds = new City[connectionCityIds.Length];
            byte canMoveIndex = 0;

            for (byte index = 0; index < connectionCityIds.Length; index++)
            {
                City city = CityListCache.GetCityByCityId(connectionCityIds[index]);
                if (city != tarCity && city.GetReservedGeneralNum() < 10)
                {
                    canMoveCityIds[canMoveIndex] = city;
                    canMoveIndex++;
                }
            }

            if (canMoveIndex == 0)
                return null;

            int moveIndex = Random.Range(0, canMoveIndex);
            if (canMoveCityIds[moveIndex] != null)
                tarCity = canMoveCityIds[moveIndex];

            return tarCity;
        }

        /// <summary>
        /// 执行在野将领的移动
        /// </summary>
        /// <param name="generalId"></param>
        /// <param name="curCity"></param>
        /// <param name="tarCity"></param>
        void TalentGenMove(short generalId, City curCity, City tarCity)
        {
            if (curCity == tarCity)
                return;

            tarCity.AddReservedGeneralId(generalId);

            curCity.RemoveReservedGeneralId(generalId);
        }


        /// <summary>
        /// 测试已出场将领
        /// </summary>
        public void TestGeneral()
        {
            short generalNum = (short)GeneralListCache.generalList.Count;

            for (short k = 0; k < generalNum; k++)
            {
                General general = GeneralListCache.GetGeneral(k);

                if (general == null)
                {
                    Debug.LogError("系统异常！！！第" + k + "个武将不存在.");
                }
                else
                {
                    string cityInfoString = "";
                    int n = 0;

                    for (byte i = 0; i < CityListCache.GetCityNum(); i++)
                    {
                        City city = CityListCache.GetCityByCityId(i);
                        byte index;

                        // 检查城市中的将领
                        for (index = 0; index < city.GetCityOfficerNum(); index++)
                        {
                            if (city.GetOfficerIds()[index] == general.generalId)
                            {
                                cityInfoString += city.cityName;
                                n++;
                            }
                        }

                        // 检查城市中的对手将领
                        for (index = 0; index < city.GetReservedGeneralNum(); index++)
                        {
                            if (city.GetReservedGeneralId(index) == general.generalId)
                            {
                                cityInfoString += "[-" + city.cityName + "-]";
                                n++;
                            }
                        }

                        // 检查未找到的将领
                        for (index = 0; index < city.GetCityNotFoundGeneralNum(); index++)
                        {
                            if (city.GetCityNotFoundGeneralIdArray()[index] == general.generalId)
                            {
                                cityInfoString += "[" + city.cityName + "]";
                                n++;
                            }
                        }
                    }

                    if (n > 1)
                        Debug.LogError("系统异常！！！" + general.generalName + "在城池：" + cityInfoString + " 任职！");
                }
            }
        }


    }
}