using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DataClass.GameInfo;

namespace TurnClass
{
    public class UISelectGeneral : MonoBehaviour
    {

        [SerializeField] GameObject generalTogglePrefab;  // 预制件按钮
        [SerializeField] Transform generalRoom;  // 按钮生成的父物体 (GeneralListPanel)
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [SerializeField] TextMeshProUGUI info;

        [SerializeField] GameObject abandonPanel;//放弃城池界面
        [SerializeField] Button abandonButton;
        [SerializeField] Button stopButton;

        // 最大将领选择数（单选和多选任务的区分）
        // 设置颜色
        Color selectedColor = Color.red;  // 选中时的颜色
        Color normalColor = Color.white;  // 未选中时的颜色
        bool secondConfirm = false;// 是否为第二次确认选择武将
        bool _singleSelect = true;//可选多个将领标记
        byte maxSelectableGenerals = 10;
        short[] alternativeprefectIds;


        void Start()
        {
            InitializeSelection();
            confirmButton.gameObject.SetActive(false);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);

        }

        // 初始化选择界面
        void InitializeSelection()
        {
            // 根据任务类型设置可选择的将领数量
            switch (Task)
            {
                case TaskType.Move:
                case TaskType.Attack:
                    if (!secondConfirm) // 第一次选将用于移动 
                    {
                        _singleSelect = false;
                        optionalGeneralIds.Clear();
                        optionalGeneralIds = CityListCache.GetCityByCityId(doCityId).GetOfficerIds().ToList();
                        CreateGeneralSelectionToggles(_singleSelect);
                    }
                    else // 第二次选将用于任命太守
                    {
                        _singleSelect = true;
                        info.text = "任命新太守";
                        var cityGeneralIds = CityListCache.GetCityByCityId(doCityId).GetOfficerIds().ToList();
                        optionalGeneralIds = cityGeneralIds.Except(targetGeneralIds).ToList();
                        CreateGeneralSelectionToggles(_singleSelect); // 选择其他人任太守
                    }
                    break;
                case TaskType.SelfBuild:
                    if (!secondConfirm) // 第一次选将选全部武将
                    {
                        _singleSelect = false;
                        info.text = "选择新势力所有人物";
                        CreateGeneralSelectionToggles(_singleSelect);
                    }
                    else // 第二次选将用于任命君主
                    {
                        _singleSelect = true;
                        info.text = "选择君主人物";
                        CreateGeneralSelectionToggles(_singleSelect); // 选择其他人任太守
                    }
                    break;
                case TaskType.SelfRemove:
                    _singleSelect = false;
                    info.text = "选择删除的武将";
                    CreateGeneralSelectionToggles(_singleSelect);
                    break;
                default:
                    _singleSelect = true;
                    info.text = "选择武将";
                    CreateGeneralSelectionToggles(_singleSelect);
                    break;
            }

        
        }

        //生成将领选择界面的选项
        void CreateGeneralSelectionToggles(bool isSingleSelect)
        {
            doGeneralIds.Clear();
            // 清除上次生成的 toggles
            foreach (Transform child in generalRoom)
            {
                Destroy(child.gameObject);
            }

            int generalNum = optionalGeneralIds.Count;

            for (int i = 0; i < generalNum; i++)
            {
                short generalId = optionalGeneralIds[i];
                General general = GeneralListCache.GetGeneral(generalId);

                // 实例化新 toggle
                GameObject toggleObj = Instantiate(generalTogglePrefab, generalRoom.transform);
                Toggle toggle = toggleObj.GetComponent<Toggle>();

                // 如果是单选模式，则将 Toggle 添加到 ToggleGroup
                if (isSingleSelect)
                {
                    ToggleGroup toggleGroup = generalRoom.GetComponent<ToggleGroup>();
                    toggle.group = toggleGroup; // 将 Toggle 添加到 Toggle Group
                }

                // 缓存 TextMeshPro 组件
                TextMeshProUGUI[] textMeshes = toggleObj.GetComponentsInChildren<TextMeshProUGUI>();
                textMeshes[0].text = general.generalName;
                textMeshes[1].text = general.level.ToString();
                textMeshes[2].text = general.curPhysical.ToString();
                textMeshes[3].text = general.phase.ToString();
                textMeshes[4].text = general.lead.ToString();
                textMeshes[5].text = general.force.ToString();
                textMeshes[6].text = general.IQ.ToString();
                textMeshes[7].text = general.political.ToString();
                textMeshes[8].text = general.moral.ToString();
                textMeshes[9].text = general.loyalty == 100 ? "--" : general.loyalty.ToString();

                // 添加监听器
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(delegate {
                    OnToggleValueChanged(toggle, generalId, isSingleSelect);
                });

                // 初始化颜色
                SetTextMeshProColor(normalColor, textMeshes);
            }
        }

        void OnToggleValueChanged(Toggle changedToggle, short generalId, bool isSingleSelect)
        {
            if (changedToggle.isOn)
            {
                if (isSingleSelect)
                {
                    doGeneralIds.Clear();
                    doGeneralIds.Add(generalId);
                    Debug.Log(generalId + "总:" + string.Join(", ", doGeneralIds));
                }
                else
                {
                    if (!doGeneralIds.Contains(generalId) && doGeneralIds.Count < maxSelectableGenerals)
                    {
                        doGeneralIds.Add(generalId);
                        Debug.Log(generalId + "总:"+string.Join(", ", doGeneralIds));
                    }
                }

                // 设置选中时的颜色
                SetTextMeshProColor(selectedColor, changedToggle.GetComponentsInChildren<TextMeshProUGUI>());
            }
            else
            {
                if (!isSingleSelect)
                {
                    doGeneralIds.Remove(generalId);
                    Debug.Log(generalId + "总:"+string.Join(", ", doGeneralIds));
                }

                // 设置未选中时的颜色
                SetTextMeshProColor(normalColor, changedToggle.GetComponentsInChildren<TextMeshProUGUI>());
            }

            // 检查是否至少有一个 Toggle 被选中
            CheckToggleStates();
        }

        // 检查所有 Toggle 的状态
        void CheckToggleStates()
        {
            bool hasSelectedToggle = false;
            foreach (Transform child in generalRoom)
            {
                Toggle toggle = child.GetComponent<Toggle>();
                if (toggle.isOn)
                {
                    hasSelectedToggle = true;
                    break;
                }
            }

            confirmButton.gameObject.SetActive(hasSelectedToggle);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        // 设置颜色的函数
        void SetTextMeshProColor(Color color, TextMeshProUGUI[] textMeshes)
        {
            foreach (var textMesh in textMeshes)
            {
                textMesh.color = color;
            }
        }




        void OnConfirmButtonClicked()
        {
            if (doGeneralIds.Count != 0)
            {
                switch (Task)
                {
                    case TaskType.Move:
                        City city = CityListCache.GetCityByCityId(doCityId);
                        City targetCity = CityListCache.GetCityByCityId(targetCityId);
                        if (!secondConfirm)//第一次选将确认
                        {
                            Debug.Log("判断超出，太守" + city.prefectId + "和" + string.Join(", ", doGeneralIds));
                            if (doGeneralIds.Count + targetCity.GetCityOfficerNum() <= 10)//移动后目标城将领数量不大于10
                            {
                                Debug.Log("判断全员太守" + city.prefectId + "和" + string.Join(", ", doGeneralIds));
                                if (doGeneralIds.Count == city.GetCityOfficerNum())//移走了出发城全部将领
                                {
                                    Debug.Log("判断包含太守" + city.prefectId + "和" + string.Join(", ", doGeneralIds));
                                    targetGeneralIds.Clear();
                                    targetGeneralIds.AddRange(doGeneralIds);
                                    targetGeneralIds.Remove(city.prefectId);
                                    targetGeneralIds.Insert(0, city.prefectId);
                                    abandonPanel.SetActive(true);
                                    abandonButton.onClick.AddListener(OnAbandonButtonClicked);
                                    stopButton.onClick.AddListener(OnStopButtonClicked);
                                    Debug.Log("所有武将都已被选中！");
                                }
                                else if (doGeneralIds.Contains(city.prefectId))//移动的将领包含太守,需要任命新太守
                                {
                                    secondConfirm = true;
                                    targetGeneralIds.Clear();
                                    targetGeneralIds.AddRange(doGeneralIds); 
                                    Debug.Log("总target:" + string.Join(", ", targetGeneralIds) + "总do:" + string.Join(", ", doGeneralIds));
                                    InitializeSelection();
                                }
                                else
                                {
                                    targetGeneralIds.Clear();
                                    targetGeneralIds.AddRange(doGeneralIds);
                                    Debug.Log("正常所选将领ID：" + string.Join(", ", targetGeneralIds));
                                    SceneManager.LoadScene("ExecutivePanel");
                                }
                            }
                            else
                            {
                                Task = TaskType.OverMove;
                                Debug.Log("所选将领过多");
                                SceneManager.LoadScene("ExecutivePanel");
                            }
                        }
                        else //任命新太守
                        {
                            city.prefectId = doGeneralIds[0];
                            Debug.Log("总target:" + string.Join(", ", targetGeneralIds) + "总do:" + string.Join(", ", doGeneralIds));
                            SceneManager.LoadScene("ExecutivePanel");
                        }
                        break;
                    case TaskType.Attack:
                        city = CityListCache.GetCityByCityId(doCityId);
                        if (!secondConfirm)//第一次确认选将战争
                        {
                            Debug.Log("判断全员太守" + city.prefectId + "和" + string.Join(", ", doGeneralIds));
                            if (doGeneralIds.Count == city.GetCityOfficerNum())//全员出征战争
                            {
                                Debug.Log("判断包含太守" + city.prefectId + "和" + string.Join(", ", doGeneralIds));
                                targetGeneralIds.Clear();
                                targetGeneralIds.AddRange(doGeneralIds);
                                targetGeneralIds.Remove(city.prefectId);
                                targetGeneralIds.Insert(0, city.prefectId);
                                abandonPanel.SetActive(true);
                                abandonButton.onClick.AddListener(OnAbandonButtonClicked);
                                stopButton.onClick.AddListener(OnStopButtonClicked);
                                Debug.Log("所有武将都已被选中！");
                            }
                            else if (doGeneralIds.Contains(city.prefectId))//出征包含太守任命为主将，需要任命新太守
                            {
                                secondConfirm = true;
                                targetGeneralIds.Clear();
                                targetGeneralIds.AddRange(doGeneralIds);
                                targetGeneralIds.Remove(city.prefectId);
                                targetGeneralIds.Insert(0, city.prefectId);
                                Debug.Log("总target:" + string.Join(", ", targetGeneralIds) + "总do:" + string.Join(", ", doGeneralIds));
                                InitializeSelection();
                            }
                            else
                            {
                                targetGeneralIds.Clear();
                                targetGeneralIds.AddRange(doGeneralIds);
                                Debug.Log("正常所选将领ID：" + string.Join(", ", targetGeneralIds));
                                SceneManager.LoadScene("ExecutivePanel");
                            }

                        }
                        else// 任命新太守
                        {
                            city.prefectId = doGeneralIds[0];
                            Debug.Log("总target:" + string.Join(", ", targetGeneralIds) + "总do:" + string.Join(", ", doGeneralIds));
                            SceneManager.LoadScene("ExecutivePanel");
                        }
                        break;
                    case TaskType.Employ:
                    case TaskType.Alienate:
                    case TaskType.Bribe:
                        city = CityListCache.GetCityByCityId(doCityId);
                        if (!secondConfirm)
                        {
                            secondConfirm = true;
                            targetGeneralIds.Add(doGeneralIds[0]);
                            doGeneralIds.Clear();
                            optionalGeneralIds.Clear();
                            optionalGeneralIds = city.GetOfficerIds().ToList();
                            CreateGeneralSelectionToggles(_singleSelect);
                        }
                        else
                        {
                            SceneManager.LoadScene("ExecutivePanel");
                        }
                        break;
                    case TaskType.TruceSelect:
                        Task = TaskType.Truce;
                        SceneManager.LoadScene("ExecutivePanel");
                        break;
                    case TaskType.Inherit:
                        Country country = CountryListCache.GetCountryByCountryId(playerCountryId);
                        country.Inherit(doGeneralIds[0]);
                        SceneManager.LoadScene("GlobalScene");
                        countryDieTips = 5;
                        break;
                    case TaskType.SelfBuild:
                        if (!secondConfirm)//第一次选择新势力所有自建将领
                        {
                            secondConfirm = true;
                            targetGeneralIds.Clear();
                            targetGeneralIds.AddRange(doGeneralIds);
                            optionalGeneralIds.Clear();
                            optionalGeneralIds.AddRange(doGeneralIds);
                            InitializeSelection();
                        }
                        else//第二次选择君主
                        {
                            SceneManager.LoadScene("GlobalScene");
                        }
                        break;
                    case TaskType.SelfRemove:
                        foreach (var id in doGeneralIds)
                        {
                            DataManagement.RemoveCustomGeneral(id);
                        }
                        SceneManager.LoadScene("StartScene");
                        break;
                    default:
                        SceneManager.LoadScene("ExecutivePanel");
                        break;
                }
            }
            Debug.Log("所选将领ID：" + string.Join(", ", doGeneralIds));
        }

        private void OnCancelButtonClicked()
        {
            doGeneralIds.Clear();
            // 返回上一个场景
            if (Task == TaskType.Inherit)
            {
                SceneManager.LoadScene("GlobalScene");
            }
            else if (Task == TaskType.SelfBuild || Task == TaskType.SelfRemove)
            {
                SceneManager.LoadScene("StartScene");
            }
            else 
            {
                SceneManager.LoadScene("CityScene");
            }
            
        }

        void OnAbandonButtonClicked()
        {
            City city = CityListCache.GetCityByCityId(doCityId);
            city.cityBelongKing = 0;
            SceneManager.LoadScene("ExecutivePanel");
        }

        void OnStopButtonClicked()
        {
            SceneManager.LoadScene("CityScene");
        }
    }
}



