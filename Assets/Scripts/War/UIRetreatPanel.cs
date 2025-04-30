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
        public void MustRetreat()
        {
            gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.gameObject.SetActive(false); // 禁止取消撤退

            _allRetreat = true;
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
                
                _withCommander = false;
                // 触发所有将军撤退完成事件
                WarManager.Instance.PlayerWithdraw();
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
            Debug.Log($"当前战争城池:{warCity.cityName},所属君主:{warCity.ownerID},所属势力:{CountryListCache.GetCountryByKingId(warCity.ownerID)}");
            Country country = CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId);
            Debug.Log($"当前势力君主：{country.KingName()},城池ID有:" + string.Join(", ", country.cityIDs));
            List<byte> retreatCityId = WarManager.GetRetreatCityList();
            Debug.Log("可撤退城池ID有" + string.Join(", ", retreatCityId));
            foreach (Transform child in retreatRoom)
            {
                Destroy(child.gameObject);
            }
            
            retreatText.text = $"{_retreatUnitObj.name} 欲往何处?";
            
            if (retreatCityId.Count == 0)
            {
                UIWar.Instance.NotifyWarEvent("无城可退！背水一战吧");
                HideRetreatPanel();
                return;
            }
            else
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
                retireButton.onClick.AddListener(() => OnClickRetireButton());
            }
        }

        /// <summary>
        /// 选择城池后执行撤退
        /// </summary>
        void OnClickRetreatOptionButton(byte cityId)
        {
            City city = CityListCache.GetCityByCityId(cityId);
            if (city == null) return;

            Debug.Log($"{_retreatUnitObj.name} 撤退至 {city.cityName}");

            bool isCommander = _retreatUnitObj.IsCommanderRetreat(cityId); // 判断是否是主将撤退

            if (isCommander)
            {
                city.AddGold(WarManager.Instance.hmGold);
                city.AddFood(WarManager.Instance.hmFood);
                if (WarManager.Instance.isHmDef)
                    city.AddTreasureNum(CityListCache.GetCityByCityId(WarManager.Instance.curWarCityId).GetTreasureNum());
                
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
        void OnClickRetireButton()
        {
            Debug.Log($"{_retreatUnitObj} 下野了," +
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
