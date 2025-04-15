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
    public class UIKing : MonoBehaviour
    {
        [SerializeField] private Transform kingRoom;
        [SerializeField] private GameObject kingToggle;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
    
        [SerializeField] private RawImage headImage;
        [SerializeField] private Image phaseBar;
        [SerializeField] private Image curPhysicalBar;
        [SerializeField] private Image soldierBar;

        [SerializeField] private TextMeshProUGUI generalName;
        [SerializeField] private TextMeshProUGUI grade;
        [SerializeField] private Text level;
        [SerializeField] private Text cityNum;
        [SerializeField] private Text phase;
        [SerializeField] private Text curPhysicl;
        [SerializeField] private Text lead;
        [SerializeField] private Text force;
        [SerializeField] private Text IQ;
        [SerializeField] private Text political;
        [SerializeField] private Text moral;
        [SerializeField] private Text soldier;
        [SerializeField] private TextMeshProUGUI weapon;
        [SerializeField] private TextMeshProUGUI armor;
        [SerializeField] private TextMeshProUGUI horse;
        [SerializeField] private TextMeshProUGUI book;
        [SerializeField] private TextMeshProUGUI pendant;
        
        [SerializeField] private UITips uiTips;
        private byte _countryId;
        private Dictionary<byte, General> _kingList = new Dictionary<byte, General>();

        public void SelectKing()
        {
            gameObject.SetActive(true);
            SpawnKing();
            confirmButton.gameObject.SetActive(true);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnClickConfirmButton);
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnClickCancelButton);
        }

        private void OnClickConfirmButton()
        {
            GameInfo.playerCountryId = _countryId;
            GameInfo.PlayingState = GameState.GameStart;
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
            SceneManager.LoadScene("GlobalScene");
        }
    
        private void OnClickCancelButton()
        {
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
            SceneManager.LoadScene("StartScene");
        }

        void SpawnKing()
        {
            ToggleGroup kingGroup = kingRoom.GetComponent<ToggleGroup>();
            foreach (var pair in CountryListCache.countryDictionary)
            {
                General king = GeneralListCache.GetGeneral(pair.Value.countryKingId);
                _kingList.Add(pair.Key, king);
                GameObject kingObj = Instantiate(kingToggle, kingRoom);
                Toggle toggle = kingObj.GetComponent<Toggle>();
                kingObj.GetComponentInChildren<Text>().text = king.generalName;
                toggle.group = kingGroup;
                toggle.onValueChanged.AddListener((isOn) => OnClickKingToggle(toggle, pair.Key));
            }
            // 自建势力
            GameObject newKingObj = Instantiate(kingToggle, kingRoom);
            Toggle newToggle = newKingObj.GetComponent<Toggle>();
            newToggle.group = kingGroup;
            newKingObj.GetComponentInChildren<Text>().text = "自建";
            newKingObj.GetComponent<Toggle>().onValueChanged.AddListener((isOn) => NewCountryToggle());
            kingRoom.GetChild(0).GetComponent<Toggle>().isOn = true;
        }

        private void OnClickKingToggle(Toggle toggle, byte countryId)
        {
            if (toggle.isOn)
            {
                _countryId = countryId;
                ShowKingInfo();
                Debug.Log($"选择势力Id:{countryId}");
            }
        }

        private void NewCountryToggle()
        {
            DataManagement.ReadCustomGeneralList();
            
            if (GameInfo.customGeneralList == null || GameInfo.customGeneralList.Count == 0)//自定义将领文件不存在
            {
                // 提示无自建将领
                uiTips.ShowOptionalTips("当前无自建武将，是否新建武将？");
                uiTips.OnOptionSelected += HandleSelfBuildGeneral;
            }
            else// 如果存在自定义将领文件
            {
                foreach (var general in GameInfo.customGeneralList)
                {
                    GeneralListCache.AddGeneral(general);
                }
                GameInfo.optionalGeneralIds = GameInfo.customGeneralList.Select(general => general.generalId).ToList();
                GameInfo.Task = TaskType.SelfBuild;
                SceneManager.LoadScene("SelectGeneral");
            }
        }

        private void HandleSelfBuildGeneral(bool isSelfBuild)
        {
            if (isSelfBuild)
            {
                GameInfo.Task = TaskType.SelfBuild;
                SceneManager.LoadScene("SelectGeneral");
            }
            
            uiTips.OnOptionSelected -= HandleSelfBuildGeneral;
        }

        private void ShowKingInfo()
        {
            General king = _kingList[_countryId];
            Country country = CountryListCache.GetCountryByCountryId(_countryId);
            headImage.texture = Resources.Load<Texture2D>($"HeadImage/{king.generalId}");
            generalName.text = king.generalName;
            level.text = "Lv." + king.level;
            cityNum.text = country.GetHaveCityNum().ToString();
            grade.text = king.GetGeneralGradeS();
            phase.text = king.phase.ToString();
            curPhysicl.text = king.curPhysical.ToString();
            lead.text = king.lead.ToString();
            force.text = king.force.ToString();
            IQ.text = king.IQ.ToString();
            political.text = king.political.ToString();
            moral.text = king.moral.ToString();
            soldier.text = country.GetCountrySoldierNum().ToString();
            weapon.text = WeaponListCache.GetWeapon(king.weapon).weaponName;
            armor.text = WeaponListCache.GetWeapon(king.armor).weaponName;

            phaseBar.transform.rotation = Quaternion.Euler(0, 0, (-(float)king.phase + 6) * (360f / 149f)); 
            curPhysicalBar.fillAmount = (float)(king.curPhysical / 100f);
            soldierBar.fillAmount = (float)country.GetCountrySoldierNum() / country.GetMaxCountrySoldierNum();
        }
    
    
    }
}
