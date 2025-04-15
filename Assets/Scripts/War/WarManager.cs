using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using GameClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace War
{
    [Serializable]
    public class WarManager : MonoBehaviour
    {
        private static WarManager _instance;
		
        public static WarManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<WarManager>(); // 在场景中查找 WarInfo
                    if (_instance == null)
                    {
                        Debug.LogError("WarManager单例为空，请确保已添加到场景中");
                    }
                }
                return _instance;
            }
        }

        // 指定的允许保留的场景
        private readonly List<string> _allowedScenes = new() { "WarScene", "PreBattleScene", "BattleScene", "SoloScene" };

        public WarState warState;// 战争状态
        public BattleState battleState;// 战斗状态
        public byte curWarCityId;// 当前战争发生的城市ID
        public byte departureCity;// 出发城池ID
        public byte day;// 天数
        public bool isHmDef;
		
        public UnitObj hmUnitObj;
        public UnitObj aiUnitObj;
        public List<UnitObj> aiUnits;
        public List<UnitObj> hmUnits;
        public static Dictionary<Vector2Int, UnitObj> UnitDictionary = new Dictionary<Vector2Int, UnitObj>();//存储所有units的位置和实例
		
        public Dictionary<short,UnitData> unitDataList = new Dictionary<short, UnitData>();//存储所有units的数据
		
        public byte[,] warMap = new byte[19, 32];
        public Vector2Int cityPos;//防守方零号为城池位置
        
        public short aiKingId;// AI方君主ID
        public short aiGold;// AI金钱
        public short aiFood;// AI粮食
        public byte aiIndex;
		
        public short hmKingId;// 玩家君主ID
        public short hmGold;// 玩家金钱
        public short hmFood;// 玩家粮食

        public short loserId;
        public byte planIndex;
        public string planResult;

        public bool isBattle;
        public AIWarStateMachine.WarStateMachine aiWarStateMachine;
        public Vector3 cameraPosition;
        public event Action OnWarOver;
        
        // Awake 方法，确保单例实例并跨场景保留
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject); // 跨场景保留
                
                //TODO 待删除调试战争信息
                DataManagement.Instance.LoadAndInitializeData();
                ReadMapData();
                
                isBattle = false;
                GameInfo.playerCountryId = 1;
                GameInfo.curTurnCountryId = 10;
                GameInfo.PlayingState = GameState.AIvsPlayer;
                GameInfo.doCityId = 10;
                GameInfo.targetCityId = 19;
                GameInfo.targetGeneralIds = new List<short>() { 26, 34,44,55,9,10,6,52,14,32 };
                GameInfo.optionalGeneralIds.Add(9999);
                GameInfo.optionalGeneralIds.Add(9999);
            }
            else
            {
                Destroy(gameObject); // 防止重复实例
            }
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
        // 检查场景是否允许保留
        public void CheckSceneValidity()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (!_allowedScenes.Contains(currentSceneName))
            {
                Debug.Log($"当前场景 {currentSceneName} 不允许保留 WarManager，清理数据...");
                Destroy(gameObject); // 销毁实例
            }
        }
		
        // 在场景加载完成后检查合法性
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckSceneValidity();
            if (scene.name == "WarScene" && isBattle)
            {
                ResumeWar();
            }
            else if (scene.name == "WarScene" && !isBattle)
            {
                WarPrepare(GameInfo.PlayingState == GameState.AIvsPlayer, GameInfo.targetCityId, GameInfo.doCityId, GameInfo.targetGeneralIds, GameInfo.optionalGeneralIds[0], GameInfo.optionalGeneralIds[1]);
                StartTurn();
                if (PlayerPrefs.GetFloat("bgmVolume") > 0)
                {
                    SoundManager.Instance.PlayBGM("3");
                    Debug.Log("播放音乐2");
                }
                else
                {
                    SoundManager.Instance.StopBGM();
                }
            }
        }
        
        
        
        /*private void Start()
        {
            if (isBattle)
            {
                ResumeWar();
            }
            else
            {
                StartCoroutine(DataManagement.LoadAllFormations());
                WarPrepare(GameInfo.PlayingState == GameState.AIvsPlayer, GameInfo.targetCityId, GameInfo.doCityId, GameInfo.targetGeneralIds, GameInfo.optionalGeneralIds[0], GameInfo.optionalGeneralIds[1]);
                StartTurn();
                if (PlayerPrefs.GetFloat("bgmVolume") > 0)
                {
                    SoundManager.Instance.PlayBGM("3");
                    Debug.Log("播放音乐2");
                }
                else
                {
                    SoundManager.Instance.StopBGM();
                }
                /*isBattle = false;
                MapManager.LoadMap();
                Debug.Log("开始战争");
                GameInfo.PlayingState = GameState.AIvsPlayer;
                GameInfo.targetCityId = 18;
                GameInfo.targetGeneralIds = new List<short>() { 26, 34,44,55,9,10,6,52,14,32 };
                GameInfo.optionalGeneralIds.Add(9999);
                GameInfo.optionalGeneralIds.Add(9999);
                WarPrepare(GameInfo.PlayingState == GameState.AIvsPlayer, GameInfo.targetCityId, GameInfo.doCityId, GameInfo.targetGeneralIds, GameInfo.optionalGeneralIds[0], GameInfo.optionalGeneralIds[1]);
                StartCoroutine(ExecuteDayLoop());#1#
            }
            Debug.Log("战争场景加载完成");
        }
        */
        
        public static UnitObj GetUnitByPos(Vector2Int pos)
        {
            if (UnitDictionary.TryGetValue(pos, out var unit))
            {
                return unit;
            }
            Debug.LogError($"未找到{pos.x},{pos.y}的单位");
            return null;
        }
        
        void ResumeWar()
        {
            // 恢复单位
            MapManager.LoadMap();
            MapManager.Instance.RestoreUnitsPosition();
            HandleBattleDieAndCaptured();
            isBattle = false;
            Debug.Log("恢复战争");
        }
        
        public void WarPrepare(bool isAIAtk, byte curWarCityID, byte departureCityID, List<short> attackerIds, short food, short gold)
        {
            day = 1;
            isHmDef = isAIAtk;
            curWarCityId = curWarCityID;
            departureCity = departureCityID;
            hmKingId = CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId; // 获取玩家君主ID
            City curCity = CityListCache.GetCityByCityId(curWarCityId); // 获取当前战争城市对象
            City departureATKCity = CityListCache.GetCityByCityId(departureCityID); // 获取出发城市对象
            List<short> aiGeneralIds = new List<short>();
            List<short> hmGeneralIds = new List<short>();
            foreach (var id in attackerIds)
            {
                departureATKCity.RemoveOfficerId(id);// 移除进攻将军
            }
            if (isHmDef)//AI进攻玩家防守
            {
                warState = WarState.AITurn; // 设置战争状态为AI回合
                aiKingId = CountryListCache.GetCountryByCountryId(GameInfo.curTurnCountryId).countryKingId; // 获取AI君主ID
                // 设置玩家和AI的战斗资源
                hmGold = curCity.GetMoney();  // 玩家资金
                hmFood = curCity.GetFood();  // 玩家粮食
                aiGold = gold;  // AI资金
                aiFood = food;  // AI粮食
                
                // 设置AI参战将军ID
                hmGeneralIds = curCity.GetOfficerIds().ToList(); // 获取当前城市的将领ID列表
                aiGeneralIds.AddRange(attackerIds); // 获取AI将领ID列表将进攻方输入
                if (aiGeneralIds.Count < 10)
                {
                    byte[] relatedCityIds = curCity.connectCityId.Concat(departureATKCity.connectCityId).ToArray();
                    foreach (var otherCityId in relatedCityIds)
                    {
                        City otherCity = CityListCache.GetCityByCityId(otherCityId); // 获取出发城市的连接其他城市对象
                        if (otherCity.cityBelongKing == aiKingId)
                        {
                            // 如果其他城市中的将军数量少于2，且AI选择的将军数量小于10，继续选择将军
                            while (otherCity.GetCityOfficerNum()  < 2 && aiGeneralIds.Count < 10)
                            {
                                short generalId = otherCity.GetMaxBattlePowerGeneralId();  // 获取战斗力最高的将军

                                // 如果将军是AI国王
                                if (generalId == aiKingId)
                                {
                                    short otherGeneralId = aiGeneralIds[0]; // 存储第一位将军的ID
                                    aiGeneralIds[0] = generalId; // 替换第一位将军
                                    aiGeneralIds.Add(otherGeneralId); // 将原来的第一位将军添加到列表末尾
                                }
                                else if (generalId > 0)
                                {
                                    aiGeneralIds.Add(generalId); // 正确添加元素到列表
                                }

                                otherCity.RemoveOfficerId(generalId);  // 移除将军
                            }
                            otherCity.AppointmentPrefect();  // 任命城主
                        }
                    }
                }
            }
            else//玩家进攻AI防守
            {
                warState = WarState.PlayerTurn; // 设置战争状态为玩家回合
                aiKingId = CityListCache.GetCityByCityId(curWarCityId).cityBelongKing; // 获取当前城市所属君主ID
                // 设置玩家的战斗资源
                hmGold = gold;  // 玩家资金
                hmFood = food;  // 玩家粮食
                aiGold = curCity.GetMoney();  // AI资金
                aiFood = curCity.GetFood();  // AI粮食
                // 设置A玩家和I参战将军ID
                hmGeneralIds.AddRange(attackerIds); // 获取玩家将领ID列表将进攻方输入
                aiGeneralIds = curCity.GetOfficerIds().ToList(); // 获取当前城市的将领ID列表
            }
            MapManager.Instance.PrepareWarStance(hmGeneralIds, aiGeneralIds); //TODO 准备战争放置单位
        }
    
        // 单方回合开始阶段，玩家为真
        void StartTurn()
        {
            if (warState == WarState.PlayerTurn)
            {
                Debug.Log("玩家回合开始");
                ExecThreaten(hmUnits, aiUnits);
                SetHumanMoveBonus();
                UIWar.Instance.DisplayWarMenu();
            }
            else if (warState == WarState.AITurn)
            {
                Debug.Log("敌方回合开始");
                ExecThreaten(aiUnits, hmUnits);
                aiIndex = 0;
                SetAIMoveBonus();
            }

            StartCoroutine(ExecuteSingleTurn());
        }

        private IEnumerator ExecuteSingleTurn()
        {
            if (warState == WarState.PlayerTurn)
            {
                yield return new WaitUntil(() => warState == WarState.EndRound);
                warState = WarState.PlayerTurn;
            }
            else if (warState == WarState.AITurn)
            {
                yield return AIWarTurn();
                yield return new WaitUntil(() => warState == WarState.EndRound);
                warState = WarState.AITurn;
            }

            EndTurn();
            yield return null;
        }

        // 单方回合结束阶段粮食结算
        private void EndTurn()
        {
            if (warState == WarState.PlayerTurn)
            {
                UpdateTrapStates(hmUnits);

                EatFoodEveryDay(true);
                if (hmFood == 0)
                {
                    UIWar.Instance.uiTips.ShowNoticeTipsWithConfirm("粮草枯竭,士卒饥疲,难以久持,暂且退兵!", () =>
                    {
                        UIWar.Instance.uiRetreatPanel.MustRetreat(PlayerWithdraw);
                    } );
                }
                else // 粮足则切换回合
                {
                    Debug.Log("玩家回合结束");
                    warState = WarState.AITurn;
                    StartTurn();
                }

                if (isHmDef) // 玩家为后手
                {
                    day++;
                    Debug.Log($"第 {day} 天结束");
                    if (day > 30)// 战斗满 30 天，AI撤退
                    {
                        AIWar.AIFollowRetreat();
                    }
                }
            }
            else if (warState == WarState.AITurn)
            {
                UpdateTrapStates(aiUnits);
            
                EatFoodEveryDay(false);
                if (aiFood == 0)
                {
                    AIWar.AIFollowRetreat();
                }
                else
                {
                    Debug.Log("敌方回合结束");
                    warState = WarState.PlayerTurn;
                    StartTurn();
                }
                
                if (!isHmDef) // AI为后手
                {
                    day++;
                    Debug.Log($"第 {day} 天结束");
                    if (day > 30)// 战斗满 30 天，玩家撤退
                    {
                        UIWar.Instance.uiTips.ShowNoticeTipsWithConfirm("久战不利,恐生变故,宜早退兵,徐图后计!", () =>
                        {
                            UIWar.Instance.uiRetreatPanel.MustRetreat(PlayerWithdraw);
                        } );
                    }
                }
            }   
        }

        public IEnumerator AIWarTurn()
        {
            Debug.Log("AI行动中...");
            // 对 AI 单位没有被俘虏且的单位移动顺序进行排序
            
            List<UnitObj> aiOrders = SortAIUnits();
            Debug.Log("排序后的AI单位列表：" + string.Join(",", aiOrders.Select(unit => GeneralListCache.GetGeneral(unit.genID).generalName)));
            for (var i = 0; i < aiOrders.Count; i++)
            {
                aiUnitObj = aiOrders[i];
                if (aiWarStateMachine == null)
                {
                    aiWarStateMachine = new AIWarStateMachine.WarStateMachine(); // 假设这个函数根据国家ID或创建AI状态管理器
                }
                aiWarStateMachine.Init(aiUnitObj.data);
            
                yield return new WaitUntil(() => aiWarStateMachine.IsFinished);  // 更新AI结束状态
                yield return new WaitForSeconds(0.5f);  // 等待1s
            }

            AIEndTurn();
        }

        private void Update()
        {
            aiWarStateMachine?.UpdateState();
        }

        /// <summary>
        /// AI单位排序方法
        /// </summary>
        /// <returns></returns>
        List<UnitObj> SortAIUnits()
        {
            List<(int score, UnitObj unit)> sortedUnits = new List<(int, UnitObj)>();

            foreach (var aiUnit in aiUnits)
            {
                if (aiUnit.unitState == UnitState.Captive) continue;
                AIWar aiWar = new AIWar(aiUnit.data);
                if (aiWar.IsAIUnitRetreat())
                {
                    sortedUnits.Add((10000, aiUnit)); // 如果AI单位准备撤退，则为第一个
                    continue;
                }
                List<(Vector2Int position, byte cost)> movablePath = MapManager.GetPathToTarget(AIWar.AITargetPosition(),aiUnit.data, true);
                int maxScore = 0;

                // 计算分数
                foreach (var hmUnit in hmUnits)
                {
                    int score = 100 - movablePath.Count - MapManager.ManhattanDistance(aiUnit.arrayPos, hmUnit.arrayPos);
                    maxScore = Math.Max(maxScore, score);
                }

                // 主将处理
                if (aiUnit.IsCommander)
                {
                    // 检查是否靠近防守位置
                    foreach (var pair in movablePath)
                    {
                        Vector2Int cell = pair.Item1;
                        if (MapManager.IsAdjacent(cell, cityPos))
                        {
                            maxScore = 0; // 如果靠近防守位置，分数设为0
                            break; // 找到后可以直接跳出循环
                        }
                    }
                }

                sortedUnits.Add((maxScore, aiUnit));
            }

            // 按照分数降序排序
            sortedUnits.Sort((a, b) => b.score.CompareTo(a.score));

            // 使用LINQ简化转换
            return sortedUnits.Select(tuple => tuple.unit).ToList();
        }
        
        void AIEndTurn()
        {
            byte count = 0;//完成行动计数器
            foreach (var unit in aiUnits)
            {
                if (unit.unitState == UnitState.Idle || unit.unitState == UnitState.Captive)
                {
                    count++;
                }
            }
            
            // 如果所有单位都已经完成行动
            if (count == aiUnits.Count)
            {
                aiUnitObj = null;
                warState = WarState.EndRound;
            }
            Debug.Log($"已完成{count}个总共{aiUnits.Count}个AI单位");
        }
    
        // 设置AI行动力数组
        public void SetAIMoveBonus()
        {
            foreach (var unit in aiUnits)
            {
                if (unit.unitState is UnitState.None or UnitState.Idle)
                {
                    unit.SetUnitState(UnitState.None);
                    unit.SetIsMoved(false);
                }
                else
                {
                    continue;
                }
                General general = GeneralListCache.GetGeneral(unit.genID);
                if (general != null)
                {
                    int bl = general.generalSoldier / 300;
                    switch (bl)
                    {
                        case 9:
                        case 10:
                            unit.SetMoveBonus(20);
                            break;
                        case 7:
                        case 8:
                            unit.SetMoveBonus(18);
                            break;
                        case 6:
                            unit.SetMoveBonus(16);
                            break;
                        case 5:
                            unit.SetMoveBonus(15);
                            break;
                        case 3:
                        case 4:
                            unit.SetMoveBonus(14);
                            break;
                        case 2:
                            unit.SetMoveBonus(12);
                            break;
                        case 0:
                        case 1:
                            unit.SetMoveBonus(10);
                            break;
                    }
                }
            }
            Debug.Log("AI行动力更新");
        }


        public void SetHumanMoveBonus()
        {
            foreach (var unit in hmUnits)
            {
                if (unit.unitState is UnitState.None or UnitState.Idle)
                {
                    unit.SetUnitState(UnitState.None);
                    unit.SetIsMoved(false);
                }
                else
                {
                    continue;
                }
                General general = GeneralListCache.GetGeneral(unit.genID);
                if (general != null)
                {
                    int bl = general.generalSoldier / 300;
                    switch (bl)
                    {
                        case 9:
                        case 10:
                            unit.SetMoveBonus(20);
                            break;
                        case 7:
                        case 8:
                            unit.SetMoveBonus(18);
                            break;
                        case 6:
                            unit.SetMoveBonus(16);
                            break;
                        case 5:
                            unit.SetMoveBonus(15);
                            break;
                        case 4:
                            unit.SetMoveBonus(14);
                            break;
                        case 3:
                            unit.SetMoveBonus(12);
                            break;
                        case 2:
                            unit.SetMoveBonus(10);
                            break;
                        case 1:
                            unit.SetMoveBonus(8);
                            break;
                        case 0:
                            unit.SetMoveBonus(7);
                            break;
                    }
                }
            }
            Debug.Log("玩家机动力更新");
        }

    
        // 特技恐吓方法
        private void ExecThreaten(List<UnitObj> doUnits, List<UnitObj> beUnits)
        {
            foreach (var doUnit in doUnits)
            {
                if (doUnit.unitState != UnitState.Captive)
                {
                    General doGeneral = GeneralListCache.GetGeneral(doUnit.genID);
                    if (doGeneral.HasSkill(5, 2))//是否拥有恐吓技能
                    {
                        if (doGeneral.generalSoldier >= 300)
                            foreach (var beUnit in beUnits)
                            {
                                int loss = doGeneral.generalSoldier / 30;
                                if (beUnit.unitState != UnitState.Captive)
                                {
                                    General beGeneral = GeneralListCache.GetGeneral(beUnit.genID);
                                    if (!beGeneral.HasSkill(5, 2) && beGeneral.generalSoldier >= 600 && doGeneral.force >= beGeneral.force && Random.Range(0, 100) <= 50)
                                    {
                                        loss += Random.Range(0, doGeneral.force);
                                        beGeneral.SubSoldier((short)loss);
                                        GetUnitByID(beUnit.genID).GetArmySprite();
                                        Debug.Log($"{doGeneral.generalName}对{beGeneral.generalName}发动恐吓，减少士兵{loss}");
                                        doGeneral.Addexperience((int)(loss * 1.2f));
                                    }
                                }
                            }
                    }
                }
            }
            Debug.Log("恐吓阶段结束");
        }
    
    
    
        public UnitObj GetUnitByID(short generalID)
        {
            UnitObj unitObj = hmUnits.FirstOrDefault(t => t.genID == generalID);
            if (unitObj == null)
            {
                unitObj = aiUnits.FirstOrDefault(t => t.genID == generalID);
                if (unitObj == null)
                {
                    Debug.LogError($"未能找到ID为{generalID}的单位");
                }
            }
            return unitObj;
        }
    
        public UnitData GetUnitDataByID(short generalID)
        {
            // 检查是否存在该ID的数据
            if (unitDataList.TryGetValue(generalID, out var data))
            {
                return data;
            }
            Debug.LogError($"未找到 ID 为 {generalID} 的 UnitData！");
            return null;
        }

        //更新虚兵、连环、奇门遁甲等计策的被困效果
        void UpdateTrapStates(List<UnitObj> units)
        {
            foreach (var unit in units)
            {
                if (unit.unitState != UnitState.Trapped)
                {
                    continue;
                }
                if (unit.trappedDay > 0)
                {
                    if (unit.trappedDay > 3)
                    {
                        General general = GeneralListCache.GetGeneral(unit.genID);
                        general.SubSoldier((short)(general.generalSoldier/ 8 + Random.Range(0, 300 - general.lead)));
                        if (general.generalSoldier < 0)
                            general.generalSoldier = 0;
                    }
                    unit.SubTrappedDay(1);
                    
                }
                else
                {
                    unit.SetUnitState(UnitState.None);
                    unit.SetTrappedDay(0);
                }
            }
        }
        


        // 每日粮食是否足够消耗
        void EatFoodEveryDay(bool isPlayer)
        {
            List<UnitObj> units = isPlayer ? hmUnits : aiUnits;
            short eatFood = (short)(-(units.Aggregate<UnitObj, short>(0, (current, unit) => (short)(current + GeneralListCache.GetGeneral(unit.genID).generalSoldier)) - 1) / 250 + 1);
            ChangeWarFood(eatFood, isPlayer);
        }

        public void ChangeWarFood(short value, bool isPlayer)
        {
            if (isPlayer)
            {
                hmFood = (short)Mathf.Clamp(hmFood + value, 0, 30000);
            }
            else
            {
                aiFood = (short)Mathf.Clamp(aiFood + value, 0, 30000);
            }
        }

        public void ChangeWarGold(short value, bool isPlayer)
        {
            if (isPlayer)
            {
                hmGold = (short)Mathf.Clamp(hmGold + value, 0, 30000);
            }
            else
            {
                aiGold = (short)Mathf.Clamp(aiGold + value, 0, 30000);
            }
        }
    
        
    
        /// <summary>
        /// 获取可以撤退的城池列表，判断退路
        /// </summary>
        /// <param name="kingId">城池所属君主ID</param>
        /// <returns>可以撤退的城池列表</returns>
        public static List<byte> GetRetreatCityList(short kingId)
        {
            List<byte> retreatCityId = new List<byte>();

            // 获取当前城市，检查是否为 null
            City curCity = CityListCache.GetCityByCityId(Instance.curWarCityId);
            if (curCity == null)
            {
                Debug.LogError($"当前城市 ID 无效：{Instance.curWarCityId}");
                return retreatCityId;
            }

            // 检查连接城市 ID 数组是否为空
            if (curCity.connectCityId == null || curCity.connectCityId.Length == 0)
            {
                Debug.LogWarning($"城市 {curCity.cityId} 没有连接的城市");
                return retreatCityId;
            }

            // 遍历连接的城市
            for (byte i = 0; i < curCity.connectCityId.Length; i++)
            {
                byte cityId = curCity.connectCityId[i];

                // 检查连接城市是否存在
                City city = CityListCache.GetCityByCityId(cityId);
                if (city == null)
                {
                    Debug.LogWarning($"连接的城市 ID {cityId} 无效，跳过处理");
                    continue;
                }

                // 检查城市归属和将领数量
                if (kingId == city.cityBelongKing && city.GetCityOfficerNum() < 10)
                {
                    retreatCityId.Add(cityId);
                }
            }

            return retreatCityId;
        }
   
        /// <summary>
        /// 将领单位撤退
        /// </summary>
        /// <param name="unitObj">撤退的单位</param>
        /// <param name="cityId">城池ID</param>
        /// <param name="belongKing">所属君主ID</param>
        public static void RetreatGeneralToCity(UnitObj unitObj, byte cityId, short belongKing)
        {
            City curWarCity = CityListCache.GetCityByCityId(Instance.curWarCityId);
            City city = CityListCache.GetCityByCityId(cityId);
            short generalId = unitObj.genID;
            curWarCity.RemoveOfficerId(generalId);
            if (cityId <= 0 || city == null)
            {
                curWarCity.AddNotFoundGeneralId(generalId);
                Debug.LogError($"城市 ID 无效：{cityId}");
                return;
            }
            if (city.cityBelongKing == 0)//如果为空城
            {
                Country country = CountryListCache.GetCountryByKingId(belongKing);
                country.AddCity(cityId);
            }
            else if (generalId == belongKing)//如果为君主
            {
                city.prefectId = generalId;
            }
            if (unitObj.unitState == UnitState.Captive)
            {
                city.AddCapture(generalId);
            }
            else if (city.GetCityOfficerNum() < 10)
            {
                city.AddOfficeGeneralId(generalId);
            }
            else
            {
                curWarCity.AddNotFoundGeneralId(generalId);
            }
            city.AppointmentPrefect();
        }
    
        /// <summary>
        /// 获取可以撤退的城池列表，与玩家不同之处在于可以撤退邻近的空城
        /// </summary>
        /// <param name="kingId">城池所属君主ID</param>
        /// <returns>可以撤退的城池列表</returns>
        public static List<byte> AIGetRetreatCityList(short kingId)
        {
            List<byte> retreatCityId = new List<byte>();

            // 获取当前城市，检查是否为 null
            City curCity = CityListCache.GetCityByCityId(WarManager.Instance.curWarCityId);
            if (curCity == null)
            {
                Debug.LogError($"当前城市 ID 无效：{WarManager.Instance.curWarCityId}");
                return retreatCityId;
            }

            // 检查连接城市 ID 数组是否为空
            if (curCity.connectCityId == null || curCity.connectCityId.Length == 0)
            {
                Debug.LogWarning($"城市 {curCity.cityId} 没有连接的城市");
                return retreatCityId;
            }

            // 遍历连接的城市
            for (byte i = 0; i < curCity.connectCityId.Length; i++)
            {
                byte cityId = (byte)curCity.connectCityId[i];

                // 检查连接城市是否存在
                City city = CityListCache.GetCityByCityId(cityId);
                if (city == null)
                {
                    Debug.LogWarning($"连接的城市 ID {cityId} 无效，跳过处理");
                    continue;
                }

                // 检查城市归属和将领数量与玩家不同之处在于可以撤退空城
                if ((kingId == city.cityBelongKing && city.GetCityOfficerNum() < 10) || city.cityBelongKing == 0)
                {
                    retreatCityId.Add(cityId);
                }
            }
            return retreatCityId;
        }

        /// <summary>
        /// 胜利方处理
        /// </summary>
        /// <param name="isHmWin">是否是玩家胜利</param>
        /// <param name="loserHaveCommander">败方是否损失了主将</param>
        public void AfterWarSettlement(bool isHmWin, bool loserHaveCommander)
        {
            warState = isHmWin ? WarState.WarWin : WarState.WarLose;
            short winKingId = isHmWin ? hmKingId : aiKingId;
            short loseKingId = isHmWin ? aiKingId : hmKingId;
            short winGold = isHmWin ? hmGold : aiGold;
            short winFood = isHmWin ? hmFood : aiFood;
            short loseGold = isHmWin ? aiGold : hmGold;
            short loseFood = isHmWin ? aiFood : hmFood;
            List<UnitObj> units = isHmWin ? hmUnits : aiUnits;
            Country loseCountry = CountryListCache.GetCountryByKingId(loseKingId);
            City city = CityListCache.GetCityByCityId(curWarCityId);
            city.SetMoney(winGold);
            city.SetFood(winFood);
            if (loserHaveCommander)
            {
                city.AddGold(loseGold);
                city.AddFood(loseFood);
            }
            loseCountry.RemoveCity(curWarCityId);
            if (loseCountry.IsDestroyed()) // 失败方城池为零则势力灭亡，其余武将被俘
            {
                List<UnitObj> enemyUnits = isHmWin ? aiUnits : hmUnits;
                foreach (var enemyUnit in enemyUnits)
                {
                    city.AddCapture(enemyUnit.genID);
                }
                GameInfo.countryDieTips = isHmWin ? (byte)4 : (byte)3;
            }
            else if (isHmWin) // 如果AI失败方有城池可以继承
            {
                //如果有城池可以继位
                aiKingId = loseCountry.Inherit();
                GameInfo.countryDieTips = 1;
                GameInfo.ShowInfo =
                    $"{GameInfo.chooseGeneralName} 死亡, 新君主 {GeneralListCache.GetGeneral(aiKingId).generalName} 继位!";

            }
            CountryListCache.GetCountryByKingId(winKingId).AddCity(curWarCityId);// 胜者得城
            city.ClearAllOfficeGeneral();// 胜者将领入城
            foreach (var unit in units)
            {
                if (unit.unitState == UnitState.Captive)
                {
                    city.AddCapture(unit.genID);
                }
                else if (city.GetCityOfficerNum() < 10)
                {
                    city.AddOfficeGeneralId(unit.genID);
                }
                else
                {
                    city.AddNotFoundGeneralId(unit.genID);
                }
            }
            city.AppointmentPrefect();
        }

    
    
    
    
    
        /*/// <summary>
        /// 失败方主将被抓或死亡处理撤退
        /// </summary>
        /// <param name="isPlayer">是否是玩家撤退</param>
        /// <param name="loseCommander">是否因失去主将撤退</param>
        public void HandleMustRetreat(bool isPlayer, bool loseCommander)
        {
            List <UnitObj> units = isPlayer ? HMUnits : AIUnits;
            byte byte0 = 0;
            City curWarCity = CityListCache.GetCityByCityId(curWarCityId);
            foreach (var unitObj in units)
            {
                if (unitObj.unitState != UnitState.Captive)
                {
                
                }
            }
            for (byte i = 0; i < aiGeneralNum_inWar; i++)
            {
                if (aiUnitTrapped[i] == 0 || aiUnitTrapped[i] > 3)
                {
                    byte0 = (byte)(byte0 + 1);
                    aiGeneralId_inWar[byte0] = aiGeneralId_inWar[i];
                }
            }
            aiGeneralNum_inWar = byte0;
            for (byte byte2 = 0; byte2 < hmGeneralNum; byte2 = (byte)(byte2 + 1))
            {
                if (hmUnitTrapped[byte2] == 2)// 玩家武将被俘获被笼络
                {
                    short userGeneralId = hmGeneralIdArray[byte2];
                    if (aiGeneralNum_inWar < 10)// 城池有空位则变为AI将领否则下野
                    {
                        RandomSetGeneralLoyalty(userGeneralId);
                        aiGeneralNum_inWar = (byte)(aiGeneralNum_inWar + 1);
                        aiGeneralId_inWar[aiGeneralNum_inWar] = userGeneralId;
                    }
                    else
                    {
                        curWarCity.AddNotFoundGeneralId(userGeneralId);
                    }
                }
            }
            curWarCity.ClearAllOfficeGeneral();
            for (int i = 0; i < aiGeneralNum_inWar; i++)
                curWarCity.AddOfficeGeneralId(aiGeneralId_inWar[i]);
            curWarCity.AppointmentPrefect();
            curWarCity.SetMoney(aiGold);
            curWarCity.SetFood(aiFood);
            if (mainGeneralLost)//主将被抓或死亡
            {
                curWarCity.AddMoney(hmGold);
                curWarCity.AddFood(hmFood);
            }
            if (GameInfo.countryDieTips > 1)
            {
                retreatCause = 8;
                warState = WarState.None;
                if (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).IsEndangered())
                    GameInfo.countryDieTips = 3;
            }
        }*/

    

    

    
        /// <summary>
        /// 是否触发军魂技能光环效果
        /// </summary>
        /// <param name="genId">要检测的武将ID</param>
        /// <param name="isPlayer">是否为玩家</param>
        /// <returns>是否触发军魂技能光环效果</returns>
        public bool TriggerJunHunHalo(short genId, bool isPlayer)
        {
            List<UnitObj> units = isPlayer ? hmUnits : aiUnits;
            UnitObj unitObj = units.FirstOrDefault(t => t.genID == genId);
            if (unitObj != null)
            {
                foreach (var friend in units)
                {
                    if (friend.genID != genId && friend.unitState != UnitState.Captive)
                    {
                        General otherGeneral = GeneralListCache.GetGeneral(friend.genID);
                        if (otherGeneral.HasSkill(3, 8) && 
                            MapManager.IsAdjacent(unitObj.arrayPos, friend.arrayPos)) return true; //军魂光环被动
                    }
                }
            }
            
            return false;
        }

        public void AIWithdraw()
        {
            Debug.Log("电脑撤退");
            AfterWarSettlement(true, false);
            UIWar.Instance.uiTips.ShowNoticeTipsWithConfirm("敌军全军逃窜!", WarOver);
            Debug.Log("敌方撤军跳转场景");
        }
        
        // 玩家主动撤退完成后结束战争
        public void PlayerWithdraw()
        {
            Debug.Log("玩家撤退完成");
            AfterWarSettlement(false, false);
            UIWar.Instance.uiTips.ShowNoticeTipsWithConfirm("我方全军撤退!", WarOver);
            Debug.Log("我方撤军跳转场景");
        }

        // 回到大战场界面后处理小战场的将军被俘、死亡的结果
        public void HandleBattleDieAndCaptured()
        {
            short generalId = loserId;
            if (generalId == 0)
            {
                return;
            }

            loserId = 0;
            UnitData unit = GetUnitDataByID(generalId);
            GameInfo.chooseGeneralName = GeneralListCache.GetGeneral(generalId).generalName;
            switch (battleState)
            {
                case BattleState.AICaptured:  // 处理AI将军被俘虏逻辑
                    if (unit.isCommander)  // 如果AI主将被擒
                    {
                        AIWar.AIFollowRetreat();  // AI撤退
                        AfterWarSettlement(true, true);
                        UIWar.Instance.uiTips.ShowNoticeTipsWithConfirm("敌军主将被擒，溃散而逃", WarOver);
                    }
                    break;  
                case BattleState.HMCaptured:  // 处理玩家将军被俘虏逻辑
                    if (unit.isCommander)  // 如果玩家主将被擒
                    {
                        UIWar.Instance.uiRetreatPanel.MustRetreat(() =>
                        {
                            AfterWarSettlement(false, true);
                            WarOver();
                        });
                    }
                    break;
                case BattleState.AIDie:  // 处理AI将军死亡逻辑
                    if (unit.isCommander)//AI主将死亡
                    {
                        if (generalId == aiKingId)  // 如果AI将军为国王
                        {
                            Country country = CountryListCache.GetCountryByKingId(aiKingId);
                            if (country.IsDestroyed())
                            {
                                GameInfo.countryDieTips = 4;
                                SceneManager.LoadSceneAsync("GlobalScene");
                                break;
                            }
                            GameInfo.countryDieTips = 1;
                            short newKingGeneralId = CountryListCache.GetCountryByKingId(aiKingId).Inherit();  // 获取新的国王
                        
                            aiKingId = newKingGeneralId;  // 更新AI国王ID
                        }
                        AIWar.AIFollowRetreat();  // AI撤退
                        AfterWarSettlement(false,true);
                        UIWar.Instance.uiTips.ShowNoticeTipsWithConfirm("敌军主将阵亡，已溃不成军", WarOver);
                    }
                    break;
                case BattleState.HMDie:  // 处理玩家将军死亡逻辑
                    if (unit.isCommander)//玩家主将死亡
                    {
                        if (generalId == hmKingId)  // 如果玩家将军为君主
                        {
                            Country country = CountryListCache.GetCountryByKingId(hmKingId);
                            if (country.IsDestroyed())// 如果是最后一城君主死亡，玩家失败
                            {
                                GameInfo.countryDieTips = 3;
                                AfterWarSettlement(false,false);
                                WarOver();
                                break;
                            }

                            //如果还有城池可以继承
                            GameInfo.countryDieTips = 2;
                        } 
                        UIWar.Instance.uiRetreatPanel.MustRetreat(() =>
                        {
                            AfterWarSettlement(false,true);
                            WarOver();
                        });
                    }
                    break;
            }
        }
        
        public void ChangeUnitDataDieAndCaptured(short generalID)
        {
            UnitData unitData = GetUnitDataByID(generalID);
            loserId = generalID;
            switch (battleState)
            {
                case BattleState.AICaptured:  // 处理AI将军被俘虏逻辑
                    unitData.isPlayer = true;
                    unitData.unitState = UnitState.Captive;
                    break;  
                case BattleState.HMCaptured:  // 处理玩家将军被俘虏逻辑
                    unitData.isPlayer = false;
                    unitData.unitState = UnitState.Captive;
                    break;
                case BattleState.AIDie:  // 处理AI将军死亡逻辑
                    GeneralListCache.GeneralDie(generalID);
                    unitData.unitState = UnitState.Dead;
                    break;
                case BattleState.HMDie:  // 处理玩家将军死亡逻辑
                    GeneralListCache.GeneralDie(generalID);
                    unitData.unitState = UnitState.Dead;
                    break;
            }
            warMap[unitData.arrayPos.y, unitData.arrayPos.x] &= 0x3f; // 移除之前位置的占用标记
            MapManager.RemoveUnit(unitData.arrayPos);
            Debug.Log($"移除单位:{unitData.genID},坐标:{unitData.arrayPos.x},{unitData.arrayPos.y},新地图值:{warMap[unitData.arrayPos.y, unitData.arrayPos.x]}");

        }
        public void WarOver()
        {
            warState = WarState.None;
            OnWarOver?.Invoke();
            Debug.Log("战争结束");
            SceneManager.LoadSceneAsync("GlobalScene");
        }


        
    }
    
    public enum WarState
    {
        None,
        PlayerTurn,
        AITurn,
        EndRound,
        WarWin,
        WarLose,
        WarEvent,

        AIOccupy,
        AIRetreat,
        AICaptive,
        AIDie,
        AIStarve,

        PlayerRetreat,
        PlayerCaptive,
        PlayerDie,
        PlayerStarve,
        DayOut,

        Battle

    }

    public enum BattleState
    {
        None,
        PlayerTurn,
        AITurn,
        BattleWin,
        BattleLose,
        BattleOver,
        
        HMRetreat,
        AIRetreat,
        HMCaptured,
        AICaptured,
        HMDie,
        AIDie,
    }

    public enum SoloState
    {
        None,
        PlayerTurn,
        AITurn,
        PlayerWin,
        PlayerRetreat,
        PlayerSurrender,
        PlayerDie,
        PlayerLoss,
        AIRetreat,
        AISurrender,
        AIDie,
    }
}