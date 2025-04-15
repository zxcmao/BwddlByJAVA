using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using War;
using Random = UnityEngine.Random;

namespace Battle
{   
    public class BattleManager : MonoBehaviour
    {
        // 单例实例
        private static BattleManager _instance;

        // 指定的允许保留的场景
        private readonly List<string> _allowedScenes = new List<string>() { "PreBattleScene", "BattleScene", "SoloScene" };

        public static BattleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<BattleManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("BattleManager单例为空!请确保已添加到此场景中.");
                    }
                }
                return _instance;
            }
        }

        // 数据字段
        public bool isPaused; // 游戏是否暂停

        public BattleState battleState
        { get => WarManager.Instance.battleState; set => WarManager.Instance.battleState = value; }
        public General hmGeneral;
        public General aiGeneral;
        public byte[,] battleMap = new byte[7, 16];
        public byte battleTerrain;
        public byte formationIndex;
        public bool isHmDef;
        public byte hmTacticPoint; // 玩家计策点
        public byte aiTacticPoint; // AI计策点
        public Tactic hmTactic; // 玩家当前的战术
        public Tactic aiTactic; // AI当前的战术
        public ushort hmTacticTermination; // 玩家开始战术的部队索引
        public ushort aiTacticTermination; // AI开始战术的部队索引
        public short hmSoldierNum;
        public short aiSoldierNum;
        public List<Troop> hmTroops = new List<Troop>();
        public List<Troop> aiTroops = new List<Troop>();
        public short hmGeneralHurt;
        public short aiGeneralHurt;
        public short hmSoldierLoss;
        public short aiSoldierLoss;
        public short[] hmCAI_Num = new short[3]; // 玩家骑弓步各种兵力
        public short[] aiCAI_Num = new short[3];
        public ushort doneTroopsCount; // 执行过操作的部队数量 
        public byte totalTroopCount; // 双方部队数量
        public byte currentTroopIndex;
        public List<TroopData> hmTurnOrder = new List<TroopData>(); // 玩家部队行动顺序
        public List<TroopData> aiTurnOrder = new List<TroopData>(); // 敌方部队行动顺序
        public SoloState soloState; // 单挑对战状态
        public event Action OnBattleOver;
        
        // Awake 方法，确保单例实例并跨场景保留
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject); // 跨场景保留
            }
            else
            {
                Destroy(gameObject); // 防止重复实例
            }
            DataManagement.Instance.LoadAndInitializeData();
        }

        // 清理所有数据
        private void ClearData()
        {
            isPaused = false;
            //battleState = default;
            hmGeneral = null;
            aiGeneral = null;
            battleMap = new byte[7, 16];
            battleTerrain = 0;
            formationIndex = 0;
            isHmDef = false;
            hmTacticPoint = 0;
            aiTacticPoint = 0;
            hmTactic = null;
            aiTactic = null;
            hmTacticTermination = 0;
            aiTacticTermination = 0;
            hmSoldierNum = 0;
            aiSoldierNum = 0;
            hmTroops.Clear();
            aiTroops.Clear();
            hmGeneralHurt = 0;
            aiGeneralHurt = 0;
            hmSoldierLoss = 0;
            aiSoldierLoss = 0;
            hmCAI_Num = new short[3];
            aiCAI_Num = new short[3];
            doneTroopsCount = 0;
            totalTroopCount = 0;
            currentTroopIndex = 0;
            hmTurnOrder.Clear();
            aiTurnOrder.Clear();
            soloState = default;

            Debug.Log("BattleInfo数据已清理");
        }

        // 检查场景是否允许保留
        private void CheckSceneValidity()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (!_allowedScenes.Contains(currentSceneName))
            {
                Debug.Log($"当前场景 {currentSceneName} 不允许保留 BattleManager，清理数据...");
                ClearData();
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
            if (scene.name == "PreBattleScene")
            {
                StartCoroutine(DataManagement.LoadAllFormations());
                InitBattle();
            }
            else if (scene.name == "BattleScene")
            {
                if (soloState != SoloState.None)
                {
                    Debug.Log("单挑撤退回到小战场");
                    SoloRetreatBackBattle();
                }
                else
                {
                    StartManualBattle();
                }
            }
        }
        
        // TODO
        private void InitBattle()
        {
            Debug.Log($"开始战斗时{Time.time}和游戏速度{Time.timeScale}");
            hmGeneral = GeneralListCache.GetGeneral(WarManager.Instance.hmUnitObj.genID);
            aiGeneral = GeneralListCache.GetGeneral(WarManager.Instance.aiUnitObj.genID);
            isHmDef = WarManager.Instance.warState == WarState.AITurn;
            battleTerrain = isHmDef ? WarManager.Instance.hmUnitObj.Terrain : WarManager.Instance.aiUnitObj.Terrain;
            hmTacticPoint = (byte)(hmGeneral.IQ * 13 / 100);
            aiTacticPoint = (byte)(aiGeneral.IQ * 13 / 100);
            
            UIPreBattle.Instance.Init();
        }
        
        private byte GetTroopsNum(General general)
        {
            short soldierNum = general.generalSoldier;
            if(soldierNum <= 0) return 1;
            return (byte)((soldierNum - 1) / 300 + 2);
        }

        public void InitTroops(TroopData[] troopData, bool isPlayer)
        {
            // 获取对应的将军和部队信息
            General general = isPlayer ? hmGeneral : aiGeneral;
            byte[] sc = FloorManager.SetCAI(general, battleTerrain);
            byte troopCount = GetTroopsNum(general);
            short remainingHealth = general.generalSoldier;
            short captainHealth = (short)(general.curPhysical * 3);

            troopData[0].health = captainHealth;
            troopData[0].index = 0;
            if (isPlayer)
            {
                hmTurnOrder.Add(troopData[0]);
            }
            else
            {
                aiTurnOrder.Add(troopData[0]);
            }
            
            SetTroop(sc[0], troopData[1], ref remainingHealth, troopCount, 1);
            SetTroop(sc[1], troopData[2], ref remainingHealth, troopCount, sc[0] + 1);
            SetTroop(sc[2], troopData[3], ref remainingHealth, troopCount, sc[0] + sc[1] + 1);
        }
        
        private void SetTroop(int count, TroopData origin, ref short remainingHealth, byte troopCount, int startIndex)
        {
            for (int i = 0; i < count; i++)
            {
                if (troopCount - startIndex - i <= 0 || remainingHealth <= 0) break;

                short health = (short)(remainingHealth / (troopCount - startIndex - i));
                remainingHealth -= health;
                TroopData data = TroopData.Copy(origin);
                data.health = health;
                data.index = (byte)(startIndex + i);
                if (data.isPlayer)
                {
                    switch (data.troopType)
                    {
                        case TroopType.Cavalry:
                            hmCAI_Num[0] += health;
                            break;
                        case TroopType.Archer:
                            hmCAI_Num[1] += health;
                            break;
                        case TroopType.Infantry:
                            hmCAI_Num[2] += health;
                            break;
                    }
                    hmTurnOrder.Add(data);
                }
                else
                {
                    switch (data.troopType)
                    {
                        case TroopType.Cavalry:
                            aiCAI_Num[0] += health;
                            break;
                        case TroopType.Archer:
                            aiCAI_Num[1] += health;
                            break;
                        case TroopType.Infantry:
                            aiCAI_Num[2] += health;
                            break;
                    }
                    aiTurnOrder.Add(data);
                }
            }
        }
        
        /// <summary>
        /// 开始手动战斗                                       
        /// </summary>
        public void StartManualBattle()
        {
            // 设置将军 ID 和索引
            hmSoldierNum = hmGeneral.generalSoldier;
            aiSoldierNum = aiGeneral.generalSoldier;
            battleState = isHmDef ? BattleState.AITurn : BattleState.PlayerTurn;
            
            FloorManager.Instance.InitFloor();
            foreach (TroopData data in hmTurnOrder)
            {
                FloorManager.Instance.PutPosition(data);
            }
            foreach (TroopData data in aiTurnOrder)
            {
                FloorManager.Instance.PutPosition(data);
            }
            
            UIBattle.Instance.InitUIBattle();
            
            InitTurnOrder();
            StartTurn();
        }
    
        // 单挑撤退返回复原小战场
        public void SoloRetreatBackBattle()
        {
            foreach (var t in hmTurnOrder.Where(t => t.troopType == TroopType.Captain))
            {
                t.health = (short)(hmGeneral.curPhysical * 3);
                break;
            }
            foreach (var t in aiTurnOrder.Where(t => t.troopType == TroopType.Captain))
            {
                t.health = (short)(aiGeneral.curPhysical * 3);
                break;
            }
            
            FloorManager.Instance.ResumeFormation();
            UIBattle.Instance.InitUIBattle();
            SoloRetreat(soloState);
            soloState = default;
            Debug.Log(Time.timeScale);
            UIBattle.Instance.ResumeGame();
        }
    
        // 初始化战斗顺序
        private void InitTurnOrder()
        {   
            doneTroopsCount = 0;
            hmTurnOrder = CreateTurnOrder(hmTurnOrder);
            aiTurnOrder = CreateTurnOrder(aiTurnOrder);
            totalTroopCount = (byte)(hmTurnOrder.Count + aiTurnOrder.Count);
        }

        private List<TroopData> CreateTurnOrder(List<TroopData> troops)
        {
            List<TroopData> shuffled = new List<TroopData>(troops);
            TroopData captain = shuffled.Find(t => t.troopType == TroopType.Captain);
            shuffled.Remove(captain);

            // 随机打乱顺序
            ShuffleList(shuffled);

            // 确保将军最后行动
            if (captain != null)
            {
                shuffled.Add(captain);
            }

            return shuffled;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        private void StartTurn()
        {
            currentTroopIndex = 0;

            if (battleState == BattleState.PlayerTurn)
            {
                Debug.Log("玩家回合开始");
            }
            else if (battleState == BattleState.AITurn)
            {
                Debug.Log("敌方回合开始");
                AIBattle.Instance.AIBattleMethod();// AI设定命令
            }

            StartCoroutine(ExecuteTurn());
        }

        public IEnumerator ExecuteTurn()
        {
            yield return new WaitForSeconds(0.5f);
            List<TroopData> turnOrder = new List<TroopData>();
            if (battleState == BattleState.PlayerTurn)
            {
                turnOrder = hmTurnOrder;
            }
            else if (battleState == BattleState.AITurn)
            {
                turnOrder = aiTurnOrder;
            }
            while (currentTroopIndex < turnOrder.Count)// 循环某一方的所有小兵
            {
                Troop currentActingTroop = null;
                if (battleState == BattleState.PlayerTurn)
                {
                    
                    currentActingTroop = hmTroops.Find(t => t.data == turnOrder[currentTroopIndex]);
                    if (currentActingTroop == null)// 跳过死亡单位
                    {
                        Debug.Log($"玩家{turnOrder[currentTroopIndex].troopType}{turnOrder[currentTroopIndex].index}已死亡，跳过行动");
                        currentTroopIndex++;
                        continue;
                    }
                }
                else if (battleState == BattleState.AITurn)
                {
                    currentActingTroop = aiTroops.Find(t => t.data == turnOrder[currentTroopIndex]);
                    if (currentActingTroop == null)// 跳过死亡单位
                    {
                        Debug.Log($"AI{turnOrder[currentTroopIndex].troopType}{turnOrder[currentTroopIndex].index}已死亡，跳过行动");
                        currentTroopIndex++;
                        continue;
                    }
                }
                else
                {
                    Debug.Log("战斗结束"+ battleState);
                    yield break; // 立即退出协程
                }
                
                yield return currentActingTroop.ExecuteAction();
                // 执行单位行动完检测将领是否被俘虏
                if (currentActingTroop.troopType == TroopType.Captain)
                {
                    Captain captain = currentActingTroop as Captain;
                    if (captain != null && captain.IsCaptured())
                    {
                        battleState = captain.isPlayer? BattleState.HMCaptured : BattleState.AICaptured; // 被俘虏提示
                        string generalName = captain.isPlayer ? hmGeneral.generalName : aiGeneral.generalName;
                        HandleBattleEnd(generalName);
                        yield break;
                    }
                }
                Debug.Log("当前行动小队索引：" + currentTroopIndex);
                doneTroopsCount++;
                if (UIBattle.Instance.uiTactic.IsTacticing(true)) // 玩家战术
                {
                    UIBattle.Instance.uiTactic.UpdateTacticalState(true);
                }
                if (UIBattle.Instance.uiTactic.IsTacticing(false))
                {
                    UIBattle.Instance.uiTactic.UpdateTacticalState(false); // AI战术
                }
                currentTroopIndex++;                                 
            }
        
            EndTurn();
        }

        public void Pause()
        {
            isPaused = true;
            StopAllCoroutines();
            
            /*
            if (battleState == BattleState.PlayerTurn)
            {
                Troop troop = hmTroops[currentTroopIndex];
                troop.StopCoroutine(troop.ExecuteAction());
            }
            else if (battleState == BattleState.AITurn)
            {
                Troop troop = aiTroops[currentTroopIndex];
                troop.StopCoroutine(troop.ExecuteAction());
            }*/
            
            Debug.Log("暂停");
        }

        public void Resume()
        {
            isPaused = false;
            StartCoroutine(ExecuteTurn());
            Debug.Log("继续");
        }
        
        private void EndTurn()
        {
            if (battleState == BattleState.PlayerTurn)
            {
                Debug.Log("玩家回合结束");
                battleState = BattleState.AITurn;
                foreach (var troop in aiTroops)
                {
                    troop.ResetActionPoints();
                }
            }
            else if (battleState == BattleState.AITurn)
            {
                Debug.Log("敌方回合结束");
                battleState = BattleState.PlayerTurn;
                foreach (var troop in hmTroops)
                {
                    troop.ResetActionPoints();
                }
            }
            
            StartTurn();
        }
    
        public void ExecuteSolo()
        {
            StopAllCoroutines();
            Debug.Log("开始单挑");
            SceneManager.LoadSceneAsync("SoloScene");
        }

    
        public void HandleBattleEnd(string text)
        {
            StopAllCoroutines();
            switch (battleState)
            {
                case BattleState.AIRetreat:
                    AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(aiGeneral.generalId);
                    UIBattle.Instance.NotifyBattleEvent(text, BattleOver);
                    break;
                case BattleState.AICaptured:
                    AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(aiGeneral.generalId);
                    UIBattle.Instance.NotifyBattleEvent($"敌将{text}已被我军生擒!", BattleOver);
                    break;
                case BattleState.AIDie:
                    AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(aiGeneral.generalId);
                    UIBattle.Instance.NotifyBattleEvent($"敌将{text}已被我军斩杀!", BattleOver);
                    break;
                case BattleState.HMRetreat:
                    AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(hmGeneral.generalId);
                    UIBattle.Instance.NotifyBattleEvent(text, BattleOver);
                    break;
                case BattleState.HMCaptured:
                    AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(hmGeneral.generalId);
                    UIBattle.Instance.NotifyBattleEvent($"我方{text}被敌军擒获!", BattleOver);
                    break;
                case BattleState.HMDie:
                    AfterBattleSettlement();
                    WarManager.Instance.ChangeUnitDataDieAndCaptured(hmGeneral.generalId);
                    UIBattle.Instance.NotifyBattleEvent($"我方{text}在战斗中阵亡了!", BattleOver);
                    break;
            }
        }
    
                // 获取当前行动的部队，默认返回玩家将军
        public Troop GetActingTroop()
        {
            foreach (var troop in hmTroops)
            {
                if (troop.isActing) return troop;
            }

            foreach (var troop in aiTroops)
            {
                if (troop.isActing) return troop;
            }

            return hmTroops[0];
        }

        public Troop GetEnemyTroopByXY(bool isPlayer, int x, int y)
        {
            if (isPlayer)
            {
                foreach (var troop in aiTroops)
                {
                    if (troop.arrayPos.x == x && troop.arrayPos.y == y)
                    {
                        return troop;
                    }
                }
            }
            else
            {
                foreach (var troop in hmTroops)
                {
                    if (troop.arrayPos.x == x && troop.arrayPos.y == y)
                    {
                        return troop;
                    }
                }
            }
            Debug.Log($"未能根据坐标{x},{y}找到可攻击的敌军对象");
            return null;
        }
    
        public Troop GetTroopByXY(int x, int y)
        {
            foreach (var troop in aiTroops)
            {
                if (troop.arrayPos.x == x && troop.arrayPos.y == y)
                {
                    return troop;
                }
            }
    
            foreach (var troop in hmTroops)
            {
                if (troop.arrayPos.x == x && troop.arrayPos.y == y)
                {
                    return troop;
                }
            }
            Debug.Log($"未能根据坐标{x},{y}找到部队");
            return null;
        }
    
        // 获取敌方的武将部队单位
        public Captain GetEnemyCaptain(bool isPlayer)
        {
            return isPlayer ? aiTroops.OfType<Captain>().FirstOrDefault() : hmTroops.OfType<Captain>().FirstOrDefault();
        }
    
        public byte GetTroopKindNum(TroopType troopType, bool isPlayer)
        {
            byte num = 0;
            if (isPlayer)
            {
                foreach (var troop in hmTroops)
                {
                    if (troop.troopType == troopType)
                        num++;
                }
            }
            else
            {
                foreach (var troop in aiTroops)
                {
                    if (troop.troopType == troopType)
                        num++;
                }
            }
            return num;
        }
    
    
    
        //TODO 某方单挑页面逃跑后，重新设置其小兵的命令为撤退
        void SoloRetreat(SoloState state)
        {
            // 根据flag的值设置不同阵营的士兵指令
            if (state == SoloState.PlayerRetreat)
            {
                UIBattle.Instance.SetPlayerTroopState(TroopType.Captain, TroopState.BackWard);
                UIBattle.Instance.SetPlayerTroopState(TroopType.Cavalry, TroopState.BackWard);
                UIBattle.Instance.SetPlayerTroopState(TroopType.Archer, TroopState.BackWard);
                UIBattle.Instance.SetPlayerTroopState(TroopType.Infantry, TroopState.BackWard);
                for (byte i = 0; i < hmTroops.Count; i++)
                {
                    hmTroops[i].data.troopState = TroopState.BackWard;
                }
            }
            else if (state == SoloState.AIRetreat)
            {
                for (byte i = 0; i < aiTroops.Count; i++)
                {
                    aiTroops[i].data.troopState = TroopState.BackWard;
                }
            }
        }
    
    
        /// <summary>
        /// 处理战斗撤退逻辑
        /// 判断玩家或 AI 将领的状态并进行相应处理
        /// </summary>
        public void HandleBattleRetreat()
        {
            switch (battleState)
            {
                case BattleState.HMRetreat:
                    ProcessRetreat(
                        hmGeneral,
                        isAI: false,
                        specialSkillCheck: (general) => general.HasSkill(3, 0) && hmSoldierNum == 0,//特技单骑                                         
                        specialHurtCalculation: () => (byte)Random.Range(0, 4),
                        normalHurtCalculation: () => (byte)Random.Range(2, 15),
                        noHurtCondition: (general) => general.generalId == 4,
                        onDeath: () =>
                        {
                            battleState = BattleState.HMDie;
                            HandleBattleEnd($"我方武将{hmGeneral.generalName}流血牺牲了!");
                        },
                        onRetreatWithHurt: (retreatHurt) =>
                        {
                            battleState = BattleState.HMRetreat;
                            HandleBattleEnd($"我军撤退!体力减少{retreatHurt},还有部分士兵投降了!");
                        }
                    );
                    break;

                case BattleState.AIRetreat:
                    ProcessRetreat(
                        aiGeneral,
                        isAI: true,
                        specialSkillCheck: (general) => general.HasSkill(3, 0) && aiSoldierNum == 0,//特技单骑
                        specialHurtCalculation: () => (byte)Random.Range(0, 4),
                        normalHurtCalculation: () => (byte)Random.Range(1, 10),
                        noHurtCondition: (general) => false,
                        onDeath: () =>
                        {
                            battleState = BattleState.AIDie;
                            HandleBattleEnd($"敌将{aiGeneral.generalName}已死于乱军之中!");
                        },
                        onRetreatWithHurt: (retreatHurt) =>
                        {
                            battleState = BattleState.AIRetreat;
                            HandleBattleEnd($"敌军逃窜!体力减少{retreatHurt}");
                        },
                        aiRetreatCityCheck: () => WarManager.AIGetRetreatCityList(WarManager.Instance.aiKingId).Count > 0
                    );
                    break;
            }
        }

    
    
        /// <summary>
        /// 通用小战场撤退处理逻辑
        /// </summary>
        /// <param name="general">当前将领</param>
        /// <param name="isAI">是否为 AI</param>
        /// <param name="specialSkillCheck">特技判定条件</param>
        /// <param name="specialHurtCalculation">特技情况下的体力消耗计算</param>
        /// <param name="normalHurtCalculation">普通情况下的体力消耗计算</param>
        /// <param name="noHurtCondition">无需扣血的特殊条件</param>
        /// <param name="onDeath">将领体力耗尽时的处理逻辑</param>
        /// <param name="onRetreatWithHurt">正常撤退时的处理逻辑</param>
        /// <param name="aiRetreatCityCheck">（可选）AI 是否有撤退城市的检查逻辑</param>
        void ProcessRetreat(
            General general,
            bool isAI,
            Func<General, bool> specialSkillCheck,
            Func<byte> specialHurtCalculation,
            Func<byte> normalHurtCalculation,
            Func<General, bool> noHurtCondition,
            Action<byte> onRetreatWithHurt,
            Action onDeath,
            Func<bool> aiRetreatCityCheck = null
        )
        {
            byte retreatHurt = 0;

            // 特殊技能判定
            if (specialSkillCheck(general))
            {
                if (noHurtCondition(general)) return;

                if (Random.Range(0, 10) > 4)
                {
                    retreatHurt = specialHurtCalculation(); // 计算体力消耗
                }
            }
            else
            {
                if (isAI && aiRetreatCityCheck != null && aiRetreatCityCheck.Invoke())
                {
                    if (general.GetCurPhysical() <= 10)
                    {
                        retreatHurt = specialHurtCalculation();
                    }
                    else if (Random.Range(0, 10) > 4)
                    {
                        retreatHurt = normalHurtCalculation();
                    }
                }
                else if (Random.Range(0, 10) > 1)
                {
                    retreatHurt = normalHurtCalculation();
                }
            }

            // 检查体力是否耗尽
            if (general.SubHp(retreatHurt))
            {
                onDeath?.Invoke();
            }
            else
            {
                onRetreatWithHurt?.Invoke(retreatHurt); // 将伤害值传递给撤退逻辑
            }
        }

    

        // AfterWarSettlement方法，处理战斗结果并进行经验值分配
        public void AfterBattleSettlement()
        {
            // 计算经验值差异
            int expHM = aiSoldierLoss;
            int expAI = hmSoldierLoss;

            // 分配经验值
            GeneralListCache.AddExp_P(hmGeneral, aiGeneral, expHM);
            GeneralListCache.AddExp_P(aiGeneral, hmGeneral, expAI);

            // 增加领导和武力经验
            hmGeneral.AddLeadExp(hmSoldierLoss / 300);
            aiGeneral.AddLeadExp(aiSoldierLoss / 300);
            hmGeneral.AddForceExp(hmGeneralHurt / 50);
            aiGeneral.AddForceExp(aiGeneralHurt / 50);

            // 重置总伤害统计
            hmSoldierLoss = 0;
            aiSoldierLoss = 0;
            hmGeneralHurt = 0;
            aiGeneralHurt = 0;

            // AI特技攻心处理
            if (aiGeneral.HasSkill(3, 6) && aiGeneral.generalSoldier > 0)
            {
                if (Random.Range(0,5) < 1)
                    aiGeneral.AddSoldier(expAI / 3);
            }

            // 玩家撤退状态下的逃兵减少处理
            if (battleState == BattleState.HMRetreat)
            {
                if (hmGeneral.generalSoldier > 0)
                {
                    int i = Random.Range(3,18);
                    short deserter = (short)(i * 15); // 计算减少的士兵数量

                    if (!isHmDef)//玩家为进攻方
                        deserter = (short)(deserter + 250); // 特殊情况下增加减少数量

                    if (hmGeneral.HasSkill(5, 7))
                        deserter = (short)(deserter / 4); // 特技冷静，减少撤退损失士兵量

                    hmGeneral.SubSoldier(deserter); // 减少玩家士兵

                    aiGeneral.AddSoldier(deserter); // 增加AI士兵
                }
                return;
            }

            // 如果玩家具有特技攻心
            if (hmGeneral.HasSkill(3, 6) && hmGeneral.generalSoldier > 0)
            {
                if (Random.Range(0,5) < 1)
                    hmGeneral.AddSoldier(expHM / 3);
            }
        }
        
        // 自动模拟人类与AI的战斗过程
        public void SimulatedBattle()
        {
            int hmPower = 0;  // 玩家战力
            int aiPower = 0;  // AI战力
            int hmPf = 0;     // 玩家战力系数
            int aiPf = 0;     // AI战力系数
            int i;
            //双方武将设定初始兵力用于计算增加经验
            short hmInitialSoldier = hmGeneral.generalSoldier;
            short aiInitialSoldier = aiGeneral.generalSoldier;	
            // 玩家小兵战斗力计算
            for (i = 1; i < hmTurnOrder.Count; i++)
            {
                int atk = (short)((hmGeneral.lead * 2 + hmGeneral.force) / 3 + (hmGeneral.lead + hmGeneral.force) * (hmGeneral.level - 1) / 25);
                int def = (short)((hmGeneral.lead * 2 + hmGeneral.IQ) / 3 + (hmGeneral.lead + hmGeneral.IQ) * (hmGeneral.level - 1) / 25);

                // 检查特技金刚，触发并调整防御力
                if (hmGeneral.HasSkill(2, 7) && Random.Range(0,3) < 1)
                    def += def / 2;
                // 检查特技不屈，触发并调整防御力
                if (hmGeneral.HasSkill(2, 8) && Random.Range(0,3) < 1)
                    def += def / 2;
                // 检查特技兵圣，增加攻击和防御力
                if (hmGeneral.HasSkill(3, 9))
                {
                    atk += atk / 2;
                    def += def / 2;
                }

                // 根据兵种调整战斗力
                if (hmTurnOrder[i].troopType == TroopType.Cavalry)  // 骑兵的处理
                {
                    if (hmGeneral.HasSkill(2, 0) && Random.Range(0,3) < 1) // 特技骑神
                    {
                        atk += atk/2;
                    }
                    else if (hmGeneral.HasSkill(2, 1) && Random.Range(0,5) < 1) // 特技骑将
                    {
                        atk += atk/2;
                    }
                    // 特技乱战加成
                    if ((battleTerrain == 10 || battleTerrain == 11) && hmGeneral.HasSkill(2, 5) && Random.Range(0,6) < 1)
                        atk += atk / 2;

                    int p = (atk + def) * 2 + atk * (Random.Range(1,3)) + atk * hmTacticPoint / 8;
                    hmPower += p * p;
                }
                else if (hmTurnOrder[i].troopType == TroopType.Archer)  // 弓兵的处理
                {
                    // 特定技能和地形加成
                    if (hmGeneral.HasSkill(2, 6) && Random.Range(0,5) < 2) // 特技连弩
                        atk += atk * 2 / 5;
                    if (hmGeneral.HasSkill(2, 2) && Random.Range(0,3) < 1) // 特技弓神
                        atk += atk / 2;
                    else if (hmGeneral.HasSkill(2, 3) && Random.Range(0,5) < 1) // 特技弓将
                        atk += atk / 2;

                    // 特技乱战加成
                    if ((battleTerrain == 10 || battleTerrain == 11) && hmGeneral.HasSkill(2, 5) && Random.Range(0,6) < 1)
                        atk += atk / 2;
                    // 特技水将加成
                    if (battleTerrain == 9 && hmGeneral.HasSkill(2, 4) && Random.Range(0,6) < 1)
                        atk += atk / 2;

                    // 战斗守城处理
                    if (battleTerrain == 8 && isHmDef)
                    {
                        if (hmTacticPoint >= 14)
                        {
                            int p = (atk + def) * 2 + atk * Random.Range(3,6);
                            hmPower += p * p;
                        }
                        else if (hmTacticPoint >= 10)
                        {
                            int p = (atk + def) * 2 + atk * Random.Range(3,5);
                            hmPower += p * p;
                        }
                        else if (hmTacticPoint >= 8)
                        {
                            int p = (atk + def) * 2 + atk * Random.Range(2,5);
                            hmPower += p * p;
                        }
                        else if (hmTacticPoint >= 7)
                        {
                            int p = (atk + def) * 2 + atk * Random.Range(2,4);
                            hmPower += p * p;
                        }
                        else
                        {
                            int p = (atk + def) * 2 + atk * Random.Range(1,3);
                            hmPower += p * p;
                        }
                    }
                    else
                    {
                        int p = (atk + def) * 2 + atk * Random.Range(0,2) + atk * hmTacticPoint / 7;
                        hmPower += p * p;
                    }
                }
                else  // 步兵处理
                {
                    // 特技水将加成
                    if (battleTerrain == 9 && hmGeneral.HasSkill(2, 4) && Random.Range(0,6) < 1)
                        atk += atk / 2;
                    // 特技乱战加成
                    if ((battleTerrain == 10 || battleTerrain == 11) && hmGeneral.HasSkill(2, 5) && Random.Range(0,6) < 1)
                        atk += atk / 2;

                    int p = (atk + def) * 2 / 3;
                    hmPower += p * p;
                }
            }
            Debug.LogWarning("玩家战力：" + hmPower);
            // AI小兵战斗力计算，逻辑同上
            for (i = 1; i < aiTurnOrder.Count; i++) 
            {
                int theAtk = (short)((aiGeneral.lead * 2 + aiGeneral.force) / 3 + (aiGeneral.lead + aiGeneral.force) * (aiGeneral.level - 1) / 25);;
                int theDef = (short)((aiGeneral.lead * 2 + aiGeneral.IQ) / 3 + (aiGeneral.lead + aiGeneral.IQ) * (aiGeneral.level - 1) / 25);

                if (aiGeneral.HasSkill(2, 7) && Random.Range(0,3) < 1)
                    theDef += theDef / 2;
                if (aiGeneral.HasSkill(2, 8) && Random.Range(0,3) < 1)
                    theDef += theDef / 2;

                if (aiGeneral.HasSkill(3, 9))
                {
                    theAtk += theAtk / 2;
                    theDef += theDef / 2;
                }

                if (aiTurnOrder[i].troopType == TroopType.Cavalry)
                {
                    if (aiGeneral.HasSkill(2, 0) && Random.Range(0,3) < 1)
                        theAtk += theAtk / 2;
                    else if (aiGeneral.HasSkill(2, 1) && Random.Range(0,5) < 1)
                        theAtk += theAtk / 3;

                    if ((battleTerrain == 10 || battleTerrain == 11) && aiGeneral.HasSkill(2, 5) && Random.Range(0,6) < 1)
                        theAtk += theAtk / 2;

                    int p = (theAtk + theDef) * 2 + theAtk * Random.Range(1,3) + theAtk * aiTacticPoint / 8;
                    aiPower += p * p;
                }
                else if (aiTurnOrder[i].troopType == TroopType.Archer)
                {
                    if (aiGeneral.HasSkill(2, 6) && Random.Range(0,5) < 2)
                        theAtk += theAtk * 2 / 5;
                    if (aiGeneral.HasSkill(2, 2) && Random.Range(0,3) < 1)
                        theAtk += theAtk / 2;
                    else if (aiGeneral.HasSkill(2, 3) && Random.Range(0,5) < 1)
                        theAtk += theAtk / 2;

                    if (battleTerrain == 9 && aiGeneral.HasSkill(2, 4) && Random.Range(0,6) < 1)
                        theAtk += theAtk / 2;
                    if ((battleTerrain == 10 || battleTerrain == 11) && aiGeneral.HasSkill(2, 5) && Random.Range(0,6) < 1)
                        theAtk += theAtk / 2;

                    if (battleTerrain == 8 && !isHmDef)
                    {
                        if (aiTacticPoint >= 14)
                        {
                            int p = (theAtk + theDef) * 2 + theAtk * Random.Range(3,6);
                            aiPower += p * p;
                        }
                        else if (aiTacticPoint >= 10)
                        {
                            int p = (theAtk + theDef) * 2 + theAtk * Random.Range(3,5);
                            aiPower += p * p;
                        }
                        else if (aiTacticPoint >= 8)
                        {
                            int p = (theAtk + theDef) * 2 + theAtk * Random.Range(3,5);
                            aiPower += p * p;
                        }
                        else if (aiTacticPoint >= 7)
                        {
                            int p = (theAtk + theDef) * 2 + theAtk * Random.Range(2,4);
                            aiPower += p * p;
                        }
                        else
                        {
                            int p = (theAtk + theDef) * 2 + theAtk * Random.Range(1,3);
                            aiPower += p * p;
                        }
                    }
                    else
                    {
                        int p = (theAtk + theDef) * 2 + theAtk * Random.Range(0,2) + theAtk * aiTacticPoint / 7;
                        aiPower += p * p;
                    }
                }
                else
                {
                    if (battleTerrain == 9 && aiGeneral.HasSkill(2, 4) && Random.Range(0,6) < 1)
                        theAtk += theAtk / 2;
                    if ((battleTerrain == 10 || battleTerrain == 11) && aiGeneral.HasSkill(2, 5) && Random.Range(0,6) < 1)
                        theAtk += theAtk / 2;

                    int p = (theAtk + theDef) * 2 / 3;
                    aiPower += p * p;
                }
            }
            Debug.LogWarning("AI战力：" + aiPower);
            hmPower /=1000;
            aiPower /=1000;
    	
            hmPf = hmPower;
            aiPf = aiPower;
            hmPower = hmPf*hmGeneral.generalSoldier;
            aiPower = aiPf*aiGeneral.generalSoldier;
            if (hmPower >= aiPower)
            {
                aiGeneral.generalSoldier = 0;
                int ends = (hmPower-aiPower)/hmPf;
                if (ends < 0)
                {
                    ends = 0;
                }
                else if (ends>3000) 
                {
                    ends = 3000;
                }
                hmGeneral.generalSoldier = (short) ends;
            }
            else 
            {
                hmGeneral.generalSoldier = 0;
                int ends = (aiPower - hmPower)/aiPf;
                if (ends < 0)
                {
                    ends = 0;
                }
                else if (ends>3000) 
                {
                    ends = 3000;
                }
                aiGeneral.generalSoldier = (short) ends;
            }
        
            int expHM = (aiInitialSoldier - aiGeneral.generalSoldier);
            int expAI = (hmInitialSoldier - hmGeneral.generalSoldier);
        
            GeneralListCache.AddExp_P(hmGeneral,aiGeneral, expHM);
            GeneralListCache.AddExp_P(aiGeneral,hmGeneral, expAI);
            hmGeneral.AddLeadExp((byte) (expHM/300));
            aiGeneral.AddLeadExp((byte) (expAI/300));
        
            if (aiGeneral.generalSoldier == 0)
            {
                battleState = BattleState.BattleWin;
            }
            else if(hmGeneral.generalSoldier == 0)
            {
                battleState = BattleState.BattleLose;
            }
        }
        
        public void BattleOver()
        {
            OnBattleOver?.Invoke();
            Debug.Log("战斗结束");
            SceneManager.LoadSceneAsync("WarScene");
        }



    }
}