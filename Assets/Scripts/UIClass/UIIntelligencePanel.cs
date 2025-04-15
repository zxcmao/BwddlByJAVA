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
            gameObject.GetComponent<Button>()?.onClick.AddListener(HideIntelligencePanel);
            General general = GeneralListCache.GetGeneral(generalId);

            headImage.texture = Resources.Load<Texture2D>($"HeadImage/{generalId}");
            generalName.text = general.generalName;
            level.text = "LV." + general.level;
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

            levelExpBar.fillAmount = (float)(general.experience / (float)general.GetMaxExp());
            phaseBar.transform.rotation = Quaternion.Euler(0, 0, (-(float)general.phase + 6) * (360f / 149f));
            curPhysicalBar.fillAmount = (float)(general.curPhysical / 100f);
            leadExpBar.fillAmount = (float)general.leadExp / 100f;
            forceExpBar.fillAmount = (float)general.forceExp / 100f;
            IQBar.fillAmount = (float)general.IQExp / 100f;
            politicalExpBar.fillAmount = (float)general.politicalExp / 100f;
            moralExpBar.fillAmount = (float)general.moralExp / 100f;
            loyaltyBar.fillAmount = (float)general.loyalty / 100f;
            soldierBar.fillAmount = (float)(general.generalSoldier / (float)general.GetMaxSoldierNum());

        }

        private void HideIntelligencePanel()
        {
            WarManager.Instance.hmUnitObj = null;
            gameObject.SetActive(false);
        }

    }
}
