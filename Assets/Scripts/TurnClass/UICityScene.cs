using System;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DataClass.GameInfo;


namespace TurnClass
{
    public class UICityScene : MonoBehaviour
    {
        [SerializeField] private GameObject cityPanel;
        [SerializeField] private TextMeshProUGUI timeTitle;
        [SerializeField] private TextMeshProUGUI cityName;
        [SerializeField] private TextMeshProUGUI king; 
        [SerializeField] private TextMeshProUGUI prefect;
        [SerializeField] private TextMeshProUGUI rule;
        [SerializeField] private RawImage head;
        [SerializeField] private Text order;
        [SerializeField] private Text population;
        [SerializeField] private Text generalNum; 
        [SerializeField] private Text citySoldier;
        [SerializeField] private Text agro;
        [SerializeField] private Text trade;
        [SerializeField] private Text flood;
        [SerializeField] private Text gold;
        [SerializeField] private Text food;
        [SerializeField] private Text treasure;

        [SerializeField] private Button globalButton;//返回全国地图
        [SerializeField] private GameObject firstMenu;
    
        
        [SerializeField] private Button moveButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button conscriptButton;
        [SerializeField] private Button transportButton;
    
        [SerializeField] private Button searchButton;
        [SerializeField] private Button employButton;
        [SerializeField] private Button rewardButton;
        [SerializeField] private Button appointButton;
    
        [SerializeField] private Button reclaimButton;
        [SerializeField] private Button mercantileButton;
        [SerializeField] private Button tameButton;
        [SerializeField] private Button patrolButton;
    
        [SerializeField] private Button truceButton;
        [SerializeField] private Button alienateButton;
        [SerializeField] private Button bribeButton;
        [SerializeField] private Button intelligenceButton;
    
        [SerializeField] private Button shopButton;
        [SerializeField] private Button smithyButton;
        [SerializeField] private Button schoolButton;
        [SerializeField] private Button hospitalButton;
    
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
    
        [SerializeField] private Button previousButton;//上一个将领
        [SerializeField] private Button nextButton;//下一个将领
        [SerializeField] private Toggle infoToggle;
        [SerializeField] private Button cancelButton;//情报查看取消
        [SerializeField] private UIIntelligencePanel intelligencePanel;// 情报查看面板
        private City _city;
        private int _curGenIndex;
        void Start()
        {
            DataManagement.Instance.LoadAndInitializeData();
            timeTitle.text = $"{years}年{month}月";
            doGeneralIds.Clear();
            optionalGeneralIds.Clear();
            targetGeneralIds.Clear();

            InitializeCityPanel();
        }

        void InitializeCityPanel()
        {
            globalButton.onClick.AddListener(OnGlobalButtonOnClick);

            if (Task == TaskType.Intelligence)
            {
                _city = CityListCache.GetCityByCityId(targetCityId);
                ShowCityInfo();
                firstMenu.SetActive(false);
                if (_city.GetCityOfficerNum() != 0)
                {
                    infoToggle.gameObject.SetActive(true);
                    infoToggle.onValueChanged.AddListener(delegate {OnInfoToggleChanged(infoToggle.isOn);});
                }
                cancelButton.gameObject.SetActive(true);
                cancelButton.onClick.AddListener(OnCancelButtonClick);
            }
            else
            {
                Task = TaskType.None;
                _city = CityListCache.GetCityByCityId(doCityId);
                ShowCityInfo();
                moveButton.onClick.AddListener(OnMoveButtonClick);
                attackButton.onClick.AddListener(OnAttackButtonClick);
                conscriptButton.onClick.AddListener(OnConscriptButtonClick);
                transportButton.onClick.AddListener(OnTransportButtonClick);
            
                searchButton.onClick.AddListener(OnSearchButtonClick);
                employButton.onClick.AddListener(OnEmployButtonClick);
                rewardButton.onClick.AddListener(OnRewardButtonClick);
                appointButton.onClick.AddListener(OnAppointButtonClick);

                reclaimButton.onClick.AddListener(OnReclaimButtonClick);
                mercantileButton.onClick.AddListener(OnMercantileButtonClick);
                tameButton.onClick.AddListener(OnTameButtonClick);
                patrolButton.onClick.AddListener(OnPatrolButtonClick);

                truceButton.onClick.AddListener(OnTruceButtonClick);
                alienateButton.onClick.AddListener(OnAlienateButtonClick);
                bribeButton.onClick.AddListener(OnBribeButtonClick);
                intelligenceButton.onClick.AddListener(OnIntelligenceButtonClick);

                shopButton.onClick.AddListener(OnShopButtonClick);
                smithyButton.onClick.AddListener(OnSmithyButtonClick);
                schoolButton.onClick.AddListener(OnSchoolButtonClick);
                hospitalButton.onClick.AddListener(OnHospitalButtonClick);

                saveButton.onClick.AddListener(OnSaveButtonClick);
                loadButton.onClick.AddListener(OnLoadButtonClick);
                settingsButton.onClick.AddListener(OnSettingsButtonClick);
                //ExitButton.onClick.AddListener(OnExitButtonClick);
            }
            System.GC.Collect(); // 强制进行垃圾回收
        }

        void ShowCityInfo()
        {
            if (_city.cityBelongKing == 0)
            {
                head.texture = Resources.Load<Texture2D>($"HeadImage/0");
                prefect.text = "太守：无";
                king.text = "君主：无";
            }
            else
            {
                if (_city.cityBelongKing == CountryListCache.GetCountryByCountryId(playerCountryId).countryKingId)
                {
                    order.text = playerOrderNum.ToString();
                }
                else
                {
                    order.text = "?";
                }
                head.texture = Resources.Load<Texture2D>($"HeadImage/{_city.prefectId}");
                prefect.text = $"太守：{GeneralListCache.GetGeneral(_city.prefectId).generalName}";
                king.text = $"君主：{GeneralListCache.GetGeneral(_city.cityBelongKing).generalName}";
            }
            cityName.text = $"【{_city.cityName}】";
            rule.text = $"統治：{_city.rule}";
            population.text = _city.population.ToString();
            generalNum.text = _city.GetCityOfficerNum().ToString();
            citySoldier.text = _city.GetCityAllSoldierNum().ToString();
            agro.text = _city.agro.ToString();
            trade.text = _city.trade.ToString();
            flood.text = _city.floodControl.ToString();
            gold.text = _city.GetMoney().ToString();
            food.text = _city.GetFood().ToString();
            treasure.text = _city.treasureNum.ToString();
        }

        //返回全国城市界面
        void OnGlobalButtonOnClick()
        {
            SceneManager.LoadScene("GlobalScene");
        }
        
        //情报探查显示将领信息
        void OnInfoToggleChanged(bool isOn)
        {
            if (isOn)
            {
                cityPanel.SetActive(false);
                firstMenu.SetActive(false);
                previousButton.gameObject.SetActive(true);
                previousButton.onClick.RemoveAllListeners();
                previousButton.onClick.AddListener(ShowPreviousGeneral);
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextGeneral);
                infoToggle.gameObject.GetComponentInChildren<Text>().text = "城池";
                optionalGeneralIds.Clear();
                optionalGeneralIds = CityListCache.GetCityByCityId(targetCityId).GetOfficerIds().ToList();
                intelligencePanel.ShowIntelligencePanel(optionalGeneralIds[_curGenIndex]);
            }
            else
            {
                previousButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                infoToggle.gameObject.GetComponentInChildren<Text>().text = "武将";
                intelligencePanel.gameObject.SetActive(false);
                cityPanel.SetActive(true);
                ShowCityInfo();
            }
        }
        void UpdateButtonStates()
        {
            previousButton.interactable = _curGenIndex > 0;
            nextButton.interactable = _curGenIndex < optionalGeneralIds.Count - 1;
        }

        void ShowPreviousGeneral()
        {
            if (_curGenIndex > 0)
            {
                _curGenIndex--;
                intelligencePanel.ShowIntelligencePanel(optionalGeneralIds[_curGenIndex]);
                UpdateButtonStates();
            }
        }

        void ShowNextGeneral()
        {
            if (_curGenIndex < optionalGeneralIds.Count - 1)
            {
                _curGenIndex++;
                intelligencePanel.ShowIntelligencePanel(optionalGeneralIds[_curGenIndex]);
                UpdateButtonStates();
            }
        }
        //情报取消
        void OnCancelButtonClick()
        {
            Task = TaskType.None;
            SceneManager.LoadScene("GlobalScene");
        }
        
        //移动
        void OnMoveButtonClick()
        {
            Task = TaskType.Move;
            SceneManager.LoadScene("GlobalScene");
        }
        //攻城
        void OnAttackButtonClick()
        {
            if (attackCount <= 2)
            {
                attackCount++;
                Task = TaskType.Attack;
                SceneManager.LoadScene("GlobalScene");
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
            SceneManager.LoadScene("GlobalScene");
        }
        
        // 当搜索按钮被点击时执行搜索检测
        void OnSearchButtonClick()
        {
            Task = TaskType.Search;
            optionalGeneralIds = CityListCache.GetCityByCityId(doCityId).GetOfficerIds().ToList();
            SceneManager.LoadScene("SelectGeneral");
        }

        void OnEmployButtonClick()
        {
            if (_city.GetCityOfficerNum() == 10) // 如果城市中的将军数量为 10
            {
                Task = TaskType.OverEmploy;
                doGeneralIds.Add(_city.prefectId);
                SceneManager.LoadScene("ExecutivePanel");
                return;
            }
            optionalGeneralIds = _city.GetTalentIds(); // 获取在野将军 ID 数组
            optionalGeneralIds.AddRange(_city.cityJailGeneralId); // 获取在押将军 ID 数组
            if (optionalGeneralIds.Count == 0) // 如果可以登用的武将数量为 0
            {
                Task = TaskType.EmployNothing;
                doGeneralIds.Add(_city.prefectId);
                SceneManager.LoadScene("ExecutivePanel");
                return;
            }
            Debug.Log("所选将领ID：" + string.Join(", ", _city.GetCityNotFoundGeneralIdArray()));
            Task = TaskType.Employ;
            SceneManager.LoadScene("SelectGeneral");
        }

        void OnRewardButtonClick()
        {
            List<short> officeGeneralIdArray = _city.GetOfficerIds().ToList(); // 获取城市中的将军 ID 数组
            List<short> canRewardGeneralIds = (from t in officeGeneralIdArray 
                select GeneralListCache.GetGeneral(t) into general where general.GetLoyalty() < 99 select general.generalId).ToList();
            
            if (canRewardGeneralIds.Count > 0)
            {
                Task = TaskType.Reward;
                optionalGeneralIds.AddRange(canRewardGeneralIds);
                SceneManager.LoadScene("SelectGeneral");
            }
            else
            {
                Task = TaskType.RewardDeny;
                SceneManager.LoadScene("ExecutivePanel");
            }
        }

        void OnAppointButtonClick()
        {
            Task = TaskType.Reward;
            if (_city.prefectId == _city.cityBelongKing)
            {
                Task = TaskType.AppointDeny;
                doGeneralIds.Add(_city.prefectId);
                SceneManager.LoadScene("ExecutivePanel");
            }
            else
            {
                Task = TaskType.Appoint;
                optionalGeneralIds = _city.GetOfficerIds().ToList(); // 获取城市中的将军 ID 数组
                optionalGeneralIds.Remove(_city.prefectId);
                SceneManager.LoadScene("SelectGeneral");
            }
        }

        

        // 内政通用方法处理按钮点击
        void UniversalInterior(TaskType taskType, int maxValue, Func<City, int> propertySelector)
        {
            // 检查是否有足够的金钱
            if (_city.GetMoney() == 0) // 如果目标城市的金钱为 0
            {
                Task = TaskType.Lack;
                doGeneralIds.Add(_city.prefectId);
                SceneManager.LoadScene("ExecutivePanel");
            }

            int propertyValue = propertySelector(_city); // 使用传入的选择器函数获取属性值

            if (propertyValue == maxValue) // 检查城市特定属性值
            {
                Task = (TaskType)Enum.Parse(typeof(TaskType), "Over" + taskType);
                doGeneralIds.Add(_city.prefectId);
                SceneManager.LoadScene("ExecutivePanel");
            }
            else
            {
                Task = taskType;
                optionalGeneralIds =_city.GetOfficerIds().ToList();
                SceneManager.LoadScene("SelectGeneral"); // 下一个选择武将场景
            }
        }

        void UniversalMarketCheck(TaskType taskType, Func<City, bool> propertySelector)
        {
            if (taskType == TaskType.School || taskType == TaskType.Hospital)
            {
                // 检查是否有足够的金钱
                if (_city.GetMoney() < 100) // 如果目标城市的金钱为 0
                {
                    Task = TaskType.Lack;
                    SceneManager.LoadScene("ExecutivePanel");              
                }
            }

            bool propertyValue = propertySelector(_city); // 使用传入的选择器函数获取属性值

            if (!propertyValue) // 检查城市特定属性值
            {
                Task = (TaskType)Enum.Parse(typeof(TaskType), "Lack"+taskType);
                SceneManager.LoadScene("ExecutivePanel");
            }
            else
            {
                switch(taskType)
                { 
                    case TaskType.Shop:
                        Task = taskType;
                        optionalGeneralIds.Clear();
                        optionalGeneralIds = _city.GetOfficerIds().ToList();
                        SceneManager.LoadScene("ExecutivePanel"); // 下一个选择武将场景
                        break;
                    case TaskType.Smithy:
                        Task = taskType;
                        optionalGeneralIds.Clear();
                        optionalGeneralIds = _city.GetOfficerIds().ToList();
                        SceneManager.LoadScene("SelectGeneral"); // 下一个选择武将场景
                        break;
                    case TaskType.School:
                        List<short> canStudyGeneralIds = _city.GetCanStudyGeneralIds();
                        if (canStudyGeneralIds.Count > 0)
                        {
                            Task = taskType;
                            optionalGeneralIds.Clear();
                            optionalGeneralIds.AddRange(canStudyGeneralIds);
                            SceneManager.LoadScene("SelectGeneral"); // 下一个选择武将场景
                        }
                        else
                        {
                            Task = TaskType.SchoolDeny;
                            SceneManager.LoadScene("ExecutivePanel"); 
                        }
                        break;
                    case TaskType.Hospital:
                        List<short> canTreatGeneralIds = _city.GetThresholdGeneralIds(100);
                        if (canTreatGeneralIds.Count > 0)
                        {
                            Task = taskType;
                            optionalGeneralIds.Clear();
                            optionalGeneralIds.AddRange(canTreatGeneralIds);
                            SceneManager.LoadScene("SelectGeneral");// 下一个选择武将场景}
                        }
                        else 
                        {
                            Task = TaskType.HospitalDeny;
                            SceneManager.LoadScene("ExecutivePanel");
                        }
                        break;
                }
            }
        }   

        // 当开垦按钮被点击时执行开垦检测 
        public void OnReclaimButtonClick()
        {
            UniversalInterior(TaskType.Reclaim,999, city => city.agro);
        }

        // 当劝商按钮被点击时执行劝商检测
        public void OnMercantileButtonClick()
        {
            UniversalInterior(TaskType.Mercantile,999, city => city.trade);
        }

        // 当治水按钮被点击时执行治水检测
        public void OnTameButtonClick()
        {
            UniversalInterior(TaskType.Tame,99, city => city.floodControl);
        }

        // 当巡查按钮被点击时执行巡查检测
        public void OnPatrolButtonClick()
        {
            UniversalInterior(TaskType.Patrol,999999, city => city.population);
        }

        
        void OnTruceButtonClick()
        {
            Country country = CountryListCache.GetCountryByCountryId(playerCountryId);
            if (country.GetAllianceSize() < CountryListCache.countryDictionary.Count)
            {
                Task = TaskType.TruceSelect;
            }
            else { Task = TaskType.OverTruce; }
            SceneManager.LoadScene("ExecutivePanel");
        }

        void OnAlienateButtonClick()
        {
            Task=TaskType.Alienate;
            SceneManager.LoadScene("GlobalScene");
        }

        void OnBribeButtonClick()
        {
            Task = TaskType.Bribe;
            if (_city.GetCityOfficerNum() == 10) // 如果城市中的将军数量为 10
            {
                Task = TaskType.OverEmploy;
                doGeneralIds.Add(_city.prefectId);
                SceneManager.LoadScene("ExecutivePanel");
                return;
            }
            SceneManager.LoadScene("GlobalScene");
        }

        void OnIntelligenceButtonClick()
        {
            Task = TaskType.Intelligence;
            optionalGeneralIds = _city.GetOfficerIds().ToList();
            SceneManager.LoadScene("GlobalScene");
        }

        // 当钱粮按钮被点击时执行钱粮检测
        void OnShopButtonClick()
        {
            UniversalMarketCheck(TaskType.Shop, city => city.cityGrainShop);
        }

        void OnSmithyButtonClick()
        {
            UniversalMarketCheck(TaskType.Smithy, city => city.HaveCitySmithy());
        }
        void OnSchoolButtonClick()
        {
            UniversalMarketCheck(TaskType.School, city => city.citySchool);
        }

        void OnHospitalButtonClick()
        {
            UniversalMarketCheck(TaskType.Hospital, city => city.cityHospital);
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
}


