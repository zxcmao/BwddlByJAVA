using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using DataClass;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using War;
using Random = UnityEngine.Random;

namespace BaseClass
{
    public enum TroopType
    {
        Captain,
        Cavalry,
        Archer,
        Infantry
    }

    public enum TroopState
    {
        Forward,     // 前进
        Idle,        // 待命
        BackWard,    // 撤退
        Outflank,    // 包围
        Exit,        // 退出小战场
    }

    [Serializable]
    public class TroopData
    {
        public TroopType troopType;
        public TroopState troopState;
        public short health;
        public short generalID;
        public bool isPlayer;
        public byte index;
        public short attackPower;
        public short defensePower;
        public byte actBonus;
        public byte face;
        public Vector2Int arrayPos;

        public TroopData(TroopType troopType, short generalID, bool isPlayer)
        {
            this.troopType = troopType;
            this.generalID = generalID;
            this.isPlayer = isPlayer;
            
            InitPower();
            InitActBonus();
        }
        public static TroopData Copy(TroopData origin)
        {
            TroopData copy = new TroopData(origin.troopType, origin.generalID, origin.isPlayer);
            copy.troopState = origin.troopState;
            copy.health = origin.health;
            copy.face = origin.face;
            copy.arrayPos = origin.arrayPos;
            return copy;
        }
        private void InitPower()
        {
            General general = GeneralListCache.GetGeneral(generalID);
            //TODO
            bool isAttacking = false;
            /*if (isPlayer)
            {
                isAttacking = WarManager.Instance.warState == WarState.PlayerTurn;
            }
            else
            {
                isAttacking = WarManager.Instance.warState == WarState.AITurn;
            }*/
            
            if (troopType == TroopType.Captain)
            {
                attackPower = general.GetAttackPower();
                defensePower = general.GetDefendPower();
            }
            else
            {
                attackPower = (short)((general.lead * 2 + general.force) / 3 + (general.lead + general.force) * (general.level - 1) / 25);
                defensePower = (short)((general.lead * 2 + general.IQ) / 3 + (general.lead + general.IQ) * (general.level - 1) / 25);
                if (general.HasSkill(3, 7))// 特技军神
                {
                    attackPower += (short)(attackPower/4);
                    defensePower += (short)(defensePower/4);
                }
                else if (general.HasSkill(3, 8))// 特技军魂本体
                {
                    attackPower += (short)(attackPower/5);
                    defensePower += (short)(defensePower/5);
                }
                else if (general.HasSkill(3, 3))// 攻城特技
                {
                    if (BattleManager.Instance.battleTerrain == 8 && isAttacking)
                    {
                        attackPower += (short)(attackPower/5);
                        defensePower += (short)(defensePower/5);
                    }
                }
                else if(general.HasSkill(3, 4))// 守城特技
                {
                    if (BattleManager.Instance.battleTerrain == 8 && !isAttacking)
                    {
                        attackPower += (short)(attackPower/5);
                        defensePower += (short)(defensePower/5);
                    }
                }
                /*else if (WarManager.Instance.TriggerJunHunHalo(generalID, isPlayer)) // 军魂光环特技
                {
                    attackPower += (short)(attackPower/7);
                    defensePower += (short)(defensePower/7);
                }*/

                if (general.HasSkill(3, 1))// 奇袭特技
                {
                    if (BattleManager.Instance.battleTerrain != 8 && isAttacking)
                    {
                        attackPower += (short)(attackPower/7);
                        defensePower += (short)(defensePower/7);
                    }
                }

                if (general.HasSkill(3, 2))// 铁壁特技
                {
                    if (BattleManager.Instance.battleTerrain != 8 && !isAttacking)
                    {
                        attackPower += (short)(attackPower/7);
                        defensePower += (short)(defensePower/7);
                    }
                }
            
                if (BattleManager.Instance.battleTerrain == 8 && !isAttacking)// 守城本身自带加强
                {
                    attackPower += (short)(attackPower/2);
                    defensePower += (short)(defensePower/2);
                }

                /*if (!isPlayer)// AI增强
                {
                    attackPower += (short)(attackPower);
                    defensePower += (short)(defensePower);
                }*/
                // 地形因素影响
                attackPower = (short)(attackPower * FloorManager.GetTef(BattleManager.Instance.battleTerrain, general));
                defensePower = (short)(defensePower * FloorManager.GetTef(BattleManager.Instance.battleTerrain, general));

                if (troopType == TroopType.Archer)
                {
                    attackPower = (short)(attackPower * 2 / 3);
                    defensePower = (short)(defensePower * 4 / 5);
                }
                else if (troopType == TroopType.Infantry)
                {
                    attackPower = (short)(attackPower * 4 / 5);
                    defensePower = (short)(defensePower * 6 / 5);
                }
            }
        }

        public void InitActBonus()
        {
            switch (troopType)
            {
                case TroopType.Captain:
                case TroopType.Cavalry:
                    actBonus = 2;
                    break;
                case TroopType.Archer:
                case TroopType.Infantry:
                    actBonus = 1;
                    break;
            }
        }
    }
    
    public abstract class Troop : MonoBehaviour
    {
        public TroopData data;
        public TroopType troopType => data.troopType;       // 部队类型
        public short health => data.health;                 // 生命值
        public byte currentActionPoints => data.actBonus;    // 当前行动次数
        public short generalID => data.generalID;            // 所属将军ID
        public bool isPlayer => data.isPlayer;              // 是否是玩家或电脑
        public byte index => data.index;                  // 部队索引
        public short attackPower => data.attackPower;            // 攻击力
        public short defensePower => data.defensePower;           // 防御力

        public Vector2Int arrayPos => data.arrayPos;        // 数组地图位置
        public byte face => data.face;                  // 面朝方向
        public bool isActing;              // 正在行动
        public TroopState currentState => data.troopState;      // 行动状态
        public byte canActionPoints = 1;           // 行动次数
        public Animator troopAnimation;     // 受击动画

    
        public List<(byte x, byte y)> detectedPositions;
    
        private const float 难度系数 = 1f;
        private const float 电脑战斗增幅 = 1.25f;
        private const float 军神系数 = 1.25f;
        private const float 军魂本体系数 = 1.2f;
        private const float 军魂光环系数 = 1.15f;
        private const float 攻城系数 = 1.2f;
        private const float 守城系数 = 1.2f;
        private const float 奇袭系数 = 1.15f;
        private const float 铁壁系数 = 1.15f;
        private const float 守城基础系数 = 1.5f;// 非特技效果守城本身自带

        // Troop 执行状态命令方法
        public IEnumerator ExecuteAction()
        {
            // 如果当前回合列表为空或索引超出范围，结束回合
            while (currentActionPoints > 0)
            {
                byte[] directions = null;
                Troop attackableObj = null;
                byte moveableDir = 255;
                // 根据当前状态来执行行为
                switch (currentState)
                {
                    case TroopState.Forward:
                        directions = isPlayer ? new byte[] { 3, 0, 1 , 2 } : new byte[] { 2, 0, 1 ,3 };
                        attackableObj = PerformTargetCheck(directions);
                        if (attackableObj != null)
                        {
                            Attack(attackableObj);
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name}无攻击目标");
                            moveableDir = MoveForward();
                            if (moveableDir != 255)
                            {
                                MoveByDirection(moveableDir);
                            }
                            else
                            {
                                Debug.Log("随机移动");
                                MoveByDirection(GetRandomDirection(GetAllMovableDirection()));
                            }
                        }
                        data.actBonus--;
                        yield return new WaitForSeconds(0.5f);
                        break;
                    case TroopState.Outflank:
                        directions = isPlayer ? new byte[] { 3 } : new byte[] { 2 };
                        byte outflankDir = MoveToEnemyGeneral();
                        if (outflankDir != 255)
                        {
                            MoveByDirection(outflankDir);
                        }
                        else
                        {
                            Attack(PerformTargetCheck(directions));
                        }
                        data.actBonus--;
                        yield return new WaitForSeconds(0.5f);
                        break;
                    case TroopState.BackWard:
                        if (IsRetreat())
                            yield break;
                        directions = isPlayer ? new byte[1] { 2 } : new byte[1] { 3 };
                        byte backWard = MoveBackWard();
                        if (backWard != 255)
                        {
                            MoveByDirection(backWard);
                        }
                        else
                        {
                            Attack(PerformTargetCheck(directions));
                        }
                        data.actBonus--;
                        yield return new WaitForSeconds(0.5f);
                        break;
                    case TroopState.Idle:
                        directions = isPlayer ? new byte[4] { 3, 0, 1 ,2} : new byte[4] { 2, 0, 1 ,3};
                        attackableObj = PerformTargetCheck(directions);
                        if (attackableObj != null)
                        {
                            Attack(attackableObj);
                            data.actBonus--;
                            yield return new WaitForSeconds(0.5f);
                        }
                        else
                        {
                            data.actBonus--;
                        }
                        break;
                }
            }
        }

        public void Init(TroopData initData)
        {
            data = initData;
        }
    
        public void ResetActionPoints()
        {
            data.InitActBonus();
        }

        public void SetTroopIndex(byte index)
        {
            data.index = index;
        }
        // 存储不同 face 和 troopType 的 Sprite 索引及翻转信息
        private readonly Dictionary<(byte face, bool isPlayer, TroopType troopType), byte> spriteMapping =
            new Dictionary<(byte, bool, TroopType), byte>
            {
                // face 0: 上
                { (0, false, TroopType.Captain), 0 },
                { (0, false, TroopType.Cavalry), 1 },
                { (0, false, TroopType.Archer), 2 },
                { (0, false, TroopType.Infantry), 3 },
                { (0, true, TroopType.Captain), 4 },
                { (0, true, TroopType.Cavalry), 5 },
                { (0, true, TroopType.Archer), 6 },
                { (0, true, TroopType.Infantry), 7 },

                // face 1: 下
                { (1, false, TroopType.Captain), 8 },
                { (1, false, TroopType.Cavalry), 9 },
                { (1, false, TroopType.Archer), 10 },
                { (1, false, TroopType.Infantry), 11 },
                { (1, true, TroopType.Captain), 12 },
                { (1, true, TroopType.Cavalry), 13 },
                { (1, true, TroopType.Archer), 14 },
                { (1, true, TroopType.Infantry), 15 },

                // face 2: 左
                { (2, false, TroopType.Captain), 16 },
                { (2, false, TroopType.Cavalry), 17 },
                { (2, false, TroopType.Archer), 18 },
                { (2, false, TroopType.Infantry), 19 },
                { (2, true, TroopType.Captain), 20 },
                { (2, true, TroopType.Cavalry), 21 },
                { (2, true, TroopType.Archer), 22 },
                { (2, true, TroopType.Infantry), 23 },

                // face 3: 右
                { (3, false, TroopType.Captain), 24 },
                { (3, false, TroopType.Cavalry), 25 },
                { (3, false, TroopType.Archer), 26 },
                { (3, false, TroopType.Infantry), 27 },
                { (3, true, TroopType.Captain), 28 },
                { (3, true, TroopType.Cavalry), 29},
                { (3, true, TroopType.Archer), 30 },
                { (3, true, TroopType.Infantry), 31 },
            };

        public byte JudgeFace(byte targetX, byte targetY)
        {
            if (arrayPos.y == targetY) return (byte)((arrayPos.x < targetX)?3:2);
            if (arrayPos.x == targetX) return (byte)((arrayPos.y < targetY)?1:0);
            return 255;
        }
    
        // 转向方法更换图片
        public void TurnDirection(byte faceDirection)
        {
            data.face = faceDirection;

            if (gameObject == null)
            {
                Debug.Log($"{arrayPos.x},{arrayPos.y}无游戏对象");
            }
            // 从字典中获取 spriteIndex 和 flipX
            if (spriteMapping.TryGetValue((face, isPlayer, troopType), out var spriteData))
            {
                SpriteRenderer image = gameObject.GetComponent<SpriteRenderer>();
                SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Battle/TroopsAtlas");// 部队的精灵
                image.sprite = spriteAtlas.GetSprite($"Troops_{spriteData}");
            }
            else
            {
                Debug.LogWarning($"未找到 face:{face}, isPlayer:{isPlayer}, troopType:{troopType} 的 Sprite 配置");
            }
        }

        // 辅助方法：检测指定坐标是否可通行
        private bool IsPassable(int x, int y)
        {
            return x is >= 0 and <= 15 && y is >= 0 and <= 6 && (BattleManager.Instance.battleMap[y, x] <= 16);
        }
    
        // 辅助方法：检测所有可攻击的方向
        public List<byte> GetAllAttackableDirection(int currentX, int currentY)
        {
            List<byte> attackableDirections = new List<byte>();
            if (IsEnemy(currentX, currentY - 1)) attackableDirections.Add(0); // 向上
            if (IsEnemy(currentX, currentY + 1)) attackableDirections.Add(1); // 向下
            if (IsEnemy(currentX - 1, currentY)) attackableDirections.Add(2); // 向左
            if (IsEnemy(currentX + 1, currentY)) attackableDirections.Add(3); // 向右
            return attackableDirections;
        }
    
        // 辅助方法：随机选择一个可行方案
        public byte GetRandomDirection(List<byte> feasibleDirections)
        {
            if (feasibleDirections == null || feasibleDirections.Count == 0) return 255;
            return feasibleDirections[Random.Range(0, feasibleDirections.Count)];
      
        }
    
        // 辅助方法：获取所有可移动方向
        public List<byte> GetAllMovableDirection()
        {
            List<byte> movableDirections = new List<byte>();
            if (IsPassable(arrayPos.x - 1, arrayPos.y)) movableDirections.Add(2); // 向左
            if (IsPassable(arrayPos.x + 1, arrayPos.y)) movableDirections.Add(3); // 向右
            if (IsPassable(arrayPos.x, arrayPos.y - 1)) movableDirections.Add(0); // 向上
            if (IsPassable(arrayPos.x, arrayPos.y + 1)) movableDirections.Add(1); // 向下
            return movableDirections;
        }
    
        // 辅助方法：获取优先移动方向，按目标接近逻辑设置（右优先或左优先）
        private byte GetPreferredDirection(int selfX, int selfY)
        {
            List<(short dx, short dy, byte direction)> directions = new List<(short, short, byte)>();

            if (isPlayer)
            {
                // 玩家：优先向右，然后向上、向下，最后尝试向目标靠近
                directions.Add((1, 0, 3)); // 向右
                directions.Add((0, -1, 0)); // 向上
                directions.Add((0, 1, 1)); // 向下
                directions.Add((-1, 0, 2)); // 向左
            }
            else
            {
                // AI：优先向左，然后向上、向下，最后尝试向目标靠近
                directions.Add((-1, 0, 2)); // 向左
                directions.Add((0, -1, 0)); // 向上
                directions.Add((0, 1, 1)); // 向下
                directions.Add((1, 0, 3)); // 向右
            }

            // 按优先级检查是否可通行并移动
            for (byte i = 0; i < directions.Count; i++)
            {
                byte newX = (byte)(selfX + directions[i].dx);
                byte newY = (byte)(selfY + directions[i].dy);
                if (IsPassable(newX, newY)) return directions[i].direction;
            }
        
            // 无法移动时返回 255
            Debug.Log($"部队在{selfX},{selfY}被包围，无法获取优先方向");
            return 255;
        }
    
    
        /// <summary>
        /// 单位前进的移动逻辑：
        /// - 玩家默认向右移动，AI 默认向左移动。
        /// - 碰到障碍物时尝试接近目标，同时避免后退。
        /// - 当目标上下相邻时，尝试占据目标右侧的相邻单元格。
        /// </summary>
        /// <returns>移动方向：0=向上，1=向下，2=向左，3=向右，255=无法移动</returns>
        public byte MoveForward()
        {
            // 获取当前单位和目标的坐标
            byte selfX = (byte)arrayPos.x;
            byte selfY = (byte)arrayPos.y;
            Captain enemyCaptain = BattleManager.Instance.GetEnemyCaptain(isPlayer);
            byte targetX = (byte)enemyCaptain.arrayPos.x;
            byte targetY = (byte)enemyCaptain.arrayPos.y;
        
            // 移动到目标的对角线位置
            if (selfX - targetX == 1 && selfY - targetY == 1)// 右下角
            {
                if (isPlayer && IsPassable(selfX, selfY - 1)) return 0;
                if (!isPlayer && IsPassable(selfX - 1, selfY)) return 2;
            }
            else if (targetX - selfX == 1 && selfY - targetY == 1)// 左下角
            {
                if (isPlayer && IsPassable(selfX + 1, selfY)) return 3;
                if (!isPlayer && IsPassable(selfX, selfY - 1)) return 0;
            }
            else if (targetX - selfX == 1 && targetY - selfY == 1)// 左上角
            {
                if(isPlayer && IsPassable(selfX + 1, selfY)) return 3;
                if (!isPlayer && IsPassable(selfX, selfY + 1)) return 1;
            }
            else if (selfX - targetX == 1 && targetY - selfY == 1)// 右上角
            {
                if (isPlayer && IsPassable(selfX, selfY + 1)) return 1;
                if (!isPlayer && IsPassable(selfX - 1, selfY)) return 2;
            }
        
            // 如果目标同Y轴
            if (selfX == targetX)
            {   // 优先尝试占据目标右侧相邻单元格（玩家）或左侧（AI）
                if (Math.Abs(selfY - targetY) == 1)// 上下相邻检测左右相邻
                {
                    int offsetX = isPlayer ? 1 : (-1); // 右侧（玩家）或左侧（AI）
                    if (IsPassable((byte)(targetX + offsetX), targetY))
                    {
                        if (IsPassable((byte)(selfX + offsetX), selfY))
                            return isPlayer ? (byte)3 : (byte)2; // 向右或向左
                    }
                }
                else if (selfY > targetY)
                {
                    if (IsPassable(selfX, selfY - 1)) return 0;
                    int offsetX = isPlayer ? 1 : (-1); // 右侧（玩家）或左侧（AI）
                    if (IsPassable((byte)(targetX + offsetX), targetY))
                    {
                        if (IsPassable((byte)(selfX + offsetX), selfY))
                            return isPlayer ? (byte)3 : (byte)2; // 向右或向左
                    }
                }
                else if (selfY < targetY)
                {
                    if (IsPassable(selfX, selfY + 1)) return 1;
                    int offsetX = isPlayer ? 1 : (-1); // 右侧（玩家）或左侧（AI）
                    if (IsPassable((byte)(targetX + offsetX), targetY))
                    {
                        if (IsPassable((byte)(selfX + offsetX), selfY))
                            return isPlayer ? (byte)3 : (byte)2; // 向右或向左
                    }
                }; // 向上或向下
            }
        
            // 如果与目标同X轴
            if (selfY == targetY)
            {
                if (Math.Abs(selfX - targetX) == 1)
                {
                    int offsetX = isPlayer ? 1 : (-1); // 右侧（玩家）或左侧（AI）
                    if (selfX == targetX + offsetX) return 255;
                }
            }
        
            // 如果越过了目标位置返回移动
            if (selfX > targetX)
            {
                if (isPlayer && selfX - targetX == 1)
                {
                    if (selfY > targetY && IsPassable(selfX, selfY - 1)) return 0;
                    if (selfY < targetY && IsPassable(selfX, selfY + 1)) return 1;
                }
                else
                {
                    if (IsPassable(selfX - 1, selfY)) return 2;
                }
            }

            if (selfX < targetX)
            {
                if (!isPlayer && targetX - selfX == 1)
                {
                    if (selfY > targetY && IsPassable(selfX, selfY - 1)) return 0;
                    if (selfY < targetY && IsPassable(selfX, selfY + 1)) return 1;
                }
                else
                {
                    if (IsPassable(selfX + 1, selfY)) return 3;
                }
            } 
        
            // 根据优先级计算方向
            byte direction = GetPreferredDirection(selfX, selfY);
            if (direction != 255) return direction;
        
            // 无法移动时返回 255
            return 255;
        }

    
        // 包围命令时移动方法
        public byte MoveToEnemyGeneral()
        {
            // 获取当前坐标和目标坐标
            byte selfX = (byte)arrayPos.x;
            byte selfY = (byte)arrayPos.y;
            Captain enemyCaptain = BattleManager.Instance.GetEnemyCaptain(isPlayer);
            byte targetX = (byte)enemyCaptain.arrayPos.x;
            byte targetY = (byte)enemyCaptain.arrayPos.y;
            // 判断与目标的相对位置
            if (selfX == targetX && selfY == targetY) return 255; // 当前与目标重叠
        
            // 移动到目标的对角线位置
            if (selfX - targetX == 1 && selfY - targetY == 1)// 右下角
            {
                if (isPlayer && IsPassable(selfX, selfY - 1)) return 0;
                if (!isPlayer && IsPassable(selfX - 1, selfY)) return 2;
            }
            else if (targetX - selfX == 1 && selfY - targetY == 1)// 左下角
            {
                if (isPlayer && IsPassable(selfX + 1, selfY)) return 3;
                if (!isPlayer && IsPassable(selfX, selfY - 1)) return 0;
            }
            else if (targetX - selfX == 1 && targetY - selfY == 1)// 左上角
            {
                if(isPlayer && IsPassable(selfX + 1, selfY)) return 3;
                if (!isPlayer && IsPassable(selfX, selfY + 1)) return 1;
            }
            else if (selfX - targetX == 1 && targetY - selfY == 1)// 右上角
            {
                if (isPlayer && IsPassable(selfX, selfY + 1)) return 1;
                if (!isPlayer && IsPassable(selfX - 1, selfY)) return 2;
            }
        
            // 移动到相邻的目标位置
            if (selfY == targetY)
            {
                if (Math.Abs(selfX - targetX) == 1)
                {
                    return 255;
                }
                if (selfX > targetX && IsPassable(selfX - 1, selfY))
                {
                    return 2;
                }
                if (selfX < targetX && IsPassable(selfX + 1, selfY))
                {
                    return 3;
                }
            }
            // 移动到同Y轴
            if (selfX == targetX)
            {
                if (Math.Abs(selfY - targetY) == 1)
                {
                    if (isPlayer)
                    {
                        if (IsPassable(targetX + 1, targetY)) return 3;
                        return 255;
                    }
                    else
                    {
                        if (IsPassable(targetX - 1, targetY)) return 2;
                        return 255;
                    }
                }
                if (selfY > targetY && IsPassable(selfX, selfY - 1))
                {
                    return 0;
                }
                if (selfY < targetY && IsPassable(selfX, selfY + 1))
                {
                    return 1;
                }
            }
       
            // 处理边界和特殊情况
            if (selfY == 0) // 最顶部
            {
                if (IsPassable(selfX + 1, selfY)) return 3; // 向右
                if (IsPassable(selfX, selfY + 1)) return 1; // 向下
            }
            else if (selfY == 6) // 最底部
            {
                if (IsPassable(selfX + 1, selfY)) return 3; // 向右
                if (IsPassable(selfX, selfY - 1)) return 0; // 向上
            }
            else // 中间位置
            { 
                // 根据优先级计算方向
                byte direction = GetPreferredDirection(selfX, selfY);
                if (direction != 255) return direction;
            }

            return 255; // 无法移动
    
        }
    
        // 撤退移动
        public byte MoveBackWard()
        {
            byte selfX = (byte)arrayPos.x;
            byte selfY = (byte)arrayPos.y;

            if (isPlayer)
            {
                if (IsPassable(selfX - 1, selfY)) return 2;
                if (IsPassable(selfX, selfY - 1)) return 0;
                if (IsPassable(selfX, selfY + 1)) return 1;
            }
            else
            {
                if (IsPassable(selfX - 1, selfY)) return 3;
                if (IsPassable(selfX, selfY - 1)) return 0;
                if (IsPassable(selfX, selfY + 1)) return 1;
            }
            return 255;
        }
    
        // 移动到数组对应位置的世界坐标
        public void ArrayPositionToWorld()
        {
            // 部队移动逻辑， 转换目标位置为世界坐标
            Vector3 worldTargetPosition = FloorManager.Instance.BattleTilemap.GetCellCenterWorld(new Vector3Int(
                arrayPos.x -8, 
                8- arrayPos.y,
                0));
            // 移动Prefab到目标位置（这里只是简单设置position，实际应用中可能需要平滑移动）
            if (gameObject != null)
            {
                gameObject.transform.position = worldTargetPosition;
            }
            else
            {
                Debug.Log($"{arrayPos.x},{arrayPos.y}未能移动到世界坐标");
            }
        }
    
    
        // 世界坐标转换成部队所在数组坐标BattleArrayPosition的方法
        public void WorldPositionToArray()
        {
            if (gameObject != null && FloorManager.Instance.BattleTilemap != null)
            {
                Vector3Int tilePosition = FloorManager.Instance.BattleTilemap.WorldToCell(gameObject.transform.position);
                data.arrayPos = new Vector2Int(
                    tilePosition.x - 8,
                    8 - tilePosition.y // 根据你的地图方向调整这个计算
                );
            }
            else
            {
                Debug.LogError("Prefab或BattleTilemap为空!无法更新BattleArrayPosition");
            }
        }

        public void MoveByDirection(byte direction)
        {
            TurnDirection(direction);
            switch (direction)
            {
                case 0:
                    MoveUp();
                    break;
                case 1:
                    MoveDown();
                    break;
                case 2:
                    MoveLeft();
                    break;
                case 3:
                    MoveRight();
                    break;
                case 255:
                    break;
            }
        }
    
        // 向上移动数组处理
        private void MoveUp()
        {
            if (troopType == TroopType.Captain)
            {
                if (arrayPos.y<=0) return;
            }
            byte lastPosition = (byte)(BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] & 0xc0);
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] &= 0x32;
            data.arrayPos.y--;
            //canActionPoints = 0;
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] |= lastPosition;
            ArrayPositionToWorld();//移动到数组对应位置
        }

        public void MoveDown()
        {
            if (troopType == TroopType.Captain)
            {
                if (arrayPos.y>=5) return;
            }
            byte lastPosition = (byte)(BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] & 0xC0);
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] &= 0x32;
            data.arrayPos.y++;
            //canActionPoints = 1;
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] |= lastPosition;
            ArrayPositionToWorld();//移动到数组对应位置
        }

        public void MoveLeft()
        {
            if(arrayPos.x > 15 || arrayPos.y > 6) return;
            byte lastPosition = (byte)(BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] & 0xC0);
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] &= 0x32;
            data.arrayPos.x--;
            //canActionPoints = 2;
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] |= lastPosition;
            ArrayPositionToWorld();//移动到数组对应位置
        }

        public void MoveRight()
        {
            if(arrayPos.x > 15 || arrayPos.y > 6) return;
            byte lastPosition = (byte)(BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] & 0xC0);
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] &= 0x32;
            data.arrayPos.x++;
            //canActionPoints = 3;
            BattleManager.Instance.battleMap[arrayPos.y, arrayPos.x] |= lastPosition;
            ArrayPositionToWorld();//移动到数组对应位置
        }
    
    
        // 这个方法用来检查多个方向上的敌人
        public bool CheckEnemyInDirections(byte[] directions)
        {
            detectedPositions = new List<(byte, byte)>(); // 用于存储检测到的终点坐标

            // 遍历给定的方向数组，检测每个方向
            for(int i = 0; i < directions.Length; i++)
            {
                CheckSingleDirectionEnemy(directions[i]);
            }

            return detectedPositions.Count > 0;
        }

        public bool IsEnemy(int x, int y)
        {
            return isPlayer ? (BattleManager.Instance.battleMap[y, x] & 0x80) != 0 : (BattleManager.Instance.battleMap[y, x] & 0x40) != 0;
        }
    
        // 单个方向检测
        public virtual bool CheckSingleDirectionEnemy(byte direction)
        {
            // 起始坐标
            byte startX = (byte)arrayPos.x;
            byte startY = (byte)arrayPos.y;

            byte distance = 1;
            // 目标坐标
            byte targetX = startX;
            byte targetY = startY;

            // 根据方向调整目标坐标
            switch (direction)
            {
                case 0: // 上方向 (Up)
                    targetY = (byte)(startY - distance);
                    break;
                case 1: // 下方向 (Down)
                    targetY = (byte)(startY + distance);
                    break;
                case 2: // 左方向 (Left)
                    targetX = (byte)(startX - distance);
                    break;
                case 3: // 右方向 (Right)
                    targetX = (byte)(startX + distance);
                    break;
                default:
                    Debug.LogError("无效的方向: " + direction);
                    return false;
            }

            // 检查是否越界
            if (targetX < 0 || targetX > 15 || targetY < 0 || targetY > 6)
            {
                return false; // 跳过本次检查，继续下一个距离
            }

            // 检查是否是敌人
            if (IsEnemy(targetX, targetY))
            {
                detectedPositions.Add((targetX, targetY));
                //Debug.Log($"本体{startX},{startY}在方向 {direction} 和距离 {Distance} 上发现敌人: ({targetX}, {targetY})");
                return true; // 找到敌人
            }
        

            //Debug.Log($"在方向 {face} 范围内未发现敌人");
            return false; // 如果没有找到敌人
        }


        // 用于调用并输出检测结果
        public virtual Troop PerformTargetCheck(byte[] directions)
        {
            if (health <= 0) return null;

            // 进行检测并返回检测到的敌人位置
            if (CheckEnemyInDirections(directions))
            {
                // 输出检测到的敌人坐标
                foreach (var position in detectedPositions)
                {
                    return BattleManager.Instance.GetEnemyTroopByXY(isPlayer, position.Item1, position.Item2);
                }
            }
            else
            {
                //Debug.Log("范围内没有敌人");
            }
            return null;
        }

        public virtual void Attack(Troop defenser)
        {
            // 攻击逻辑
            if (defenser != null)
            {
                // 获取目标坐标
                byte targetX = (byte)defenser.arrayPos.x;
                byte targetY = (byte)defenser.arrayPos.y;
                data.face = JudgeFace(targetX, targetY);
                TurnDirection(face);
                if (troopType == TroopType.Captain && defenser.troopType == TroopType.Captain)
                {
                    BattleManager.Instance.ExecuteSolo();
                }
                else
                {
                    
                    defenser.TakeDamage(this);
                }
            }
            else
            {
                Debug.Log("攻击对象为空");
            }
        }

        public virtual float CalculateDamage(Troop attacker)
        {
            General atkGen = GeneralListCache.GetGeneral(attacker.generalID);
            // 获取进攻方攻击力
            int gjl = attacker.attackPower; // 当前攻击方的攻击力

            // 计算当前血量的20分之一作为最小伤害
            int F = attacker.health / 20;

            // 计算最终的伤害值
            float damage = Mathf.Max(gjl,F);

            // 如果血量低于200，伤害减少
            if (attacker.health < 200)
                damage = damage * attacker.health / 200.0f;

            // 确保伤害最小为1
            damage = Mathf.Max(damage,1.0f);

            // 标记是否暴击
            bool isbaoji = false;

            // AI进攻时的计算
            if (attacker.isPlayer)
            {
                // 检查是否触发特定攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,true))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
            }
            else if (!attacker.isPlayer)
            {
                // 检查是否触发特定攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,false))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
            }
        
            // 判断地形是否触发特殊效果
            if (atkGen.HasSkill(2, 4) && BattleManager.Instance.battleTerrain == 9 && !isbaoji)// 特技水将
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 5) && (BattleManager.Instance.battleTerrain == 10 || BattleManager.Instance.battleTerrain == 11) && !isbaoji)// 特技乱战
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            // 返回最终的伤害值
            return damage;
        }
    
        // 执行小战场战斗攻击伤害逻辑
        public virtual void BattleHurtCalculate(Troop attacker)
        {
            General defGen = GeneralListCache.GetGeneral(generalID);
            float damage = CalculateDamage(attacker); // 调用方法获取伤害值
            short fyl = defensePower; // 当前防御方的防御力
        
            // 检查技能3，减少伤害值
            if (defGen.HasSkill(3, 9))// 特技兵圣
            {
                damage /= 2.0f;
            }
        
            // 检查技能2，如果伤害值小于30可能将伤害值置为0
            if (damage < 30)
            {
                if (defGen.HasSkill(2, 7) && Random.Range(0,3) < 1)//特技 金刚
                {
                    damage = 0;
                }
            }

            // 检查血量，如果士兵的血量小于50并且不是将军
            if (health < 50)
            {
                if (defGen.HasSkill(2, 8) && Random.Range(0,3) < 1)//特技 不屈
                {
                    damage = 0;
                }
            }
        
            // 防御系数计算
            float defIndex = fyl / 150.0f;
            defIndex *= TextLibrary.hj[fyl - 1]; // 乘以一个修正系数
            damage = damage * 1.0f / (defIndex + 1.0f);
            short loss = (short)Math.Min(damage, health);
            UpdateHealth((short)-loss);
            UIBattle.Instance.UpdateBattleInfo(isPlayer, loss, troopType);
            Debug.Log($"{gameObject.name}({arrayPos.x},{arrayPos.y})受到来自{attacker.gameObject.name}({attacker.arrayPos.x},{attacker.arrayPos.y})的伤害{loss}");
        }
    
        // 受到伤害动画
        public void TakeDamage(Troop attacker)
        {
            // 受伤逻辑
            BattleHurtCalculate(attacker);
            // 播放受击动画
            troopAnimation = gameObject.GetComponent<Animator>();
            if (troopAnimation != null)
            {
                troopAnimation.SetTrigger("HurtTrigger");
            }
            else
            {
                Debug.LogError("未找到受击动画");
            }    
        }

        // 部队更新生命值及血条
        public void UpdateHealth(short variation)
        {
            if (health + variation >= 300)
            {
                data.health = 300;
            }
            else if (health + variation <= 0)
            {
                data.health = 0;
                Die();
            }
            else
            {
                data.health += variation;
            }
            float healthPercent = health / 300f;
            gameObject.transform.Find("BarBGI").transform.Find("HPBar").GetComponent<Image>().fillAmount = healthPercent;
        }

        public virtual void Die()
        {
            BattleManager.Instance.battleMap[arrayPos.y,arrayPos.x] &= 0x32;
            
            if (isPlayer)
            {
                BattleManager.Instance.hmTroops.Remove(this);
            }
            else if (!isPlayer)
            {
                BattleManager.Instance.aiTroops.Remove(this);
            }
            Debug.Log($"单位{arrayPos.x},{arrayPos.y}死亡");
            Destroy(gameObject);
        }

        // 小战场撤退检测
        public bool IsRetreat()
        {
            if (isPlayer)// 玩家部队撤退
            {
                if(arrayPos.x == 0 && currentState == TroopState.BackWard)
                {
                    if (troopType == TroopType.Captain)
                    {
                        BattleManager.Instance.battleState = BattleState.HMRetreat;
                        BattleManager.Instance.battleMap[arrayPos.y,arrayPos.x] &= 0x32;
                        BattleManager.Instance.HandleBattleRetreat();
                        return true;
                    }
                    BattleManager.Instance.battleMap[arrayPos.y,arrayPos.x] &= 0x32;
                    BattleManager.Instance.hmTroops.Remove(this);
                    Destroy(gameObject);
                    return true;
                }
            }
            else// AI部队撤退
            {
                if(arrayPos.x == 15 && currentState == TroopState.BackWard)
                {
                    if (troopType == TroopType.Captain)
                    {
                        BattleManager.Instance.battleState = BattleState.AIRetreat;
                        BattleManager.Instance.battleMap[arrayPos.y,arrayPos.x] &= 0x32;
                        BattleManager.Instance.HandleBattleRetreat();
                        return true;
                    }
                    else
                    {
                        BattleManager.Instance.battleMap[arrayPos.y,arrayPos.x] &= 0x32;
                        BattleManager.Instance.aiTroops.Remove(this);
                        Destroy(gameObject);
                        return true;
                    }
                }
            }
            return false;
        }
    
    }

    
    // 武将的小战场单位    
    public class Captain : Troop
    {
        // 待做：激励士气的逻辑，提升附近部队的攻击力或防御力
    
        public override float CalculateDamage(Troop attacker)
        {
            General atkGen = GeneralListCache.GetGeneral(attacker.generalID);
            // 获取进攻方攻击力
            short gjl = attacker.attackPower;

            // 计算当前血量的20分之一作为最小伤害
            int F = attacker.health / 20;

            // 计算最终的伤害值
            float damage = Mathf.Max(gjl, F);

            // 如果血量低于200，伤害减少
            if (attacker.health < 200)
                damage = damage * attacker.health / 200.0f;

            // 确保伤害最小为1
            damage = Mathf.Max(damage, 1.0f);

            // 标记是否暴击
            bool isbaoji = false;

            // AI进攻时的计算
            if (attacker.isPlayer)
            {
                // 检查是否触发特定攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,true))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
            }
            else if (!attacker.isPlayer)
            {
                // 检查是否触发特定攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,false))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
            }

            // 判断不同兵种是否触发技能暴击
            if (atkGen.HasSkill(3, 0))// 单骑特技
            {
                if (Random.Range(0,2) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 9))// 猛将特技
            {
                if (Random.Range(0,3) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
        
            // 骑系技能暴击
            if (atkGen.HasSkill(2, 0))// 特技骑神
            {
                if (Random.Range(0,3) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 1))// 特技骑将
            {
                if (Random.Range(0,5) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
        
            // 弓系技能暴击
            if (atkGen.HasSkill(2, 2))// 特技弓神
            {
                if (Random.Range(0,3) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 3))// 特技弓将
            {
                if (Random.Range(0,5) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
        
            // 判断地形是否触发特殊效果
            if (atkGen.HasSkill(2, 4) && BattleManager.Instance.battleTerrain == 9 && !isbaoji)// 特技水将
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 5) && (BattleManager.Instance.battleTerrain == 10 || BattleManager.Instance.battleTerrain == 11) && !isbaoji)// 特技乱战
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            // 返回最终的伤害值
            return damage;
        }
    
        // 将军受伤的逻辑
        public override void BattleHurtCalculate(Troop attacker)
        {
            General defGen = GeneralListCache.GetGeneral(generalID);
            float damage = CalculateDamage(attacker); // 调用sht方法获取伤害值
            short fyl = defensePower; // 当前防御方的防御力
        
            // 检查技能3，减少伤害值
            if (defGen.HasSkill(3, 9))// 特技兵圣
            {
                damage /= 2.0f;
            }
        
            // 检查技能2，如果伤害值小于30可能将伤害值置为0
            if (damage < 30)
            {
                if (defGen.HasSkill(2, 7) && Random.Range(0,3) < 1)//特技 金刚
                {
                    damage = 0;
                }
            }
        
            // 是否进入单挑界面
            if (attacker.troopType == TroopType.Captain)
            {
                //this.singleFigth = true; // 设置单挑模式
                return;
            }
        
            // 防御系数计算
            float defIndex = fyl / 150.0f;
            defIndex *= TextLibrary.hj[fyl - 1]; // 乘以一个修正系数

            damage = damage * 1.0f / (defIndex + 1.0f);
            short loss = (short)Math.Min(damage/3.0f, health);
            UpdateHealth((short)-loss);

            UIBattle.Instance.UpdateBattleInfo(isPlayer, (short)(loss / 3), troopType );
            Debug.Log($"将军{arrayPos.x},{arrayPos.y}受到来自{attacker.arrayPos.x},{attacker.arrayPos.y}的伤害{loss}");
        }
    
        // 将领使用爆炎
        public void Explosion()
        {
            byte selfX = (byte)arrayPos.x;
            byte selfY = (byte)arrayPos.y;
            if (isPlayer)
            {
                troopAnimation = gameObject.GetComponent<Animator>();
                if (troopAnimation != null)
                {
                    troopAnimation.SetTrigger("RightBoom");
                }
                else
                {
                    Debug.LogError("未找到爆炎动画");
                } 
                // 在将领前方三格附近的坐标范围内搜索
                for (int x = selfX + 1; x < selfX + 4 && x <= 15; x++)
                {
                    for (int y = selfY - 1; y < selfY + 2; y++)
                    {
                        if (y >= 0)
                        {
                            if (y > 6)
                                break;
                            Troop enemy = BattleManager.Instance.GetEnemyTroopByXY(isPlayer, x, y);
                            if (enemy != null)
                            {
                                short boomLoss;
                                // 处理主将单位
                                if (enemy.troopType == TroopType.Captain)
                                {
                                    boomLoss = (short)Random.Range(90, 151);
                                    enemy.UpdateHealth((short)-boomLoss);
                                    UIBattle.Instance.UpdateBattleInfo(!isPlayer, (short)(boomLoss / 3), enemy.troopType);
                                }
                                else// 对其他士兵造成伤害
                                {
                                    boomLoss = (short)Random.Range(60, 111);
                                    enemy.UpdateHealth((short)-boomLoss);
                                    UIBattle.Instance.UpdateBattleInfo(!isPlayer, boomLoss, enemy.troopType);
                                }
                            }

                        }
                    }
                }
            }
            else // AI使用爆炎
            {
                troopAnimation = gameObject.GetComponent<Animator>();
                if (troopAnimation != null)
                {
                    troopAnimation.SetTrigger("LeftBoom");
                }
                else
                {
                    Debug.LogError("未找到爆炎动画");
                } 
                // 在敌军将领附近的坐标范围内搜索我军部队
                for (int x = selfX - 1; x > selfX - 4 && x >= 0; x--)
                {
                    for (int y = selfY - 1; y < selfY + 2; y++)
                    {
                        if (y >= 0)
                        {
                            if (y > 6)
                                break;
                            Troop enemy = BattleManager.Instance.GetEnemyTroopByXY(!isPlayer, x, y);
                            if (enemy != null)
                            {
                                short boomLoss;
                                // 处理主将单位
                                if (enemy.troopType == TroopType.Captain)
                                {
                                    boomLoss = (short)Random.Range(90, 151);
                                    enemy.UpdateHealth((short)-boomLoss);
                                    UIBattle.Instance.UpdateBattleInfo(isPlayer, (short)(boomLoss / 3), enemy.troopType);
                                }
                                else
                                {
                                    // 对其他士兵造成伤害
                                    boomLoss = (short)Random.Range(60, 111);
                                    enemy.UpdateHealth((short)-boomLoss);
                                    UIBattle.Instance.UpdateBattleInfo(isPlayer, boomLoss, enemy.troopType);
                                }
                            }

                        }
                    }
                }
            }
        }
        
        //TODO 是否被擒获方法，判断武将是否被擒
        public bool IsCaptured()
        {
            return false;
            
            byte cityNum = 0;// 获取君主是否有可以撤退的城市
            if (isPlayer)
            {
                List<byte> retreatCityList = WarManager.GetRetreatCityList(WarManager.Instance.hmKingId);
                // 如果返回结果为 null 或空，设置默认值
                if (retreatCityList != null && retreatCityList.Count != 0)
                {
                    cityNum = (byte)retreatCityList.Count; // 初始化为空列表
                }
            }
            else
            {
                List<byte> retreatCityList = WarManager.AIGetRetreatCityList(WarManager.Instance.aiKingId);
                // 如果返回结果为 null 或空，设置默认值
                if (retreatCityList != null && retreatCityList.Count != 0)
                {
                    cityNum = (byte)retreatCityList.Count; // 初始化为空列表
                }
            }
            
            
            
            byte selfX = (byte)arrayPos.x;
            byte selfY = (byte)arrayPos.y;
            General general = GeneralListCache.GetGeneral(generalID); // 获取将领
            
            // 如果将领的体力或者忠诚度超过一定值，AI将不会被俘
            if (general.GetCurPhysical() > (int)(25.0 - general.force * 1.7 / 10.0) || general.GetLoyalty() > 99)
                return false;

            // 检查周围的敌方单位，并计算撤退的可能性
            byte probability = (byte)GetAllAttackableDirection(selfX, selfY).Count;

            // 根据包围情况设置撤退概率
            if (probability == 4)
            {
                probability = 100;
            }
            else if (probability == 3)
            {
                probability = 80;
            }
            else if (probability == 2)
            {
                probability = 60;
            }
            else if (probability == 1)
            {
                probability = 20;
            }
            else
            {
                return false;
            }

            // 如果AI的力量或忠诚度较高，降低被俘获的概率
            if ((general.force > 95 || general.GetLoyalty() > 95) && cityNum > 0)
            {
                probability = (byte)(probability - 50);
            }
            else if ((general.force > 85 || general.GetLoyalty() > 85) && cityNum > 0)
            {
                probability = (byte)(probability - 30);
            }
            else if ((general.force > 75 || general.GetLoyalty() > 75) && cityNum > 0)
            {
                probability = (byte)(probability - 20);
            }

            // 根据忠诚度进一步调整被俘获概率
            if (probability > 0)
            {
                if (general.GetLoyalty() < 15)
                {
                    probability = (byte)(probability + 40);
                }
                else if (general.GetLoyalty() < 35)
                {
                    probability = (byte)(probability + 20);
                }
                else if (general.GetLoyalty() < 50)
                {
                    probability = (byte)(probability + 10);
                }
            }

            // 根据计算的被俘获概率决定是否被俘获
            return (Random.Range(0,100) < probability);
        }
        
        // 死亡方法
        public override void Die()
        {
            WarManager.Instance.loserId = generalID;
            BattleManager.Instance.battleState = isPlayer ? BattleState.HMDie : BattleState.AIDie;
            BattleManager.Instance.battleMap[arrayPos.y,arrayPos.x] &= 0x32;
            if (isPlayer)
            {
                BattleManager.Instance.hmTroops.Remove(this);
            }
            else if (!isPlayer)
            {
                BattleManager.Instance.aiTroops.Remove(this);
            }
            Debug.Log($"将军{arrayPos.x},{arrayPos.y}死亡");
            BattleManager.Instance.HandleBattleEnd(GeneralListCache.GetGeneral(generalID).generalName);
            Destroy(gameObject);
        }
    }
    
    public class Cavalry : Troop
    {
        public override float CalculateDamage(Troop attacker)
        {
            General atkGen = GeneralListCache.GetGeneral(attacker.generalID);
            // 获取进攻方攻击力
            int gjl = attacker.attackPower; 

            // 计算当前血量的20分之一作为最小伤害
            int F = attacker.health / 20;

            // 计算最终的伤害值
            float damage = Mathf.Max(gjl, F);
        
            // 如果血量低于200，伤害减少
            if (attacker.health < 200)
                damage = damage * attacker.health / 200.0f;

            // 确保伤害最小为1
            damage = Mathf.Max(damage, 1.0f);

            // 标记是否暴击
            bool isbaoji = false;

            // AI进攻时的计算
            if (attacker.isPlayer)
            {
                // 检查是否触发呐喊攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,true))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
            }
            else if(!attacker.isPlayer)
            {
                // 检查是否触发呐喊攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,false))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
            }

            // 骑兵兵种暴击的处理逻辑类似
            if (atkGen.HasSkill(2, 0))// 特技骑神
            {
                if (Random.Range(0,3) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 1))// 特技骑将
            {
                if (Random.Range(0,5) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
        
            // 判断地形是否触发特殊效果
            if (atkGen.HasSkill(2, 4) && BattleManager.Instance.battleTerrain == 9 && !isbaoji)// 特技水将
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 5) && (BattleManager.Instance.battleTerrain == 10 || BattleManager.Instance.battleTerrain == 11) && !isbaoji)// 特技乱战
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            // 返回最终的伤害值
            return damage;
        }
    }

// 弓箭手类
    public class Archer : Troop
    {
        public byte range;
        public GameObject arrowPrefab;
        

        public override Troop PerformTargetCheck(byte[] directions) 
        {
            if (health <= 0) return null;

            range = 5;

            // 检查是否有技能和计策加成
            if (GeneralListCache.GetGeneral(generalID).HasSkill(2, 6)) // 连弩特技
                range = (byte)(range + 1);

            if (isPlayer)
            {
                if (UIBattle.Instance.uiTactic.CheckTacticalState(3,true)) // 连弩计策中
                    range = (byte)(range + 2);
            }
            else
            {
                if (UIBattle.Instance.uiTactic.CheckTacticalState(3,false)) // 连弩计策中
                    range = (byte)(range + 2);
            }
        
            // 进行检测并返回检测到的敌人位置
            if (CheckEnemyInDirections(directions))
            {
                // 输出检测到的敌人坐标
                foreach (var position in detectedPositions)
                {
                    return BattleManager.Instance.GetEnemyTroopByXY(isPlayer, position.Item1, position.Item2);
                }
            }
            else
            {
                //Debug.Log("范围内没有敌人");
            }
            return null;
        }

        public override bool CheckSingleDirectionEnemy(byte direction)
        {
            // 起始坐标
            byte startX = (byte)arrayPos.x;
            byte startY = (byte)arrayPos.y;

            // 检测范围从1到最大攻击范围
            for (byte distance = 1; distance <= range; distance++)
            {
                // 目标坐标
                byte targetX = startX;
                byte targetY = startY;

                // 根据方向调整目标坐标
                switch (direction)
                {
                    case 0: // 上方向 (Up)
                        targetY = (byte)(startY - distance);
                        break;
                    case 1: // 下方向 (Down)
                        targetY = (byte)(startY + distance);
                        break;
                    case 2: // 左方向 (Left)
                        targetX = (byte)(startX - distance);
                        break;
                    case 3: // 右方向 (Right)
                        targetX = (byte)(startX + distance);
                        break;
                    default:
                        Debug.LogError("无效的方向: " + direction);
                        return false;
                }

                // 检查是否越界
                if (targetX < 0 || targetX > 15 || targetY < 0 || targetY > 6)
                {
                    continue; // 跳过本次检查，继续下一个距离
                }

                // 检查是否是敌人
                if (IsEnemy(targetX, targetY))
                {
                    detectedPositions.Add((targetX, targetY));
                    //Debug.Log($"本体{startX},{startY}在方向 {direction} 和距离 {Distance} 上发现敌人: ({targetX}, {targetY})");
                    return true; // 找到敌人
                }
            }
            return false; // 如果没有找到敌人
        }

    
    
    
        public override void Attack(Troop defenser)
        {
            // 攻击逻辑
            if (defenser != null)
            {
                // 获取目标坐标
                byte targetX = (byte)defenser.arrayPos.x;
                byte targetY = (byte)defenser.arrayPos.y;
                data.face = JudgeFace(targetX, targetY);
                TurnDirection(face);
                float rotationAngle = 0f;
                switch (face)
                {
                    case 0:
                        rotationAngle = 90f;
                        break;

                    case 1:
                        rotationAngle = -90f;  
                        break;

                    case 2:
                        rotationAngle = 180f;
                        break;

                    case 3:
                        rotationAngle = 0f;
                        break;
                }
                bool fireArrow = false;
                if (isPlayer && UIBattle.Instance.uiTactic.CheckTacticalState(5,true))
                {
                    fireArrow = true;
                }
                else if (!isPlayer && UIBattle.Instance.uiTactic.CheckTacticalState(5,false))
                {
                    fireArrow = true;
                }
                arrowPrefab = Resources.Load<GameObject>("Prefab/ArrowPrefab");
                Transform troopRoom = gameObject.transform.parent;
                Quaternion rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
                GameObject arrowObject = Instantiate(arrowPrefab, troopRoom);
                arrowObject.gameObject.transform.position = gameObject.transform.position;
                arrowObject.gameObject.transform.rotation = rotation;
                arrowObject.layer = isPlayer ? LayerMask.NameToLayer("PlayerArrow"): LayerMask.NameToLayer("AIArrow");
                Arrow arrow = arrowObject.GetComponent<Arrow>();
                arrow.Launch(face, fireArrow, this);
            }
            else
            {
                Debug.Log("攻击对象为空");
            }
        }
    

        public override float CalculateDamage(Troop attacker)
        {
            General atkGen = GeneralListCache.GetGeneral(attacker.generalID);
            // 获取进攻方攻击力和防守方防御力
            int gjl = attacker.attackPower;

            // 计算当前血量的20分之一作为最小伤害
            int F = attacker.health / 20;

            // 计算最终的伤害值
            float damage = Mathf.Max(gjl, F);

            // 如果血量低于200，伤害减少
            if (attacker.health < 200)
                damage = damage * attacker.health / 200.0f;

            // 确保伤害最小为1
            damage = Mathf.Max(damage, 1.0f);

            // 标记是否暴击
            bool isbaoji = false;

            // AI进攻时的计算
            if (attacker.isPlayer)
            {
                // 检查是否触发特定攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,true))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
                else if (UIBattle.Instance.uiTactic.CheckTacticalState(5,true))
                {
                    damage = damage * 5.0f / 3.0f; // 火箭增伤效果
                }
            }
            else if (!attacker.isPlayer)
            {
                // 检查是否触发特定攻击效果
                if (UIBattle.Instance.uiTactic.CheckTacticalState(4,false))
                {
                    damage = damage * 4.0f / 3.0f; // 呐喊增伤效果
                }
                else if (UIBattle.Instance.uiTactic.CheckTacticalState(5,false))
                {
                    damage = damage * 5.0f / 3.0f; // 火箭增伤效果
                }
            }

            if (atkGen.HasSkill(2, 2))// 特技弓神
            {
                if (Random.Range(0,3) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 3))// 特技弓将
            {
                if (Random.Range(0,5) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
        
            // 判断地形是否触发特殊效果
            if (atkGen.HasSkill(2, 4) && BattleManager.Instance.battleTerrain == 9 && !isbaoji)// 特技水将
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            else if (atkGen.HasSkill(2, 5) && (BattleManager.Instance.battleTerrain == 10 || BattleManager.Instance.battleTerrain == 11) && !isbaoji)// 特技乱战
            {
                if (Random.Range(0,6) < 1)
                {
                    damage += damage / 2.0f;
                    isbaoji = true;
                }
            }
            // 返回最终的伤害值
            return damage;
        }
    }

// 步兵类
    public class Infantry : Troop
    {
        
    }
}