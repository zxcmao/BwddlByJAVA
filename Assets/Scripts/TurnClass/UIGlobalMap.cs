using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using GameClass;
using TMPro;
using TurnClass.AITurnStateMachine;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DataClass.GameInfo;

namespace TurnClass
{
    public class UIGlobe : MonoBehaviour
    {
        // 持有CityButton的预制件
        [SerializeField] private GameObject cityPrefab;

        // 地图的引用
        [SerializeField] private Transform globalMap;

        // 按钮的大小
        [SerializeField] private Vector2 buttonSize = new Vector2(50, 50);
        
        [SerializeField] private UIEnd endPanel;
        [SerializeField] private TextMeshProUGUI timeTitle;
        [SerializeField] private TextMeshProUGUI turnInfo;
        public UITips tips;

        private static Dictionary<byte, GameObject> CityButtons = new Dictionary<byte, GameObject>();
        private void Start()
        {
            Debug.Log(PlayerPrefs.GetFloat("bgmVolume"));
            if (PlayerPrefs.GetFloat("bgmVolume") > 0)
            {
                SoundManager.Instance.PlayBGM("2");
                Debug.Log("播放音乐2");
            }
            else
            {
                SoundManager.Instance.StopBGM();
            }
            timeTitle.text = $"{years}年{month}月";
            SpawnButtons();
            switch (Task)
            {
                case TaskType.Move:
                    turnInfo.text = "前往何城？";
                    break;
                case TaskType.Attack:
                    turnInfo.text = "攻打何城？";
                    break;
                case TaskType.Transport:
                    turnInfo.text = "运输何城？";
                    break;
                case TaskType.Alienate:
                    turnInfo.text = "被离间者在何城？";
                    break;
                case TaskType.Bribe:
                    turnInfo.text = "被招揽者在何城？";
                    break;
                case TaskType.Intelligence:
                    turnInfo.text = "侦查何城？";
                    break;
                case TaskType.Inherit:
                    turnInfo.text = "继承者在何城？";
                    break;
                case TaskType.SelfBuild:
                    turnInfo.text = "何座空城建都？";
                    break;
            }
            
        }

        
        void SpawnButtons()
        {
            foreach (var city in CityListCache.cityDictionary)
            {
                if (city.Value == null || city.Value.mapPosition == null || city.Value.mapPosition.Length < 2)
                {
                    Debug.LogWarning($"城市数据无效: {city.Value?.cityName ?? "null"}");
                    continue;
                }
                
                // 获取颜色
                Color cityColor = Color.white;
                if (city.Value.cityBelongKing != 0)
                {
                    Country country = CountryListCache.GetCountryByKingId(city.Value.cityBelongKing);
                    if (country != null && !ColorUtility.TryParseHtmlString(country.countryColor, out cityColor))
                    {
                        Debug.LogWarning($"无效颜色: {country.countryColor} for country {country.countryId}");
                    }
                }

                // 从城市对象中获取坐标
                int x = city.Value.mapPosition[0];
                int y = city.Value.mapPosition[1];

                // 生成按钮
                GameObject newButton = Instantiate(cityPrefab, globalMap);
                RectTransform rectTransform = newButton.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(x, y);
                rectTransform.sizeDelta = buttonSize;

                // 设置按钮的颜色
                Image buttonImage = newButton.GetComponent<Image>();
                buttonImage.color = cityColor;

                // 设置按钮监听事件
                Button button = newButton.GetComponent<Button>();
                button.onClick.AddListener(() => OnCityButtonClicked(city.Value.cityId));

                // 存储按钮到字典
                CityButtons[city.Value.cityId] = newButton;
            }
        }

        
        
        public static void UpdateCityButtonColor(int cityId, string newColor)
        {
            if (CityButtons.TryGetValue((byte)cityId, out GameObject button))
            {
                Image buttonImage = button.GetComponent<Image>();
                if (ColorUtility.TryParseHtmlString(newColor, out Color parsedColor))
                {
                    buttonImage.color = parsedColor;
                    Debug.Log($"更新城市 {cityId} 按钮颜色为: {newColor}");
                }
                else
                {
                    Debug.LogWarning($"无效颜色字符串: {newColor}");
                }
            }
            else
            {
                Debug.LogWarning($"未找到城市 {cityId} 对应的按钮");
            }
        }

        public IEnumerator Notify(string text, GameState state)
        {
            yield return tips.ShowTurnTips(text, state);
        }

        public void UpdateTimeTitle()
        {
            timeTitle.text = $"{years}年{month}月";
        }
        
        public void UpdateTurnInfo(string text)
        {
            turnInfo.text = text;
        }

        public void GameEnd()
        {
            endPanel.gameObject.SetActive(true);
        }
        
        void OnCityButtonClicked(byte cityId)
        {
            City city = CityListCache.GetCityByCityId(cityId);
            short playerKingId =
                Task == TaskType.SelfBuild ? doGeneralIds[0] : 
                    CountryListCache.GetCountryByCountryId(playerCountryId).countryKingId;
            
            switch (Task)
            {
                case TaskType.Move:
                    if (city.cityId != doCityId && city.cityBelongKing == playerKingId)
                    {
                        targetCityId = city.cityId;
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break; 
                case TaskType.Attack:
                    Country playerCountry = CountryListCache.GetCountryByCountryId(playerCountryId);
                    if (city.IsConnected(doCityId) && city.cityBelongKing != playerKingId && !playerCountry.IsAlliance(cityId))
                    {
                        targetCityId = city.cityId;
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.Transport:
                    if (city.cityId != doCityId && city.cityBelongKing == playerKingId)
                    {
                        targetCityId = city.cityId;
                        SceneManager.LoadScene("ExecutivePanel");
                    }
                    break;
                case TaskType.Alienate:
                    if (city.cityBelongKing != 0 && city.cityBelongKing != playerKingId)
                    {
                        optionalGeneralIds.Clear();
                        optionalGeneralIds = city.GetCitySubjectsGeneralIdArray().ToList();
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.Bribe:
                    if (city.cityBelongKing != 0 && city.cityBelongKing != playerKingId)
                    {
                        targetCityId = cityId;
                        optionalGeneralIds.Clear();
                        optionalGeneralIds = city.GetCitySubjectsGeneralIdArray().ToList();
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.Intelligence:
                    targetCityId = cityId;
                    SceneManager.LoadScene("CityScene");
                    break;
                case TaskType.Inherit:
                    if (city.cityBelongKing == playerKingId)
                    {
                        doCityId = cityId;
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.SelfBuild:
                    if (city.cityBelongKing == 0)
                    {
                        doCityId = cityId;
                        CountryListCache.SelfBuildCountry(doCityId, doGeneralIds[0], targetGeneralIds);
                        Task = TaskType.None;
                        UpdateCityButtonColor(doCityId, CountryListCache.GetCountryByCountryId(playerCountryId).countryColor);
                        TurnManager.Instance.GameStart();
                    }
                    break;
                default:
                    if (city.cityBelongKing == 0)
                    {
                        turnInfo.text = $"城池:{city.cityName}不属于任何势力";
                    }
                    else if (city.cityBelongKing == playerKingId)
                    {
                        doCityId = cityId;
                        SceneManager.LoadScene("CityScene");
                        Debug.Log($"切换到城市ID:{cityId}");
                    }
                    else
                    {
                        turnInfo.text = $"城池:{city.cityName} 属于{GeneralListCache.GetGeneral(city.cityBelongKing).generalName}势力";
                    }
                    break;
            }          
        
        }
    }
}
