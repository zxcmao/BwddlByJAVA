using System;
using BaseClass;
using DataClass;
using TMPro;
using UIClass;
using UnityEngine;

namespace War
{
    public class UIPlanPanel : MonoBehaviour
    {
        private static UIPlanPanel _instance;
        public static UIPlanPanel Instance => _instance;

        public byte PlanNum;

        public byte planIndex
        {
            get => WarManager.Instance.planIndex;
            set => WarManager.Instance.planIndex = value;
        }
    
        [SerializeField] private TextMeshProUGUI planText;

        private string planResult
        {
            get => WarManager.Instance.planResult;
            set => WarManager.Instance.planResult = value;
        }
        [SerializeField] private SelectHorizontalScroll horizontalScroll;
        private UIPlanPanel() { }

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

        public void ShowScrollPlanPanel()
        {
            //使用列表时要在列表初始化之前设置选项信息
            if (horizontalScroll != null)
            {
                string[] names = new string[PlanNum];
                Sprite[] sprites = new Sprite[PlanNum];
                string[] descriptions = new string[PlanNum];
                for (byte i = 0; i < PlanNum; i++)
                {
                    names[i] = TextLibrary.PlanName[i];
                    sprites[i] = Resources.Load<Sprite>("UI/MultipleOption");
                    descriptions[i] = TextLibrary.PlanExplain[i];
                }
                horizontalScroll.SetItemsInfo(names, sprites, descriptions);
                gameObject.SetActive(true);
                Debug.Log("打开计策面板");
                horizontalScroll.SelectAction += (index) =>
                {
                    horizontalScroll.isSelected = false;
                    planIndex = (byte) index;
                    PreparePlan();
                };
            }
        }

    

        void PreparePlan()
        {
            Debug.Log("选择的计谋系数:"+planIndex);
            General doGeneral = GeneralListCache.GetGeneral(WarManager.Instance.hmUnitObj.genID);
            Plan plan = Plan.GetPlan(planIndex);
            // 检测当前机动力是否足够
            if (plan.UseCost(doGeneral) > WarManager.Instance.hmUnitObj.moveBonus) 
            {
                UIWar.Instance.NotifyWarEvent("当前机动力不足");
            }
            else 
            {   //如果机动力足够先检测是计谋否需要自身地形满足条件
                if (plan is IDoTerrain doTerrainPlan)
                {
                    if (!doTerrainPlan.IsSuitable(WarManager.Instance.hmUnitObj.Terrain))
                    {
                        UIWar.Instance.NotifyWarEvent("此地无法施展本计");
                    }
                }
                else if (planIndex == 15)
                {   //如果不需要再检测是否是奇门遁甲否则选地施放
                    if(GeneralListCache.GetGeneral(WarManager.Instance.hmUnitObj.genID).generalSoldier < 100)
                    {
                        UIWar.Instance.NotifyWarEvent("兵马不足施展本计");
                    }
                    else
                    {
                        MapManager.Instance.DisplayPlanCells(planIndex, MapManager.GetCellsInRange(WarManager.Instance.hmUnitObj.arrayPos, plan.InDistance(doGeneral)));
                        UIWar.Instance.SetupCancelButton("选择计策实施地点,点此取消",  UIWar.Instance.ResetCancelButton);
                    }
                }
                else//否则选地施放
                {
                    MapManager.Instance.DisplayPlanCells(planIndex, MapManager.GetCellsInRange(WarManager.Instance.hmUnitObj.arrayPos, plan.InDistance(doGeneral)));
                    UIWar.Instance.SetupCancelButton("选择计策实施地点,点此取消",  UIWar.Instance.ResetCancelButton);
                }
            }
            gameObject.SetActive(false);
        }
        
    }
}
