using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using UnityEngine;

namespace War
{
    public class AIWar
    {
        public float _aiInterval = 0.2f;
        public UnitData _aiUnit;
        private General _aiGeneral;
        private UnitObj tarHmUnit;
        public byte bestPlanId;
        
        private byte _aiIndex
        {
            get => WarManager.Instance.aiIndex;
            set => WarManager.Instance.aiIndex = value;
        }
        
        public AIWar(UnitData aiUnit)
        {
            _aiUnit = aiUnit;
            _aiGeneral = GeneralListCache.GetGeneral(aiUnit.genID);
            tarHmUnit = null;
            bestPlanId = 255;
        }
        
        IEnumerator AIExecute(UnitObj aiUnitObj)
        {
            if (aiUnitObj.IsCommander)
            {
                if (IsAICommanderRetreat())
                {
                    SingletonRetreat();
                    AIFollowRetreat();
                    yield return UIWar.Instance.uiTips. ShowNoticeTips("敌军全军撤退");
                    yield break;
                }
            }
            else
            {
                if (IsAIUnitRetreat())
                {
                    SingletonRetreat();
                    yield return UIWar.Instance.uiTips.ShowNoticeTips("敌将逃遁");
                    yield break;
                }
            }

            //判断是否要逃跑
            // 如果 AI 单位尚未行动
            if (aiUnitObj.unitState != UnitState.Idle)
            {
                if (aiUnitObj.IsCommander && !WarManager.Instance.isHmDef) // 特殊处理AI主将坚守城池
                {
                    Debug.Log("主将守城");
                    // 循环处理 AI 的行动直到完成
                    while (aiUnitObj.unitState != UnitState.Idle)
                    {
                        if (GetBestPlanTarget(0))
                        {
                            AIExecutePlan();
                            yield return new WaitForSeconds(_aiInterval);
                            continue;
                        }

                        aiUnitObj.SetUnitState(UnitState.Idle);
                    }
                }
                // 处理其他移动过的 AI 单位
                else if (aiUnitObj.isMoved)
                {
                    // 循环处理 AI 的行动直到完成
                    while (aiUnitObj.unitState != UnitState.Idle)
                    {
                        if (GetBestPlanTarget(35))
                        {
                            AIExecutePlan();
                            yield return new WaitForSeconds(_aiInterval);
                            continue;
                        }

                        // 判断 AI 是否可以攻击
                        if (aiUnitObj.moveBonus >= 2)
                        {
                            if (GetBestBattleTarget(GetBattleScore))
                            {
                                aiUnitObj.SubMoveBonus(2);
                                if (aiUnitObj.moveBonus < 2)
                                    aiUnitObj.SetUnitState(UnitState.Idle);
                                AIExecuteBattle();
                                yield return WarManager.Instance.battleState == BattleState.BattleOver;
                                yield return new WaitForSeconds(_aiInterval);
                                yield break; // 攻击后返回不在执行下面的移动
                            }
                        }

                        // 如果没有攻击目标，判断最后是否可以用计，此次判断不考虑成功率，即机动力满足就用计
                        if (GetBestPlanTarget(0))
                        {
                            AIExecutePlan();
                            yield return new WaitForSeconds(_aiInterval);
                            continue;
                        }

                        aiUnitObj.SetUnitState(UnitState.Idle);
                    }
                }
                else // 处理其他还没移动过的 AI 单位
                {
                    yield return AIGetMovePath(aiUnitObj, AIMoveMap());
                    yield return null;
                    Debug.Log($"AI:{aiUnitObj.genID}移动完成");
                    // 移动过但还有机动力的武将
                    if (aiUnitObj.isMoved && aiUnitObj.unitState != UnitState.Idle)
                    {
                        while (aiUnitObj.unitState != UnitState.Idle)
                        {
                            UnitObj tarHmUnitObj = null;
                            byte bestPlanId = 255;
                            if (GetBestPlanTarget( 35))
                            {
                                AIExecutePlan();
                                yield return new WaitForSeconds(_aiInterval);
                                continue;
                            }

                            if (aiUnitObj.moveBonus >= 2)
                            {
                                if (GetBestBattleTarget(GetBattleIqScore))
                                {
                                    aiUnitObj.SubMoveBonus(2);
                                    if (aiUnitObj.moveBonus < 2)
                                        aiUnitObj.SetUnitState(UnitState.Idle);
                                    AIExecuteBattle();
                                    yield return WarManager.Instance.battleState == BattleState.BattleOver;
                                    yield return new WaitForSeconds(_aiInterval);
                                    yield break;
                                }
                            }

                            if (GetBestPlanTarget(0))
                            {
                                AIExecutePlan();
                                yield return new WaitForSeconds(_aiInterval);
                                continue;
                            }

                            aiUnitObj.SetUnitState(UnitState.Idle);
                            Debug.Log($"AI单位:{aiUnitObj.genID}的行动完成");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 判断AI主将是否撤退
        /// </summary>
        /// <returns>AI主将撤退为真</returns>
        public bool IsAICommanderRetreat()
        {
            // 检查是否满足撤退条件
            List<byte> retreatCityIdList = WarManager.AIGetRetreatCityList(WarManager.Instance.aiKingId);
            if (retreatCityIdList == null || retreatCityIdList.Count == 0)
            {
                return false;
            }
            
            // 计算双方战斗力
            int i = CalculateAIRetreatValue(true);
            int j = CalculateAIRetreatValue(false);

            // 获取主将信息
            General aiCommander = _aiGeneral;

            // 如果主将不存在，则允许撤退
            if (aiCommander == null)
                return true;

            // 计算额外士兵数量
            int k = aiCommander.lead * aiCommander.generalSoldier * 3 / 2;
            j += k;

            // 判断是否满足撤退条件
            if (j * 3 < i * 2 && aiCommander.generalSoldier <= 250)
                return true;

            // 最终判断是否撤退
            return !(aiCommander.generalSoldier >= 100 && (aiCommander.generalSoldier >= 400 || aiCommander.GetCurPhysical() >= 15));
        }

        /// <summary>
        /// 用于评估是否AI撤退的评分
        /// </summary>
        /// <param name="isPlayer">是否是玩家方</param>
        /// <returns>计算战争中某方将领的总能力值</returns>
        int CalculateAIRetreatValue(bool isPlayer)
        {
            List<UnitObj> units = isPlayer ? WarManager.Instance.hmUnits : WarManager.Instance.aiUnits;
            return units.Where(unit => unit.unitState != UnitState.Captive).Sum(unit => GeneralListCache.GetGeneral(unit.genID).CalculateWarValue());
        
        }

        /// <summary>
        /// 其他武将是否撤退
        /// </summary>
        /// <returns>撤退为真</returns>
        public bool IsAIUnitRetreat()
        {
            // 检查是否满足撤退条件
            List<byte> retreatCityIdList = WarManager.AIGetRetreatCityList(WarManager.Instance.aiKingId);
            if (retreatCityIdList == null || retreatCityIdList.Count == 0)
            {
                return false;
            }

            // 获取将领信息
            General general = _aiGeneral;

            // 如果将领不存在，则不允许撤退
            if (general == null)
                return false;

            // 检查士兵数量是否少于 300或者低血量且士兵数量少于 500
            return general.generalSoldier < 300 || (general.GetCurPhysical() < 15 && general.generalSoldier < 500);
        }

        /// <summary>
        /// 单个AI单位的撤退
        /// </summary>
        public void SingletonRetreat()
        {
            UnitObj t = WarManager.GetUnitByPos(_aiUnit.arrayPos);
            WarManager.RetreatGeneralToCity(t, GetCanRetreatCityId(), WarManager.Instance.aiKingId);
            t.Remove(UnitState.Retreat);
            if (WarManager.Instance.aiUnits.Count == 0)
            {
                WarManager.Instance.AIWithdraw();
            }
        }

        /// <summary>
        /// 主将能撤退时全军撤退
        /// </summary>
        public static void AIFollowRetreat()
        {
            List<UnitObj> sortUnits= WarManager.Instance.aiUnits.OrderByDescending(unit => GeneralListCache.GetGeneral(unit.genID).AllStatus()).ToList();
            foreach (var unit in sortUnits)
            {
                if (unit.unitState == UnitState.Captive) continue;
                AIWar aiWar = new AIWar(unit.data);
                aiWar.SingletonRetreat();
            }
        }

        /// <summary>
        /// 获取可以撤退的城市 ID
        /// </summary>
        /// <returns>可以撤退的城市ID，如果没有可撤退的城市，则返回0</returns>
        static byte GetCanRetreatCityId()
        {
            // 获取可以撤退的城市列表
            List<byte> retreatCityIdList = WarManager.AIGetRetreatCityList(WarManager.Instance.aiKingId);

            // 如果没有可撤退的城市，返回 0
            if (retreatCityIdList == null || retreatCityIdList.Count == 0)
            {
                return 0;
            }

            // 返回将领数量最少的城市 ID
            return retreatCityIdList
                .OrderBy(cityId => CityListCache.GetCityByCityId(cityId).GetCityOfficerNum())
                .FirstOrDefault(); // 如果列表为空，则返回默认值 0
        }

        

        
        
        



        /// <summary>
        /// 遍历所有可用的人类将军目标并计算计策成功率，选择一个最合适的目标进行计策。
        /// </summary>
        /// <param name="minRate">最小成功率阈值</param>
        /// <returns>返回一个是否得到最佳的计谋目标和ID</returns>
        public bool GetBestPlanTarget(byte minRate)
        {
            if (_aiUnit.moveBonus < 4)
                return false;
                
            General aiGeneral = _aiGeneral;
            int distance = aiGeneral.HasSkill(1,1) ? 6 : 5;//特技鬼谋
            tarHmUnit = null;
            bestPlanId = 255;
            byte maxScore = 0;

            // 遍历所有玩家的单位
            foreach (var hmUnit in WarManager.Instance.hmUnits)
            {
                // 只考虑在计谋范围内的非俘虏单位
                if (hmUnit.unitState != UnitState.Captive && MapManager.IsInRange(_aiUnit.arrayPos, hmUnit.arrayPos, distance))
                {
                    var result = GetUsePlan(hmUnit, minRate);
                    if (result.Item1 != 255)
                    {
                        byte score = result.Item2;
                        // 选择最优计谋
                        if (score > minRate && score > maxScore)
                        {
                            maxScore = score;
                            tarHmUnit = hmUnit;
                            bestPlanId = result.Item1;
                        }
                    }
                }
            }

            if (tarHmUnit != null && bestPlanId != 255)
            {
                Debug.Log("获取到最佳计谋目标");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// AI确认执行计谋
        /// </summary>
        public void AIExecutePlan()
        {
            General aiGeneral = _aiGeneral;
            General hmGeneral = GeneralListCache.GetGeneral(tarHmUnit.genID);
            WarManager.Instance.planResult = "计谋施展失败";
            // 执行计谋
            Plan plan = Plan.GetPlan(bestPlanId);
            if (_aiUnit.moveBonus >= plan.UseCost(aiGeneral))
            {
                if (plan.IsExecute(aiGeneral, hmGeneral, false))
                {
                    WarManager.GetUnitByPos(_aiUnit.arrayPos).GetArmySprite();
                    WarManager.GetUnitByPos(tarHmUnit.arrayPos).GetArmySprite();
                }
                _aiUnit.SubMoveBonus(plan.UseCost(aiGeneral));
            }
            
            Debug.Log($"AI执行计谋：{plan.Name}，消耗机动力：{plan.UseCost(aiGeneral)}，剩余机动力：{_aiUnit.moveBonus},目标地形{WarManager.GetUnitByPos(tarHmUnit.arrayPos).Terrain}");
        }

        /// <summary>
        /// 根据AI单位和玩家单位选择最佳计谋
        /// </summary>
        /// <param name="hmUnitObj"></param>
        /// <param name="minRate">最小成功率要求</param>
        /// <returns>最佳计谋ID，返回255表示没有可用计谋</returns>
        (byte , byte) GetUsePlan(UnitObj hmUnitObj, byte minRate)
        {
            General aiGeneral = _aiGeneral;
            General hmGeneral = GeneralListCache.GetGeneral(hmUnitObj.genID);
            byte planNum = aiGeneral.GetPlanNum();

            // 计谋价值字典
            var planValues = new Dictionary<byte, byte>
            {
                { 0, 3 }, { 3, 3 }, { 4, 3 },
                { 1, 1 },
                { 2, 2 },
                { 6, 6 }, { 7, 6 }, { 9, 6 },
                { 5, 10 }, { 8, 10 },
                { 10, 8 }, { 11, 8 }, { 12, 8 }, { 13, 8 }, { 14, 8 }, { 15, 8 }
            };

            // 筛选有效计谋
            var validPlans = new List<(byte PlanId, byte Rate)>();

            for (byte i = 0; i < planNum; i++)
            {
                Plan plan = Plan.GetPlan(i);

                // 检查基本条件
                if (plan.UseCost(aiGeneral) > _aiUnit.moveBonus ||
                    !plan.IsApplicable(hmUnitObj.Terrain) ||
                    !MapManager.IsInRange(_aiUnit.arrayPos, hmUnitObj.arrayPos, plan.InDistance(aiGeneral)) ||
                    (plan is IDoTerrain dtp && !dtp.IsSuitable(_aiUnit.Terrain)) ||
                    (plan is IMultiplePlan mp && !mp.IsMultipleTarget(hmGeneral, false)) ||
                    (plan is IBeCommanderPlan bcp && !bcp.IsBeCommanderTarget(hmGeneral, false)))
                {
                    continue;
                }

                // 检查成功率是否满足要求
                byte planRate = plan.Rate(aiGeneral, hmGeneral);
                if (planRate <= minRate) continue;

                // 检查特殊计谋条件
                bool isValid = i switch
                {
                    0 or 3 or 4 or 6 or 7 or 9 or 11 or 12 or 13 or 14 => hmGeneral.generalSoldier > 30,
                    1 => hmGeneral.GetCurPhysical() > 1,
                    2 => hmGeneral.generalSoldier > 30 && hmUnitObj.unitState != UnitState.Trapped && hmUnitObj.unitState != UnitState.Captive,
                    5 or 8 => WarManager.Instance.hmFood > 0,
                    10 => hmUnitObj.trappedDay < 2,
                    _ => false
                };

                if (isValid)
                {
                    validPlans.Add((i, planRate));
                }
            }

            // 如果没有有效计谋，返回默认值
            if (validPlans.Count == 0) return (255, 0);

            // 如果没有最小成功率限制，优先选择最大成功率的计谋
            if (minRate == 0)
            {
                return validPlans.OrderByDescending(p => p.Rate).First();
            }

            // 根据计谋价值筛选最佳计谋
            byte planId = 255; // 默认无效值
            byte maxPlanValue = 0;

            foreach (var (id, rate) in validPlans)
            {
                byte planValue = planValues.GetValueOrDefault(id, (byte)0);

                // 特殊逻辑处理
                if (id == 2 && hmGeneral.generalSoldier > 1400)
                {
                    planValue = 4;
                }

                // 更新最大价值计谋
                if (planValue > maxPlanValue)
                {
                    maxPlanValue = planValue;
                    planId = id;
                }
            }
           
            return validPlans.FirstOrDefault(t => t.PlanId == planId);
        }
        
        /// <summary>
        /// 获取 AI 单位的最佳计谋
        /// </summary>
        /// <param name="hmUnitObj">玩家单位对象</param>
        /// <param name="tarPos">施放时数组位置</param>
        /// <returns>计谋中对玩家单位造成的最大伤害评分</returns>
        byte GetAIThinkPlan(UnitObj hmUnitObj, Vector2Int tarPos)
        {
            General aiGeneral = _aiGeneral;
            General hmGeneral = GeneralListCache.GetGeneral(hmUnitObj.genID);
            byte planNum = aiGeneral.GetPlanNum();

            // 创建计谋价值字典
            var planValues = new Dictionary<byte, byte>
            {
                { 0, 3 }, { 3, 3 }, { 4, 3 },
                { 1, 1 },
                { 2, 2 },
                { 6, 6 }, { 7, 6 }, { 9, 6 },
                { 5, 10 }, { 8, 10 },
                { 10, 8 }, { 11, 8 }, { 12, 8 }, { 13, 8 }, { 14, 8 }, { 15, 8 }
            };

            // 筛选有效计谋
            var validPlans = new List<byte>();

            for (byte i = 0; i < planNum; i++)
            {
                Plan plan = Plan.GetPlan(i);

                // 检查基本条件
                if (plan.UseCost(aiGeneral) > _aiUnit.moveBonus ||
                    !plan.IsApplicable(hmUnitObj.Terrain) ||
                    !MapManager.IsInRange(tarPos, hmUnitObj.arrayPos, plan.InDistance(aiGeneral)) ||
                    (plan is IDoTerrain dtp && !dtp.IsSuitable(WarManager.Instance.warMap[tarPos.y, tarPos.x])) ||
                    (plan is IMultiplePlan mp && !mp.IsMultipleTarget(hmGeneral, false)) ||
                    (plan is IBeCommanderPlan bcp && !bcp.IsBeCommanderTarget(hmGeneral, false)) ||
                    plan.Rate(aiGeneral, hmGeneral) <= 35)
                {
                    continue;
                }

                // 根据不同的计谋条件筛选
                bool isValid = i switch
                {
                    0 or 3 or 4 or 6 or 7 or 9 or 11 or 12 or 13 or 14 => hmGeneral.generalSoldier > 30,
                    1 => hmGeneral.GetCurPhysical() > 1,
                    2 => hmGeneral.generalSoldier > 30 && hmUnitObj.unitState != UnitState.Trapped && hmUnitObj.unitState != UnitState.Captive,
                    5 or 8 => WarManager.Instance.hmFood > 0,
                    10 => hmUnitObj.trappedDay < 2,
                    _ => false
                };

                if (isValid)
                {
                    validPlans.Add(i);
                }
            }

            // 如果没有有效计谋，返回默认值
            if (validPlans.Count == 0) return 255; // 未找到计谋

            // 筛选价值最高的计谋
            byte bestPlanValue = 0; 

            foreach (var planId in validPlans)
            {
                // 根据 planId 获取价值
                byte planValue = planValues.ContainsKey(planId) ? planValues[planId] : (byte)0;

                // 特殊逻辑处理（例如 计谋2 的士兵限制）
                if (planId == 2 && hmGeneral.generalSoldier > 1400)
                {
                    planValue = 4;
                }

                // 更新最大价值计谋
                bestPlanValue = (byte)Mathf.Max(bestPlanValue, planValue);
            }

            return bestPlanValue;
        }

        
        /// <summary>
        /// 获取避免AI步入玩家计谋的方法
        /// </summary>
        /// <param name="hmUnitObj">玩家施放计谋单位</param>
        /// <param name="tarPos">要判断的数组位置</param>
        /// <returns>AI中玩家计策的可能最大伤害</returns>
        byte GetHmThinkPlan(UnitObj hmUnitObj, Vector2Int tarPos)
        {
            General hmGeneral = GeneralListCache.GetGeneral(hmUnitObj.genID);
            General aiGeneral = _aiGeneral;
            byte planNum = hmGeneral.GetPlanNum();

            // 创建计谋价值字典
            var planValues = new Dictionary<byte, byte>
            {
                { 0, 1 }, { 1, 1 }, { 4, 1 },
                { 2, 3 }, { 3, 3 },
                { 6, 4 }, { 7, 4 }, { 9, 4 },
                { 5, 10 }, { 8, 10 },
                { 10, 7 }, { 11, 7 }, { 12, 7 }, { 13, 7 }, { 14, 7 }, { 15, 7 }
            };

            // 筛选有效计谋
            var validPlans = new List<byte>();

            for (byte i = 0; i < planNum; i++)
            {
                Plan plan = Plan.GetPlan(i);

                // 检查基本条件
                if (plan.UseCost(hmGeneral) > hmUnitObj.moveBonus ||
                    !plan.IsApplicable(hmUnitObj.Terrain) ||
                    !MapManager.IsInRange(tarPos, hmUnitObj.arrayPos, plan.InDistance(hmGeneral)) ||
                    (plan is IDoTerrain dtp && !dtp.IsSuitable(WarManager.Instance.warMap[tarPos.y, tarPos.x])) ||
                    (plan is IMultiplePlan mp && !mp.IsMultipleTarget(aiGeneral, true)) ||
                    (plan is IBeCommanderPlan bcp && !bcp.IsBeCommanderTarget(aiGeneral, true)) ||
                    plan.Rate(hmGeneral, aiGeneral) <= 40)
                {
                    continue;
                }

                // 根据不同的计谋条件筛选
                bool isValid = i switch
                {
                    0 or 3 or 4 or 6 or 7 or 9 or 11 or 12 or 13 or 14 => aiGeneral.generalSoldier > 30,
                    1 => aiGeneral.GetCurPhysical() > 1,
                    2 => aiGeneral.generalSoldier > 30 && _aiUnit.unitState != UnitState.Trapped && _aiUnit.unitState != UnitState.Captive,
                    5 or 8 => WarManager.Instance.aiFood > 0,
                    10 => _aiUnit.trappedDay < 2,
                    _ => false
                };

                if (isValid)
                {
                    validPlans.Add(i);
                }
            }

            // 如果没有有效计谋，返回默认值
            if (validPlans.Count == 0) return 255; // 未找到计谋

            // 筛选价值最高的计谋
            byte bestPlanValue = 0; 

            foreach (var planId in validPlans)
            {
                // 根据 planId 获取价值
                byte planValue = planValues.ContainsKey(planId) ? planValues[planId] : (byte)0;

                // 更新最大价值计谋
                bestPlanValue = (byte)Mathf.Max(bestPlanValue, planValue);
            }

            return bestPlanValue;
        }


        // 获取最佳的战斗目标，根据评分方法选择最佳目标
        public bool GetBestBattleTarget(Func<short, int> scoreCalculator)
        {
            if (_aiUnit.moveBonus < 2)
                return false;
            
            tarHmUnit = null;
            int maxScore = 0;

            foreach (var hmUnit in WarManager.Instance.hmUnits)
            {
                if (hmUnit.unitState != UnitState.Captive)
                {
                    if (MapManager.IsAdjacent(_aiUnit.arrayPos, hmUnit.arrayPos))
                    {
                        int score = scoreCalculator(hmUnit.genID);
                        if (score > maxScore)
                        {
                            maxScore = score;
                            tarHmUnit = hmUnit;
                        }
                    }
                }
            }

            if (tarHmUnit != null)
            {
                Debug.Log($"获取到最佳战斗目标{GeneralListCache.GetGeneral(tarHmUnit.genID).generalName}");
                return true;
            }
            return false;
        }
        
        public void AIExecuteBattle()
        {
            if (tarHmUnit != null && _aiUnit.SubMoveBonus(2))
            {
                WarManager.Instance.hmUnitObj = WarManager.GetUnitByPos(tarHmUnit.arrayPos);
                WarManager.Instance.aiUnitObj = WarManager.GetUnitByPos(_aiUnit.arrayPos);
                WarManager.Instance.isBattle = true;
                WarManager.Instance.battleState = BattleState.None;
                Debug.Log($"AI:{_aiUnit.genID}攻击{tarHmUnit.genID}战争状态:{WarManager.Instance.warState}");
            }
            else
            {
                _aiUnit.SetUnitState(UnitState.Idle);
                Debug.LogError("AI未找到目标单位或剩余行动力不足");
            }
        }
        
        
        
        /// <summary>
        /// 攻击目标综合型评估方法
        /// </summary>
        /// <param name="hmGenId"></param>
        /// <returns></returns>
        public int GetBattleScore(short hmGenId)
        {
            General aiGen = _aiGeneral;
            General pyGen = GeneralListCache.GetGeneral(hmGenId);
            int a = aiGen.GetWarValue();
            int p = pyGen.GetWarValue();
            int sa = 1 + aiGen.generalSoldier / 100;
            int sp = 1 + pyGen.generalSoldier / 100;
            int score = (a * sa * sp) / (p * sp);
            return score;
        }

        /// <summary>
        /// 攻击目标智力型评估方法
        /// </summary>
        /// <param name="hmGenId"></param>
        /// <returns></returns>
        public int GetBattleIqScore(short hmGenId)
        {
            General aiGeneral = _aiGeneral;
            General pyGeneral = GeneralListCache.GetGeneral(hmGenId);
            int a = aiGeneral.lead * 3 + aiGeneral.force + aiGeneral.IQ + aiGeneral.level * 20;
            int p = pyGeneral.lead * 3 + pyGeneral.force + pyGeneral.IQ + pyGeneral.level * 20;
            int sa = 1 + aiGeneral.generalSoldier / 100;
            int sp = 1 + pyGeneral.generalSoldier / 100;
            byte iq = pyGeneral.IQ;
            
            int score = a * sa * sp / p * sp;
            if (iq >= 100)
            {
                score *= 2;
            }
            else if (iq >= 90)
            {
                score = score * 7 / 4;
            }
            else if (iq >= 85)
            {
                score = score * 5 / 3;
            }
            else if (iq >= 80)
            {
                score = score * 3 / 2;
            }
            else if (iq >= 60)
            {
                score = score * 4 / 3;
            }
        
            return score;
        }

        
        
        
        
        
        
        // 获取AI要移动到的目标位置
        public static Vector2Int AITargetPosition()
        {
            if (WarManager.Instance.isHmDef) //AI主动进攻玩家
            {
                // 城池位置
                return WarManager.Instance.cityPos;
            }
            // 玩家主将位置
            return WarManager.Instance.hmUnits[0].arrayPos;
        }
        
        // 获取AI的最佳移动位置
        public Vector2Int AIMoveMap()
        {
            // 使用二维数组存储得分，可以避免字典的查找开销
            int[,] scoreMap = new int[19, 32];
            Dictionary<Vector2Int, byte> canMoveTiles = MapManager.GetMovableCellsAStar(_aiUnit); // 每个位置的剩余移动力
            int aiPlanDis = GeneralListCache.GetGeneral(_aiUnit.genID).HasSkill(1, 1) ? 6 : 5; // 特技范围

            Vector2Int targetPos = AITargetPosition(); // AI单位目标坐标
            Debug.Log($"AI:{_aiUnit.genID}行动力:{_aiUnit.moveBonus}在({_aiUnit.arrayPos.x},{_aiUnit.arrayPos.y})目标:({targetPos.x},{targetPos.y})");
            int initialDis = MapManager.ManhattanDistance(_aiUnit.arrayPos, targetPos); // 起点到目标的曼哈顿距离

            // 提前将关键数据存入 HashSet，提高查找效率
            HashSet<Vector2Int> canMoveCells = canMoveTiles.Keys.ToHashSet();
            HashSet<Vector2Int> hmUnitsPos = WarManager.Instance.hmUnits
                .Where(hmUnit => hmUnit.unitState != UnitState.Captive)
                .Select(hmUnit => hmUnit.arrayPos)
                .ToHashSet();

            // 扩展区域计算技能范围
            HashSet<Vector2Int> aiMaxPlanCells = MapManager.ExpandRegion(canMoveCells, aiPlanDis);
            HashSet<Vector2Int> hmInPlanCells = aiMaxPlanCells.Intersect(hmUnitsPos).ToHashSet();

            bool canAttackOrPlan = false;

            // 处理技能范围内的玩家单位
            foreach (var hmPosInRange in hmInPlanCells)
            {
                UnitObj targetUnitObj = MapManager.GetUnitByXY(hmPosInRange); // 获取目标单位

                // 可对玩家施放计谋的 AI 位置
                foreach (var pos in MapManager.GetCellsInRange(hmPosInRange, aiPlanDis).Keys.Intersect(canMoveCells))
                {
                    byte planGrade = GetAIThinkPlan(targetUnitObj, pos);
                    if (planGrade != 255)
                    {
                        scoreMap[pos.y, pos.x] += planGrade * 500;
                        canAttackOrPlan = true;

                        // 多次计谋加分
                        if (canMoveTiles[pos] >= 12)
                        {
                            scoreMap[pos.y, pos.x] += planGrade * 500;
                        }
                    }
                }

                // 可攻击玩家位置
                foreach (var pos in MapManager.GetCanAttackCell(hmPosInRange).Keys.Intersect(canMoveCells))
                {
                    if (canMoveTiles[pos] >= 2 && MapManager.IsAdjacent(hmPosInRange, pos))
                    {
                        int battleScore = GetBattleScore(targetUnitObj.genID);
                        scoreMap[pos.y, pos.x] += battleScore * 100;
                        canAttackOrPlan = true;
                    }
                }
            }

            // 如果没有找到攻击或计谋，评估危险位置
            if (!canAttackOrPlan)
            {
                foreach (var hmPos in hmUnitsPos)
                {
                    if (MapManager.IsInRange(hmPos, _aiUnit.arrayPos, 6))
                    {
                        foreach (var pos in MapManager.GetCellsInRange(hmPos, 6).Keys.Intersect(canMoveCells))
                        {
                            byte dangerScore = GetHmThinkPlan(MapManager.GetUnitByXY(hmPos), pos);
                            if (dangerScore != 255)
                            {
                                scoreMap[pos.y, pos.x] -= dangerScore * 150;
                            }
                        }
                    }
                }
                // 初始化得分地图
                foreach (var pos in canMoveTiles.Keys)
                {
                    //var step = MapManager.ManhattanDistance(pos, _aiUnit.arrayPos); // 移动步数
                    var posDis = initialDis - MapManager.ManhattanDistance(pos, targetPos); // 起点到位置的曼哈顿距离
                    scoreMap[pos.y, pos.x] = posDis * 50; 
                }
                // 处理优先路径
                foreach (var (pos, index) in MapManager.GetPathToTarget(targetPos, _aiUnit, true).Select((value, idx) => (value.Item1, idx)))
                {
                    if (canMoveCells.Contains(pos))
                    {
                        if (MapManager.ManhattanDistance(pos, targetPos) <= initialDis)
                        {
                            scoreMap[pos.y, pos.x] += index * 50;
                        }
                        scoreMap[pos.y, pos.x] += index * 50;
                    }
                }
            }
            
                
            

            // 找到得分最高的点
            Vector2Int bestPos = canMoveTiles.Keys.OrderByDescending(pos => scoreMap[pos.y, pos.x]).First();
            Debug.Log($"AI{_aiUnit.genID}现在({_aiUnit.arrayPos.x},{_aiUnit.arrayPos.y})将移动到({bestPos.x},{bestPos.y})得分{scoreMap[bestPos.y, bestPos.x]}");
            return bestPos;
        }
        
        

        IEnumerator AIGetMovePath(UnitObj unitObj, Vector2Int tarPos)
        {
            if (unitObj.unitState == UnitState.Captive || unitObj.unitState == UnitState.Trapped)
                yield break;
            var movePath = MapManager.GetPathToTarget(tarPos, unitObj.data, false);
            CameraController cameraController = Camera.main?.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.StartAIUnitFollow();
            }
            if (movePath.Count > 0)
            {
                Debug.Log("路径已找到:");
                foreach (var destination in movePath)
                {
                    yield return MoveUnit(unitObj, (destination.Item1, destination.Item2));
                    Debug.Log($"点: {destination.Item1.x}, {destination.Item1.y}, 消耗移动力: {destination.Item2}，剩余移动力: {unitObj.moveBonus}");
                    yield return new WaitForSeconds(0.2f);
                }
                unitObj.SetIsMoved(true);
            }
            else
            {
                Debug.Log($"将军 {GeneralListCache.GetGeneral(unitObj.genID).generalName} 未找到移动路线！");
                unitObj.SetIsMoved(true);
            }

            yield return null;
        }


        

        IEnumerator MoveUnit(UnitObj unitObj, (Vector2Int, byte) destination)
        {
            Vector2Int tarPos = destination.Item1;
            unitObj.SubMoveBonus(destination.Item2);
            MapManager.ChangeXYInDictionary(tarPos, unitObj.arrayPos);
            WarManager.Instance.warMap[unitObj.arrayPos.y, unitObj.arrayPos.x] &= 0x3F;
            WarManager.Instance.warMap[tarPos.y, tarPos.x] |= 0x80;
            unitObj.data.arrayPos = tarPos;
            Vector3 worldPosition = MapManager.TurnToWorldPos(tarPos);
            unitObj.gameObject.transform.position = worldPosition;
            unitObj.GetArmySprite();
            
            if ((WarManager.Instance.warMap[tarPos.y, tarPos.x] & 0x20) != 0)
            {
                unitObj.SetUnitState(UnitState.Trapped);
                unitObj.SetTrappedDay(9);
                yield return UIWar.Instance.uiPlanResult.ShowPlanResult(15, "中计谋奇门遁甲");
                yield break;
            }

            if (WarManager.Instance.isHmDef && unitObj.arrayPos == AITargetPosition())
            {  
                Debug.Log("AI占领目标城池位置");
                yield return UIWar.Instance.uiTips.ShowNoticeTips("敌军占领了城池");
                UIWar.Instance.uiRetreatPanel.MustRetreat(WarManager.Instance.WarOver);
            }
        }
    }
}