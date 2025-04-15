using System;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIClass
{
    public class UISmithy : MonoBehaviour
    {
        [SerializeField] private Toggle sword;
        [SerializeField] private Toggle knife;
        [SerializeField] private Toggle spear;
        [SerializeField] private Toggle armor;
        [SerializeField] private Transform weaponRoom;
        [SerializeField] private GameObject weaponOptionPrefab;
        [SerializeField] private Button confirmButton;
        
        private City _city;
        private short _gold;
        private General _general;
        private Weapon _weapon;
        private List<Weapon> _swordList = new();
        private List<Weapon> _knifeList = new();
        private List<Weapon> _spearList = new();
        private List<Weapon> _armorList = new();
        
        public event Action<short, string> GoSmithy;

        public void Smithy(byte cityID, short gold, bool isInWar, short generalID)
        {
            _gold = gold;
            _city = CityListCache.GetCityByCityId(cityID);
            _general = GeneralListCache.GetGeneral(generalID);
            byte citySmithy = isInWar ? _city.warWeaponShop : _city.cityWeaponShop;
            gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(true);
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            var allWeapons = WeaponListCache.GetWeaponsBySmithy(citySmithy);

            // 使用 LINQ 将武器分类
            _swordList = allWeapons.Where(w => w.weaponType == 0).ToList(); // 剑
            _knifeList = allWeapons.Where(w => w.weaponType == 1).ToList(); // 刀
            _spearList = allWeapons.Where(w => w.weaponType == 2).ToList(); // 矛
            _armorList = allWeapons.Where(w => w.weaponType == 3).ToList(); // 护甲
            

            // 为每个 Toggle 添加监听器
            sword.onValueChanged.AddListener(delegate { WeaponTypeChanged(sword); });
            knife.onValueChanged.AddListener(delegate { WeaponTypeChanged(knife); });
            spear.onValueChanged.AddListener(delegate { WeaponTypeChanged(spear); });
            armor.onValueChanged.AddListener(delegate { WeaponTypeChanged(armor); });
            WeaponTypeChanged(sword);
        }
        
        
        void WeaponTypeChanged(Toggle weaponTypeToggle)
        {
            _weapon = null;
            ClearWeaponRoom(); // 清空之前生成的 Toggle

            List<Weapon> goods = new List<Weapon>();

            if (sword.isOn)
            {
                goods = new List<Weapon>(_swordList);
            }
            else if (knife.isOn)
            {
                goods = new List<Weapon>(_knifeList);
            }
            else if (spear.isOn)
            {
                goods = new List<Weapon>(_spearList);
            }
            else if (armor.isOn)
            {
                goods = new List<Weapon>(_armorList);
            }
            
            foreach (var weapon in goods)
            {
                GameObject toggleObj = Instantiate(weaponOptionPrefab, weaponRoom);
                Toggle toggle = toggleObj.GetComponent<Toggle>();
                // 添加组件
                TextMeshProUGUI[] columns = toggle.GetComponentsInChildren<TextMeshProUGUI>();
                if (columns.Length >= 4)
                {
                    columns[0].text = weapon.weaponName;     // 武器名称
                    columns[1].text = weapon.weaponProperties.ToString(); // 属性
                    columns[2].text = weapon.weaponWeight.ToString();   // 重量
                    columns[3].text = weapon.weaponPrice.ToString();    // 价格
                }
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(delegate{OnWeaponToggleValueChanged(toggle, toggle.isOn, weapon);} );
            }
            
        }

        void ClearWeaponRoom()
        {
            // 遍历并销毁 weaponList 下的所有子物体
            foreach (Transform child in weaponRoom)
            {
                Destroy(child.gameObject);
            }
        }

        void OnWeaponToggleValueChanged(Toggle selectedToggle, bool isOn, Weapon weapon)
        {
            // 获取当前行的所有文本
            TextMeshProUGUI[] texts = selectedToggle.GetComponentsInChildren<TextMeshProUGUI>();

            if (isOn)
            {
                _weapon = weapon;
                // 将当前行的文本变为红色
                SetTextColor(texts, Color.red);

                // 单选逻辑：取消其他行的选中状态，并恢复颜色
                foreach (var toggle in weaponRoom.transform.GetComponentsInChildren<Toggle>())
                {
                    if (toggle != selectedToggle)
                    {
                        toggle.isOn = false; // 取消选中
                        SetTextColor(toggle.GetComponentsInChildren<TextMeshProUGUI>(), Color.white); // 恢复默认颜色
                    }
                }
            }
            else
            {
                _weapon = null;
                // 如果取消选中，恢复当前行的文本颜色
                SetTextColor(texts, Color.white);
            }
        }

        private void SetTextColor(TextMeshProUGUI[] texts, Color color)
        {
            foreach (var text in texts)
            {
                text.color = color;
            }
        }

        private void OnConfirmButtonClicked()
        {
            string text = "退出武器铺！";
            if (_weapon != null)
            {
                if (_gold >= _weapon.weaponPrice)
                {
                    if (_general.BuyNewWeapon(_weapon.weaponId, _city.cityId))
                    { 
                        _gold -= _weapon.weaponPrice;
                        text = TextLibrary.DoThingsResultInfo[7][4]; 
                    }
                    else
                    {
                        text = TextLibrary.DoThingsResultInfo[7][5]; 
                    }
                }
                else
                {
                    text = TextLibrary.DoThingsResultInfo[1][2]; 
                }
            }
            GoSmithy?.Invoke(_gold, text);
            gameObject.SetActive(false);
        }
    }
}