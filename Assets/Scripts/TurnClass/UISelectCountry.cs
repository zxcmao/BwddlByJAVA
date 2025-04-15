using UnityEngine;
using UnityEngine.UIElements;


namespace SelectCountry.UI
{
    /*public class SelectCountry : MonoBehaviour
    {
        VisualElement rootVisualElement;
        private ScrollView scrollView;
        private VisualElement Head;
        private UnityEngine.UIElements.Label KingNameLabel;
        private UnityEngine.UIElements.Label LevelLabel;
        private UnityEngine.UIElements.Label GradeLabel;
        private UnityEngine.UIElements.Label PhaseLabel;
        private UnityEngine.UIElements.Label LeadLabel;
        private UnityEngine.UIElements.Label PoliticalLabel;
        private UnityEngine.UIElements.Label forceLabel;
        private UnityEngine.UIElements.Label IQLabel;
        private UnityEngine.UIElements.Label PhysicalLabel;
        private UnityEngine.UIElements.Label MoralLabel;
        private UnityEngine.UIElements.Label SoldierLabel;
        private UnityEngine.UIElements.Label CityLabel;
        private UnityEngine.UIElements.Label WeaponLabel;
        private UnityEngine.UIElements.Label ArmorLabel;



        void Awake()
        {
            Debug.Log("Awake called");
            rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

            DataManagement.LoadAndInitializeData();

        }

        void Start()
        {

            // 获取 UIDocument 和 ScrollView

            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
            scrollView = root.Q<ScrollView>("ScrollView"); // 请替换为你的 ScrollView 名称
            Head = root.Q<VisualElement>("Head");
            KingNameLabel = root.Q<UnityEngine.UIElements.Label>("KingName");
            LevelLabel = root.Q<UnityEngine.UIElements.Label>("level");
            GradeLabel = root.Q<UnityEngine.UIElements.Label>("Grade");
            PhaseLabel = root.Q<UnityEngine.UIElements.Label>("phase");
            LeadLabel = root.Q<UnityEngine.UIElements.Label>("lead");
            PoliticalLabel = root.Q<UnityEngine.UIElements.Label>("Political");
            forceLabel = root.Q<UnityEngine.UIElements.Label>("force");
            IQLabel = root.Q<UnityEngine.UIElements.Label>("IQ");
            PhysicalLabel = root.Q<UnityEngine.UIElements.Label>("Physical");
            MoralLabel = root.Q<UnityEngine.UIElements.Label>("Moral");
            SoldierLabel = root.Q<UnityEngine.UIElements.Label>("Soldier");
            CityLabel = root.Q<UnityEngine.UIElements.Label>("City");
            WeaponLabel = root.Q<UnityEngine.UIElements.Label>("Weapon");
            ArmorLabel = root.Q<UnityEngine.UIElements.Label>("Armor");



            // 遍历 countryList
            foreach (Country getCanBeChooseCountryByIndex in CountryListCache.countryDictionary)
            {
                // 使用 countryKingId 获取对应的 General
                General general = GeneralListCache.GetGeneral(getCanBeChooseCountryByIndex.countryKingId);

                if (general != null)
                {
                    // 创建按钮并设置文本为 generalName
                    var button = new Button();
                    button.style.color = Color.white; // 设置字体颜色为白色
                    button.style.fontSize = 60; // 设置字体大小为60
                    button.style.backgroundColor = new Color(0, 0, 0, 0);
                    button.text = general.generalName;
                    button.name = "Button_" + getCanBeChooseCountryByIndex.countryId;

                    // 为按钮添加点击事件
                    button.clicked += () => OnButtonClicked(general, getCanBeChooseCountryByIndex);

                    // 将按钮添加到 ScrollView 中
                    scrollView.contentContainer.Add(button);
                }
                else
                {
                    Debug.LogWarning($"General with ID {getCanBeChooseCountryByIndex.countryKingId} not found for country ID {getCanBeChooseCountryByIndex.countryId}.");
                }
            }

            // 额外添加一个“自建”按钮到 ScrollView 的最后
            var customButton = new Button();
            customButton.style.color = Color.white; // 设置字体颜色为白色
            customButton.style.fontSize = 60; // 设置字体大小为60
            customButton.style.backgroundColor = new Color(0, 0, 0, 0); // 设置自建按钮的背景颜色
            customButton.text = "自建"; // 自定义按钮的文本
            customButton.name = "Custom";

            // 为自建按钮添加点击事件
            //customButton.clicked += () => OnCustomButtonClicked();//

            // 将自建按钮添加到 ScrollView 中
            scrollView.contentContainer.Add(customButton);


        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (rootVisualElement != null)
                {
                    rootVisualElement.style.display = rootVisualElement.style.display == DisplayStyle.Flex ?
                        DisplayStyle.None :
                        DisplayStyle.Flex;
                }
            }
        }

        private void OnButtonClicked(General general,Country getCanBeChooseCountryByIndex)
        {
            KingNameLabel.text = general.generalName;
            LevelLabel.text = "LV."+general.level.ToString();
            GradeLabel.text = general.getGeneralGradeS();
            PhaseLabel.text = "相性："+general.phase.ToString();
            LeadLabel.text = general.lead.ToString();
            PoliticalLabel.text = general.political.ToString();
            forceLabel.text = general.force.ToString();
            IQLabel.text = general.IQ.ToString();
            PhysicalLabel.text = "100";
            MoralLabel.text = general.moral.ToString();
            SoldierLabel.text = general.generalSoldier.ToString();
            CityLabel.text = getCanBeChooseCountryByIndex.GetHaveCityNum().ToString();
            WeaponLabel.text = WeaponListCache.GetWeapon(general.weapon).weaponName;
            ArmorLabel.text = WeaponListCache.GetWeapon(general.armor).weaponName;


            // 生成文件路径
            string pngPath = $"HeadImage/{general.generalId}";
            // 加载对应的图片
            Sprite HeadSprite = Resources.Load<Sprite>(pngPath);

            // 输出路径，检查是否正确
            Debug.Log($"加载的图像路径: {pngPath}");

            // 检查是否加载到图片
            if (HeadSprite != null)
            {
                // 创建一个 StyleBackground
                var background = new StyleBackground(HeadSprite);

                // 设置 VisualElement 的背景图像
                Head.style.backgroundImage = background;
            }
            else
            {
                Debug.LogError($"未找到与 generalId {general.generalId} 对应的图像文件。");
            }



        }
    }*/
}