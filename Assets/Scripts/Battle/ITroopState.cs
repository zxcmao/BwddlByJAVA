using System;
using System.Collections.Generic;
using BaseClass;
using TurnClass.AITurnStateMachine;
using UIClass;
using UnityEngine;
using War;

namespace Battle
{
    public abstract class BaseTroopState : IState
    {
        protected TroopStateMachine stateMachine;

        protected BaseTroopState(TroopStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }

        /// <summary>
        /// 暂停当前状态，等待玩家确认后继续
        /// </summary>
        /// <param name="message">提示消息</param>
        /// <param name="onConfirm">玩家确认后的回调</param>
        protected void PauseForPlayerConfirmation(string message, Action onConfirm)
        {
            UITips tips = UIBattle.Instance.uiTips;
            if (tips == null)
            {
                Debug.LogError("UITips组件未找到");
                return;
            }

            tips.ShowNoticeTipsWithConfirm(message, () =>
            {
                onConfirm?.Invoke(); // 玩家确认后执行回调
            });

            // 通知状态机当前状态暂停
            stateMachine.IsPaused = true;
        }

        protected void PlanForPlayerConfirmation(byte planID, string msg, Action onConfirm)
        {
            UITips tips = UIWar.Instance.uiPlanResult;
            if (tips == null)
            {
                Debug.LogError("UITips组件未找到");
                return;
            }

            tips.ShowPlanResultWithConfirm(planID, msg, () =>
            {
                onConfirm?.Invoke(); // 玩家确认后执行回调
            });

            // 通知状态机当前状态暂停
            stateMachine.IsPaused = true;
        }
        
        
        
    }

    /// <summary>
    /// 思考状态类，处理思考逻辑
    /// </summary>
    public class TroopThinkState : BaseTroopState
    {
        public TroopThinkState(TroopStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log($"{stateMachine.CurTroop.gameObject.name}进入思考状态");
        }

        public override void OnUpdate()
        {

        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurTroop.gameObject.name}退出思考状态");
        }
    }

    public class TroopForwardState : BaseTroopState
    {
        private float actionTimer = 0f; // 用于延迟模拟等待
        private float actionInterval = 0.5f; // 行为间隔
        
        public TroopForwardState(TroopStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            actionTimer = 0f;
            
            Debug.Log($"{stateMachine.CurTroop.name} 进入 Forward 状态");
        }

        public override void OnUpdate()
        {
            var troop = stateMachine.CurTroop;
            if (troop.data.actBonus <= 0)
            {
                stateMachine.IsFinished = true; // 本轮行动结束
                return;
            }

            actionTimer += Time.deltaTime;
            if (actionTimer < actionInterval)
                return;

            actionTimer = 0f; // 重置计时器

            byte[] directions = troop.isPlayer ? new byte[] { 3, 0, 1, 2 } : new byte[] { 2, 0, 1, 3 };
            Troop target = troop.PerformTargetCheck(directions);

            if (target != null)
            {
                troop.Attack(target);
            }
            else
            {
                byte dir = troop.MoveForward();
                if (dir != 255)
                {
                    troop.MoveByDirection(dir);
                }
                else
                {
                    List<byte> allDirs = troop.GetAllMovableDirection();
                    troop.MoveByDirection(troop.GetRandomDirection(allDirs));
                }
            }

            troop.data.actBonus--; // 扣除行动力
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurTroop.name} 退出 Forward 状态");
        }
    }
    
    public class TroopOutflankState : BaseTroopState
    {
        private float actionTimer = 0f;
        private float actionInterval = 0.5f;

        public TroopOutflankState(TroopStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            actionTimer = 0f;
            Debug.Log($"{stateMachine.CurTroop.name} 进入 Outflank 状态");
        }

        public override void OnUpdate()
        {
            if (stateMachine.CurTroop.data.actBonus <= 0)
            {
                stateMachine.IsFinished = true;
                return;
            }

            actionTimer += Time.deltaTime;
            if (actionTimer < actionInterval)
                return;

            actionTimer = 0f;
            var troop = stateMachine.CurTroop;

            byte dir = troop.MoveToEnemyGeneral(); // 尝试靠近主将
            if (dir != 255)
            {
                troop.MoveByDirection(dir);
            }
            else
            {
                byte[] directions = troop.isPlayer ? new byte[] { 3 } : new byte[] { 2 };
                Troop target = troop.PerformTargetCheck(directions);
                if (target != null)
                {
                    troop.Attack(target);
                }
            }

            troop.data.actBonus--;
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurTroop.name} 退出 Outflank 状态");
        }
    }
    
    public class TroopBackWardState : BaseTroopState
    {
        private float actionTimer = 0f;
        private float actionInterval = 0.5f;

        public TroopBackWardState(TroopStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            actionTimer = 0f;
            Debug.Log($"{stateMachine.CurTroop.name} 进入 BackWard 状态");
        }

        public override void OnUpdate()
        {
            var troop = stateMachine.CurTroop;

            if (troop.IsRetreat())
            {
                Debug.Log($"{troop.name} 撤退成功，结束状态机");
                stateMachine.IsFinished = true;
                return;
            }

            if (troop.data.actBonus <= 0)
            {
                stateMachine.IsFinished = true;
                return;
            }

            actionTimer += Time.deltaTime;
            if (actionTimer < actionInterval)
                return;

            actionTimer = 0f;

            byte dir = troop.MoveBackWard(); // 尝试后退
            if (dir != 255)
            {
                troop.MoveByDirection(dir);
            }
            else
            {
                byte[] directions = troop.isPlayer ? new byte[] { 2 } : new byte[] { 3 };
                Troop target = troop.PerformTargetCheck(directions);
                if (target != null)
                {
                    troop.Attack(target);
                }
            }

            troop.data.actBonus--;
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurTroop.name} 退出 BackWard 状态");
        }
    }

    public class TroopIdleState : BaseTroopState
    {
        private float actionTimer = 0f;
        private float actionInterval = 0.5f;

        public TroopIdleState(TroopStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            actionTimer = 0f;
            Debug.Log($"{stateMachine.CurTroop.name} 进入 Idle 状态");
        }

        public override void OnUpdate()
        {
            var troop = stateMachine.CurTroop;

            if (troop.data.actBonus <= 0)
            {
                stateMachine.IsFinished = true;
                return;
            }

            actionTimer += Time.deltaTime;
            if (actionTimer < actionInterval)
                return;

            actionTimer = 0f;

            byte[] directions = troop.isPlayer ? new byte[] { 3, 0, 1, 2 } : new byte[] { 2, 0, 1, 3 };
            Troop target = troop.PerformTargetCheck(directions);
            if (target != null)
            {
                troop.Attack(target);
            }

            troop.data.actBonus--;
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurTroop.name} 退出 Idle 状态");
        }
    }


}
