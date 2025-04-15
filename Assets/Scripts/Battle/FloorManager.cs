using System.Collections.Generic;
using System.IO;
using BaseClass;
using DataClass;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Battle
{
    public class FloorManager : MonoBehaviour
    {
        private static FloorManager _instance;

        public static FloorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<FloorManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("FormationManager单例为空，请确保已添加到场景中");
                    }
                }
                return _instance;
            }
        }

        private FloorManager(){ }
        
        [Tooltip("战斗地图7x16为byte数组")]
        private static byte[,] _battleMap
        {
            get => BattleManager.Instance.battleMap;
            set => BattleManager.Instance.battleMap = value;
        }

        private const byte BattleCols = 16; //Battle地图数组列数对应X坐标
        private const byte BattleRows = 7;  //Battle地图数组行数对应Y坐标
    
        public Tilemap BattleTilemap;
        [SerializeField] private Tile[] terrainTiles;
    
        public GameObject troopPrefab;
        public GameObject troopRoom;
    
        private Vector2Int[] _hmTroopArrayPos = new Vector2Int[11];
        private Vector2Int[] _aiTroopArrayPos = new Vector2Int[11];
    
        // 初始化小战场战斗地形
        public void InitFloor()
        {
            GenerateTileMap(BattleManager.Instance.battleTerrain);
            FillFormation();
        }
    
        // 从单挑中恢复小战场阵型
        public void ResumeFormation()
        {
            GenerateTileMap(BattleManager.Instance.battleTerrain);
            BattleManager.Instance.hmTroops.Clear();
            BattleManager.Instance.aiTroops.Clear();
            
            foreach (var data in BattleManager.Instance.hmTurnOrder)
            {
                if (data.health <= 0)
                {
                    continue;
                }
                SpawnTroop(data);
            }
            foreach (var data in BattleManager.Instance.aiTurnOrder)
            {
                if (data.health <= 0)
                {
                    continue;
                }
                SpawnTroop(data);
            }
        }
    
        // 设置骑弓步比例
        public static byte[] SetCAI(General general, byte terrain)
        {
            byte[] sc = new byte[3];

            if (terrain >= 1 && terrain <= 7) // 平原
                sc = TextLibrary.CAI_Proportion[0][general.army[0]];
            else if (terrain >= 10 && terrain <= 12) // 山地
                sc = TextLibrary.CAI_Proportion[1][general.army[1]];
            else if (terrain == 9) // 水域
                sc = TextLibrary.CAI_Proportion[2][general.army[2]];
            else if (terrain == 8) // 城战
            {
                int index = general.level switch
                {
                    >= 7 => 0,
                    >= 5 => 1,
                    >= 3 => 2,
                    _ => 3
                };
                sc = TextLibrary.CAI_Proportion[3][index];
            }
        
            return sc;
        }

        // 获取地形攻防倍数
        public static float GetTef(byte terrain, General general)
        {
            float tef = 1.0f;

            if (terrain is >= 1 and <= 7) // 平原
                tef = TextLibrary.TopInf[general.army[0]];
            else if (terrain >= 10 && terrain <= 12) // 山地
                tef = TextLibrary.TopInf[general.army[1]];
            else if (terrain == 9) // 水域
                tef = TextLibrary.TopInf[general.army[2]];

            return tef;
        }

        // 获取守城地形
        public void GetDefCastleMap()
        {
            // 根据地形设置城堡布局
            if (BattleManager.Instance.battleTerrain == 8)
            {
                if (!BattleManager.Instance.isHmDef)// AI守城
                {
                    byte[,] aiCastle = DataManagement.GetFormation("ac");
                    for (byte y = 0; y < 7; y++)
                    {
                        for (byte x = 0; x < 16; x++)
                        {
                            if (aiCastle[y,x] > 1)
                                switch (aiCastle[y, x])
                                {
                                    case 2:
                                        _battleMap[y, x] = 32;
                                        break;
                                    case 3:
                                        _battleMap[y, x] = 16;
                                        break;
                                    case 4:
                                        _battleMap[y, x] = 2;
                                        break;
                                }
                        }
                    }
                }
                else//玩家守城
                {
                    byte[,] hmCastle = DataManagement.GetFormation("hc");
                    for (byte y = 0; y < 7; y++)
                    {
                        for (byte x = 0; x < 16; x++)
                        {
                            if (hmCastle[y,x] > 1)
                                switch (hmCastle[y,x])
                                {
                                    case 2:
                                        _battleMap[y, x] = 32;
                                        break;
                                    case 3:
                                        _battleMap[y, x] = 16;
                                        break;
                                    case 4:
                                        _battleMap[y, x] = 2;
                                        break;
                                }
                        }
                    }
                }
            }
        }

        void FillFormation()
        {
            if (BattleManager.Instance.battleTerrain == 8)//城池地形
            {
                if (BattleManager.Instance.isHmDef)//玩家守城
                {
                    DefCastleSoldierPos();
                    AISelectFormation(GetAIFormation());
                }
                else//AI守城
                {
                    DefCastleSoldierPos();
                    SelectFormation(BattleManager.Instance.formationIndex);
                }
            }
            else
            {
                AISelectFormation(Random.Range(0, 4));
                SelectFormation(BattleManager.Instance.formationIndex);
            }
        }
        
        // 用于人类玩家选择阵型
        public void SelectFormation(int index)
        {
            // 初始化7行16列的阵型数组
            byte[,] hmFormation = DataManagement.GetFormation($"h{index}");

            // 遍历阵型，设置士兵位置
            for (byte y = 0; y < 7; y++)
            {
                for (byte x = 0; x < 16; x++)
                {
                    // 判断是否有士兵
                    if (hmFormation[y, x] != 255) // 修正条件判断
                    {
                        // 遍历所有士兵
                        for (byte i = 0; i < 11; i++)
                        {
                            // 如果士兵编号匹配
                            if (i == hmFormation[y, x])
                            {
                                _hmTroopArrayPos[i].x = x;
                                _hmTroopArrayPos[i].y = y;
                            }
                        }
                    }
                }
            }
        
        }

        // 确定 AI 的阵型选择通过玩家弓兵部队数来判断
        public byte GetAIFormation()
        {
            if (BattleManager.Instance.GetTroopKindNum(TroopType.Archer,true) >= 3)
                return 1;
            return 2;
        }


        // 用于 AI 玩家选择阵型
        public void AISelectFormation(int index)
        {
            // 初始化7行16列的阵型数组
            byte[,] aiFormation =  DataManagement.GetFormation($"a{index}");

            // 遍历阵型，设置士兵位置
            for (byte y = 0; y < 7; y++)
            {
                for (byte x = 0; x < 16; x++)
                {
                    // 判断是否有士兵
                    if (aiFormation[y, x] != 255) // 修正条件判断
                    {
                        // 遍历所有敌方士兵
                        for (byte i = 0; i < 11; i++)
                        {
                            // 如果士兵编号匹配
                            if (i == aiFormation[y, x])
                            {
                                _aiTroopArrayPos[i].x = x;
                                _aiTroopArrayPos[i].y = y;
                            }
                        }
                    }
                }
            }
        
        }
    
        // 守城时初始化士兵的阵型和位置
        public void DefCastleSoldierPos()
        {
            // 初始化阵型数据，7 行 16 列
            byte[,] formation = new byte[7, 16];

            if (BattleManager.Instance.isHmDef)//玩家守城
            {
                // 解析阵型数据
                formation = DataManagement.GetFormation("hc0");

                // 遍历阵型行列，分配士兵位置
                for (byte y = 0; y < 7; y++)
                {
                    for (byte x = 0; x < 16; x++)
                    {
                        // 如果当前单元格有士兵编号
                        if (formation[y, x] != 255) // 修正条件判断
                        {
                            // 遍历己方所有士兵，匹配编号
                            for (byte i = 0; i < 11; i++)
                            {
                                if (formation[y, x] == i)
                                {
                                    // 设置士兵位置
                                    _hmTroopArrayPos[i].x = x;
                                    _hmTroopArrayPos[i].y = y;
                                }
                            }
                        }
                    }
                }
            }
            else//AI守城
            {
                // 敌方士兵阵型文件路径
                byte fileIndex;

                // 根据敌方将领等级选择不同阵型文件
                if (BattleManager.Instance.aiGeneral.level >= 7)
                {
                    fileIndex = 2;
                }
                else if (BattleManager.Instance.aiGeneral.level >= 4)
                {
                    fileIndex = 1;
                }
                else
                {
                    fileIndex = 0;
                }

                // 解析阵型数据
                formation = DataManagement.GetFormation($"ac{fileIndex}");

                // 遍历阵型行列，分配敌方士兵位置
                for (byte y = 0; y < 7; y++)
                {
                    for (byte x = 0; x < 16; x++)
                    {
                        // 如果当前单元格有士兵编号
                        if (formation[y, x] != 255) // 修正条件判断
                        {
                            // 遍历敌方所有士兵，匹配编号
                            for (byte i = 0; i < 11; i++)
                            {
                                if (formation[y, x] == i)
                                {
                                    // 设置敌方士兵位置
                                    _aiTroopArrayPos[i].x = x;
                                    _aiTroopArrayPos[i].y = y;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void GenerateTileMap(byte battleTerrain)
        {
            // 映射 _battleMap 值到 terrainTiles 索引
            Dictionary<byte, byte> battleMapTileMapping = new Dictionary<byte, byte>
            {
                { 0, 3 },  // 城池地板
                { 2, 6 },  // 横向楼梯
                { 16, 7 }, // 纵向楼梯
                { 32, 8 }  // 不可通过墙
            };

            // 获取瓦片类型索引的通用方法
            byte GetTerrainTileIndex(int x, int y)
            {
                if (battleTerrain is >= 1 and <= 7) return 0; // 平地
                if (battleTerrain == 9) return 1;            // 水域
                if (battleTerrain is >= 10 and <= 12) return 2; // 山地
                if (battleTerrain == 8) // 城池地形
                {
                    if (y <= 1) return 3;                      // 城池地板
                    if (y == 2) return 4;                      // 城池中心
                    if (y == 3) return 5;                      // 城池顶部
                    if (y is >= 4 and <= 10 && x is >= 0 and < 16 && 10 - y < _battleMap.GetLength(0))
                    {
                        return battleMapTileMapping.TryGetValue(_battleMap[10 - y, x], out byte tileIndex) ? tileIndex : (byte)255;
                    }
                    if (y == 11) return 9;                     // 城池边缘
                    if (y == 12) return 10;                    // 城池外部
                }
                return 255; // 无效地形
            }

            // 设置瓦片的通用方法
            void SetTile(int x, int y, int tileIndex)
            {
                if (tileIndex >= 0)
                {
                    Vector3Int tilePos = new Vector3Int(x - 8, y - 2, 0);
                    BattleTilemap.SetTile(tilePos, terrainTiles[tileIndex]);
                }
            }

            // 处理城池地形前，调用初始化逻辑
            if (battleTerrain == 8)
            {
                GetDefCastleMap();
            }

            // 遍历地图并设置瓦片
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 13; y++)
                {
                    int tileIndex = GetTerrainTileIndex(x, y);
                    SetTile(x, y, tileIndex);
                }
            }
        }
        

        public void PutPosition(TroopData data)
        {
            // 获取部队位置数组
            Vector2Int[] arrayPositions = data.isPlayer ? _hmTroopArrayPos : _aiTroopArrayPos;
            data.arrayPos = arrayPositions[data.index];
            data.face = data.isPlayer ? (byte)3 : (byte)2;
            SpawnTroop(data);
        }

        private void SpawnTroop(TroopData data)
        {
            // 生成部队
            GameObject troopObj = Instantiate(troopPrefab, troopRoom.transform);
            Troop troop;
            switch (data.troopType)
            {
                case TroopType.Captain:
                    troop = troopObj.AddComponent<Captain>();
                    break;
                case TroopType.Cavalry:
                    troop = troopObj.AddComponent<Cavalry>();
                    break;
                case TroopType.Archer:
                    troop = troopObj.AddComponent<Archer>();
                    break;
                case TroopType.Infantry:
                    troop = troopObj.AddComponent<Infantry>();
                    break;
                default:
                    throw new System.ArgumentException($"未知的 TroopType 类型: {data.troopType}");
            }
            troop.Init(data);
            troop.UpdateHealth(0);
            troop.TurnDirection(data.face);

            // 设置部队位置
            int x = troop.arrayPos.x - 8;
            int y = 8 - troop.arrayPos.y;
            Vector3Int tilePosition = new Vector3Int(x, y, 0);
            Vector3 worldPosition = BattleTilemap.GetCellCenterWorld(tilePosition);
            troop.gameObject.transform.position = worldPosition;
            troop.gameObject.layer = data.isPlayer ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("AI");
            troop.gameObject.name = data.isPlayer ? "Hm" + data.troopType + data.index : "Ai" + data.troopType + data.index;
            
            // 添加到对应列表
            if (data.isPlayer)
            {
                BattleManager.Instance.hmTroops.Add(troop);
            }
            else
            {
                BattleManager.Instance.aiTroops.Add(troop);
            }
            // 更新战场地图
            byte layerFlag = data.isPlayer ? (byte)0x40 : (byte)0x80;
            _battleMap[troop.arrayPos.y, troop.arrayPos.x] |= layerFlag;
        }
    }
}