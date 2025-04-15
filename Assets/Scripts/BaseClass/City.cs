using System;
using System.Collections.Generic;
using System.Linq;
using DataClass;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BaseClass
{
    [Serializable]
    public class City
    {
        public byte cityId { get; set; }
        public string cityName { get; set; }
        public short cityBelongKing { get; set; }
        public short prefectId { get; set; }
        public byte rule { get; set; }
        public short money { get; set; }
        public short food { get; set; }
        public short agro { get; set; }
        public short trade { get; set; }
        public int population { get; set; }
        public byte floodControl { get; set; }

        public short[] cityOfficeGeneralId = new short[0];

        private short[] cityReservedGeneralId = new short[0];

        private short[] cityNotFoundGeneralId = new short[0];

        public List<short> cityJailGeneralId = new List<short>();

        public int cityTotalSoldier { get; set; }
        public int cityReserveSoldier { get; set; }
        public bool cityGrainShop { get; set; }
        public bool citySchool { get; set; }
        public bool cityHospital { get; set; }
        public byte cityWeaponShop { get; set; }
        public byte warWeaponShop { get; set; }
        public byte treasureNum { get; set; }
        public int development { get; set; }

        [JsonProperty("connectCityId[]")] public byte[] connectCityId;

        [JsonProperty("mapPosition[]")] public short[] mapPosition = new short[2];
     

        // 设置金钱
        public void SetMoney(int num)
        {
            if (num > 30000 || num < 0)
            {
                money = 30000;
            }
            else
            {
                money = (short)num;
            }
        }

        // 获取金钱
        public short GetMoney()
        {
            return money;
        }

        // 增加金钱
        public short AddGold(int num)
        {
            if (money + num > 30000 || money + num < 0)
            {
                num = 30000 - money;
                money = 30000;
            }
            else
            {
                money += (short)num;
            }
            return (short)num;
        }

        // 减少金钱
        public short SubGold(int num)
        {
            if (money - num > 30000 || money - num < 0)
            {
                num = money;
                money = 0;
            }
            else
            {
                money -= (short)num;
            }
            return (short)num;
        }

        /// <summary>
        /// 计算金钱收入
        /// </summary>
        /// <returns></returns>
        public int MoneyIncome()
        {
            int l1 = population / 400;
            l1++;
            int i2 = l1 * trade * 3 / 5 / 125;
            l1 += i2;
            i2 = 0;
            if (rule > 90)
            {
                i2 += l1 * 4 / 5;
            }
            else if (rule > 80)
            {
                i2 += l1 * 3 / 5;
            }
            else if (rule > 70)
            {
                i2 += l1 * 2 / 5;
            }
            else if (rule > 50)
            {
                i2 += l1 / 5;
            }
            l1 += i2;
            return l1;
        }


        /// <summary>
        /// 计算粮食收入
        /// </summary>
        /// <returns></returns>
        public int FoodIncome()
        {
            int l1 = population / 100;
            int i2 = l1 * agro * 4 / 5 / 125;
            l1 += i2;
            i2 = 0;
            if (rule > 90)
            {
                i2 += l1 * 4 / 5;
            }
            else if (rule > 80)
            {
                i2 += l1 * 3 / 5;
            }
            else if (rule > 70)
            {
                i2 += l1 * 2 / 5;
            }
            else if (rule > 50)
            {
                i2 += l1 / 5;
            }
            l1 += i2;
            return l1;
        }



        // 设置食物
        public void SetFood(int num)
        {
            if (num > 30000 || num < 0)
            {
                food = 30000;
            }
            else
            {
                food = (short)num;
            }
        }




        // 获取城池食物数值
        public short GetFood()
        {
            return food;
        }

        // 增加食物
        public short AddFood(int num)
        {
            if (food + num > 30000 || food + num < 0)
            {
                num = 30000 - food;
                food = 30000;
            }
            else
            {
                food += (short)num;
            }
            return (short)num;
        }

        // 减少食物
        public short SubFood(int num)
        {
            if (food - num > 30000 || food - num < 0)
            {
                num = food;
                food = 0;
            }
            else
            {
                food -= (short)num;
            }
            return (short)num;
        }

        public byte AddTreasureNum(byte num)
        {
            if (treasureNum + num > 100||treasureNum + num < 0)
            {
                num= (byte)(100 - treasureNum);
                treasureNum = 100;
            }
            else
            {
                treasureNum = (byte)(treasureNum + num);
            }
            return num;
        }

        public byte DecreaseTreasureNum(byte num)
        {
            if (treasureNum - num > 100||treasureNum - num < 0)
            {
                num = treasureNum;
                treasureNum = 0;
            }
            else
            {
                treasureNum = (byte)(treasureNum - num);
            }
            return num;
        }

        // 添加城池人口
        public int AddPopulation(int num)
        {
            if (population + num > 999999 || population + num < 0)
            {
                num = 999999 - population;
                population = 999999;
            }
            else
            {
                population += num;
            }
            return num;
        }

        // 减少城池人口
        public int SubPopulation(int num)
        {
            if (population - num > 999999 || population - num < 0)
            {
                num = population;
                population = 0;
            }
            else
            {
                population -= num;
            }
            return num;
        }

        // 获取城池人口
        public int GetPopulation()
        {
            return population;
        }

        // 士兵消耗食物
        public void SoldierEatFood()
        {
            short needFood = (short)(GetAlreadySoldierNum() / 100 + cityReserveSoldier / 300);
            if (needFood > food)
            {
                food = 0;
                if (cityReserveSoldier > 100)
                {
                    cityReserveSoldier -= 100;
                }
                else
                {
                    cityReserveSoldier = 0;
                }
                for (int i = 0; i < cityOfficeGeneralId.Length; i++)
                {
                    General general = GeneralListCache.GetGeneral(cityOfficeGeneralId[i]);
                    if (general.generalSoldier > 100)
                    {
                        general.generalSoldier = (short)(general.generalSoldier - 100);
                    }
                    else
                    {
                        general.generalSoldier = 0;
                    }
                }
            }
            else
            {
                food = (short)(food - needFood);
            }
        }


        // 支付官员薪水
        public void PaySalaries()
        {
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                General general = GeneralListCache.GetGeneral(cityOfficeGeneralId[i]);
                if (money <= 0)
                {
                    if (general.generalId != cityBelongKing)
                    {
                        general.DecreaseLoyalty((byte)(Random.Range(0, 4) + 1));
                        Debug.Log($"{cityName}欠薪{general.generalName}");
                    }
                }
                else
                {
                    SubGold(general.GetSalary());
                }
            }
        }

        // 获取太守
        public int GetprefectId()
        {
            return prefectId;
        }

        // 获取城市所属君主
        public int GetcityBelongKing()
        {
            return cityBelongKing;
        }



        // 获取未被搜索到官员的数量
        public byte GetCityNotFoundGeneralNum()
        {
            byte generalNum = 0; // 用于计数的变量
            foreach (var generalId in cityNotFoundGeneralId)
            {
                if (generalId > 0)
                {
                    generalNum++; // 增加计数器
                }
            }
            return generalNum; // 返回计数结果
        }

        // 获取未被搜到将领的ID数组
        public short[] GetCityNotFoundGeneralIdArray()
        {
            int generalNum = GetCityNotFoundGeneralNum(); // 确保这是非零将领的数量
            short[] result = new short[generalNum];
            int index = 0; // 使用 int 类型索引

            for (int i = 0; i < cityNotFoundGeneralId.Length; i++)
            {
                short generalId = cityNotFoundGeneralId[i];
                if (generalId > 0)
                {
                    result[index] = generalId;
                    index++; // 自增索引
                }
            }
            return result;
        }

        // 获取未被任命官员的ID
        public short GetNotFoundGeneralId(short index)
        {
            return cityNotFoundGeneralId[index];
        }

        /// <summary>
        /// 添加待搜索到任命官员的ID
        /// </summary>
        /// <param name="generalId"></param>
        /// <returns></returns>
        public bool AddNotFoundGeneralId(short generalId)
        {
            // 检查 generalId 是否为 0
            if (generalId <= 0)
                return false;

            // 获取将领对象
            General general = GeneralListCache.GetGeneral(generalId);
            if (general == null)
                return false;

            // 检查该将领是否已经在其他城市的职位列表中
            for (byte b = 1; b < CityListCache.GetCityNum(); b++)
            {
                City city = CityListCache.GetCityByCityId(b);
                if (city.GetOfficerIds() != null)
                {
                    for (byte index = 0; index < city.GetCityOfficerNum(); index++)
                    {
                        if (city.GetOfficerIds()[index] == generalId)
                        {
                            city.RemoveOfficerId(generalId);
                            Debug.Log($"将军{general.generalName}在{city.cityName}");
                            break; // 找到后退出内层循环
                        }
                    }
                }
            }

            // 将领不再在职
            general.isOffice = 0;
            general.debutCity = cityId;

            // 检查未找到的将领ID数组是否已满
            if (cityNotFoundGeneralId == null)
            {
                cityNotFoundGeneralId = new short[0]; // 初始化为空数组
            }

            if (cityNotFoundGeneralId.Length >= 10)
            {
                Debug.Log($"未发掘到的人才过多: {general.generalName}");
                GeneralListCache.GeneralDie(generalId);
                return false;
            }

            // 添加新的将领ID
            short[] tempNotFoundGeneralId = new short[cityNotFoundGeneralId.Length + 1];
            for (int j = 0; j < cityNotFoundGeneralId.Length; j++)
            {
                short tempGeneralId = cityNotFoundGeneralId[j];
                if (tempGeneralId > 0)
                    tempNotFoundGeneralId[j] = tempGeneralId;
            }
            tempNotFoundGeneralId[cityNotFoundGeneralId.Length] = generalId;
            cityNotFoundGeneralId = tempNotFoundGeneralId;
            return true;
        }


        // 搜索到在野官员的ID从未搜索到数组中删除
        public void RemoveNotFoundGeneralId(short generalId)
        {
            bool isExistence = false;
            for (int i = 0; i < cityNotFoundGeneralId.Length; i++)
            {
                short tempGeneralId = cityNotFoundGeneralId[i];
                if (tempGeneralId == generalId)
                {
                    isExistence = true;
                    break;
                }
            }
            if (!isExistence)
                return;
            short[] tempOfficeGeneralId = new short[cityNotFoundGeneralId.Length - 1];
            int index = 0;
            for (int j = 0; j < cityNotFoundGeneralId.Length; j++)
            {
                short tempGeneralId = cityNotFoundGeneralId[j];
                if (tempGeneralId > 0 && tempGeneralId != generalId)
                {
                    tempOfficeGeneralId[index] = tempGeneralId;
                    index++;
                }
            }
            cityNotFoundGeneralId = tempOfficeGeneralId;
        }

        // 分配预备役士兵
        public void AssignSoldier()
        {
            if (cityReserveSoldier <= 0)
                return;
            short[] officeGeneralIdArray = GetOfficerIds().OrderByDescending(t => GeneralListCache.GetGeneral(t).GetWarValue()).ToArray();
            for (int i = 0; i < officeGeneralIdArray.Length && cityReserveSoldier > 0; i++)
            {
                short generalId = officeGeneralIdArray[i];
                General general = GeneralListCache.GetGeneral(generalId);
                if (general.generalSoldier < general.GetMaxSoldierNum())
                {
                    short needSoldier = (short)(general.GetMaxSoldierNum() - general.generalSoldier);
                    if (needSoldier <= cityReserveSoldier)
                    {
                        cityReserveSoldier -= needSoldier;
                        general.generalSoldier = general.GetMaxSoldierNum();
                    }
                    else
                    {
                        general.generalSoldier = (short)(general.generalSoldier + cityReserveSoldier);
                        cityReserveSoldier = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 获取城中在职官员ID数组
        /// </summary>
        /// <returns></returns>
        public short[] GetOfficerIds()
        {
            byte generalNum = GetCityOfficerNum();
            short[] result = new short[generalNum];
            byte index = 0;

            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                short generalId = cityOfficeGeneralId[i];
                if (generalId > 0)
                {
                    result[index] = generalId;
                    index++;
                }
            }
            return result;
        }

        // 获取非君主官员ID数组
        public short[] GetCitySubjectsGeneralIdArray()
        {
            byte generalNum = GetCityOfficerNum();
            if(prefectId == cityBelongKing)
                generalNum = (byte)(generalNum - 1);
            short[] result = new short[generalNum];
            byte index = 0;

            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                short generalId = cityOfficeGeneralId[i];
                if (generalId > 0 && generalId!=cityBelongKing)
                {
                    result[index] = generalId;
                    index++;
                }
            }
            return result;
        }

        // 获取在野武将数量
        public byte GetReservedGeneralNum()
        {
            byte count = 0;
            for (int i = 0; i < cityReservedGeneralId.Length; i++)
            {
                if (cityReservedGeneralId[i] > 0)
                    count = (byte)(count + 1);
            }
            return count;
        }

        // 获取在野武将ID
        public short GetReservedGeneralId(int index)
        {
            return cityReservedGeneralId[index];
        }

        public List<short> GetRecluseIds()
        {
            return cityNotFoundGeneralId.Where(id => id > 0).ToList();
        }
        public List<short> GetTalentIds()
        {
            return cityReservedGeneralId.Where(id => id > 0).ToList();
        }




        /// <summary>
        /// 把将领添加到城池储备武将ID
        /// </summary>
        /// <param name="generalId">武将ID</param>
        /// <returns>是否添加到城池储备武将ID</returns>
        public bool AddReservedGeneralId(short generalId)
        {
            if (GetReservedGeneralNum() > 9)
                return false;
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                if (cityOfficeGeneralId[i] == generalId)
                {
                    Debug.Log($"官员ID{generalId}从职位上移除");

                    RemoveOfficerId(generalId);
                }
            }
            General general = GeneralListCache.GetGeneral(generalId);
            if (general == null)
                return false;
            general.isOffice = 0;
            general.debutCity = cityId;
            short[] tempReservedGeneralId = new short[cityReservedGeneralId.Length + 1];
            for (int j = 0; j < cityReservedGeneralId.Length; j++)
            {
                short tempGeneralId = cityReservedGeneralId[j];
                if (tempGeneralId > 0)
                    tempReservedGeneralId[j] = tempGeneralId;
            }
            tempReservedGeneralId[cityReservedGeneralId.Length] = generalId;
            cityReservedGeneralId = tempReservedGeneralId;
            return true;
        }

        // 移除已找到的储备将领ID
        public void RemoveReservedGeneralId(short generalId)
        {
            bool isExistence = Array.Exists(cityReservedGeneralId, t => t == generalId);
            
            if (!isExistence)
                return;
            short[] tempOfficeGeneralId = new short[cityReservedGeneralId.Length - 1];
            int index = 0;
            for (int j = 0; j < cityReservedGeneralId.Length; j++)
            {
                short tempGeneralId = cityReservedGeneralId[j];
                if (tempGeneralId > 0 && tempGeneralId != generalId)
                {
                    tempOfficeGeneralId[index] = tempGeneralId;
                    index++;
                }
            }
            cityReservedGeneralId = tempOfficeGeneralId;
        }

        // 清除所有官员
        public void ClearAllOfficeGeneral()
        {
            cityOfficeGeneralId = new short[0];
        }

        // 添加单个官员ID
        public bool AddOfficeGeneralId(short generalId)
        {
            // 检查 generalId 是否为 0
            if (generalId == 0)
            {
                Debug.LogError("武将ID不能为0");
                return false;
            }

            General general = GeneralListCache.GetGeneral(generalId);
            if (general == null)
            {
                RemoveOfficerId(generalId);
                return false;
            }

            // 检查 cityOfficeGeneralId 是否为空或未初始化
            if (cityOfficeGeneralId == null || cityOfficeGeneralId.Length == 0)
            {
                // 如果为空，直接将 generalId 加入
                cityOfficeGeneralId = new[] { generalId };
                prefectId = generalId;
                general.isOffice = 1;
                general.debutCity = cityId;
                return true;
            }

            // 检查是否已存在于 cityOfficeGeneralId 中
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                if (cityOfficeGeneralId[i] == generalId)
                {
                    Debug.Log($"该城池武将:{generalId}已经存在");
                    return true;
                }
            }

            general.isOffice = 1;
            general.debutCity = cityId;
            byte generalNum = GetCityOfficerNum();

            if (generalNum > 9) // 如果城池已满员
            {
                short minGeneralId = GetMinBattlePowerGeneralId();
                if (CountryListCache.GetCountryByKingId(generalId) != null)//添加的是君主
                {
                    for (int j = 0; j < cityOfficeGeneralId.Length; j++)
                    {
                        if (cityOfficeGeneralId[j] == minGeneralId)
                        {
                            cityOfficeGeneralId[j] = cityOfficeGeneralId[0];
                            cityOfficeGeneralId[0] = generalId;
                            prefectId = generalId;
                            AddNotFoundGeneralId(minGeneralId);
                            return true;
                        }
                    }
                }
                else
                {
                    int generalScore = general.AllStatus();
                    General minGeneral = GeneralListCache.GetGeneral(minGeneralId);
                    if (generalScore > minGeneral.AllStatus())
                    {
                        for (int j = 0; j < cityOfficeGeneralId.Length; j++)
                        {
                            if (cityOfficeGeneralId[j] == minGeneralId)
                            {
                                cityOfficeGeneralId[j] = generalId;
                                if (j == 0)
                                {
                                    prefectId = generalId;
                                }
                                AddNotFoundGeneralId(minGeneralId);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        AddNotFoundGeneralId(generalId);
                        return false;
                    }
                }
            }
            else //城池有空位可以任职
            {
                // 创建一个新数组并确保足够大
                short[] tempOfficeGeneralId = new short[cityOfficeGeneralId.Length + 1];
                Array.Copy(cityOfficeGeneralId, 0, tempOfficeGeneralId, 0, cityOfficeGeneralId.Length);

                if (CountryListCache.GetCountryByKingId(generalId) != null)// 如果为君主任命为太守
                {
                    if (generalNum != 0)
                    {
                        short tempGeneralId = cityOfficeGeneralId[0];
                        tempOfficeGeneralId[generalNum] = tempGeneralId;
                    }
                    tempOfficeGeneralId[0] = generalId;
                    prefectId = generalId;
                    cityOfficeGeneralId = tempOfficeGeneralId;
                    return true;
                }

                tempOfficeGeneralId[generalNum] = generalId;
                cityOfficeGeneralId = tempOfficeGeneralId;

                if (cityOfficeGeneralId.Length == 1)
                {
                    prefectId = generalId;
                }
                return true;
            }

            return false;
        }




        // 移除官员ID
        public void RemoveOfficerId(short generalId)
        {
            if (!Array.Exists(cityOfficeGeneralId, element => element == generalId))
                return;

            short[] tempOfficeGeneralId = new short[cityOfficeGeneralId.Length - 1];
            int index = 0;
            for (int j = 0; j < cityOfficeGeneralId.Length; j++)
            {
                short tempGeneralId = cityOfficeGeneralId[j];
                if (tempGeneralId > 0 && tempGeneralId != generalId)
                {
                    tempOfficeGeneralId[index] = tempGeneralId;
                    index++;
                }
            }
            cityOfficeGeneralId = tempOfficeGeneralId;

            if (GetCityOfficerNum() < 1)
            {
                Country country = CountryListCache.GetCountryByKingId(cityBelongKing);
                country.RemoveCity(cityId);
                cityBelongKing = 0;
                prefectId = 0;
            }
            else if (prefectId == generalId)
            {
                AppointmentPrefect();
            }
        }
        
        /// <summary>
        /// 把俘虏关进城池监狱中
        /// </summary>
        /// <param name="generalId">俘虏将领ID</param>
        /// <returns>是否成功添加俘虏</returns>
        public bool AddCapture(short generalId)
        {
            if (generalId == 0)
            {
                Debug.LogError("关进监狱的武将ID为零！");
                return false;
            }
            if (cityJailGeneralId.Contains(generalId))
                return false;
            GeneralListCache.GetGeneral(generalId).SetLoyalty((byte)Random.Range(40, 75));
            cityJailGeneralId.Add(generalId);
            return true;
        }

        public bool RemoveCapture(short generalId)
        {
            if (generalId == 0)
            {
                Debug.LogError("监狱释放的武将ID为零！");
                return false;
            }
            if (!cityJailGeneralId.Contains(generalId))
                return false;
            
            cityJailGeneralId.Remove(generalId);
            return true;
        }
        
        // 获取发展程度
        public byte GetDevelopment()
        {
            return 0;
        }

        // 获取城市所有士兵数量
        public int GetCityAllSoldierNum()
        {
            return GetAlreadySoldierNum() + cityReserveSoldier;
        }

        // 获取理论最大士兵数量
        public int GetMaxSoldierNum()
        {
            int count = 0;
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                short generalId = cityOfficeGeneralId[i];
                if (generalId > 0)
                {
                    General general = GeneralListCache.GetGeneral(generalId);
                    count += general.GetMaxSoldierNum();
                }
            }
            return count;
        }

        // 获取已征所有士兵数量
        public int GetAlreadySoldierNum()
        {
            int count = 0;
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                short generalId = cityOfficeGeneralId[i];
                if (generalId > 0)
                {
                    General general = GeneralListCache.GetGeneral(generalId);
                    count += general.generalSoldier;
                }
            }
            return count;
        }

        /// <summary>
        /// 一键满编的最大士兵数量
        /// </summary>
        /// <returns></returns>
        public short GetMaxConscriptSoldierNum()
        {
            int goldLimit = money * 5;
            int ruleLimit = rule * 500;
            int populationLimit = population * 2 / 3;
            int diff = GetMaxSoldierNum() - GetCityAllSoldierNum();
            Debug.Log($"{goldLimit} {ruleLimit} {populationLimit} {diff}");
            return (short)Mathf.Min(goldLimit, ruleLimit, populationLimit, diff, 30000);
        }
        
        public void Conscript(int soldierNum)
        {
            cityReserveSoldier += soldierNum;
            SubGold((short)((soldierNum + 4) / 5));
            if (rule * 500 < soldierNum)
            {
                rule = 0;
            }
            else
            {
                rule = (byte)(rule - soldierNum / 500);
            }

            if (population * 2 < soldierNum * 3)
            {
                population = 0;
            }
            else
            {
                population -= soldierNum * 3 / 2;
            }
        }

        // 获取城市官员数量
        public  byte GetCityOfficerNum()
        {
            return cityOfficeGeneralId.Where(t => t > 0).Aggregate<short, byte>(0, (current, t) => (byte)(current + 1));
        }

        // 指定任命太守
        public void AppointmentPrefect(int generalId)
        {
            int index = 0;
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                if (cityOfficeGeneralId[i] == generalId)
                    index = i;
            }
            if (generalId == 0)
            {
                Debug.Log("官员ID为零: " + generalId);
                return;
            }
            if (index == 0)
            {
                Debug.Log("官员已经是太守: " + generalId);
            }
            else
            {
                short tempGeneralId = cityOfficeGeneralId[0];
                cityOfficeGeneralId[0] = cityOfficeGeneralId[index];
                cityOfficeGeneralId[index] = tempGeneralId;
            }
            prefectId = (short)generalId;
        }

        /// <summary>
        /// 自动任命太守
        /// </summary>
        public void AppointmentPrefect()
        {
            int i1 = 0;
            int prefectValue = 0;
            short prefectID = 0;
            short[] cityOfficeGeneralIdArray = GetOfficerIds();
            if (cityOfficeGeneralIdArray.Length <= 0)
                return;
            if (cityOfficeGeneralIdArray.Length == 1)
            {
                this.prefectId = cityOfficeGeneralIdArray[0];
                return;
            }

            for (byte index = 0; index < cityOfficeGeneralIdArray.Length; index = (byte)(index + 1))
            {
                short generalId = cityOfficeGeneralIdArray[index];
                General general = GeneralListCache.GetGeneral(generalId);
                if (generalId == cityBelongKing)
                {
                    prefectValue = 1000;
                    prefectID = generalId;
                    i1 = index;
                    break;
                }
                if (general.GetLoyalty() >= 60 && general.GetWarValue() > prefectValue)
                {
                    prefectValue = general.GetWarValue();
                    prefectID = cityOfficeGeneralIdArray[index];
                    i1 = index;
                }
            }

            if (prefectValue == 0)
            {
                i1 = 0;
                prefectID = cityOfficeGeneralIdArray[0];
                for (byte byte3 = 1; byte3 < cityOfficeGeneralIdArray.Length; byte3 = (byte)(byte3 + 1))
                {
                    if (GeneralListCache.GetGeneral(prefectID).GetLoyalty() < GeneralListCache.GetGeneral(cityOfficeGeneralIdArray[byte3]).GetLoyalty())
                    {
                        prefectID = cityOfficeGeneralIdArray[byte3];
                        i1 = byte3;
                    }
                }
            }

            this.prefectId = prefectID;
            for (byte byte4 = (byte)i1; byte4 > 0; byte4 = (byte)(byte4 - 1))
                cityOfficeGeneralIdArray[byte4] = cityOfficeGeneralIdArray[byte4 - 1];
            cityOfficeGeneralIdArray[0] = prefectID;
            cityOfficeGeneralId = cityOfficeGeneralIdArray;
        }




        // 获取最大攻击力
        public int GetMaxAtkPower()
        {
            short[] officerIds = GetOfficerIds();
            if (officerIds.Length < 1)
                return 0;
            double[] power = new double[officerIds.Length];
            for (byte index = 0; index < officerIds.Length; index = (byte)(index + 1))
            {
                short generalId = officerIds[index];
                General general = GeneralListCache.GetGeneral(generalId);
                power[index] = general.GetBattlePower();
            }
            double totalPower = 0.0;
            for (int i = 0; i < power.Length; i++)
                totalPower += power[i];
            if (cityBelongKing != CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId)
            {
                double minPower = power[0];
                for (int j = 1; j < power.Length; j++)
                {
                    if (minPower > power[j])
                        minPower = power[j];
                }
                totalPower -= minPower;
            }
            else
            {
                totalPower *= 1.2;
            }
            int needFood = GetAlreadySoldierNum() / 8 + 1;
            if (food < needFood)
                totalPower = totalPower * food / needFood;
            short needMoney = (short)(GetMaxSoldierNum() / 10);
            if (money > needMoney)
                totalPower = totalPower * 5.0 / 3.0;
            return (int)totalPower;
        }

        // 获取防御力
        public int GetDefenseAbility()
        {
            int curEnemyCityDefPower;
            if (cityBelongKing == CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId)
            {
                curEnemyCityDefPower = (int)(GetCityDefPower() * 1.2);
            }
            else
            {
                curEnemyCityDefPower = (int)GetCityDefPower();
            }
            return curEnemyCityDefPower;
        }

        // 获取AI城市的防御力
        private int GetAICityDefPower()
        {
            int power = 1;
            short[] cityOfficeGeneralIdArray = GetOfficerIds();
            int needFood = GetAlreadySoldierNum() / 8 + 1;
            for (byte index = 0; index < cityOfficeGeneralIdArray.Length; index = (byte)(index + 1))
            {
                short generalId = cityOfficeGeneralIdArray[index];
                General general = GeneralListCache.GetGeneral(generalId);
                if (generalId == prefectId)
                {
                    power += GetSatrapGenPower();
                }
                else
                {
                    power += general.GetGeneralPower();
                }
            }
            if (food < needFood)
                power = power * (food + 1) / needFood;
            return power;
        }

        // 获取城市的防御力
        public double GetCityDefPower()
        {
            double power = 0.0;
            short[] cityOfficeGeneralIdArray = GetOfficerIds();
            for (byte index = 0; index < cityOfficeGeneralIdArray.Length; index = (byte)(index + 1))
            {
                short generalId = cityOfficeGeneralIdArray[index];
                General general = GeneralListCache.GetGeneral(generalId);
                if (generalId == prefectId)
                {
                    power += general.GetBattlePower() * 1.2;
                }
                else
                {
                    power += general.GetBattlePower();
                }
            }
            int needFood = cityOfficeGeneralIdArray.Length / 8 + 1;
            if (food < needFood)
                power = power * food / needFood;
            return power;
        }

        // 获取人类城市的防御力
        private int GetHmCityDefPower()
        {
            int power = 1;
            int needFood = GetMaxSoldierNum() / 8 + 1;
            for (byte index = 0; index < GetCityOfficerNum(); index = (byte)(index + 1))
            {
                short generalId = cityOfficeGeneralId[index];
                General general = GeneralListCache.GetGeneral(generalId);
                if (generalId == prefectId)
                {
                    if (general.generalSoldier < 800)
                    {
                        power += GetSatrapGenPower() / 2;
                        continue;
                    }
                }
                power += general.GetGeneralPower();
            }
            if (food < needFood)
                power = power * (food + 1) / needFood * 2;
            power -= power / 2;
            return power;
        }

        /// <summary>
        /// 获取城市中德望最高的将领ID
        /// </summary>
        /// <returns></returns>
        public short GetMostMoralGeneralInCity()
        {
            short[] officeGeneralIdArray = GetOfficerIds();
            short generalId = officeGeneralIdArray[0];
            byte general = GeneralListCache.GetGeneral(generalId).moral;

            // 遍历将领数组，找出德望最高的将领
            for (byte i = 1; i < GetCityOfficerNum(); i++)
            {
                if (general < GeneralListCache.GetGeneral(officeGeneralIdArray[i]).moral)
                {
                    generalId = officeGeneralIdArray[i];
                    general = GeneralListCache.GetGeneral(generalId).moral;
                }
            }
            return generalId;
        }

        /// <summary>
        /// 获取城市中智力和德望的综合最高将领ID
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public short GetMostIqMoralGeneralInCity()
        {
            short[] officeGeneralIdArray = GetOfficerIds();
            short generalId = officeGeneralIdArray[0];
            General general = GeneralListCache.GetGeneral(generalId);
            int i1 = general.IQ + general.moral / 2;

            // 遍历城市将领，计算出智力+德望的综合值最高的将领
            for (byte byte1 = 1; byte1 < GetCityOfficerNum(); byte1++)
            {
                General otherGeneral = GeneralListCache.GetGeneral(officeGeneralIdArray[byte1]);
                if (i1 < otherGeneral.IQ + otherGeneral.moral / 2)
                {
                    generalId = officeGeneralIdArray[byte1];
                    i1 = otherGeneral.IQ + otherGeneral.moral / 2;
                }
            }
            return generalId;
        }

        /// <summary>
        /// 根据获取最适合搜索的将领
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="beGenId"></param>
        /// <returns></returns>
        public short GetDoSearchGen(short beGenId)
        {
            short[] officeGeneralIdArray = GetOfficerIds();
            short generalId = officeGeneralIdArray[0];
            byte d = 100;

            // 遍历城市将领，找出与目标将领相性最接近的将领
            for (byte byte2 = 1; byte2 < GetCityOfficerNum(); byte2++)
            {
                short curId = officeGeneralIdArray[byte2];
                byte curd = (byte)GeneralListCache.GetdPhase(curId, beGenId);
                if (curd < d)
                {
                    generalId = officeGeneralIdArray[byte2];
                    d = curd;
                }
            }
            return generalId;
        }

        /// <summary>
        /// 获取战斗官员ID列表
        /// </summary>
        /// <param name="power">所需战斗力</param>
        /// <returns></returns>
        public List<short> GetWarOfficeGeneralIds(int power)
        {
            List<short> result = new List<short>(GetOfficerIds()); // 将数组转换为列表
            if (result.Count < 2)
                return new List<short>(); // 返回空列表
        
            do
            {
                short minBattlePowerGeneralId = GetMinBattlePowerGeneralId();
                result.Remove(minBattlePowerGeneralId); // 直接从列表中移除最小战斗力将军的ID
                power -= (int)GeneralListCache.GetGeneral(minBattlePowerGeneralId).GetBattlePower();
            } while (power > 0 && result.Count > 1);
        
            // 优先级调整：如果列表中有属于国王或者太守的将军ID，则将其移动到列表首位
            for (int i = 0; i < result.Count; i++)
            {
                short generalId = result[i];
                if (generalId == cityBelongKing || generalId == prefectId)
                {
                    if (i != 0) // 只有当该将军ID不在首位时才需要交换
                    {
                        result.RemoveAt(i); // 移除当前位置的将军ID
                        result.Insert(0, generalId); // 插入到列表首位
                    }
                    break; // 只处理第一个符合条件的将军ID
                }
            }
        
            return result;
        }
        

        

        // 获取城市中战斗值最低的武将ID
        public short GetMinBattlePowerGeneralId()
        {
            return GetOfficerIds()
                .OrderBy(t => GeneralListCache.GetGeneral(t).GetBattlePower()).FirstOrDefault();
        }

        // 获取城市中战斗值最高的武将ID
        public short GetMaxBattlePowerGeneralId()
        {
            return GetOfficerIds()
                .OrderByDescending(t => GeneralListCache.GetGeneral(t).GetBattlePower()).FirstOrDefault();
        }

        // 计算太守的战斗力
        private int GetSatrapGenPower()
        {
            General general = GeneralListCache.GetGeneral(prefectId);
            int power = 1;
            short gjl = (short)((int)(general.GetWarValue() * 1.3));
            long attackValue = (1 + gjl * gjl * gjl / 100000);
            if (general.generalSoldier < 500)
                attackValue = (long)Mathf.Min(150L, attackValue);
            if (attackValue < 20L)
                attackValue = (long)Mathf.Max((general.generalSoldier / 150f), attackValue);
            power = (int)(power + attackValue * (general.generalSoldier + 1));
            return power;
        }

        // 计算收获所需的食物量
        public short NeedFoodToHarvest()
        {
            int need = GetAlreadySoldierNum() / 100 + cityReserveSoldier / 300;
            if (GameInfo.month >= 5 && GameInfo.month < 10)
            {
                need = (10 - GameInfo.month) * need;
            }
            else if (GameInfo.month < 5)
            {
                need = (5 - GameInfo.month) * need;
            }
            else
            {
                need = (12 - GameInfo.month - 5) * need;
            }
            return (short)need;
        }

        // 计算所有武将的薪水总和
        public int NeedAllSalariesMoney()
        {
            int result = 0;
            for (int i = 0; i < cityOfficeGeneralId.Length; i++)
            {
                General general = GeneralListCache.GetGeneral(cityOfficeGeneralId[i]);
                result += general.GetSalary();
            }
            return result;
        }

        // 计算一个月战争所需的食物量
        public short NeedFoodWarAMonth()
        {
            return (short)(GetAlreadySoldierNum() / 8 + 1);
        }
        
        /// <summary>
        /// 搜索事件的结果
        /// </summary>
        /// <param name="generalId">选中的武将 ID。</param>
        /// <returns>事件的结果文本。</returns>
        public string Search(short generalId)
        {
            General general = GeneralListCache.GetGeneral(generalId); // 获取指定将领
            general.SubHp(2); // 扣除将领体力

            // 主要逻辑
            TaskType taskType = DetermineSearchResult(general); // 确定具体结果

            int rewardValue = 0; // 奖励数值
            string result = String.Empty; // 结果文本

            switch (taskType)
            {
                case TaskType.SearchFood:
                    rewardValue = Random.Range(0, 40) + (general.IQ + general.moral * 3) / 4;
                    if (general.HasSkill(4, 4)) rewardValue += rewardValue / 2; // 如果拥有技能【眼力】
                    AddFood((short)rewardValue);
                    result = $"{TextLibrary.DoThingsResultInfo[6][1]}{rewardValue}石";
                    break;

                case TaskType.SearchMoney:
                    rewardValue = Random.Range(0, 30) + (general.IQ + general.moral * 2) / 4;
                    if (general.HasSkill(4, 4)) rewardValue += rewardValue / 2; // 如果拥有技能【眼力】
                    AddGold((short)rewardValue);
                    result = $"{TextLibrary.DoThingsResultInfo[6][2]}{rewardValue}两";
                    break;

                case TaskType.SearchGeneral:
                    short foundGeneralId = GetNotFoundGeneralId((short)Random.Range(0, GetCityNotFoundGeneralNum()));
                    General foundGeneral = GeneralListCache.GetGeneral(foundGeneralId);
                    RemoveNotFoundGeneralId(foundGeneralId);
                    AddReservedGeneralId(foundGeneralId);
                    result = $"{TextLibrary.DoThingsResultInfo[6][3]}{foundGeneral.generalName}";
                    break;

                case TaskType.SearchTreasure:
                    rewardValue = general.HasSkill(4, 4) ? 2 : 1; // 如果拥有技能【眼力】
                    AddTreasureNum((byte)(treasureNum + rewardValue));
                    result = $"{TextLibrary.DoThingsResultInfo[6][4]}{rewardValue}件";
                    break;

                case TaskType.SearchNothing:
                    result = $"{TextLibrary.DoThingsResultInfo[6][0]}"; // 无结果
                    break;
            }

            return result;
        }
        
        /// <summary>
        /// 确定具体的搜索结果类型（人才、钱粮或宝物）。
        /// </summary>
        /// <param name="general">当前将领。</param>
        /// <returns>最终搜索结果类型。</returns>
        private TaskType DetermineSearchResult(General general)
        {
            int randomValue = Random.Range(0, 100);
            byte notFoundGeneralNum = GetCityNotFoundGeneralNum();

            if (notFoundGeneralNum > 0)
            {
                if (general.HasSkill(4, 4)) // 如果拥有技能【眼力】
                {
                    return TaskType.SearchGeneral;
                }
                if (general.IQ >= 60 && general.moral  >= 70)
                {
                    return TaskType.SearchGeneral;
                }
                if (randomValue >= 60)
                {
                    return TaskType.SearchGeneral;
                }
            }
            
            if (randomValue > general.IQ && !general.HasSkill(3, 4))
            {
                return TaskType.SearchNothing;
            }

            if (general.HasSkill(4, 4)) // 如果拥有技能【眼力】，更有可能搜索到高价值目标
            {
                if (randomValue <= 40 && money < 30000) return TaskType.SearchMoney;
                if (randomValue <= 80 && food < 30000) return TaskType.SearchFood;
                return TaskType.SearchTreasure;
            }

            // 未拥有技能时的搜索结果
            if (randomValue < 10) return TaskType.SearchTreasure;
            if (randomValue < 50 && money < 30000) return TaskType.SearchMoney;
            if (randomValue < 90 && food < 30000) return TaskType.SearchFood;

            return TaskType.SearchNothing; // 默认无结果
        }
        
        /// <summary>
        /// 是否雇佣成功
        /// </summary>
        /// <param name="doGeneralId">执行的将领ID</param>
        /// <param name="beGeneralId">目标的将领ID</param>
        /// <returns>是否雇佣成功</returns>
        public bool IsEmploy(short doGeneralId, short beGeneralId)
        {
            General doGeneral = GeneralListCache.GetGeneral(doGeneralId);  // 获取当前将领
            General kingGeneral = GeneralListCache.GetGeneral(cityBelongKing);  // 获取该城市所属的国王
            General beGeneral = GeneralListCache.GetGeneral(beGeneralId);  // 获取被雇佣的将领

            // 如果被雇佣的将领不存在，返回false
            if (beGeneral == null)
                return false;

            int d1 = GeneralListCache.GetPhaseDifference(kingGeneral, beGeneral);  // 计算国王与被雇佣将领的相位差
            int d2 = GeneralListCache.GetPhaseDifference(doGeneral, beGeneral);  // 计算雇佣将领与被雇佣将领的相位差
            int i = Random.Range(0, 75);  // 获取随机数用于雇佣成功判定

            // 如果相位差满足雇佣条件，雇佣成功
            if ((d1 + d2) / 2 < i)
            {
                doGeneral.AddMoralExp(Random.Range(10, 25));  // 增加道德经验
                doGeneral.AddIqExp(Random.Range(4, 10));  // 增加智力经验
                AddOfficeGeneralId(beGeneralId);  // 将被登用的将领加入城市的职务名单
                RemoveReservedGeneralId(beGeneralId);  // 将该将领从在野将领列表中移除
                Debug.Log($"{kingGeneral.generalName}势力成功登用{beGeneral.generalName}！");
                return true;
            }

            return false;  // 雇佣失败
        }
        
        // 奖励将军
        public void Reward(short generalId, bool useTreasure)
        {
            General general = GeneralListCache.GetGeneral(generalId); // 获取将军对象
            if (useTreasure)
            {
                treasureNum = (byte)(treasureNum - 1); // 从城市中减少宝物数量
                general.RewardAddLoyalty(false); // 增加将军的忠诚度
            }
            else
            {
                SubGold(100); // 从城市中减少金钱
                general.RewardAddLoyalty(true); // 增加将军的忠诚度
            }
        }
        
        /// <summary>
        /// 计算将领进行屯田操作时增加的农业值
        /// </summary>
        /// <param name="general">屯田将领</param>
        /// <param name="useMoney">耗费金钱</param>
        /// <returns>增加的农业值</returns>
        public byte Reclaim(General general, int useMoney)
        {
            // 计算将领的内政和武力对农业值的贡献
            var contribution = ((general.force + general.political * 2) / 3);
            general.SubHp(2); // 将领进行屯田操作，减少当前体力
            general.Addexperience(Random.Range(0, 50) + 10); // 将领获得经验
            general.AddPoliticalExp(10); // 将领获得政治经验

            // 计算基础农业值增加
            int val = contribution < 12 ? 1 : (contribution * useMoney / 100) / 2;
            val += Random.Range(0, val + 1) / 2; // 随机增加农业值

            // 如果将领有特定技能，增加计算值
            if (general.HasSkill(4, 0))
            {
                val += val / 4;  // 技能王佐
            }
            else if (general.HasSkill(4, 2))
            {
                val += val / 3;  // 技能屯田
            }

            // 扣除使用的资金
            SubGold((short)useMoney);

            // 确保农业值不会超过999
            int maxAgroIncrease = 999 - agro;
            int actualAgroIncrease = Mathf.Clamp(val, 0, maxAgroIncrease);

            // 增加城市的农业值
            agro += (short)actualAgroIncrease;

            // 返回增加的农业值
            return (byte)actualAgroIncrease;
        }

        /// <summary>
        /// 计算将领进行劝商操作时增加的商业值
        /// </summary>
        /// <param name="general">劝商将领</param>
        /// <param name="useMoney">耗费金钱</param>
        /// <returns>增加的商业值</returns>
        public byte Mercantile(General general, int useMoney)
        {
            // 计算劝商提升的数值
            var contribution = (general.IQ + general.political * 2) / 3;
            general.SubHp(2); // 减少将领当前体力
            general.Addexperience(Random.Range(0, 50) + 10); // 增加将领经验
            general.AddPoliticalExp(10); // 增加将领政治经验

            // 计算基础贸易值增加
            int val = contribution < 12 ? 1 : (contribution * useMoney / 100) / 2;
            val += Random.Range(0, val + 1) / 2; // 随机增加贸易值

            // 如果将领有特定技能，增加计算值
            if (general.HasSkill(4, 0))
            {
                val += val / 4;  // 技能王佐
            }
            else if (general.HasSkill(4, 3))
            {
                val += val / 3;  // 技能商才
            }

            // 扣除使用的资金
            SubGold((short)useMoney);

            // 确保贸易值不会超过999
            int maxTradeIncrease = 999 - trade;
            int actualTradeIncrease = Mathf.Clamp(val, 0, maxTradeIncrease);

            // 增加城市的贸易值
            trade += (short)actualTradeIncrease;

            // 返回增加的贸易值
            return (byte)actualTradeIncrease;
        }

        /// <summary>
        /// 计算将领进行治水操作时增加的防洪值
        /// </summary>
        /// <param name="general">治水将领</param>
        /// <param name="useMoney">耗费金钱</param>
        /// <returns>增加的防洪值</returns>
        public byte Tame(General general, int useMoney)
        {
            // 计算内政治水提升的数值
            var contribution = (general.lead + general.political * 2) / 4;
            general.SubHp(2); // 减少将领当前体力
            general.Addexperience(Random.Range(0, 50) + 10); // 增加将领经验
            general.AddPoliticalExp(10); // 增加将领政治经验

            // 计算基础防洪值增加
            int val = contribution < 10 ? 1 : (contribution * useMoney / 100) / 2;
            val += Random.Range(0, val + 1) / 3; // 随机增加防洪值

            // 如果将领有特定技能，增加计算值
            if (general.HasSkill(4, 0))
            {
                val += val / 4;  // 技能王佐
            }

            // 扣除使用的资金
            SubGold((short)useMoney);

            // 确保防洪值不会超过99
            int maxFloodControlIncrease = 99 - floodControl;
            int actualFloodControlIncrease = Mathf.Clamp(val, 0, maxFloodControlIncrease);

            // 根据人口增加量增加统治度
            if (rule < 99)
            {
                int ruleIncrease = (actualFloodControlIncrease >= 5 ? 2 : 1);
                rule = (byte)Mathf.Clamp(rule + ruleIncrease, 0, 99);
            }

            // 增加城市的防洪值
            floodControl += (byte)actualFloodControlIncrease;

            // 返回增加的防洪值
            return (byte)actualFloodControlIncrease;
        }

        /// <summary>
        /// 计算将领进行巡查操作时增加的人口值
        /// </summary>
        /// <param name="general">巡查将领</param>
        /// <param name="useMoney">耗费金钱</param>
        /// <returns>增加的人口值</returns>
        public byte Patrol(General general, int useMoney)
        {
            // 计算内政巡查的提升值
            var contribution = (general.IQ + general.moral * 2 + general.political * 2) / 5;
            general.SubHp(2); // 减少将领当前体力
            general.Addexperience(Random.Range(0, 50) + 10); // 增加将领经验
            general.AddPoliticalExp(10); // 增加将领政治经验
            general.AddMoralExp(10); // 增加将领道德经验

            // 计算基础人口增加值
            int val = contribution < 50 ? 500 : contribution * useMoney;
            if (general.HasSkill(4, 0))
            {
                val += val / 4;  // 技能王佐
            }
            else if (general.HasSkill(4, 1))
            {
                val += val / 3;  // 技能仁政
            }

            // 扣除使用的资金
            SubGold((short)useMoney);

            // 确保人口不超过999999
            int maxPopulationIncrease = 999999 - population;
            int actualPopulationIncrease = Mathf.Clamp(val, 0, maxPopulationIncrease);

            // 根据人口增加量增加统治度
            if (rule < 99)
            {
                int ruleIncrease = (actualPopulationIncrease >= 2500) ? 3 : (actualPopulationIncrease >= 1500 ? 2 : 1);
                rule = (byte)Mathf.Clamp(rule + ruleIncrease, 0, 99);
            }

            // 增加人口
            population += actualPopulationIncrease;

            // 返回实际增加的人口值
            return (byte)actualPopulationIncrease;
        }

        public bool HaveCitySmithy()
        {   
            if(cityWeaponShop!=0)
            {
                return true;
            }
            return false;
        }   

        public bool HasWarSmithy()
        {   
            if(warWeaponShop!=0)
            {
                return true;
            }
            return false;
        }
        
 
        
        //卖出特殊武器锻造坊降级
        public void SellUniqueWeapon()
        {
            warWeaponShop = 2;
        }

        /// <summary>
        /// 检查城中低于生命阈值武将
        /// </summary>
        /// <param name="hpLimit">生命指标</param>
        /// <returns>城中低于生命阈值的武将ID列表</returns>
        public List <short> GetThresholdGeneralIds(int hpLimit)
        {
            List<short> thresholdGeneralIds = new List<short>();
            short[] officeGeneralIdArray = GetOfficerIds(); // 获取城市中的将军 ID 数组
            // 遍历城市中的将军
            for (byte i = 0; i < GetCityOfficerNum(); i++)
            {
                General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]); // 获取将军对象
                if (general.curPhysical < hpLimit) // 检查将军的血量
                {
                    thresholdGeneralIds.Add (officeGeneralIdArray[i]); // 添加将军 ID 到列表中
                }
            }
            return thresholdGeneralIds; // 返回可治疗将领列表
        }
        


        /// <summary>
        /// 检查是城中武将否可以学习
        /// </summary>
        /// <returns>城中可学习的武将ID列表</returns>
        public List<short> GetCanStudyGeneralIds()
        {
            List<short> canStudyGeneralIds = new List<short>();
            short[] officeGeneralIdArray = GetOfficerIds(); // 获取城市中的将军 ID 数组

            // 遍历城市中的将军
            for (byte i = 0; i < GetCityOfficerNum(); i++)
            {
                General general = GeneralListCache.GetGeneral(officeGeneralIdArray[i]); // 获取将军对象
                if (general.IQ < 120 && general.experience >= general.GetLearnNeedExp()) // 检查将军的 IQ 和经验
                {
                    canStudyGeneralIds.Add(officeGeneralIdArray[i]); // 添加将军 ID 到列表中
                }
            }
            return canStudyGeneralIds; // 返回可学习将领列表
        }
        

        // 检查是否与另一个城市相连
        public bool IsConnected(byte beCityId)
        {
            return Array.Exists(connectCityId, id => id == beCityId);
        }

        public bool IsRebel()
        {
            if (cityBelongKing == 0 || prefectId == 0)
                return false;
            // 获取城市的督察将军
            General prefectGeneral = GeneralListCache.GetGeneral(prefectId);

            // 获取城市所属的国家
            Country oldCountry = CountryListCache.GetCountryByKingId(cityBelongKing);

            // 如果将军的忠诚度大于90或者城市的所属国王ID等于督察ID，则不叛乱
            if (prefectGeneral.GetLoyalty() > 90 || cityBelongKing == prefectId)
                return false;

            // 计算忠诚度
            int loyalty = 100 - prefectGeneral.GetLoyalty();

            // 获取城市所属国王的将军
            General kingGeneral = GeneralListCache.GetGeneral(cityBelongKing);

            // 计算将军的阶段差
            int phaseDifference = GeneralListCache.GetdPhase(prefectGeneral.phase, kingGeneral.phase);

            // 如果忠诚度小于10或阶段差小于5，则不叛乱
            if (loyalty < 10 || phaseDifference < 5)
                return false;

            // 计算叛乱的临界值
            int threshold = loyalty - 5 + phaseDifference / 2 - oldCountry.GetHaveCityNum() - ((kingGeneral.moral - 80) / 9) - CountryListCache.GetCountrySize() / 2;
            threshold /= 2;

            // 如果计算出的值小于等于0，则不叛乱
            if (threshold <= 0)
                return false;

            // 随机值除以临界值
            int randomValue = Random.Range(0, 100) / threshold;

            // 如果随机值大于0，则不叛乱
            if (randomValue > 0)
                return false;

            // 获取城市连接的所有城市ID
            byte[] connectedCityIds = connectCityId;
            int maxAttackPower = 0;

            // 遍历连接城市，找出最大攻击力
            foreach (short tempCityId in connectedCityIds)
            {
                City tempCity = CityListCache.GetCityByCityId((byte)tempCityId);
                int attackPower = tempCity.GetMaxAtkPower();
                if (attackPower > maxAttackPower)
                    maxAttackPower = attackPower;
            }

            // 如果城市的防御能力小于最大攻击力的70%，则不叛乱
            if (GetDefenseAbility() < maxAttackPower * 0.7f)
                return false;

            // 如果以上条件均满足，则发生叛乱
            return true;
        }
    }
}


