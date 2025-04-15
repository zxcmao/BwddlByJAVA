using System.Collections.Generic;
using System.Linq;
using DataClass;
using Newtonsoft.Json;
using UnityEngine;

namespace BaseClass
{
    [System.Serializable]
    public class Country
    {    // 定义公开的字段
        public byte countryId; // 势力ID
        public short countryKingId;// 君主ID
        public string countryColor;// 势力颜色
        public short power; // 力量值
        public bool canBeChoose;// 是否可以被选择
        [JsonProperty("inheritGeneralIds[]")] public short[] inheritGeneralIds; // 继承武将ID数组

        // 用于存储城市的列表
        public List<byte> cityIDs = new List<byte>();//势力所有城市ID列表
        public List<Alliance> allianceList = new List<Alliance>();// 盟友列表

        public string KingName()
        {
            General king = GeneralListCache.GetGeneral(countryKingId);
            if (king != null)
            {
                return king.generalName;
            }
            Debug.LogError("找不到君主对象");
            return null;
        }

        // 添加继承武将ID的方法
        public void addInheritGeneralId(short generalId)
        {
            short[] tempInheritGeneralIds = new short[this.inheritGeneralIds.Length + 1];
            System.Array.Copy(this.inheritGeneralIds, 0, tempInheritGeneralIds, 0, this.inheritGeneralIds.Length);
            tempInheritGeneralIds[tempInheritGeneralIds.Length - 1] = generalId;
            inheritGeneralIds = tempInheritGeneralIds;
        }

        
        /// <summary>
        /// 寻找势力内武将所在的城池ID
        /// </summary>
        /// <param name="generalId">寻找的武将ID</param>
        /// <returns>势力内武将所在的城池ID，0表示不存在</returns>
        public byte FindCityOfGeneral(short generalId)
        {
            // 遍历所有城市ID
            foreach (byte cityId in cityIDs)
            {
                // 获取城市对象
                City city = CityListCache.GetCityByCityId(cityId);
                if (city == null) continue; // 如果城市对象为空，跳过

                // 获取将军ID数组
                short[] generalIdArray = city.GetOfficerIds();
                if (generalIdArray == null) continue; // 如果将军ID数组为空，跳过

                // 使用 Array.Exists 判断将军是否存在于该城市
                if (System.Array.Exists(generalIdArray, id => id == generalId))
                {
                    return cityId; // 找到将军所在城市，返回城市ID
                }
            }

            // 未找到返回0
            return 0;
        }


        /// <summary>
        /// 用于计算继承武将的评分
        /// </summary>
        /// <param name="generalId">武将ID</param>
        /// <returns>武将的评分</returns>
        int InheritScore(short generalId)
        {
            General general = GeneralListCache.GetGeneral(generalId);
            int score = general.IQ + general.lead + general.moral;

            // IQ 条件
            if (general.IQ >= 95) score += 100;
            else if (general.IQ >= 90) score += 70;
            else if (general.IQ >= 80) score += 40;
            else if (general.IQ >= 70) score += 20;

            // lead 条件
            if (general.lead >= 95) score += 50;
            else if (general.lead >= 90) score += 30;
            else if (general.lead >= 80) score += 20;
            else if (general.lead >= 70) score += 10;

            // Moral 条件
            if (general.moral >= 95) score += 50;
            else if (general.moral >= 90) score += 40;
            else if (general.moral >= 80) score += 20;
            else if (general.moral >= 70) score += 10;

            return score;
        }


        /// <summary>
        /// 获取继承武将ID的方法
        /// </summary>
        /// <returns>返回继承武将ID</returns>
        public short GetInheritGeneralId()
        {
            // 首先检查 inheritGeneralIds 数组
            foreach (short id in inheritGeneralIds)
            {
                General general = GeneralListCache.GetGeneral(id);
                if (general != null && FindCityOfGeneral(id) != 0)
                {
                    return id;
                }
            }

            // 初始化最大值
            int maxValue = 0;
            short generalId = 0;

            // 遍历国家下辖的城市集合
            foreach (byte cityId in this.cityIDs)  // 假设 this.citys 是 List<City>
            {
                // 根据城市ID获取具体的City对象
                City city = CityListCache.GetCityByCityId(cityId); // 你需要实现或使用的查找方法
                // 获取该城市的将领 ID 数组
                short[] generalIdArray = city.GetOfficerIds();

                // 遍历将领 ID 数组并计算其分数
                foreach (short id in generalIdArray)
                {
                    int value = InheritScore(id);
                    if (value > maxValue)
                    {
                        maxValue = value;
                        generalId = id;
                    }
                }
            }

            // 返回分数最高的将领 ID
            return generalId;
        }


        // 查找君主所在城市
        public byte FindKingCity()
        {
            byte cityId = cityIDs
                    .Select(CityListCache.GetCityByCityId) // 获取 City 对象
                    .FirstOrDefault(city => city != null && city.prefectId == countryKingId)?.cityId ?? (byte)0;
            
            // 如果没有找到，打印调试信息
            if (cityId == 0)
            {
                Debug.LogError("没有找到君主所在城市");
            }
            return cityId;
        }

        public bool FindVacantCity(out byte cityID)
        {
            cityID = 0;
            foreach (var id in cityIDs)
            {
                City city = CityListCache.GetCityByCityId(id);
                if (city.GetCityOfficerNum() < 10)
                {
                    cityID = id;
                    return true;
                }
            }
            Debug.Log("没有找到该势力中有空职的城池");
            return false;
        }

        public short GetVacantOfficerNum()
        {
            short num = (short)(10 * cityIDs.Count);
            foreach (var id in cityIDs)
            {
                City city = CityListCache.GetCityByCityId(id);
                num -= city.GetCityOfficerNum();
            }
            return num;
        }
        
        // 执行继承武将操作
        public short Inherit()
        {
            // 获取继承武将ID
            short inheritGeneralId = ((short)GetInheritGeneralId());

            // 如果找到了符合条件的武将ID
            if (inheritGeneralId != 0)
            {
                // 执行继承操作
                Inherit(inheritGeneralId);
            }

            // 返回继承的武将ID
            return inheritGeneralId;
        }



        // 执行继承操作
        public void Inherit(short inheritGeneralId)
        {
            string oldKing = GeneralListCache.GetGeneral(countryKingId).generalName;
            Debug.Log(oldKing);
            // 更新每个城市的归属
            foreach (var curCity in cityIDs.Select(CityListCache.GetCityByCityId))
            {
                curCity.cityBelongKing = inheritGeneralId;
            }

            // 获取继承武将所在的城市ID
            byte cityId = FindCityOfGeneral(inheritGeneralId);

            // 如果找不到对应的武将所在城市，则直接返回
            if (cityId == 0)
                return;

            // 获取城市对象并任命太守
            City liveCity = CityListCache.GetCityByCityId(cityId);
            liveCity.AppointmentPrefect(inheritGeneralId);

            // 更新武将的忠诚度
            General general = GeneralListCache.GetGeneral(inheritGeneralId);
            general.SetLoyalty(100);

            // 更新势力君主ID
            GameInfo.ShowInfo = $"原{oldKing}势力的{general.generalName}继承了势力";
            countryKingId = inheritGeneralId;
        }

        // 获取势力的现有士兵数
        public int GetCountrySoldierNum()
        {
            // 返回力量值
            return cityIDs.Sum(cityId => CityListCache.GetCityByCityId(cityId).GetCityAllSoldierNum());
        }

        public int GetMaxCountrySoldierNum()
        {
            return cityIDs.Sum(cityId => CityListCache.GetCityByCityId(cityId).GetMaxSoldierNum());
        }
        
        // 获取拥有的城市数量
        public byte GetHaveCityNum()
        {
            return (byte)cityIDs.Count;
        }

        public bool IsEndangered() => cityIDs.Count == 1;
        public bool IsDestroyed() => cityIDs.Count <= 0;
        
        public bool AddCity(byte cityId)
        {
            if (cityId == 0)
            {
                Debug.LogError($"势力{countryId}要添加的城池ID为零");
                return false;
            }
            // 如果城市ID已经在列表中，跳过添加
            if (cityIDs.Contains(cityId))
            {
                return true;
            }

            // 获取城市对象
            City city = CityListCache.GetCityByCityId(cityId);

            // 将城市的所属国王ID设置为当前国家的国王ID
            city.cityBelongKing = countryKingId;

            // 将城市ID添加到列表中
            cityIDs.Add(cityId);
            return true;
        }

        // 移除城市
        public bool RemoveCity(byte cityId)
        {
            if (cityId == 0)
            {
                Debug.LogError($"势力{countryId}要移除的城池ID为零!");
                return false;
            }
            if (!cityIDs.Contains(cityId)) return false;
            City city = CityListCache.GetCityByCityId(cityId);  // 获取城市对象
            city.prefectId = 0;  // 清除城市的太守ID
            city.cityBelongKing = 0;  // 清除城市的归属国家
            cityIDs.Remove(cityId);
            return true;
        }
        
        
        public bool Truce(byte otherCountryId)
        {
            General countryKing = GeneralListCache.GetGeneral(countryKingId);// 获取当前国家国王
            
            // 获取其他国家
            Country otherCountry = CountryListCache.GetCountryByCountryId(otherCountryId);
            General otherCountryKing = GeneralListCache.GetGeneral(otherCountry.countryKingId);// 获取其他国家国王

            // 计算当前国家国王和其他国家国王的相位差
            int d = GeneralListCache.GetPhaseDifference(countryKing, otherCountryKing);                

            // 如果相位差加上被结盟国家的城市数小于随机值
            if (d + otherCountry.GetHaveCityNum() <= Random.Range(0, 75))
            {
                // 执行结盟操作
                AddAlliance(otherCountry.countryId);
                // 输出结盟成功的日志
                Debug.Log(countryKing.generalName + "势力与" + otherCountryKing.generalName + "势力同盟成功！");
                return true;
            }
            else
            {
                // 输出结盟失败的日志
                Debug.Log(countryKing.generalName + "势力与" + otherCountryKing.generalName + "势力同盟失败！");
                return false;
            }
        }

        
        /*     */
        /*     */

        // 检查城市是否属于联盟
        public bool IsAlliance(byte cityId)
        {
            // 获取城市对象
            City city = CityListCache.GetCityByCityId(cityId);

            // 获取归属君主ID
            short belongKing = city.cityBelongKing;

            // 如果归属君主ID为0，则不属于任何势力
            if (belongKing == 0)
                return false;

            // 获取归属君主所属的势力
            Country otherCountry = CountryListCache.GetCountryByKingId(belongKing);

            // 如果没有找到对应的势力，则不属于任何联盟
            if (otherCountry == null)
                return false;

            // 检查是否有联盟关系
            Alliance alliance = GetAllianceById(otherCountry.countryId);

            // 如果没有找到对应的联盟，则不属于任何联盟
            if (alliance == null)
                return false;

            // 属于联盟
            return true;
        }


        // 获取联盟大小
        public byte GetAllianceSize()
        {
            // 返回联盟大小
            return (byte)allianceList.Count;
        }
        
        // 通过势力ID获取联盟对象
        public Alliance GetAllianceById(byte countryID)
        {
            return allianceList.FirstOrDefault(alliance => alliance.countryId == countryID);
        }

        // 添加联盟
        public void AddAlliance(Alliance alliance)
        {
            // 检查是否已经有这个势力的联盟
            Alliance existingAlliance = GetAllianceById(alliance.countryId);

            // 如果已经有这个势力的联盟，则直接返回
            if (existingAlliance != null)
                return;

            // 添加新的联盟到列表
            allianceList.Add(alliance);

            // 获取对应的势力
            Country country = CountryListCache.GetCountryByCountryId(alliance.countryId);

            // 如果没有找到对应的势力，则直接返回
            if (country == null)
                return;

            // 在对应的势力中添加联盟
            Alliance reverseAlliance = new Alliance(countryId, alliance.Months);
            country.AddAlliance(reverseAlliance);
        }

        // 通过另一个势力ID添加联盟
        public void AddAlliance(byte otherCountryId)
        {
            // 检查是否已经有这个势力的联盟
            Alliance existingAlliance = GetAllianceById(otherCountryId);

            // 如果已经有这个势力的联盟，则直接返回
            if (existingAlliance != null)
                return;

            // 创建新的联盟
            Alliance alliance = new Alliance(otherCountryId, 12);

            // 添加新的联盟到列表
            allianceList.Add(alliance);

            // 获取对应的势力
            Country country = CountryListCache.GetCountryByCountryId(otherCountryId);

            // 创建反向联盟
            Alliance reverseAlliance = new Alliance(this.countryId, 12);

            // 在对应的势力中添加联盟
            country.AddAlliance(reverseAlliance);
        }
    
   
        // 移除联盟
        public bool RemoveAlliance(byte countryID)
        {
            bool result = false;

            for (int i = 0; i < allianceList.Count; i++)
            {
                Alliance alliance = allianceList[i];
                if (alliance.countryId == countryID)
                {
                    allianceList.Remove(alliance); 
                    Country country = CountryListCache.GetCountryByCountryId(countryID);
                    if (country == null) return false;
                    if (countryID == GameInfo.playerCountryId)
                    {
                        result = true;
                    }
                    country.RemoveAlliance(this.countryId);
                    break;
                }
            }

            return result;
        }
    

        // 移除所有联盟
        public void RemoveAllAlliance()
        {
            while (allianceList.Count > 0)
            {
                Alliance alliance = allianceList[0];
                allianceList.RemoveAt(0); // 移除列表中的第一个元素
                Country country = CountryListCache.GetCountryByCountryId(alliance.countryId);
                if (country != null)
                {
                    country.RemoveAlliance(this.countryId);
                }
            }
        }


        // 获取没有联盟关系的势力ID数组
        public byte[] GetNoCountryIdAllianceCountryIdArray()
        {
            // 初始化势力ID数组
            List<byte> countryIdList = new List<byte>();

            // 遍历所有势力
            foreach (var i in CountryListCache.countryDictionary)
            {
                // 获取当前势力
                Country country = i.Value;//CountryListCache.GetCountryByCountryId(i);

                // 如果找到了对应的势力并且该势力至少有一个城市
                if (country != null && country.GetHaveCityNum() >= 1 &&
                    country.countryId != countryId)
                {
                    // 标记是否有联盟
                    bool haveAlliance = allianceList.Any(alliance => alliance.countryId == country.countryId);

                    // 如果没有联盟
                    if (!haveAlliance)
                    {
                        // 将当前势力ID添加到列表
                        countryIdList.Add(country.countryId);
                    }
                }
            }

            // 将列表转换为数组并返回
            return countryIdList.ToArray();
        }
    
        // 获取没有联盟关系的势力数量
        public byte getNoAllianceCountrySize()
        {
            // 调用方法获取没有联盟关系的势力ID数组
            byte[] noAllianceCountryIds = GetNoCountryIdAllianceCountryIdArray();

            // 返回数组长度作为势力数量
            return (byte)noAllianceCountryIds.Length;
        }

   
        // AI势力是否需要救济金
        public void NeedAlms()
        {
            foreach (var cityID in cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityID);
                if (city.GetMoney() <= 100)  // 如果城市资金少于等于100
                    city.AddGold((short)Random.Range(0, 200));  // 增加城市资金
                if (city.GetFood() <= 300)  // 如果城市粮食少于等于300
                    city.AddFood((short)Random.Range(0, 400));  // 增加城市粮食
            }
        }

        
        
        /// <summary>
        /// 将武将们从指定城池撤退到其他城池
        /// </summary>
        /// <param name="generalIdArray"></param>
        /// <param name="curCityId"></param>
        /// <param name="food"></param>
        /// <param name="money"></param>
        /// <param name="chiefGeneralCaptured"></param>
        /// <returns></returns>
        public bool AIRetreatGeneralToCity(short[] generalIdArray, byte curCityId, int food, int money, bool chiefGeneralCaptured)
        {
            bool retreat = false;
            City curCity = CityListCache.GetCityByCityId(curCityId);

            if (IsDestroyed())
            {
                Debug.Log($"{GeneralListCache.GetGeneral(this.countryKingId).generalName}势力已无城池可撤退！");
                for (int j = 0; j < generalIdArray.Length; j++)
                {
                    short generalId = generalIdArray[j];
                    General general = GeneralListCache.GetGeneral(generalId);
                    if (general != null)
                    {
                        if (generalId == this.countryKingId)
                        {
                            Debug.Log($"君主：{general.generalName}无城池可撤退！死亡了");
                            GeneralListCache.GeneralDie(generalId);
                        }
                        else if (curCity.AddReservedGeneralId(generalId))
                        {
                            Debug.Log($"{general.generalName}无城池可撤退！在{curCity.cityName}下野！");
                        }
                    }
                }

                return retreat;
            }

            int index = 0;
            for (int i = 0; i < generalIdArray.Length; i++)
            {
                short generalId = generalIdArray[i];
                General general = GeneralListCache.GetGeneral(generalId);
                if (general != null)
                {
                    foreach (var t in cityIDs)
                    {
                        byte cityId = (byte)t;
                        if (cityId != curCityId)
                        {
                            City city = CityListCache.GetCityByCityId(cityId);
                            if (city.GetCityOfficerNum() <= 9 || generalId == this.countryKingId)
                            {
                                city.AddOfficeGeneralId(generalId);
                                index++;
                                if (i == 0 && !chiefGeneralCaptured)
                                {
                                    city.AddFood((short)food);
                                    city.AddGold((short)money);
                                    retreat = true;
                                    Debug.Log($"主将：{general.generalName}撤退到{city.cityName}");
                                    break;
                                }
                                Debug.Log($"武将：{general.generalName}撤退到{city.cityName}");
                                break;
                            }
                        }
                    }
                }
            }

            if (index < generalIdArray.Length - 1)
            {
                for (; index < generalIdArray.Length; index++)
                {
                    short generalId = generalIdArray[index];
                    General general = GeneralListCache.GetGeneral(generalId);
                    if (general != null && curCity.AddReservedGeneralId(generalId))
                    {
                        Debug.Log($"{general.generalName}无城池可撤退！在{curCity.cityName}下野！");
                    }
                }
            }

            return !chiefGeneralCaptured && retreat;
        }
    }
}
