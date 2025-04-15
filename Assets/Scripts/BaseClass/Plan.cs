using System;
using System.Collections.Generic;
using System.Linq;
using DataClass;
using UnityEngine;
using Random = UnityEngine.Random;
using War;

namespace BaseClass
{
    // 要求我军所处地形的计谋
    public interface IDoTerrain
    {
        // 使用自身地形限制
        byte[] DoTerrain { get; }
        
        // 判断自身是否适用地形
        bool IsSuitable(byte curTerrain)
        {
            return Array.Exists(DoTerrain, t => t == (curTerrain & 0x1F));
        }
    }
    
    // 针对敌军主将的计谋
    public interface IBeCommanderPlan
    {
        public bool IsBeCommanderTarget(General beGeneral, bool isPlayer)
        {
            if (isPlayer)
            {
                return beGeneral.generalId == WarManager.Instance.aiUnits[0].genID;
            }
            return beGeneral.generalId == WarManager.Instance.hmUnits[0].genID;
        }
    }
    
    // 针对多个目标的计谋
    public interface IMultiplePlan
    {
        byte FriendMinimum { get; }
        public bool IsMultipleTarget(General beGeneral, bool isPlayer)
        {
            return MapManager.GetAdjacentFriendNum(beGeneral,isPlayer) >= FriendMinimum;
        }
    }
    
    //[CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public abstract class Plan
    {
        // 计谋ID
        public abstract byte PlanID { get; }
    
        // 计谋名称
        public abstract string Name { get; }

        // 计谋机动力消耗
        protected abstract byte Cost { get; }

        // 是否够发动计谋
        public byte UseCost(General doGeneral)
        {
            byte baseCost = Cost;
            // 特技百出减少两点计谋机动力消耗
            if (doGeneral.HasSkill(1, 2))
            {
                baseCost -= 2;
            }
            return baseCost;
        }
        
        // 计谋说明文本
        public string Description => TextLibrary.PlanExplain[PlanID];
       
        // 施放距离
        protected abstract byte Distance { get; }
        
        // 判断是否增加计谋距离
        public virtual byte InDistance(General doGeneral)
        {
            // 特技鬼谋增加计谋距离
            return doGeneral.HasSkill(1,1) ? (byte)(Distance + 1) : Distance;
        }
        
        // 适用目标地形限制
        protected abstract byte[] BeTerrain { get; }
        
        // 判断目标是否适用地形
        public bool IsApplicable(byte curTerrain)
        {
            return Array.Exists(BeTerrain, t => t == (curTerrain & 0x1F));
        }
        
        // 计谋字典
        private static Dictionary<byte, Plan> _planDictionary = new()
        {
            { 0 , new HuoJiPlan() },
            { 1 , new XianJingPlan() },
            { 2 , new XuBingPlan() },
            { 3 , new YaoJiPlan() },
            { 4 , new LuanShuiPlan() },
            { 5 , new JieLiangPlan() },
            { 6 , new GongShaPlan() },
            { 7 , new WeiJiPlan() },
            { 8 , new ShaoLiangPlan() },
            { 9 , new LuoShiPlan() },
            { 10, new LianHuanPlan() },
            { 11, new FuBingPlan() },
            { 12, new ShuiGongPlan() },
            { 13, new JianYuPlan() },
            { 14, new JieHuoPlan() },
            { 15, new QiMenPlan() }
        };
        
        // 计谋获取方法
        public static Plan GetPlan(byte planID)
        {
            return _planDictionary[planID];
        }
        
        public bool IsExecute(General doGen, General beGen, bool isPlayer)
        {
            WarManager.Instance.planResult = "计谋施展失败";
            if (beGen.HasSkill(1,6))// 特技反计
            {
                if (Random.Range(0, 71) <= Rate(beGen, doGen))
                {
                    WarManager.Instance.planResult = $"{beGen.generalName}将计就计,反而" + Result(beGen, doGen,!isPlayer);
                    return true;
                }
            }
            if (Random.Range(0, 71) <= Rate(doGen, beGen))
            {
                WarManager.Instance.planResult = Result(doGen, beGen, isPlayer);
                return true;
            }
            return false;

        }
        
        // 判断计谋成功率
        public abstract byte Rate(General doGen, General beGen);

        // 增强施放成功率
        protected void RateEnhance(General doGen, ref int rate)
        {
            if (doGen.HasSkill(1, 5)) // 特技神算
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
        }
        
        // 削弱施放成功率
        protected void RateWeaken(General beGen, ref int rate)
        {
            if (beGen.HasSkill(1,0)) // 特技冷静
            {
                rate -= rate / 2;
            }
            else if (beGen.HasSkill(1, 5)) // 特技神算
            {
                if (beGen.generalName == "诸葛亮")
                {
                    rate -= Mathf.Max(rate / 4, 15);
                }
                else
                {
                    rate -= rate / 5;
                }
            }
        }
        
        // 执行计谋结果（每个子类实现具体逻辑）
        public abstract string Result(General doGen, General beGen, bool isPlayer);
        
        // 计谋伤害调整
        protected void ResultEnhance(General doGeneral, ref int loss)
        {   // 特技军师深谋1.5倍伤害
            if (doGeneral.HasSkill(1, 3)) loss += loss / 2;
        }
        
        /// 计算周围围单位的士兵损失
        protected int CalculateSurroundingLoss(General beGen, int surroundLoss, bool isPlayer)
        {
            int totalSurroundLoss = 0;

            // 获取相关单位数据
            UnitObj unitObj = isPlayer
                ? WarManager.Instance.aiUnits.FirstOrDefault(t => t.genID == beGen.generalId)
                : WarManager.Instance.hmUnits.FirstOrDefault(t => t.genID == beGen.generalId);

            if (unitObj == null) return 0;

            List<UnitObj> units = isPlayer ? WarManager.Instance.aiUnits : WarManager.Instance.hmUnits;

            foreach (var adjacentUnit in units)
            {
                // 检查单位是否被困，跳过不符合条件的单位
                if (adjacentUnit.unitState == UnitState.Captive) continue;

                // 判断单位是否在被计谋单位的相邻格
                if (MapManager.IsAdjacent(unitObj.arrayPos, adjacentUnit.arrayPos))
                {
                    General nearbyGeneral = GeneralListCache.GetGeneral(adjacentUnit.genID);
                    if (nearbyGeneral == null) continue;

                    // 处理围杀士兵损失
                    nearbyGeneral.SubSoldier(surroundLoss);
                    totalSurroundLoss = Mathf.Min(totalSurroundLoss + surroundLoss, 
                        totalSurroundLoss + nearbyGeneral.generalSoldier);
                }
            }

            return totalSurroundLoss;
        }

        protected void AddPlanExp(General doGeneral, int exp)
        {
            doGeneral.Addexperience(exp / 3);
            doGeneral.AddIqExp((byte)(exp / 100));
        }
    }


    public class HuoJiPlan : Plan
    {
        public override byte PlanID => 0;
        public override string Name => "火攻";
        protected override byte Cost => 6;
        protected override byte Distance => 5;
        protected override byte[] BeTerrain => new byte[] {1,2,3,4,10,11};

        public override byte Rate(General doGen, General beGen)
        {
            // 基本属性
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead2 = beGen.lead;

            // 初始成功率计算
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 40 - (100 - iq1) * 0.1f - lead2 * 0.05f);

            // IQ差异调整成功率
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f);

            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (doGen.HasSkill(1, 4)) // 特技火攻
            {
                rate += doGen.generalName == "周瑜" ? rate / 2 : rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }


        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            // 提取基础属性
            byte iq1 = doGen.IQ; // 攻击方智力
            byte iq2 = beGen.IQ; // 防御方智力

            // 初始化伤害值
            int loss = 250; // 基础伤害

            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 2 + 2); // 攻击方智力高，增加伤害
            }
            else
            {
                loss += Random.Range(iqDifference * 2 - 2, 0); // 防御方智力高，降低伤害
            }

            // 技能加成
            if (doGen.HasSkill(1, 4)) // 特技火攻
            {
                loss *= 2; // 双倍伤害
            }
            else if (doGen.HasSkill(1, 3)) // 特技军师
            {
                loss += loss / 2; // 1.5倍伤害
            }

            // 确保伤害不会超过现有士兵数量
            loss = Mathf.Min(loss, beGen.generalSoldier);

            // 减少防御方士兵数量
            beGen.SubSoldier(loss);
            
            // 添加计谋经验
            AddPlanExp(doGen, loss);
            // 返回结果
            return $"{beGen.generalName}中了{doGen.generalName}之计！折损：{loss} 兵马";
        }
    }


    public class XianJingPlan : Plan
    {
        public override byte PlanID => 1;
        public override string Name => "陷阱";
        protected override byte Cost => 5;
        protected override byte Distance => 5; 
        protected override byte[] BeTerrain => new byte[] { 10,12 };

        public override byte Rate(General doGen, General beGen)
        {
            // 基本属性
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead2 = beGen.lead;

            // 初始成功率计算
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 40 - (100f - iq1) * 0.1f - lead2 * 0.05f);

            // IQ差异调整成功率
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f);

            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            RateWeaken(beGen, ref rate);

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            int hurt = Random.Range(10,21); // 计算体力减少的值
            ResultEnhance(doGen, ref hurt);// 军师增强
            if (beGen.GetCurPhysical() - 1 < hurt)
                hurt = beGen.GetCurPhysical() - 1; // 防止体力减少到负值
            beGen.SubHp((byte)hurt); // 减少体力
            // 添加计谋经验
            AddPlanExp(doGen, hurt * 15);
            return $"{beGen.generalName}中了{doGen.generalName}之计！受到：{hurt}伤害";
        }
    }

    public class XuBingPlan : Plan
    {
        public override byte PlanID => 2;
        public override string Name => "虚兵";
        protected override byte Cost => 4;
        protected override byte Distance  => 5;
        protected override byte[] BeTerrain => new byte[] {10,12};

        public override byte Rate(General doGen, General beGen)
        {
            // 基本属性
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte force2 = beGen.force;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70- force2 * 0.08f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 45 - (100f - iq1) * 0.1f - force2 * 0.08f);
            if (iq1 < iq2)
                 r2 -= (int)((iq2 - iq1) * 0.6f);
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            RateEnhance(doGen, ref rate);

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            byte days = 1; // 初始困兵天数
            if (doGen.HasSkill(1, 3))// 特技军师
                days = 2; // 技能加成困兵时间
            if (isPlayer)
            {
                UnitObj unitObj = WarManager.Instance.aiUnits.FirstOrDefault(t => t.genID == beGen.generalId);
                if (unitObj != null)
                {
                    unitObj.SetUnitState(UnitState.Trapped); // 设置AI单位的被困状态
                    unitObj.SetTrappedDay(days); // 更新AI单位的被困天数
                }
                
            }
            else
            {
                UnitObj unitObj = WarManager.Instance.hmUnits.FirstOrDefault(t => t.genID == beGen.generalId);
                if (unitObj != null)
                {
                    unitObj.SetUnitState(UnitState.Trapped); // 设置玩家单位的被困状态
                    unitObj.SetTrappedDay(days); // 更新AI单位的被困天数
                }
            }
            
            // 添加计谋经验
            AddPlanExp(doGen, days * 100);
            return $"{beGen.generalName}中了{doGen.generalName}之计！动弹不得"; 
        }
    }


    public class YaoJiPlan : Plan
    {
        public override byte PlanID => 3;
        public override string Name => "要击";
        protected override byte Cost => 6;
        protected override byte Distance => 5;
        protected override byte[] BeTerrain => new byte[] {10,12};

        public override byte Rate(General doGen, General beGen)
        {
            XuBingPlan xuBingPlan = new XuBingPlan();
            return xuBingPlan.Rate(doGen, beGen);
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            // 提取基础属性
            byte iq1 = doGen.IQ; // 攻击方智力
            byte iq2 = beGen.IQ; // 防御方智力

            // 初始化伤害值
            int loss = 250; // 基础伤害

            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 2 + 2); // 攻击方智力高，增加伤害
            }
            else
            {
                loss += Random.Range(iqDifference * 2 - 2, 0); // 防御方智力高，降低伤害
            }

            // 技能加成
            ResultEnhance(doGen, ref loss);

            // 确保伤害不会超过现有士兵数量
            loss = Mathf.Min(loss, beGen.generalSoldier);

            // 减少防御方士兵数量
            beGen.SubSoldier(loss);
            
            // 添加计谋经验
            AddPlanExp(doGen, loss);

            // 返回结果
            return $"{beGen.generalName}中了{doGen.generalName}之计！折损：{loss} 兵马";
        }
    }


    public class LuanShuiPlan : Plan
    {
        public override byte PlanID => 4;
        public override string Name => "乱水";
        protected override byte Cost => 6;
        protected override byte Distance => 5;
        protected override byte[] BeTerrain => new byte[] {9};
        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead2 = beGen.lead;
            byte type = beGen.army[2];
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 40 - (100f - iq1) * 0.1f - lead2 * 0.05f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f);
            if (type > 0)
            {
                r1 -= type * 3;
                r2 -= type * 2;
            }
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            RateEnhance(doGen, ref rate);
            
            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            YaoJiPlan plan = new YaoJiPlan();
            return plan.Result(doGen, beGen, isPlayer);
        }
    }

    public class JieLiangPlan : Plan, IBeCommanderPlan
    {
        public override byte PlanID => 5;
        public override string Name => "劫粮";
        protected override byte Cost => 8;
        protected override byte Distance => 2;
        protected override byte[] BeTerrain => new byte[] {1,2,3,4,5,6,7,10,11,12};
        public override byte InDistance(General doGeneral)
        {
            byte baseDistance = Distance;
            
            if (doGeneral.HasSkill(1,1))// 特技鬼谋
            {
                baseDistance += 1;
            }
            else if(doGeneral.HasSkill(1,8))// 特技袭粮
            {
                baseDistance += 1;
            }
            return baseDistance;
        }

        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead2 = beGen.lead;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 60 - (100 - iq1) * 0.1f - lead2 * 0.05f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f);
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (beGen.HasSkill(1, 8))// 特技袭粮
            {
                rate += rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            // 获取当前操作的粮食信息
            ref short targetFood = ref (isPlayer ? ref WarManager.Instance.aiFood : ref WarManager.Instance.hmFood);
            ref short gainFood = ref (isPlayer ? ref WarManager.Instance.hmFood : ref WarManager.Instance.aiFood);

            // 计算粮食损失
            int loss = targetFood / 4; 
            ResultEnhance(doGen, ref loss); // 应用特技军师加成
            if (loss < 200) loss = targetFood; // 若损失过少，设为全部损失

            // 减少目标的粮食
            targetFood -= (short)loss;

            // 增加当前阵营的粮食，确保不超过 30000 上限
            gainFood = (short)Mathf.Min(gainFood + loss, 30000);
            
            // 添加计谋经验
            AddPlanExp(doGen, loss);

            // 返回结果字符串
            return $"{beGen.generalName}中了{doGen.generalName}之计！被劫：{loss}军粮";
        }
    }


    public class GongShaPlan : Plan, IMultiplePlan
    {//TODO
        public override byte PlanID => 6;
        public override string Name => "共杀";
        protected override byte Cost => 8;
        protected override byte Distance => 2;
        protected override byte[] BeTerrain => new byte[] { 12 };

        public byte FriendMinimum => 1;

        public override byte InDistance(General doGeneral)
        {
            byte baseDistance = Distance;
            if (doGeneral.HasSkill(1,1))// 特技鬼谋
            {
                baseDistance += 1;
            }
            else if(doGeneral.HasSkill(1,9))// 特技内讧
            {
                baseDistance += 1;
            }
            return baseDistance;
        }

        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte moral2 = beGen.moral;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - moral2 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 60 - (100 - iq1) * 0.1f - moral2 * 0.05f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f);
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);
            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (beGen.HasSkill(1, 9))// 特技内讧
            {
                rate += rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            int loss = 450;
            int surroundLoss = 0;
            int totalLoss = 0;
            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 3 + 2); // 攻击方智力高，增加伤害
            }
            else
            {
                loss += Random.Range(iqDifference * 3 - 2, 0); // 防御方智力高，降低伤害
            }
            
            ResultEnhance(doGen, ref loss);
            
            surroundLoss = loss;
            loss = Mathf.Min(loss, beGen.generalSoldier);
            beGen.SubSoldier(loss);
            totalLoss += loss;
            totalLoss += CalculateSurroundingLoss(beGen, surroundLoss, isPlayer);
            
            // 添加计谋经验
            AddPlanExp(doGen, totalLoss);
            return $"{beGen.generalName}等中了{doGen.generalName}之计！总共折损：{totalLoss} 兵马";
        }
    }


    public class WeiJiPlan : Plan
    {
        public override byte PlanID => 7;
        public override string Name => "伪击转杀";
        protected override byte Cost => 10;
        protected override byte Distance => 1;
        protected override byte[] BeTerrain => new byte[] { 8 };
        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte moral2 = beGen.moral;
            int r1, r2, rate;
            if (beGen.generalSoldier > 1800 + Random.Range(0,300))
            {
                r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - moral2 * 0.05f);
                r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 60 - (100 - iq1) * 0.1f - moral2 * 0.05f);
                if (iq1 < iq2)
                    r2 -= (int)((iq2 - iq1) * 0.6);
                // 取较低值作为基础成功率
                rate = Mathf.Min(r1, r2);

                // 调整成功率：施放者技能
                RateEnhance(doGen, ref rate);

                // 调整成功率：受害者技能
                RateEnhance(beGen, ref rate);

                // 限制成功率范围为 0 到 99
                rate = Mathf.Clamp(rate, 0, 99);

                return (byte)rate;
            }
            return 0;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            YaoJiPlan plan = new YaoJiPlan();
            return plan.Result(doGen, beGen, isPlayer);
        }
    }


    public class ShaoLiangPlan : Plan, IBeCommanderPlan
    {
        public override byte PlanID => 8;
        public override string Name => "烧粮";
        protected override byte Cost => 10;
        protected override byte Distance => 2;
        protected override byte[] BeTerrain => new byte[] { 10,11 };
        public override byte InDistance(General doGeneral)
        {
            JieLiangPlan jieLiangPlan = new JieLiangPlan();
            return jieLiangPlan.InDistance(doGeneral);
        }

        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead2 = beGen.lead;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 70 - (100 - iq1) * 0.1f - lead2 * 0.05f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f); 
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (beGen.HasSkill(1, 8))// 特技袭粮
            {
                rate += rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            int loss;
            if (isPlayer)
            {
                loss = WarManager.Instance.aiFood / 2;
                ResultEnhance(doGen, ref loss);
                if (loss < 500)
                    loss = WarManager.Instance.aiFood;
                WarManager.Instance.aiFood = (short)(WarManager.Instance.aiFood - loss);
            }
            else
            {
                loss = WarManager.Instance.hmFood / 2;
                ResultEnhance(doGen, ref loss);
                if (loss < 500)
                    loss = WarManager.Instance.hmFood;
                WarManager.Instance.hmFood = (short)(WarManager.Instance.hmFood - loss);
            }
            
            // 添加计谋经验
            AddPlanExp(doGen, loss);
            return $"{beGen.generalName}中了{doGen.generalName}之计！烧毁了：{loss}军粮";
        }
    }


    public class LuoShiPlan : Plan, IDoTerrain
    {
        public override byte PlanID => 9;
        public override string Name => "落石";
        protected override byte Cost  => 9;
        protected override byte Distance => 2;
        public byte[] DoTerrain => new byte[] { 8, 12 };
        protected override byte[] BeTerrain => new byte[] { 1,2,3,4,5,6,7,9,10,11,12 };

        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead1 = doGen.lead;
            byte lead2 = beGen.lead;
            byte type = beGen.army[1];
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f + lead1 * 0.05f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 40 - (100 - iq1) * 0.1f - lead2 * 0.05f + lead1 * 0.05f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6f); 
            if (type == 3) 
            {
                r1 -= 6;
                r2 -= 3;
            }  
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);

            // 调整成功率：施放者技能
            RateEnhance(doGen, ref rate);

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            int loss = 350;
            int hurt = 0;
            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 3 + 2); // 攻击方智力高，增加伤害
            }
            else
            {
                loss += Random.Range(iqDifference * 3 - 2, 0); // 防御方智力高，降低伤害
            }
            ResultEnhance(doGen, ref loss);
            loss = Mathf.Min(loss, beGen.generalSoldier);
            beGen.SubSoldier(loss);
            hurt = Random.Range(10,21);
            ResultEnhance(doGen, ref hurt);
            if (beGen.GetCurPhysical() - 1 < hurt)
                hurt = beGen.GetCurPhysical() - 1;
            beGen.SubHp((byte)hurt);
            
            // 添加计谋经验
            AddPlanExp(doGen, loss + hurt * 15);
            return $"{beGen.generalName}中了{doGen.generalName}之计！受到{hurt}伤害,折损{loss} 兵马";
        }
    }


    public class LianHuanPlan : Plan, IMultiplePlan
    {
        public override byte PlanID => 10;
        public override string Name => "连环";
        protected override byte Cost => 10;
        protected override byte Distance => 5;
        protected override byte[] BeTerrain => new byte[] { 9 };
        public byte FriendMinimum => 1;

        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead1 = doGen.lead;
            byte lead2 = beGen.lead;
            byte type = beGen.army[2];
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.33f + 80 - lead2 * 0.1f + lead1 * 0.1f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 50 - (100 - iq1) * 0.1f - lead2 * 0.1f + lead1 * 0.1f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6); 
            if (type == 3) 
            {
                r1 -= 6;
                r2 -= 3;
            } 
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);
            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (beGen.HasSkill(1, 9))// 特技内讧
            {
                rate += rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            // 获取相关单位和信息
            UnitObj unitObj = isPlayer
                ? WarManager.Instance.aiUnits.FirstOrDefault(t => t.genID == doGen.generalId)
                : WarManager.Instance.hmUnits.FirstOrDefault(t => t.genID == doGen.generalId);

            if (unitObj == null) return $"未找到{beGen.generalName}等";

            // 设置主单位被困
            List<UnitObj> units = isPlayer ? WarManager.Instance.aiUnits : WarManager.Instance.hmUnits;

            unitObj.SetUnitState(UnitState.Trapped);
            unitObj.SetTrappedDay(3);

            // 遍历相邻单位并设置被困状态
            foreach (var adjacentUnit in units)
            {
                if (adjacentUnit.unitState == UnitState.Captive) continue; // 跳过无效单位

                // 检查单位是否在主单位的相邻格
                if (MapManager.IsAdjacent(unitObj.arrayPos, adjacentUnit.arrayPos))
                {
                    adjacentUnit.SetUnitState(UnitState.Trapped);
                    adjacentUnit.SetTrappedDay(3);
                }
            }
            // 添加计谋经验
            AddPlanExp(doGen, 900);

            return $"{beGen.generalName}等中了{doGen.generalName}之计, 被困三天";
        }

    }


    public class FuBingPlan : Plan, IDoTerrain
    {
        public override byte PlanID => 11;
        public override string Name => "伏兵";
        protected override byte Cost => 10;
        protected override byte Distance => 5;
        public byte[] DoTerrain => new byte[] { 11,12 };
        protected override byte[] BeTerrain => new byte[] { 1,2,3,4,5,6,7,9,10,11,12 };

        public override byte InDistance(General doGeneral)
        {
            byte baseDistance = Distance;
            if (doGeneral.HasSkill(1,1)) baseDistance += 1;//特技鬼谋
            else if (doGeneral.HasSkill(1,7)) baseDistance += 2;//特技待伏
            return baseDistance;
        }

        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead1 = doGen.lead;
            byte lead2 = beGen.lead;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f + lead1 * 0.08f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 60 - (100 - iq1) * 0.1f - lead2 * 0.05f + lead1 * 0.08f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6); 
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);
            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (beGen.HasSkill(1, 7))// 特技待伏
            {
                rate += rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;

            // 初始伤害值
            int loss = 600;

            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 3 + 2); // 攻击方智力高
            }
            else
            {
                loss += Random.Range(iqDifference * 4 - 2, 0); // 防御方智力高
            }

            // 应用特技加成
            ResultEnhance(doGen, ref loss);

            // 确保损失不会超过现有士兵数
            loss = Mathf.Min(loss, beGen.generalSoldier);

            // 更新士兵数
            beGen.SubSoldier(loss);

            // 更新单位被困状态
            // 获取相关单位和被困状态数组
            UnitObj unitObj = isPlayer
                ? WarManager.Instance.aiUnits.FirstOrDefault(t => t.genID == beGen.generalId)
                : WarManager.Instance.hmUnits.FirstOrDefault(t => t.genID == beGen.generalId);

            if (unitObj == null) return "未找到敌军目标";

            // 更新被困状态
            unitObj.SetUnitState(UnitState.Trapped);
            unitObj.SetTrappedDay(1);
            
            // 添加计谋经验
            AddPlanExp(doGen, loss + 300);

            return $"{beGen.generalName}中了{doGen.generalName}之计！折损：{loss} 兵马且动弹不得";
        }
    }


    public class ShuiGongPlan : Plan, IMultiplePlan
    {
        public override byte PlanID => 12;
        public override string Name => "水攻";
        protected override byte Cost => 12;
        protected override byte Distance => 3;
        protected override byte[] BeTerrain => new byte[] { 9 };
        public byte FriendMinimum => 1;
        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead1 = doGen.lead;
            byte lead2 = beGen.lead;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f + lead1 * 0.08f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 60 - (100 - iq1) * 0.1f - lead2 * 0.05f + lead1 * 0.08f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6); 
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);
            // 调整成功率：施放者技能
            if (doGen.HasSkill(1, 5)) // 特技神算
            {
                rate += doGen.generalName == "诸葛亮" ? Mathf.Max(rate / 4, 15) : rate / 5;
            }
            else if (beGen.HasSkill(1, 9))// 特技内讧
            {
                rate += rate / 3;
            }

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            GongShaPlan plan = new GongShaPlan();
            return plan.Result(doGen, beGen, isPlayer);
        }
    }


    public class JianYuPlan : Plan, IDoTerrain
    {
        public override byte PlanID => 13;
        public override string Name => "箭雨";
        protected override byte Cost => 10;
        protected override byte Distance => 2;
        public byte[] DoTerrain => new byte[] { 8,12 };
        protected override byte[] BeTerrain => new byte[] { 1,2,3,4,5,6,7,9,10,11,12 };
        public override byte Rate(General doGen, General beGen)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            byte lead1 = doGen.lead;
            byte lead2 = beGen.lead;
            int r1 = (int)(iq1 * 0.3f - iq2 * 0.2f + 70 - lead2 * 0.05f + lead1 * 0.08f);
            int r2 = (int)((iq1 * iq1) * (100 - iq2 * 0.9f) * 100 / (iq1 * iq1 + iq2 * iq2) / 60f - (100 - iq1) * 0.1f - lead2 * 0.05f + lead1 * 0.08f);
            if (iq1 < iq2)
                r2 -= (int)((iq2 - iq1) * 0.6); 
            // 取较低值作为基础成功率
            int rate = Mathf.Min(r1, r2);
            // 调整成功率：施放者技能
            RateEnhance(doGen, ref rate);

            // 调整成功率：受害者技能
            RateWeaken(beGen, ref rate);

            // 限制成功率范围为 0 到 99
            rate = Mathf.Clamp(rate, 0, 99);

            return (byte)rate;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            int loss = 400;
            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 3 + 2); // 攻击方智力高
            }
            else
            {
                loss += Random.Range(iqDifference * 3 - 2, 0); // 防御方智力高
            }
            ResultEnhance(doGen, ref loss);
            loss = Mathf.Min(loss, beGen.generalSoldier);
            beGen.SubSoldier(loss);
            // 添加计谋经验
            AddPlanExp(doGen, loss);
            return $"{beGen.generalName}中了{doGen.generalName}之计！折损：{loss} 兵马 ";
        }
    }


    public class JieHuoPlan : Plan, IMultiplePlan
    {
        public override byte PlanID => 14;
        public override string Name => "劫火";

        protected override byte Cost => 12;

        protected override byte Distance => 3;

        protected override byte[] BeTerrain => new byte[] { 10, 11 };

        public byte FriendMinimum => 2;
        public override byte Rate(General doGen, General beGen)
        {
            JianYuPlan plan = new JianYuPlan();
            return plan.Rate(doGen, beGen);
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            byte iq1 = doGen.IQ;
            byte iq2 = beGen.IQ;
            int loss = 550;

            // 根据智力差异调整伤害
            int iqDifference = iq1 - iq2;
            if (iqDifference >= 0)
            {
                loss += Random.Range(0, iqDifference * 3 + 2); // 攻击方智力高
            }
            else
            {
                loss += Random.Range(iqDifference * 3 - 2, 0); // 防御方智力高
            }

            // 技能加成
            if (doGen.HasSkill(1, 3)) // 特技军师
            {
                loss += loss / 2;
            }
            else if (doGen.HasSkill(1, 4)) // 特技火攻
            {
                loss *= 2;
            }
            // 计算围杀损失
            int surroundLoss = loss;
            
            // 减少被攻击将领的士兵数
            loss = Mathf.Min(loss, beGen.generalSoldier);
            beGen.SubSoldier(loss);
            
            int totalLoss = loss;

            // 获取相关单位和信息
            UnitObj unitObj = isPlayer
                ? WarManager.Instance.aiUnits.FirstOrDefault(t => t.genID == beGen.generalId)
                : WarManager.Instance.hmUnits.FirstOrDefault(t => t.genID == beGen.generalId);

            if (unitObj == null) return String.Empty;

            // 动态选择数据源
            List<UnitObj> units = isPlayer ? WarManager.Instance.aiUnits : WarManager.Instance.hmUnits;

            foreach (var adjacentUnit in units)
            {
                // 跳过被困或无效单位
                if (adjacentUnit.unitState == UnitState.Captive) continue;

                // 判断是否在相邻格
                if (!MapManager.IsAdjacent(unitObj.arrayPos, adjacentUnit.arrayPos)) continue;

                // 获取相邻将领
                General nearbyGen = GeneralListCache.GetGeneral(adjacentUnit.genID);
                if (nearbyGen == null) continue;

                // 计算损失
                nearbyGen.SubSoldier(surroundLoss);
                totalLoss = Mathf.Min(totalLoss + surroundLoss, totalLoss + nearbyGen.generalSoldier);
            }
            // 添加计谋经验
            AddPlanExp(doGen, totalLoss);
            return $"{beGen.generalName}等中了{doGen.generalName}之计！总共折损：{totalLoss} 兵马";
        }
    }

    public class QiMenPlan : Plan
    {
        public override byte PlanID => 15;
        public override string Name => "奇门遁甲";
        protected override byte Cost => 10;
        protected override byte Distance => 9;
        protected override byte[] BeTerrain => new byte[] {10,11,12};

        public override byte Rate(General doGen, General beGen)
        {
            return 99;
        }

        public override string Result(General doGen, General beGen, bool isPlayer)
        {
            General humanGeneral = GeneralListCache.GetGeneral(beGen.generalId);
            if (humanGeneral == null) return String.Empty; // 检查将军是否存在

            // 减少士兵数，计算随机损失
            int loss = 100 - Random.Range(0, 150 - humanGeneral.lead);
            humanGeneral.SubSoldier(loss);
            
            // 添加计谋经验
            AddPlanExp(doGen, 6 * loss);
            return $"{doGen.generalName}已布下奇门遁甲之阵";
        }
        

    }
}