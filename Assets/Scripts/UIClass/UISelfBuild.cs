using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UIClass
{
    public class UISelfBuild : MonoBehaviour
    {
        [SerializeField] RawImage headImage;
        [SerializeField] TMP_InputField generalName;
        [SerializeField] TextMeshProUGUI grade;
        [SerializeField] TextMeshProUGUI army;
        [SerializeField] Image phaseBar;
        [SerializeField] Text phase;
        [SerializeField] Text curPhysical;
        [SerializeField] Text lead;
        [SerializeField] Text force;
        [SerializeField] Text IQ;
        [SerializeField] Text political;
        [SerializeField] Text charm;
        [SerializeField] TextMeshProUGUI skills;

        [SerializeField] Button resetButton;
        [SerializeField] Button attributeButton;
        [SerializeField] Button skillButton;
        [SerializeField] Button buildButton;
        [SerializeField] Button backButton;
        
        private General _general;
        private short _id;
        
        private void Start()
        {
            _general = new General(generalName.text, GenerateCharacterStatus(), GenerateCharacterAttributes(), GenerateCharacterSkills());
            ShowGeneralInfo();
            generalName.onValueChanged.AddListener(RenameCharacter);
            resetButton.onClick.AddListener(ResetCharacterStatus);
            attributeButton.onClick.AddListener(ChangeCharacterAttributes);
            skillButton.onClick.AddListener(ChangeCharacterSkills);
            buildButton.onClick.AddListener(OnBuildButtonClick);
            backButton.onClick.AddListener(OnBackButtonClick);
        }



        private void RenameCharacter(string arg0)
        {
            if (generalName.text != null)
            {
                _general.generalName = arg0;
                ShowGeneralInfo();
            }
        }


        private void ResetCharacterStatus()
        {
            byte[] status = GenerateCharacterStatus();
            _id = (short)Random.Range(0, 21);
            headImage.texture = Resources.Load<Texture>($"HeadImage/SelfHead/{_id}");
            _general.army[0] = status[0];
            _general.army[1] = status[1];
            _general.army[2] = status[2];
            _general.phase = status[3];
            ShowGeneralInfo();
        }

        private void ChangeCharacterAttributes()
        {
            List<byte> attributes = GenerateCharacterAttributes();
            _general.lead = attributes[0];
            _general.political = attributes[1];
            _general.force = attributes[2];
            _general.IQ = attributes[3];
            _general.moral = attributes[4];
            ShowGeneralInfo();
        }

        private void ChangeCharacterSkills()
        {
            List<short> skills = GenerateCharacterSkills();
            _general.skills = skills.ToArray();
            ShowGeneralInfo();
        }
        private void ShowGeneralInfo()
        {
            generalName.text = _general.generalName;
            grade.text = _general.GetGeneralGradeS();
            army.text = _general.GetArmyS();
            phase.text = _general.phase.ToString();
            phaseBar.transform.rotation = Quaternion.Euler(0, 0, (-(float)_general.phase + 6) * (360f / 149f)); 
            curPhysical.text = _general.curPhysical.ToString();
            lead.text = _general.lead.ToString();
            force.text = _general.force.ToString();
            IQ.text = _general.IQ.ToString();
            political.text = _general.political.ToString();
            charm.text = _general.moral.ToString();
            skills.text = _general.GetActiveSkills();
        }
        /// <summary>
        /// 随机生成将领适应性和相性
        /// </summary>
        /// <returns>技能组</returns>
        private static byte[] GenerateCharacterStatus()
        {
            byte[] status = new byte[4]; // 4 个技能组
            for (var i = 0; i < 3; i++)
            {
                status[i] = (byte)Random.Range(0, 4);
            }
            status[3] = (byte)Random.Range(0, 151);
            return status;
        }
        
        /// <summary>
        /// 生成正态分布的随机数
        /// </summary>
        /// <param name="mean">均值</param>
        /// <param name="stdDev">标准差</param>
        /// <returns>随机数</returns>
        private static double GenerateGaussianRandom(double mean, double stdDev)
        {
            double u1 = Random.value;
            double u2 = Random.value;
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // 标准正态分布
            return mean + stdDev * randStdNormal; // 转换为指定均值和标准差的正态分布
        }
        
        /// <summary>
        /// 生成五维属性（统帅、智力、武力、政治、魅力）
        /// </summary>
        /// <returns>五维属性的列表</returns>
        public static List<byte> GenerateCharacterAttributes()
        {
            const double mean = 60; // 属性均值
            const double stdDev = 30; // 标准差，控制分布的离散程度
            const int minValue = 10; // 属性的最小值
            const int maxValue = 90; // 属性的最大值

            List<byte> attributes = new List<byte>();

            for (int i = 0; i < 5; i++) // 五维属性
            {
                byte value;
                do
                {
                    value = (byte)Math.Round(GenerateGaussianRandom(mean, stdDev)); // 生成正态分布的值
                } while (value < minValue || value > maxValue); // 约束值在范围内

                attributes.Add(value);
            }
            Debug.Log(String.Join(",", attributes));
            return attributes;
        }
        
        /// <summary>
        /// 随机生成技能数量，符合正态分布
        /// </summary>
        /// <returns>技能数量</returns>
        private static int GenerateSkillCount()
        {
            const double mean = 5; // 平均技能数
            const double stdDev = 2; // 标准差
            int skillCount = Mathf.Clamp((int)Math.Round(GenerateGaussianRandom(mean, stdDev)), 1, 10);
            return skillCount;
        }
        
        /// <summary>
        /// 随机生成技能标识
        /// </summary>
        /// <param name="totalSkills">技能的总数（假设最多有 5 * 10 = 50 个技能）</param>
        /// <param name="skillCount">要生成的技能数量</param>
        /// <returns>技能标识集合</returns>
        private static HashSet<int> GenerateRandomSkillFlags(int totalSkills, int skillCount)
        {
            List<int> allSkills = Enumerable.Range(0, totalSkills).ToList(); // 技能标识池
            HashSet<int> selectedSkills = new HashSet<int>();

            // 洗牌并选取前 skillCount 个
            for (int i = 0; i < skillCount; i++)
            {
                int index = Random.Range(i, allSkills.Count);
                selectedSkills.Add(allSkills[index]);
                (allSkills[i], allSkills[index]) = (allSkills[index], allSkills[i]); // 交换
            }

            return selectedSkills;
        }

        /// <summary>
        /// 将技能标识分配到技能组
        /// </summary>
        /// <param name="selectedSkills">选择的技能标识集合</param>
        /// <returns>技能组</returns>
        private static List<short> AssignSkillsToGroups(HashSet<int> selectedSkills)
        {
            // 初始化包含 5 个值为 0 的元素的列表
            List<short> skills = Enumerable.Repeat((short)0, 5).ToList();
    
            foreach (int skill in selectedSkills)
            {
                int groupIndex = skill / 10; // 每组最多 10 个技能
                int bitPosition = skill % 10;
                skills[groupIndex] |= (short)(1 << bitPosition); // 使用位操作设置对应技能
            }
            return skills;
        }

        /// <summary>
        /// 随机生成技能组
        /// </summary>
        /// <returns>包含技能位元标识的技能组</returns>
        private static List<short> GenerateCharacterSkills()
        {
            int totalSkills = 50; // 假设最多 5 组，每组最多 10 个技能
            int skillCount = GenerateSkillCount(); // 随机生成技能数量
            HashSet<int> selectedSkills = GenerateRandomSkillFlags(totalSkills, skillCount); // 随机选取技能标识
            return AssignSkillsToGroups(selectedSkills); // 分配技能到技能组
        }

        private void OnBuildButtonClick()
        {
            DataManagement.AddCustomGeneral(_general, _id);
            gameObject.SetActive(false);
        }
        
        private void OnBackButtonClick()
        {
            gameObject.SetActive(false);
        }
    
    }
}