using System;
using BaseClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace War
{
    public class UIWar :MonoBehaviour
    {
        private static UIWar _instance;
        public static UIWar Instance => _instance;
        public static event Action<bool> OnInfoPanelShow; 
    
        public GameObject warInfo; // 战争中的底部文本
        public GameObject warMenu; // 战争中的按钮菜单
        public MapManager mapManager;
        public UITips uiTips;// 战争中的消息面板
        public UIPlanPanel uiPlanPanel;// 战争中的计策面板
        public UITips uiPlanResult;// 战争中的计策结果消息面板
        public UIIntelligencePanel uiIntelligencePanel;// 战争中的情报面板
        public UIRetreatPanel uiRetreatPanel;// 战争中的撤退面板
        [SerializeField] private UIStore uiStore;
        [SerializeField] private UISmithy uiSmithy;
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI moveBonus;
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private TextMeshProUGUI goldText;
    
        [SerializeField] private Button moveButton;
        [SerializeField] private Button planButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button intelligenceButton;
        [SerializeField] private Button retreatButton;
        [SerializeField] private Button idleButton;
    

        private UIWar() { }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this; // 确保在 Awake 中完成初始化
            }
            else
            {
                Destroy(gameObject); // 防止多个实例
            }
        }

        private void Start()
        {
            // 添加按钮点击事件
            moveButton.onClick.AddListener(OnClickMoveButton);
            planButton.onClick.AddListener(OnClickPlanButton);
            attackButton.onClick.AddListener(OnClickAttackButton);
            intelligenceButton.onClick.AddListener(OnClickIntelligenceButton);
            retreatButton.onClick.AddListener(OnClickRetreatButton);
            idleButton.onClick.AddListener(OnClickIdleButton);
            /*if (PlayerPrefs.GetFloat("bgmVolume") > 0)
            {
                SoundManager.Instance.PlayBGM("3");
            }*/
        }

        void Update()
        {
            dayText.text = "第" + WarManager.Instance.day + "天\n天气:阴";

            if(WarManager.Instance.warState == WarState.PlayerTurn)
            {
                if (WarManager.Instance.hmUnitObj != null)
                {
                    moveBonus.text = $"机动力:{WarManager.Instance.hmUnitObj.moveBonus}";
                }
                foodText.text = $"粮食:{WarManager.Instance.hmFood}";
                goldText.text = $"金钱:{WarManager.Instance.hmGold}";
            }
            else if (WarManager.Instance.warState == WarState.AITurn)
            {
                if (WarManager.Instance.aiUnitObj != null)
                {
                    moveBonus.text = $"机动力:{WarManager.Instance.aiUnitObj.moveBonus}";
                }
                foodText.text = $"粮食:{WarManager.Instance.aiFood}";
                goldText.text = $"金钱:{WarManager.Instance.aiGold}";
            }
        }
        
        //TODO
        public void NotifyWarEvent(string text)
        {
            HideUnderMenu();
            StartCoroutine(uiTips.ShowNoticeTips(text));
        }

        private void HideUnderMenu()
        {
            warInfo.gameObject.SetActive(false);
            warMenu.gameObject.SetActive(false);
        }
    
        public void DisplayWarMenu()
        {
            if (WarManager.Instance.hmUnitObj != null)
            {
                warInfo.gameObject.SetActive(false);
                warMenu.gameObject.SetActive(true);
            }
            else
            {
                WarTipText("请选择军队执行命令");
            }
        }
    
        public void WarTipText(string text)
        {
            warMenu.SetActive (false);
            warInfo.gameObject.SetActive(true);
            TextMeshProUGUI textMesh = warInfo.GetComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.color = Color.black;
            warInfo.GetComponent<Button>().enabled = false;
        }
        
        public void SetupCancelButton(string tipText, UnityAction cancelAction)
        {
            WarTipText(tipText);
            Button button = warInfo.GetComponent<Button>();
            button.enabled = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(cancelAction);
        }

        public void ResetCancelButton()
        {
            if (WarManager.Instance.warState == WarState.AITurn) return;

            mapManager.ClearAllMarkers();
            Button button = warInfo.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.enabled = false;
            DisplayWarMenu();
        }

        // 点击移动按钮
        private void OnClickMoveButton()
        {
            // 如果已经移动过，则不显示可移动范围
            if (WarManager.Instance.hmUnitObj.isMoved)
            {
                if (WarManager.Instance.hmUnitObj.InVillage())
                {
                    WhetherGoVillage(WarManager.Instance.hmUnitObj.Terrain);
                }
                SetupCancelButton("武将已经移动过,点此取消", ResetCancelButton);
                return; // 直接返回
            }
            
            // 获取将军的当前位置并计算可移动范围
            if (true)
            {
                SetupCancelButton("请选择行军到何处,点此取消", ResetCancelButton);
                Debug.Log("当前位置:" + WarManager.Instance.hmUnitObj.arrayPos);
                // 确保 tilePos 是将军当前的单元格位置
                // 显示可移动范围
                mapManager.DisplayNavigation(MapManager.GetMovableCellsAStar(WarManager.Instance.hmUnitObj.data)); 
            }
        }

        private void OnClickCancelMove()
        {
            if (WarManager.Instance.warState == WarState.AITurn)
            {
                return;
            }
            mapManager.ClearAllMarkers();
            Button button = warInfo.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.enabled = false;
            DisplayWarMenu();
            Debug.Log("取消移动");
        }
        
        //TODO
        public void WhetherGoVillage(byte terrain) //是否进入村庄提示
        {
            HideUnderMenu();
            uiTips.OnOptionSelected += HandleInVillage;
            switch (terrain)
            {
                case 5: //武器店
                    uiTips.ShowOptionalTips("是否进入武器店？");
                    break;
                case 6: //粮食店
                    uiTips.ShowOptionalTips("是否进入粮食店？");
                    break;
                case 7: //医馆
                    uiTips.ShowOptionalTips("是否进入医馆？");
                    break;
            }
        }
    
        // 点击计策按钮
        private void OnClickPlanButton()
        {
            SetupCancelButton("请选择要实行何计,点此取消", () =>
            {
                if (uiPlanPanel.gameObject.activeInHierarchy)
                {
                    uiPlanPanel.gameObject.SetActive(false);
                }
                ResetCancelButton();
            });
            uiPlanPanel.PlanNum = 16;//GeneralListCache.GetGeneral(selectedUnit.genID).GetPlanNum();
            uiPlanPanel.ShowScrollPlanPanel();
        }

        private void OnClickCancelPlan()
        {
            if (WarManager.Instance.warState == WarState.AITurn)
            {
                return;
            }
            if (uiPlanPanel.gameObject.activeInHierarchy)
            {
                uiPlanPanel.gameObject.SetActive(false);
            }
            Button button = warInfo.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.enabled = false;
            DisplayWarMenu();
            Debug.Log("取消计谋");
        }
        
        // 点击攻击按钮
        private void OnClickAttackButton()
        {
            SetupCancelButton("请选择进攻方向,点此取消", ResetCancelButton);
            MapManager.Instance.DisplayAttackCells(MapManager.GetCanAttackCell(WarManager.Instance.hmUnitObj.arrayPos));
            Debug.Log("开始战斗");
        }

        private void OnClickCancelAttack()
        {
            if (WarManager.Instance.warState == WarState.AITurn)
            {
                return;
            }
            mapManager.ClearAllMarkers();
            Button button = warInfo.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.enabled = false;
            DisplayWarMenu();
            Debug.Log("取消攻击");
        }

        // 切换情报开关
        private void OnClickIntelligenceButton()
        {
            OnInfoPanelShow?.Invoke(true);
            SetupCancelButton("请选择要查看的军队,点此取消", () =>
            {
                if (uiIntelligencePanel.gameObject.activeInHierarchy)
                {
                    uiIntelligencePanel.gameObject.SetActive(false);
                }
                OnInfoPanelShow?.Invoke(false);
                ResetCancelButton();
            });
            
            Debug.Log("开启情报");
        }

        private void OnClickCancelIntelligence()
        {
            if (WarManager.Instance.warState == WarState.AITurn)
            {
                return;
            }

            if (uiIntelligencePanel.gameObject.activeInHierarchy)
            {
                uiIntelligencePanel.gameObject.SetActive(false);
            }
            OnInfoPanelShow?.Invoke(false);
            Button button = warInfo.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.enabled = false;
            DisplayWarMenu();
            Debug.Log("取消情报");
        }
        
        //点击撤退按钮
        void OnClickRetreatButton()
        {
            HideUnderMenu();
            uiRetreatPanel.AllRetreatOver += WarManager.Instance.PlayerWithdraw;
            uiRetreatPanel.ShowRetreatPanel();
        }
        
        
        
        //点击待机按钮
        void OnClickIdleButton()
        {
            WarManager.Instance.hmUnitObj.SetUnitState(UnitState.Idle);
            WarManager.Instance.hmUnitObj.GetComponent<Toggle>().interactable = false;
            WarManager.Instance.hmUnitObj.GetComponent<Toggle>().isOn = false;
            
            byte c = 0;//完成行动计数器
            foreach (var unit in WarManager.Instance.hmUnits)
            {
                if (unit.unitState == UnitState.Idle || unit.unitState == UnitState.Captive)
                {
                    c++;
                }
            }
            Debug.Log(c);
            // 如果所有单位都已经完成行动
            if (c == WarManager.Instance.hmUnits.Count)
            {
                foreach (var unit in WarManager.Instance.hmUnits)
                {
                    unit.GetComponent<Toggle>().interactable = true;
                }
                foodText.text = $"粮食:{WarManager.Instance.aiFood}";
                goldText.text = $"食物:{WarManager.Instance.aiGold}";
                WarTipText("敌军行动中...");
                WarManager.Instance.warState = WarState.EndRound;
            }
            else
            {
                WarTipText("请选择军队执行命令");
            }
            WarManager.Instance.hmUnitObj = null;
            Debug.Log("单位待机");
        }


        private void HandleInVillage(bool option)
        {
            if (option)
            {
                switch (WarManager.Instance.hmUnitObj.Terrain)
                {
                    case 5:// 战场武器铺
                        uiSmithy.GoSmithy += DoneWarSmithy;
                        uiSmithy.Smithy(WarManager.Instance.curWarCityId, WarManager.Instance.hmGold, true, WarManager.Instance.hmUnitObj.genID);
                        break;
                    case 6:// 战场粮店
                        uiStore.GoStore += DoneWarStore;
                        uiStore.Store(WarManager.Instance.hmFood, WarManager.Instance.hmGold);
                        break;
                    case 7:// 战场医馆
                        if (WarManager.Instance.hmGold < 100)
                        {
                            NotifyWarEvent("财资不足");
                        }
                        else
                        {
                            WarManager.Instance.hmGold -= 100;
                            byte hpTreat = (byte)UnityEngine.Random.Range(20, 36);
                            General general = GeneralListCache.GetGeneral(WarManager.Instance.hmUnitObj.genID);
                            general.AddCurPhysical(hpTreat);
                            NotifyWarEvent($"{general.generalName}体力恢复了{hpTreat}");
                        }
                        break;
                }
            }
            DisplayWarMenu();
            uiTips.OnOptionSelected -= HandleInVillage;
        }

        private void DoneWarSmithy(short gold, string text)
        {
            WarManager.Instance.hmGold = gold;
            goldText.text = $"黄金：{gold}";
            NotifyWarEvent(text);
            uiSmithy.GoSmithy -= DoneWarSmithy;
        }

        private void DoneWarStore(short food, short gold)
        {
            DisplayWarMenu();
            WarManager.Instance.hmFood = food;
            WarManager.Instance.hmGold = gold;
            foodText.text = $"粮食：{food}";
            goldText.text = $"黄金：{gold}";
            uiStore.GoStore -= DoneWarStore;
        }
        
    }
}
