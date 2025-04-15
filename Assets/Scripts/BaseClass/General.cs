using System;
using System.Collections.Generic;
using System.Text;
using DataClass;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BaseClass
{
    [System.Serializable]
    public class General  // 武将类
    {
        public short generalId;         // 武将ID
        private string headImage;        // 头像图片路径
        public string generalName;      // 武将姓名
        public byte lead;               // 领导力
        public byte political;          // 政治能力
        public short phase;             // 当前阶段
        public byte isOffice;           // 是否在职
        public byte level;              // 等级
        [JsonProperty("army[]")] public byte[] army = new byte[3];  // 军队构成
        public byte force;              // 武力值
        public short generalSoldier;    // 所属士兵数量                               
        public int maxAttributeValue = 120; // 最大属性值
        public byte moral;              // 德行
        public byte curPhysical; // 当前体力
        public byte maxPhysical = 100;  // 最大体力
        public byte IQ;                 // 智商
        public short debutYears;        // 出道年份
        public byte loyalty=99;           // 忠诚度
        public byte debutCity;          // 出道城市
        public short followGeneralId;   // 跟随的武将ID
        public int experience;        // 经验值
        public byte weapon;             // 武器
        public byte armor;              // 铠甲
        [JsonProperty("skills[]")] public short[] skills = new short[5]; // 技能列表
        public byte leadExp;            // 领导力经验值
        public byte forceExp;           // 武力经验值
        public byte IQExp;              // 智商经验值
        public byte moralExp;           // 德行经验值
        public byte politicalExp;       // 政治能力经验值
        private byte salary = 1;        // 工资
        public short haveMoney = 0;     // 拥有的金钱
        public bool IsDie = false;      // 是否死亡
        public string profile = "";     // 个人简介

        // JSON反序列化的构造函数
        public General() { }
        // 自建的构造函数
        public General(string name, byte[] status, List<byte> attributes, List<short> skills)
        {
            generalName = name;
            lead = attributes[0];
            political = attributes[1];
            force = attributes[2];
            IQ = attributes[3];
            moral = attributes[4];
            army[0] = status[0];
            army[1] = status[1];
            army[2] = status[2];
            phase = status[3];
            curPhysical = 100;
            loyalty = 99;
            level = 1;
            debutYears = GameInfo.years;
            weapon = 1;
            armor = 1;
            
            for (var i = 0; i < skills.Count; i++)
            {
                skills[i] = skills[i];
            }
        }

        // 根据数组中的值生成特定格式的字符串
        public string GetArmyS()
        {
            // 定义每个数字对应的后缀字符
            char[] suffixes = { 'C', 'B', 'A', 'S' }; // 假设0-3分别对应'C', 'S', 'A', 'B'

            // 定义前缀字符串数组
            string[] prefixes = { "平", "山", "水" };

            // 使用StringBuilder来构建结果字符串
            StringBuilder result = new StringBuilder();

            // 确保数组长度不超过prefixes数组长度
            int Length = Mathf.Min(army.Length, prefixes.Length);

            // 遍历数组中的每个元素
            for (int i = 0; i < Length; i++)
            {
                // 检查数组值是否在suffixes数组的索引范围内
                if (army[i] < suffixes.Length)
                {
                    // 添加前缀和对应的后缀字符
                    result.Append(prefixes[i]);
                    result.Append(suffixes[army[i]]);
                }
                else
                {
                    // 如果数组值超出范围，可以添加错误处理，例如追加一个错误字符或者忽略
                    result.Append("?");
                }
            }

            return result.ToString();
        }


        /// <summary>
        /// 获取当前体力值
        /// </summary>
        /// <returns>当前体力值</returns>
        public byte GetCurPhysical()
        {
            return curPhysical;
        }

        /// <summary>
        /// 设置当前体力值
        /// </summary>
        /// <param name="curPhysical">新的体力值</param>
        public void SetCurPhysical(byte curPhysical)
        {
            if (curPhysical > maxPhysical)
            {
                curPhysical = maxPhysical;
            }
            else if (curPhysical < 0)
            {
                curPhysical = 0;
            }

            curPhysical = curPhysical;
        }

        /// <summary>
        /// 增加体力值
        /// </summary>
        /// <param name="physical">增加的体力值</param>
        public void AddCurPhysical(byte physical)
        {
            if (physical > maxPhysical - curPhysical)
            {
                physical = (byte)(maxPhysical - curPhysical);
            }

            curPhysical = (byte)(curPhysical + physical);
        }

        /// <summary>
        /// 减少体力值
        /// </summary>
        /// <param name="physical">减少的体力值</param>
        /// <returns>是否体力降为0</returns>
        public bool SubHp(int physical)
        {
            if (curPhysical <= physical)
            {
                curPhysical = 0;
                return true;
            }
            curPhysical = (byte)(curPhysical - physical);
            return false;
        }



        /// <summary>
        /// 获取调整后的薪水
        /// </summary>
        /// <returns>返回调整后的薪水</returns>
        public byte GetSalary()
        {
            return (byte)(salary + level * 2);
        }

        /// <summary>
        /// 设置忠诚度，最大值为100
        /// </summary>
        /// <param name="tempLoyalty">新的忠诚度值</param>
        public void SetLoyalty(byte tempLoyalty)
        {
            if (tempLoyalty > 100)
            {
                tempLoyalty = 100;
            }
            loyalty = tempLoyalty;
        }

        /// <summary>
        /// 获取当前忠诚度
        /// </summary>
        /// <returns>返回当前忠诚度</returns>
        public byte GetLoyalty()
        {
            return loyalty;
        }

        /// <summary>
        /// 减少忠诚度，最小值为0
        /// </summary>
        /// <param name="tempLoyalty">减少的忠诚度值</param>
        public void DecreaseLoyalty(byte tempLoyalty)
        {
            if (loyalty - tempLoyalty < 0)
            {
                loyalty = 0;
            }
            else
            {
                loyalty = (byte)(loyalty - tempLoyalty);
            }
        }

        /// <summary>
        /// 增加忠诚度，最大值为100
        /// </summary>
        /// <param name="tempLoyalty">增加的忠诚度值</param>
        public void AddLoyalty(byte tempLoyalty)
        {
            if (tempLoyalty + loyalty >= 100)
            {
                Country country = CountryListCache.GetCountryByKingId(generalId);
                if (country == null)
                {
                    loyalty = 99;
                }
                else
                {
                    loyalty = 100;
                }
            }
            else
            {
                loyalty = (byte)(loyalty + tempLoyalty);
            }
        }

        /// <summary>
        /// 根据条件增加忠诚度
        /// </summary>
        /// <param name="useMoney">是否使用金钱</param>
        /// <returns>返回忠诚度的变化量</returns>
        public byte RewardAddLoyalty(bool useMoney)
        {
            byte tempLoyalty = loyalty;
            if (loyalty < 30)
            {
                loyalty = (byte)(loyalty + Random.Range(0, 11) + 20);
            }
            else if (loyalty < 50)
            {
                loyalty = (byte)(loyalty + Random.Range(0, 11) + 10);
            }
            else if (loyalty < 70)
            {
                loyalty = (byte)(loyalty + Random.Range(0, 11) + 5);
            }
            else if (loyalty < 80)
            {
                loyalty = (byte)(loyalty + Random.Range(0, 8) + 3);
            }
            else if (loyalty < 85)
            {
                loyalty = (byte)(loyalty + Random.Range(0, 7) + 1);
            }
            else if (loyalty < 90)
            {
                loyalty = (byte)(loyalty + Random.Range(0, 5) + 1);
            }
            if (!useMoney)
            {
                loyalty = (byte)(loyalty + Random.Range(5, 15));
            }
            if (loyalty > 99)
            {
                loyalty = 99;
            }
            return (byte)(loyalty - tempLoyalty);
        }
        
        /// <summary>
        /// 随机设置俘虏、笼络来的将领忠诚度
        /// </summary>
        public void SetTraitorLoyalty()
        {
            SetLoyalty((byte)Random.Range(40, 75));
        }


        /// <summary>
        /// 获取升级到下一级所需的最大经验值
        /// </summary>
        /// <returns>最大经验值</returns>
        public int GetMaxExp()
        {
            switch (level)
            {
                case 1: return 6000;
                case 2: return 10000;
                case 3: return 14000;
                case 4: return 18000;
                case 5: return 22000;
                case 6: return 26000;
                case 7: return 30000;
                case 8: return short.MaxValue;
                default: return short.MaxValue;
            }
        }

        /// <summary>
        /// 向玩家添加经验值
        /// </summary>
        /// <param name="exp">要添加的经验值</param>
        public void Addexperience(int exp)
        {
            while (exp > 0)
            {
                exp--;
                experience++;

                // 检查当前经验值是否大于或等于最大经验值
                if (experience >= GetMaxExp())
                {
                    // 升级后将经验值重置为零
                    experience = 0;
                    LevelUp();
                }
            }
        }

        /// <summary>
        /// 处理升级过程
        /// </summary>
        private void LevelUp()
        {
            level++;
            GeneralUpgrade();
        }

        // 是否是势力君主
        public bool IsKing()
        {
            foreach (var country in CountryListCache.countryDictionary)
            {
                if (country.Value.countryKingId == generalId)
                {
                    return true;
                }
            }
            return false;
        }
    
        /// <summary>
        /// 计算单个武将的总战斗力
        /// </summary>
        /// <param name="generalId"></param>
        /// <returns></returns>
        public int CalculateWarValue()
        {
            return (force + IQ + lead * 2) / 2 * (generalSoldier + 150);
        }
        
        // 获取武将单挑的攻击力
        public short GetAttackPower()
        {
            byte weaponProperties = WeaponListCache.GetWeapon(weapon).weaponProperties;
            return (short)(force * (1 + weaponProperties / 100f) * (level + 19) / 20f);
        }

        // 获取武将单挑的防御力
        public short GetDefendPower()
        {
            byte armorProperties = WeaponListCache.GetWeapon(armor).weaponProperties;
            return (short)(force * (1 + armorProperties / 100f) * (level + 19) / 20f);
        }

        /// <summary>
        /// 获取战斗能力
        /// </summary>
        /// <returns>返回计算后的战斗力</returns>
        public double GetBattlePower()
        {
            double power = 0.0D;

            // 获取武器对象
            Weapon weaponObj = WeaponListCache.GetWeapon(weapon);
            if (weaponObj != null)
            {
                // 计算武器对战斗力的影响
                power += weaponObj.weaponProperties * 1.3D;
            }

            // 获取防具对象
            // 注意: 通常防具不会从武器列表中获取，这里假设是正确的逻辑
            Weapon armorObj = WeaponListCache.GetWeapon(armor);
            if (armorObj != null)
            {
                // 计算防具对战斗力的影响
                power += armorObj.weaponProperties * 1.3D;
            }

            // 计算其他属性对战斗力的影响
            power += force * 1.3D + IQ * 1.2D + curPhysical * 1.1D + (generalSoldier / 5);

            return power;
        }

        /// <summary>
        /// 获取将领得分
        /// </summary>
        /// <returns>返回将领的各项属性总和</returns>
        public int AllStatus()
        {
            return lead + political + force + moral + IQ;
        }

        /// <summary>
        /// 获取将领等级
        /// </summary>
        /// <returns>返回将领的等级</returns>
        public byte GetGeneralGrade()
        {
            int generalScore = AllStatus();
            byte grade = 0;

            if (generalScore <= 196)
            {
                grade = 0; // 一无所长
            }
            if (generalScore >= 197 && generalScore <= 282)
            {
                grade = 1; // 才疏学浅
            }
            if (generalScore >= 283 && generalScore <= 345)
            {
                grade = 2; // 中流砥柱
            }
            if (generalScore >= 346 && generalScore <= 421)
            {
                grade = 3; // 出类拔萃
            }
            if (generalScore >= 422)
            {
                grade = 4; // 天生奇才(关羽及以上)
            }

            // 检查是否有任何一个属性超过95，且当前评级低于4
            if (lead >= 95 || political >= 95 || force >= 95 || moral >= 95 || IQ >= 95)
            {
                if (grade < 4)
                {
                    grade = 5; // 独步天下
                }
            }

            return grade;
        }

        public string GetGeneralGradeS()
        {
            // 调用已有的getGeneralGrade方法获取等级数值
            byte grade = GetGeneralGrade();

            // 根据等级数值返回相应的描述
            switch (grade)
            {
                case 0:
                    return "愚钝";
                case 1:
                    return "平庸";
                case 2:
                    return "良才";
                case 3:
                    return "精英";
                case 4:
                    return "奇才";
                default:
                    return "怪才";
            }
        }

        /// <summary>
        /// 将领升级
        /// </summary>
        public void GeneralUpgrade()
        {
            if (level >= 8)
            {
                // 如果等级已经达到最大值，则不进行升级
                return;
            }

            level = (byte)(level + 1); // 提升等级
            curPhysical = 100; // 回复体力

            byte grade = GetGeneralGrade(); // 获取当前等级

            // 根据等级随机增加属性点
            int totalValue = Random.Range(0, grade) + 1;
            for (int i = 0; i < totalValue; i++)
            {
                int index = Random.Range(0, 5);
                AddGeneralAttributeValue(index, 0); // 注意: 第二个参数为0，可能需要修改为实际增加的属性点数
            }
        }



        // 向武将添加属性值
        public void AddGeneralAttributeValue(int attributeIndex, int iterationCount)
        {
            // 如果迭代次数达到5次，则退出递归
            if (iterationCount >= 5) return;

            switch (attributeIndex)
            {
                // 如果领导力小于最大值，则增加领导力
                case 0 when lead < maxAttributeValue:
                    lead++;
                    break;
                // 如果政治力小于最大值，则增加政治力
                case 1 when political < maxAttributeValue:
                    political++;
                    break;
                // 如果武力小于最大值，则增加武力
                case 2 when force < maxAttributeValue:
                    force++;
                    break;
                // 如果道德小于最大值，则增加道德
                case 3 when moral < maxAttributeValue:
                    moral++;
                    break;
                // 如果智力小于最大值，则增加智力
                case 4 when IQ < maxAttributeValue:
                    IQ++;
                    break;
                // 默认情况，设置属性索引为0
                default:
                    attributeIndex = 0;
                    break;
            }

            // 如果属性索引不是0，则递增属性索引并继续递归
            if (attributeIndex != 0)
            {
                attributeIndex++;
                iterationCount++;
                AddGeneralAttributeValue(attributeIndex, iterationCount);
            }
        }


        /// <summary>
        /// 获取武将的最大士兵数
        /// </summary>
        /// <returns>武将的最大士兵上限</returns>
        public short GetMaxSoldierNum()
        {
            // 检查第 5 个技能（索引为 4）统帅是否解锁
            if (((skills[4] >> 10 - 0) & 0x1) == 1)
            {
                // 如果特定技能被解锁，返回 3000
                return 3000;
            }
            else
            {
                // 如果特定技能未解锁，根据领导力和等级计算士兵数量
                return (short)(1000 + 12 * lead + 10 * level);
            }
        }
        
        /// <summary>
        /// 增加士兵数量
        /// </summary>
        /// <param name="soldier"></param>
        public void AddSoldier(int soldier)
        {
            generalSoldier += (short)soldier;
            if (generalSoldier > GetMaxSoldierNum())
            {
                generalSoldier = GetMaxSoldierNum();
            }
        }

        /// <summary>
        /// 减少士兵数量
        /// </summary>
        /// <param name="soldier"></param>
        public void SubSoldier(int soldier)
        {
            generalSoldier -= (short)soldier;
            if (generalSoldier < 0)
            {
                generalSoldier = 0;
            }
        }

        /// <summary>
        /// 获取战争综合能力值
        /// </summary>
        /// <returns>战争综合能力值</returns>
        public int GetWarValue()
        {
            // 计算公式
            return (int)(lead * 1.42f + IQ * 0.25f + force * 0.33f + ((lead * 2 + force + IQ) * (level - 1)) * 0.04f);
        }

        /// <summary>
        /// 获取武将的战斗力
        /// </summary>
        /// <returns>战斗力</returns>
        public int GetGeneralPower()
        {
            int power = 1;
            short satrapValue = (short)GetWarValue();
            long adjustedValue = (1 + satrapValue * satrapValue * satrapValue / 100000);

            if (generalSoldier < 100)
            {
                adjustedValue = Math.Min(100L, adjustedValue);
                return 0;
            }

            if (adjustedValue < 20L)
            {
                adjustedValue = Math.Max((generalSoldier / 150), adjustedValue);
            }

            power += (int)(adjustedValue * (generalSoldier + 1));
            return power;
        }

        /// <summary>
        /// 执行AI俘获将领到城市方法
        /// </summary>
        /// <param name="cityId"></param>
        public void CapturedGeneralTo(byte cityId)
        {
            byte addLoyalty = (byte)(100 - GetLoyalty()); // 计算增加的忠诚度
            byte baseLoyalty = (byte)(60 + addLoyalty); // 计算基础忠诚度
            if (baseLoyalty >= 99 || baseLoyalty <= 0)
            {
                SetLoyalty(99); // 如果基础忠诚度不在有效范围内，则设置为 99
            }
            else
            {
                SetLoyalty((byte)Random.Range(70, 91)); // 设置将军的忠诚度
            }
            CityListCache.GetCityByCityId(cityId).AddOfficeGeneralId(generalId); // 将将军 ID 添加到城市中
        }



        /// <summary>
        /// 增加统帅经验
        /// </summary>
        /// <param name="exp">增加的经验值</param>
        public void AddLeadExp(int exp)
        {
            if (lead >= 120)
            {
                leadExp = 100; // 达到最大值，经验归满
                return;
            }

            // 累计经验值
            int totalExp = leadExp + exp;

            // 计算增加的领导属性
            int newLead = totalExp / 100;
            lead += (byte)newLead;

            // 检查是否达到领导最大值
            if (lead >= 120)
            {
                lead = 120;
                leadExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            leadExp = (byte)(totalExp % 100);
        }
        
        /// <summary>
        /// 增加武力经验
        /// </summary>
        /// <param name="exp">增加的经验值</param>
        public void AddForceExp(int exp)
        {
            if (force >= 120)
            {
                forceExp = 100; // 达到最大值，经验归满
                return;
            }

            // 累计经验值
            int totalExp = forceExp + exp;

            // 计算增加的武力属性
            int newForce = totalExp / 100;
            force += (byte)newForce;

            // 检查是否达到武力最大值
            if (force >= 120)
            {
                force = 120;
                forceExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            forceExp = (byte)(totalExp % 100);
        }

        /// <summary>
        /// 增加智力经验值
        /// </summary>
        /// <param name="exp">增加的经验值</param>
        public void AddIqExp(int exp)
        {
            if (IQ >= 120)
            {
                IQExp = 100; // 达到最大值，经验归满
                return;
            }

            // 累计经验值
            int totalExp = IQExp + exp;

            // 计算增加的智力属性
            int newIQ = totalExp / 100;
            IQ += (byte)newIQ;

            // 检查是否达到智力最大值
            if (IQ >= 120)
            {
                IQ = 120;
                IQExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            IQExp = (byte)(totalExp % 100);
        }
        
        /// <summary>
        /// 增加政治经验值。
        /// </summary>
        /// <param name="exp">经验值</param>
        public void AddPoliticalExp(byte exp)
        {
            if (political >= 120)
            {
                politicalExp = 100; // 达到最大值，经验归满
                return;
            }
            
            // 累计经验值
            int totalExp = politicalExp + exp;

            // 计算增加的政治属性
            int newPolitical = totalExp / 100;
            political += (byte)newPolitical;

            // 检查是否达到政治最大值
            if (political >= 120)
            {
                political = 120;
                politicalExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            politicalExp = (byte)(totalExp % 100);
        }

        /// <summary>
        /// 增加道德经验值。
        /// </summary>
        /// <param name="exp">经验值</param>
        public void AddMoralExp(int exp)
        {
            if (moral >= 120)
            {
                moralExp = 100; // 达到最大值，经验归满
                return;
            }

            // 累计经验值
            int totalExp = moralExp + exp;

            // 计算增加的道德属性
            int newMoral = totalExp / 100;
            moral += (byte)newMoral;

            // 检查是否达到道德最大值
            if (moral >= 120)
            {
                moral = 120;
                moralExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            moralExp = (byte)(totalExp % 100);
        }

        public short GetLearnNeedExp()
        {
            short needExp = 0; // 初始化所需经验
            needExp = (short)(200 + IQ * 50); // 计算所需经验
            return needExp; // 返回所需经验
        }



        /// <summary>
        /// 开始学习
        /// </summary>
        public void StudyUp()
        {
            // 减少所需的经验值
            experience = (short)(experience - GetSchoolNeedExp());
            // 提升智力
            IQ = (byte)(IQ + 1);
            // 输出学习结果
            Debug.Log($"{generalName}学习后智力增加1");
        }

        /// <summary>
        /// 获取书院学习升智所需的最小经验值
        /// </summary>
        /// <returns>所需经验值</returns>
        public short GetSchoolNeedExp()
        {
            // 计算所需经验值
            return (short)(200 + IQ * 50);
        }

        /// <summary>
        /// 根据IQ和等级返回武将计划的数量
        /// </summary>
        /// <returns>返回计划数量</returns>
        public byte GetPlanNum()
        {
            // 如果智商大于等于100且等级大于等于8，则返回16
            if (IQ >= 100 && level >= 8)
                return 16;

            // 如果智商大于等于95且等级大于等于7，则返回15
            if (IQ >= 95 && level >= 7)
                return 15;

            // 如果智商大于等于93且等级大于等于5，则返回14
            if (IQ >= 93 && level >= 5)
                return 14;

            // 如果智商大于等于90且等级大于等于4，则返回12
            if (IQ >= 90 && level >= 4)
                return 12;

            // 如果智商大于等于85，则返回10
            if (IQ >= 85)
                return 10;

            // 如果智商大于等于80，则返回8
            if (IQ >= 80)
                return 8;

            // 如果智商大于等于60，则返回6
            if (IQ >= 60)
                return 6;

            // 如果智商小于40，则返回2；否则返回4
            return (byte)((IQ < 40) ? 2 : 4);
        }
        
        
        //在战场锻造坊购买某武器
        public bool BuyNewWeapon(byte weaponId, byte cityId)
        {
            Weapon newWeapon = WeaponListCache.GetWeapon(weaponId);
            City city = CityListCache.GetCityByCityId(cityId);
            if(newWeapon.weaponUnique)
            {
                if (newWeapon.weaponId == 15)//青龙偃月刀
                {
                    if (force >= 95)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponId == 21)//方天画戟
                {
                    if (force>= 95)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponId == 22)//狼牙棒
                {
                    if (force >= 90)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponId == 23)//丈八蛇矛
                {
                    if (force >= 95)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponId == 30)//圣者之衣
                {
                    if (IQ >= 90)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
            }
            GetNewWeapon(weaponId);
            return true;
        }

        /// <summary>
        /// 将领根据武器ID获得新武器
        /// </summary>
        /// <param name="weaponId"></param>
        public void GetNewWeapon(byte weaponId)
        {
            Weapon newWeapon=WeaponListCache.GetWeapon(weaponId) ;
            if(newWeapon.weaponType==3)
            {
                armor = weaponId;
            }
            else
            {
                weapon = weaponId;
            }
        }

        public string GetActiveSkills()
        {
            if (skills == null || skills.Length != 5)
            {
                Debug.LogError("技能数组不能为空且必须包含 5 个元素。");
                return string.Empty;
            }

            List<string> activeSkills = new List<string>();

            // 遍历 skills[] 数组
            for (int i = 0; i < skills.Length; i++)
            {
                short skillValue = skills[i];

                // 如果这一行的值为 0，跳过
                if (skillValue == 0)
                {
                    Debug.Log($"技能[{i}]的值为0，跳过。");
                    continue;
                }

                // 遍历每个 skillValue 的 10 位
                for (int bitPosition = 0; bitPosition < 11; bitPosition++)
                {
                    // 检查 skillValue 的第 (10 - bitPosition) 位是否为 1
                    if ((skillValue & (1 << (10 - bitPosition))) != 0)
                    {
                        int skillIndex = i * 10 + bitPosition;

                        // 确保索引在技能表的范围内
                        if (skillIndex < TextLibrary.skillNames.Length)
                        {
                            Debug.Log($"发现特技: {TextLibrary.skillNames[skillIndex]} (技能[{i}]的第 {bitPosition + 1} 位)");
                            activeSkills.Add(TextLibrary.skillNames[skillIndex]);
                        }
                        else
                        {
                            Debug.LogWarning($"技能索引超出范围: {skillIndex}");
                        }
                    }
                }
            }

            // 返回拼接后的特技字符串，使用两个空格分隔
            return string.Join("  ", activeSkills);
        }

        public int GetSkillCount()
        {
            if (skills == null || skills.Length != 5)
            {
                Debug.LogError("技能数组不能为空且必须包含 5 个元素。");
                return 0;
            }

            // 特技的映射表，50 个技能
            string[] skillNames = new string[]
            {
                "沉着", "鬼谋", "百出", "军师", "火攻", "神算", "反计", "待伏", "袭粮", "内讧",
                "骑神", "骑将", "弓神", "弓将", "水将", "乱战", "连弩", "金刚", "不屈", "猛将",
                "单骑", "奇袭", "铁壁", "攻城", "守城", "神速", "攻心", "精兵", "军魂", "军神",
                "王佐", "仁政", "屯田", "商才", "名士", "风水", "义军", "内助", "仁义", "抢运",
                "统领", "掠夺", "恐吓", "一骑", "水练", "能吏", "练兵", "言教", "冷静", "束缚"
            };

            int skillCount = 0;

            // 遍历 skills[] 数组
            for (int i = 0; i < skills.Length; i++)
            {
                short skillValue = skills[i];

                // 如果这一行的值为 0，跳过
                if (skillValue == 0)
                {
                    Debug.Log($"技能[{i}]的值为0，跳过。");
                    continue;
                }

                // 遍历每个 skillValue 的 10 位
                for (int bitPosition = 0; bitPosition < 10; bitPosition++)
                {
                    // 检查 skillValue 的第 (10 - bitPosition) 位是否为 1
                    if ((skillValue & (1 << (10 - bitPosition))) != 0)
                    {
                        int skillIndex = i * 10 + bitPosition;

                        // 确保索引在技能表的范围内
                        if (skillIndex < skillNames.Length)
                        {
                            Debug.Log($"发现特技: {skillNames[skillIndex]} (技能[{i}]的第 {bitPosition + 1} 位)");
                            skillCount++; // 增加技能数量计数
                        }
                        else
                        {
                            Debug.LogWarning($"技能索引超出范围: {skillIndex}");
                        }
                    }
                }
            }

            // 返回技能数量
            return skillCount;
        }


    

        /// <summary>
        /// 根据武将对象技能判定
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public bool HasSkill_1(int skillID)
        {
            return HasSkill(0, skillID);
        }

        public bool HasSkill_2(int skillID)
        {
            return HasSkill(1, skillID);
        }

        public bool HasSkill_3(int skillId)
        {
            return HasSkill(2, skillId);
        }

        public bool HasSkill_4(int skillID)
        {
            return HasSkill(3, skillID);
        }

        public bool HasSkill_5(int skillId)
        {
            return HasSkill(4, skillId);
        }

        public bool HasSkill(int kind, int index)
        {
            return ((skills[kind-1] >> 10 - index & 0x1) == 1);
        }



        /// <summary>
        /// 计算武将执行内政需要的金钱
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int GetNeedMoneyOfInterior(TaskType task)
        {
            int gold = 0;  // 用于存储计算结果
            switch (task)
            {
                case TaskType.Reclaim:
                    gold = (force + political) / 2;  // 计算力量与政治的平均值
                    break;
                case TaskType.Mercantile:
                    gold = (IQ + political) / 2;  // 计算智商与政治的平均值
                    break;
                case TaskType.Tame:
                    gold = (lead + political) / 2;  // 计算领导力与政治的平均值
                    break;
                case TaskType.Patrol:
                    gold = (moral + political) / 2;  // 计算道德与政治的平均值
                    break;
            }
            int needMoney = 10;  // 基础金钱需求
            needMoney += gold / 10;  // 根据计算结果增加金钱需求
            needMoney += 2 * (Random.Range(0,5) + 1);  // 加上随机值
            return needMoney;
        }

        /// <summary>
        /// 获取将领所属的君主ID
        /// </summary>
        /// <returns></returns>
        public short GetOfficeGenBelongKing()
        {
            short kingId = 0;
            City debutedCity = CityListCache.GetCityByCityId(debutCity); // 获取将领初次登场的城市
            short[] officeGeneralIdArray = debutedCity.GetOfficerIds(); // 获取城市的任职将领ID数组

            // 遍历城市的任职将领ID数组，查找将领是否在该城市任职
            for (int i = 0; i < officeGeneralIdArray.Length; i++)
            {
                if (officeGeneralIdArray[i] == generalId)
                {
                    return debutedCity.cityBelongKing; // 返回该城市所属的君主ID
                }
            }

            int inCount = 0; // 计数器
            string cityInfoString = ""; // 用于记录将领所在城市信息

            // 遍历所有城市，查找将领是否在其他城市任职
            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
            {
                City city = CityListCache.GetCityByCityId(cityId); // 获取城市对象

                // 遍历城市的任职将领ID数组
                for (byte index = 0; index < city.GetCityOfficerNum(); index = (byte)(index + 1))
                {
                    if (city.GetOfficerIds()[index] == generalId)
                    {
                        kingId = city.cityBelongKing; // 找到该城市所属的君主ID
                        inCount++; // 计数
                        cityInfoString += city.cityName; // 记录城市名称

                        // 如果将领初次登场城市与当前城市不一致，则更新信息
                        if (debutCity != cityId)
                        {
                            city.RemoveOfficerId(generalId); // 从旧城市移除将领任职信息
                            debutCity = cityId; // 更新将领的初次登场城市
                        }
                    }
                }
            }

            // 如果计数器大于0，输出将领的任职城市信息
            if (inCount > 0)
            {
                Debug.Log(generalName + "在" + cityInfoString + "任职！"); // 使用Unity的Debug.Log输出信息
            }

            return kingId; // 返回君主ID
        }
    }
}
