using System.Collections.Generic;
using BaseClass;
using DataClass;
using UIClass;
using UnityEngine;
using UnityEngine.Serialization;
using War;

namespace Battle
{
    public class UITactic : MonoBehaviour
    {
        public GameObject hmTacticState;// 玩家状态栏
        public GameObject aiTacticState;// AI状态栏
        [SerializeField] private SelectHorizontalScroll horizontalScroll;
        [SerializeField] private GameObject tacticalPanel;
    
        // 私有字段
        public byte tacticsNum = 6;
        
        private Tactic _hmTactic 
        {
            get => BattleManager.Instance.hmTactic;
            set => BattleManager.Instance.hmTactic = value;
        }

        private Tactic _aiTactic
        {
            get => BattleManager.Instance.aiTactic;
            set=>BattleManager.Instance.aiTactic=value;
        }
        
        private ushort hmTacticTermination
        {
            get => BattleManager.Instance.hmTacticTermination;
            set => BattleManager.Instance.hmTacticTermination = value;
        }

        private ushort aiTacticTermination
        {
            get => BattleManager.Instance.aiTacticTermination;
            set => BattleManager.Instance.aiTacticTermination = value;
        }
        // 战术映射字典
        private static readonly Dictionary<byte, Tactic> TacticDictionary = new()
        {
            { 1, new DuelTactic() },
            { 2, new BindTactic() },
            { 3, new CrossbowTactic() },
            { 4, new ShoutTactic() },
            { 5, new FireArrowTactic() },
            { 6, new ExplosionTactic() }
        };
    
        public void ShowScrollTacticsPanel()
        {
            //使用列表时要在列表初始化之前设置选项信息
            if (horizontalScroll != null)
            {
                string[] names = new string[tacticsNum];
                Sprite[] sprites = new Sprite[tacticsNum];
                string[] descriptions = new string[tacticsNum];
                for (byte i = 0; i < tacticsNum; i++)
                {
                    names[i] = TextLibrary.TacticNames[i];
                    sprites[i] = Resources.Load<Sprite>("UI/MultipleOption");
                    descriptions[i] = TextLibrary.TacticExplain[i];
                }
                horizontalScroll.SetItemsInfo(names, sprites, descriptions);
                tacticalPanel.SetActive(true);
                Debug.Log("打开战术面板");
                horizontalScroll.SelectAction += (index) =>
                {
                    horizontalScroll.isSelected = false;
                    SelectTactic((byte)(index + 1));
                    Debug.Log(index + 1);
                };
            }
            else
            {
                Debug.Log("horizontalScroll为空");
            }
        }

        private void SelectTactic(byte tacticIndex)
        {
            Tactic tactic = GetTactic(tacticIndex);
        
            if (tactic == null)
            {
                Debug.LogError("战术为空，无法应用！");
                return;
            }
            
            // 假设当前计策点，且为玩家执行
            if(tactic.CanExecute(BattleManager.Instance.hmTacticPoint, true))
            {
                tactic.Execute(true);
                ApplyTactic(tactic.TacticID, true);
                Debug.Log($"玩家执行了{tactic.Name}");
                tacticalPanel.SetActive(false);
                UIBattle.Instance.NotifyBattleEvent(tactic.Name+"实施成功", () =>
                {
                    if (tactic.TacticID == 1) // 决斗
                    {
                        BattleManager.Instance.ExecuteSolo();
                    }
                    else if (tactic is ExplosionTactic) // 爆炎
                    {
                        Troop troop = BattleManager.Instance.hmTroops[0];
                        if (troop is Captain)
                        {
                            Captain captain = troop as Captain;
                            captain.Explosion();
                        }
                        UIBattle.Instance.pausePlayButton.onClick.Invoke();
                    }
                    else // 其他战术
                    {
                        UIBattle.Instance.pausePlayButton.onClick.Invoke();
                    }
                });
            }
            else
            {
                BattleManager.Instance.hmTacticPoint -= tactic.Cost;
                UIBattle.Instance.hmTacticText.text = BattleManager.Instance.hmTacticPoint.ToString();
                tacticalPanel.SetActive(false);
                UIBattle.Instance.NotifyBattleEvent("执行战术失败", () =>
                {
                    UIBattle.Instance.pausePlayButton.onClick.Invoke();
                }); 
            }
        }
    
        // 获取武将可使用的战术种类
        public void GetTacticsNum(General general)
        {
            byte need = general.IQ;
            tacticsNum = need switch
            {
                < 16 => 1,
                < 39 => 2,
                < 54 => 3,
                < 62 => 4,
                < 77 => 5,
                < 93 => 6,
                _ => 1
            };
        }
        
        // 获取战术对象
        public static Tactic GetTactic(byte tacticID)
        {
            return TacticDictionary.TryGetValue(tacticID, out var tactic) ? tactic : null;
        }

        // 判断战术状态
        public bool IsTacticing(bool isPlayer)
        {
            return isPlayer ? _hmTactic != null : _aiTactic != null;
        }

        // 应用战术效果（通用）
        public void ApplyTactic(byte tacticID, bool isPlayer)
        {
            var tactic = GetTactic(tacticID);
            if (tactic == null)
            {
                Debug.LogError($"{(isPlayer ? "玩家" : "AI")}战术为空，无法应用！");
                return;
            }

            if (isPlayer)
            {
                _hmTactic = tactic;
                if (tactic is IUpdatableTactic updatableTactic)
                {
                    hmTacticTermination = (ushort)(BattleManager.Instance.doneTroopsCount +
                                                    updatableTactic.Duration * BattleManager.Instance.totalTroopCount);
                }
                BattleManager.Instance.hmTacticPoint -= tactic.Cost;
                UIBattle.Instance.hmTacticText.text = BattleManager.Instance.hmTacticPoint.ToString();
                HandleUITacticUpdate(true);
                Debug.Log($"玩家战术 {tactic.Name} 应用成功");
            }
            else
            {
                _aiTactic = tactic;
                if (tactic is IUpdatableTactic updatableTactic)
                {
                    aiTacticTermination = (ushort)(BattleManager.Instance.doneTroopsCount +
                                                    updatableTactic.Duration * BattleManager.Instance.totalTroopCount);
                }
                BattleManager.Instance.aiTacticPoint -= tactic.Cost;
                HandleUITacticUpdate(false);
                Debug.Log($"AI战术 {tactic.Name} 应用成功");
            }
        }

        // 更新战术效果（通用）
        public void UpdateTacticalState(bool isPlayer)
        {
            var currentTactic = isPlayer ? _hmTactic : _aiTactic;

            if (currentTactic == null || currentTactic is not IUpdatableTactic) return;
            
            // 清除战术
            if (isPlayer)
            {
                if (hmTacticTermination == BattleManager.Instance.doneTroopsCount)
                {
                    _hmTactic = null;
                    hmTacticTermination = 0;
                    hmTacticState.SetActive(false);
                    Debug.Log($"玩家战术 {currentTactic.Name} 的效果结束");
                }
            }
            else
            {
                if (aiTacticTermination == BattleManager.Instance.doneTroopsCount)
                {
                    _aiTactic = null;
                    aiTacticTermination = 0;
                    aiTacticState.SetActive(false);
                    Debug.Log($"AI战术 {currentTactic.Name} 的效果结束");
                }
            }
        }

        // 检查当前是否有特定战术
        public bool CheckTacticalState(byte tacticID, bool isPlayer)
        {
            var currentTactic = isPlayer ? _hmTactic : _aiTactic;
            return currentTactic != null && tacticID == currentTactic.TacticID;
        }

        // UI更新效果逻辑
        public void HandleUITacticUpdate(bool isPlayer)
        {
            var tactic = isPlayer ? _hmTactic : _aiTactic;
            if (tactic is IUpdatableTactic)
            {
                var uiTacticState = isPlayer ? hmTacticState : aiTacticState;
                uiTacticState.SetActive(true);
                uiTacticState.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = tactic.Name;
            }
        
            if (tactic is ExplosionTactic)
            {
                Troop troop = isPlayer ? BattleManager.Instance.hmTroops[0] : BattleManager.Instance.aiTroops[0];
                if (troop is Captain captain)
                {
                    captain.Explosion();
                }
            }
        }
    }
}

