/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using BaseClass;
using DataClass;
using static DataClass.GameInfo;


public class UICityPanel : MonoBehaviour
{
    [SerializeField]RawImage Head;
    [SerializeField]TextMeshProUGUI CityName;
    [SerializeField]TextMeshProUGUI King;
    [SerializeField]TextMeshProUGUI Order;
    [SerializeField]TextMeshProUGUI Prefect;
    [SerializeField]TextMeshProUGUI Rule;
    [SerializeField]TextMeshProUGUI Population;
    [SerializeField]TextMeshProUGUI GeneralNum;
    [SerializeField]TextMeshProUGUI CitySoldier;
    [SerializeField]TextMeshProUGUI Agro;
    [SerializeField]TextMeshProUGUI Trade;
    [SerializeField]TextMeshProUGUI Flood;
    [SerializeField]TextMeshProUGUI Money;
    [SerializeField]TextMeshProUGUI Food;

    [SerializeField] Button GlobalButton;//返回全国地图
    [SerializeField] GameObject FirstMenu;

    [SerializeField] Button MoveButton;
    [SerializeField] Button AttackButton;
    [SerializeField] Button ConscriptButton;
    [SerializeField] Button TransportButton;

    [SerializeField] Button ReclaimButton;
    [SerializeField] Button MercantileButton;
    [SerializeField] Button TameButton;
    [SerializeField] Button PatrolButton;

    [SerializeField] Button SearchButton;
    [SerializeField] Button EmployButton;
    [SerializeField] Button RewardButton;
    [SerializeField] Button AppointButton;

    [SerializeField] Button TruceButton;
    [SerializeField] Button AlienateButton;
    [SerializeField] Button BribeButton;
    [SerializeField] Button IntelligenceButton;
    [SerializeField] GameObject GeneralInfoButton;
    [SerializeField] GameObject CancelButton;//情报查看取消



    [SerializeField] Button ShopButton;
    [SerializeField] Button SmithyButton;
    [SerializeField] Button SchoolButton;
    [SerializeField] Button HospitalButton;

    [SerializeField] Button SaveButton;
    [SerializeField] Button LoadButton;
    [SerializeField] Button SettingsButton;
    [SerializeField] Button ExitButton;



    void Start()
    {
        DataManagement.LoadAndInitializeData(); ;

        doGeneralIds.Clear();

        targetGeneralIds.Clear();

        InitializeCityPanel();



    }

    void InitializeCityPanel()
    {
        GlobalButton.onClick.AddListener(OnGlobalButtonOnClick);

        if (Task == TaskType.Intelligence)
        {
            ShowCityInfo(targetCityId);
            FirstMenu.SetActive(false);
            GeneralInfoButton.SetActive(true);
            CancelButton.SetActive(true);
            GeneralInfoButton.GetComponent<Button>().onClick.AddListener(OnGeneralInfoButtonClick);
            CancelButton.GetComponent<Button>().onClick.AddListener(OnCancelButtonClick);
        }
        else
        {
            Task = TaskType.None;
            ShowCityInfo(doCityId);
            MoveButton.onClick.AddListener(OnMoveButtonClick);
            AttackButton.onClick.AddListener(OnAttackButtonClick);
            ConscriptButton.onClick.AddListener(OnConscriptButtonClick);
            TransportButton.onClick.AddListener(OnTransportButtonClick);
            
            SearchButton.onClick.AddListener(OnSearchButtonClick);
            EmployButton.onClick.AddListener(OnEmployButtonClick);
            RewardButton.onClick.AddListener(OnRewardButtonClick);
            AppointButton.onClick.AddListener(OnAppointButtonClick);

            ReclaimButton.onClick.AddListener(OnReclaimButtonClick);
            MercantileButton.onClick.AddListener(OnMercantileButtonClick);
            TameButton.onClick.AddListener(OnTameButtonClick);
            PatrolButton.onClick.AddListener(OnPatrolButtonClick);

            TruceButton.onClick.AddListener(OnTruceButtonClick);
            AlienateButton.onClick.AddListener(OnAlienateButtonClick);
            BribeButton.onClick.AddListener(OnBribeButtonClick);
            IntelligenceButton.onClick.AddListener(OnIntelligenceButtonClick);

            ShopButton.onClick.AddListener(OnShopButtonClick);
            SmithyButton.onClick.AddListener(OnSmithyButtonClick);
            SchoolButton.onClick.AddListener(OnSchoolButtonClick);
            HospitalButton.onClick.AddListener(OnHospitalButtonClick);

            SaveButton.onClick.AddListener(OnSaveButtonClick);
            LoadButton.onClick.AddListener(OnLoadButtonClick);
            SettingsButton.onClick.AddListener(OnSettingsButtonClick);
            //ExitButton.onClick.AddListener(OnExitButtonClick);
        }
        System.GC.Collect(); // 强制进行垃圾回收
    }

    void ShowCityInfo(byte cityId)
    {
        City city = CityListCache.GetCityByCityId(cityId);
        if (city.prefectId == 0)
        {
            Head.texture = Resources.Load<Texture2D>($"HeadImage/1");
            Prefect.text = "无";
            King.text = "无";
            Order.text = "";
        }
        else
        {
            Debug.Log(city.cityId);
            Head.texture = Resources.Load<Texture2D>($"HeadImage/{city.prefectId}");
            Prefect.text = GeneralListCache.GetGeneral(city.prefectId).generalName;
            King.text = GeneralListCache.GetGeneral(city.cityBelongKing).generalName;
            Order.text = GetPlayerOrderNum().ToString();
        }
        CityName.text = city.cityName;
        Rule.text = city.rule.ToString();
        Population.text = city.population.ToString();
        GeneralNum.text = city.getCityGeneralNum().ToString();
        CitySoldier.text = city.GetCityAllSoldierNum().ToString();
        Agro.text = city.agro.ToString();
        Trade.text = city.trade.ToString();
        Flood.text = city.floodControl.ToString();
        Money.text = city.money.ToString();
        Food.text = city.food.ToString();
        
    }

    //返回全国城市界面
    void OnGlobalButtonOnClick()
    {
        SceneManager.LoadScene("GlobalMap");
    }
    //情报探查显示将领信息
    void OnGeneralInfoButtonClick()
    {
        doGeneralIds= CityListCache.GetCityByCityId(targetCityId).GetCityOfficeGeneralIdArray().ToList();
        SceneManager.LoadScene("GeneralPanel");
    }
    //情报取消
    void OnCancelButtonClick()
    {
        Task = TaskType.None;
        SceneManager.LoadScene("CityPanel");
    }
    //移动
    void OnMoveButtonClick()
    {
        Task = TaskType.Move;
        SceneManager.LoadScene("GlobalMap");
    }
    //攻城
    void OnAttackButtonClick()
    {
        if(attackCount<=2)
        {
            attackCount++;
            Task = TaskType.Attack;
            SceneManager.LoadScene("GlobalMap");
        }
        else
        {
            Task = TaskType.OverAttack;
            SceneManager.LoadScene("ExecutivePanel");
        }
    }

    void OnConscriptButtonClick()
    {
        Task = TaskType.Conscript;
        SceneManager.LoadScene("ExecutivePanel");
    }

    void OnTransportButtonClick()
    {
        Task = TaskType.Transport;
        SceneManager.LoadScene("GlobalMap");
    }
    void EmployCheck()
    {
        City city = CityListCache.GetCityByCityId(doCityId); // 获取目标城市对象

        if (city.getCityGeneralNum() == 10) // 如果城市中的将军数量为 10
        {
            Task = TaskType.OverEmploy;
            doGeneralIds.Add(city.prefectId);
            SceneManager.LoadScene("ExecutivePanel");
            //SetTipsInfo(city.prefectId, "本城置吏已足!"); // 设置提示信息
            return;
        }

        else if (city.GetReservedGeneralNum() == 0) // 如果在野将军数量为 0
        {
            Task = TaskType.EmployNothing;
            doGeneralIds.Add(city.prefectId);
            SceneManager.LoadScene("ExecutivePanel");
            return;
        }

               /* int reservedGeneralNum = city.GetReservedGeneralNum(); // 设置可执行将军数量
                if (reservedGeneralNum != 0) 
                {       
                    // 如果在野将军数量小于可执行将军数量
                    if (reservedGeneralNum> 10)
                        reservedGeneralNum = 10; // 如果可执行将军数量大于 10，设置为 10

                // 将在野将军 ID 添加到数组中
                for (byte i = 0; i < reservedGeneralNum; i++)
                    doGeneralIds[i] = city.GetReservedGeneralId(i);
                }#1#
        Debug.Log("所选将领ID：" + string.Join(", ", city.GetCityNotFoundGeneralIdArray()));
        Task = TaskType.Employ;
        SceneManager.LoadScene("SelectGeneral");

    }

    void RewardCheck()
    {
        City city = CityListCache.GetCityByCityId(doCityId); // 获取目标城市对象
        short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray(); // 获取城市中的将军 ID 数组
        List<short> canRewardGeneralIds = new List<short>();

        // 遍历城市中的将军，检查忠诚度
        for (int i = 0; i < city.getCityGeneralNum(); i++)
        {
            General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]); // 获取将军对象
            if (general.getLoyalty() < 99) // 如果将军忠诚度小于 99
            {
                canRewardGeneralIds.Add (general.generalId); // 增加可执行将军数量
            }
        }

        if (canRewardGeneralIds.Count > 0)
        {
            Task = TaskType.Reward;
            doGeneralIds = canRewardGeneralIds;
            SceneManager.LoadScene("SelectGeneral");
        }
        else
        {
            Task = TaskType.RewardDeny;
            doGeneralIds.Add(city.prefectId);
            SceneManager.LoadScene("ExecutivePanel");
        }
    }

    void AppointCheck()
    {
        City city = CityListCache.GetCityByCityId(doCityId); // 获取目标城市对象
        if (city.prefectId == city.cityBelongKing)
        {
            Task = TaskType.AppointDeny;
            doGeneralIds.Add(city.prefectId);
            SceneManager.LoadScene("ExecutivePanel");
        }
        else
        {
            Task = TaskType.Appoint;
            doGeneralIds = city.GetCityOfficeGeneralIdArray().ToList(); // 获取城市中的将军 ID 数组
            doGeneralIds.Remove(city.prefectId);
            SceneManager.LoadScene("SelectGeneral");
        }
        
    }

    // 内政通用方法处理按钮点击
    void UniversalInterior(TaskType taskType, City currentCity, int maxValue, Func<City, int> propertySelector)
    {
        // 检查是否有足够的金钱
        if (currentCity.GetMoney() == 0) // 如果目标城市的金钱为 0
        {
            Task = TaskType.Lack;
            doGeneralIds[0] = currentCity.prefectId;
            SceneManager.LoadScene("ExecutivePanel");
        }

        int propertyValue = propertySelector(currentCity); // 使用传入的选择器函数获取属性值

        if (propertyValue == maxValue) // 检查城市特定属性值
        {
            Task = (TaskType)Enum.Parse(typeof(TaskType), "Over" + taskType);
            doGeneralIds[0] = currentCity.prefectId;
            SceneManager.LoadScene("ExecutivePanel");
        }
        else
        {
            Task = taskType;
            doGeneralIds =currentCity.GetCityOfficeGeneralIdArray().ToList();
            SceneManager.LoadScene("SelectGeneral"); // 下一个选择武将场景
        }
    }

    void UniversalMarketCheck(TaskType taskType, City currentCity, Func<City, bool> propertySelector)
    {
        if (taskType != TaskType.Shop||taskType!=TaskType.Smithy)
        {
            // 检查是否有足够的金钱
            if (currentCity.GetMoney() == 0) // 如果目标城市的金钱为 0
            {
                Task = TaskType.Lack;
                doGeneralIds.Add(currentCity.prefectId);
                SceneManager.LoadScene("ExecutivePanel");              
            }
        }

        bool propertyValue = propertySelector(currentCity); // 使用传入的选择器函数获取属性值

        if (!propertyValue) // 检查城市特定属性值
        {
            Task = (TaskType)Enum.Parse(typeof(TaskType), "Lack"+taskType);
            doGeneralIds.Add(currentCity.prefectId);
            SceneManager.LoadScene("ExecutivePanel");
        }
        else
        {
            switch(taskType)
            { 
                case TaskType.Shop:
                    Task = taskType;
                    doGeneralIds.Add(currentCity.prefectId);
                    SceneManager.LoadScene("ExecutivePanel"); // 下一个选择武将场景
                    break;
                case TaskType.Smithy:
                    Task = taskType;
                    doGeneralIds.Add(currentCity.prefectId);
                    doGeneralIds = currentCity.GetCityOfficeGeneralIdArray().ToList();
                    SceneManager.LoadScene("SelectGeneral"); // 下一个选择武将场景
                    break;
                case TaskType.School:
                    if (currentCity.CanCityGeneralStudy())
                    {
                        Task = taskType;
                        doGeneralIds = currentCity.GetCanStudyGeneralIds(currentCity.cityId);
                        SceneManager.LoadScene("SelectGeneral"); // 下一个选择武将场景
                    }
                    else
                    {
                        Task = TaskType.SchoolDeny;
                        doGeneralIds.Add(currentCity.prefectId);
                        SceneManager.LoadScene("ExecutivePanel"); 
                    }
                        break;
                case TaskType.Hospital:
                    if (currentCity.CanCityGeneralTreat())
                    {
                        Task = taskType;
                        doGeneralIds = currentCity.GetCanTreatGeneralIds();
                        SceneManager.LoadScene("SelectGeneral");// 下一个选择武将场景}
                    }
                    else 
                    {
                        Task = TaskType.HospitalDeny;
                        doGeneralIds.Add(currentCity.prefectId);
                        SceneManager.LoadScene("ExecutivePanel");
                    }
                break;
            }
        }
    }   

    // 当开垦按钮被点击时执行开垦检测 
    public void OnReclaimButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalInterior(TaskType.Reclaim, currentCity, 999, city => city.agro);
    }

    // 当劝商按钮被点击时执行劝商检测
    public void OnMercantileButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalInterior(TaskType.Mercantile, currentCity, 999, city => city.trade);
    }

    // 当治水按钮被点击时执行治水检测
    public void OnTameButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalInterior(TaskType.Tame, currentCity, 99, city => city.floodControl);
    }

    // 当巡查按钮被点击时执行巡查检测
    public void OnPatrolButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalInterior(TaskType.Patrol, currentCity, 999999, city => city.population);
    }

    // 当搜索按钮被点击时执行搜索检测
    void OnSearchButtonClick()
    {
        Task = TaskType.Search;
        optionalGeneralIds = CityListCache.GetCityByCityId(doCityId).GetCityOfficeGeneralIdArray().ToList();
        SceneManager.LoadScene("SelectGeneral");
    }

    void OnEmployButtonClick()
    {
        Task = TaskType.Employ;
        EmployCheck();
    }

    void OnRewardButtonClick()
    {
        Task = TaskType.Reward;
        RewardCheck();
    }

    void OnAppointButtonClick()
    {
        Task = TaskType.Reward;
        AppointCheck();
    }

    void OnTruceButtonClick()
    {
        Country country = CountryListCache.GetCountryByCountryId(playerCountryId);
        if (country.GetAllianceSize() <CountryListCache.countryDictionary.Count)
        {
            Task = TaskType.TruceSelect;
        }
        else { Task = TaskType.OverTruce; }
        SceneManager.LoadScene("ExecutivePanel");
    }

    void OnAlienateButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        Task=TaskType.Alienate;
        SceneManager.LoadScene("GlobalMap");
    }

    void OnBribeButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        Task = TaskType.Bribe;
        SceneManager.LoadScene("GlobalMap");
    }

    void OnIntelligenceButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        Task = TaskType.Intelligence;
        doGeneralIds= currentCity.GetCityOfficeGeneralIdArray().ToList();
        SceneManager.LoadScene("GlobalMap");
    }

    // 当钱粮按钮被点击时执行钱粮检测
    void OnShopButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalMarketCheck(TaskType.Shop, currentCity, city => city.cityGrainShop);
    }

    void OnSmithyButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalMarketCheck(TaskType.Smithy, currentCity, city => city.haveCitySmithy());
    }
    void OnSchoolButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalMarketCheck(TaskType.School, currentCity, city => city.citySchool);
    }

    void OnHospitalButtonClick()
    {
        City currentCity = CityListCache.GetCityByCityId(doCityId);
        UniversalMarketCheck(TaskType.Hospital, currentCity, city => city.cityHospital);
    }

    void OnSaveButtonClick()
    {
        Task = TaskType.Save;
        SceneManager.LoadScene("ExecutivePanel");
    }

    void OnLoadButtonClick()
    {
        Task = TaskType.Load;
        SceneManager.LoadScene("ExecutivePanel");
    }
    void OnSettingsButtonClick()
    {
        Task = TaskType.Settings;
        SceneManager.LoadScene("ExecutivePanel");
    }
}
*/


