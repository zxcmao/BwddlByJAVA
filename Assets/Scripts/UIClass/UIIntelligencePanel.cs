using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using War;

namespace UIClass
{
    public class UIIntelligencePanel : MonoBehaviour
    {
        [SerializeField] Image headImage;
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
        [SerializeField] Text level;
        [SerializeField] TextMeshProUGUI grade;
        [SerializeField] TextMeshProUGUI army;
        [SerializeField] Text phase;
        [SerializeField] Text curPhysicl;
        [SerializeField] Text lead;
        [SerializeField] Text force;
        [SerializeField] Text IQ;
        [SerializeField] Text political;
        [SerializeField] Text moral;
        [SerializeField] Text loyalty;
        [SerializeField] Text soldier;
        [SerializeField] TextMeshProUGUI weapon;
        [SerializeField] TextMeshProUGUI armor;
        [SerializeField] TextMeshProUGUI skills;
        

        /// <summary>
        /// 根据将领ID显示武将信息
        /// </summary>
        /// <param name="generalId"></param>
        public void ShowIntelligencePanel(short generalId)
        {
            gameObject.SetActive(true);
            gameObject.GetComponent<Button>()?.onClick.RemoveAllListeners();
            gameObject.GetComponent<Button>()?.onClick.AddListener(HideIntelligencePanel);
            General general = GeneralListCache.GetGeneral(generalId);

            DataManager.LoadSpriteToImage($"Assets/Image/Head/{general.generalId}.jpg", headImage);
            generalName.text = general.generalName;
            level.text = "LV." + general.level;
            grade.text = general.GetGeneralGradeS();
            army.text = general.GetArmyS();
            phase.text = general.phase.ToString();
            curPhysicl.text = general.health.ToString();
            lead.text = general.lead.ToString();
            force.text = general.force.ToString();
            IQ.text = general.wisdom.ToString();
            political.text = general.govern.ToString();
            moral.text = general.charm.ToString();
            loyalty.text = general.loyalty.ToString();
            soldier.text = general.soldiers.ToString();
            weapon.text = WeaponListCache.GetWeapon(general.arm).weaponName;
            armor.text = WeaponListCache.GetWeapon(general.armor).weaponName;
            skills.text = general.GetActiveSkills();

            levelExpBar.fillAmount = (float)(general.experience / (float)general.GetMaxExp());
            phaseBar.transform.rotation = Quaternion.Euler(0, 0, (-(float)general.phase + 6) * (360f / 149f));
            curPhysicalBar.fillAmount = (float)(general.health / 100f);
            leadExpBar.fillAmount = (float)general.leadExp / 100f;
            forceExpBar.fillAmount = (float)general.forceExp / 100f;
            IQBar.fillAmount = (float)general.wisdomExp / 100f;
            politicalExpBar.fillAmount = (float)general.governExp / 100f;
            moralExpBar.fillAmount = (float)general.charmExp / 100f;
            loyaltyBar.fillAmount = (float)general.loyalty / 100f;
            soldierBar.fillAmount = (float)(general.soldiers / (float)general.GetMaxSoldierNum());

        }

        private void HideIntelligencePanel()
        {
            DataManager.Release(headImage.sprite);
            WarManager.Instance.hmUnitObj = null;
            gameObject.SetActive(false);
        }

    }
}
