using System;
using System.Collections.Generic;
using BaseClass;
using Battle;
using TurnClass.AITurnStateMachine;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace War.AIWarStateMachine
{
    public abstract class BaseWarState : IState
    {
        protected WarStateMachine stateMachine;

        protected BaseWarState(WarStateMachine stateMachine)
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
            UITips tips = UIWar.Instance.uiTips;
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
    public class WarThinkState : BaseWarState
    {
        public WarThinkState(WarStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}进入思考状态");
            
            if (stateMachine.CurAIWar._aiUnit.unitState is UnitState.Captive or UnitState.Dead or UnitState.Idle)
            {
                stateMachine.IsFinished = true;
                return;
            }
            
            if (stateMachine.CurAIWar._aiUnit.isCommander && !WarManager.Instance.isHmDef)// 判断是否是AI主将守城
            {
                if (stateMachine.CurAIWar.GetBestPlanTarget(35))
                {
                    stateMachine.ChangeState(AIWarState.Plan);
                }
                else if (stateMachine.CurAIWar.GetBestBattleTarget(stateMachine.CurAIWar.GetBattleScore))
                {
                    stateMachine.ChangeState(AIWarState.Attack);
                }
                else if (stateMachine.CurAIWar.GetBestPlanTarget(0))
                {
                    stateMachine.ChangeState(AIWarState.Plan);
                }
                else if (stateMachine.CurAIWar.IsAICommanderRetreat())
                {
                    stateMachine.ChangeState(AIWarState.Retreat);
                }
                else
                {
                    stateMachine.ChangeState(AIWarState.Idle);
                }
            }
            else if (stateMachine.CurAIWar._aiUnit.isMoved)
            {
                if (stateMachine.CurAIWar.GetBestPlanTarget(35))
                {
                    stateMachine.ChangeState(AIWarState.Plan);
                }
                else if (stateMachine.CurAIWar.GetBestBattleTarget(stateMachine.CurAIWar.GetBattleScore))
                {
                    stateMachine.ChangeState(AIWarState.Attack);
                }
                else if (stateMachine.CurAIWar.GetBestPlanTarget(0))
                {
                    stateMachine.ChangeState(AIWarState.Plan);
                }
                else if (stateMachine.CurAIWar.IsAIUnitRetreat())
                {
                    stateMachine.ChangeState(AIWarState.Retreat);
                }
                else
                {
                    stateMachine.ChangeState(AIWarState.Idle);
                }
            }
            else
            {
                stateMachine.ChangeState(AIWarState.Move);
            }
        } 

        public override void OnUpdate()
        {

        } 

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}退出思考状态");
        } 
    }

    public class WarRetreatState : BaseWarState
    {
        public WarRetreatState(WarStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter() // 进入撤退状态的逻辑实现
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}进入撤退状态");
            
            if (stateMachine.CurAIWar._aiUnit.isCommander)
            {
                PauseForPlayerConfirmation("敌军全军撤退", () =>
                {
                    stateMachine.CurAIWar.SingletonRetreat();
                    AIWar.AIFollowRetreat();
                    stateMachine.IsFinished = true;
                });
            }
            else
            {
                PauseForPlayerConfirmation("敌将逃遁", () =>
                {
                    stateMachine.CurAIWar.SingletonRetreat();
                    stateMachine.IsFinished = true;
                });
            }
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}退出撤退状态");
        }
    }

    public class WarPlanState : BaseWarState
    {
        public WarPlanState(WarStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}进入计谋状态");
            
            stateMachine.CurAIWar.AIExecutePlan();
            PlanForPlayerConfirmation(stateMachine.CurAIWar.bestPlanId, WarManager.Instance.planResult, () =>
            {
                stateMachine.ChangeState(AIWarState.Think);
            });
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}退出计谋状态");
        }
    }

    public class WarMoveState : BaseWarState
    {
        public WarMoveState(WarStateMachine stateMachine) : base(stateMachine) { }

        private Vector2Int _destination;

        private float _elapsedTime;

        private List<(Vector2Int, byte)> _movePath;

        public override void OnEnter() // 进入移动状态的逻辑实现
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}进入移动状态");
            _elapsedTime = 0f;
            _movePath = new List<(Vector2Int, byte)>();
            
            if ((stateMachine.CurAIWar._aiUnit.isCommander && !WarManager.Instance.isHmDef) ||
                stateMachine.CurAIWar._aiUnit.unitState == UnitState.Trapped ||
                stateMachine.CurAIWar._aiUnit.isMoved)
            {
                stateMachine.ChangeState(AIWarState.Think);
            }
            else if (stateMachine.CurAIWar._aiUnit.unitState == UnitState.Captive ||
                     stateMachine.CurAIWar._aiUnit.moveBonus < 2)
            {
                stateMachine.ChangeState(AIWarState.Idle);
            }
            else
            {
                _destination = stateMachine.CurAIWar.AIMoveMap(); // 获取目标位置
                _movePath = MapManager.GetPathToTarget(_destination, stateMachine.CurAIWar._aiUnit, false);
                Debug.Log($"路径包含{_movePath.Count}个格子已找到:");
                
                if (_movePath.Count == 0)
                {
                    Debug.Log(
                        $"{stateMachine.CurAIWar._aiUnit.UnitName}未找到移动路线！");
                    stateMachine.CurAIWar._aiUnit.SetIsMoved(true);
                    stateMachine.ChangeState(AIWarState.Think);
                }
            }
        }

        public override void OnUpdate() // 处于移动状态的逻辑实现
        {
            if (_movePath.Count > 0)
            {
                // 增加已过去的时间
                _elapsedTime += Time.deltaTime;

                // 检查是否达到了设定的时间间隔
                if (_elapsedTime >= 0.2f)
                {
                    // 重置计时器
                    _elapsedTime = 0f;

                    // 执行你的方法
                    Vector2Int tarPos = _movePath[0].Item1;
                    stateMachine.CurAIWar._aiUnit.SubMoveBonus(_movePath[0].Item2);
                    MapManager.ChangeXYInDictionary(tarPos, stateMachine.CurAIWar._aiUnit.arrayPos);
                    WarManager.Instance.warMap[stateMachine.CurAIWar._aiUnit.arrayPos.y,
                        stateMachine.CurAIWar._aiUnit.arrayPos.x] &= 0x3F;
                    WarManager.Instance.warMap[tarPos.y, tarPos.x] |= 0x80;
                    stateMachine.CurAIWar._aiUnit.arrayPos = tarPos;
                    MapManager.MoveAIUnit(tarPos, stateMachine.CurAIWar._aiUnit.arrayPos);
                    
                    Debug.Log($"点:{_movePath[0].Item1.x}, {_movePath[0].Item1.y},消耗移动力:{_movePath[0].Item2}," +
                              $"剩余移动力: {stateMachine.CurAIWar._aiUnit.moveBonus}");
                    _movePath.RemoveAt(0);

                    if ((WarManager.Instance.warMap[tarPos.y, tarPos.x] & 0x20) != 0)
                    {
                        stateMachine.CurAIWar._aiUnit.SetUnitState(UnitState.Trapped);
                        stateMachine.CurAIWar._aiUnit.SetTrappedDay(9);
                        PlanForPlayerConfirmation(15, "中计谋奇门遁甲", () =>
                        {
                            stateMachine.ChangeState(AIWarState.Think);
                        });
                    }

                    if (WarManager.Instance.isHmDef && stateMachine.CurAIWar._aiUnit.arrayPos ==
                        WarManager.Instance.cityPos)
                    {
                        Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}占领目标城池位置");
                        PauseForPlayerConfirmation("敌军占领了城池", () =>
                        {
                            stateMachine.IsFinished = true;
                        });
                    }
                }
            }
            else
            {
                stateMachine.CurAIWar._aiUnit.SetIsMoved(true);
                stateMachine.ChangeState(AIWarState.Think);
            }
        }

        public override void OnExit()
        {
            /*CameraController cameraController = Camera.main?.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.StopAIUnitFollow();
            }*/

            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}退出移动状态");
        }
    }

    public class WarAttackState : BaseWarState
    {
        public WarAttackState(WarStateMachine stateMachine) : base(stateMachine) { }
        
        private bool _isBattleStart;
        private bool _isBattleOver;
        /// <summary>
        /// 场景加载完成时回调
        /// </summary>
        private void OnBattleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "PreBattleScene")
            {
                // 确保 BattleManager 已经加载
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.OnBattleOver -= BattleEnd;
                    BattleManager.Instance.OnBattleOver += BattleEnd;
                }
                else
                {
                    Debug.LogError("BattleManager单例未找到！");
                }

                // 移除事件监听，避免多次触发
                //SceneManager.sceneLoaded -= OnBattleSceneLoaded;
            }

            if (_isBattleOver && scene.name == "WarScene")
            {
                stateMachine.ChangeState(AIWarState.Think);
                // 移除事件监听，避免多次触发
                SceneManager.sceneLoaded -= OnBattleSceneLoaded;
            }
        }
        
        private void BattleEnd()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}发起的战斗结束");
            _isBattleOver = true;
            
            BattleManager.Instance.OnBattleOver -= BattleEnd;
        }

        public override void OnEnter() // 进入攻击状态的逻辑实现
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}进入攻击状态");
            _isBattleStart = false;
            _isBattleOver = false;
            if (stateMachine.CurAIWar._aiUnit.unitState != UnitState.Idle ||
                stateMachine.CurAIWar._aiUnit.unitState != UnitState.Captive ||
                stateMachine.CurAIWar._aiUnit.moveBonus >= 2)
            {
                if (stateMachine.CurAIWar._aiUnit.isMoved)
                {
                    if (stateMachine.CurAIWar.GetBestBattleTarget(stateMachine.CurAIWar.GetBattleScore))
                    {
                        stateMachine.CurAIWar.AIExecuteBattle();
                    }
                }
                else
                {
                    if (stateMachine.CurAIWar.GetBestBattleTarget(stateMachine.CurAIWar.GetBattleIqScore))
                    {
                        stateMachine.CurAIWar.AIExecuteBattle();
                    }
                }
            }
            else
            {
                stateMachine.ChangeState(AIWarState.Idle);
            }
        }

        public override void OnUpdate() // 处于攻击状态的逻辑实现
        {
            if (!_isBattleStart)
            {
                PauseForPlayerConfirmation("敌军攻打我军", () =>
                {
                    _isBattleStart = true;
                    // 监听场景加载完成事件
                    SceneManager.sceneLoaded += OnBattleSceneLoaded;

                    // 加载新场景
                    SceneManager.LoadScene("PreBattleScene");
                });
            }
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}退出攻击状态");
        }
    }
    
    public class WarIdleState : BaseWarState
    {
        public WarIdleState(WarStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter() // 进入待机状态的逻辑实现
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}进入待机状态");
            
            if (stateMachine.CurAIWar._aiUnit.unitState != UnitState.Captive ||
                stateMachine.CurAIWar._aiUnit.unitState != UnitState.Trapped ||
                stateMachine.CurAIWar._aiUnit.moveBonus < 2)
            {
                stateMachine.CurAIWar._aiUnit.SetUnitState(UnitState.Idle);
            }
            stateMachine.IsFinished = true;
        }

        public override void OnUpdate() // 处于待机状态的逻辑实现
        {
            
        }

        public override void OnExit()
        {
            Debug.Log($"{stateMachine.CurAIWar._aiUnit.UnitName}退出待机状态");
        }
    }
}