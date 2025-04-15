using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using BaseClass;
using Newtonsoft.Json;
using static DataClass.GameInfo;


// City 类假设用于表示城市的各种属性
[System.Serializable]
public class CityListCache : MonoBehaviour
{

    // 缓存字典，用于存储城市数据
    public static Dictionary<byte, City> cityDictionary = new Dictionary<byte, City>();
    // 定义城市数量的常量
    public static byte CITY_NUM = 49;
    

    // 清空所有城市列表
    public static void ClearAllCities()
    {
        cityDictionary.Clear();
    }

    // 根据城市ID获取城市对象
    public static City GetCityByCityId(byte id)
    {
        if (cityDictionary.TryGetValue(id, out var cityId))
        {
            return cityId;
        }
        Debug.LogError("获取城池错误, 城池Id: " + id);
        return null;
    }

    // 添加城市到列表
    public static void AddCity(City city)
    {
        cityDictionary.TryAdd(city.cityId, city);
    }

    // 获取城市数量
    public static byte GetCityNum()
    {
        return (byte)cityDictionary.Count;
    }


    // 执行移动方法并根据是否包含君主
    public static bool MoveTask(byte fromCityId, byte toCityId, List<short> generalIds)
    {
        bool isKingMove = false;
        // 获取起始城市和目标城市
        City fromCity = GetCityByCityId(fromCityId);
        City toCity = GetCityByCityId(toCityId);

        foreach (short generalId in generalIds)
        {
            if (generalId == fromCity.cityBelongKing)
            {
                fromCity.RemoveOfficerId(generalId);
                toCity.AddOfficeGeneralId(generalId);
                toCity.AppointmentPrefect(generalId);
                isKingMove = true;
                Debug.Log("将君主移动到" + toCity.cityName);
            }
            else
            {
                fromCity.RemoveOfficerId(generalId);
                toCity.AddOfficeGeneralId(generalId);
                Debug.Log("将武将移动到" + toCity.cityName);
            }
        }

        return isKingMove;
    }


    // 执行输送调整城池方法
    public static void TransportBetweenCitys(byte outcityId, byte incityId,short food,short money,byte treasureNum)
    {
        City outCity = GetCityByCityId(outcityId);
        City inCity = GetCityByCityId(incityId);
        outCity.SubGold(money); // 从目标城市减少金钱
        outCity.SubFood(food); // 从目标城市减少粮食
        outCity.treasureNum = (byte)(GetCityByCityId(outcityId).treasureNum - treasureNum); // 更新目标城市的宝物数量
        inCity.AddGold(money); // 向当前城市添加金钱
        inCity.AddFood(food); // 向当前城市添加粮食
        inCity.treasureNum = (byte)(GetCityByCityId(incityId).treasureNum + treasureNum); // 更新当前城市的宝物数量
    }
}

