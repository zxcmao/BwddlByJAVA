using System;
using System.Collections;
using System.Collections.Generic;
using BaseClass;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using War;

namespace Battle
{
    public class UIPreBattle : MonoBehaviour
    {
        private static UIPreBattle _instance;

        public static UIPreBattle Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<UIPreBattle>();
                    if (_instance == null)
                    {
                        Debug.LogError("UIPreBattle单例为空!请确保已添加到此场景中.");
                    }
                }
                return _instance;
            }
        }
        
        private UIPreBattle(){ }
        
        // 绑定的UI对象
        [SerializeField] private GameObject startBattlePanel;
        [SerializeField] private RawImage hmHead;
        [SerializeField] private RawImage aiHead;
        [SerializeField] private TextMeshProUGUI hmName;
        [SerializeField] private TextMeshProUGUI aiName;
        [SerializeField] private TextMeshProUGUI hmSoldierNum;
        [SerializeField] private TextMeshProUGUI aiSoldierNum;
        [SerializeField] private Toggle formation0;
        [SerializeField] private Toggle formation1;
        [SerializeField] private Toggle formation2;
        [SerializeField] private Toggle formation3;
        [SerializeField] private Toggle manualBattle;
        [SerializeField] private Toggle autoBattle;
        [SerializeField] private Slider attackBar0;
        [SerializeField] private Slider attackBar1;
        [SerializeField] private Slider attackBar2;
        [SerializeField] private Slider attackBar3;
        [SerializeField] private Slider defenseBar0;
        [SerializeField] private Slider defenseBar1;
        [SerializeField] private Slider defenseBar2;
        [SerializeField] private Slider defenseBar3;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private GameObject battleCaption;
        
        private short[] dataSet1;
        private short[] dataSet2;
        
        private General hmGeneral;
        private General aiGeneral;
        
        private TroopData[] hmTroopData;
        private TroopData[] aiTroopData;
        

        public void Init()
        {
            BattleManager.Instance.formationIndex = 0;
            hmGeneral = BattleManager.Instance.hmGeneral;
            aiGeneral = BattleManager.Instance.aiGeneral;
            ShowStartPanel();
        }
        
        private void ShowStartPanel()
        {
            gameObject.SetActive(true);
            formation0.onValueChanged.AddListener(isOn => OnToggleValueChanged(formation0, isOn));
            formation1.onValueChanged.AddListener(isOn => OnToggleValueChanged(formation1, isOn));
            formation2.onValueChanged.AddListener(isOn => OnToggleValueChanged(formation2, isOn));
            formation3.onValueChanged.AddListener(isOn => OnToggleValueChanged(formation3, isOn));
            InitData();
            ShowBattlePanel();
            BattleManager.Instance.InitTroops(hmTroopData, true);
            BattleManager.Instance.InitTroops(aiTroopData, false);
        }

   
        private void InitTroopData(ref TroopData[] troopData, bool isPlayer)
        {
            // 获取对应的将军和部队信息
            General general = isPlayer ? hmGeneral : aiGeneral;
            troopData = new TroopData[4];
            troopData[0] = new TroopData(TroopType.Captain, general.generalId, isPlayer);
            troopData[1] = new TroopData(TroopType.Cavalry, general.generalId, isPlayer);
            troopData[2] = new TroopData(TroopType.Archer, general.generalId, isPlayer);
            troopData[3] = new TroopData(TroopType.Infantry, general.generalId, isPlayer);
        }

        void FillTroopData(ref short[] dataSet, bool isPlayer)
        {
            TroopData[] troopData = isPlayer ? hmTroopData : aiTroopData;
            dataSet = new short[8];
            dataSet[0] = troopData[0].attackPower;
            dataSet[1] = troopData[0].defensePower;
            dataSet[2] = troopData[1].attackPower;
            dataSet[3] = troopData[1].defensePower;
            dataSet[4] = troopData[2].attackPower;
            dataSet[5] = troopData[2].defensePower;
            dataSet[6] = troopData[3].attackPower;
            dataSet[7] = troopData[3].defensePower;
        }

        // 获取战斗力信息
        void InitData()
        {
            // 初始化基础数据和指挥官数据
            InitTroopData(ref hmTroopData, true);
            InitTroopData(ref aiTroopData, false);

            // 填充数据集
            FillTroopData(ref dataSet1, true);
            FillTroopData(ref dataSet2, false);
            attackBar0.value = (float)dataSet1[0] / (dataSet1[0] + dataSet2[0]);
            defenseBar0.value = (float)dataSet1[1]/ (dataSet1[1] + dataSet2[1]);
            attackBar1.value = (float)dataSet1[2] / (dataSet1[2] + dataSet2[2]);
            defenseBar1.value = (float)dataSet1[3]/ (dataSet1[3] + dataSet2[3]);
            attackBar2.value = (float)dataSet1[4] / (dataSet1[4] + dataSet2[4]);
            defenseBar2.value = (float)dataSet1[5]/ (dataSet1[5] + dataSet2[5]);
            attackBar3.value = (float)dataSet1[6] / (dataSet1[6] + dataSet2[6]);
            defenseBar3.value = (float)dataSet1[7]/ (dataSet1[7] + dataSet2[7]);
            Debug.Log(String.Join(",", dataSet1));
            Debug.Log(String.Join(",", dataSet2));
        }
        
        // 战斗信息显示
        private void ShowBattlePanel()
        {
            hmHead.texture = Resources.Load<Texture2D>($"HeadImage/{hmGeneral.generalId}");
            aiHead.texture = Resources.Load<Texture2D>($"HeadImage/{aiGeneral.generalId}");
            hmName.text = hmGeneral.generalName;
            aiName.text = aiGeneral.generalName;
            hmSoldierNum.text = "兵力:\n" + hmGeneral.generalSoldier;
            aiSoldierNum.text = "兵力:\n" + aiGeneral.generalSoldier;
        }

        // 选择阵型
        private void OnToggleValueChanged(Toggle toggle, bool isOn)
        {
            if (isOn)
            {
                if (toggle == formation0)
                {
                    BattleManager.Instance.formationIndex = 0;
                }
                else if (toggle == formation1)
                {
                    BattleManager.Instance.formationIndex = 1;
                }
                else if (toggle == formation2)
                {
                    BattleManager.Instance.formationIndex = 2;
                }
                else if (toggle == formation3)
                {
                    BattleManager.Instance.formationIndex = 3;
                }
                Debug.Log("选择阵型" + BattleManager.Instance.formationIndex);
                // 选择阵型后设置战斗方式选项
                manualBattle.gameObject.SetActive(true);
                manualBattle.onValueChanged.RemoveAllListeners();
                manualBattle.onValueChanged.AddListener(ManualBattle);

                bool canAutoBattle = hmGeneral.generalSoldier >= 500;
                autoBattle.gameObject.SetActive(canAutoBattle);
                if (canAutoBattle)
                {
                    autoBattle.onValueChanged.RemoveAllListeners();
                    autoBattle.onValueChanged.AddListener(delegate { StartCoroutine(AutoBattle());});
                }
                else
                {
                    autoBattle.onValueChanged.RemoveAllListeners();
                }
            }
        }

        // 手动战斗
        private void ManualBattle(bool isOn)
        {
            SceneManager.LoadScene("BattleScene");
        }

        // 自动战斗
        private IEnumerator AutoBattle()
        {
            manualBattle.gameObject.SetActive(false);
            autoBattle.gameObject.SetActive(false);
            loadingBar.gameObject.SetActive(true);
            
            BattleManager.Instance.SimulatedBattle();
            Animator loadingAni = loadingBar.gameObject.GetComponent<Animator>();
            if (loadingAni != null)
            {
                loadingAni.SetTrigger("Loading");
            }

            battleCaption.gameObject.SetActive(true);
            battleCaption.gameObject.GetComponent<Button>().onClick.AddListener(OnEndButtonClick);
            battleCaption.gameObject.GetComponent<TextMeshProUGUI>().text = "战斗结束,点此处继续";
            yield return new WaitForSeconds(1.0f);
            hmSoldierNum.text = "兵力:\n" + hmGeneral.generalSoldier;
            aiSoldierNum.text = "兵力:\n" + aiGeneral.generalSoldier;
        }

        
        // 结束按钮
        private void OnEndButtonClick()
        {
            startBattlePanel.gameObject.SetActive(false);
            WarManager.Instance.battleState = BattleState.BattleOver;
            BattleManager.Instance.BattleOver();
            Debug.Log("小战场自动战斗战斗结束");
        }

        public void Hide()
        {
            formation0.onValueChanged.RemoveAllListeners();
            formation1.onValueChanged.RemoveAllListeners();
            formation2.onValueChanged.RemoveAllListeners();
            formation3.onValueChanged.RemoveAllListeners();
            gameObject.SetActive(false);
        }
    }
}
