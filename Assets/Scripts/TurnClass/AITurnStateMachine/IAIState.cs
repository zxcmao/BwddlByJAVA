using System;
using DataClass;
using UIClass;
using UnityEngine;
using War;

namespace TurnClass.AITurnStateMachine
{
    public enum AITurnState
    {
        AIIdle,
        AIAlliance,
        AIInterior,
        AISearch,
        AIStore,
        AITransport,
        AIConscript,
        AIDefence,
        AIAttack,
        AIvsAI,
        AIVsPlayer,
        AIAnnihilate,
        AISchool,
    }

    public interface IState
    {
        /// <summary>
        /// 进入状态
        /// </summary>
        public void OnEnter();

        /// <summary>
        /// 保持状态
        /// </summary>
        public void OnUpdate();

        /// <summary>
        /// 离开状态
        /// </summary>
        public void OnExit();
    }

    public abstract class BaseTurnState : IState
    {
        protected AITurnStateMachine stateMachine;

        public BaseTurnState(AITurnStateMachine stateMachine)
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
        /// <param name="gameState">提示类型</param>
        /// <param name="onConfirm">玩家确认后的回调</param>
        protected void PauseForPlayerConfirmation(string message, GameState gameState, Action onConfirm)
        {
            UIGlobe uiGlobe = GameObject.Find("Canvas")?.GetComponent<UIGlobe>();
            if (uiGlobe == null)
            {
                Debug.LogError("UIGlobe not found. Cannot display confirmation UI.");
                return;
            }

            UITips tips = uiGlobe.tips;
            if (tips == null)
            {
                Debug.LogError("UITips not found in UIGlobe.");
                return;
            }

            tips.ShowTurnTipsWithConfirm(message, gameState, () =>
            {
                onConfirm?.Invoke(); // 玩家确认后执行回调
            });

            // 通知状态机当前状态暂停
            stateMachine.IsPaused = true;
        }

    }

    public class IdleState : BaseTurnState
    {
        public IdleState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入空闲状态");
            if (stateMachine.CurAITurn.CanAIDoOrder())
            {
                stateMachine.ChangeState(AITurnState.AIAlliance);
            }
            else
            {
                stateMachine.IsFinished = true;
                Debug.Log(stateMachine.CurAITurn.AIName + "结束回合状态");
            }
        }

        public override void OnUpdate()
        {
            
        }
        
        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出空闲状态");
        }
    }

    // 联盟状态
    public class AllianceState : BaseTurnState
    {
        public AllianceState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入联盟状态");
        }

        public override void OnUpdate()
        {
            if (stateMachine.CurAITurn.AiAlliance(out string text))
            {
                // 示例：暂停并等待玩家确认
                PauseForPlayerConfirmation(text, GameState.AITruce, () =>
                {
                    Debug.Log("玩家确认后切换到内政状态");
                    stateMachine.ChangeState(AITurnState.AIInterior);
                });
            }
            else
            {
                stateMachine.ChangeState(AITurnState.AIInterior);
            }
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出联盟状态");
        }
    }

    public class InteriorState : BaseTurnState
    {
        public InteriorState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入内政状态");
        }

        public override void OnUpdate()
        {
            var interior = stateMachine.CurAITurn.AiInterior();
            // 根据随机结果选择内政行动
            switch (interior)
            {
                case 0:
                    stateMachine.CurAITurn.AiTameOrder();
                    stateMachine.ChangeState(AITurnState.AISearch);
                    break;
                case 1:
                    stateMachine.CurAITurn.AiReclaimOrder();
                    stateMachine.ChangeState(AITurnState.AISearch);
                    break;
                case 2:
                    stateMachine.CurAITurn.AiMercantileOrder();
                    stateMachine.ChangeState(AITurnState.AISearch);
                    break;
                case 3:
                case 4:
                    stateMachine.CurAITurn.AiPatrolOrder();
                    stateMachine.ChangeState(AITurnState.AISearch);
                    break;
                case 5:
                case 6:
                    if (stateMachine.CurAITurn.AiJudgeBribe(out short doGenId, out short beGenId, out byte beCityId,
                            out byte doCityId))
                    {
                        // 判断招揽成功
                        if (stateMachine.CurAITurn.AiBribe(doCityId, beCityId, doGenId, beGenId, out string result))
                        {
                            // 示例：暂停并等待玩家确认
                            PauseForPlayerConfirmation(result, GameState.AIBribe, () =>
                            {
                                Debug.Log("玩家确认后切换到搜索状态");
                                stateMachine.ChangeState(AITurnState.AISearch);
                            });
                        }
                        else // 如果招揽失败，执行离间操作
                        {
                            if (stateMachine.CurAITurn.AiAlienate(beCityId, doGenId, beGenId, out string result2))
                            {
                                // 示例：暂停并等待玩家确认
                                PauseForPlayerConfirmation(result2, GameState.AIAlienate, () =>
                                {
                                    Debug.Log("玩家确认后切换到搜索状态");
                                    stateMachine.ChangeState(AITurnState.AISearch);
                                });
                            }
                        }
                    }

                    break;
                case 7:
                case 8:
                    stateMachine.CurAITurn.AiDoSearchEmploy(); // 执行招揽行动
                    stateMachine.ChangeState(AITurnState.AISearch);
                    break;
            }
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出内政状态");
        }
    }

    public class SearchState : BaseTurnState
    {
        public SearchState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入搜索状态");
            stateMachine.CurAITurn.AiSearch();
            stateMachine.ChangeState(AITurnState.AIStore);
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出搜索状态");
        }
    }

    public class StoreState : BaseTurnState
    {
        public StoreState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入粮店状态");
            stateMachine.CurAITurn.AIStore();
            stateMachine.ChangeState(AITurnState.AIConscript);
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出粮店状态");
        }
    }

    public class ConscriptState : BaseTurnState
    {
        public ConscriptState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入征兵状态");
            stateMachine.CurAITurn.AIConscription();
            stateMachine.ChangeState(AITurnState.AIDefence);
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出征兵状态");
        }
    }

    public class DefenceState : BaseTurnState
    {
        public DefenceState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入防御状态");
            stateMachine.CurAITurn.AIDefence();
            stateMachine.ChangeState(AITurnState.AITransport);
        }

        public override void OnUpdate()
        {
           
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出防御状态");
        }
    }

    public class TransportState : BaseTurnState
    {
        public TransportState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入运输状态");
            stateMachine.CurAITurn.AiTransport();
            stateMachine.CurAITurn.AIReward();
            stateMachine.CurAITurn.AIHospital();
            stateMachine.ChangeState(AITurnState.AIAttack);
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出运输状态");
        }
    }
    

    public class AttackState : BaseTurnState
    {
        public AttackState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入攻击状态");
        }

        public override void OnUpdate()
        {
            
            if (stateMachine.CurAITurn.AIAttack())
            {
                byte kind = stateMachine.CurAITurn.AIStartWar(out string result);
                if (kind == 0)
                {
                    // 示例：暂停并等待玩家确认
                    PauseForPlayerConfirmation(result, GameState.AIOccupy, () =>
                    {
                        Debug.Log("玩家确认后切换到学院状态");
                        stateMachine.CurAITurn.AIConscription();
                        stateMachine.CurAITurn.AIHospital();
                        stateMachine.ChangeState(AITurnState.AISchool);
                    });
                }
                else if (kind == 1)
                {
                    // 暂停并等待玩家确认AI攻打AI
                    PauseForPlayerConfirmation(result, GameState.AIvsAI, () =>
                    {
                        Debug.Log("玩家确认后切换到AI相互战争状态");
                        stateMachine.ChangeState(AITurnState.AIvsAI);
                    });

                }
                else if (kind == 2)
                {
                    // 暂停并等待玩家确认AI攻打玩家
                    PauseForPlayerConfirmation(result, GameState.AIvsPlayer, () =>
                    {
                        Debug.Log("玩家确认后切换到搜索状态");
                        UnityEngine.SceneManagement.SceneManager.LoadScene("WarScene");
                        stateMachine.ChangeState(AITurnState.AIVsPlayer);
                    });
                }
            }
            else
            {
                stateMachine.ChangeState(AITurnState.AISchool);
            }
        }

        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出攻击状态");
        }
    }

    public class AIvsState : BaseTurnState
    {
        public AIvsState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入AIvsAI状态");
        }

        public override void OnUpdate()
        {
            // 判断AI是否能够战胜AI
            if (stateMachine.CurAITurn.IsAIWinAI(out string result))
            {
                // 战胜后判断是否消灭防守方AI势力
                PauseForPlayerConfirmation(result, GameState.AIWinAI, () =>
                {
                    Debug.Log("玩家确认后切换到消灭状态");
                    stateMachine.ChangeState(AITurnState.AIAnnihilate);
                });
                
                //yield return IsDestroyed(defCity, generalIds, needFood, needMoney); // 处理战后事宜
            }
            else
            {
                // 战争失败，判断是否能够撤退哪些武将
                stateMachine.CurAITurn.AIRetreat();
                //yield return TurnManager.Instance.uiGlobe.tips.ShowTurnTips(lose, GameState.AILoseAI);
                //IsRetreat(defCity, generalIds, _country.countryKingId, needFood, needMoney);
                // 示例：暂停并等待玩家确认
                PauseForPlayerConfirmation(result, GameState.AILoseAI, () =>
                {
                    Debug.Log("玩家确认后切换到学院状态");
                    stateMachine.ChangeState(AITurnState.AISchool);
                });
            }
        }
        
        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出AI相互攻击状态");
        }
    }

    public class AnnihilateState : BaseTurnState
    {
        public AnnihilateState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }
        
        private byte _annihilateState;
        private string _result;
        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入判断消灭势力状态");
            _annihilateState = stateMachine.CurAITurn.AIAnnihilate(out string result);
            _result = result;
        }

        public override void OnUpdate()
        {
            // 已经胜利，判断AI是否能够歼灭AI
            if (_annihilateState == 2)
            {
                // 暂停并等待玩家确认被攻击方被消灭
                PauseForPlayerConfirmation(_result, GameState.AIFail, () =>
                {
                    Debug.Log("玩家确认后切换到学院状态");
                    stateMachine.CurAITurn.AIConscription();
                    stateMachine.CurAITurn.AIHospital();
                    stateMachine.ChangeState(AITurnState.AISchool);
                });
            }
            else if (_annihilateState == 1)
            {
                // 暂停并等待玩家确认被攻击方继承
                PauseForPlayerConfirmation(_result, GameState.AIInherit, () =>
                {
                    Debug.Log("玩家确认后切换到学院状态");
                    stateMachine.CurAITurn.AIConscription();
                    stateMachine.CurAITurn.AIHospital();
                    stateMachine.ChangeState(AITurnState.AISchool);
                });
            }
            else
            {
                stateMachine.CurAITurn.AIConscription();
                stateMachine.CurAITurn.AIHospital();
                stateMachine.ChangeState(AITurnState.AISchool);
            }
        }
        
        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出消灭状态");
        }
    }
    
    public class VsPlayerState : BaseTurnState
    {
        public VsPlayerState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }
        
        private bool isWarEnd;
        private bool hasChangedState;

        private void WarOver()
        {
            isWarEnd = true;
            GameInfo.PlayingState = GameState.AITurn;
            WarManager.Instance.OnWarOver -= WarOver;
        }
        public override void OnEnter()
        {
            isWarEnd = false;
            hasChangedState = false;

            // 确保事件不会重复注册
            WarManager.Instance.OnWarOver -= WarOver;
            WarManager.Instance.OnWarOver += WarOver;

            Debug.Log(stateMachine.CurAITurn.AIName + "进入对战玩家状态");
        }

        public override void OnUpdate()
        {
            // 判断AI是否能够继续
            if (isWarEnd && !hasChangedState)
            {
                hasChangedState = true; // 防止重复切换状态
                stateMachine.CurAITurn.AIConscription();
                stateMachine.CurAITurn.AIHospital();
                stateMachine.ChangeState(AITurnState.AISchool);
            }
        }
        
        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出对战玩家状态");
        }
    }
    
    public class SchoolState : BaseTurnState
    {
        public SchoolState(AITurnStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "进入学院状态");
            // 判断AI是否能够学习
            stateMachine.CurAITurn.AISchool();
            stateMachine.CurAITurn.AIReward();
            stateMachine.ChangeState(AITurnState.AIIdle);
        }

        public override void OnUpdate()
        {
            
        }
        
        public override void OnExit()
        {
            Debug.Log(stateMachine.CurAITurn.AIName + "退出学院状态");
        }
    }
    
    
}