using System.Collections.Generic;
using UnityEngine;

namespace TurnClass.AITurnStateMachine
{
    public class AITurnStateMachine
    {
        /// <summary>
        /// 管理所有状态的字典容器
        /// </summary>
        private Dictionary<AITurnState, BaseTurnState> _stateDic = new Dictionary<AITurnState, BaseTurnState>();

        private IState _nowState; // 当前状态
    
        public AITurn CurAITurn; // 管理的所需数据对象
        
        public bool IsPaused { get; set; } // 是否处于暂停状态
        public bool IsFinished { get; set; } // 是否已结束

        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Init(byte countryId)
        {
            CurAITurn = new AITurn(countryId);
            IsPaused = false; // 状态机默认不处于暂停状态
            IsFinished = false; // 状态机默认未结束
            AddState(AITurnState.AIAlliance, new AllianceState(this));
            AddState(AITurnState.AIInterior, new InteriorState(this));
            AddState(AITurnState.AISearch, new SearchState(this));
            AddState(AITurnState.AIStore, new StoreState(this));
            AddState(AITurnState.AIConscript, new ConscriptState(this));
            AddState(AITurnState.AIIdle, new IdleState(this));
            AddState(AITurnState.AIDefence, new DefenceState(this));
            AddState(AITurnState.AITransport, new TransportState(this));
            AddState(AITurnState.AIAttack, new AttackState(this));
            AddState(AITurnState.AIAnnihilate, new AnnihilateState(this));
            AddState(AITurnState.AISchool, new SchoolState(this));
            AddState(AITurnState.AIvsAI, new AIvsState(this));
            AddState(AITurnState.AIVsPlayer, new VsPlayerState(this));
            ChangeState(AITurnState.AIIdle); // 默认状态为 AIIdle
        }

        public void SetTurn(byte countryId)
        {
            CurAITurn = new AITurn(countryId);
            IsPaused = false; // 状态机默认不处于暂停状态
            IsFinished = false; // 状态机默认未结束
            ChangeState(AITurnState.AIIdle); // 默认状态为 AIIdle
        }
        
        /// <summary>
        /// 添加 AI 状态
        /// </summary>
        public void AddState(AITurnState state, BaseTurnState newState)
        {
            if (_stateDic.TryAdd(state, newState)) 
            {
                Debug.Log(state+"状态添加成功！");
            }
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(AITurnState state)
        {
            if (!_stateDic.ContainsKey(state)) // 若状态不存在，则报错
            {
                Debug.Log("当前状态不为空！");
                return;
            }

            if (_nowState != null) // 若当前状态不为空，则先退出当前状态
            {
                _nowState.OnExit();
            }
            _nowState = _stateDic[state]; // 切换状态
            IsPaused = false; // 恢复状态机的运行（防止暂停状态未重置的情况）
            _nowState.OnEnter(); // 进入新状态
        }

        /// <summary>
        /// 更新当前状态
        /// </summary>
        public void UpdateState()
        {
            // 如果状态机处于暂停状态，跳过执行
            if (IsPaused || IsFinished)
                return;
            _nowState?.OnUpdate();
        }
            
    }
}