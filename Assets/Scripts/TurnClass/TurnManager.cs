using System;
using System.Collections;
using System.Collections.Generic;
using BaseClass;
using DataClass;
using TurnClass.AITurnStateMachine;
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
        
        
        private Dictionary<byte, GameState> _disasterCity;
        public UIGlobe uiGlobe;
        
        private EventQueueManager monthlyQueue = new EventQueueManager();
        private AITurnStateMachine.AITurnStateMachine aiTurnStateMachine;
        public Action OnPlayerConfirm;
        
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
                Debug.Log("进入全局场景游戏状态为" + PlayingState);
                if (Task == TaskType.SelfBuild) return;
                if (PlayingState == GameState.GameStart)
                {
                    GameStart();
                }
                else if (PlayingState == GameState.Playing)//处于回合切换间
                {
                    StartTurn();
                }
                else if (PlayingState == GameState.AITurn || PlayingState == GameState.AIvsPlayer || PlayingState == GameState.PlayervsAI)//AI发起战争后返回
                {
                    BackFromWar();
                }
                else
                {
                    Debug.LogWarning("当前游戏状态" + PlayingState);
                }
                
            }
        }

        public void GameStart()
        {
            DataManager.ReadMapData();
            Debug.Log(PlayerPrefs.GetFloat("bgmVolume"));
            if (PlayerPrefs.GetFloat("bgmVolume") > 0)
            {
                GameClass.SoundManager.Instance.PlayBGM("Assets/Audio/Bgm/2.ogg");
                Debug.Log("播放音乐2");
            }
            else
            {
                GameClass.SoundManager.Instance.StopBGM();
            }
            curTurnIndex = -1;  // 初始化变量
            curTurnCountryId = 0;
            PlayingState = GameState.Playing;
            CountryListCache.TurnSort();  // 执行国家顺序排序
            Debug.Log(String.Join(", ", CountryListCache.countrySequence));

            StartTurn();
        }
        

        void StartTurn()
        {
            if (PlayingState == GameState.GameOver)
            {
                Debug.Log("游戏失败，退出");
                return;
            }

            if (PlayingState == GameState.Playing)
            {
                Debug.Log("开始新回合");
                curTurnCountryId = CountryListCache.GetCurrentExecutionCountryId();
                Debug.Log($"获取当前执行国家ID: {curTurnCountryId}, 下一个序号为 {curTurnIndex}");

                attackCount = 0;
                Debug.Log("重置攻击计数器");
                StartCoroutine(ExecuteTurn());
            }
            else
            {
                Debug.Log("开始回合当前状态错误: " + PlayingState);
            }
        }

        private void EndTurn()
        {
            uiGlobe?.UpdateTurnInfo(String.Empty);
            if (curTurnCountryId == CountryListCache.countrySequence[^1]) // 最后一个国家
            {
                AddMonth();

                // 月度事件处理，处理完毕后继续执行回合
                StartMonthlyEvents(() =>
                {
                    PlayingState = GameState.Playing;
                    StartTurn();
                });
            }
            else
            {
                PlayingState = GameState.Playing;
                StartTurn();
            }
        }

        void BackFromWar()
        {
            switch (countryDieTips)
            {
                case 1:
                    // AI继承操作
                    uiGlobe.tips.ShowTurnTipsWithConfirm(ShowInfo, GameState.Inherit, () =>
                    {
                        if (PlayingState == GameState.AIvsPlayer) // AI攻打玩家返回AI君主死亡继承
                        {
                            OnPlayerConfirm.Invoke();
                        }
                        else if (PlayingState == GameState.PlayervsAI) // 玩家攻打AI返回AI君主死亡继承
                        {
                            if (SubPlayerOrder())
                            {
                                StartTurn(); // 切换AI势力继承状态并处理
                            }
                            else
                            {
                                Task = TaskType.None;
                                PlayingState = GameState.PlayerTurn;
                            }
                        }
                    }); 
                    break;
                case 2: // 玩家继承,选城后跳转选将
                    if (PlayingState == GameState.AIvsPlayer)
                    {
                        PlayingState = GameState.AITurn;
                    }
                    else if (PlayingState == GameState.PlayervsAI)
                    {
                        playerOrderNum = 1;  // 设置玩家指令
                    }
                    
                    Task = TaskType.Inherit;  // 调用玩家继承逻辑
                    uiGlobe.UpdateTurnInfo("请选择继任者所在的城池");
                    break;
                case 3: // AI势力灭亡状态
                    uiGlobe.tips.ShowTurnTipsWithConfirm(ShowInfo, GameState.GameOver, () =>
                    {
                        if (PlayerHaveAllCity()) // 检查玩家是否拥有所有城市
                        {
                            PlayingState = GameState.GameWin;
                            uiGlobe.UpdateTurnInfo(String.Empty);
                            uiGlobe.GameEnd();
                        }
                        if (PlayingState == GameState.AIvsPlayer) // AI攻打玩家返回AI君主死亡继承
                        {
                            OnPlayerConfirm.Invoke();
                        }
                        else if (PlayingState == GameState.PlayervsAI) // 玩家攻打AI返回AI君主死亡继承
                        {
                            if (SubPlayerOrder())
                            {
                                StartTurn(); // 切换AI势力继承状态并处理
                            }
                            else
                            {
                                Task = TaskType.None;
                                PlayingState = GameState.PlayerTurn;
                            }
                        }
                    });  
                    break;
                case 4:
                    playerOrderNum = 1;  // 设置玩家指令
                    uiGlobe.tips.ShowTurnTipsWithConfirm(ShowInfo, GameState.GameOver, () =>
                    {
                        // 检查玩家是否没有城池
                        if (PlayerHaveNoneCity())
                        {
                            PlayingState = GameState.GameOver;
                            uiGlobe.UpdateTurnInfo(String.Empty);
                            uiGlobe.GameEnd();
                            StopAllCoroutines(); // 结束方法
                        }
                    });  // 切换玩家势力灭亡状态
                    break;
                default:
                    if (PlayingState == GameState.AITurn)
                    {
                        OnPlayerConfirm.Invoke();
                    }
                    else if (PlayingState == GameState.PlayervsAI)
                    {
                        if (SubPlayerOrder())
                        {
                            StartTurn(); // 切换AI势力继承状态并处理
                        }
                        else
                        {
                            Task = TaskType.None;
                            PlayingState = GameState.PlayerTurn;
                        }
                    }
                    else
                    {
                        Debug.LogError("任务类型" + Task + "当前状态错误" + PlayingState);
                    }
                    break;
            }

            countryDieTips = 0;// 重置国家灭亡提示
        }
        
        IEnumerator ExecuteTurn()
        {
            //根据势力ID开始执行回合
            if (curTurnCountryId == playerCountryId) //轮到玩家回合
            {
                yield return HandlePlayerTurn();  // 处理玩家回合
            }
            else //轮到AI回合
            {
                yield return HandleAITurn(curTurnCountryId);  // 处理AI回合
            }
            EndTurn();  // 结束回合
        }
        
        
        // 增加月份的方法
        void AddMonth()
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
            
            Country playerCountry = CountryListCache.GetCountryByCountryId(playerCountryId);

            if (isWatch)  // 如果是观察状态，直接返回
                yield break;

            if (PlayingState == GameState.Playing)  // 初始状态
            {
                playerOrderNum = GetPlayerOrderNum();  // 获取玩家指令编号
                doCityId = playerCountry.FindKingCity();  // 执行某个操作
                SceneManager.LoadScene("CityScene");
                PlayingState = GameState.PlayerTurn;
            }
            else if (PlayingState == GameState.PlayervsAI)
            {
                //ToDo
                //战争打AI
                //CountryDieAfterWar();  // 执行战斗后的结算
                Debug.Log("玩家战争结束");
                PlayingState = GameState.PlayerTurn;
            }
            
            yield return new WaitUntil(() => playerOrderNum == 0);  // 等待玩家指令执行完毕
            
            Debug.Log("玩家回合结束");
        }
        
        private IEnumerator HandleAITurn(byte countryId)
        {
            if (PlayingState == GameState.Playing)
            {
                if (SceneManager.GetActiveScene().name != "GlobalScene")
                {
                    yield return SceneManager.LoadSceneAsync("GlobalScene");  // 加载全地图场景
                }
                uiGlobe.UpdateTurnInfo(CountryListCache.GetCountryByCountryId(countryId).KingName() + " 战略中...");
                PlayingState = GameState.AITurn;  // 更新状态
            }
            
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
            Debug.Log("AI回合结束");
        }

        private void Update()
        {
            aiTurnStateMachine?.UpdateState();
        }
        

        public void StartMonthlyEvents(Action onComplete)
        {
            Debug.Log("开始处理月度事件...");
            _disasterCity = new Dictionary<byte, GameState>(); // 初始化灾难城池哈希表
            monthlyQueue.Clear(); // 清空上一次的队列
            
            // 以下添加所有要执行的月度事件步骤（顺序重要）
            monthlyQueue.AddEvent(OnFinish => Uprising(OnFinish));
            monthlyQueue.AddEvent(OnFinish => CheckForDisasters(OnFinish));
            monthlyQueue.AddEvent(OnFinish => AutoManageCities(OnFinish));
            monthlyQueue.AddEvent(OnFinish => HandleCityGenerals(OnFinish));
            monthlyQueue.AddEvent(OnFinish => HandleMonthlyEvents(OnFinish));
            monthlyQueue.AddEvent(OnFinish => HandleMonthlySkills(OnFinish));
            monthlyQueue.AddEvent(OnFinish => UpdateAlliances(OnFinish));
            monthlyQueue.AddEvent(OnFinish => TalentGenMove(OnFinish));

            // 最后一步
            monthlyQueue.AddEvent(OnFinish => {
                Debug.Log("月度事件处理完成！");
                onComplete?.Invoke();
            });

            // 开始执行
            monthlyQueue.Start();
        }

        
        
        /// <summary>
        /// 检查所有城池是否都属于玩家国家的国王
        /// </summary>
        /// <returns></returns>
        private bool PlayerHaveAllCity()
        {
            byte count = CountryListCache.GetCountryByCountryId(playerCountryId).GetHaveCityNum();
            byte cityNum = CityListCache.GetCityNum(); // 总城池数量
            return count == cityNum; // 所有城池都属于玩家国家的国王，返回true
        }

        /// <summary>
        /// 检查玩家国家是否没有城池
        /// </summary>
        /// <returns></returns>
        private bool PlayerHaveNoneCity()
        {
            Country userCountry = CountryListCache.GetCountryByCountryId(playerCountryId);
            // 如果玩家国家存在且拥有的城池数量不为0，则返回false，否则返回true
            return !(userCountry != null && userCountry.GetHaveCityNum() != 0);
        }
        

        /// <summary>
        /// 起义
        /// </summary>
        /// <returns></returns>
        private void Uprising(Action onComplete)
        {
            bool isUprising = false;  // 初始化是否发生起义
            foreach (var city in CityListCache.cityDictionary.Values)// 遍历所有城池
            {
                if (city.IsRebel())
                {
                    short prefectId = city.prefectID;  // 获取城池太守ID
                    General general = GeneralListCache.GetGeneral(prefectId);  // 获取将领对象
                    Country oldCountry = CountryListCache.GetCountryByKingId(city.ownerID);  // 获取旧国家
                    oldCountry.RemoveCity(city.cityID);  // 从旧国家移除该城池
                    Country newCountry = new Country();  // 创建新国家
                    newCountry.countryId = (byte)(CountryListCache.GetCountrySize() + 1);  // 设置新国家ID
                    newCountry.countryKingId = general.generalId;  // 设置新国家的国王ID
                    city.prefectID = general.generalId;  // 设置城池的太守为该将领
                    newCountry.AddCity(city.cityID);  // 新国家添加城池
                    CountryListCache.AddCountry(newCountry);  // 将新国家添加到国家缓存
                    CountryListCache.countrySequence.Insert(0, newCountry.countryId);  // 插入势力顺序
                    GameInfo.ShowInfo = general.generalName + "在" + city.cityName + "起义！";
                    Debug.Log(ShowInfo);  // 输出起义日志
                    isUprising = true;
                    break;
                }
            }
            
            if (isUprising)
            {
                uiGlobe.tips.ShowTurnTipsWithConfirm(ShowInfo, GameState.Rebel, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 检查指定灾难发生的城池
        /// </summary>
        /// <param name="onComplete">完成回调</param>
        private void CheckForDisasters(Action onComplete)
        {
            _disasterCity.Clear();  // 初始化是否发生灾难
            
            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId++)
            {
                // 只处理存在的城池
                City city = CityListCache.GetCityByCityId(cityId);
                if (city == null) continue;

                bool disasterOccurred = false;

                // 1. 检查旱灾、洪灾、蝗灾（三灾）
                if (NaturalDisasterRate(city, out var disasterType))
                {
                    _disasterCity.TryAdd(cityId, disasterType);
                    disasterOccurred = true;
                } 
                

                // 2. 如果没有三灾，检查瘟疫
                if (!disasterOccurred)
                {
                    if (PlagueRate(city))
                    {
                        _disasterCity.TryAdd(cityId, GameState.Plague);
                        disasterOccurred = true;
                    }
                }

                // 3. 如果还没有灾难且城池属于某势力，检查骚乱
                if (!disasterOccurred && city.ownerID > 0)
                {
                    if (TurmoilRate(city))
                    {
                        _disasterCity.TryAdd(cityId, GameState.Turmoil);
                    }
                }
            }

            // 灾难提示逐个播放
            if (_disasterCity.Count > 0)
            {
                PlayDisasterTips(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }


        private void PlayDisasterTips(Action onComplete)
        {
            var disasterQueue = new EventQueueManager();

            foreach (var record in _disasterCity)
            {
                disasterQueue.AddEvent(next =>
                {
                    uiGlobe.tips.ShowTurnTipsWithConfirm(
                        DisastersResult(record.Key, record.Value), record.Value, next);
                });
            }

            disasterQueue.AddEvent(next =>
            {
                Debug.Log("全部灾难提示完成");
                onComplete?.Invoke();
            });

            disasterQueue.Start();
        }
        


        // 判断是否发生旱灾、洪灾和蝗灾并处理返回是否发生灾难
        private bool NaturalDisasterRate(City city, out GameState disasterType)
        {
            disasterType = GameState.None;
            if (Random.Range(0, 500) < 5)
            {
                HandleNaturalDisaster(city); // 处理具体影响
                int disasterKind = Random.Range(0, 3);

                switch (disasterKind)
                {
                    case 0:
                        disasterType = GameState.Drought;
                        break;
                    case 1:
                        disasterType = GameState.Flood;
                        break;
                    case 2:
                        disasterType = GameState.LocustPlague;
                        break;
                }
                return true;
            }
            return false;
        }

        // 判断是否发生瘟疫
        bool PlagueRate(City city)
        {
            // 随机生成一个0到499之间的整数，并判断是否小于等于1
            if (Random.Range(0, 500) <= 2)
            {
                HandlePlague(city); // 处理具体影响
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否有骚乱的可能性
        /// </summary>
        /// <param name="city">城池</param>
        /// <returns>是否叛乱</returns>
        private bool TurmoilRate(City city)
        {
            byte rule = city.GetRule();

            int chance = rule switch
            {
                < 15 => rule * 10 / 15 + 80,
                < 30 => (rule - 15) / 3 + 90,
                < 40 => (rule - 30) / 5 + 98,
                < 60 => (rule - 40) / 5 + 998,
                _ => 1000 // 极小概率
            };

            int random = (rule < 60) ? Random.Range(0, 101) : Random.Range(0, 1001);
            if (random >= chance)
            {
                HandleTurmoil(city); // 处理具体影响
                return true;
            }
            return false;
        }

        /// <summary>
        /// 处理旱灾、洪灾、蝗灾三种灾难事件种类
        /// </summary>
        private string DisastersResult(byte cityId, GameState disasterType)
        {
            string message = CityListCache.GetCityByCityId(cityId).cityName;
            
            switch (disasterType)
            {
                case GameState.Drought:
                    message += "发生旱灾";
                    break;
                case GameState.Flood:
                    message += "发生洪涝";
                    break;
                case GameState.LocustPlague:
                    message += "发生蝗灾";
                    break;
                case GameState.Plague:
                    message += "发生瘟疫";
                    break;
                case GameState.Turmoil:
                    message += "发生骚乱";
                    break;
            }
            return message;
        }

        
        // 计算旱灾、洪水、蝗灾的灾难损失值
        int DisasterLoss(int param, byte floodControl, int i)
        {
            param /= 2;
            return floodControl * param / i;
        }


        // 处理城池的旱灾、洪水和蝗灾灾难
        private void HandleNaturalDisaster(City city)
        {
            // 获取指定ID的城池
            byte floodControl = city.GetFloodControl();  // 获取城池的洪水控制值

            // 如果城池的洪水控制小于90
            if (floodControl < 90)
            {
                // 减少城池的金钱、食物和统治力
                city.SubGold(DisasterLoss(city.GetMoney(), floodControl, 90));
                city.SubFood(DisasterLoss(city.GetFood(), floodControl, 90));
                city.SubRule(DisasterLoss(city.GetRule(), floodControl, 90));
            }

            // 如果城池的洪水控制小于99
            if (floodControl < 99)
            {
                // 减少城池的贸易和农业
                city.SubTrade(DisasterLoss(city.GetTrade(), floodControl, 99));
                city.SubAgro(DisasterLoss(city.GetAgro(), floodControl, 99));
            }

            // 如果城池的洪水控制大于0
            if (floodControl > 0)
                // 减少洪水控制值，每次减少1/10加1
                city.SubFloodControl(floodControl / 10 + 1);
        }



        /// <summary>
        /// 处理城池的瘟疫灾害
        /// </summary>
        /// <param name="city">城池</param>
        private void HandlePlague(City city)
        {
            byte floodControl = city.GetFloodControl();  // 获取城池的洪水控制值
            // 获取城池中的将军ID数组
            short[] officeGeneralIdArray = city.GetOfficerIds();

            // 如果城池的洪水控制小于90
            if (floodControl < 90)
            {
                // 遍历城池中的将军，减少将军的士兵数量
                for (byte i = 0; i < city.GetCityOfficerNum(); i++)
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]);
                    general.soldiers = (short)(general.soldiers - DisasterLoss(general.soldiers, floodControl, 90));
                }
                // 减少城池储备士兵数量和统治力
                city.reserveSoldiers -= DisasterLoss(city.reserveSoldiers, floodControl, 90);
                city.SubRule(DisasterLoss(city.GetRule(), floodControl, 90));
            }

            // 如果城池的洪水控制小于99
            if (floodControl < 99)
                // 减少城池人口
                city.SubPopulation(DisasterLoss(city.GetPopulation(), floodControl, 99));

            // 如果城池的洪水控制大于0
            if (floodControl > 0)
                // 减少洪水控制值，每次减少1/10加1
                city.SubFloodControl(floodControl / 10 + 1);
        }

        
        /// <summary>
        /// 处理城池的叛乱灾难
        /// </summary>
        /// <param name="city">城池</param>
        private void HandleTurmoil(City city)
        {
            int i1 = 0;
            // 获取指定ID的城池
            byte rule = city.GetRule();
            short gold = city.GetMoney();
            short food = city.GetFood();
            short agro = city.GetAgro();
            short trade = city.GetTrade();
            int population = city.GetPopulation();
            // 获取城池中的将军ID数组
            short[] officeGeneralIdArray = city.GetOfficerIds();

            // 根据城池的统治度来处理不同的情况
            if (rule < 15)
            {
                // 统治度小于15，城池的各种资源减少
                city.SetPopulation(population / 3);
                city.SetAgro(agro / 3);
                city.SetTrade(trade / 3);
                city.SetMoney(gold / 2);
                city.SetFood(food / 2);
                city.SubRule(Random.Range(0,15));
                city.reserveSoldiers -= Random.Range(city.reserveSoldiers * 1 / 3,city.reserveSoldiers* 2 / 3);
                for (byte i = 0; i < city.GetCityOfficerNum(); i++)
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]);
                    general.soldiers -= (short)Random.Range(general.soldiers *1/3,general.soldiers* 2/3);
                    i1 += general.soldiers;
                }
            }
            else if (rule < 30)
            {
                // 统治度在15到30之间，城池的各种资源减少
                city.SetPopulation(population / 2);
                city.SetAgro(agro / 2);
                city.SetTrade(trade / 2);
                city.SetMoney(gold * 3 / 7);
                city.SetFood(food * 3 / 7);
                city.SubRule(Random.Range(10, 20));
                city.reserveSoldiers -= Random.Range(city.reserveSoldiers * 1 / 4, city.reserveSoldiers * 1 / 2);
                for (byte i = 0; i < city.GetCityOfficerNum(); i++)
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]);
                    general.soldiers -= (short)Random.Range(general.soldiers * 1 / 4, general.soldiers * 1 / 2);
                    i1 += general.soldiers;
                }
            }
            else
            {
                // 统治度在30以上，城池的各种资源减少
                city.SetPopulation(population * 2 / 3);
                city.SetAgro(agro * 2 / 3);
                city.SetTrade(trade * 2 / 3);
                city.SetMoney(gold * 1 / 7);
                city.SetFood(food * 1 / 7);
                city.SubRule(Random.Range(15, 25));
                city.reserveSoldiers -= Random.Range(city.reserveSoldiers * 1 / 5, city.reserveSoldiers * 1 / 4);
                for (byte i = 0; i < city.GetCityOfficerNum(); i++)
                {
                    General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]);
                    general.soldiers -= (short)Random.Range(general.soldiers * 1 / 5, general.soldiers * 1 / 4);
                    i1 += general.soldiers;
                }
            }
        }

    




        /// <summary>
        /// 处理城池中的将军逻辑
        /// </summary>
        private void HandleCityGenerals(Action onComplete)
        {
            short userKingId = CountryListCache.GetCountryByCountryId(playerCountryId).countryKingId; // 获取玩家国王 ID

            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId++)
            {
                City city = CityListCache.GetCityByCityId(cityId);
                if (city.ownerID > 0)
                {
                    if (city.ownerID != userKingId)
                    {
                        city.AutoAppointPrefect(); // 任命城池长
                        RandomlyAddGeneral(city); // 随机添加将军
                    }
                    city.SoldierEatFood(); // 士兵吃粮食
                    city.PaySalaries(); // 支付工资
                    BaseGeneralTreat(city); // 更新将军状态
                }
            }
            
            onComplete?.Invoke();
        }
        

        /// <summary>
        /// 随机添加AI在野将军
        /// </summary>
        /// <param name="city">城池实例</param>
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
        /// <param name="city">城池实例</param>
        private void BaseGeneralTreat(City city)
        {
            short[] officeGeneralIdArray = city.GetOfficerIds();

            foreach (short id in officeGeneralIdArray)
            {
                General general = GeneralListCache.GetGeneral(id);
                if (general.GetHP() < general.maxHealth)
                {
                    byte addPhysical = (byte)(1 + Random.Range(0, 3));
                    general.AddHP(addPhysical);
                }

            }
        }


        /// <summary>
        /// 处理按月的定期事件
        /// </summary>
        private void HandleMonthlyEvents(Action onComplete)
        {
            if (month == 3 || month == 6 || month == 9 || month == 12)
            {
                HandleGeneralLoyaltyDecay(); // 处理将军忠诚度衰减
            }
            if (month == 4 || month == 8 || month == 12)
            {
                // 遍历所有城池 ID
                for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
                {
                    // 如果城池的国王 ID 大于 0
                    if ((CityListCache.GetCityByCityId(cityId)).ownerID > 0)
                        RegularTaxMoney(cityId); // 调用 RegularTaxMoney 方法
                }
                PlayingState = GameState.Tax;
                uiGlobe.tips.ShowTurnTipsWithConfirm("收金的季度到了!", GameState.Tax, onComplete);
            }
            else if (month == 5 || month == 10)
            {
                // 遍历所有城池 ID
                for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
                {
                    // 如果城池的国王 ID 大于 0
                    if ((CityListCache.GetCityByCityId(cityId)).ownerID > 0)
                        RegularTaxFood(cityId); // 调用 RegularTaxFood 方法
                }
                PlayingState = GameState.Harvest;
                uiGlobe.tips.ShowTurnTipsWithConfirm("收粮的季度到了!", GameState.Harvest, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

    



        /// <summary>
        /// 定期征收金钱
        /// </summary>
        /// <param name="cityId"></param>
        private void RegularTaxMoney(byte cityId)
        {
            // 获取指定ID的城池
            City city = CityListCache.GetCityByCityId(cityId);
            // 计算金钱收入
            int income = city.MoneyIncome();
            // 如果城池不属于玩家国家，则金钱收入增加20%
            if (city.ownerID != (CountryListCache.GetCountryByCountryId(playerCountryId)).countryKingId)
                income = (int)(income * 1.2f);
            // 处理风水技能
            bool fengShui = false;
            foreach (var generalId in city.GetOfficerIds())
            {
                if (GeneralListCache.GetGeneral(generalId).HasSkill(4, 5)) fengShui = true;
            }
            if (fengShui) income += income /2;
            // 添加金钱到城池
            city.AddGold((short)income);
        }


        /// <summary>
        /// 定期征收食物
        /// </summary>
        /// <param name="cityId"></param>
        private void RegularTaxFood(byte cityId)
        {
            // 获取指定ID的城池
            City city = CityListCache.GetCityByCityId(cityId);
            // 计算食物产量
            int income = city.FoodIncome();
            // 如果城池不属于玩家国家，则食物产量增加20%
            if (city.ownerID != (CountryListCache.GetCountryByCountryId(playerCountryId)).countryKingId)
                income = (int)(income * 1.2D);
            // 处理风水技能
            bool fengShui = false;
            foreach (var generalId in city.GetOfficerIds())
            {
                if (GeneralListCache.GetGeneral(generalId).HasSkill(4, 5)) fengShui = true;
            }
            if (fengShui) income += income /2;
            // 添加食物到城池
            city.AddFood((short)income);
        }

        /// <summary>
        /// 处理将军欠薪的忠诚度衰减
        /// </summary>
        private void HandleGeneralLoyaltyDecay()
        {
            for (byte b = 1; b < CityListCache.CITY_NUM; b = (byte)(b + 1))
            {
                City city = CityListCache.GetCityByCityId(b);
                short[] officeGeneralIdArray = city.GetOfficerIds();

                // 遍历城池中的将军
                for (int j = 0; j < city.GetCityOfficerNum(); j++)
                {
                    short generalId = officeGeneralIdArray[j];
                    short kingId = city.ownerID;
                    General general = GeneralListCache.GetGeneral(generalId);

                    // 计算将军与城池国王的阶段差
                    int d = GeneralListCache.GetdPhase(general.phase, (GeneralListCache.GetGeneral(kingId)).phase);

                    // 如果阶段差大于 10 且将军的忠诚度不等于 100
                    if (d > 10 && general.GetLoyalty() != 100)
                    {
                        // 如果随机数小于阶段差75
                        if (Random.Range(0,75) < d)
                        {
                            int val = d / 10;
                            val = Mathf.Max(0, general.GetLoyalty() - val);
                            general.SubLoyalty((byte)val); // 减少将军的忠诚度
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 处理回合技能效果
        /// </summary>
        private void HandleMonthlySkills(Action onComplete)
        {
            bool isLoot = false;
            ShowInfo = String.Empty;
            // 遍历所有城池 ID
            for (byte i = 1; i < CityListCache.CITY_NUM; i++)
            {
                City city = CityListCache.GetCityByCityId(i);
                short[] officeGeneralIdArray = city.GetOfficerIds();

                // 遍历城池中的将军
                for (int j = 0; j < city.GetCityOfficerNum(); j++)
                {
                    short id = officeGeneralIdArray[j];
                    General general = GeneralListCache.GetGeneral(id);
                    
                    // 处理义军技能
                    if (general.HasSkill(4, 6) && city.reserveSoldiers <= 10000 && city.GetPopulation() >= 20000) HandleYiJunSkill(city, general);
                    // 处理内助神医技能
                    if (general.HasSkill(4, 7)) HandleNeiZhuSkill(officeGeneralIdArray);
                    // 处理仁义技能
                    if (general.HasSkill(4, 8)) HandleRenYiSkill(officeGeneralIdArray);
                    // 处理掠夺技能
                    if (general.HasSkill(5, 1) && general.wisdom >= Random.Range(0,120)) isLoot = ExecuteLueDuo(city, general);
                    // 处理能吏技能
                    if (general.HasSkill(5, 5)) HandleNengLiSkill(city, general);
                    // 处理练兵技能
                    if (general.HasSkill(5, 6)) HandleLianBingSkill(officeGeneralIdArray, general);
                    // 处理言教技能
                    if (general.HasSkill(5, 7)) HandleYanJiaoSkill(officeGeneralIdArray, general);
                }
            }

            if (isLoot)
            {
                uiGlobe.tips.ShowTurnTipsWithConfirm(ShowInfo, GameState.Plunder, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        // 义军技能效果
        private void HandleYiJunSkill(City city, General general)
        {
            int addReserveSoldier = general.charm + city.GetPopulation() / 1000 + Random.Range(0, 200) - 100;
            if (addReserveSoldier >= 0)
            {
                city.reserveSoldiers += addReserveSoldier;
                city.SubPopulation(addReserveSoldier);
            }
        }

        // 神医内助技能效果[10,20]
        private void HandleNeiZhuSkill(short[] officeGeneralIdArray)
        {
            foreach (short generalId in officeGeneralIdArray)
            {
                General general = GeneralListCache.GetGeneral(generalId);
                if (general.GetHP() < general.maxHealth)
                {
                    byte addPhysical = (byte)Random.Range(10, 20);// 加上基础回复总范围[10,20]
                    general.AddHP(addPhysical);
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
        private bool ExecuteLueDuo(City city, General general)
        {
            bool isLoot = false;
            byte[] enemyCityIds = CountryListCache.getEnemyCityIdArray_new(city.cityID);

            foreach (byte enemyCityId in enemyCityIds)
            {
                City enemyCity = CityListCache.GetCityByCityId(enemyCityId);
                General prefectGeneral = GeneralListCache.GetGeneral(enemyCity.prefectID);
                byte forceDifference = (byte)(general.force - prefectGeneral.force);

                if (GetLueDuoByForceD(forceDifference) >= Random.Range(0, 70))
                {
                    LueDuoNum(city, enemyCity);
                    isLoot = true;
                    break; // 一旦成功掠夺，退出循环
                }
            }
            return isLoot;
        }

        private void LueDuoNum(City city, City enemyCity)
        {
            short food = (short)LueDuoRate(enemyCity.GetFood(), city.GetFood());
            short money = (short)LueDuoRate(enemyCity.GetMoney(), city.GetMoney());
            int population = LueDuoRate(enemyCity.GetPopulation(), city.GetPopulation());
            string text = $"{enemyCity.cityName}被贼寇洗劫!";

            // 减少敌方城池资源
            enemyCity.SubFood(food);
            enemyCity.SubGold(money);
            enemyCity.SubPopulation(population);
            
            // 增加本城池资源
            city.AddFood(food);
            city.AddGold(money);
            city.AddPopulation(population);
            
            // 打印掠夺信息
            Debug.Log($"{city.cityName}掠夺了{enemyCity.cityName}的粮:{food},金{money}！");
            GameInfo.ShowInfo = text;
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
            if (general.govern < Random.Range(0, 100))
                return;

            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0:
                    city.AddAgro(general.govern / 10);
                    break;
                case 1:
                    city.AddTrade(general.govern / 10);
                    break;
                case 2:
                    city.AddPopulation(general.govern * 30);
                    break;
                case 3:
                    city.AddRule(Random.Range(0, 4));
                    break;
            }
        }


        // 练兵技能效果
        private void HandleLianBingSkill(short[] officeGeneralIdArray, General general)
        {
            foreach (short otherGeneralId in officeGeneralIdArray)
            {
                General otherGeneral = GeneralListCache.GetGeneral(otherGeneralId);
                otherGeneral.AddExperience(Random.Range(0, general.force));
            }
        }
    

        // 
        private void HandleYanJiaoSkill(short[] officeGeneralIdArray, General general)
        {
            foreach (short otherGeneralId in officeGeneralIdArray)
            {
                General otherGeneral = GeneralListCache.GetGeneral(otherGeneralId);
                otherGeneral.AddIqExp((byte)Random.Range(1, general.wisdom / 10));
            }
        }



    

        /// <summary>
        /// 处理自动治理所有城池的逻辑
        /// </summary>
        private void AutoManageCities(Action onComplete)
        {
            AITurn.AutoInteriorAllCity();
            onComplete?.Invoke();
        }

    
        private void UpdateAlliances(Action onComplete)
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
            onComplete?.Invoke();
        }

        //TODO
        /// <summary>
        /// 处理已经发掘的人才将领的移动逻辑
        /// </summary>
        void TalentGenMove(Action onComplete)
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

                        if (city.ownerID > 0)
                        {
                            General cityKing = GeneralListCache.GetGeneral(city.ownerID);

                            if (ev)
                            {
                                // 如果找到符合条件的国家
                                Country evCountry = CountryListCache.GetCountryByCountryId(evCountryId);
                                if (evCountry != null && evCountry.countryKingId != city.ownerID)
                                {
                                    // 遍历符合条件的城池，并进行在野将领移动
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
                                // 如果将领的阶段差距大于5，或者城池将领数量大于等于10
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
            onComplete?.Invoke();
        }


        /// <summary>
        /// 获取在野将领可以移动到的目标城池
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

                        // 检查城池中的将领
                        for (index = 0; index < city.GetCityOfficerNum(); index++)
                        {
                            if (city.GetOfficerIds()[index] == general.generalId)
                            {
                                cityInfoString += city.cityName;
                                n++;
                            }
                        }

                        // 检查城池中的对手将领
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