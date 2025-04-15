/*using System.Collections.Generic;
using BaseClass;
using UnityEngine;

namespace DataClass
{   
    // 小战场战斗数据单例类
    public class BattleInfo : MonoBehaviour
    {
        // 单例实例
        private static BattleInfo _instance;

        // 指定的允许保留的场景
        private readonly List<string> _allowedScenes = new() { "StartBattleScene", "BattleScene", "SoloScene" };

        public static BattleInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("BattleInfo instance is null! Ensure it's added to a GameObject in the scene.");
                }
                return _instance;
            }
        }

        // 数据字段
        public bool isPaused; // 游戏是否暂停
        public BattleState battleState;
        public BattleState beforeState; // 暂停前保留状态
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
        public byte hmTacticTermination; // 玩家开始战术的部队索引
        public byte aiTacticTermination; // AI开始战术的部队索引
        public short hmSoldierNum;
        public short aiSoldierNum;
        public List<Troop> hmTroops = new();
        public List<Troop> aiTroops = new();
        public short hmGeneralHurt;
        public short aiGeneralHurt;
        public short hmSoldierLoss;
        public short aiSoldierLoss;
        public short[] hmCAI_Num = new short[3]; // 玩家骑弓步各种兵力
        public short[] aiCAI_Num = new short[3];
        public short retreatLoss; // 撤退减少兵力    
        public byte currentTroopIndex;
        public List<Troop> hmTurnOrder = new(); // 玩家部队行动顺序
        public List<Troop> aiTurnOrder = new(); // 敌方部队行动顺序
        public SoloState soloState; // 单挑对战状态

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
        }

        // 清理所有数据
        public void ClearData()
        {
            isPaused = false;
            battleState = default;
            beforeState = default;
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
            retreatLoss = 0;
            currentTroopIndex = 0;
            hmTurnOrder.Clear();
            aiTurnOrder.Clear();
            soloState = default;

            Debug.Log("BattleInfo data cleared.");
        }

        // 检查场景是否允许保留
        public void CheckSceneValidity()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (!_allowedScenes.Contains(currentSceneName))
            {
                Debug.Log($"当前场景 {currentSceneName} 不允许保留 BattleInfo，清理数据...");
                ClearData();
                Destroy(gameObject); // 销毁实例
            }
        }

        // 在场景加载完成后检查合法性
        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            CheckSceneValidity();
        }
    }
}*/