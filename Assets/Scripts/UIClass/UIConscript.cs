using System;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using TMPro;
using TurnClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIClass
{
    public class UIConscript : MonoBehaviour // 士兵征兵界面
    {
        [SerializeField] private Text gold;
        [SerializeField] private Text soldiers;
        [SerializeField] private Text reserves;
        [SerializeField] private TMP_InputField need;
        [SerializeField] private TextMeshProUGUI useInfo;
        [SerializeField] private TextMeshProUGUI assignInfo;
        [SerializeField] private Button conscriptButton;
        [SerializeField] private Button assignButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject assignPrefab;
        [SerializeField] private Transform assignRoom;

        private City _city;
        private short _gold;
        private short _soldiers;
        private short _reserves;
        private int _maxConscriptSoldiers;
        private int _bestSoldiers;
        private int _sliderLimit;
        private List<General> _generals = new List<General>();
        private short[] _generalSoldiers;

        public void Conscript(byte cityId)
        {
            _city = CityListCache.GetCityByCityId(cityId);
            if (_city == null) return;
            _gold = _city.GetMoney();
            _soldiers = (short)_city.GetAlreadySoldierNum();
            _reserves = (short)_city.cityReserveSoldier;
            _maxConscriptSoldiers = _city.GetMaxConscriptSoldierNum(); // 最大可征兵士兵数量
            gameObject.SetActive(true);
            gold.text = _gold.ToString();
            soldiers.text = _soldiers.ToString();
            reserves.text = _reserves.ToString();
            
            useInfo.text = "征100兵需20金";
            assignInfo.text = "可分配数:" + _reserves;
            
            SpawnAssignObj();
            need.onValueChanged.AddListener(OnNeedChanged);
            
            conscriptButton.onClick.AddListener(OnConscriptButtonClicked);
            assignButton.onClick.AddListener(OnAssignButtonClicked);
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        void SpawnAssignObj()
        {
            // 清空之前的子物体
            foreach (GameObject child in assignRoom)
            {
                Destroy(child);
            }
            
            _sliderLimit = _city.GetCityAllSoldierNum(); // 总可分配士兵数量

            foreach (var id in _city.GetOfficerIds())
            {
                _generals.Add(GeneralListCache.GetGeneral(id)); // 获取所有将领ID
            }
            _generals = _generals.OrderByDescending(g => g.GetWarValue()).ToList(); // 根据统帅能力降序排列
            _generalSoldiers = new short[_generals.Count];
            foreach (var general in _generals)
            {
                // 创建并设置子物体
                GameObject assignObj = Instantiate(assignPrefab, assignRoom);
                TextMeshProUGUI[] text = assignObj.GetComponentsInChildren<TextMeshProUGUI>();
                Slider slider = assignObj.GetComponentInChildren<Slider>();
                
                // 设置文本和滑块
                text[0].text = general.generalName;
                text[1].text = $"统兵:{general.generalSoldier}";
                slider.minValue = 0;
                slider.maxValue = general.GetMaxSoldierNum();
                slider.value = general.generalSoldier;
                
                // 监听滑块变化
                int index = _generals.IndexOf(general); // 为了在 lambda 表达式中使用当前索引
                _generalSoldiers[index] = (short)slider.value;
                slider.onValueChanged.AddListener((value) =>
                {
                    text[1].text = $"统兵:{(int)value}"; // 更新士兵数量文本
                    _generalSoldiers[index] = (short)value; // 更新对应将领的士兵数
                    ClampSliders(index); // 限制滑块行为
                });
            }
        }
        
        void ClampSliders(int changedIndex)
        {
            // 计算当前已分配的士兵总数
            int currentTotalAssigned = 0;
            int slidersCount = assignRoom.childCount;

            // 逐个遍历每个滑块，计算当前总分配
            for (int i = 0; i < slidersCount; i++)
            {
                Slider slider = assignRoom.GetChild(i).Find("AssignSlider").GetComponent<Slider>();
                currentTotalAssigned += (int)slider.value;
            }

            // 处理被改变的滑块
            Slider changedSlider = assignRoom.GetChild(changedIndex).Find("AssignSlider").GetComponent<Slider>();
            int maxAssignable = _sliderLimit - (currentTotalAssigned - (int)changedSlider.value);

            // 限制当前滑块值
            if ((int)changedSlider.value > maxAssignable)
            {
                changedSlider.value = maxAssignable; // 限制为最大可分配值
            }

            // 确保可分配数不为负数
            int remainingSoldiers = _sliderLimit - currentTotalAssigned;
            if (remainingSoldiers < 0)
            {
                remainingSoldiers = 0; // 确保至少为零
            }

            // 更新可分配数的文本显示
            assignInfo.text = $"可分配数:{remainingSoldiers}";
        }
        
        void OnNeedChanged(string value)
        {
            if (int.TryParse(value, out int intValue))
            {
                // 限制在0到30000之间（包括0但不包括30000）
                if (intValue < 0 || intValue >= 30000)
                {
                    need.text = "0"; // 设置为默认值
                }
                else if (intValue > _city.GetMoney() * 5)
                {
                    need.text = $"{_city.GetMoney() * 5}"; // 设置为城市现有的金钱量最大招募兵数
                    useInfo.text = $"将花费{_city.GetMoney()}金钱";
                }
                else if (intValue > _city.rule * 500)
                {
                    need.text = $"{_city.rule * 500}"; // 设置为城市现有的统治值最大招募兵数
                    useInfo.text = $"将花费{_city.rule * 100}金钱";
                }
                else if (intValue * 3 > _city.population * 2 )
                {
                    need.text = $"{_city.population * 2 / 3}"; // 设置为城市现有的人口量最大招募兵数
                    useInfo.text = $"将花费{_city.population * 2 / 3 / 5}金钱";
                }
                confirmButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
        }

        void OnConscriptButtonClicked()
        {
            if (int.TryParse(need.text, out int intValue))
            {
                _city.Conscript(intValue);
                _gold = _city.GetMoney();
                _soldiers = (short)_city.GetAlreadySoldierNum();
                _reserves = (short)_city.cityReserveSoldier;
                _sliderLimit = _city.GetCityAllSoldierNum(); // 总可分配士兵数量
                _maxConscriptSoldiers = _city.GetMaxConscriptSoldierNum(); // 最大可征兵士兵数量
                gold.text = _gold.ToString();
                soldiers.text = _soldiers.ToString();
                reserves.text = _reserves.ToString();
            
                need.text = "00000";
                useInfo.text = "征100兵需20金";
                assignInfo.text = "可分配数:" + _reserves;
            }
        }
        
        void OnAssignButtonClicked()
        {
            _bestSoldiers = (short)(_city.GetCityAllSoldierNum() + _maxConscriptSoldiers); // 总可分配士兵数量
            _sliderLimit = _bestSoldiers; // 总可分配士兵数量
            need.text = _maxConscriptSoldiers.ToString();
            useInfo.text = $"征{_maxConscriptSoldiers}兵需{(_maxConscriptSoldiers + 4)/ 5}金钱";
            assignInfo.text = "可分配如下:";
            for (int i = 0; i < _generalSoldiers.Length; i++)
            {
                GameObject obj = assignRoom.GetChild(i).gameObject;
                Slider slider = obj.GetComponentInChildren<Slider>();
                TextMeshProUGUI[] text = obj.GetComponentsInChildren<TextMeshProUGUI>();
                int index = i; // 为了在 lambda 表达式中使用当前索引
                slider.onValueChanged.RemoveAllListeners();
                if (_bestSoldiers < slider.maxValue)// 如果总可分配士兵数量小于当前将领的最大可分配士兵数量
                {
                    slider.value = _bestSoldiers;
                    _generalSoldiers[i] = (short)slider.value;
                    _bestSoldiers = 0;
                }
                else
                {
                    slider.value = slider.maxValue;
                    _generalSoldiers[i] = (short)slider.value;
                    _bestSoldiers -= (int)slider.maxValue;
                }
                text[1].text = $"统兵:{(int)slider.value}";
                slider.onValueChanged.AddListener((value) =>
                {
                    text[1].text = $"统兵:{(int)value}"; // 更新士兵数量文本
                    _generalSoldiers[index] = (short)value; // 更新对应将领的士兵数
                    ClampSliders(index); // 限制滑块行为
                });
            }
            _city.Conscript(_maxConscriptSoldiers);
            Debug.Log(String.Join(",", _generalSoldiers));
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        void OnConfirmButtonClicked()
        {
            int assignSoldiers = 0;
            for (byte i = 0; i < _generals.Count; i++)
            {
                _generals[i].generalSoldier = _generalSoldiers[i];
                assignSoldiers += _generalSoldiers[i];
            }
            _city.cityReserveSoldier = _sliderLimit - assignSoldiers;
            GameInfo.SubPlayerOrder();
            gameObject.SetActive(false);
            UIExecutivePanel.Instance.GoToNextScene();
        }

        void OnCancelButtonClicked()
        {
            gameObject.SetActive(false);
            UIExecutivePanel.Instance.GoToNextScene();
        }
    }
}