/*using System.Collections.Generic;
using BaseClass;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataClass
{
    public class WarInfo : MonoBehaviour
    {
        private static WarInfo _instance;
		
		public static WarInfo Instance
		{
			get
			{
				if (_instance == null)
                {
					Debug.LogError("WarInfo单例为空!请确保已添加到场景中");
                }
                return _instance;
			}
		}

		// 指定的允许保留的场景
		private readonly List<string> _allowedScenes = new() { "WarScene","StartBattleScene", "BattleScene", "SoloScene" };

		public WarState warState;// 战争状态
		public byte curWarCityId = 10;// 当前战争发生的城市ID
		public byte departureCity = 19;// 出发城池ID
		public byte day{ get; set; }// 天数
		public byte chooseGeneralNum = 3;// 选择将领数
		public short[] chooseGeneralIdArray = { 61, 26, 18 };// 选择将领ID数组

		public Unit hmUnit = null ;
		public Unit aiUnit = null;
		public List<Unit> aiUnits;
		public List<Unit> hmUnits;
		public Dictionary<Vector2Int, Unit> UnitDictionary = new Dictionary<Vector2Int, Unit>();//存储所有units的id和实例
		public bool isSelected = false;
		
		public byte[,] warMap = new byte[19, 32];
		public byte[] aiUnitCellX = new byte[10];
		public byte[] aiUnitCellY = new byte[10];
		public byte[] hmUnitCellX = new byte[10];
		public byte[] hmUnitCellY = new byte[10];
		
		public General aiGeneral{ get; set; }// AI当前将领
		public byte[] aiUnitTrapped = new byte[10];//电脑被困将领状态数组
		public byte aiGeneralNum{ get; set; }// AI将领数
		public short[] aiGeneralIdArray = new short[10];// AI将领ID数组
		public short aiKingId = 1;// AI方君主ID
		public short aiGeneralId{ get; set; }// AI方将领ID
		public short aiGold = 1000;// AI金钱
		public short aiFood = 1000;// AI粮食
		public byte[] aiGeneralFinishMove{ get; set; }// AI完成移动的将领数组
		public byte[] aiGeneralHaveMove{ get; set; }// AI还能移动将领数组
		public byte[] aiGeneralMoveBonus{ get; set; }// AI机动点数组


		public General hmGeneral{ get; set; }// 玩家当前将领
		public byte[] hmUnitTrapped = new byte[10];//玩家被困将领状态数组
		public byte hmGeneralNum{ get; set; }// 玩家武将数
		public short[] hmGeneralIdArray = new short[10];// 玩家将领ID数组
		public short hmGeneralId{ get; set; }// 玩家将领ID
		public short hmGold = 1000;// 玩家金钱
		public short hmFood = 1000;// 玩家粮食

		public byte planIndex;
		public string planResult;
		public bool doAgree;
		public byte retreatCause{ get; set; }//撤退原因
		
		
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
		
		// 检查场景是否允许保留
		public void CheckSceneValidity()
		{
			string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

			if (!_allowedScenes.Contains(currentSceneName))
			{
				Debug.Log($"当前场景 {currentSceneName} 不允许保留 WarInfo，清理数据...");
				//ClearData();
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