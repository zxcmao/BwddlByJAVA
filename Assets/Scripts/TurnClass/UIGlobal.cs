using System;
using System.Collections.Generic;
using BaseClass;
using DataClass;
using GameClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        [SerializeField] private Button backButton;
        public UITips tips;

        private static Dictionary<byte, GameObject> _cityButtons;
        public Dictionary<GameState, Sprite> cachedEventSprites;

        
        private void OnEnable()
        {
            _cityButtons = new Dictionary<byte, GameObject>();
            cachedEventSprites = new Dictionary<GameState, Sprite>();
            PreloadAllEventSprites();
        }
        
        private void OnDestroy()
        {
            foreach (var kvp in cachedEventSprites)
            {
                if (kvp.Value != null)
                    Addressables.Release(kvp.Value);
            }
            cachedEventSprites.Clear();
            foreach (var kvp in _cityButtons)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
            }
            _cityButtons.Clear();
            Debug.Log("已释放所有事件图片和城池资源");
        }

        private void Start()
        {
            Debug.Log(PlayerPrefs.GetFloat("bgmVolume"));
            if (PlayerPrefs.GetFloat("bgmVolume") > 0)
            {
                SoundManager.Instance.PlayBGM("Assets/Audio/Bgm/2.ogg");
                Debug.Log("播放音乐2");
            }
            else
            {
                SoundManager.Instance.StopBGM();
            }
            timeTitle.text = $"{years}年{month}月";
            backButton.onClick.RemoveAllListeners();
            backButton.gameObject.SetActive(false);
            SpawnButtons();
            switch (Task)
            {
                case TaskType.Move:
                    EnableBackButton();
                    turnInfo.text = "前往何城？";
                    break;
                case TaskType.Attack:
                    EnableBackButton();
                    turnInfo.text = "攻打何城？";
                    break;
                case TaskType.Transport:
                    EnableBackButton();
                    turnInfo.text = "运输何城？";
                    break;
                case TaskType.Alienate:
                    EnableBackButton();
                    turnInfo.text = "被离间者在何城？";
                    break;
                case TaskType.Bribe:
                    EnableBackButton();
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
                default:
                    if (PlayingState == GameState.PlayerTurn)
                    {
                        turnInfo.text = "对何州郡下令？";
                    }
                    else
                    {
                        turnInfo.text = "";
                    }
                    break;
            }
            
        }
        
        private void PreloadAllEventSprites()
        {
            foreach (GameState state in Enum.GetValues(typeof(GameState)))
            {
                if (state is GameState.None or GameState.GameStart or GameState.Playing or GameState.PlayerTurn 
                    or GameState.AITurn or GameState.PlayervsAI)
                    continue;
                Addressables.LoadAssetAsync<Sprite>($"Assets/Image/Event/{state}.png").Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        cachedEventSprites[state] = handle.Result;
                    }
                    else
                    {
                        Debug.LogError($"预加载事件图片失败: {state}");
                    }
                };
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
                if (city.Value.ownerID != 0)
                {
                    Country country = CountryListCache.GetCountryByKingId(city.Value.ownerID);
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
                newButton.name = city.Value.cityName;
                RectTransform rectTransform = newButton.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(x, y);
                rectTransform.sizeDelta = buttonSize;

                // 设置按钮的颜色
                Image buttonImage = newButton.GetComponent<Image>();
                buttonImage.color = cityColor;

                // 设置按钮监听事件
                Button button = newButton.GetComponent<Button>();
                button.onClick.AddListener(() => OnCityButtonClicked(city.Value.cityID));

                // 存储按钮到字典
                _cityButtons[city.Value.cityID] = newButton;
            }
        }

        
        
        public static void UpdateCityButtonColor(int cityId, string newColor)
        {
            if (_cityButtons.TryGetValue((byte)cityId, out GameObject button))
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
        
        private void EnableBackButton()
        {
            backButton.gameObject.SetActive(true);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void OnBackButtonClick()
        {
            Task = TaskType.None;
            backButton.onClick.RemoveAllListeners();
            backButton.gameObject.SetActive(false);
            SceneManager.LoadSceneAsync("CityScene");
        }
        
        void OnCityButtonClicked(byte cityId)
        {
            if (PlayingState != GameState.PlayerTurn 
                && Task != TaskType.Inherit 
                && Task != TaskType.SelfBuild)
            {
                return;
            }
            
            City city = CityListCache.GetCityByCityId(cityId);
            short playerKingId =
                Task == TaskType.SelfBuild ? doGeneralIds[0] : 
                    CountryListCache.GetCountryByCountryId(playerCountryId).countryKingId;
            
            switch (Task)
            {
                case TaskType.Move:
                    if (city.cityID != doCityId && city.ownerID == playerKingId)
                    {
                        targetCityId = city.cityID;
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break; 
                case TaskType.Attack:
                    Country playerCountry = CountryListCache.GetCountryByCountryId(playerCountryId);
                    if (city.IsConnected(doCityId) && city.ownerID != playerKingId && !playerCountry.IsAlliance(cityId))
                    {
                        targetCityId = city.cityID;
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.Transport:
                    if (city.cityID != doCityId && city.ownerID == playerKingId)
                    {
                        targetCityId = city.cityID;
                        SceneManager.LoadScene("ExecutivePanel");
                    }
                    break;
                case TaskType.Alienate:
                    if (city.ownerID != 0 && city.ownerID != playerKingId)
                    {
                        SetGeneralOption(city.GetCitySubjectsGeneralIdArray());
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.Bribe:
                    if (city.ownerID != 0 && city.ownerID != playerKingId)
                    {
                        targetCityId = cityId;
                        SetGeneralOption(city.GetCitySubjectsGeneralIdArray());
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.Intelligence:
                    targetCityId = cityId;
                    SceneManager.LoadScene("CityScene");
                    break;
                case TaskType.Inherit:
                    if (city.ownerID == playerKingId)
                    {
                        doCityId = cityId;
                        SetGeneralOption(city.GetOfficerIds());
                        SceneManager.LoadScene("SelectGeneral");
                    }
                    break;
                case TaskType.SelfBuild:
                    if (city.ownerID == 0)
                    {
                        doCityId = cityId;
                        CountryListCache.SelfBuildCountry(doCityId, doGeneralIds[0], targetGeneralIds);
                        Task = TaskType.None;
                        UpdateCityButtonColor(doCityId, CountryListCache.GetCountryByCountryId(playerCountryId).countryColor);
                        TurnManager.Instance.GameStart();
                    }
                    break;
                default:
                    if (city.ownerID == 0)
                    {
                        turnInfo.text = $"城池:{city.cityName}不属于任何势力";
                    }
                    else if (city.ownerID == playerKingId)
                    {
                        doCityId = cityId;
                        SceneManager.LoadScene("CityScene");
                        Debug.Log($"切换到城市ID:{cityId}");
                    }
                    else
                    {
                        turnInfo.text = $"城池:{city.cityName} 属于{GeneralListCache.GetGeneral(city.ownerID).generalName}势力";
                    }
                    break;
            }          
        
        }

        
    }
}
