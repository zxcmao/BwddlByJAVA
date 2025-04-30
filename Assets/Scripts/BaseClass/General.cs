using System.Collections.Generic;
using System.Text;
using DataClass;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BaseClass
{
    [System.Serializable]
    public class General  // 武将类
    {
        public short generalId;                 // 武将ID
        public string generalName;              // 武将姓名
        public short debutYear;                 // 出道年份
        public byte debutCity;                  // 出道城市
        public byte phase;                      // 相性
        public byte status;                     // 身份
        public byte level;                      // 等级
        public byte[] army;                     // 军队适应性
        public byte health;                     // 当前体力
        public byte maxHealth = 100;            // 最大体力
        public byte lead;                       // 统率值
        public byte force;                      // 武力值
        public byte wisdom;                     // 智谋值
        public byte govern;                     // 行政值
        public byte charm;                      // 魅力值
        public byte loyalty;                    // 忠诚度
        public int maxAttribute = 120;          // 最大属性值
        public byte arm;                        // 武器
        public byte armor;                      // 铠甲

        public short soldiers;                   // 所属士兵数量
                         
        public short followWho;                 // 跟随的武将ID
        public int experience;                  // 经验值
        public short[] skills;                  // 技能列表
        public byte leadExp;                    // 领导力经验值
        public byte forceExp;                   // 武力经验值
        public byte wisdomExp;                  // 智商经验值
        public byte charmExp;                   // 德行经验值
        public byte governExp;                  // 政治能力经验值
        public bool isDie;                      // 是否死亡
        
        public General(){}
        // 自建的构造函数
        public General(string name, byte[] adaptability, List<byte> attributes, List<short> skills)
        {
            generalName = name;
            lead = attributes[0];
            govern = attributes[1];
            force = attributes[2];
            wisdom = attributes[3];
            charm = attributes[4];
            army = new byte[3];
            army[0] = adaptability[0];
            army[1] = adaptability[1];
            army[2] = adaptability[2];
            phase = adaptability[3];
            health = 100;
            loyalty = 99;
            level = 1;
            debutYear = GameInfo.years;
            arm = 1;
            armor = 1;
            
            for (var i = 0; i < skills.Count; i++)
            {
                skills[i] = skills[i];
            }
        }

        /// <summary>
        /// 判断武将是否为君主
        /// </summary>
        /// <returns></returns>
        public bool IsKing()
        {
            return CountryListCache.GetCountryByKingId(generalId) != null;
        }
        
        /// <summary>
        /// 获取武将的军队适应性
        /// </summary>
        /// <returns></returns>
        public string GetArmyS()
        {
            // 定义每个数字对应的后缀字符
            char[] suffixes = { 'C', 'B', 'A', 'S' }; // 假设0-3分别对应'C', 'S', 'A', 'B'

            // 定义前缀字符串数组
            string[] prefixes = { "平", "山", "水" };

            // 使用StringBuilder来构建结果字符串
            StringBuilder result = new StringBuilder();

            // 确保数组长度不超过prefixes数组长度
            int length = Mathf.Min(army.Length, prefixes.Length);

            // 遍历数组中的每个元素
            for (int i = 0; i < length; i++)
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
        public byte GetHP()
        {
            return health;
        }

        /// <summary>
        /// 设置当前体力值
        /// </summary>
        /// <param name="num">新的体力值</param>
        public void SetHP(int num)
        {
            health = (byte)Mathf.Clamp(num, 0, maxHealth);
        }

        
        /// <summary>
        /// 增加体力值，并返回实际增加的生命值
        /// </summary>
        /// <param name="num">要增加的体力值</param>
        /// <returns>实际增加的生命值</returns>
        public int AddHP(int num)
        {
            // 计算增加后的生命值（确保不会超过最大生命值）
            int newHP = Mathf.Clamp(health + num, 0, maxHealth);
            // 计算实际增加的体力值
            int actualIncrease = newHP - health;
    
            // 更新当前生命值
            health = (byte)newHP;
    
            // 返回实际增加的生命值（增加后的生命值减去增加前的生命值）
            return actualIncrease;
        }

        /// <summary>
        /// 减少体力值
        /// </summary>
        /// <param name="num">减少的体力值</param>
        /// <returns>是否体力降为0</returns>
        public bool SubHP(int num)
        {
            if (health <= num)
            {
                health = 0;
                return true;
            }
            health = (byte)(health - num);
            return false;
        }

        
        /// <summary>
        /// 获取调整后的薪水
        /// </summary>
        /// <returns>返回调整后的薪水</returns>
        public byte GetSalary()
        {
            return (byte)(1 + level * 2);
        }

        /// <summary>
        /// 设置忠诚度，最大值为100
        /// </summary>
        /// <param name="num">新的忠诚度值</param>
        public void SetLoyalty(int num)
        {
            loyalty = (byte)Mathf.Clamp(num, 0, 100);
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
        /// <param name="num">减少的忠诚度值</param>
        public void SubLoyalty(int num)
        {
            loyalty = (byte)Mathf.Clamp(loyalty - num, 0, 100);
        }

        /// <summary>
        /// 增加忠诚度，最大值为100
        /// </summary>
        /// <param name="num">增加的忠诚度值</param>
        public void AddLoyalty(byte num)
        {
            if (num + loyalty >= 100)
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
                loyalty = (byte)(loyalty + num);
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

        private static readonly int[] LevelUpExp = { 6000, 10000, 14000, 18000, 22000, 26000, 30000, short.MaxValue };

        public int GetMaxExp()
        {
            if (level <= 0 || level > LevelUpExp.Length)
                return short.MaxValue;
            return LevelUpExp[level - 1];
        }

        /// <summary>
        /// 增加经验值
        /// </summary>
        /// <param name="exp"></param>
        public void AddExperience(int exp)
        {
            if (exp <= 0)
                return;

            experience += exp;

            while (experience >= GetMaxExp())
            {
                experience -= GetMaxExp();
                GeneralUpgrade();

                if (level >= LevelUpExp.Length)
                {
                    experience = 0;
                    break;
                }
            }
        }
        

        
    
        

        /// <summary>
        /// 获取将领属性和
        /// </summary>
        /// <returns>返回将领的各项属性总和</returns>
        public int AllStatus()
        {
            return lead + govern + force + charm + wisdom;
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
            if (lead >= 95 || govern >= 95 || force >= 95 || charm >= 95 || wisdom >= 95)
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
        private void GeneralUpgrade()
        {
            if (level >= 8)
            {
                // 如果等级已经达到最大值，则不进行升级
                return;
            }

            level++; // 提升等级
            health = maxHealth; // 回复体力

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
                case 0 when lead < maxAttribute:
                    lead++;
                    break;
                // 如果政治力小于最大值，则增加政治力
                case 1 when govern < maxAttribute:
                    govern++;
                    break;
                // 如果武力小于最大值，则增加武力
                case 2 when force < maxAttribute:
                    force++;
                    break;
                // 如果道德小于最大值，则增加道德
                case 3 when charm < maxAttribute:
                    charm++;
                    break;
                // 如果智力小于最大值，则增加智力
                case 4 when wisdom < maxAttribute:
                    wisdom++;
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
            if (wisdom >= 120)
            {
                wisdomExp = 100; // 达到最大值，经验归满
                return;
            }

            // 累计经验值
            int totalExp = wisdomExp + exp;

            // 计算增加的智力属性
            int newWisdom = totalExp / 100;
            wisdom += (byte)newWisdom;

            // 检查是否达到智力最大值
            if (wisdom >= 120)
            {
                wisdom = 120;
                wisdomExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            wisdomExp = (byte)(totalExp % 100);
        }
        
        /// <summary>
        /// 增加政治经验值。
        /// </summary>
        /// <param name="exp">经验值</param>
        public void AddPoliticalExp(byte exp)
        {
            if (govern >= 120)
            {
                governExp = 100; // 达到最大值，经验归满
                return;
            }
            
            // 累计经验值
            int totalExp = governExp + exp;

            // 计算增加的政治属性
            int newPolitical = totalExp / 100;
            govern += (byte)newPolitical;

            // 检查是否达到政治最大值
            if (govern >= 120)
            {
                govern = 120;
                governExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            governExp = (byte)(totalExp % 100);
        }

        /// <summary>
        /// 增加道德经验值。
        /// </summary>
        /// <param name="exp">经验值</param>
        public void AddMoralExp(int exp)
        {
            if (charm >= 120)
            {
                charmExp = 100; // 达到最大值，经验归满
                return;
            }

            // 累计经验值
            int totalExp = charmExp + exp;

            // 计算增加的道德属性
            int newMoral = totalExp / 100;
            charm += (byte)newMoral;

            // 检查是否达到道德最大值
            if (charm >= 120)
            {
                charm = 120;
                charmExp = 100; // 达到最大值，经验归满
                return;
            }

            // 更新剩余的经验值
            charmExp = (byte)(totalExp % 100);
        }

        public short GetLearnNeedExp()
        {
            short needExp = (short)(200 + wisdom * 50); // 计算所需经验
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
            wisdom = (byte)(wisdom + 1);
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
            return (short)(200 + wisdom * 50);
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
                return (short)Mathf.Clamp(1000 + 12 * lead + 100 * level, 0, 3000);
            }
        }
        
        /// <summary>
        /// 增加士兵数量
        /// </summary>
        /// <param name="soldier">士兵数</param>
        public void AddSoldier(int soldier)
        {
            soldiers = (short)Mathf.Clamp(soldiers + soldier, 0, GetMaxSoldierNum());
        }

        /// <summary>
        /// 减少士兵数量
        /// </summary>
        /// <param name="soldier">士兵数</param>
        public void SubSoldier(int soldier)
        {
            soldiers = (short)Mathf.Clamp(soldiers - soldier, 0, GetMaxSoldierNum());
        }

        /// <summary>
        /// 计算单个武将的总战斗力
        /// </summary>
        /// <returns></returns>
        public int CalculateWarValue()
        {
            return (force + wisdom + lead * 2) / 2 * (soldiers + 150);
        }
        
        // 获取武将单挑的攻击力
        public short GetAttackPower()
        {
            byte weaponProperties = WeaponListCache.GetWeapon(arm).property;
            return (short)(force * (1 + weaponProperties / 100f) * (level + 19) / 20f);
        }

        // 获取武将单挑的防御力
        public short GetDefendPower()
        {
            byte armorProperties = WeaponListCache.GetWeapon(armor).property;
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
            Weapon weaponObj = WeaponListCache.GetWeapon(arm);
            if (weaponObj != null)
            {
                // 计算武器对战斗力的影响
                power += weaponObj.property * 1.3D;
            }

            // 获取防具对象
            // 注意: 通常防具不会从武器列表中获取，这里假设是正确的逻辑
            Weapon armorObj = WeaponListCache.GetWeapon(armor);
            if (armorObj != null)
            {
                // 计算防具对战斗力的影响
                power += armorObj.property * 1.3D;
            }

            // 计算其他属性对战斗力的影响
            power += force * 1.3D + wisdom * 1.2D + health * 1.1D + soldiers * 0.2D;

            return power;
        }
        
        /// <summary>
        /// 获取战争综合能力值
        /// </summary>
        /// <returns>战争综合能力值</returns>
        public int GetWarValue()
        {
            // 计算公式
            return (int)(lead * 1.42f + wisdom * 0.25f + force * 0.33f + ((lead * 2 + force + wisdom) * (level - 1)) * 0.04f);
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

            if (soldiers < 100)
            {
                adjustedValue = System.Math.Min(100L, adjustedValue);
                return 0;
            }

            if (adjustedValue < 20L)
            {
                adjustedValue = System.Math.Max((soldiers / 150), adjustedValue);
            }

            power += (int)(adjustedValue * (soldiers + 1));
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
        /// 根据IQ和等级返回武将计划的数量
        /// </summary>
        /// <returns>返回计划数量</returns>
        public byte GetPlanNum()
        {
            // 如果智商大于等于100且等级大于等于8，则返回16
            if (wisdom >= 100 && level >= 8)
                return 16;

            // 如果智商大于等于95且等级大于等于7，则返回15
            if (wisdom >= 95 && level >= 7)
                return 15;

            // 如果智商大于等于93且等级大于等于5，则返回14
            if (wisdom >= 93 && level >= 5)
                return 14;

            // 如果智商大于等于90且等级大于等于4，则返回12
            if (wisdom >= 90 && level >= 4)
                return 12;

            // 如果智商大于等于85，则返回10
            if (wisdom >= 85)
                return 10;

            // 如果智商大于等于80，则返回8
            if (wisdom >= 80)
                return 8;

            // 如果智商大于等于60，则返回6
            if (wisdom >= 60)
                return 6;

            // 如果智商小于40，则返回2；否则返回4
            return (byte)((wisdom < 40) ? 2 : 4);
        }
        
        
        //在战场锻造坊购买某武器
        public bool BuyNewWeapon(byte weaponId, byte cityId)
        {
            Weapon newWeapon = WeaponListCache.GetWeapon(weaponId);
            City city = CityListCache.GetCityByCityId(cityId);
            if(newWeapon.isUnique)
            {
                if (newWeapon.weaponID == 15)//青龙偃月刀
                {
                    if (force >= 95)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponID == 21)//方天画戟
                {
                    if (force>= 95)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponID == 22)//狼牙棒
                {
                    if (force >= 90)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponID == 23)//丈八蛇矛
                {
                    if (force >= 95)
                    {
                        GetNewWeapon(weaponId);
                        city.SellUniqueWeapon();
                        return true;
                    }
                    return false;
                }
                else if(newWeapon.weaponID == 30)//圣者之衣
                {
                    if (wisdom >= 90)
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
            if(newWeapon.kind==3)
            {
                armor = weaponId;
            }
            else
            {
                arm = weaponId;
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
                    gold = (force + govern) / 2;  // 计算力量与政治的平均值
                    break;
                case TaskType.Mercantile:
                    gold = (wisdom + govern) / 2;  // 计算智商与政治的平均值
                    break;
                case TaskType.Tame:
                    gold = (lead + govern) / 2;  // 计算领导力与政治的平均值
                    break;
                case TaskType.Patrol:
                    gold = (charm + govern) / 2;  // 计算道德与政治的平均值
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
                    return debutedCity.ownerID; // 返回该城市所属的君主ID
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
                        kingId = city.ownerID; // 找到该城市所属的君主ID
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
