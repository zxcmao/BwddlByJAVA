using System.Collections.Generic;
using UnityEngine;
using System;
using BaseClass;
using DataClass;
using War;
using Random = UnityEngine.Random;


public class GeneralListCache 
{
    private static short maxGeneralId = 0;
    public static List<General> generalList = new List<General>();//已出场武将列表
    public static List<General> noDebutGeneralList = new List<General>();//待年份出场武将

    public static Dictionary<short, General> generalDictionary = new Dictionary<short, General>();
    public static void clearAllGenerals()
    {
        // 清空所有将领列表
        generalList.Clear();
    }

    public static void clearAllNoDebutGenerals()
    {
        // 清空未登场的将领列表
        noDebutGeneralList.Clear();
        maxGeneralId = 0;
    }

    public static void ClearAllTotalGenerals()
    {
        generalList.Clear();
        noDebutGeneralList.Clear();
        generalDictionary.Clear();
    }

    public static void DebutByYears(short years)
    {
        // 根据指定的年份让将领登场
        if (noDebutGeneralList.Count == 0)
        {
            return; // 如果没有未登场的将领，则直接返回
        }

        for (int i = 0; i < noDebutGeneralList.Count; i++)
        {
            General general = noDebutGeneralList[i];
            if (general.debutYears <= years) // 如果该将领的登场年份小于或等于当前年份
            {
                // 移除未登场将领列表中的当前将领，并将其加入已登场将领列表
                noDebutGeneralList.RemoveAt(i);
                i--; // 因为移除了元素，所以需要将索引回退一位
                generalList.Add(general);
                noDebutGeneralList.Remove(general);

                byte cityId = general.debutCity; // 获取将领的登场城市ID
                if (general.followGeneralId != 0) // 如果该将领跟随其他将领
                {
                    short followCityId = general.followGeneralId;
                    General followGeneral = GetGeneral(followCityId); // 获取跟随的将领
                    if (followGeneral != null)
                    {
                        cityId = followGeneral.debutCity; // 更新城市ID为跟随将领的登场城市
                        City city = CityListCache.GetCityByCityId(cityId);
                        if (followGeneral.isOffice == 1) // 如果跟随的将领是官员
                        {
                            if (city.GetCityOfficerNum() >= 10) // 如果城市官员数量已满
                            {
                                if (city.GetReservedGeneralNum() < 10) // 如果城市非官员数量未满
                                {
                                    city.AddReservedGeneralId(general.generalId);
                                    Debug.Log("跟随将"+$"{general.generalName}" +"前往"+$"{city.cityName}" +"下野");
                                }
                            }
                            else // 如果城市官员数量未满
                            {
                                city.AddOfficeGeneralId(general.generalId);
                                Debug.Log("跟随将"+$"{general.generalName}" +"成为"+$"{city.cityName}" +"的官员");
                            }
                        }
                        else if (city != null) // 如果城市存在
                        {
                            if (city.GetReservedGeneralNum() < 10) // 如果城市非官员数量未满
                            {
                                city.AddReservedGeneralId(general.generalId);
                                Debug.Log("武将"+$"{general.generalName}" +"前往"+$"{city.cityName}" +"下野");
                            }
                        }
                    }
                }
                else if (general.debutCity == 0) // 如果将领没有指定的登场城市
                {
                    cityId = (byte) UnityEngine.Random.Range(0,CityListCache.CITY_NUM); // 随机选择一个城市ID
                    City city = CityListCache.GetCityByCityId(cityId);
                    city.AddNotFoundGeneralId(general.generalId); // 将将领添加到城市的未找到将领列表
                    Debug.Log("武将"+$"{general.generalName}" +"前往"+$"{city.cityName}" +"归隐");
                }
                else
                {
                    City city = CityListCache.GetCityByCityId(general.debutCity);
                    city.AddNotFoundGeneralId(general.generalId); // 将将领添加到指定城市的未找到将领列表
                    Debug.Log("武将"+$"{general.generalName}" +"前往"+$"{city.cityName}"    +"归隐");
                }
            }
        }
    }


    /// <summary>
    /// 根据武将ID获取武将对象
    /// </summary>
    /// <param name="generalId">武将ID</param>
    /// <returns>找到的武将对象，如果没有找到则返回null</returns>
    public static General GetGeneral(short generalId)
    {
        // 如果找到匹配的武将ID，则返回该武将对象
        if (generalDictionary.TryGetValue(generalId, out var general))
        {
            return general;
        }
        // 如果没有找到匹配的武将，打印错误信息并返回null
        Debug.LogError("获取武将错误,将领Id:" + generalId);
        return null;
    }

    /// <summary>
    /// 根据索引获取未登场的武将对象
    /// </summary>
    /// <param name="indexId">索引ID</param>
    /// <returns>指定索引位置的未登场武将对象</returns>
    public static General GetNoDebutGeneralByIndex(short indexId)
    {
        // 直接通过索引获取未登场的武将对象
        return noDebutGeneralList[indexId];
    }

    /// <summary>
    /// 获取未登场的武将总数
    /// </summary>
    /// <returns>未登场武将的数量</returns>
    public static short GetTotalNoDebutGeneralNum()
    {
        // 计算未登场武将的数量
        int count = 0;
        for (int i = 0; i < noDebutGeneralList.Count; i++)
        {
            General general = noDebutGeneralList[i];
            // 如果对象不为空，则计数加一
            if (general != null) count++;
        }
        // 返回未登场武将的数量
        return (short)count;
    }

    /// <summary>
    /// 获取已出场将领
    /// </summary>
    /// <returns></returns>
    public static short GetDebtedGeneralNum()
    {
        int count = 0; // 初始化计数器

        foreach (General general in generalList) // 遍历武将列表
        {
            if (general == null)
            {
                break; // 如果元素为空，则退出循环
            }
            count++; // 增加计数器
        }

        return (short)count; // 返回计数结果
    }

    /// <summary>
    /// 获取总的武将数量
    /// </summary>
    /// <returns>总的武将数量</returns>
    public static short GetTotalGeneralNum()
    {
        return (short)generalDictionary.Count; // 返回计数结果
    }

    /// <summary>
    /// 添加武将到缓存字典
    /// </summary>
    /// <param name="general">武将</param>
    public static void AddGeneral(General general)
    {
        if (general.generalId <= 0)
        {
            Debug.Log("要添加的武将ID小于0");
            general.generalId = 10000; // 如果ID小于0，则分配新的ID
        }
        
        if (general.generalId > maxGeneralId)
        {
            maxGeneralId = general.generalId; // 更新最大武将ID
        }
        
        generalDictionary.TryAdd(general.generalId, general); // 添加到武将字典中
    }
    
    public static bool RemoveGeneral(short generalId)
    {
        return generalDictionary.Remove(generalId); // 从武将字典中移除指定ID的武将
    }
    
    /// <summary>
    /// 添加未首秀的武将
    /// </summary>
    /// <param name="general">武将对象</param>
    public static void AddNoDebutGeneral(General general)
    {
        if (general.generalId > maxGeneralId)
        {
            maxGeneralId = general.generalId; // 更新最大武将ID
        }

        noDebutGeneralList.Add(general); // 添加到未首秀武将列表
    }

    /// <summary>
    /// 添加武将
    /// </summary>
    /// <param name="general">武将对象</param>
    public static void AddDebutedGeneral(General general)
    {
        if (general.generalId == -1)
        {
            general.generalId = (short)(maxGeneralId + 1); // 如果ID为0，则分配新的ID
        }

        if (general.generalId > maxGeneralId)
        {
            maxGeneralId = general.generalId; // 更新最大武将ID
        }

        if (general.IsDie == false)
        {
            generalList.Add(general); // 添加到武将列表
        }
    }



    // 当武将死亡时调用此方法
    public static void GeneralDie(short generalId)
    {
        // 获取指定ID的武将对象
        General general = GetGeneral(generalId);

        // 如果武将不存在，则返回
        if (general == null)
            return;

        // 设置当前选择的武将名称
        GameInfo.chooseGeneralName = general.generalName;

        // 获取武将所在的城市ID
        byte cityId = general.debutCity;

        // 通过城市ID获取城市对象
        City city = CityListCache.GetCityByCityId(cityId);

        // 移除城市中的官员列表中的武将ID
        city.RemoveOfficerId(generalId);

        // 移除城市中的未发现武将列表中的武将ID
        city.RemoveNotFoundGeneralId(generalId);

        // 移除城市中的储备武将列表中的武将ID
        city.RemoveReservedGeneralId(generalId);

        // 将武将是否在职设置为0（假定0表示不在职）
        general.isOffice = 0;

        // 通过君主ID获取势力对象
        Country country = CountryListCache.GetCountryByKingId(generalId);

        // 如果势力存在
        if (country != null)
        {
            // 输出君主死亡的消息
            Debug.Log("君主：" + $"{general.generalName}" + "死亡了!!");

            // 如果是玩家控制的势力
            if (GameInfo.playerCountryId == country.countryId)
            {
                // 设置显示信息为君主死亡
                GameInfo.ShowInfo = general.generalName + " 君主死亡!";

                // 如果势力还有城市
                if (!country.IsDestroyed())
                {
                    // 设置势力灭亡提示为玩家继承
                    GameInfo.countryDieTips = 2;
                }
                else
                {
                    // 设置势力灭亡提示为玩家失败
                    GameInfo.countryDieTips = 3;

                    // 设置显示信息为势力灭亡
                    GameInfo.ShowInfo = general.generalName + " 势力灭亡了!";
                }
            }
            else
            {
                // 如果AI势力还有城市
                if (!country.IsDestroyed())
                {
                    // 如果只有一个城市且正在和玩家战争中
                    if (country.IsEndangered() && WarManager.Instance.curWarCityId == country.cityIDs[0])
                    {
                        // 设置势力灭亡提示为AI失败
                        GameInfo.countryDieTips = 4;

                        // 设置显示信息为势力灭亡
                        GameInfo.ShowInfo = general.generalName + " 势力灭亡了!";
                    }
                    else
                    {
                        // 设置势力灭亡提示为AI继承
                        GameInfo.countryDieTips = 1;

                        // 继承新的君主
                        short newKingGeneralId = country.Inherit();

                        // 设置显示信息为新君主继位
                        GameInfo.ShowInfo = general.generalName + "死亡,新君主 " + GetGeneral(newKingGeneralId).generalName + " 继位!";
                    }
                }
                else
                {
                    // 设置势力灭亡提示为AI失败
                    GameInfo.countryDieTips = 4;

                    // 设置显示信息为势力灭亡
                    GameInfo.ShowInfo = general.generalName + " 势力灭亡了!";
                }
            }
        }
        else
        {
            // 如果不是君主，则输出普通武将死亡的消息
            Debug.Log("武将：" + $"{general.generalName}" + "死亡了!!");
        }
        general.IsDie = true;
    }

    

    // 增加玩家与AI对战时武将的等级经验
    public static void AddExp_P(General atkGen, General defGen, int totalExp)
    {
        // 计算经验比例
        float ratio = (float)defGen.GetWarValue() / atkGen.GetWarValue();
        ratio = Mathf.Clamp(ratio, 0.5f, 1.5f);
        // 计算可获取的经验值
        short exp = (short)(int)(totalExp * ratio);
        atkGen.Addexperience(exp / 3); // 增加经验
    }
    
    // AI之间对战时增加经验
    public static void AIWarAddEXP2(General atkGen, General defGen, int totalExp)
    {
        // 计算经验百分比，取值范围在0.5到1.5之间
        float ratio = (float)defGen.GetWarValue() / atkGen.GetWarValue();
        ratio = Mathf.Clamp(ratio, 0.5F, 1.5F);
        // 计算可获得的总经验
        int exp = (int)(totalExp * ratio + 1.0F);
        var IQExp = 0;
        var leadExp = 0;

        // 如果攻击将领的智力比防守将领高出20，智力经验加成
        if (atkGen.IQ > defGen.IQ + 20)
            IQExp = (int)(exp * 0.3F);

        // 剩余经验分配给领导力
        leadExp = (short)(exp - IQExp);

        // 增加将领的智力和领导力经验
        atkGen.AddIqExp(IQExp / 100);
        atkGen.AddLeadExp(leadExp / 300);
    }
    
    /// <summary>
    /// 根据武将ID技能判定
    /// </summary>
    /// <param name="genId"></param>
    /// <param name="skillID"></param>
    /// <returns></returns>
    public static bool GetSkill_1(short genId, int skillID)
    {
        return GetSkill(0, genId, skillID);
    }

    public static bool GetSkill_2(short genId, int skillID)
    {
        return GetSkill(1, genId, skillID);
    }

    public static bool GetSkill_3(short genId, int skillId)
    {
        return GetSkill(2, genId, skillId);
    }

    public static bool GetSkill_4(short genId, int skillID)
    {
        return GetSkill(3, genId, skillID);
    }

    public static bool GetSkill_5(short genId, int skillId)
    {
        return GetSkill(4, genId, skillId);
    }

    public static bool GetSkill(int i, short genId, int skillId)
    {
        General general = GeneralListCache.GetGeneral(genId);
        return ((general.skills[i] >> 10 - skillId & 0x1) == 1);
    }



    public static int GetdPhase(short ph1, short ph2)
    {
        int total = Math.Abs(ph1 - ph2);  // 计算绝对差值
        if (total >= 75)
            total = 150 - total;  // 超过75则取反方向相性差值
        return total;
    }

    /// <summary>
    /// 获取将领的相性差值
    /// </summary>
    /// <param name="doGen">将领1</param>
    /// <param name="beGen">将领2</param>
    /// <returns></returns>
    public static int GetPhaseDifference(General doGen, General beGen)
    {
        int diff = Mathf.Abs(doGen.phase - beGen.phase); // 获取进攻将领对象
        if (diff >= 75) diff = 150 - diff; // 超过75则取反方向相性差值
        return diff;
        
    }
    
    /// <summary>
    /// 获取将领所属的君主ID
    /// </summary>
    /// <param name="general"></param>
    /// <returns></returns>
    static short GetOfficeGenBelongKing(General general)
    {
        short kingId = 0;
        City debutCity = CityListCache.GetCityByCityId(general.debutCity); // 获取将领初次登场的城市
        short[] officerIds = debutCity.GetOfficerIds(); // 获取城市的任职将领ID数组

        // 遍历城市的任职将领ID数组，查找将领是否在该城市任职
        foreach (var id in officerIds)
        {
            if (id == general.generalId)
            {
                return debutCity.cityBelongKing; // 返回该城市所属的君主ID
            }
        }

        int inCount = 0; // 计数器
        string cityInfoString = ""; // 用于记录将领所在城市信息

        // 遍历所有城市，查找将领是否在非出场城市任职
        foreach (var city in CityListCache.cityDictionary.Values)
        {
            short[] officerIds2 = city.GetOfficerIds(); // 获取城市任职将领ID
            // 遍历城市的任职将领ID数组
            foreach (var id in officerIds2)
            {
                if (id == general.generalId)
                {
                    kingId = city.cityBelongKing; // 找到该城市所属的君主ID
                    inCount++; // 计数
                    cityInfoString += city.cityName; // 记录城市名称

                    // 如果将领初次登场城市与当前城市不一致，则更新信息
                    if (general.debutCity != city.cityId && inCount > 1)
                    {
                        city.RemoveOfficerId(general.generalId); // 从旧城市移除将领任职信息
                        general.debutCity = city.cityId; // 更新将领的初次登场城市
                    }
                }
            }
        }

        // 如果计数器大于0，输出将领的任职城市信息
        if (inCount > 0)
        {
            Debug.Log($"{general.generalName}" + "在" + cityInfoString + "任职！"); // 使用Unity的Debug.Log输出信息
        }

        return kingId; // 返回君主ID
    }
    

    /// <summary>
    /// 检查离间是否成功
    /// </summary>
    /// <param name="doGenId"></param>
    /// <param name="beGenId"></param>
    /// <returns></returns>
    public static bool IsAlienate(short doGenId, short beGenId)
    {
        General doGeneral = GetGeneral(doGenId);
        General beGeneral = GetGeneral(beGenId);
        int i =0;
        if (beGeneral.GetLoyalty() > 95) return false;
        if (beGeneral.GetLoyalty() > 87)
        {
            if (Random.Range(0, 100) > (95 - beGeneral.GetLoyalty()) * 2)
                return false;
        }
        else
        {
            i = (95 - beGeneral.GetLoyalty()) * (95 - beGeneral.GetLoyalty()) / 4;
            if (i > 90) i = 90;
            if (Random.Range(0, 100) > i)
                return false;
        }
        int d1 = GetPhaseDifference(GetGeneral(GetOfficeGenBelongKing(beGeneral)), beGeneral);
        int d2 = GetPhaseDifference(GetGeneral(GetOfficeGenBelongKing(doGeneral)), beGeneral);
        int d3 = GetPhaseDifference(doGeneral, beGeneral);
        if (d1 == 0)
            return false;
        if (d2 == 0)
            i = 10 + Random.Range(0, 15);
        if (d3 > d1 + 10)
            return false;
        int val = d1 - d2 * 4 / 3 - d3 * 2 + 110 - beGeneral.GetLoyalty();
        if (val > 0)
        {
            if (Random.Range(0, val) > 5)
                i = 5 + Random.Range(0, 10);
            if (Random.Range(0, 40) < 100 - beGeneral.GetLoyalty())
            {
                i = 2 + Random.Range(0, 5);
            }
            else
            {
                i = 1 + Random.Range(0, 3);
            }    
            
        }

        if (Random.Range(0, 120) < doGeneral.IQ - beGeneral.IQ)
        {
            i = 1 + Random.Range(0, 3); 
        }
        else
        {
            return false;
        }    
        if (i > 0)
        {
            Debug.Log($"{doGeneral.generalName}离间了{beGeneral.generalName}忠诚度降低:{i}");
            beGeneral.DecreaseLoyalty((byte)i); // 降低忠诚度
            doGeneral.AddMoralExp(Random.Range(5, 15)); // 增加将领的道德经验
            doGeneral.AddIqExp(Random.Range(2, 10)); // 增加将领的智力经验
            return true;
        }
        return false;
    }


    /// <summary>
    /// 检查将领是否可以被笼络
    /// </summary>
    /// <param name="doGeneral"></param>
    /// <param name="beGeneral"></param>
    /// <returns></returns>
    private static bool CanBribe(General doGeneral, General beGeneral)
    {
        if (beGeneral.GetLoyalty() >= 95)
            return false;
        int d1 = GetPhaseDifference(GetGeneral(GetOfficeGenBelongKing(doGeneral)), beGeneral);
        int d2 = GetPhaseDifference(GetGeneral(GetOfficeGenBelongKing(beGeneral)), beGeneral);
        int d3 = GetPhaseDifference(doGeneral, beGeneral);
        if (d2 == 0)
            return false;
        if (d2 < 5)
        {
            int val = d2 - d1 * 2 - d3 * 2 + (100 - beGeneral.GetLoyalty()) / 2;
            if (val > 0)
                return true;
        }
        else if (d2 <= 10)
        {
            int val = d2 - d1 * 3 / 2 - d3 * 2 + (100 - beGeneral.GetLoyalty()) / 2;
            if (val > 0)
                return true;
        }
        else if (d2 <= 20)
        {
            int val = d2 - d1 - d3 * 2 + (100 - beGeneral.GetLoyalty()) / 2;
            if (val > 0)
                return true;
        }
        else
        {
            int val = d2 - d1 - d3 * 2 + (100 - beGeneral.GetLoyalty()) / 3;
            if (val > 0)
                return true;
        }
        return false;
    }
    

    /// <summary>
    /// 尝试笼络将领从一个城市转移到另一个城市
    /// </summary>
    /// <param name="doCityId">说客城池ID</param>
    /// <param name="beCityId">被招揽城池ID</param>
    /// <param name="doGenId">说客ID</param>
    /// <param name="beGenId">被招揽者ID</param>
    /// <returns></returns>
    public static bool IsBribe(byte doCityId, byte beCityId, short doGenId, short beGenId)
    {
        General doGeneral = GetGeneral(doGenId); // 获取说客
        General beGeneral = GetGeneral(beGenId); // 获取被招揽者
        City doCity = CityListCache.GetCityByCityId(doCityId); // 获取说客所在城市
        City beCity = CityListCache.GetCityByCityId(beCityId); // 获取被招揽者所在城市
        if (CanBribe(doGeneral, beGeneral))
        {
            beGeneral.SetTraitorLoyalty(); // 随机设置将领忠诚度
            doGeneral.AddMoralExp(Random.Range(10, 25)); // 增加将领的道德经验
            doGeneral.AddIqExp(Random.Range(4, 10)); // 增加将领的智力经验
            beCity.RemoveOfficerId(beGenId); // 从城市中移除将领
            doCity.AddOfficeGeneralId(beGenId); // 将将领添加到另一个城市
            Debug.Log($"{beCity.cityName}的{doGeneral.generalName}招揽了{doCity.cityName}的{beGeneral.generalName}");
            return true;
        }
        return false;
    }



    /// <summary>
    /// 雇佣判定操作
    /// </summary>
    /// <param name="gohireId"></param>
    /// <param name="behireId"></param>
    /// <returns></returns>
    public bool EmployProbability(short gohireId, short behireId)
    {
        General goGeneral = GeneralListCache.GetGeneral(gohireId);  // 获取雇佣将领
        City city = CityListCache.GetCityByCityId(goGeneral.debutCity);  // 获取该将领所在城市
        General kingGeneral = GeneralListCache.GetGeneral(city.cityBelongKing);  // 获取该城市所属的国王
        General beGeneral = GeneralListCache.GetGeneral(behireId);  // 获取被雇佣的将领

        // 如果被雇佣的将领不存在，返回false
        if (beGeneral == null)
            return false;

        int d1 = GetdPhase(kingGeneral.phase, beGeneral.phase);  // 计算国王与被雇佣将领的相位差
        int d2 = GetdPhase(goGeneral.phase, beGeneral.phase);  // 计算雇佣将领与被雇佣将领的相位差
        int i = UnityEngine.Random.Range(0, 75);  // 获取随机数用于雇佣成功判定

        // 如果相位差满足雇佣条件，雇佣成功
        if ((d1 + d2) / 2 < i)
        {
            Console.WriteLine($"{kingGeneral.generalName} 势力成功雇佣 {beGeneral.generalName}！");
            return true;
        }

        return false;  // 雇佣失败
    }
}



