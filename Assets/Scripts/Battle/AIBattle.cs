using System;
using BaseClass;
using DataClass;
using UnityEngine;

namespace Battle
{
    public class AIBattle
    {
        private static AIBattle _instance;
        public static AIBattle Instance => _instance ??= new AIBattle();
        private AIBattle(){}
    
        private bool _aiForward;
        
        private BattleManager BM => BattleManager.Instance;
        
        // AI选择战场命令方法
        public void AIBattleMethod()
        {
            if (BM.battleTerrain == 8)
            {
                if (BM.isHmDef)
                {
                    AISiege();
                }
                else
                {
                    AIDefSiege();
                }
            }
            else
            {
                AIField();
            }
        }
    
    
        // Battle时AI守城策略
        void AIDefSiege()
        {
            byte x = (byte)BM.aiTroops[0].arrayPos.x;
            byte y = (byte)BM.aiTroops[0].arrayPos.y;;
            byte s0Num = 0;
            byte s50Num = 0;

            // 统计 AI 小兵的血量
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0)
                    s0Num++;
                if (BM.aiTroops[i].health  >= 50)
                    s50Num++;
            }

            byte hmX = (byte)BM.hmTroops[0].arrayPos.x;
            byte hmY = (byte)BM.hmTroops[0].arrayPos.y;

            if (BM.aiTacticPoint >= 12)
            {
                byte dx = (byte)(x - hmX);
                byte dy = (byte)Mathf.Abs(y - hmY);

                if (dx is >= 1 and <= 3 && dy <= 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(6, false);// AI爆炎
                }
                else
                {
                    byte canBoomNum = 0;

                    // 检查是否有可以爆炎攻击的范围
                    for (byte cellY = 0; cellY < 7; cellY++)
                    {
                        for (byte cellX = 0; cellX < 16; cellX++)
                        {
                            if (BM.battleMap[cellY, cellX] == 64)// 玩家部队
                            {
                                byte dsx = (byte)(x - cellX);
                                byte dsy = (byte)Mathf.Abs(y - cellY);
                                if (dsx >= 1 && dsx <= 3 && dsy <= 1)
                                    canBoomNum++;// 爆炎范围内的玩家部队
                            }
                        }
                    }

                    bool doBoom = false;// 是否选择爆炎
                    if (s50Num <= 1 && canBoomNum >= 1)
                    {
                        doBoom = true;
                    }
                    else if (s50Num <= 2 && canBoomNum >= 2)
                    {
                        doBoom = true;
                    }
                    else if (canBoomNum >= 3)
                    {
                        doBoom = true;
                    }

                    if (doBoom)
                    {
                        UIBattle.Instance.uiTactic.ApplyTactic(6, false);
                    }
                }
            }

            short hmSoldierNum = GetBattleSoldierNum(true);
            short aiSoldierNum = GetBattleSoldierNum(false);
            AICastleDefTactic();

            // 初始化小兵命令
            for (byte i = 0; i < BM.aiTroops.Count; i++)
                BM.aiTroops[i].data.troopState = TroopState.Idle;

            byte aiArcherNum = 0;// AI弓箭手数量
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Archer)
                    aiArcherNum++;
            }

            byte hmArcherNum = 0;// 玩家弓箭手数量
            for (byte i = 1; i < BM.hmTroops.Count; i++)
            {
                if (BM.hmTroops[i].health > 0 && BM.hmTroops[i].troopType == TroopType.Archer)
                    hmArcherNum++;
            }

            // 根据弓箭手的数量决定步兵的攻击策略
            if (hmArcherNum >= 1 && aiArcherNum <= 1)
                for (byte i = 1; i < BM.aiTroops.Count; i++)
                {
                    if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Infantry && !CanAtkAdjacentEnemy((byte)BM.aiTroops[i].arrayPos.x, (byte)BM.aiTroops[i].arrayPos.y))
                        BM.aiTroops[i].data.troopState = TroopState.BackWard;
                }

            if (BM.hmGeneral.HasSkill(2,6))
                for (byte i = 1; i < BM.aiTroops.Count; i++)
                {
                    if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Archer)
                        BM.aiTroops[i].data.troopState = TroopState.Forward;
                }

            if (hmX >= 9)
                for (byte i = 1; i < BM.aiTroops.Count; i++)
                {
                    if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Archer)
                        BM.aiTroops[i].data.troopState = TroopState.Forward;
                }

            if (aiArcherNum == 0 && hmArcherNum > 0)
                BM.aiTroops[0].data.troopState = TroopState.BackWard;

            if ((aiSoldierNum < 100 && (hmSoldierNum > 450 || hmArcherNum >= 1)) || AIGenBattleRetreat() || (CanAdjacentSolo() && AIGenRefuseSolo()))
                BM.aiTroops[0].data.troopState = TroopState.BackWard;
        }
    
        // Battle时AI 进行围攻攻城
        void AISiege()
        {
            byte x = (byte)BM.aiTroops[0].arrayPos.x;
            byte y = (byte)BM.aiTroops[0].arrayPos.y;;
            byte s0Num = 0;
            byte s50Num = 0;

            // 统计 AI 小兵状态
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0)
                {
                    if (BM.aiTroops[i].health > 0)
                        s0Num++;
                    if (BM.aiTroops[i].health >= 50)
                        s50Num++;
                }
            }

            if (BM.aiTacticPoint >= 12)
            {
                byte hmX = (byte)BM.hmTroops[0].arrayPos.x;
                byte hmY = (byte)BM.hmTroops[0].arrayPos.y;
                byte dx = (byte)(x - hmX);
                byte dy = (byte)Mathf.Abs(y - hmY);

                if (dx >= 1 && dx <= 3 && dy <= 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(6, false);
                }
                else
                {
                    byte canBoomNum = 0;
                    for (byte cellY = 0; cellY < 7; cellY++)
                    {
                        for (byte cellX = 0; cellX < 16; cellX++)
                        {
                            if (BM.battleMap[cellY, cellX]  == 64)
                            {
                                byte dsx = (byte)(x - cellX);
                                byte dsy = (byte)Mathf.Abs(y - cellY);
                                if (dsx >= 1 && dsx <= 3 && dsy <= 1)
                                    canBoomNum++;
                            }
                        }
                    }

                    bool doBoom = false;
                    if (s50Num <= 1 && canBoomNum >= 1)
                    {
                        doBoom = true;
                    }
                    else if (s50Num <= 2 && canBoomNum >= 2)
                    {
                        doBoom = true;
                    }
                    else if (canBoomNum >= 3)
                    {
                        doBoom = true;
                    }

                    if (doBoom)
                    {
                        UIBattle.Instance.uiTactic.ApplyTactic(6, false);
                    }
                }
            }

            short hmSoldierNum = GetBattleSoldierNum(true);
            short aiSoldierNum = GetBattleSoldierNum(false);
            AICastleDefTactic();

            for (byte i = 1; i < BM.aiTroops.Count; i++)
                BM.aiTroops[i].data.troopState = TroopState.Forward;
            BM.aiTroops[0].data.troopState = TroopState.Idle;

            byte aiArcherNum = 0;
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Archer)
                    aiArcherNum++;
            }

            byte hmArcherNum = 0;
            for (int i = 1; i < BM.hmTroops.Count; i++)
            {
                if (BM.hmTroops[i].health > 0 && BM.hmTroops[i].troopType == TroopType.Archer)
                    hmArcherNum++;
            }

            if (hmArcherNum >= 2 && aiArcherNum <= 1)
                for (byte i = 1; i < BM.aiTroops.Count; i++)
                {
                    if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Infantry && BM.aiTroops[i].arrayPos.x >= 8 && !CanAtkAdjacentEnemy((byte)BM.aiTroops[i].arrayPos.x, (byte)BM.aiTroops[i].arrayPos.y))
                        BM.aiTroops[i].data.troopState = TroopState.BackWard;
                }

            if (aiSoldierNum < 100 || AIGenBattleRetreat() || (CanAdjacentSolo() && AIGenRefuseSolo()))
                BM.aiTroops[0].data.troopState = TroopState.BackWard;

            if (hmSoldierNum <= 100 && !AIGenRefuseSolo())
                BM.aiTroops[0].data.troopState = TroopState.Forward;
        }
     
        // 获取在战斗中的士兵数量
        short GetBattleSoldierNum(bool isPlayer)
        {
            short num = 0;

            if (isPlayer)
            {
                for (byte i = 1; i < BM.hmTroops.Count; i++)
                {
                    if (BM.hmTroops[i].health > 0)
                        num = (short)(num + BM.hmTroops[i].health);
                }
            }
            else
            {
                for (byte i = 1; i < BM.aiTroops.Count; i++)
                {
                    if (BM.aiTroops[i].health > 0)
                        num = (short)(num + BM.aiTroops[i].health);
                }
            }

            return num;
        }
     
        // AI 守城防御使用战术
        void AICastleDefTactic()
        {
            byte troopNum = 0; // 小兵数量
            byte artherNum = 0; // 弓箭手数量

            // 统计存活的小兵数量和弓箭手数量
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0)
                {
                    troopNum++;
                    if (BM.aiTroops[i].troopType == TroopType.Archer)
                        artherNum++;
                }
            }

            // 根据不同条件选择战术
            if (BM.aiTacticPoint >= 10)
            {
                byte fireNum = FireArrowAtkValue();
                if (fireNum >= artherNum / 3 + 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(5, false);// 战术火矢
                    return;
                }
                if (fireNum >= 2 && artherNum <= 2)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(5, false);// 战术火矢
                    return;
                }
                if (fireNum == 1 && artherNum == 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(5, false);// 战术火矢
                    return;
                }
            }
            else if (BM.aiTacticPoint >= 8)
            {
                byte shoutNum = DefNaHanValue();
                if (shoutNum >= troopNum / 3 + 1 && troopNum >= 3)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(4, false);// 战术呐喊
                    return;
                }
                CrossbowAtkValue();
                if (BM.aiTacticPoint >= 8 && shoutNum >= troopNum / 2 + 1 && troopNum >= 2)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(4, false);// 战术呐喊
                    return;
                }
                if (BM.aiTacticPoint >= 8 && shoutNum >= troopNum && troopNum >= 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(4, false);// 战术呐喊
                    return;
                }
            }
            else if (BM.aiTacticPoint >= 7)
            {
                CrossbowAtkValue();
            }
        }
     
        // 计算火箭攻击的数量
        byte FireArrowAtkValue()
        {
            byte canAtkNum = 0;

            // 遍历所有小兵
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0 && BM.aiTroops[i].troopType == TroopType.Archer)
                {
                    byte x = (byte) BM.aiTroops[i].arrayPos.x;
                    byte y = (byte) BM.aiTroops[i].arrayPos.y;

                    // 检查上下左右的攻击范围
                    for (int d = 1; d < 5; d++)
                    {
                        if (x > d - 1)
                        {
                            byte hx = (byte)(x - d);
                            byte hy = y;
                            if (BM.battleMap[hy, hx] == 64)
                            {
                                if (BM.GetTroopByXY(hx, hy) !=null && BM.GetTroopByXY(hx, hy).troopType == TroopType.Archer)
                                    canAtkNum++;// 对弓箭手优先攻击
                                canAtkNum++;
                                break;
                            }
                        }
                        if (y > d - 1)
                        {
                            byte hx = x;
                            byte hy = (byte)(y - d);
                            if (BM.battleMap[hy, hx] == 64)
                            {
                                if (BM.GetTroopByXY(hx, hy) !=null && BM.GetTroopByXY(hx, hy).troopType == TroopType.Archer)
                                    canAtkNum++;// 对弓箭手优先攻击
                                canAtkNum++;
                                break;
                            }
                        }
                        if (y < 7 - d)
                        {
                            byte hx = x;
                            byte hy = (byte)(y + d);
                            if (BM.battleMap[hy, hx] == 64)
                            {
                                if (BM.GetTroopByXY(hx, hy) !=null && BM.GetTroopByXY(hx, hy).troopType == TroopType.Archer)
                                    canAtkNum++;// 对弓箭手优先攻击
                                canAtkNum++;
                                break;
                            }
                        }
                    }
                }
            }
            return canAtkNum;
        }


        // 计算可以进行呐喊攻击的数量
        byte DefNaHanValue()
        {
            byte canAtkNum = 0;

            // 遍历所有小兵
            for (int i = 0; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i])
                {
                    byte x = (byte) BM.aiTroops[i].arrayPos.x;
                    byte y = (byte) BM.aiTroops[i].arrayPos.y;

                    // 判断近战小兵类型，并检查其周围是否有可攻击的目标
                    if (BM.GetTroopByXY(x, y) !=null && BM.GetTroopByXY(x, y).troopType != TroopType.Archer)
                    {
                        if (x > 0 && BM.battleMap[y, x - 1] == 64)
                        {
                            if (BM.GetTroopByXY((byte)(x - 1), y).troopType == TroopType.Archer)
                                canAtkNum++;
                            canAtkNum++;
                        }
                        else if (y > 0 && BM.battleMap[y - 1, x] == 64)
                        {
                            if (BM.GetTroopByXY(x, (byte)(y - 1)).troopType == TroopType.Archer)
                                canAtkNum++;
                            canAtkNum++;
                        }
                        else if (y < 6 && BM.battleMap[y + 1, x] == 64)
                        {
                            if (BM.GetTroopByXY(x, (byte)(y + 1)).troopType == TroopType.Archer)
                                canAtkNum++;
                            canAtkNum++;
                        }
                    }
                    else
                    {
                        // 检查弓箭手周围的长距离攻击范围
                        for (int d = 1; d < 5; d++)
                        {
                            if (x > d - 1)
                            {
                                byte hx = (byte)(x - d);
                                byte hy = y;
                                if (BM.battleMap[hy, hx] == 64)
                                {
                                    if (BM.GetTroopByXY(hx, hy) !=null && BM.GetTroopByXY(hx, hy).troopType == TroopType.Archer)
                                        canAtkNum++;
                                    canAtkNum++;
                                    break;
                                }
                            }
                            if (y > d - 1)
                            {
                                byte hx = x;
                                byte hy = (byte)(y - d);
                                if (BM.battleMap[hy, hx] == 64)
                                {
                                    if (BM.GetTroopByXY(hx, hy) !=null && BM.GetTroopByXY(hx, hy).troopType == TroopType.Archer)
                                        canAtkNum++;
                                    canAtkNum++;
                                    break;
                                }
                            }
                            if (y < 7 - d)
                            {
                                byte hx = x;
                                byte hy = (byte)(y + d);
                                if (BM.battleMap[hy, hx] == 64)
                                {
                                    if (BM.GetTroopByXY(hx, hy) !=null && BM.GetTroopByXY(hx, hy).troopType == TroopType.Archer)
                                        canAtkNum++;
                                    canAtkNum++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return canAtkNum;
        }
     
        //TODO
        /// <summary>
        /// AI连弩攻击值
        /// </summary>
        void CrossbowAtkValue()
        {
            byte archerNum = 0;  // 弓箭小兵数量
            byte longAtkNum = 0;  // 电脑弓箭手如果使用射程可以攻击到玩家兵的次数
            byte longAtkArcherNum = 0;  // 电脑弓箭手如果使用射程可以攻击到玩家弓箭手的次数

            // 遍历所有小士兵
            for (byte index = 0; index < BM.aiTroops.Count; index++)
            {
                // 判断士兵是否存活且种类为弓兵
                if (BM.aiTroops[index].health > 0 && BM.aiTroops[index].troopType == TroopType.Archer)
                {
                    archerNum++;  // 增加存在的士兵数量
                    
                    // 检查其增加距离攻击能力
                    for (var dx = 1; dx < 7; dx++)
                    {
                        var x = BM.aiTroops[index].arrayPos.x - dx;  // 获取弓箭手连弩射程内士兵的X坐标
                        var y = BM.aiTroops[index].arrayPos.y;  // 获取士兵的Y坐标

                        // 检查是否超出地图范围
                        if (x < 0)
                            break;

                        // 检查是否可以增加距离攻击
                        if (BM.battleMap[y, x] == 64 && dx >= 5)
                        {
                            longAtkNum++;
                        }

                        // 检查玩家弓箭手是否在增加的攻击范围内
                        for (byte i = 1; i < BM.hmTroops.Count; i++)
                        {
                            if (BM.hmTroops[i].health > 0 && BM.hmTroops[i] == BM.GetTroopByXY(x,y) && BM.hmTroops[i].troopType == TroopType.Archer)
                                longAtkArcherNum++;
                        }
                        
                    }
                }
            }

            // 根据AI战术点的值调整攻击条件
            if (BM.aiTacticPoint < 8)
            {
                if (archerNum > 0 && longAtkNum >= archerNum / 2 + 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(3, false);
                    return;
                }
            }
            else
            {
                if (archerNum > 1 && longAtkNum == archerNum)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(3, false);
                    return;
                }
                if (archerNum > 1 && longAtkArcherNum >= archerNum / 2 + 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(3, false);
                    return;
                }
                if (archerNum == 1 && longAtkNum >= archerNum)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(3, false);
                    return;
                }
            }
        }
     
        /// <summary>
        /// 呐喊攻击值计算
        /// </summary>
        /// <returns></returns>
        byte ShoutAtkValue()
        {
            byte canAtkNum = 0;  // 可以攻击的士兵数量

            // 遍历所有敌方小士兵
            for (byte i = 0; i < BM.aiTroops.Count; i++)
            {
                // 判断士兵是否存活
                if (BM.aiTroops[i].health > 0)
                {
                    byte x = (byte)BM.aiTroops[i].arrayPos.x;  // 士兵的X坐标
                    byte y = (byte)BM.aiTroops[i].arrayPos.y;  // 士兵的Y坐标

                    // 判断士兵类型并检查是否可以攻击
                    if (BM.aiTroops[i].troopType == TroopType.Cavalry || BM.aiTroops[i].troopType == TroopType.Captain)
                    {
                        if (x > 0 && BM.battleMap[y, x - 1] == 64)// 检查左边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (y > 0 && BM.battleMap[y - 1, x] == 64)// 检查上边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (y < 6 && BM.battleMap[y + 1, x] == 64)// 检查下边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (x > 1 && BM.battleMap[y, x - 2] == 64)// 检查左边隔一格士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (x > 0 && y > 0 && BM.battleMap[y - 1, x - 1] == 64)// 检查左上边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (x > 0 && y < 6 && BM.battleMap[y + 1, x - 1] == 64)// 检查左下边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                    }
                    else if (BM.aiTroops[i].troopType == TroopType.Infantry)
                    {
                        if (x > 0 && BM.battleMap[y, x - 1] == 64)// 检查左边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (y > 0 && BM.battleMap[y - 1, x] == 64)// 检查上边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                        else if (y < 6 && BM.battleMap[y + 1, x] == 64)// 检查下边士兵是否可以攻击
                        {
                            canAtkNum++;
                        }
                    }
                    else
                    {
                        // 弓箭手长距离攻击
                        for (int d = 1; d < 5; d++)
                        {
                            if (x > d - 1)
                            {
                                var hx = x - d;
                                var hy = y;
                                
                                if (BM.battleMap[hy, hx] == 64)
                                {
                                    canAtkNum++;
                                }
                            }
                            if (y > d - 1)
                            {
                                var hx = x;
                                var hy = y - d;
                                if (BM.battleMap[hy, hx] == 64)
                                {
                                    canAtkNum++;
                                }
                            }
                            if (y < 7 - d)
                            {
                                var hx = x;
                                var hy = y + d;
                                if (BM.battleMap[hy, hx] == 64)
                                {
                                    canAtkNum++;
                                }
                            }
                        }
                    }
                }
            }
            return canAtkNum;
        }
     
        // 检查给定坐标 (x, y) 是否可以被近战攻击
        bool CanAtkAdjacentEnemy(int x, int y)
        {
            if (x < 15 && x > 0)
            {
                if (y == 0)
                {
                    // 检查上、下、左右相邻坐标是否可攻击
                    if (BM.battleMap[y, x - 1] == 64 ||
                        BM.battleMap[y, x + 1] == 64 ||
                        BM.battleMap[y + 1, x] == 64)
                        return true;
                }
                else if (y == 6)
                {
                    // 检查上、下、左右相邻坐标是否可攻击
                    if (BM.battleMap[y, x - 1] == 64 ||
                        BM.battleMap[y, x + 1] == 64 ||
                        BM.battleMap[y - 1, x] == 64)
                        return true;
                }
                else
                {
                    // 检查上、下、左右相邻坐标是否可攻击
                    if (BM.battleMap[y, x - 1] == 64 ||
                        BM.battleMap[y, x + 1] == 64 ||
                        BM.battleMap[y - 1, x] == 64 ||
                        BM.battleMap[y + 1, x] == 64)
                        return true;
                }
            }
            return false;
        }
     

        //TODO Battle时AI将军判断是否撤退
        bool AIGenBattleRetreat()
        {
            // 获取参与战斗的士兵数量，分别计算己方和敌方的数量
            short pySoldierNum = GetBattleSoldierNum(true);  // 己方士兵数量
            short aiSoldierNum = GetBattleSoldierNum(false); // 敌方士兵数量

            // 获取己方士兵的初始坐标
            byte aix = (byte)BM.aiTroops[0].arrayPos.x; // AI士兵的X坐标
            byte aiy = (byte)BM.aiTroops[0].arrayPos.y; // AI士兵的Y坐标

            short canatkps = 0;  // 可攻击点数
            byte testx = 0;     // 测试用X坐标
            byte testy = 0;     // 测试用Y坐标

            try
            {
                // 遍历战场的每一个单元格，查找可以进行攻击的敌人
                for (byte cellY = 0; cellY < 7; cellY++)
                {
                    for (byte cellX = 0; cellX < 16; cellX++)
                    {
                        testx = cellX;
                        testy = cellY;

                        // 检查当前单元格是否有敌方士兵
                        if (BM.battleMap[cellY, cellX]  == 64)
                        {
                            byte dx = (byte)Math.Abs(aix - cellX); // X方向上的距离
                            byte dy = (byte)Math.Abs(aiy - cellY); // Y方向上的距离
                            bool flag1 = false; // 判断是否可以攻击
                            bool flag2 = false; // 未使用的标志位

                            // 检查士兵种类和攻击条件（不同的攻击逻辑适用于不同的士兵种类）
                            if (BM.GetTroopByXY(cellX, cellY)!=null && BM.GetTroopByXY(cellX, cellY).troopType == TroopType.Archer && (dx <= 6 && dy == 0 || dx == 0 || dx + dy <= 2))
                            {
                                // 处理玩家连弩战术的情况并判断玩家弓箭手是否可以攻击到
                                if (dx is >= 5 and <= 7 && dy == 0 && UIBattle.Instance.uiTactic.CheckTacticalState(3,true) && aix > cellX)
                                {
                                    for (int i = 1; i < dx; i++)
                                    {
                                        if (BM.battleMap[cellY, cellX + i] == Byte.MinValue)
                                        {
                                            flag1 = false;
                                            break;
                                        }
                                        flag1 = true;
                                    }
                                }

                                // 处理正常的情况并判断玩家弓箭手是否可以攻击到
                                if (dx is >= 1 and <= 4 && dy == 0 && aix > cellX)
                                {
                                    if (aix > cellX + 1)
                                    {
                                        for (int i = 1; i < dx; i++)
                                        {
                                            if (BM.battleMap[cellY, cellX + i] == Byte.MinValue)
                                            {
                                                flag1 = false;
                                                break;
                                            }
                                            flag1 = true;
                                        }
                                    }
                                    else if (aix == cellX + 1)
                                    {
                                        flag1 = true;
                                    }
                                }

                                // 同在竖直方向上的判断
                                if (dx == 0)
                                {
                                    if (aiy > cellY + 1)
                                    {
                                        for (int i = 1; i < dy; i++)
                                        {
                                            if (BM.battleMap[cellY + i, cellX] == Byte.MinValue)
                                            {
                                                flag1 = false;
                                                break;
                                            }
                                            flag1 = true;
                                        }
                                    }
                                    else if (aiy < cellY - 1)
                                    {
                                        for (int i = 1; i < dy; i++)
                                        {
                                            if (BM.battleMap[cellY - i, cellX] == Byte.MinValue)
                                            {
                                                flag1 = false;
                                                break;
                                            }
                                            flag1 = true;
                                        }
                                    }
                                    else if (aiy == cellY + 1 || aiy == cellY - 1)
                                    {
                                        flag1 = true;
                                    }
                                }
                            }

                            // 类似的逻辑用于不同种类的士兵
                            else if (BM.GetTroopByXY(cellX, cellY)!=null && BM.GetTroopByXY(cellX, cellY).troopType == TroopType.Cavalry && dx + dy <= 2)
                            {
                                // 判断是否可以攻击的条件
                                if (aiy == cellY && aix == cellX + 2 && BM.battleMap[aiy, aix - 1] != Byte.MinValue)
                                    flag1 = true;
                                if (aiy == cellY && aix == cellX + 1)
                                    flag1 = true;
                                if (aiy == cellY && aix == cellX - 2 && BM.battleMap[aiy, aix + 1] != Byte.MinValue)
                                    flag1 = true;
                                if (aiy == cellY && aix == cellX + 1)
                                    flag1 = true;
                                if (aix == cellX && aiy == cellY + 2 && BM.battleMap[aiy + 1, aix] != Byte.MinValue)
                                    flag1 = true;
                                if (aix == cellX && aiy == cellY + 1)
                                    flag1 = true;
                                if (aix == cellX && aiy == cellY - 2 && BM.battleMap[aiy - 1, aix] != Byte.MinValue)
                                    flag1 = true;
                                if (aix == cellX && aiy == cellY - 1)
                                    flag1 = true;
                            }

                            // 如果可以攻击，计算攻击点数
                            if (flag1)
                            {
                                short blood = 1;
                                short atk = 1;

                                // 遍历己方士兵列表，找到对应的士兵并计算其攻击点数
                                for (int hmindex = 0; hmindex < BM.hmTroops.Count; hmindex++)
                                {
                                    if (BM.hmTroops[hmindex].health > 0 && BM.GetTroopByXY(cellX, cellY)!=null&& BM.GetTroopByXY(cellX, cellY) != null)
                                    {
                                        if (hmindex == 0)
                                        {
                                            blood = 300;
                                            //TODO BM.hmTroops[hmindex].InitPower();
                                            atk = BM.hmTroops[hmindex].attackPower;
                                            break;
                                        }
                                        blood = BM.hmTroops[hmindex].health;
                                        //BM.hmTroops[hmindex].InitPower();
                                        atk = BM.hmTroops[hmindex].attackPower;
                                        break;
                                    }
                                }
                                //BM.aiTroops[0].InitPower();
                                canatkps = (short)(canatkps + BM.aiTroops[0].CalculateDamage(BM.hmTroops[0]));
                            }
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                canatkps = 50; // 捕获异常，设置默认攻击点数
                Debug.LogError(e);
            }

            // 根据计算的攻击点数和当前AI将领的体力判断是否撤退
            if ((canatkps > BM.aiGeneral.GetCurPhysical() - 35 && canatkps > 0) || BM.aiGeneral.GetCurPhysical() < 35 && pySoldierNum > 450 && aiSoldierNum < 100 && BM.aiTacticPoint < 12)
                return true;

            // 判断是否需要执行"可能被包围"策略
            if (SurroundEarlyWarning(canatkps))
                return true;

            return false;
        }

        /// <summary>
        /// 判断AI是否单挑能赢否则撤退
        /// </summary>
        /// <returns></returns>
        bool AIGenRefuseSolo()
        {
            // 判断当前将领的体力是否低于对方攻击造成的伤害
            if (BM.aiGeneral.GetCurPhysical() < SoloManager.GetAtkDea(BM.hmGeneral, BM.hmTroops[0].attackPower, BM.aiTroops[0].defensePower) + 1)
                return true;
            DuelTactic duelTactic = new DuelTactic();
            bool isSuccessOfDuel = duelTactic.CanExecute(2, true);
            // 如果单挑会失败则撤退
            return !isSuccessOfDuel;
        }
    
        // 判断是否可以单次攻击
        bool CanAdjacentSolo()
        {
            byte aix = (byte)BM.aiTroops[0].arrayPos.x;
            byte aiy = (byte)BM.aiTroops[0].arrayPos.y;
            byte hmx = (byte)BM.hmTroops[0].arrayPos.x;
            byte hmy = (byte)BM.hmTroops[0].arrayPos.y;

            // 判断条件是否满足攻击
            if (aix > hmx)
            {
                if (aix - hmx == 2 && aiy == hmy && BM.battleMap[aiy, aix - 1] >= 0 && BM.battleMap[aiy, aix - 1] <= 16)
                    return true;//横向间隔一个空位
                if (aix - hmx == 1 && aiy == hmy)//横向邻接
                    return true;
                if (aix - hmx == 1 && aiy - hmy == 1 && BM.battleMap[aiy - 1, aix] >= 0 && BM.battleMap[aiy - 1, aix] <= 16)
                    return true;//左上角
                if (aix - hmx == 1 && hmy - aiy == 1 && BM.battleMap[aiy + 1, aix] >= 0 && BM.battleMap[aiy + 1, aix] <= 16)
                    return true;//左下角
            }
            else if (aix == hmx)// 同在竖直方向上
            {
                // 判断纵坐标差是否小于等于 2
                if (Mathf.Abs(aiy - hmy) <= 2)
                    return true;
            }
            else
            {
                if (hmx - aix == 2 && hmy == aiy)
                    return true;
                if (hmx - aix == 1 && Mathf.Abs(aiy - hmy) <= 2)
                    return true;
            }
            return false;
        }

        // 判断是否可以进行单次攻击（另一种情况）
        bool CanAdjacentSolo2()
        {
            byte aix = (byte)BM.aiTroops[0].arrayPos.x;
            byte aiy = (byte)BM.aiTroops[0].arrayPos.y;
            byte hmx = (byte)BM.hmTroops[0].arrayPos.x;
            byte hmy = (byte)BM.hmTroops[0].arrayPos.y;

            // 判断距离是否小于等于 3
            if (Mathf.Abs(aix - hmx) + Mathf.Abs(aiy - hmy) <= 3)
                return true;
            return false;
        }
    

        // 野战时AI行为控制
        void AIField()
        {
            byte x = (byte)BM.aiTroops[0].arrayPos.x; // AI士兵X坐标
            byte y = (byte)BM.aiTroops[0].arrayPos.y; // AI士兵Y坐标
            byte s0Num = 0; // 0血量的士兵数量
            byte s50Num = 0; // 血量大于等于50的士兵数量

            // 统计AI所有士兵的血量信息
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0)
                    s0Num++;
                if (BM.aiTroops[i].health >= 50)
                    s50Num++;
            }

            // 如果W值大于等于12，则根据距离决定AI行为
            if (BM.aiTacticPoint >= 12)
            {
                byte hmX = (byte)BM.hmTroops[0].arrayPos.x; // 玩家士兵X坐标
                byte hmY = (byte)BM.hmTroops[0].arrayPos.y; // 玩家士兵Y坐标
                byte dx = (byte)(x - hmX); // X轴距离
                byte dy = (byte)Math.Abs(y - hmY); // Y轴距离

                // 如果AI与玩家士兵的距离符合条件，则减少W值并设置AI行动标志
                if (dx >= 1 && dx <= 3 && dy <= 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(6, false);
                }
                else
                {
                    byte canBoomNum = 0; // 可行动的格子数量

                    // 遍历战场上的所有格子，计算可以行动的格子数量
                    for (byte cellY = 0; cellY < 7; cellY = (byte)(cellY + 1))
                    {
                        for (byte cellX = 0; cellX < 16; cellX = (byte)(cellX + 1))
                        {
                            if (BM.battleMap[cellY, cellX]  == 64)
                            {
                                byte dsx = (byte)(x - cellX);
                                byte dsy = (byte)Math.Abs(y - cellY);
                                if (dsx >= 1 && dsx <= 3 && dsy <= 1)
                                    canBoomNum++;
                            }
                        }
                    }

                    bool doBoom = false; // 是否决定行动

                    // 根据血量大于等于50的士兵数量和可以行动的格子数量决定是否行动
                    if (s50Num <= 1 && canBoomNum >= 1)
                    {
                        doBoom = true;
                    }
                    else if (s50Num <= 2 && canBoomNum >= 2)
                    {
                        doBoom = true;
                    }
                    else if (canBoomNum >= 3)
                    {
                        doBoom = true;
                    }

                    if (doBoom)
                    {
                        UIBattle.Instance.uiTactic.ApplyTactic(6, false);
                    }
                }
            }

            // 获取AI和玩家战场中的士兵数量
            short pySoldierNum = GetBattleSoldierNum(true);
            short aiSoldierNum = GetBattleSoldierNum(false);
            AIUseTactic(); // 执行AI的战术功能

            // 如果AI未设置前进标志
            if (!_aiForward)
            {
                // 设置所有AI士兵的行动指令为待机
                for (byte i = 0; i < BM.aiTroops.Count; i++)
                    BM.aiTroops[i].data.troopState = TroopState.Idle;

                // 判断是否需要前进
                if (aiSoldierNum > pySoldierNum * 2 || aiSoldierNum >= 350)
                {
                    _aiForward = true; // 设置前进标志
                }
                else
                {
                    HumanSoldierDetection(); // 执行AI的进攻逻辑
                }

                // 如果需要前进，则将所有AI士兵的行动指令设置为前进
                if (_aiForward)
                {
                    for (byte i = 1; i < BM.aiTroops.Count; i++)
                        BM.aiTroops[i].data.troopState = TroopState.Forward;
                }
            }

            int maxsh = 0; // 玩家最大伤害值
            int count = Mathf.Min(BM.hmTroops.Count, BM.aiTroops.Count); // 获取玩家和AI士兵数量更小的值
            // 计算玩家所有士兵的总伤害
            for (int i = 1; i < count; i++)
            {
                if (BM.hmTroops[i].health > 0)
                {
                    int cursh = (int)BM.aiTroops[i].CalculateDamage(BM.hmTroops[i]);
                    maxsh += cursh;
                }
            }

            byte hm70Num = 0; // 血量大于等于100的玩家士兵数量
            for (byte i = 1; i < BM.hmTroops.Count; i++)
            {
                if (BM.hmTroops[i].health >= 100)
                    hm70Num++;
            }

            // 根据玩家当前状态和AI状态调整AI的策略
            if (BM.aiGeneral.GetCurPhysical() - 35 > maxsh && (hm70Num < 3 || maxsh < 25) && !AIGenRefuseSolo())
            {
                if (CanAdjacentSolo2() && AIGenRefuseSolo())
                {
                    BM.aiTroops[0].data.troopState = TroopState.Idle; // 待机
                }
                else
                {
                    BM.aiTroops[0].data.troopState = TroopState.Forward; // 进攻
                }
            }
            else
            {
                BM.aiTroops[0].data.troopState = TroopState.Idle; // 待机
            }

            // AI撤退判断
            if (AIGenBattleRetreat())
                BM.aiTroops[0].data.troopState = TroopState.BackWard;// 撤退

            if ((AIGenRefuseSolo() && aiSoldierNum < 100) || (CanAdjacentSolo() && AIGenRefuseSolo()))
                BM.aiTroops[0].data.troopState = TroopState.BackWard;// 撤退

            // 如果AI能够单挑胜利且接近玩家士兵，则进行单挑
            if (CanSingleWin() && CanNearSingle())
                BM.aiTroops[0].data.troopState = TroopState.Idle; // 待机
        }
    
        // ai使用战术方法
        void AIUseTactic()
        {
            byte troopNum = 0;
            // 计算幸存小兵数量
            for (byte i = 1; i < BM.aiTroops.Count; i++)
            {
                if (BM.aiTroops[i].health > 0)
                    troopNum++;
            }

            // 当W值大于等于8时，优先使用战术
            if (BM.aiTacticPoint >= 8)
            {
                byte shoutAtkValue = ShoutAtkValue();  // 获取可攻击单位数量
                // 如果可攻击数量足够多且小兵数量足够时，减少AI战术点并执行战术
                if (shoutAtkValue >= troopNum / 3 + 1 && troopNum >= 3)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(4, false);
                    return;
                }

                CrossbowAtkValue();  // 计算远程攻击数量
                if (BM.aiTacticPoint >= 8 && shoutAtkValue >= troopNum / 2 + 1 && troopNum >= 2)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(4, false);
                    return;
                }

                if (BM.aiTacticPoint >= 8 && shoutAtkValue >= troopNum && troopNum >= 1)
                {
                    UIBattle.Instance.uiTactic.ApplyTactic(4, false);
                    return;
                }
            }
            else if (BM.aiTacticPoint >= 7)
            {
                CrossbowAtkValue();  // 计算远程攻击数量
            }
        }
    
    
        // 人类小兵检测
        void HumanSoldierDetection()
        {
            // 遍历所有人类小兵
            for (byte i = 1; i < BM.hmTroops.Count; i++)
            {
                if (BM.hmTroops[i].health > 0)
                {
                    byte x = (byte)BM.hmTroops[i].arrayPos.x;
                    byte y = (byte)BM.hmTroops[i].arrayPos.y;

                    // 检查小兵是否在指定坐标内，并检查对应的ai小兵是否可以被攻击
                    for (byte d = 1; d < 5; d = (byte)(d + 1))
                    {
                        // 检查上方
                        if (y >= d && (BM.battleMap[y - d, x] & 0x80) != 0)
                        {
                            byte a = 1;
                            while (a < BM.aiTroops.Count)
                            {
                                if (BM.aiTroops[a].health > 0 && BM.GetTroopByXY(x, (byte)(y- d)) != null)
                                {
                                    // 如果对应的小兵不是远程攻击小兵，标记aiForward为true
                                    if (BM.aiTroops[a].troopType != TroopType.Archer)
                                    {
                                        _aiForward = true;
                                        return;
                                    }
                                    break;
                                }
                                a++;
                            }
                        }

                        // 检查下方
                        if (y + d <= 6 && (BM.battleMap[y + d, x] & 0x80) != 0)
                        {
                            byte b = 1;
                            while (b < BM.aiTroops.Count)
                            {
                                if (BM.aiTroops[b].health > 0 && BM.GetTroopByXY(x, (byte)(y + d)) != null)
                                {
                                    if (BM.aiTroops[b].troopType != TroopType.Archer)
                                    {
                                        _aiForward = true;
                                        return;
                                    }
                                    break;
                                }
                                b++;
                            }
                        }

                        // 检查左侧
                        if (x >= d && (BM.battleMap[y, x - d] & 0x80) != 0)
                        {
                            byte byte4 = 1;
                            while (byte4 < BM.aiTroops.Count)
                            {
                                if (BM.aiTroops[byte4].health > 0 && BM.GetTroopByXY((byte)(x - d), y) != null)
                                {
                                    if (BM.aiTroops[byte4].troopType != TroopType.Archer)
                                    {
                                        _aiForward = true;
                                        return;
                                    }
                                    break;
                                }
                                byte4 = (byte)(byte4 + 1);
                            }
                        }

                        // 检查右侧
                        if (x + d <= 15 && (BM.battleMap[y, x + d] & 0x80) != 0)
                        {
                            byte byte5 = 1;
                            while (byte5 < BM.aiTroops.Count)
                            {
                                if (BM.aiTroops[byte5].health > 0 && BM.GetTroopByXY((byte)(x + d), y) != null)
                                {
                                    if (BM.aiTroops[byte5].troopType != TroopType.Archer)
                                    {
                                        _aiForward = true;
                                        return;
                                    }
                                    break;
                                }
                                byte5 = (byte)(byte5 + 1);
                            }
                        }
                    }
                }
            }
        }
    
        /// <summary>
        /// 旧版计算伤害值
        /// </summary>
        /// <param name="atk"></param>
        /// <param name="def"></param>
        /// <param name="blood"></param>
        /// <returns></returns>
        short Getshs(short atk, short def, short blood)
        {
            int gjl = atk; // 攻击力
            int fyl = def; // 防御力
            int F = blood / 20; // 血量因子
            float t1 = fyl / 150.0F; // 计算防御比例
            t1 *= TextLibrary.hj[fyl - 1]; // 防御系数调整
            float sh = gjl * 1.0F / (1.0F + t1); // 计算基础伤害值
            if (blood < 200) // 如果血量低于200，伤害按比例调整
                sh = sh * blood / 200.0F;
            if (sh < F) // 伤害值不小于最低伤害值F
                sh = F;
            sh /= 6.0F; // 平均伤害值
            if (sh < 1.0F) // 伤害值不小于1
                sh = 1.0F;
            return (short)(int)sh;
        }
    
        /// <summary>
        /// 判断AI是否能够单挑获胜
        /// </summary>
        /// <returns></returns>
        bool CanSingleWin()
        {
            if (BM.aiGeneral.GetCurPhysical() < SoloManager.GetAtkDea(BM.hmGeneral, BM.hmTroops[0].attackPower, BM.aiTroops[0].defensePower) + 1)
                return false;
            DuelTactic duel =new DuelTactic();
            if (duel.CanExecute(2, true))
                return true;
            return false;
        }

        /// <summary>
        /// 判断AI是否接近可以单挑的位置
        /// </summary>
        /// <returns></returns>
        bool CanNearSingle()
        {
            byte aix = (byte)BM.aiTroops[0].arrayPos.x; // AI士兵X坐标
            byte aiy = (byte)BM.aiTroops[0].arrayPos.y; // AI士兵Y坐标
            byte hmx = (byte)BM.hmTroops[0].arrayPos.x; // 玩家士兵X坐标
            byte hmy = (byte)BM.hmTroops[0].arrayPos.y; // 玩家士兵Y坐标

            if (BM.aiTroops[0].currentActionPoints > 0) // 如果AQ值大于0，不允许单挑必需还有可以移动两格
                return false;

            // 判断AI是否可以接近玩家士兵
            if (aix > hmx)
            {
                if (aix - hmx == 2 && aiy == hmy && BM.battleMap[aiy, aix - 1] >= 0 && BM.battleMap[aiy, aix - 1] <= 16)
                    return true;
                if (aix - hmx == 1 && aiy - hmy == 1 && BM.battleMap[aiy - 1, aix] >= 0 && BM.battleMap[aiy - 1, aix] <= 16)
                    return true;
                if (aix - hmx == 1 && hmy - aiy == 1 && BM.battleMap[aiy + 1, aix] >= 0 && BM.battleMap[aiy + 1, aix] <= 16)
                    return true;
            }
            else if (aix == hmx)
            {
                if (Math.Abs(aiy - hmy) <= 2)
                    return true;
            }
            else
            {
                if (hmx - aix == 2 && hmy == aiy)
                    return true;
                if (hmx - aix == 1 && Math.Abs(aiy - hmy) <= 2)
                    return true;
            }
            return false;
        }


        // 被包围前的预警逃跑
        bool SurroundEarlyWarning(short hurt)
        {
            byte aix = (byte)BM.aiTroops[0].arrayPos.x;
            byte aiy = (byte)BM.aiTroops[0].arrayPos.y;
            bool willBeAtk = false;

            // 遍历战场坐标
            for (int cellY = 0; cellY < 7; cellY++)
            {
                for (int cellX = 0; cellX < 16; cellX++)
                {
                    if (BM.battleMap[cellY, cellX]  == 64 && BM.GetTroopByXY((byte)cellX, (byte)cellY)!=null && (BM.GetTroopByXY((byte)cellX, (byte)cellY).troopType == TroopType.Captain || BM.GetTroopByXY((byte)cellX, (byte)cellY).troopType == TroopType.Cavalry))
                    {
                        // 检查是否在包围范围内
                        if (aix == cellX && Mathf.Abs(cellY - aiy) == 1)
                        {
                            willBeAtk = true;
                            break;
                        }
                        if (cellX == aix + 1 && Mathf.Abs(cellY - aiy) <= 2)
                        {
                            willBeAtk = true;
                            break;
                        }
                    }
                }
            }

            if (BM.aiGeneral.GetCurPhysical() - hurt - 15 < 0 && willBeAtk)
                return true;

            byte curps = BM.aiGeneral.GetCurPhysical();
            BM.aiGeneral.SubHp((byte)hurt);
            if (BM.aiGeneral.GetCurPhysical() < 1)
                BM.aiGeneral.SetCurPhysical((byte)1);

            if (AIGenRefuseSolo() && willBeAtk)
            {
                BM.aiGeneral.SetCurPhysical(curps);
                return true;
            }

            BM.aiGeneral.SetCurPhysical(curps);
            return false;
        }
    }
}