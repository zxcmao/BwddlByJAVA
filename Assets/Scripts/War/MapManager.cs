using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace War
{
    public class MapManager : MonoBehaviour
    {
        // 单例实例
        private static MapManager _instance;

        // 确保线程安全地获取单例实例
        public static MapManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<MapManager>(); // 在场景中查找
                    if (_instance == null)
                    {
                        Debug.LogError("战争地图管理器单例为空!请确认是否已添加到场景中");
                    }
                }
                return _instance;
            }
        }

        // 私有化构造函数以防止外部实例化
        private MapManager() { }
    
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Canvas unitCanvas;
        [SerializeField] private Tile[] terrainTiles;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] public GameObject markerPrefab;
        private Vector3? _selectedCell; // 存储当前选中的单元格
        private List<GameObject> _markers = new List<GameObject>(); // 存储所有生成的标记


        private static byte _rows = 19;// 行数
        private static byte _cols = 32;// 列数

        private static byte[,] WarMap
        {
            get => WarManager.Instance.warMap;
            set => WarManager.Instance.warMap = value;
        }
        
        
        private Vector2Int[] _attackerPos = new Vector2Int[15];
        private Vector2Int[] _defenderPos = new Vector2Int[15];

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this; // 确保在 Awake 中完成初始化
            }
            else
            {
                Destroy(gameObject); // 防止多个实例
            }
        }
        

        public static void LoadMap()
        {
            // 获取单例实例并调用方法
            Instance.LoadAndGenerateMap();
        }
        
        void LoadAndGenerateMap()
        {
            LoadMap(WarManager.Instance.curWarCityId);
            GenerateMap(WarMap);
        }

        private void LoadMap(byte cityId)
        {
            if (DataManagement.maps.TryGetValue(cityId, out byte[,] mapData))
            {
                WarMap = mapData;
                byte i = 0;
                byte j = 0;

                for (byte row = 0; row < _rows; row++)
                {
                    for (byte col = 0; col < _cols; col++)
                    {
                        if (WarMap[row, col] == 1)
                        {
                            _attackerPos[i] = new Vector2Int(col, row);
                            i++;
                        }
                        else if (WarMap[row, col] == 2)
                        {
                            _defenderPos[j] = new Vector2Int(col, row);
                            j++;
                        }
                        else if (WarMap[row, col] == 8)
                        {
                            WarManager.Instance.cityPos = new Vector2Int(col, row);
                        }
                    }
                }
                Log2DArray(WarMap);// 测试输出地图数组
                
            }
            else
            {
                Debug.LogError("加载地图数据失败: " + cityId);
            }
            /*string filePath = Path.Combine(Application.streamingAssetsPath, "Map", + cityId + ".CSV");

            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                WarMap = new byte[_rows, _cols];
                byte i = 0;
                byte j = 0;

                for (byte row = 0; row < _rows; row++)
                {
                    string[] values = lines[row].Split(',');
                    for (byte col = 0; col < _cols; col++)
                    {
                        WarMap[row, col] = byte.Parse(values[col]);
                        if (WarMap[row, col] == 1)
                        {
                            _attackerPos[i] = new Vector2Int(col, row);
                            i++;
                        }
                        else if (WarMap[row, col] == 2)
                        {
                            _defenderPos[j] = new Vector2Int(col, row);
                            j++;
                        }
                        else if (WarMap[row, col] == 8)
                        {
                            cityPos = new Vector2Int(col, row);
                        }
                    }
                }
                Log2DArray(WarMap);// 测试输出地图数组
            }
            else
            {
                Debug.LogError("加载地图文件失败: " + filePath);
            }*/
        }

        // 测试输出地图数组
        private static void Log2DArray(byte[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                string row = "";
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    row += $"{array[i, j]} ";
                }
                Debug.Log(row.Trim());
            }
        }



        // 生成瓦片地图
        void GenerateMap(byte[,] mapData)
        {
            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _cols; col++)
                {
                    int tileIndex = (mapData[row, col] - 1) & 0x1F;

                    if (tileIndex >= 0 && tileIndex < terrainTiles.Length)
                    {
                        Vector3Int tilePosition = new Vector3Int(col, _rows - 1 - row, 0);
                        tilemap.SetTile(tilePosition, terrainTiles[tileIndex]);
                    }
                }
            }
            Debug.Log("地图已生成");
        }

    
        // 添加单位到字典
        private void AddUnit(Vector2Int position, UnitObj unitObj)
        {
            WarManager.UnitDictionary.Add(position, unitObj);
        }
        
        public static void RemoveUnit(Vector2Int arrayPos)
        {
            WarManager.UnitDictionary.Remove(arrayPos);
        }

        // 更改键的方法
        public static void ChangeXYInDictionary(Vector2Int newKey, Vector2Int oldKey)
        {
            if (WarManager.UnitDictionary.TryGetValue(oldKey, out UnitObj unit))
            {
                // 移除旧的键值对
                WarManager.UnitDictionary.Remove(oldKey);
                // 添加新的键值对，保持值不变
                WarManager.UnitDictionary[newKey] = unit;
            }
            else
            {
                Debug.LogWarning("旧坐标键不存在于字典中。");
            }
        }
    
        // 根据位置查询单位
        public static UnitObj GetUnitByXY(Vector2Int arrayPos)
        {
            if (WarManager.UnitDictionary.TryGetValue(arrayPos, out UnitObj unit))
            {
                return unit;
            }
            //Debug.Log($"未能根据坐标:{arrayPos.x},{arrayPos.y}找到单位");
            return null; // 或者抛出异常，或者返回默认值
        }

        public static void MoveAIUnit(Vector2Int newKey, Vector2Int oldKey)
        {
            UnitObj unitObj = GetUnitByXY(oldKey);
            if (unitObj != null)
            {
                Vector3 destination = TurnToWorldPos(newKey);
                unitObj.gameObject.transform.position = destination;
                unitObj.GetArmySprite();
            }
            else
            {
                Debug.LogWarning("旧坐标键不存在于字典中。");
            }
        }
        // 将数组坐标转换为世界坐标
        public static Vector3 TurnToWorldPos(Vector2Int arrayPos)
        {
            return Instance.tilemap.GetCellCenterWorld(new Vector3Int(arrayPos.x, _rows - 1 - arrayPos.y, 0)) ;
        }
        
        public static int GetCellNeedMoves(int terrainValue, short genId)
        {
            if ((terrainValue & 0xC0) != 0)//如果有单位在地形
                return 100;
            terrainValue = (byte)(terrainValue & 0x1F);
            int movesNeed = 0;
            General general = GeneralListCache.GetGeneral(genId);
            switch (terrainValue)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 19:
                case 22:
                    movesNeed = 2;
                    return movesNeed;
                case 10: //树林
                case 12: //山地
                    movesNeed = 6;
                    if (general.HasSkill(2, 5))
                        movesNeed--;
                    switch (general.army[1])
                    {
                        case 1:
                            movesNeed--;
                            break;
                        case 2:
                            movesNeed -= 2;
                            break;
                        case 3:
                            movesNeed -= 3;
                            break;
                    }

                    return movesNeed;
                case 9:
                case 15:
                    movesNeed = 6;
                    if (general.HasSkill(2, 5))
                        movesNeed--;
                    switch (general.army[2])
                    {
                        case 1:
                            movesNeed--;
                            break;
                        case 2:
                            movesNeed -= 2;
                            break;
                        case 3:
                            movesNeed -= 3;
                            break;
                    }

                    return movesNeed;
                case 11:
                    movesNeed = 5;
                    switch (general.army[2])
                    {
                        case 1:
                            movesNeed--;
                            break;
                        case 2:
                            movesNeed -= 2;
                            break;
                        case 3:
                            movesNeed -= 3;
                            break;
                    }

                    return movesNeed;
            }

            movesNeed = 120;
            return movesNeed;
        }

        public static int GetCellNeedMoveNoUnit(int terrainValue, short genId)
        {
            terrainValue &= 0x1F; // 保留低5位

            General general = GeneralListCache.GetGeneral(genId);
            bool shenSu = general.HasSkill(3, 5);// 特技神速
            byte[] army = general.army;

            switch (terrainValue)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 19:
                case 22:
                    return 2;

                case 10: // 树林
                case 12: // 山地
                    int movesNeed = 6;
                    if (shenSu)
                        movesNeed--;
                    movesNeed -= army[1];
                    return movesNeed;
                case 11: // 森林
                    movesNeed = 5;
                    movesNeed -= army[2];
                    return movesNeed;
                case 9:  // 水域
                case 15: // 雪地
                    movesNeed = 6;
                    if (shenSu)
                        movesNeed--;
                    movesNeed -= army[2];
                    return movesNeed;

                
            }

            return 120; // 默认值
        }
        
        static int GetCellUnit(byte byte0) // 判断块地上是玩家部队还是电脑部队，或者没有部队
        {
            var i1 = byte0 & 0xc0; // 通过位运算提取高两位
            if (i1 == 64)
                return 1; // 玩家部队
            return i1 != 128 ? 0 : 2; // 0是空地，2是电脑部队
        }
        
        // 检查两个坐标之间的距离是否小于等于指定的距离
        public static bool IsInRange(Vector2Int start, Vector2Int end, int distance)
        {
            return (Mathf.Abs(start.x - end.x) <= distance && Mathf.Abs(start.y - end.y) <= distance);
        }
        
        // 检查两个坐标是否相邻
        public static bool IsAdjacent(Vector2Int start, Vector2Int end)
        {
            return (Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y) == 1);
        }
        

        /// <summary>
        /// 获取相邻友军单位的数量
        /// </summary>
        /// <param name="beGeneral">目标将军</param>
        /// <param name="isPlayer">是否为玩家单位</param>
        /// <returns>相邻友军单位的数量</returns>
        public static byte GetAdjacentFriendNum(General beGeneral, bool isPlayer)
        {
            byte adjacentFriendCount = 0;

            // 获取对应的单位列表
            List<UnitObj> unitList = isPlayer ? WarManager.Instance.aiUnits : WarManager.Instance.hmUnits;

            // 根据将军 ID 查找对应单位
            UnitObj unitObj = unitList.FirstOrDefault(t => t.genID == beGeneral.generalId);
            if (unitObj == null) return adjacentFriendCount;

            Vector2Int curPosition = unitObj.arrayPos;

            // 友军标识，玩家友军为 1，AI 友军为 2
            byte friendUnitType = isPlayer ? (byte)2 : (byte)1;

            // 遍历相邻单元格
            foreach (Vector2Int direction in GetAdjacentDirections())
            {
                Vector2Int neighbor = curPosition + direction;

                // 检查邻居是否在有效范围内
                if (IsValidCell(neighbor.x, neighbor.y))
                {
                    // 检查是否是友军单位
                    if (GetCellUnit(WarMap[neighbor.y, neighbor.x]) == friendUnitType)
                    {
                        adjacentFriendCount++;
                    }
                }
            }

            return adjacentFriendCount;
        }

    
    
    



        // 开始战争前的准备
        public void PrepareWarStance(List<short> hmGeneralIds, List<short> aiGeneralIds)
        {
            LoadAndGenerateMap();
            if (WarManager.Instance.isHmDef)//AI进攻玩家防守
            {
                SpawnUnits(hmGeneralIds, true, true);
                SpawnUnits(aiGeneralIds, false, false);
            }
            else// 玩家进攻AI防守
            {
                SpawnUnits(hmGeneralIds, true, false);
                SpawnUnits(aiGeneralIds, false, true);
            }
        }

        List<Vector2Int> GetStance(List<short> generalIds, bool isDefender)
        {
            if (isDefender)
            {
                var posList= _defenderPos.OrderBy(x => Random.value).Take(generalIds.Count).ToList();
                posList[0] = WarManager.Instance.cityPos;
                return posList;
            }
            else
            {
                return _attackerPos.OrderBy(x => Random.value).Take(generalIds.Count).ToList();
            }
        }
       

        private void SpawnUnits(List<short> generalIds, bool isPlayer, bool isDefender)
        {
            var posList = GetStance(generalIds, isDefender);
            foreach (var generalId in generalIds)
            {
                var worldPosition = TurnToWorldPos(posList[generalIds.IndexOf(generalId)]);
                GameObject unitPre = Instantiate(unitPrefab, unitCanvas.transform);
                unitPre.transform.position = worldPosition;
                UnitData data = new UnitData(generalId, isPlayer, generalIds.IndexOf(generalId) == 0, 0, 0, false,
                    UnitState.None, posList[generalIds.IndexOf(generalId)]);
                WarManager.Instance.unitDataList.TryAdd(generalId,data);
                UnitObj unitObj = unitPre.AddComponent<UnitObj>();
                unitObj.Init(data);
                unitPre.name = unitObj.UnitName;
                unitObj.GetArmySprite();
                if (isPlayer)
                {
                    WarMap[unitObj.arrayPos.y, unitObj.arrayPos.x] |= 0x40;
                    WarManager.Instance.hmUnits.Add(unitObj);
                }
                else
                {
                    WarMap[unitObj.arrayPos.y, unitObj.arrayPos.x] |= 0x80;
                    WarManager.Instance.aiUnits.Add(unitObj);
                }
                AddUnit(unitObj.arrayPos, unitObj);
                Debug.Log("单位" + unitObj.genID + "在" + unitObj.arrayPos.x + "," + unitObj.arrayPos.y);
            }
        }
        
        public void RestoreUnitsPosition()
        {
            Debug.Log($"玩家单位数:{WarManager.Instance.hmUnits.Count},AI单位数:{WarManager.Instance.aiUnits.Count},字典单位数:{WarManager.UnitDictionary.Count}");
            WarManager.Instance.hmUnits.Clear();
            WarManager.Instance.aiUnits.Clear();
            WarManager.UnitDictionary.Clear();
            foreach (var unitData in WarManager.Instance.unitDataList.Values)
            {
                if (unitData.unitState == UnitState.Dead || unitData.unitState == UnitState.Retire || unitData.unitState == UnitState.Retreat)
                {
                    continue;
                }
                
                GameObject unitPre = Instantiate(unitPrefab, unitCanvas.transform);
                UnitObj unitObj = unitPre.AddComponent<UnitObj>();
                unitObj.Init(unitData);
                unitPre.name = unitObj.UnitName;
                unitObj.transform.position = TurnToWorldPos(unitObj.arrayPos);
                unitObj.GetArmySprite();
                if (unitObj.isPlayer)
                {
                    if (unitData.unitState == UnitState.Captive)
                    {
                        unitPre.SetActive(false);
                    }
                    else
                    {
                        WarMap[unitObj.arrayPos.y, unitObj.arrayPos.x] |= 0x40;
                        AddUnit(unitObj.arrayPos, unitObj);
                    }
                    WarManager.Instance.hmUnits.Add(unitObj);
                }
                else
                {
                    if (unitData.unitState == UnitState.Captive)
                    {
                        unitPre.SetActive(false);
                    }
                    else
                    {
                        WarMap[unitObj.arrayPos.y, unitObj.arrayPos.x] |= 0x80;
                        AddUnit(unitObj.arrayPos, unitObj);
                    }
                    WarManager.Instance.aiUnits.Add(unitObj);
                }
                Debug.Log("单位" + unitObj.genID + "在" + unitObj.arrayPos.x + "," + unitObj.arrayPos.y);
            }
        }
        
        public static Dictionary<Vector2Int, byte> GetMovableCellsAStar(UnitData unitObj)
        {
            Dictionary<Vector2Int, byte> movableCellsWithMoves = new Dictionary<Vector2Int, byte>(); // 记录单元格和剩余移动力
            PriorityQueue<Node> openList = new PriorityQueue<Node>((a, b) => a.F.CompareTo(b.F));  // 使用优先队列
            HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(); // 关闭列表

            // 起点节点
            Node startNode = new Node(unitObj.arrayPos, 0, 0);  // 从起点开始
            openList.Enqueue(startNode);

            while (openList.Count > 0)
            {
                // 从优先队列中取出 F 值最低的节点
                Node currentNode = openList.Dequeue();

                // 如果当前节点已在关闭列表中，跳过
                if (closedList.Contains(currentNode.position))
                    continue;

                // 将当前节点添加到关闭列表
                closedList.Add(currentNode.position);

                // 计算剩余移动力
                int remainingMoves = unitObj.moveBonus - currentNode.G;

                // 转换为 Tilemap 坐标并添加到结果列表，同时记录剩余移动力
                movableCellsWithMoves.Add(new Vector2Int(currentNode.position.x, currentNode.position.y), (byte)remainingMoves);

                // 检查相邻单元格
                foreach (Vector2Int direction in GetAdjacentDirections())
                {
                    Vector2Int neighbor = currentNode.position + direction;

                    // 检查邻居是否在有效范围内
                    if (!IsValidCell(neighbor.x, neighbor.y))
                        continue; // 跳过超出边界的单元格

                    // 检查邻居是否已在关闭列表中
                    if (closedList.Contains(neighbor))
                        continue; // 如果已经评估过，跳过

                    // 获取邻居的地形类型和消耗
                    byte terrainType = WarMap[neighbor.y, neighbor.x];
                    int moveCost = GetCellNeedMoves(terrainType, unitObj.genID);

                    // 计算新的 G 值
                    int newG = currentNode.G + moveCost;

                    // 检查行动力是否足够
                    if (newG > unitObj.moveBonus) // 确保 G 值不超过总行动力
                        continue; // 如果超出行动力限制，跳过

                    // 计算曼哈顿距离作为启发值 H
                    int H = ManhattanDistance(neighbor, unitObj.arrayPos);

                    // 计算 F 值
                    int F = newG + H;

                    // 创建或更新邻居节点
                    Node neighborNode = new Node(neighbor, newG, H, currentNode);
                    neighborNode.F = F;

                    // 添加到开启列表（优先队列）
                    openList.Enqueue(neighborNode);
                }
            }

            Debug.Log($"可移动单元格数量: {movableCellsWithMoves.Count}");
            return movableCellsWithMoves;
        }


        /// <summary>
        /// 获取启发值的方法：曼哈顿距离
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int ManhattanDistance(Vector2Int current, Vector2Int target)
        {
            return Mathf.Abs(current.x - target.x) + Mathf.Abs(current.y - target.y);
        }

    
        /// <summary>
        /// 获取相邻方向的方法
        /// </summary>
        /// <returns></returns>
        public static Vector2Int[] GetAdjacentDirections()
        {
            return new Vector2Int[]
            {
                new Vector2Int(1, 0), // 右
                new Vector2Int(-1, 0), // 左
                new Vector2Int(0, -1), // 下
                new Vector2Int(0, 1) // 上
            };
        }
    
        /// <summary>
        /// 检查单元格是否在数组地图范围内
        /// </summary>
        /// <param name="x">单元格的列索引</param>
        /// <param name="y">单元格的行索引</param>
        /// <returns>是否有效</returns>
        private static bool IsValidCell(int x, int y)
        {
            return x >= 0 && x < _cols && y >= 0 && y < _rows;
        }
    
        /// <summary>
        /// 地图上显示可移动到的单元格
        /// </summary>
        /// <param name="movableCellsWithMoves">可移动到的单元格列表</param>
        public void DisplayNavigation(Dictionary<Vector2Int, byte> movableCellsWithMoves)
        {
            foreach (var cell in movableCellsWithMoves)
            {
                Vector2Int arrayPos = cell.Key;
                Vector3 worldPos = TurnToWorldPos(arrayPos);
                if (GetUnitByXY(arrayPos) == null)
                {
                    // 实例化标记并添加到 panel 下
                    GameObject marker = Instantiate(markerPrefab, unitCanvas.transform);
                    marker.transform.position = worldPos; // 设置位置
                    // 添加按钮点击事件
                    marker.GetComponent<Button>().onClick.AddListener(() => OnMoveMarkerButtonClick((cell.Key, cell.Value)));
                    // 显示剩余移动力
                    TextMeshProUGUI textComponent = marker.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = cell.Value.ToString(); // 显示剩余移动力
                    }

                    // 将 marker 添加到 markers 列表中
                    _markers.Add(marker);
                }
            }
            Debug.Log("展示导航单元格");
        }

        /// <summary>
        /// 当移动单元格标记按钮被点击时调用的方法
        /// </summary>
        /// <param name="clickedCell">所点击的单元格标识坐标和行动力</param>
        private void OnMoveMarkerButtonClick((Vector2Int, byte) clickedCell)
        {
            var worldPos = TurnToWorldPos(clickedCell.Item1);
            if (_selectedCell == null)
            {
                // 第一次选择单元格
                _selectedCell = worldPos;
            }
            else
            {
                // 第二次点击确认移动
                if (_selectedCell.Value == worldPos)
                {
                    WarManager.Instance.hmUnitObj.MoveUnitToCell(clickedCell); // 移动将军到选中的位置
                    WarManager.Instance.hmUnitObj.SetUnitState(UnitState.None);
                    
                    _selectedCell = null; // 清空选中的单元格
                    UIWar.Instance.DisplayWarMenu();
                }
                else
                {
                    _selectedCell = worldPos; // 更新选中的单元格
                }
            }
        }
    
        /// <summary>
        /// 清除所有标记的方法
        /// </summary>
        public void ClearAllMarkers()
        {
            // 遍历 markers 列表，销毁所有标记
            foreach (GameObject marker in _markers)
            {
                if (marker != null && !marker.gameObject.CompareTag("QiMen"))
                {
                    Destroy(marker); // 销毁标记对象
                }
            }
            _selectedCell = null;
            // 清空 markers 列表
            _markers.Clear();
            Debug.Log("所有标记已清除");
        }
    
    






    
        /// <summary>
        /// 获取单位附近指定距离的单元格瓦片地图坐标及地形类型
        /// </summary>
        /// <param name="arrayPos">当前单位数组坐标</param>
        /// <param name="distance">扩展距离</param>
        /// <returns>所有有效单元格及地形类型的列表</returns>
        public static Dictionary<Vector2Int, byte> GetCellsInRange(Vector2Int arrayPos, int distance)
        {
            Dictionary<Vector2Int, byte> result = new Dictionary<Vector2Int, byte>();

            // 当前单位的数组坐标
            int arrayY = arrayPos.y;
            int arrayX = arrayPos.x;

            // 遍历范围内的所有单元格
            for (int offsetY = -distance; offsetY <= distance; offsetY++)
            {
                for (int offsetX = -distance; offsetX <= distance; offsetX++)
                {
                    // 计算目标单元格的数组坐标
                    int targetX = arrayX + offsetX;
                    int targetY = arrayY + offsetY;

                    // 排除自身格子
                    if (offsetX == 0 && offsetY == 0)
                        continue;

                    // 检查目标单元格是否在地图范围内
                    if (IsValidCell(targetX, targetY))
                    {
                        // 获取地形类型
                        byte terrainType = WarMap[targetY, targetX];

                        // 添加到结果列表
                        result.Add(new Vector2Int(targetX, targetY), terrainType);
                    }
                }
            }
            //Debug.Log("计谋地形数量:"+result.Count);
            return result;
        }
    
        /// <summary>
        /// 地图上显示筛选出当前可施展计谋的单元格标识
        /// </summary>
        /// <param name="planID">技能ID</param>
        /// <param name="planCells">技能范围单元格列表</param>
        public void DisplayPlanCells(byte planID, Dictionary<Vector2Int, byte> planCells)
        {
            // 获取计划信息
            Plan plan = Plan.GetPlan(planID);
            foreach (var pair in planCells)
            {
                // 当前瓦片坐标信息
                Vector3 worldPos = TurnToWorldPos(pair.Key);
                byte terrainType = pair.Value;
                
                // 实例化标记并设置位置
                Vector2Int arrayPos = pair.Key;
                GameObject marker = Instantiate(markerPrefab, unitCanvas.transform);
                marker.transform.position = worldPos;

                // 添加按钮点击事件
                Button buttonComponent = marker.GetComponent<Button>();
            
                // 设置标记显示的文本
                TextMeshProUGUI textComponent = marker.GetComponentInChildren<TextMeshProUGUI>();

                // 设置标记显示的颜色
                Image imageComponent = marker.GetComponentInChildren<Image>();
                if (planID != 15 && plan.IsApplicable(terrainType) && GetCellUnit(terrainType) == 2)
                {
                    buttonComponent.onClick.AddListener(() => OnPlanMarkerButtonClick(planID, arrayPos));
                    textComponent.text = $"{GeneralListCache.GetGeneral(GetUnitByXY(arrayPos).genID).generalName}"; // 显示标记
                    textComponent.color = Color.white;
                    imageComponent.color = Color.green;
                }
                else if (planID == 15  && plan.IsApplicable(terrainType))
                {
                    buttonComponent.onClick.AddListener(() => OnPlanMarkerButtonClick(planID, arrayPos));
                    textComponent.text = "〇"; // 显示标记
                    textComponent.color = Color.white;
                    imageComponent.color = Color.green;
                }
                else
                {
                    textComponent.text = String.Empty; // 显示为空
                    imageComponent.color = Color.red;
                }

                // 将标记添加到 markers 列表中
                _markers.Add(marker);
            }

            if (_markers.Count == 0)
            {
                UIWar.Instance.NotifyWarEvent("当前无处施展本计");
                WarManager.Instance.hmUnitObj.SetUnitState(UnitState.None);
            }
            else
            {
                Debug.Log("找到目标数:"+_markers.Count);
            }
        }

    
        /// <summary>
        /// 当选中计谋目标标识时触发的方法
        /// </summary>
        /// <param name="planID">技能ID</param>
        /// <param name="clickedCell">所选标识的坐标</param>
        public void OnPlanMarkerButtonClick(byte planID, Vector2Int clickedCell)
        { 
            Plan plan = Plan.GetPlan(planID);
            General doGen = GeneralListCache.GetGeneral(WarManager.Instance.hmUnitObj.genID);
            if (planID != 15)
            {
                UnitObj tarUnitObj = GetUnitByXY(clickedCell);
                short beGenId = tarUnitObj.genID;
                General beGen = GeneralListCache.GetGeneral(beGenId);
                if (plan is IMultiplePlan multiplePlan)
                {
                    if (!multiplePlan.IsMultipleTarget(beGen, true))
                    {
                        UIWar.Instance.NotifyWarEvent("此处无法施展本计");
                    }
                    else
                    {
                        WarManager.Instance.hmUnitObj.SubMoveBonus(plan.UseCost(doGen));
                    }
                }

                if (plan is IBeCommanderPlan beCommanderPlan)
                {
                    if (!beCommanderPlan.IsBeCommanderTarget(beGen, true))
                    {
                        UIWar.Instance.NotifyWarEvent("无法对其施展本计");
                    }
                    else
                    {
                        WarManager.Instance.hmUnitObj.SubMoveBonus(plan.UseCost(doGen));
                    }
                }
            
                if (plan.IsExecute(doGen, beGen, true))
                {
                    WarManager.Instance.hmUnitObj.SubMoveBonus(plan.UseCost(doGen));
                    WarManager.Instance.hmUnitObj.GetArmySprite();
                    tarUnitObj.GetArmySprite();
                }
                else
                {
                    WarManager.Instance.hmUnitObj.SubMoveBonus(plan.UseCost(doGen));
                }
                StartCoroutine(UIWar.Instance.uiPlanResult.ShowPlanResult(WarManager.Instance.planIndex, 
                    WarManager.Instance.planResult));
            }
            else// 奇门遁甲对地施放
            {
                // 获取目标瓦片的中心世界坐标
                Vector3Int targetCell = new Vector3Int(clickedCell.x, _rows - 1 - clickedCell.y, 0);
                Vector3 worldPosition = tilemap.GetCellCenterWorld(targetCell);

                WarManager.Instance.warMap[targetCell.y, targetCell.x] |= 0x20; // 新位置标记为已占用
            
                GameObject qiMenObj = Instantiate(markerPrefab, unitCanvas.transform);
                qiMenObj.transform.position = worldPosition; // 显示奇门遁甲标识
                qiMenObj.tag = "QiMen";
                Image imageComponent = qiMenObj.GetComponentInChildren<Image>();
                imageComponent.sprite = Resources.Load<Sprite>("War/Tile/QiMen");
                imageComponent.color = Color.white;
                qiMenObj.GetComponent<Button>().enabled = false;
                Text textComponent = qiMenObj.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = String.Empty; // 显示X
                }
                StartCoroutine(UIWar.Instance.uiPlanResult.ShowPlanResult(WarManager.Instance.planIndex, plan.Result(doGen, null ,true)));
                WarManager.Instance.hmUnitObj.SubMoveBonus(plan.UseCost(doGen));
                GeneralListCache.GetGeneral(WarManager.Instance.hmUnitObj.genID).generalSoldier -= 100;
                WarManager.Instance.hmUnitObj.GetArmySprite(); // 更新将军贴图
            }
            ClearAllMarkers();
            UIWar.Instance.DisplayWarMenu();
            WarManager.Instance.hmUnitObj.SetUnitState(UnitState.None);
        }


        public static Dictionary<Vector2Int, byte> GetCanAttackCell(Vector2Int arrayPos)
        {
            var x = arrayPos.x;
            var y = arrayPos.y;
            Dictionary<Vector2Int, byte> result = new Dictionary<Vector2Int, byte>();
            // 检查相邻单元格
            foreach (Vector2Int direction in GetAdjacentDirections())
            {
                Vector2Int neighbor = arrayPos + direction;

                // 检查邻居是否在有效范围内
                if (IsValidCell(neighbor.x, neighbor.y))
                {
                    // 获取地形类型
                    byte terrainType = WarMap[neighbor.y, neighbor.x];

                    // 添加到结果列表
                    result.Add(neighbor, terrainType);
                }
            }
            return result;
        }


        public void DisplayAttackCells(Dictionary<Vector2Int, byte> attackCells)
        {
            foreach (var pair in attackCells)
            {
                // 当前瓦片坐标信息
                Vector2Int arrayPos = pair.Key;
                Vector3 worldPos = TurnToWorldPos(pair.Key);
                byte terrainType = pair.Value;
                
                // 实例化标记并设置位置
                GameObject marker = Instantiate(markerPrefab, unitCanvas.transform);
                marker.transform.position = worldPos;

                // 添加按钮点击事件
                Button buttonComponent = marker.GetComponent<Button>();
            
                // 设置标记显示的文本
                TextMeshProUGUI textComponent = marker.GetComponentInChildren<TextMeshProUGUI>();

                // 设置标记显示的颜色
                Image imageComponent = marker.GetComponentInChildren<Image>();
                if (GetCellUnit(terrainType) == 2)
                {
                    buttonComponent.onClick.AddListener(() => OnAttackMarkerButtonClick(arrayPos));
                    textComponent.text = $"{GeneralListCache.GetGeneral(GetUnitByXY(arrayPos).genID).generalName}"; // 显示标记
                    textComponent.color = Color.white;
                    imageComponent.color = Color.green;
                }
                else
                {
                    textComponent.text = String.Empty; // 显示为空
                    imageComponent.color = Color.red;
                }

                // 将标记添加到 markers 列表中
                _markers.Add(marker);
            }
        }

        //TODO
        private void OnAttackMarkerButtonClick(Vector2Int arrayPos)
        {
            var targetUnit = GetUnitByXY(arrayPos);
            if (targetUnit != null)
            {
                WarManager.Instance.aiUnitObj = targetUnit;
                WarManager.Instance.hmUnitObj.SubMoveBonus(2);
                WarManager.Instance.isBattle = true;
                WarManager.Instance.battleState = BattleState.None;
                SceneManager.LoadScene("PreBattleScene");
            }
            else
            {
                Debug.LogError("未找到目标单位");
            }
        }
                                
        /// <summary>
        /// 获取从起点到目标的最低移动力消耗路径（包括起点的第一步和目标的最后一步）。
        /// </summary>
        /// <param name="targetPosition">目标位置。</param>
        /// <param name="unit">单位对象。</param>
        /// <returns>路径上每个节点的坐标及单步行动力消耗。</returns>
        public static List<(Vector2Int position, byte cost)> GetPathToTarget(Vector2Int targetPosition, UnitData unit, bool ignoreAI)
        {
            // 使用支持删除的优先队列实现
            PriorityQueue<Node> openList = new PriorityQueue<Node>((a, b) => a.G.CompareTo(b.G));
            Dictionary<Vector2Int, Node> visitedNodes = new Dictionary<Vector2Int, Node>();

            // 初始化起点节点
            Node startNode = new Node(unit.arrayPos, 0, 0);
            openList.Enqueue(startNode);
            visitedNodes[unit.arrayPos] = startNode;

            Node targetNode = null;

            while (openList.Count > 0)
            {
                // 从优先队列中取出 G 值最低的节点
                Node currentNode = openList.Dequeue();

                // 如果到达目标节点，记录目标节点
                if (currentNode.position == targetPosition)
                {
                    targetNode = currentNode;
                    break;
                }

                // 遍历邻居节点
                foreach (Vector2Int direction in GetAdjacentDirections())
                {
                    Vector2Int neighbor = currentNode.position + direction;

                    // 如果邻居无效，跳过
                    if (!IsValidCell(neighbor.x, neighbor.y))
                        continue;

                    // 获取邻居的地形消耗
                    byte terrainType = ignoreAI ? (byte)(WarMap[neighbor.y, neighbor.x] & 0x7F) : WarMap[neighbor.y, neighbor.x];
                    byte moveCost = (byte)GetCellNeedMoves(terrainType, unit.genID);

                    // 计算新 G 值
                    int newG = currentNode.G + moveCost;

                    // 检查是否访问过此节点
                    if (visitedNodes.TryGetValue(neighbor, out Node existingNode))
                    {
                        // 如果当前路径更优，更新 G 值和父节点
                        if (newG < existingNode.G)
                        {
                            openList.Remove(existingNode); // 从队列中移除旧节点
                            existingNode.G = newG;
                            existingNode.parent = currentNode;
                            openList.Enqueue(existingNode); // 加入更新后的节点
                        }
                    }
                    else
                    {
                        // 如果未访问过，创建新节点并加入队列
                        Node neighborNode = new Node(neighbor, newG, 0, currentNode);
                        openList.Enqueue(neighborNode);
                        visitedNodes[neighbor] = neighborNode;
                    }
                }
            }

            return targetNode != null ? ReconstructPath(targetNode, unit.genID) : new List<(Vector2Int position, byte cost)>();
        }

        private static List<(Vector2Int position, byte cost)> ReconstructPath(Node targetNode, short genID)
        {
            List<(Vector2Int position, byte cost)> path = new();
            Node currentNode = targetNode;

            // 回溯路径
            while (currentNode.parent != null) // 忽略起点
            {
                byte terrainType = WarMap[currentNode.position.y, currentNode.position.x];
                byte moveCost = (byte)GetCellNeedMoves(terrainType, genID); // 当前点的单步消耗
                path.Add((currentNode.position, moveCost));
                currentNode = currentNode.parent;
            }

            path.Reverse(); // 翻转路径以保持顺序
            return path;
        }





        
        public static HashSet<Vector2Int> ExpandRegion(HashSet<Vector2Int> originalRegion, int expansionDistance)
        {
            // 保存扩张后的区域
            HashSet<Vector2Int> expandedRegion = new HashSet<Vector2Int>(originalRegion);
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>(originalRegion); // 防止重复处理

            // 用于广度优先搜索的队列
            Queue<(Vector2Int position, int distance)> queue = new Queue<(Vector2Int position, int distance)>();

            // 初始化队列：从原区域边缘开始扩张
            foreach (var cell in originalRegion)
            {
                queue.Enqueue((cell, 0));
            }

            // 广度优先搜索
            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();

                // 如果超出扩张范围，停止处理
                if (distance >= expansionDistance)
                    continue;

                // 遍历当前格子周围的相邻格子
                foreach (var direction in GetAdjacentDirections())
                {
                    Vector2Int neighbor = current + direction;

                    // 检查邻居是否在棋盘范围内或者邻居已经处理过，跳过
                    if (!IsValidCell(neighbor.x, neighbor.y) || visited.Contains(neighbor))
                        continue;

                    // 标记为已处理
                    visited.Add(neighbor);
                    expandedRegion.Add(neighbor);

                    // 将邻居添加到队列中，增加距离
                    queue.Enqueue((neighbor, distance + 1));
                }
            }

            return expandedRegion;
        }

    }

    // A星算法定义节点类
    public class Node
    {
        public Vector2Int position; // 该节点的位置
        public int G; // 当前消耗的行动力
        public int F; // 优先级 (G + H)
        public Node parent; // 父节点

        public Node(Vector2Int position, int g, int f, Node parent = null)
        {
            this.position = position;
            this.G = g;
            this.F = f;
            this.parent = parent;
        }
    }
    
    // A星算法优先队列
    public class PriorityQueue<T>
    {
        private readonly List<T> _heap = new();
        private readonly Comparison<T> _comparer;

        public PriorityQueue(Comparison<T> comparer)
        {
            _comparer = comparer;
        }

        public int Count => _heap.Count;

        /// <summary>
        /// 添加元素到优先队列
        /// </summary>
        public void Enqueue(T item)
        {
            _heap.Add(item);
            HeapifyUp(_heap.Count - 1);
        }

        /// <summary>
        /// 删除并返回优先队列中的最小值
        /// </summary>
        public T Dequeue()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("PriorityQueue is empty.");

            T topItem = _heap[0];
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
                HeapifyDown(0);

            return topItem;
        }

        /// <summary>
        /// 从优先队列中移除指定元素
        /// </summary>
        public bool Remove(T item)
        {
            int index = _heap.IndexOf(item);
            if (index == -1)
                return false;

            // 将要移除的元素与堆末尾的元素交换
            _heap[index] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            // 修复堆结构
            if (index < _heap.Count)
            {
                HeapifyDown(index);
                HeapifyUp(index);
            }

            return true;
        }

        /// <summary>
        /// 检查队列中是否包含某个元素
        /// </summary>
        public bool Contains(T item) => _heap.Contains(item);

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_comparer(_heap[index], _heap[parentIndex]) >= 0)
                    break;

                (_heap[index], _heap[parentIndex]) = (_heap[parentIndex], _heap[index]);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = _heap.Count - 1;

            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if (leftChild <= lastIndex && _comparer(_heap[leftChild], _heap[smallest]) < 0)
                    smallest = leftChild;

                if (rightChild <= lastIndex && _comparer(_heap[rightChild], _heap[smallest]) < 0)
                    smallest = rightChild;

                if (smallest == index)
                    break;

                (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
                index = smallest;
            }
        }
    }



}