using System;
using Battle;
using DataClass;
using UnityEngine;

namespace BaseClass
{
    public interface IUpdatableTactic
    {
        byte Duration { get; set; }
    }

    public abstract class Tactic
    {
        // 战术ID
        public abstract byte TacticID { get; }
    
        // 战术名称
        public abstract string Name { get; }

        // 战术说明
        public abstract string Description { get; }

        // 消耗计策点
        public abstract byte Cost { get; }
    
        // 是否可用（默认判断计策点是否足够）
        public virtual bool CanExecute(int currentTacticPoints, bool isPlayer)
        {
            return currentTacticPoints >= Cost;
        }

        // 执行战术（每个子类实现具体逻辑）
        public abstract void Execute(bool isPlayer);

        // 打印战术详情
        public override string ToString()
        {
            return $"{Name}: {Description} (消耗计策点: {Cost})";
        }
    }

    // 子类：挑战
    public class DuelTactic : Tactic
    {
        public override byte TacticID => 1;
        public override string Name => "挑战";
        public override string Description => "向敌将发出单挑邀请.耗费2个计策点";
        public override byte Cost => 2;

        public override bool CanExecute(int currentTacticPoints, bool isPlayer)
        {
            if (currentTacticPoints < Cost) return false;
            General aiGeneral = BattleManager.Instance.aiGeneral;
            General hmGeneral = BattleManager.Instance.hmGeneral;
            General doGen = null;
            General beGen = null;
            if (isPlayer)
            {
                doGen = hmGeneral;
                beGen = aiGeneral;
            }
            else
            {
                doGen = aiGeneral;
                beGen = hmGeneral;
            }
        
            if (beGen.GetCurPhysical() < 25) {
                return false;
            }
     
            if (doGen.HasSkill(5, 3) && beGen.force > 80) {
                return true;
            }
 
     
            int beI = (int)Math.Ceiling((SoloManager.GetAtkDea(
                    beGen, beGen.GetAttackPower(),doGen.GetDefendPower()) *
                SoloManager.GetPercentage(beGen, doGen, false, false) / 100.0));
            int doI = (int)Math.Ceiling((SoloManager.GetAtkDea(
                    doGen, doGen.GetAttackPower(), beGen.GetDefendPower()) *
                SoloManager.GetPercentage(doGen, beGen, false, false) / 100.0));
            int beval = beI * beGen.GetCurPhysical();

            int doval = doI * doGen.GetCurPhysical();
            if (beval > doval + 25 &&
                UnityEngine.Random.Range(0, beval - doval) > 25)
            {
                return true;
            }

            return false;
        }

        public override void Execute(bool isPlayer)
        {
            Console.WriteLine("执行战术: 向敌将发出单挑邀请！");
        }
    }

    // 子类：咒缚
    public class BindTactic : Tactic, IUpdatableTactic
    {
        public override byte TacticID => 2;
        public override string Name => "咒缚";
        public override string Description => "限制敌将行动.耗费5个计策点";
        public override byte Cost => 5;
    
        // 持续时间
        public byte Duration { get; set; } = 2;
        public override bool CanExecute(int currentTacticPoints, bool isPlayer)
        {
            if (currentTacticPoints < Cost) return false;
            General hmGeneral = BattleManager.Instance.hmGeneral;
            General aiGeneral = BattleManager.Instance.aiGeneral;
            General doGen = null;
            General beGen = null;
            if (isPlayer)
            {
                doGen = hmGeneral;
                beGen = aiGeneral;
            }
            else
            {
                doGen = aiGeneral;
                beGen = hmGeneral;
            }
            
        
            int i1 = doGen.IQ - beGen.IQ;
            if (doGen.HasSkill(5, 9))// 特技束缚
            {
                i1 += 30;
            }

            if (i1 < 0)
                return false;
            byte retreatCityId = 0;//TODO UIRetreatPanel.Instance.GetRetreatCityId();
            if (retreatCityId > 0)
            {
                return (UnityEngine.Random.Range(0,100) <= i1);
            }
            return (UnityEngine.Random.Range(0,80) <= i1);
        }
    
        public override void Execute(bool isPlayer)//TODO 诸葛亮增强
        {
            Console.WriteLine("执行战术: 限制敌将行动！");
            if (isPlayer)
            {
                BattleManager.Instance.aiTroops[0].data.troopState = TroopState.Idle;
            }
            else
            {
                BattleManager.Instance.hmTroops[0].data.troopState = TroopState.Idle;
            }
        }
    
        
    }

    // 子类：连弩
    public class CrossbowTactic : Tactic, IUpdatableTactic
    {
        public override byte TacticID => 3;
        public override string Name => "连弩";
        public override string Description => "箭手射程增加.耗费7个计策点";
        public override byte Cost => 7;
    
        // 持续时间
        public byte Duration { get; set; } = 4;
    
        public override void Execute(bool isPlayer)
        {
            Console.WriteLine("执行战术: 箭手射程增加！");
        }   
    }

    // 子类：呐喊
    public class ShoutTactic : Tactic, IUpdatableTactic
    {
        public override byte TacticID => 4;
        public override string Name => "呐喊";
        public override string Description => "全军攻击力增加.耗费8个计策点";
        public override byte Cost => 8;
    
        // 持续时间
        public byte Duration { get; set; } = 4;
        public override void Execute(bool isPlayer)
        {
            Console.WriteLine("执行战术: 全军攻击力增加！");
        }
    }

    // 子类：火矢
    public class FireArrowTactic : Tactic, IUpdatableTactic
    {
        public override byte TacticID => 5;
        public override string Name => "火矢";
        public override string Description => "箭手攻击力大大增加.耗费10个计策点";
        public override byte Cost => 10;
    
        // 持续时间
        public byte Duration { get; set; } = 3;
        public override void Execute(bool isPlayer)
        {
            Console.WriteLine("执行战术: 箭手攻击力大大增加！");
        }   
    }

    // 子类：爆炎
    public class ExplosionTactic : Tactic
    {
        public override byte TacticID => 6;
        public override string Name => "爆炎";
        public override string Description => "我方兵力大于500.利用火器攻击敌军.耗费12个计策点";
        public override byte Cost => 11;

        public override void Execute(bool isPlayer)
        {
            Console.WriteLine("执行战术: 利用火器攻击敌军！");
        }

        // 自定义逻辑：需要我方兵力 > 500
        public new bool CanExecute(int currentTacticPoints, bool isPlayer)
        {
            if (currentTacticPoints >= Cost)
            {
                if (isPlayer)
                {
                    return BattleManager.Instance.hmGeneral.generalSoldier >= 500;
                }
            }
            return false;
        }
    }
}