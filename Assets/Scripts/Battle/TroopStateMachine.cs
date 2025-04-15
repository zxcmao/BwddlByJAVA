using System.Collections.Generic;
using BaseClass;
using DataClass;
using UnityEngine;
using War;
using War.AIWarStateMachine;

namespace Battle
{
    public class TroopStateMachine
    {
        /// <summary>
        /// 管理所有状态的字典容器
        /// </summary>
        private Dictionary<TroopState, BaseTroopState> _stateDic = new Dictionary<TroopState, BaseTroopState>();

        private BaseTroopState _nowState; // 当前状态

        public Troop CurTroop; // 管理的所需数据对象

        public bool IsPaused { get; set; } // 是否处于暂停状态
        public bool IsFinished { get; set; } // 是否已结束

        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Init(Troop aiUnit)
        {
            CurTroop = aiUnit;
            IsPaused = false; // 状态机默认不处于暂停状态
            IsFinished = false; // 状态机默认未结束
            AddState(TroopState.Forward, new TroopForwardState(this));
            AddState(TroopState.Outflank, new TroopOutflankState(this));
            AddState(TroopState.BackWard, new TroopBackWardState(this));
            AddState(TroopState.Idle, new TroopIdleState(this));
            ChangeState(TroopState.Forward); 
        }

        /// <summary>
        /// 添加 AI 状态
        /// </summary>
        public void AddState(TroopState state, BaseTroopState newState)
        {
            if (_stateDic.TryAdd(state, newState))
            {
                Debug.Log(state + "状态添加成功！");
            }
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(TroopState state)
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

