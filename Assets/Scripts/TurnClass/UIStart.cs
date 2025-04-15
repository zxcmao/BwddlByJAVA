using System.Linq;
using DataClass;
using GameClass;
using TMPro;
using UIClass;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TurnClass
{
    public class UIStart : MonoBehaviour
    {
        private static UIStart _instance;
        public static UIStart Instance => _instance; 
        private UIStart() { }
        
        [SerializeField] private GameObject backGround;
        [SerializeField] private GameObject startMenu;
        [SerializeField] private GameObject optionButtonPrefab;
        [SerializeField] private Button startButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button selfBuildButton;
        [SerializeField] private Button infoButton;
        [SerializeField] private Button exitButton;
        
        [SerializeField] private GameObject difficultyMenu;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;
        
        [SerializeField] private GameObject chapterMenu;
        
        [SerializeField] private UIKing kingPanel;
        
        [SerializeField] private UIRecord loadPanel;
        
        [SerializeField] private GameObject selfBuildPanel;
        
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Button confirmButton;

        [SerializeField] private UITips uiTips;
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

        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClick);
            loadButton.onClick.AddListener(OnLoadButtonClick);
            selfBuildButton.onClick.AddListener(OnSelfBuildButtonClick);
            infoButton.onClick.AddListener(OnInfoButtonClick);
            exitButton.onClick.AddListener(OnExitButtonClick);
            
            SoundManager.Instance.SetBGMVolume(PlayerPrefs.GetFloat("bgmVolume", 0.5f));
            if (PlayerPrefs.GetFloat("bgmVolume", 0.5f) != 0)
            {
                SoundManager.Instance.PlayBGM("1");
            }
        }

       
        private void OnStartButtonClick()
        {
            startMenu.SetActive(false);
            chapterMenu.SetActive(true);
            //TODO
            Button button = optionButtonPrefab.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnChapterButtonClick);
        }

        private void OnChapterButtonClick()
        {
            GameInfo.chapter = "群雄起源";
            DataManagement.Instance.LoadAndInitializeData();
            chapterMenu.SetActive(false);
            difficultyMenu.SetActive(true);
            easyButton.onClick.AddListener(delegate { OnDifficultyButtonClick(1);});
            normalButton.onClick.AddListener(delegate { OnDifficultyButtonClick(2);});
            hardButton.onClick.AddListener(delegate { OnDifficultyButtonClick(3);});
        }

        private void OnDifficultyButtonClick(byte difficulty)
        {
            GameInfo.difficult = difficulty;
            difficultyMenu.SetActive(false);
            kingPanel.SelectKing();
            startMenu.SetActive(true);
        }

        private void OnLoadButtonClick()
        {
            startMenu.SetActive(false);
            loadPanel.RecordIndex += LoadGame;
            loadPanel.Load();
        }

        private void LoadGame(byte index)
        {
            loadPanel.RecordIndex -= LoadGame;
            if (index != 0)
            {
                DataManagement.Instance.LoadGame(index - 1);
                SceneManager.LoadSceneAsync("CityScene");
            }
            else
            {
                startMenu.SetActive(true);
            }
        }

        private void OnSelfBuildButtonClick()
        {
            startMenu.SetActive(false);
            DataManagement.ReadCustomGeneralList();
            uiTips.ShowOptionalTips("创建还是删除武将？");
            uiTips.OnOptionSelected += OnSelfBuildSelected;
        }

        private void OnSelfBuildSelected(bool isCreate)
        {
            uiTips.OnOptionSelected -= OnSelfBuildSelected;
            uiTips.gameObject.SetActive(false);
            if (!isCreate)//如果选择删除武将
            {
                if (GameInfo.customGeneralList.Count == 0)
                {
                    uiTips.ShowNoticeTipsWithConfirm("没有自建武将可以删除", delegate {
                        startMenu.SetActive(true); 
                    });
                }
                else
                {
                    GameInfo.Task = TaskType.SelfRemove;
                    GameInfo.optionalGeneralIds.Clear();
                    GameInfo.optionalGeneralIds = GameInfo.customGeneralList.Select(x => x.generalId).ToList();
                    SceneManager.LoadScene("SelectGeneral");
                }
            }
            else
            {
                startMenu.SetActive(true);
                selfBuildPanel.SetActive(true);
            }
        }

        private void OnInfoButtonClick()
        {
            infoPanel.SetActive(true);
            confirmButton.gameObject.SetActive(true);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(delegate
            {
                infoPanel.SetActive(false);
                startMenu.SetActive(true);
                confirmButton.gameObject.SetActive(false);
            });
             StartCoroutine(DataManagement.ReadIntroduction((introText) =>
             {
                 if (!string.IsNullOrEmpty(introText))
                 {
                     infoText.text = introText;
                 }
                 else
                 {
                     Debug.Log("说明文件加载失败或为空！");
                 }
             }));
        }
        
        private void OnExitButtonClick()
        {
            Debug.Log("退出游戏");
            Application.Quit();
        }
    }
        
}