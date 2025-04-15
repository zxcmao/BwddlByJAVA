// 保存数据的类，用于处理字节数组的动态扩展和数据操作

using System.Collections.Generic;
using BaseClass;
using Newtonsoft.Json;

namespace DataClass
{
    [System.Serializable]
    public class SaveData
    {
        public string recordInfo;
        public byte playerCountryId;
        public byte doCityId;
        public byte playerOrderNum;
        public byte month;
        public short years;
        public byte difficult;
        public byte attackCount;
        [JsonProperty("countrySequence[]")]public List<byte> countrySequence = new List<byte>();
        [JsonProperty("generalList[]")] public List<General> generalList;
        [JsonProperty("cityList[]")] public List<City> cityList;
        [JsonProperty("countryList[]")] public List<Country> countryList;
    }
}
/*
[System.Serializable]
public class GeneralData
{
    // 将领的属性
    public short generalId;               // 将领的ID
    public string generalName;            // 将领的名字
    public byte lead;                     // 领导能力
    public byte political;                // 政治能力
    public short phase;                   // 阶段
    public byte isOffice;                 // 是否在任
    public byte level;                    // 等级
    public byte[] army;                   // 军队数组，假设长度为3
    public byte force;                    // 势力
    public short generalSoldier;          // 将领士兵数量
    public byte moral;                    // 道德值
    public byte curPhysical;              // 当前体力
    public byte maxPhysical;              // 最大体力
    public byte IQ;                       // 智商
    public short debutYears;              // 出道年数
    public byte loyalty;                  // 忠诚度
    public byte debutCity;                // 出道城市
    public short followGeneralId;         // 追随将领的ID
    public int experience;                // 经验值
    public byte weapon;                   // 武器
    public byte armor;                    // 盔甲
    public short[] skills;                // 技能数组，假设长度为5
    public byte leadExp;                  // 领导经验
    public byte forceExp;                 // 势力经验
    public byte IQExp;                    // 智商经验
    public byte moralExp;                 // 道德经验
    public byte politicalExp;             // 政治经验
    public bool IsDie;                    // 是否死亡

    // 构造函数，从 General 对象初始化
    public GeneralData(General general)
    {
        this.generalId = general.generalId;
        this.generalName = general.generalName;
        this.lead = general.lead;
        this.political = general.political;
        this.phase = general.phase;
        this.isOffice = general.isOffice;
        this.level = general.level;
        this.army =  general.army;
        this.force = general.force;
        this.generalSoldier = general.generalSoldier;
        this.moral = general.moral;
        this.curPhysical = general.curPhysical; 
        this.maxPhysical = general.maxPhysical;
        this.IQ = general.IQ;
        this.debutYears = general.debutYears;
        this.loyalty = general.loyalty; 
        this.debutCity = general.debutCity;
        this.followGeneralId = general.followGeneralId;
        this.experience = general.experience;
        this.weapon = general.weapon;
        this.armor = general.armor;
        this.skills = general.skills;
        this.leadExp = general.leadExp;
        this.forceExp = general.forceExp;
        this.IQExp = general.IQExp;
        this.moralExp = general.moralExp;
        this.politicalExp = general.politicalExp;
        this.IsDie = general.IsDie;
    }
}


[System.Serializable]
public class CityData
{
    // 城市的属性
    public byte cityId;                                     // 城市ID
    public string cityName;                                 // 城市名称
    public short cityBelongKing;                            // 所属国王ID
    public short prefectId;                                 // 太守ID
    public byte rule;                                       // 统治等级
    public short money;                                     // 货币数量
    public short food;                                      // 食物数量
    public short agro;                                      // 农业
    public short trade;                                     // 贸易
    public int population;                                  // 人口
    public byte floodControl;                               // 防洪等级
    public int cityTotalSoldier;                            // 城市总士兵数
    public int cityReserveSoldier;                          // 城市预备士兵数
    public bool cityGrainShop;                              // 是否有粮店
    public bool citySchool;                                 // 是否有学校
    public bool cityHospital;                               // 是否有医院
    public byte treasureNum;                                 // 宝藏数量
    public byte cityWeaponShop;                             // 是否有武器商店等级
    public byte warWeaponShop;                              // 战场武器商店等级
    public short[] connectCityId;                           // 连接的城市ID
    public short[] mapPosition=new short[2];                // 地图坐标

    // 构造函数，从 City 对象初始化
    public CityData(City city)
    {
        this.cityId = city.cityId;                          // 城市ID
        this.cityName = city.cityName;                      // 城市名称
        this.cityBelongKing = city.cityBelongKing;          // 所属国王ID
        this.prefectId = city.prefectId;                    // 太守ID
        this.rule = city.rule;                              // 统治等级
        this.money = city.money;                            // 货币数量
        this.food = city.food;                              // 食物数量
        this.agro = city.agro;                              // 农业
        this.trade = city.trade;                            // 贸易
        this.population = city.population;                  // 人口
        this.floodControl = city.floodControl;              // 防洪等级
        this.cityTotalSoldier = city.cityTotalSoldier;      // 城市总士兵数
        this.cityReserveSoldier = city.cityReserveSoldier;  // 城市预备士兵数;
        this.cityGrainShop = city.cityGrainShop;              // 是否有粮店
        this.citySchool = city.citySchool;                  // 是否有学校
        this.cityHospital = city.cityHospital;              // 是否有医院
        this.treasureNum = city.treasureNum;                // 宝藏数量
        this.cityWeaponShop = city.cityWeaponShop;          // 是否有武器商店等级
        this.warWeaponShop = city.warWeaponShop;          // 战场武器商店等级
        this.connectCityId = city.connectCityId;            // 连接的城市ID
        this.mapPosition = city.mapPosition;                // 地图坐标
    }
}


[System.Serializable]
public class CountryData
{
    // 国家属性
    public byte countryId;                // 国家ID
    public short countryKingId;           // 国王ID
    public string countryColor;           // 国家颜色（十六进制字符串）
    public short[] inheritGeneralIds;     // 继承将领ID数组
    public byte isTurns;                  // 是否轮到这个国家
    public byte haveCityNum;              // 拥有城市数量
    public List<byte> cityIDs;             // 拥有城市ID列表
    public List<Alliance> allianceList;      // 同盟信息数组

    // 构造函数，从 Country 对象初始化
    public CountryData(Country country)
    {
        this.countryId = country.countryId;
        this.countryKingId = country.countryKingId;
        this.countryColor = country.countryColor;
        this.inheritGeneralIds = country.inheritGeneralIds;
        this.isTurns = country.isTurns;
        this.haveCityNum = country.haveCityNum;
        this.cityIDs=country.cityIDs;
        this.allianceList = country.allianceList; // 初始化同盟数组
    }
}

// 假设 AllianceData 类如下
[System.Serializable]
public class AllianceData
{
    public byte countryId;   // 同盟国家ID
    public int months;       // 维持月份

    // 构造函数
    public AllianceData(Alliance alliance)
    {
        this.countryId = alliance.countryId;
        this.months = alliance.Months; // 假设有属性获取维持月份
    }
}
*/
