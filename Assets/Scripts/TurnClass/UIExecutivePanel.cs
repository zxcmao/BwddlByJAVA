using System.Collections;
using System.Linq;
using BaseClass;
using DataClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DataClass.GameInfo;
using static DataClass.TextLibrary;

namespace TurnClass
{
    public class UIExecutivePanel : MonoBehaviour
    {
        [SerializeField] UITips uiTips;
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] GameObject interiorPanel;
        [SerializeField] GameObject rewardPanel;
        [SerializeField] GameObject confirmButton;
        [SerializeField] GameObject cancelButton;

        [SerializeField] GameObject attackPanel;//准备钱粮攻打
        [SerializeField] TMP_InputField setFood;
        [SerializeField] TMP_InputField setMoney;
        [SerializeField] TextMeshProUGUI assetInfo;

        [SerializeField] UIConscript conscriptPanel;//征兵面板

        [SerializeField] GameObject transportPanel;//准备运输的信息
        [SerializeField] TMP_InputField transportFood;
        [SerializeField] TMP_InputField transportMoney;
        [SerializeField] TMP_InputField transportTreasure;
        [SerializeField] TextMeshProUGUI transportInfo;

        [SerializeField] TextMeshProUGUI Info1;//显示内政的信息
        [SerializeField] TextMeshProUGUI Info2;
        [SerializeField] TextMeshProUGUI SelectName;
        [SerializeField] RawImage HeadImage;
        [SerializeField] TextMeshProUGUI UseInfo;
        [SerializeField] Toggle useTreasure;
        [SerializeField] Toggle useMoney;
        private int _useGold = 0;

        [SerializeField] GameObject trucePanel;
        [SerializeField] UIStore storePanel;

        [SerializeField] UISmithy smithyPanel;
        
        [SerializeField] UIRecord recordPanel;//显示存档面板

        [SerializeField] GameObject settingsPanel;//显示设置面板
        [SerializeField] Toggle closeMusic;
        [SerializeField] Toggle openMusic;
        [SerializeField] Slider volumeSlider;

        private static UIExecutivePanel instance;

        public static UIExecutivePanel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UIExecutivePanel>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this; // 设置实例
        }

        void Start()
        {
            _useGold = 0;
            StartExecutivePanel();
        }
    
        public void StartExecutivePanel()
        {
            confirmButton.SetActive(false);
            cancelButton.SetActive(false);
            title.text = TextLibrary.GetTaskDescription(Task);
            City city = CityListCache.GetCityByCityId(doCityId); // 获取目标城市对象
            // 检查 SceneData 中的任务类型
            switch (Task)
            {
                case TaskType.Move:
                    Move();
                    break;
                case TaskType.OverMove:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[0][1]));
                    break;
                case TaskType.OverAttack:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[0][3]));
                    break;
                case TaskType.Attack:
                    AttackAsset();
                    break;
                case TaskType.Conscript:
                    conscriptPanel.Conscript(doCityId);
                    break;
                case TaskType.Transport:
                    Transport();
                    break;
                case TaskType.Search:
                    SubPlayerOrder();
                    StartCoroutine(uiTips.ShowTaskTips(city.Search(doGeneralIds[0]), TaskType.Search));
                    break;
                case TaskType.OverEmploy:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[2][0]));
                    break;
                case TaskType.EmployNothing:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[2][1]));
                    break;
                case TaskType.Employ:
                    SubPlayerOrder();
                    if (city.IsEmploy(doGeneralIds[0], targetGeneralIds[0]))
                    {
                        StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[2][2]));
                    }
                    else//如果登用失败
                    {
                        StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[2][3]));
                    }
                    break;
                case TaskType.RewardDeny:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[2][4]));
                    break;
                case TaskType.Reward:
                    RewardPanel();
                    break;
                case TaskType.AppointDeny:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[2][7]));
                    break;
                case TaskType.Appoint:
                    city.AppointmentPrefect(doGeneralIds[0]);
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[2][6]));
                    break;
                case TaskType.Lack:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[1][2]));
                    break;
                case TaskType.OverReclaim:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[3][4]));
                    break;
                case TaskType.OverMercantile:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[3][5]));
                    break;
                case TaskType.OverTame:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[3][6]));
                    break;
                case TaskType.OverPatrol:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[3][7]));
                    break;
                case TaskType.OverTruce:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[1][3]));
                    break;
                case TaskType.TruceSelect:
                    TrucePanel();
                    break;
                case TaskType.Truce:
                    short otherKingId = CountryListCache.GetCountryByCountryId(playerCountryId).countryKingId;
                    bool isTruce = CountryListCache.GetCountryByCountryId(playerCountryId).Truce(targetCountryId);
                    SubPlayerOrder();
                    StartCoroutine(isTruce
                        ? uiTips.ShowHeadTips(otherKingId, DoThingsResultInfo[1][0])
                        : uiTips.ShowHeadTips(otherKingId, DoThingsResultInfo[1][1]));
                    break;
                case TaskType.Alienate:
                    SubPlayerOrder();
                    if (GeneralListCache.IsAlienate(doGeneralIds[0], targetGeneralIds[0]))
                    {
                        StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[5][0]));
                    }
                    else 
                    {
                        StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[5][1]));
                    }
                    break;
                case TaskType.Bribe:
                    SubPlayerOrder();
                    if (GeneralListCache.IsBribe(doCityId, targetCityId, doGeneralIds[0], targetGeneralIds[0]))
                    {
                        StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[5][2]));
                    }
                    else
                    {
                        StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[5][3]));
                    }
                    break;
                case TaskType.LackShop:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[7][0]));
                    break;
                case TaskType.LackSmithy:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[7][1]));
                    break;
                case TaskType.LackSchool:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[7][2]));
                    break;
                case TaskType.Shop:
                    EnableCancelButton();
                    storePanel.GoStore += OnStorePanelBuy;
                    storePanel.Store(city.GetFood(), city.GetMoney());
                    break;
                case TaskType.Smithy:
                    EnableCancelButton();
                    smithyPanel.GoSmithy += OnSmithyPanelBuy;
                    smithyPanel.Smithy(doCityId, city.GetMoney(), false , doGeneralIds[0]);
                    break;
                case TaskType.SchoolDeny:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[7][6]));
                    break;
                case TaskType.School:
                    SubPlayerOrder();
                    GeneralListCache.GetGeneral(doGeneralIds[0]).StudyUp();
                    StartCoroutine(uiTips.ShowHeadTips(doGeneralIds[0], DoThingsResultInfo[4][3]));
                    break;
                case TaskType.LackHospital:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[7][3]));
                    break;
                case TaskType.HospitalDeny:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[7][7]));
                    break;
                case TaskType.Hospital:
                    SubPlayerOrder();
                    GeneralListCache.GetGeneral(doGeneralIds[0]).SetCurPhysical(100);
                    city.SubGold(100);  // 从城市的金钱中扣除 100
                    StartCoroutine(uiTips.ShowHeadTips(doGeneralIds[0], DoThingsResultInfo[4][4]));
                    break;
                case TaskType.Settings:
                    Settings();
                    break;
                case TaskType.Save:
                    recordPanel.RecordIndex += SaveRecord;
                    recordPanel.Save();
                    break;
                case TaskType.Load:
                    recordPanel.RecordIndex += LoadRecord;
                    recordPanel.Load();
                    break;
                default:
                    // 初始化结果面板为隐藏状态
                    InteriorPanel();
                    break;
            }
            

        }

        


        void ToggleValueChanged(bool isOn)
        {
            UseInfo.text = "";
            if (useTreasure.isOn)
            {
                UseInfo.text = "赏赐宝物一件";
                confirmButton.SetActive(true);
                confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            }
            else if (useMoney.isOn)
            {
                UseInfo.text = "赏赐黄金百两";
                confirmButton.SetActive(true);
                confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            }
        }

        void EnableCancelButton()
        {
            cancelButton.SetActive(true);
            cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
            cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
        }

        /// <summary>
        /// 点击确认按钮
        /// </summary>
        void OnConfirmButtonClicked()
        {
            City city = CityListCache.GetCityByCityId(doCityId);
            confirmButton.SetActive(false);
            cancelButton.SetActive(false);
            switch (Task)
            {
                case TaskType.Attack:
                    StartCoroutine(ConfirmWar());
                    break;
                case TaskType.Transport:
                    if (city.GetFood() >= short.Parse(transportFood.text) && city.GetMoney() >= short.Parse(transportMoney.text) && city.treasureNum >= byte.Parse(transportTreasure.text))
                    {
                        CityListCache.TransportBetweenCitys(doCityId, targetCityId, short.Parse(transportFood.text), short.Parse(transportMoney.text), byte.Parse(transportTreasure.text));
                        bool hasQiangYun = false;//特技抢运
                        foreach (var id in city.GetOfficerIds())
                        {
                            if (GeneralListCache.GetGeneral(id).HasSkill(4, 9))
                            {
                                hasQiangYun = true;
                                break;
                            }
                        }

                        if (!hasQiangYun)
                        {
                            SubPlayerOrder();
                        }
                        StartCoroutine(uiTips.ShowHeadTips(city.prefectId, DoThingsResultInfo[0][0]));
                    }
                    break;
                case TaskType.Reward:
                    SubPlayerOrder();
                    city.Reward(doGeneralIds[0], useTreasure.isOn);
                    StartCoroutine(uiTips.ShowHeadTips(doGeneralIds[0], DoThingsResultInfo[2][5]));
                    break;
                case TaskType.Reclaim:
                case TaskType.Mercantile:
                case TaskType.Tame:
                case TaskType.Patrol:
                    InteriorResult(doGeneralIds[0], Task);
                    break;
                case TaskType.TruceSelect:
                    optionalGeneralIds = city.GetOfficerIds().ToList();
                    SceneManager.LoadScene("SelectGeneral");
                    break;
                default:
                    StartCoroutine(uiTips.ShowHeadTips(city.prefectId, _useGold.ToString()));
                    break;
            }
        
        }
        /// <summary>
        /// 点击取消按钮
        /// </summary>
        public void GoToNextScene()
        {
            if (playerOrderNum > 0)
            {
                SceneManager.LoadScene("CityScene");
            }
            else
            {
                SceneManager.LoadScene("GlobalScene");
            }
            Debug.Log("返回城市场景");
        }
        
        

        private void Move()
        {
            SubPlayerOrder();
            bool isKingMove = CityListCache.MoveTask(doCityId, targetCityId, targetGeneralIds);
            if (isKingMove)
            {
                City tarCity = CityListCache.GetCityByCityId(targetCityId);
                Debug.Log($"君主:" + string.Join(", ", targetGeneralIds) +"从"+ doCityId +"前往"+targetCityId);
                StartCoroutine(uiTips.ShowHeadTips(tarCity.cityBelongKing, DoThingsResultInfo[0][0]));
            }
            else
            {
                Debug.Log($"将领:" + string.Join(", ", targetGeneralIds) + "从" + doCityId + "前往" + targetCityId);
                StartCoroutine(uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[0][0]));
            }
        }

        void AttackAsset()
        {
            City city = CityListCache.GetCityByCityId(doCityId);
            attackPanel.SetActive(true);
            setFood.onValueChanged.AddListener(FoodInputFieldSubmit);
            setMoney.onValueChanged.AddListener(GoldInputFieldSubmit);

            assetInfo.text = $"{city.cityName}粮食:{city.GetFood()}  金钱:{city.GetMoney()}";

            void FoodInputFieldSubmit(string value)
            {
                if (int.TryParse(value, out int intValue))
                {
                    // 限制在0到10000之间（包括0但不包括10000）
                    if (intValue < 0 || intValue >= 10000)
                    {
                        setFood.text = "0"; // 设置为默认值
                    }
                    else if (intValue > city.GetFood())
                    {
                        setFood.text = $"{city.GetFood()}"; // 设置为城市现有的粮食量
                        Debug.Log($"{city.cityName}将带走{city.GetFood()}粮食");
                    }
                    else
                    {
                        Debug.Log($"{city.cityName}将带走{intValue}粮食");
                    }
                    confirmButton.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                    cancelButton.SetActive(true);
                    cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
                }
            }

            void GoldInputFieldSubmit(string value)
            {
                if (int.TryParse(value, out int intValue))
                {
                    // 限制在0到10000之间（包括0但不包括10000）
                    if (intValue < 0 || intValue >= 10000)
                    {
                        setMoney.text = "0"; // 设置为默认值
                    }
                    else if (intValue > city.GetMoney())
                    {
                        setMoney.text = $"{city.GetMoney()}"; // 设置为城市现有的金钱量
                        Debug.Log($"{city.cityName}将带走{city.GetMoney()}金钱");
                    }
                    else
                    {
                        Debug.Log($"{city.cityName}将带走{intValue}金钱");
                    }
                    confirmButton.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                    cancelButton.SetActive(true);
                    cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
                }
            }
        }

        IEnumerator ConfirmWar()
        {
            attackPanel.SetActive(false);
            short food = short.Parse(setFood.text);
            short money = short.Parse(setMoney.text);
            optionalGeneralIds.Clear();
            optionalGeneralIds.Add(food);
            optionalGeneralIds.Add(money);
            City city = CityListCache.GetCityByCityId(doCityId);
            city.SubFood(food);
            city.SubGold(money);
            yield return uiTips.ShowHeadTips(targetGeneralIds[0], DoThingsResultInfo[0][2]);
            //玩家发起战争数据传递
            PlayingState = GameState.PlayervsAI;
            Task = TaskType.None;
            SceneManager.LoadScene("WarScene");
        }








        void Transport()
        {
            City city = CityListCache.GetCityByCityId(doCityId);
            transportPanel.SetActive(true);
            transportFood.onValueChanged.AddListener(FoodInputFieldSubmit);
            transportMoney.onValueChanged.AddListener(GoldInputFieldSubmit);
            transportTreasure.onValueChanged.AddListener(TreasureInputFieldSubmit);

            transportInfo.text = $"粮食:{city.GetFood()} 金钱:{city.GetMoney()} 宝物:{city.treasureNum}";

            void FoodInputFieldSubmit(string value)
            {
                if (int.TryParse(value, out int intValue))
                {
                    // 限制在0到10000之间（包括0但不包括10000）
                    if (intValue < 0 || intValue >= 10000)
                    {
                        transportFood.text = "0"; // 设置为默认值
                    }
                    else if (intValue > city.GetFood())
                    {
                        transportFood.text = $"{city.GetFood()}"; // 设置为城市现有的粮食量
                        Debug.Log($"{city.cityName}将消耗{city.GetFood()}粮食");
                    }
                    else
                    {
                        Debug.Log($"{city.cityName}将消耗{intValue}粮食");
                    }
                    confirmButton.SetActive(true);
                    cancelButton.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                    cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
                }
            }

            void GoldInputFieldSubmit(string value)
            {
                if (int.TryParse(value, out int intValue))
                {
                    // 限制在0到10000之间（包括0但不包括10000）
                    if (intValue < 0 || intValue >= 10000)
                    {
                        transportMoney.text = "0"; // 设置为默认值
                    }
                    else if (intValue > city.GetMoney())
                    {
                        transportMoney.text = $"{city.GetMoney()}"; // 设置为城市现有的金钱量
                        Debug.Log($"{city.cityName}将消耗{city.GetMoney()}金钱");
                    }
                    else
                    {
                        Debug.Log($"{city.cityName}将消耗{intValue}金钱");
                    }
                    confirmButton.SetActive(true);
                    cancelButton.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                    cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
                }
            }

            void TreasureInputFieldSubmit(string value)
            {
                if (int.TryParse(value, out int intValue))
                {
                    // 限制在0到100之间（包括0但不包括100）
                    if (intValue < 0 || intValue >= 100)
                    {
                        transportTreasure.text = "0"; // 设置为默认值
                    }
                    else if (intValue > city.treasureNum)
                    {
                        transportTreasure.text = $"{city.treasureNum}"; // 设置为城市现有的金钱量
                        Debug.Log($"{city.cityName}将消耗{city.treasureNum}宝物");
                    }
                    else
                    {
                        Debug.Log($"{city.cityName}将消耗{intValue}宝物");
                    }
                    confirmButton.SetActive(true);
                    cancelButton.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                    cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
                }
            }
        }

        void RewardPanel()
        {
            InteriorPanel();
            rewardPanel.SetActive(true);
            useTreasure.onValueChanged.AddListener(delegate { ToggleValueChanged(useTreasure); });
            useMoney.onValueChanged.AddListener(delegate { ToggleValueChanged(useMoney); });
        }


        void InteriorPanel()
        {
            City city = CityListCache.GetCityByCityId(doCityId);
            General general = GeneralListCache.GetGeneral(doGeneralIds[0]);
            interiorPanel.SetActive(true);
            confirmButton.SetActive(true);
            cancelButton.SetActive(true);
            confirmButton.GetComponent<Button>().onClick.RemoveAllListeners();
            confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
            cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);
            title.text = $"{TextLibrary.GetTaskDescription(Task)}";
            HeadImage.texture = Resources.Load<Texture2D>($"HeadImage/{doGeneralIds[0]}");
            SelectName.text= general.generalName;
            _useGold = general.GetNeedMoneyOfInterior(Task);
            UseInfo.text = $"耗费金钱:{_useGold}";

            switch(Task)
            { 
                case TaskType.Reward:
                    Info1.text = $"忠诚：{general.loyalty}";
                    Info2.text = $"宝物：{city.treasureNum}";
                    UseInfo.text = "选择赏赐百金或宝物";
                    if (city.treasureNum <= 0)
                        useTreasure.interactable = false;
                    if (city.GetMoney() < 100)
                        useMoney.interactable = false;
                    break;
                case TaskType.Reclaim:
                    Info1.text = $"农业：{city.agro}";
                    Info2.text = $"粮食：{city.GetFood()}";
                    break;
                case TaskType.Mercantile:
                    Info1.text = $"商业：{city.trade}";
                    Info2.text = $"金钱：{city.GetMoney()}";
                    break;
                case TaskType.Tame:
                    Info1.text = $"统治：{city.rule}";
                    Info2.text = $"防灾：{city.floodControl}";
                    break;
                case TaskType.Patrol:
                    Info1.text = $"统治：{city.rule}";
                    Info2.text = $"人口:{city.population}";
                    break;
            }
        }

        void InteriorResult(short generalId, TaskType task)
        {
            confirmButton.SetActive(false);
            cancelButton.SetActive(false);
            City city = CityListCache.GetCityByCityId(doCityId);
            General general = GeneralListCache.GetGeneral(generalId);
            SubPlayerOrder();
            switch (task)
            {
                case TaskType.Reclaim:
                    _useGold = city.Reclaim(general, _useGold);
                    StartCoroutine(uiTips.ShowTaskTips(DoThingsResultInfo[3][0] + _useGold, Task));
                    break;
                case TaskType.Mercantile:
                    _useGold = city.Mercantile(general, _useGold);
                    StartCoroutine(uiTips.ShowTaskTips(DoThingsResultInfo[3][1] + _useGold, Task));
                    break;
                case TaskType.Tame:
                    _useGold = city.Tame(general, _useGold);
                    StartCoroutine(uiTips.ShowTaskTips(DoThingsResultInfo[3][2] + _useGold, Task));
                    break;
                case TaskType.Patrol:
                    _useGold = city.Patrol(general, _useGold);
                    StartCoroutine(uiTips.ShowTaskTips(DoThingsResultInfo[3][3] + _useGold, Task));
                    break;
            }
        }


        void TrucePanel()
        {
            trucePanel.SetActive(true);

            Transform truceArea = trucePanel.GetComponent<Transform>();
            RawImage HeadImage = truceArea.Find("HeadImage").GetComponent<RawImage>();
            TextMeshProUGUI SelectName = truceArea.Find("SelectName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI Info1 = truceArea.Find("Info1").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI Info2 = truceArea.Find("Info2").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI UseInfo = truceArea.Find("UseInfo").GetComponent<TextMeshProUGUI>();

            Button PreviousButton = truceArea.Find("PreviousButton").GetComponent<Button>();
            Button NextButton = truceArea.Find("NextButton").GetComponent<Button>();

            Country country = CountryListCache.GetCountryByCountryId(playerCountryId);
            byte[] canTruceCountryIds = country.GetNoCountryIdAllianceCountryIdArray();
            Debug.Log(canTruceCountryIds);
            int currentIndex = 0;
            ShowCountryInfo(canTruceCountryIds[0]);

            // 添加按钮的监听器，只添加一次
            PreviousButton.onClick.AddListener(ShowPreviousCountry);
            NextButton.onClick.AddListener(ShowNextCountry);

            confirmButton.SetActive(true);
            cancelButton.SetActive(true);
            confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.GetComponent<Button>().onClick.AddListener(GoToNextScene);

            void ShowCountryInfo(byte tarCountryId)
            {
                Country targetCountry = CountryListCache.GetCountryByCountryId(tarCountryId);
                General general = GeneralListCache.GetGeneral(targetCountry.countryKingId);

                HeadImage.texture = Resources.Load<Texture2D>($"HeadImage/{general.generalId}");
                SelectName.text = general.generalName;
                Info1.text = $"城池：{targetCountry.GetHaveCityNum()}";
                Info2.text = $"相性：{general.phase}";
                UseInfo.text = "要与其停战吗？";
                targetCountryId = tarCountryId;
            }

            void ShowPreviousCountry()
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                    UpdateCountryInfo();
                    UpdateButtonStates();
                }
            }

            void ShowNextCountry()
            {
                if (currentIndex < canTruceCountryIds.Length - 1)
                {
                    currentIndex++;
                    UpdateCountryInfo();
                    UpdateButtonStates();
                }
            }

            void UpdateCountryInfo()
            {
                if (canTruceCountryIds.Length == 0) return;

                byte targetCountryId = canTruceCountryIds[currentIndex];
                ShowCountryInfo(targetCountryId);
            }

            void UpdateButtonStates()
            {
                PreviousButton.interactable = currentIndex > 0;
                NextButton.interactable = currentIndex < canTruceCountryIds.Length - 1;
            }
        }
        
        private void OnStorePanelBuy(short food, short gold)
        {
            storePanel.GoStore -= OnStorePanelBuy;
            City city = CityListCache.GetCityByCityId(doCityId);
            city.SetFood(food);
            city.SetMoney(gold);
            SubPlayerOrder();
            GoToNextScene();
        }

        private void OnSmithyPanelBuy(short gold, string text)
        {
            smithyPanel.GoSmithy -= OnSmithyPanelBuy;
            City city = CityListCache.GetCityByCityId(doCityId);
            city.SetMoney(gold);
            StartCoroutine(uiTips.ShowHeadTips(doGeneralIds[0], text));
            SubPlayerOrder();
            GoToNextScene();
        }

        

        

    
        // 存档
        void SaveRecord(byte index)
        {
            if (index != 0)
            {
                DataManagement.SaveGame(index - 1);
            }
            recordPanel.RecordIndex -= SaveRecord;
            GoToNextScene();
        }
        //载入存档方法
        void LoadRecord(byte index)
        {
            if (index != 0)
            {
                DataManagement.Instance.LoadGame(index - 1);
            }
            recordPanel.RecordIndex -= LoadRecord;
            GoToNextScene();
        }

        void Settings()
        {
            settingsPanel.SetActive(true);
            cancelButton.SetActive(true);
            cancelButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                settingsPanel.SetActive(false);
                GoToNextScene();
            });
        }
    }
}
