/*
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BaseClass;
using DataClass;
using UnityEngine.EventSystems;
using TMPro;
using UIClass;

public class UIWarPanel :MonoBehaviour
{
    private static UIWarPanel _instance;
    public static UIWarPanel Instance => _instance;

    private Unit hmUnit {get=>WarInfo.Instance.hmUnit;set=>WarInfo.Instance.hmUnit=value;}
    private Unit aiUnit {get=>WarInfo.Instance.aiUnit;set=>WarInfo.Instance.aiUnit=value;}
    private List<Unit> aiUnits{ get=>WarInfo.Instance.aiUnits; set=>WarInfo.Instance.aiUnits=value; }
    private List<Unit> hmUnits{get=>WarInfo.Instance.hmUnits; set=>WarInfo.Instance.hmUnits=value; }
    private bool IsSelected {get=>WarInfo.Instance.isSelected;set=>WarInfo.Instance.isSelected=value;}
    
    
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
    

    private UIWarPanel() { }

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
            
    }

    void Update()
    {
        /#1#/ 检查是否点击了 UI 元素
        if (EventSystem.current.IsPointerOverGameObject())
        {
            //Debug.Log("点击了 UI 元素");
            return; // 如果点击的是 UI，直接返回
        }#1#

        dayText.text = "第" + WarInfo.Instance.day + "天\n天气:阴";

        if(WarInfo.Instance.warState == WarState.PlayerTurn)
        {
            foodText.text = $"粮食:{WarInfo.Instance.hmFood}";
            goldText.text = $"食物:{WarInfo.Instance.hmGold}";
        }
        
        
    }

    public static byte GetUnitIndex(General general, List<Unit> units)
    {
        return units.Find(t => t.genID == general.generalId).index;
    }
    
    
    
    public void SwitchOnUnitToggle(bool isOn, List<Unit> units)
    {
        foreach (var unit in units)
        {
            unit.gameObject.GetComponent<Toggle>().interactable = isOn;
        }
    }

    public void DisplayWarEvent(string text)
    {
        StartCoroutine(uiTips.ShowTipsOnlyConfirm(text));
    }

    public void DisableUnderMenu()
    {
        warInfo.gameObject.SetActive(true);
        warMenu.gameObject.SetActive(true);
    }
    
    public void DisplayWarMenu()
    {
        warInfo.gameObject.SetActive(false);
        warMenu.gameObject.SetActive(true);
    }
    
    public void WarTipText(string text)
    {
        warMenu.SetActive (false);
        warInfo.gameObject.SetActive(true);
        warInfo.GetComponent<TextMeshProUGUI>().text = text;
        warInfo.GetComponent<Button>().onClick.AddListener(OnClickWarTipButton);
    }

    // 点击移动按钮
    private void OnClickMoveButton()
    {
        // 如果已经移动过，则不显示可移动范围
        if (hmUnit.isMoved)
        {
            if (hmUnit.InVillage())
            {
                WhetherGoVillage(hmUnit.Terrain);
            }
            WarTipText("武将已经移动过,点此取消");
            return; // 直接返回
        }
            
        // 获取将军的当前位置并计算可移动范围
        if (true)
        {
            WarTipText("请选择行军到何处,点此取消");
            Debug.Log("当前位置:" + hmUnit.arrayPos);
            // 确保 tilePos 是将军当前的单元格位置
            hmUnit.unitState = UnitState.Move;
            // 显示可移动范围
            mapManager.DisplayNavigation(mapManager.GetMovableCellsAStar(hmUnit)); 
        }
    }

    //TODO
    public void WhetherGoVillage(byte terrain) //是否进入村庄提示
    {
        string text;
        uiTips.OnOptionSelected += ExecuteActionBasedOnSelection;
        switch (terrain)
        {
            case 5: //武器店
                text = "是否进入武器店？";
                hmUnit.unitState = UnitState.Smithy;
                WarInfo.Instance.warState = WarState.WarPause;
                uiTips.ShowOptionalTips(text);
                break;
            case 6: //粮食店
                text = ("是否进入粮食店？");
                hmUnit.unitState = UnitState.Shop;
                WarInfo.Instance.warState = WarState.WarPause;
                uiTips.ShowOptionalTips(text);
                break;
            case 7: //医馆
                text = ("是否进入医馆？");
                hmUnit.unitState = UnitState.Hospital;
                WarInfo.Instance.warState = WarState.WarPause;
                uiTips.ShowOptionalTips(text);
                break;
        }
    }
    
    // 点击计策按钮
    private void OnClickPlanButton()
    {
        hmUnit.unitState = UnitState.Plan;
        WarInfo.Instance.warState = WarState.WarPause;
        WarTipText("请选择要实行何计,点此取消");
        uiPlanPanel.PlanNum = 16;//GeneralListCache.GetGeneral(hmUnit.genID).getGeneralPlanNum();
        uiPlanPanel.ShowScrollPlanPanel();
    }

    // 点击攻击按钮
    private void OnClickAttackButton()
    {
        Debug.Log("开始战斗");
    }


    // 切换情报开关
    private void OnClickIntelligenceButton()
    {
        hmUnit.unitState = UnitState.Information;
        SwitchOnUnitToggle(true, aiUnits);
        Debug.Log("开启情报");
        WarTipText("请选择要查看的军队,点此取消");   
    }

    //点击撤退按钮
    void OnClickRetreatButton()
    {
        hmUnit.unitState = UnitState.Retreat;
        WarTipText("请选择撤退到何城,点此取消");
    }

    //点击待机按钮
    void OnClickIdleButton()
    {
        hmUnit.unitState = UnitState.Idle;
        hmUnit.GetComponent<Image>().color = Color.white;
        hmUnit = null;
        byte c = 0;
        for (byte i = 1; i < hmUnits.Count; i++)
        {
            if (hmUnits[i].unitState == UnitState.Idle)
                c++;
        }

        if (c == hmUnits.Count)
        {
            WarInfo.Instance.warState = WarState.EndRound;
            foodText.text = $"粮食:{WarInfo.Instance.aiFood}";
            goldText.text = $"食物:{WarInfo.Instance.aiGold}";
            warMenu.gameObject.SetActive(false);
            warInfo.gameObject.SetActive(true);
            warInfo.GetComponent<TextMeshProUGUI>().text = "我军待机中...";
        }
        //warInfo.GetComponent<TextMeshProUGUI>().text = "请选择军队执行命令,点此取消";
        Debug.Log("单位待机");
    }

    // 点击操作提示文字返回主面板
    public void OnClickWarTipButton()
    {
        if (hmUnit == null) return;
        switch (hmUnit.unitState) 
        { 
            case UnitState.Move:
                mapManager.ClearAllMarkers();
                break;
            case UnitState.Plan:
                mapManager.ClearAllMarkers();
                uiPlanPanel.gameObject.SetActive(false);
                break;
            case UnitState.Information:
                uiIntelligencePanel.gameObject.SetActive(false);
                break;
        }
        hmUnit.unitState = UnitState.None;
        warInfo.gameObject.SetActive(false);
        warMenu.gameObject.SetActive(true);
        Debug.Log("取消操作");
    }
    
    public bool IsTriggerJunHun(short GenId)
    {
        General general = GeneralListCache.GetGeneral(GenId);
        
        if(general.HasSkill(3,8))
        {    
            if(hmUnit.genID == GenId)
            {
                foreach (var hmUnit in hmUnits)
                {
                    if (hmUnit.genID != GenId)
                    {
                        byte x1 = (byte)hmUnit.arrayPos.x;
                        byte y1 = (byte)hmUnit.arrayPos.y;
                        byte x2 = (byte)hmUnit.arrayPos.x;
                        byte y2 = (byte)hmUnit.arrayPos.y;
                        return MapManager.IsAdjacent(x1, y1, x2, y2);
                    }
                }
            }
            else
            {
                foreach (var aiUnit in aiUnits)
                {
                    if (aiUnit.genID != GenId)
                    {
                        byte x1 = (byte)aiUnit.arrayPos.x;
                        byte y1 = (byte)aiUnit.arrayPos.y;
                        byte x2 = (byte)aiUnit.arrayPos.x;
                        byte y2 = (byte)aiUnit.arrayPos.y;
                        return MapManager.IsAdjacent(x1, y1, x2, y2);
                    }
                }
            }
           
        }
        return false;
    }
    
    
    public void ExecuteActionBasedOnSelection(bool option)
    {
        if (option)
        {
            DisableUnderMenu();
            Debug.Log("执行 Option1 的后续操作");
            switch (hmUnit.Terrain)
            {
                case 5:// 战场武器铺
                    //TODO
                    Debug.Log("武器库");
                    uiSmithy.GoSmithy += UiSmithyOnGoSmithy;
                    uiSmithy.Smithy(WarInfo.Instance.curWarCityId, WarInfo.Instance.hmGold, true, WarInfo.Instance.hmUnit.genID);
                    break;
                case 6:// 战场粮店
                    uiStore.OnResourcesChanged += ResourcesChanged;
                    uiStore.Restore(WarInfo.Instance.hmFood, WarInfo.Instance.hmGold);
                    break;
                case 7:// 战场医馆
                    if (WarInfo.Instance.hmGold < 100)
                    {
                        StartCoroutine(uiTips.ShowTipsOnlyConfirm("财资不足"));
                    }
                    else
                    {
                        WarInfo.Instance.hmGold -= 100;
                        byte hpTreat = WarManager.WarTreatValue();
                        General general = GeneralListCache.GetGeneral(hmUnit.genID);
                        general.addCurPhysical(hpTreat);
                        StartCoroutine(uiTips.ShowTipsOnlyConfirm($"{general.generalName}体力恢复了{hpTreat}"));
                    }
                    break;
            }
        }
        uiTips.OnOptionSelected -= ExecuteActionBasedOnSelection;
    }

    private void UiSmithyOnGoSmithy(short outGold, string obj)
    {
        WarInfo.Instance.hmGold = outGold;
        StartCoroutine(uiTips.ShowTipsOnlyConfirm(obj));
        uiSmithy.GoSmithy -= UiSmithyOnGoSmithy;
    }

    private void ResourcesChanged(short food, short gold)
    {
        DisplayWarMenu();
        WarInfo.Instance.hmFood = food;
        WarInfo.Instance.hmGold = gold;
        foodText.text = $"粮食：{food}";
        goldText.text = $"黄金：{gold}";
        uiStore.OnResourcesChanged -= ResourcesChanged;
    }
}
*/
