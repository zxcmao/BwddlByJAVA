using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using War;

namespace Battle
{
    public class UIBattle : MonoBehaviour
    {
        private static UIBattle _instance;

        public static UIBattle Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<UIBattle>();
                    if (_instance == null)
                    {
                        Debug.LogError("Battle单例为空！请检查此场景!");
                    }
                }

                return _instance;
            }
        }

        private UIBattle() { }

        void Awake()
        {
            if (_instance == null) _instance = this;
            else Destroy(gameObject);
        }


        // 暂停/继续按钮
        [Header("暂停/继续按钮")] public Button pausePlayButton; // 暂停/继续按钮引用
        public Sprite playIcon; // 继续图标
        public Sprite pauseIcon; // 暂停图标

        // 加速和减速按钮
        [Header("加速和减速按钮")] public Button speedUpButton; // 加速按钮引用
        public Button slowDownButton; // 减速按钮引用

        // 当前速度
        [SerializeField] private Text speedDisplay; // 当前速度显示文本
        private float[] speedLevels = { 0.5f, 0.75f, 1f, 1.5f, 2f }; // 支持的速度等级
        private int currentSpeedIndex = 2; // 当前速度在数组中的索引（1倍速为默认值）

        [SerializeField] private RawImage hmHead;
        [SerializeField] private RawImage aiHead;
        [SerializeField] private TextMeshProUGUI hmName;
        [SerializeField] private TextMeshProUGUI aiName;
        [SerializeField] private Text hmGenHpText;
        [SerializeField] private Text hmCavalryText;
        [SerializeField] private Text hmArcherText;
        [SerializeField] private Text hmInfantryText;
        [SerializeField] private Text hmTotalSoldierText;
        [SerializeField] public Text hmTacticText; // 玩家当前战术点文本
        [SerializeField] private Text hmForceText;
        [SerializeField] private Text hmLeadText;
        [SerializeField] private Text aiGenHpText;
        [SerializeField] private Text aiCavalryText;
        [SerializeField] private Text aiArcherText;
        [SerializeField] private Text aiInfantryText;
        [SerializeField] private Text aiTotalSoldierText;
        [SerializeField] private GameObject troopMenu;
        [SerializeField] private TMP_Dropdown dropdown0; // 武将下拉菜单
        [SerializeField] private TMP_Dropdown dropdown1; // 骑兵下拉菜单
        [SerializeField] private TMP_Dropdown dropdown2; // 弓兵下拉菜单
        [SerializeField] private TMP_Dropdown dropdown3; // 步兵下拉菜单
        public UITactic uiTactic; // UI战术面板
        public UITips uiTips; // UI提示通知
        
        // 开始时UI设置
        public void InitUIBattle()
        {
            UpdatePausePlayButton();
            UpdateSpeedButtons();
            pausePlayButton.onClick.AddListener(TogglePausePlay);
            speedUpButton.onClick.AddListener(SpeedUp);
            slowDownButton.onClick.AddListener(SlowDown);

            ShowBattleInfo();
        }
        


        void ShowBattleInfo()
        {
            /*if (uiTactic.IsTacticing(true)) // 玩家战术状态栏
            {
                uiTactic.HandleUITacticUpdate(true);
            }
            if (uiTactic.IsTacticing(false))
            {
                uiTactic.HandleUITacticUpdate(false); // AI战术状态栏
            }*/
            General hmGen = BattleManager.Instance.hmGeneral;
            General aiGen = BattleManager.Instance.aiGeneral;
            hmHead.texture = Resources.Load<Texture2D>($"HeadImage/{BattleManager.Instance.hmGeneral.generalId}");
            aiHead.texture = Resources.Load<Texture2D>($"HeadImage/{BattleManager.Instance.aiGeneral.generalId}");
            hmName.text = hmGen.generalName;
            aiName.text = aiGen.generalName;
            speedDisplay.text = "x" + speedLevels[currentSpeedIndex].ToString("F2");
            hmTacticText.text = BattleManager.Instance.hmTacticPoint.ToString();
            hmForceText.text = hmGen.force.ToString();
            hmLeadText.text = hmGen.lead.ToString();
            hmTotalSoldierText.text = hmGen.generalSoldier.ToString();
            aiTotalSoldierText.text = aiGen.generalSoldier.ToString();
            hmGenHpText.text = hmGen.curPhysical.ToString();
            aiGenHpText.text = aiGen.curPhysical.ToString();
            hmCavalryText.text = BattleManager.Instance.hmCAI_Num[0].ToString();
            aiCavalryText.text = BattleManager.Instance.aiCAI_Num[0].ToString();
            hmArcherText.text = BattleManager.Instance.hmCAI_Num[1].ToString();
            aiArcherText.text = BattleManager.Instance.aiCAI_Num[1].ToString();
            hmInfantryText.text = BattleManager.Instance.hmCAI_Num[2].ToString();
            aiInfantryText.text = BattleManager.Instance.aiCAI_Num[2].ToString();
        }


        /// <summary>
        /// 切换暂停和继续
        /// </summary>
        public void TogglePausePlay()
        {
            if (BattleManager.Instance.isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        private void PauseGame()
        {
            BattleManager.Instance.isPaused = true;
            BattleManager.Instance.Pause();
            Time.timeScale = 0f; // 暂停游戏
            speedUpButton.gameObject.SetActive(false);
            slowDownButton.gameObject.SetActive(false);
            troopMenu.gameObject.SetActive(true);
            dropdown0.onValueChanged.AddListener(OnDropdown0ValueChanged);
            dropdown1.onValueChanged.AddListener(OnDropdown1ValueChanged);
            dropdown2.onValueChanged.AddListener(OnDropdown2ValueChanged);
            dropdown3.onValueChanged.AddListener(OnDropdown3ValueChanged);
            UpdatePausePlayButton();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ResumeGame()
        {
            BattleManager.Instance.isPaused = false;
            BattleManager.Instance.Resume();
            Time.timeScale = speedLevels[currentSpeedIndex]; // 恢复到当前速度
            dropdown0.onValueChanged.RemoveAllListeners();
            dropdown1.onValueChanged.RemoveAllListeners();
            dropdown2.onValueChanged.RemoveAllListeners();
            dropdown3.onValueChanged.RemoveAllListeners();
            speedUpButton.gameObject.SetActive(true);
            slowDownButton.gameObject.SetActive(true);
            troopMenu.gameObject.SetActive(false);
            if (uiTactic.gameObject.activeInHierarchy)
            {
                uiTactic.gameObject.SetActive(false);
            }
            UpdatePausePlayButton();
            Debug.Log("游戏继续");
        }

        /// <summary>
        /// 更新暂停/继续按钮的图标
        /// </summary>
        public void UpdatePausePlayButton()
        {
            if (BattleManager.Instance.isPaused)
            {
                pausePlayButton.image.sprite = playIcon; // 切换为播放图标
            }
            else
            {
                pausePlayButton.image.sprite = pauseIcon; // 切换为暂停图标
            }
        }

        /// <summary>
        /// 加速游戏
        /// </summary>
        public void SpeedUp()
        {
            if (currentSpeedIndex < speedLevels.Length - 1)
            {
                currentSpeedIndex++;
                UpdateGameSpeed();
            }
        }

        /// <summary>
        /// 减速游戏
        /// </summary>
        public void SlowDown()
        {
            if (currentSpeedIndex > 0)
            {
                currentSpeedIndex--;
                UpdateGameSpeed();
            }
        }

        /// <summary>
        /// 更新游戏速度
        /// </summary>
        private void UpdateGameSpeed()
        {
            if (!BattleManager.Instance.isPaused)
            {
                Time.timeScale = speedLevels[currentSpeedIndex];
            }

            UpdateSpeedButtons();

            // 更新文本显示当前倍速
            if (speedDisplay != null)
            {
                speedDisplay.text = "x" + speedLevels[currentSpeedIndex].ToString("F2");
            }
        }

        /// <summary>
        /// 更新加速和减速按钮的状态
        /// </summary>
        public void UpdateSpeedButtons()
        {
            // 禁用超出范围的按钮
            speedUpButton.interactable = currentSpeedIndex < speedLevels.Length - 1;
            slowDownButton.interactable = currentSpeedIndex > 0;
        }

        private void EnableButtons()
        {
            pausePlayButton.interactable = true;
            UpdateSpeedButtons();
        }
        
        private void DisableButtons()
        {
            pausePlayButton.interactable = false;
            speedUpButton.interactable = false;
            slowDownButton.interactable = false;
        }

        // 更新兵力等信息
        public void UpdateBattleInfo(bool isPlayer, short loss, TroopType troopType)
        {
            if (isPlayer)
            {
                switch (troopType)
                {
                    case TroopType.Captain:
                        // 限制体力不能为负数
                        BattleManager.Instance.hmGeneral.SubHp(loss);
                        BattleManager.Instance.hmGeneralHurt += loss; // 记录总伤害，无需检查
                        break;

                    case TroopType.Cavalry:
                        // 限制士兵数和分配值不能为负数
                        BattleManager.Instance.hmGeneral.SubSoldier(loss);
                        BattleManager.Instance.hmCAI_Num[0] = (short)Math.Max(BattleManager.Instance.hmCAI_Num[0] - loss, 0);
                        BattleManager.Instance.hmSoldierLoss += loss; // 记录总损失，无需检查
                        break;

                    case TroopType.Archer:
                        BattleManager.Instance.hmGeneral.SubSoldier(loss);
                        BattleManager.Instance.hmCAI_Num[1] = (short)Math.Max(BattleManager.Instance.hmCAI_Num[1] - loss, 0);
                        BattleManager.Instance.hmSoldierLoss += loss;
                        break;

                    case TroopType.Infantry:
                        BattleManager.Instance.hmGeneral.SubSoldier(loss);
                        BattleManager.Instance.hmCAI_Num[2] = (short)Math.Max(BattleManager.Instance.hmCAI_Num[2] - loss, 0);
                        BattleManager.Instance.hmSoldierLoss += loss;
                        break;
                }
            }
            else
            {
                switch (troopType)
                {
                    case TroopType.Captain:
                        BattleManager.Instance.aiGeneral.SubHp(loss);
                        BattleManager.Instance.aiGeneralHurt += loss;
                        break;
                    case TroopType.Cavalry:
                        BattleManager.Instance.aiGeneral.SubSoldier(loss);
                        BattleManager.Instance.aiCAI_Num[0] = (short)Math.Max(BattleManager.Instance.aiCAI_Num[0] - loss, 0);
                        BattleManager.Instance.aiSoldierLoss += loss;
                        break;

                    case TroopType.Archer:
                        BattleManager.Instance.aiGeneral.SubSoldier(loss);
                        BattleManager.Instance.aiCAI_Num[1] = (short)Math.Max(BattleManager.Instance.aiCAI_Num[1] - loss, 0);
                        BattleManager.Instance.aiSoldierLoss += loss;
                        break;

                    case TroopType.Infantry:
                        BattleManager.Instance.aiGeneral.SubSoldier(loss);
                        BattleManager.Instance.aiCAI_Num[2] = (short)Math.Max(BattleManager.Instance.aiCAI_Num[2] - loss, 0);
                        BattleManager.Instance.aiSoldierLoss += loss;
                        break;
                }
            }

            // 显示战斗信息
            ShowBattleInfo();
        }

        
        public void SetPlayerTroopState(TroopType troopType, TroopState state)
        {
            switch (troopType)
            {
                case TroopType.Captain:
                    dropdown0.value = (int)state;
                    break;
                case TroopType.Cavalry:
                    dropdown1.value = (int)state;
                    break;
                case TroopType.Archer:
                    dropdown2.value = (int)state;
                    break;
                case TroopType.Infantry:
                    dropdown3.value = (int)state;
                    break;
            }
        }
        
        void OnDropdown0ValueChanged(int option) //武将命令切换
        {
            var captain = BattleManager.Instance.hmTroops.OfType<Captain>().FirstOrDefault();
            if (captain == null) return;
            switch (option)
            {
                case 0:
                    captain.data.troopState = TroopState.Forward;
                    break;
                case 1:
                    captain.data.troopState = TroopState.Idle;
                    break;
                case 2:
                    captain.data.troopState = TroopState.BackWard;
                    break;
                case 3:
                    captain.data.troopState = TroopState.Outflank;
                    break;
                case 4:
                    dropdown0.value = 1;
                    PreTactic();
                    break;
            }
        }

        private void PreTactic()
        {
            if (uiTactic.IsTacticing(true))
            {
                uiTips.ShowNoticeTipsWithConfirm("战术正在实施中...", () =>
                {
                    pausePlayButton.onClick.Invoke();
                });
            }
            else
            {
                Debug.Log("开始战术");
                uiTactic.GetTacticsNum(BattleManager.Instance.hmGeneral);
                uiTactic.ShowScrollTacticsPanel();
            }
        }
        
        void OnDropdown1ValueChanged(int option) //骑兵命令切换
        {
            var cavalryTroops = BattleManager.Instance.hmTroops.OfType<Cavalry>();
            foreach (var cavalry in cavalryTroops)
            {
                switch (option)
                {
                    case 0:
                        cavalry.data.troopState = TroopState.Forward;
                        break;
                    case 1:
                        cavalry.data.troopState = TroopState.Idle;
                        break;
                    case 2:
                        cavalry.data.troopState = TroopState.BackWard;
                        break;
                    case 3:
                        cavalry.data.troopState = TroopState.Outflank;
                        break;
                }
            }

        }

        void OnDropdown2ValueChanged(int option) //弓兵命令切换
        {
            var archerTroops = BattleManager.Instance.hmTroops.OfType<Archer>();
            foreach (var archer in archerTroops)
            {
                switch (option)
                {
                    case 0:
                        archer.data.troopState = TroopState.Forward;
                        break;
                    case 1:
                        archer.data.troopState = TroopState.Idle;
                        break;
                    case 2:
                        archer.data.troopState = TroopState.BackWard;
                        break;
                    case 3:
                        archer.data.troopState = TroopState.Outflank;
                        break;
                }
            }

        }

        void OnDropdown3ValueChanged(int option) //步兵命令切换
        {
            var infantryTroops = BattleManager.Instance.hmTroops.OfType<Infantry>();
            foreach (var infantry in infantryTroops)
            {
                switch (option)
                {
                    case 0:
                        infantry.data.troopState = TroopState.Forward;
                        break;
                    case 1:
                        infantry.data.troopState = TroopState.Idle;
                        break;
                    case 2:
                        infantry.data.troopState = TroopState.BackWard;
                        break;
                    case 3:
                        infantry.data.troopState = TroopState.Outflank;
                        break;
                }
            }

        }

        public void NotifyBattleEvent(string text, Action onConfirm)
        {
            DisableButtons();
            uiTips.ShowNoticeTipsWithConfirm(text, () =>
            {
                onConfirm();
                EnableButtons();
            });
        }
    }








}


