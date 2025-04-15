using System;
using System.Collections.Generic;
using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace War
{
    public class UIRetreatPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI retreatText;
        [SerializeField] private Transform retreatRoom;
        [SerializeField] private GameObject retreatOptionPrefab;
        [SerializeField] private Button closeButton;

        private UnitObj _retreatUnitObj;
        private bool _allRetreat = false; // 是否是全体撤退模式
        public bool _withCommander = false; // 是否包含主将
        
        // 事件：所有将军撤退完成
        public event Action AllRetreatOver;

        /// <summary>
        /// 仅撤退当前将军（适用于普通将军、主将撤退前）
        /// </summary>
        public void ShowRetreatPanel()
        {
            gameObject.SetActive(true);
            _retreatUnitObj = WarManager.Instance.hmUnitObj;
            CreateRetreatCityOption();
        
            // 允许手动关闭撤退面板
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HideRetreatPanel);
        }

        /// <summary>
        /// 主将战死或被俘，所有将军必须撤退（强制撤退）
        /// </summary>
        public void MustRetreat(Action allRetreatOver)
        {
            gameObject.SetActive(true);
            closeButton.gameObject.SetActive(false); // 禁止取消撤退

            _allRetreat = true;
            AllRetreatOver = allRetreatOver;
            ProcessNextRetreat();
        }

        /// <summary>
        /// 处理当前单位撤退，并在其完成后继续下一个
        /// </summary>
        private void ProcessNextRetreat()
        {
            if (WarManager.Instance.hmUnits.Count == 0)
            {
                // 所有单位撤退完毕
                _allRetreat = false;
                HideRetreatPanel();
            
                // 触发所有将军撤退完成事件
                AllRetreatOver?.Invoke();
                _withCommander = false;
                
                AllRetreatOver -= WarManager.Instance.PlayerWithdraw;
                return;
            }

            // 获取当前要撤退的单位
            WarManager.Instance.hmUnitObj = WarManager.Instance.hmUnits[0];
            _retreatUnitObj = WarManager.Instance.hmUnitObj;

            // 显示撤退选项 UI
            CreateRetreatCityOption();
        }

        private void CreateRetreatCityOption()
        {
            City warCity = CityListCache.GetCityByCityId(WarManager.Instance.curWarCityId);
            Debug.Log($"当前战争城池:{warCity.cityName},所属君主:{warCity.cityBelongKing},所属势力:{CountryListCache.GetCountryByKingId(warCity.cityBelongKing)}");
            Country country = CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId);
            Debug.Log($"当前国家：{country.countryId}" + $"当前国家君主：{country.KingName()},城池ID有:" + string.Join(", ", country.cityIDs));
            List<byte> retreatCityId = WarManager.GetRetreatCityList(WarManager.Instance.hmKingId);
            Debug.Log($"撤退城池ID有" + string.Join(", ", retreatCityId));
            foreach (Transform child in retreatRoom)
            {
                Destroy(child.gameObject);
            }

            if (country.IsEndangered() && retreatCityId.Count == 0)
            {
                UIWar.Instance.NotifyWarEvent("无城可退！背水一战吧");
                HideRetreatPanel();
                return;
            }
            
            General general = GeneralListCache.GetGeneral(_retreatUnitObj.genID);
            retreatText.text = $"{general.generalName} 欲往何处?";
            if (retreatCityId != null && retreatCityId.Count != 0)
            {
                foreach (byte cityId in retreatCityId)
                {
                    City city = CityListCache.GetCityByCityId(cityId);
                    GameObject cityOption = Instantiate(retreatOptionPrefab, retreatRoom);
                    Button button = cityOption.GetComponent<Button>();
                    TextMeshProUGUI text = cityOption.GetComponentInChildren<TextMeshProUGUI>();
                    text.text = city.cityName;

                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnClickRetreatOptionButton(cityId));
                }
            }
            // 下野选项（如果不是君主）
            if (_retreatUnitObj.genID != WarManager.Instance.hmKingId)
            {
                GameObject retireOption = Instantiate(retreatOptionPrefab, retreatRoom);
                Button retireButton = retireOption.GetComponent<Button>();
                TextMeshProUGUI retireText = retireOption.GetComponentInChildren<TextMeshProUGUI>();
                retireText.text = "下野";
                retireText.color = Color.red;
                retireButton.onClick.AddListener(() => OnClickRetireButton(general.generalId));
            }
        }

        /// <summary>
        /// 选择城池后执行撤退
        /// </summary>
        void OnClickRetreatOptionButton(byte cityId)
        {
            if (cityId <= 0) return;

            Debug.Log($"{_retreatUnitObj.name} 撤退至 {CityListCache.GetCityByCityId(cityId).cityName}");

            bool isCommander = !_retreatUnitObj.HandleGeneralRetreat(cityId); // 判断是否是主将撤退

            if (isCommander)
            {
                Debug.Log($"主将 {_retreatUnitObj.name} 已撤退，其他将军开始撤退");

                closeButton.gameObject.SetActive(false); // 禁止取消
                _withCommander = true;
                // 触发其他将军的撤退
                _allRetreat = true;
                ProcessNextRetreat();
            }
            else if (_allRetreat)
            {
                ProcessNextRetreat();
            }
            else
            {
                HideRetreatPanel();
            }
        }

        /// <summary>
        /// 武将撤退下野
        /// </summary>
        void OnClickRetireButton(short generalId)
        {
            Debug.Log($"{GeneralListCache.GetGeneral(generalId).generalName} 下野了," +
                      $"{CityListCache.GetCityByCityId(WarManager.Instance.curWarCityId).cityName}中已有{CityListCache.GetCityByCityId(WarManager.Instance.curWarCityId).GetCityNotFoundGeneralNum()}隐居");

            bool isCommander = !_retreatUnitObj.HandleGeneralRetire(); // 判断是否是主将下野

            if (isCommander)
            {
                Debug.Log($"主将 {_retreatUnitObj.name} 下野，其他将军开始撤退");

                closeButton.gameObject.SetActive(false); // 禁止取消
                _withCommander = false;
                // 触发其他将军的撤退
                _allRetreat = true;
                ProcessNextRetreat();
            }
            else if (_allRetreat)
            {
                ProcessNextRetreat();
            }
            else
            {
                HideRetreatPanel();
            }
        }

        void HideRetreatPanel()
        {
            gameObject.SetActive(false);
            UIWar.Instance.DisplayWarMenu();
            Debug.Log("关闭撤退面板");
        }
    }
}
