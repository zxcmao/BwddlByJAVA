using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BaseClass;
using DataClass;
using Random = UnityEngine.Random;


[System.Serializable]
public class CountryListCache
{
    static byte maxCountryId;
    
    public static Dictionary<byte, Country> countryDictionary = new Dictionary<byte, Country>();

    public static List<byte> countrySequence = new List<byte>();
    
    private static HashSet<string> _countryColors = new(); // 已用势力颜色集合
    // 清空所有势力列表
    public static void ClearAllCountries()
    {
        countryDictionary.Clear();
    }

    /// <summary>
    /// 自建新势力
    /// </summary>
    /// <param name="cityId">自建势力首都</param>
    /// <param name="kingId">自建君主ID</param>
    /// <param name="generalIds">自建所有武将</param>
    public static void SelfBuildCountry(byte cityId, short kingId, List<short> generalIds)
    {
        Country country = new Country();
        country.countryId = (byte)(GetCountrySize() + 1);
        country.countryKingId = kingId;
        
        City city = CityListCache.GetCityByCityId(cityId);
        city.cityBelongKing = kingId;
        foreach (var id in generalIds)
        {
            city.AddOfficeGeneralId(id);
        }
        city.AppointmentPrefect(kingId);
        country.AddCity(cityId);
        
        generalIds.Remove(kingId);
        country.inheritGeneralIds = generalIds.ToArray();
        AddCountry(country);
        GameInfo.playerCountryId = country.countryId;
    }
    

    /// <summary>
    /// 添加国家并确保颜色唯一
    /// </summary>
    /// <param name="country">要添加的国家</param>
    public static void AddCountry(Country country)
    {
        // 分配唯一国家 ID
        if (country.countryId == 0)
        {
            Debug.LogWarning($"添加的势力ID为零");
            return;
        }

        // 设置国王的忠诚度
        short kingId = country.countryKingId;
        General general = GeneralListCache.GetGeneral(kingId);
        if (general != null)
            general.SetLoyalty(100);

        // 确保颜色唯一
        if (string.IsNullOrEmpty(country.countryColor))
        {
            country.countryColor = GenerateUniqueColor();
        }
        else if (_countryColors.Contains(country.countryColor))
        {
            // 如果颜色已存在，重新生成唯一颜色
            country.countryColor = GenerateUniqueColor();
        }

        // 添加到字典并记录颜色
        countryDictionary.TryAdd(country.countryId, country);
        _countryColors.Add(country.countryColor);
    }
    
    /// <summary>
    /// 生成唯一颜色字符串
    /// </summary>
    /// <returns>唯一的 HTML 颜色字符串</returns>
    private static string GenerateUniqueColor()
    {
        string colorStr;
        bool isUnique = false;

        do
        {
            // 随机生成颜色
            Color color = new Color(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
            colorStr = "#" + ColorUtility.ToHtmlStringRGB(color);

            // 检查唯一性
            isUnique = !_countryColors.Contains(colorStr);

        } while (!isUnique);

        Debug.Log($"生成唯一颜色: {colorStr}");
        return colorStr;
    }

    
    /// <summary>
    /// 处理势力灭亡逻辑
    /// </summary>
    /// <param name="countryId">灭亡的势力ID</param>
    public static void RemoveCountry(byte countryId)
    {
        Country country = GetCountryByCountryId(countryId);
        if (country == null) return;

        country.RemoveAllAlliance();
        if (country.GetHaveCityNum() > 0)
        {
            Debug.LogError($"{GeneralListCache.GetGeneral(country.countryKingId).generalName} 势力灭亡！但还有：{country.GetHaveCityNum()}个城池！");
            return;
        }

        General general = GeneralListCache.GetGeneral(country.countryKingId);
        if (general == null)
        {
            Debug.Log($"{GameInfo.chooseGeneralName} 势力灭亡！君主已死亡！");
        }
        else
        {
            GameInfo.chooseGeneralName = general.generalName;
        }
        
        RemoveSequence(countryId);
        Debug.Log($"势力: {GameInfo.chooseGeneralName} 灭亡!!! 剩余势力：{countrySequence.Count}");

        if (GameInfo.playerCountryId == countryId)
        {
            GameInfo.countryDieTips = 3;
        }
        else
        {
            GameInfo.countryDieTips = 4;
            GameInfo.ShowInfo = $"{GameInfo.chooseGeneralName} 势力灭亡了!";
        }
    }

    public static string KingName(byte countryId)
    {
        Country country = GetCountryByCountryId(countryId);
        General king = GeneralListCache.GetGeneral(country.countryKingId);
        if (king != null)
        {
            return king.generalName;
        }
        Debug.Log("找不到君主的姓名");
        return null;
    }


    public static byte GetCountrySize()
    {
        return (byte)countryDictionary.Count;
    }

    public byte GetCanBeChooseCountrySize()
    {
        int count = 0;
        foreach (KeyValuePair<byte, Country> country in countryDictionary)
        {
            if (country.Value.canBeChoose)
            {
                count++;
            }
        }
        return (byte)count;
    }

    public static Country GetCountryByCountryId(byte id)
    {
        if (countryDictionary.TryGetValue(id, out Country country))
        {
            return country;
        }
        Debug.LogError($"找不到{id}的势力");
        return null;
    }

    public static Country GetCanBeChooseCountryByIndex(byte id)
    {
        if (countryDictionary == null || countryDictionary.Count == 0)
        {
            Debug.LogError("countryList 为 null 或为空");
            return null;
        }
        int i = 0;
        foreach (KeyValuePair<byte, Country> country in countryDictionary)
        {
            if (country.Value == null) // 处理空对象的情况
            {
                break;
            }

            if (country.Value.canBeChoose) // 检查 canBeChoose
            {
                if (i == id) // 如果当前计数与目标 id 相同，返回该 country
                {
                    return country.Value;
                }
                i++; // 增加计数器
            }
        }
        return null; // 没有找到匹配的 country，返回 null
    }

    /// <summary>
    /// 根据君主ID获取国家对象
    /// </summary>
    /// <param name="kingId"></param>
    /// <returns>君主的国家</returns>
    public static Country GetCountryByKingId(short kingId)
    {
        return countryDictionary.Values.FirstOrDefault(country => country.countryKingId == kingId);
    }
    

    // 处理回合顺序，是否刷新国家顺序
    public static void TurnSort()
    {
        bool isFlush = false;  // 是否刷新国家顺序
        foreach (var pair in countryDictionary)
        {
            if (pair.Key <= 0 || pair.Value == null) // 处理空对象的情况
            {
                Debug.LogError("countryList存在null或ID为空");
                break;
            }
            Country country = pair.Value;
            if (country.GetHaveCityNum() <= 0 && countrySequence.Contains(pair.Key))
            {
                countrySequence.Remove(pair.Key);
            }
            else if (!countrySequence.Contains(pair.Key))  // 如果国家有城池且ID不在国家顺序列表中
            {
                isFlush = true;  // 标记需要刷新国家顺序
                countrySequence.Add(pair.Key);  // 添加到国家顺序列表
            }
        }
        if (isFlush)
        {
            countrySequence = countrySequence.OrderBy(c => GetCountryByCountryId(c).GetHaveCityNum()).ToList();  // 根据国家城池数量排序
        }
    }
    

    /// <summary>
    /// 添加一个国家到回合顺序中。
    /// </summary>
    /// <param name="countryId">国家ID</param>
    /// <param name="insertAfter">插入到某个国家之后，如果为 null，则追加到末尾</param>
    public static void AddSequence(byte countryId, byte? insertAfter = null)
    {
        if (countrySequence.Contains(countryId))
            return; // 避免重复添加

        if (insertAfter.HasValue)
        {
            int insertIndex = countrySequence.IndexOf(insertAfter.Value);
            if (insertIndex != -1)
                countrySequence.Insert(insertIndex + 1, countryId);
            else
                countrySequence.Add(countryId); // 如果找不到指定国家，直接追加
        }
        else
        {
            countrySequence.Add(countryId); // 直接追加到末尾
        }
    }

    /// <summary>
    /// 移除一个国家的顺序。
    /// </summary>
    /// <param name="countryId">国家ID</param>
    public static void RemoveSequence(byte countryId)
    {
        int index = countrySequence.IndexOf(countryId);
        if (index != -1)
        {
            countrySequence.RemoveAt(index);

            // 如果当前索引受影响，调整到下一个国家
            if (index <= GameInfo.curTurnIndex)
                GameInfo.curTurnIndex = (GameInfo.curTurnIndex - 1 + countrySequence.Count) % countrySequence.Count;
        }
    }
    // 获取当前执行的国家ID
    public static byte GetCurrentExecutionCountryId()
    {
        if (countrySequence.Count == 0)
        {
            TurnSort();
            return countrySequence[0]; 
        }
        
        GameInfo.curTurnIndex = (GameInfo.curTurnIndex + 1) % countrySequence.Count;
        if (countrySequence[GameInfo.curTurnIndex] != 0)
        {
            var nextCountry = GetCountryByCountryId(countrySequence[GameInfo.curTurnIndex]);// 根据国家ID获取国家对象
            if (nextCountry != null &&  nextCountry.GetHaveCityNum() > 0)// 如果国家存在并且轮到其回合
            {
                return countrySequence[GameInfo.curTurnIndex];
            }
            else
            {
                Debug.LogError(countrySequence[GameInfo.curTurnIndex]+"当前国家不存在或没有城池");
            }
        }
        else
        {
            Debug.LogError("国家ID为零");
        }
        
        return countrySequence[0];
    }
    






    public static int GetEnemyAdjacentCityDefenseAbility(City defCity, City atkCity)
    {
        Country atkCountry = GetCountryByKingId(atkCity.cityBelongKing);
        int maxDefenseAbility = defCity.GetDefenseAbility();
        byte[] cityIdArray = defCity.connectCityId;

        foreach (byte tempCityId in cityIdArray)
        {
            if (tempCityId != atkCity.cityId)
            {
                City tempCity = CityListCache.GetCityByCityId(tempCityId);
                if (tempCity.cityBelongKing != 0)
                {
                    if (tempCity.cityBelongKing == atkCity.cityBelongKing || atkCountry.IsAlliance(tempCityId))
                    {
                        maxDefenseAbility -= (int)(tempCity.GetMaxAtkPower() * 0.2);
                    }
                    else
                    {
                        maxDefenseAbility += (int)(tempCity.GetMaxAtkPower() * 0.1);
                    }
                }
            }
        }
        return maxDefenseAbility;
    }

    public static byte[] GetEnemyAdjacentCityIdArray(byte countryId)
    {
        List<byte> resultCityIdArray = new List<byte>();
        Country country = GetCountryByCountryId(countryId);
        foreach (var cityID in country.cityIDs)
        {
            byte[] connectCityIdArray = CityListCache.GetCityByCityId(cityID).connectCityId;

            bool haveEnemy = false;

            foreach (byte cityId in connectCityIdArray)
            {
                short belongKing = CityListCache.GetCityByCityId(cityId).cityBelongKing;
                if (belongKing != country.countryKingId)
                {
                    Country otherCountry = GetCountryByKingId(belongKing);
                    if (otherCountry != null && otherCountry.GetAllianceById(countryId) == null)
                    {
                        haveEnemy = true;
                        break;
                    }
                }
            }

            if (haveEnemy)
            {
                resultCityIdArray.Add(cityID);
            }
        }
        return resultCityIdArray.ToArray();
    }

    public static byte[] GetNoEnemyAdjacentCityIdArray(byte countryId)
    {
        List<byte> resultCityIdArray = new List<byte>();
        Country country = GetCountryByCountryId(countryId);
        foreach (var cityID in country.cityIDs)
        {
            byte[] cityIdArray = CityListCache.GetCityByCityId(cityID).connectCityId;

            bool noHaveEnemy = true;

            foreach (byte cityId in cityIdArray)
            {
                short belongKing = CityListCache.GetCityByCityId(cityId).cityBelongKing;
                if (belongKing != country.countryKingId)
                {
                    Country otherCountry = GetCountryByKingId(belongKing);
                    if (otherCountry != null && otherCountry.GetAllianceById(countryId) == null)
                    {
                        noHaveEnemy = false;
                        break;
                    }
                }
            }

            if (noHaveEnemy)
            {
                resultCityIdArray.Add(cityID);
            }
        }
        return resultCityIdArray.ToArray();
    }

    /// <summary>
    /// 获取指定城池周边的敌方城池
    /// </summary>
    /// <param name="cityId">指定城池ID</param>
    /// <returns>敌方城池列表</returns>
    public static List<byte> GetAdjacentEnemyCityIds(byte cityId)
    {
        City city = CityListCache.GetCityByCityId(cityId);
        short kingId = city.cityBelongKing;
        Country country = GetCountryByKingId(kingId);
        byte[] cityIdArray = city.connectCityId;
        List<byte> resultCityIdArray = new List<byte>();

        foreach (byte adjacentCityId in cityIdArray)
        {
            short belongKing = CityListCache.GetCityByCityId(adjacentCityId).cityBelongKing;
            if (belongKing != kingId)
            {
                Country otherCountry = GetCountryByKingId(belongKing);
                if (otherCountry == null || otherCountry.GetAllianceById(country.countryId) == null)
                {
                    resultCityIdArray.Add(adjacentCityId);
                }
            }
        }
        return resultCityIdArray;
    }

    public static byte[] getEnemyCityIdArray_new(byte cityId)
    {
        short kingId = CityListCache.GetCityByCityId(cityId).cityBelongKing;
        byte[] cityIdArray = CityListCache.GetCityByCityId(cityId).connectCityId;
        List<byte> resultCityIdArray = new List<byte>();

        foreach (byte tempCityId in cityIdArray)
        {
            short belongKing = CityListCache.GetCityByCityId(tempCityId).cityBelongKing;
            if (belongKing != kingId)
            {
                resultCityIdArray.Add(tempCityId);
            }
        }
        return resultCityIdArray.ToArray();
    }


    // 获取敌方相邻的最大攻击力
    public static int GetEnemyAdjacentMaxAtkPower(byte cityId)
    {
        City city = CityListCache.GetCityByCityId(cityId);
        short kingId = city.cityBelongKing;
        Country country = GetCountryByKingId(kingId);
        List<int> resultAtkPowerList = (from adjacentCityId in city.connectCityId select CityListCache
            .GetCityByCityId(adjacentCityId) into otherCity let belongKing = otherCity
            .cityBelongKing where belongKing != kingId let otherCountry = GetCountryByKingId(belongKing) 
            where otherCountry != null where otherCountry.GetAllianceById(country.countryId) == null select otherCity
                .GetMaxAtkPower()).ToList();

        return resultAtkPowerList.OrderByDescending(t => t).FirstOrDefault();
    }

    public static int GetOtherCityMaxAtkPower(byte cityId, byte beCityId)
    {
        short kingId = CityListCache.GetCityByCityId(cityId).cityBelongKing;
        Country country = GetCountryByKingId(kingId);
        byte[] cityIdArray = CityListCache.GetCityByCityId(cityId).connectCityId;
        int[] resultAtkPowerArray = new int[0];

        for (int i = 0; i < cityIdArray.Length; i++)
        {
            if (beCityId != cityIdArray[i])
            {
                short belongKing = CityListCache.GetCityByCityId(cityIdArray[i]).cityBelongKing;
                if (belongKing != kingId)
                {
                    Country otherCountry =  GetCountryByKingId(belongKing);
                    if (otherCountry != null && otherCountry.GetAllianceById(country.countryId) == null)
                    {
                        int[] tempResultAtkPowerArray = new int[resultAtkPowerArray.Length + 1];
                        for (int k = 0; k < resultAtkPowerArray.Length; k++)
                        {
                            tempResultAtkPowerArray[k] = resultAtkPowerArray[k];
                        }
                        tempResultAtkPowerArray[resultAtkPowerArray.Length] = CityListCache.GetCityByCityId(cityIdArray[i]).GetMaxAtkPower();
                        resultAtkPowerArray = tempResultAtkPowerArray;
                    }
                }
            }
        }

        if (resultAtkPowerArray.Length == 0)
        {
            return 0;
        }

        int maxAtkPower = resultAtkPowerArray[0];
        for (int j = 1; j < resultAtkPowerArray.Length; j++)
        {
            if (maxAtkPower < resultAtkPowerArray[j])
            {
                maxAtkPower = resultAtkPowerArray[j];
            }
        }

        return maxAtkPower;
    }
    public static int GetMaxAtkPowerInEnemyCities(City city, City excludeCity)
    {
        // 检查 city 是否为 null
        if (city == null)
        {
            Debug.LogError("City is null. Unable to calculate max attack power.");
            return 0;
        }

        short tarKingId = city.cityBelongKing;
        // 检查 kingId 是否有效并获取所属国家
        Country tarCountry = GetCountryByKingId(tarKingId);
        if (tarCountry == null)
        {
            Debug.Log($"考虑占领空城: {city.cityName}");
        }

        // 检查相邻城池是否为 null
        byte[] cityIdArray = city.connectCityId;
        if (cityIdArray == null || cityIdArray.Length == 0)
        {
            Debug.LogWarning($"City {city.cityId} has no connected cities.");
            return 0;
        }

        List<int> resultAtkPowerList = new List<int>();

        // 遍历连接的城市 ID
        for (int i = 0; i < cityIdArray.Length; i++)
        {
            if (cityIdArray[i] == excludeCity.cityId) 
                continue; // 排除特定城市 ID

            // 获取连接城市的对象并检查
            City connectedCity = CityListCache.GetCityByCityId(cityIdArray[i]);
            if (connectedCity == null)
            {
                Debug.LogWarning($"Connected city with ID {cityIdArray[i]} not found.");
                continue;
            }

            short belongKing = connectedCity.cityBelongKing;

            // 检查是否是敌方城市
            if (belongKing != 0 && belongKing != tarKingId)
            {
                Country otherCountry = GetCountryByKingId(belongKing);
                if (otherCountry == null)
                {
                    Debug.LogWarning($"Country not found for kingId: {belongKing}");
                    continue;
                }

                // 检查是否为联盟国家
                if (otherCountry.GetAllianceById(GetCountryByKingId(excludeCity.cityBelongKing).countryId) == null)
                {
                    int atkPower = connectedCity.GetMaxAtkPower();
                    resultAtkPowerList.Add(atkPower);
                }
            }
        }

        // 如果没有有效的敌方城市，返回 0
        if (resultAtkPowerList.Count == 0)
        {
            Debug.LogWarning("No enemy cities found with valid attack power.");
            return 0;
        }

        // 找出最大攻击力
        int maxAtkPower = resultAtkPowerList.Max();
        return maxAtkPower;
    }

    
    public static byte GetAIOredrNum(byte curTurnsCountryId)
    {
        byte haveCityNum = GetCountryByCountryId(curTurnsCountryId).GetHaveCityNum();
        if (haveCityNum < 3)
        {
            haveCityNum = 3;
        }
        else if (haveCityNum > 8)
        {
            haveCityNum = 8;
        }

        return haveCityNum;
    }

}





// 定义一个名为Alliance的类，用于表示联盟信息
[System.Serializable] // 此属性允许Unity序列化此类，以便在Inspector中使用
public class Alliance
{
    // 存储联盟所属势力ID的字段
    public byte countryId;

    // 存储联盟持续月份的字段，已在构造函数中初始化
    public byte months;

    // Alliance类的构造函数，接受势力ID和月份作为参数
    public Alliance(byte countryId, byte months)
    {
        this.countryId = countryId; // 初始化势力ID
        this.months = months;      // 初始化月份
    }

    // 联盟持续月份的属性，提供getter和setter方法
    public byte Months
    {
        get { return months; }
        set { months = value; }
    }
}
