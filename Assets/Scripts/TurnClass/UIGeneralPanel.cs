using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TurnClass
{
    public class UIGeneralPanel :MonoBehaviour
    {

        [SerializeField] RawImage headImage;
        [SerializeField] Image levelExpBar;
        [SerializeField] Image phaseBar;
        [SerializeField] Image curPhysicalBar;
        [SerializeField] Image leadExpBar;
        [SerializeField] Image forceExpBar;
        [SerializeField] Image IQBar;
        [SerializeField] Image politicalExpBar;
        [SerializeField] Image moralExpBar;
        [SerializeField] Image loyaltyBar;
        [SerializeField] Image soldierBar;

        [SerializeField] TextMeshProUGUI generalName;
        [SerializeField] TextMeshProUGUI level;
        [SerializeField] TextMeshProUGUI grade;
        [SerializeField] TextMeshProUGUI army;
        [SerializeField] TextMeshProUGUI phase;
        [SerializeField] TextMeshProUGUI curPhysicl;
        [SerializeField] TextMeshProUGUI lead;
        [SerializeField] TextMeshProUGUI force;
        [SerializeField] TextMeshProUGUI IQ;
        [SerializeField] TextMeshProUGUI political;
        [SerializeField] TextMeshProUGUI moral;
        [SerializeField] TextMeshProUGUI loyalty;
        [SerializeField] TextMeshProUGUI soldier;
        [SerializeField] TextMeshProUGUI weapon;
        [SerializeField] TextMeshProUGUI armor;
        [SerializeField] TextMeshProUGUI skills;


        [SerializeField] Button previousButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button cancelButton;

        private int currentIndex = 0;


        void Start()
        {
            DataManagement.Instance.LoadAndInitializeData();


            UpdateGeneralInfo();
            UpdateButtonStates();

            previousButton.onClick.AddListener(ShowPreviousGeneral);
            nextButton.onClick.AddListener(ShowNextGeneral);
            cancelButton.onClick.AddListener(OnClickOncancelButton);



        }
        /// <summary>
        /// 根据将领ID显示武将信息
        /// </summary>
        /// <param name="generalId"></param>
        void ShowGeneralInfoPanel(short generalId)
        {
            General general = GeneralListCache.GetGeneral(generalId);

            generalName.text = general.generalName;
            level.text = "LV." + general.level.ToString();
            grade.text = general.GetGeneralGradeS();
            army.text = general.GetArmyS();
            phase.text = general.phase.ToString();
            curPhysicl.text = general.curPhysical.ToString();
            lead.text = general.lead.ToString();
            force.text = general.force.ToString();
            IQ.text = general.IQ.ToString();
            political.text = general.political.ToString();
            moral.text = general.moral.ToString();
            loyalty.text = general.loyalty.ToString();
            soldier.text = general.generalSoldier.ToString();
            weapon.text = WeaponListCache.GetWeapon(general.weapon).weaponName;
            armor.text = WeaponListCache.GetWeapon(general.armor).weaponName;
            skills.text = general.GetActiveSkills();

            levelExpBar.fillAmount = (float)(general.experience / general.GetMaxExp());
            phaseBar.transform.rotation = Quaternion.Euler(0, 0, (-(float)general.phase + 6) * (360f / 149f)); 
            curPhysicalBar.fillAmount = (float)(general.curPhysical / 100f);
            leadExpBar.fillAmount = (float)general.leadExp / 100f;
            forceExpBar.fillAmount = (float)general.forceExp / 100f;
            IQBar.fillAmount = (float)general.IQExp / 100f;
            politicalExpBar.fillAmount = (float)general.politicalExp / 100f;
            moralExpBar.fillAmount = (float)general.moralExp / 100f;
            loyaltyBar.fillAmount = (float)general.loyalty / 100f;
            soldierBar.fillAmount = (float)(general.generalSoldier / general.GetMaxSoldierNum());



            headImage.texture = Resources.Load<Texture2D>($"HeadImage/{generalId}");
        }

        void UpdateGeneralInfo()
        {
            if (GameInfo.doGeneralIds.Count == 0) return;

            short generalId = GameInfo.doGeneralIds[currentIndex];
            ShowGeneralInfoPanel(generalId);
        }

        void UpdateButtonStates()
        {
            previousButton.interactable = currentIndex > 0;
            nextButton.interactable = currentIndex < GameInfo.doGeneralIds.Count - 1;
        }

        void ShowPreviousGeneral()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateGeneralInfo();
                UpdateButtonStates();
            }
        }

        void ShowNextGeneral()
        {
            if (currentIndex < GameInfo.doGeneralIds.Count - 1)
            {
                currentIndex++;
                UpdateGeneralInfo();
                UpdateButtonStates();
            }
        }

        void OnClickOncancelButton()
        {
            SceneManager.LoadScene("CityPanel");
        }
    }
}
    

