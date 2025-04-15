/*using System;
using System.Collections;
using System.Collections.Generic;
using BaseClass;
using UnityEngine;
using UnityEngine.Android;
using static GameInfo;

public class AIStateMachine
{
    private AIState currentState;
    private byte curTurnsCountryId;
    private byte AIUseOrderNum = 0;//当前AI消耗的命令数
    private byte orderNum;//当前AI总命令数
    private byte warNum = 2;
    private byte j_byte_fld;
    private short AIUseGeneralId;
    private short AITargetGeneralId;
    private byte AIUseCityId;
    private byte AITargetCityId;
    private byte eventId;
    private byte chooseGeneralNum;
    private short[] chooseGeneralIdArray;
    private short aiMoney_inWar;
    private short aiGrain_inWar;
    private byte loseCountryId;
    private short inheritGeneralId;

    private string kingName { get{ return King(); } set { kingName = value;} }
    public enum AIState
    {
        Idle,
        IsTurn,
        Alliance,
        MustShopping,
        TransportMoney,
        SearchGeneral,
        Conscript,
        Attack,
        Defence,
        Interior,
        Reward,
        Treat,
        Uprising
    }


    public AIStateMachine(byte countryId)
    {
        currentState = AIState.Idle;
        curTurnsCountryId = countryId;
    }

    /*public void Update()
    {
        switch (currentState)
        {
            case AIState.Idle:
                Idle();
                NextState(AIState.Alliance);
                break;
            case AIState.IsTurn:
                //TransitionToNextState();
                break;
            case AIState.Alliance:
                Alliance(curTurnsCountryId);
                break;
            case AIState.Interior:
                Interior();
                break;
            case AIState.SearchGeneral:
                SearchGeneral();
                break;
            case AIState.MustShopping:
                MustShopping();
                break;
            case AIState.Conscript:
                Conscript();
                break;
            case AIState.TransportMoney:
                TransportMoney();
                break;
            case AIState.Attack:
                Attack();
                break;
            case AIState.Treat:
                Treat();
                break;
            case AIState.Uprising:
                Uprising();
                break;
            default:
                throw new NotImplementedException();
        }
    }#1#

  


    private string King()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        short kingId = country.countryKingId;
        General king = GeneralListCache.GetGeneral(kingId);
        string KingName = king.generalName;
        return (KingName);
    }



    public IEnumerator AiDoOeder(byte curTurnsCountryId)
    {
        orderNum = CountryListCache.GetAIOredrNum(curTurnsCountryId);
        while (AIUseOrderNum < orderNum)  // 当AI未执行完所有指令时
        {
            AIUseOrderNum++;
            GC.Collect();
            yield return AiAlliance();
            AiInterior();
            AiSearch();
            AiMustShopping();
            AiConscription();
            AiDefence();
            AiTransportMoney();
            if (AiFindLowLoyaltyGeneral())
            {
                AiReward();
            }
            AiTreat();
            if (AIUseOrderNum < orderNum && warNum > 0)  // 如果AI还有指令未执行且还有战争次数
            {
                yield return AiAttack();
                AiConscription();  // AI进行额外的征兵
                AiTreat();  // 执行战后治疗操作
            }
            AiStudy();  // 进行学习操作
            
            if (AiFindLowLoyaltyGeneral())  // 如果AI状态为8
            {
                AiReward();  // 执行特定操作
            }
            //AiInterior(curTurnsCountryId);  // 继续进行内政处理
            Debug.Log($"{kingName}执行第{AIUseOrderNum}次命令");

        }
        yield return AiUprising();
    } 

    private IEnumerator AiAlliance()
    {
        Debug.Log($"{kingName}正分析天下形势");
        // 获取指定ID的国家
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        // 获取该国家拥有的城市数量
        byte CityNum = country.GetHaveCityNum();
        // 如果城市数量大于3，则无需结盟
        if (CityNum > 3)
            yield break;

        // 获取该国家拥有的所有城市ID
        byte[] cityIds = country.GetCities();
        int qz = 0;  // 记录是否有城市与其他国家结盟

        // 遍历所有城市
        for (int i = 0; i < cityIds.Length; i++)
        {
            byte cityId = cityIds[i];
            // 获取城市对象
            City city = CityListCache.GetCityByCityId(cityId);
            // 获取该城市连接的城市ID
            short[] connectCityId = city.connectCityId;

            // 遍历连接的城市
            for (int m = 0; m < connectCityId.Length; m++)
            {
                // 获取连接城市的所属国王ID
                short kingId = (CityListCache.GetCityByCityId((byte)connectCityId[m])).cityBelongKing;
                // 获取该国王所属的国家
                Country otherCountry = CountryListCache.GetCountryByKingId(kingId);

                // 如果其他国家存在且其ID为人类国家ID
                if (otherCountry != null && otherCountry.countryId == GameInfo.playerCountryId)
                {
                    qz++;  // 增加结盟计数
                    break;  // 跳出循环
                }
            }
        }

        // 如果没有找到可结盟的国家且随机值大于10，则无需结盟
        if (qz == 0 && UnityEngine.Random.Range(0, 100) > 10)
            yield break;

        // 计算结盟阈值
        byte gl = (byte)(8 - Math.Pow(2, CityNum - 1) + CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).GetHaveCityNum());
        int k = UnityEngine.Random.Range(0, 100);
        gl = (byte)(gl + qz);

        // 如果结盟阈值小于随机值，则无需结盟
        if (gl < k)
            yield break;

        // 获取没有结盟的国家ID数组
        byte[] noAllianceCountryId = country.GetNoCountryIdAllianceCountryIdArray();
        // 如果没有没有结盟的国家，则返回
        if (noAllianceCountryId.Length < 1)
            yield break;

        Country allianceCountry = null;  // 记录要结盟的国家
        int dPhase = 0;  // 记录最小的相位差
                         // 获取当前国家国王
        General countryKing = GeneralListCache.GetGeneral(country.countryKingId);

        // 遍历没有结盟的国家
        for (int j = 0; j < noAllianceCountryId.Length; j++)
        {
            byte otherCountryId = noAllianceCountryId[j];
            // 获取其他国家
            Country otherCountry = CountryListCache.GetCountryByCountryId(otherCountryId);
            if (otherCountry != null && otherCountryId != GameInfo.playerCountryId)
            {
                // 获取其他国家国王
                General otherCountryKing = GeneralListCache.GetGeneral(otherCountry.countryKingId);
                if (otherCountryKing != null)
                {
                    // 计算当前国家国王和其他国家国王的相位差
                    int d1 = GeneralListCache.GetdPhase(countryKing.phase, otherCountryKing.phase);
                    if (dPhase == 0 || dPhase > d1)
                    {
                        dPhase = d1;  // 更新最小的相位差
                        allianceCountry = otherCountry;  // 记录当前最适合结盟的国家
                    }
                }
            }
        }

        // 如果没有找到适合结盟的国家，则返回
        if (allianceCountry == null)
            yield break;

        // 如果相位差加上被结盟国家的城市数小于随机值
        if (dPhase + allianceCountry.GetHaveCityNum() <= UnityEngine.Random.Range(0, 75))
        {
            // 执行结盟操作
            country.AddAlliance(allianceCountry.countryId);
            // 输出结盟成功的日志
            Debug.Log(countryKing.generalName + "势力与" + (GeneralListCache.GetGeneral(allianceCountry.countryKingId)).generalName + "势力同盟成功！");
            // 更新游戏信息显示
            string text = countryKing.generalName + "势力与" + (GeneralListCache.GetGeneral(allianceCountry.countryKingId)).generalName + "达成同盟！";
            TurnManager.PlayingState = GameState.AITruce;
            yield return UITurnTips.GetInstance().ShowTurnTips(text);

            Debug.Log(countryKing.generalName +"势力与" + (GeneralListCache.GetGeneral(allianceCountry.countryKingId)).generalName + "达成同盟！");
        }
    }

    //AI执行内政操作
    private void AiInterior()
    {
        Debug.Log($"{kingName}正在治理城池");
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        byte CITY_NUM = country.GetHaveCityNum();
        // 随机选择一个城市进行内政计算
        int index = UnityEngine.Random.Range(0,CITY_NUM);
        byte cityId = country.getCity(index);
        AiCalculateInterior(cityId);
    }

    void AiCalculateInterior(byte cityId)
    {
        int i = UnityEngine.Random.Range(0, 13);
        short generalId = AiFindMostIQPoliticalGeneralInCity(cityId);
        City city = CityListCache.GetCityByCityId(cityId);

        // 城市资金不足时，优先选择特定策略
        if (city.GetMoney() < 50)
            i = 7;

        // 根据随机结果选择内政行动
        switch (i)
        {
            case 0:
                if (city.floodControl < 99)
                    AiTameOrder(cityId, generalId);
                break;
            case 1:
                if (city.agro < 999)
                    AiReclaimOrder(cityId, generalId);
                break;
            case 2:
                if (city.trade < 999)
                    AiMercantileOrder(cityId, generalId);
                break;
            case 3:
            case 4:
                if (city.population < 990000)
                {
                    generalId = AIgetGenToXC(cityId);
                    AiPatrolOrder(cityId, generalId);
                }
                break;
            case 5:
            case 6:
                AiJudgeBribe(); // 执行特定行为
                break;
            case 7:
            case 8:
                AiDoSearchEmploy(cityId); // 执行招揽行动
                break;
        }
    }

    /// <summary>
    /// AI获取城市中智力和政治综合能力最高的将领ID
    /// </summary>
    /// <param name="cityId"></param>
    /// <returns></returns>
    static short AiFindMostIQPoliticalGeneralInCity(byte cityId)
    {
        City city = CityListCache.GetCityByCityId(cityId);
        short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray();
        short generalId = officeGeneralIdArray[0];
        General general = GeneralListCache.GetGeneral(generalId);

        // 计算综合能力值 (智力 + 政治 * 2) / 3
        short byte1 = (short)((general.IQ + general.political * 2) / 3);

        // 遍历将领数组，找出综合能力值最高的将领
        for (byte byte2 = 1; byte2 < city.getCityGeneralNum(); byte2++)
        {
            General curGeneral = GeneralListCache.GetGeneral(officeGeneralIdArray[byte2]);
            short curVal = (short)((curGeneral.IQ + curGeneral.political * 2) / 3);
            if (byte1 < curVal)
            {
                generalId = officeGeneralIdArray[byte2];
                byte1 = curVal;
                // 如果将领具备某些特技王佐屯田商才，能力值加倍
                if (curGeneral.HasSkill(3, 0) || curGeneral.HasSkill(3, 2) || curGeneral.HasSkill(3, 3))
                    byte1 = (short)(byte1 * 2);
            }
        }
        return generalId;
    }

    // AI获取最佳治理将领
    static short AIgetGenToXC(byte city)
    {
        short id = CityListCache.GetCityByCityId(city).GetCityOfficeGeneralIdArray()[0];
        General general = GeneralListCache.GetGeneral(id);

        // 计算初始综合能力值 (智力 + 政治 * 2 + 德望 * 2) / 5
        int val = (general.IQ + general.political * 2 + general.moral * 2) / 5;

        // 遍历将领数组，找出综合能力值最高的将领
        for (byte index = 0; index < CityListCache.GetCityByCityId(city).getCityGeneralNum(); index++)
        {
            short curId = CityListCache.GetCityByCityId(city).GetCityOfficeGeneralIdArray()[index];
            General curGeneral = GeneralListCache.GetGeneral(curId);
            int curVal = (curGeneral.IQ + curGeneral.political * 2 + curGeneral.moral * 2) / 5;

            // 如果将领具备某些特技，能力值加倍
            if (curGeneral.HasSkill(3, 0) || curGeneral.HasSkill(3, 1))
                curVal *= 2;
            if (curVal > val)
            {
                id = curId;
                val = curVal;
            }
        }
        return id;
    }




    /// <summary>
    /// 自动重建AI灾后所有城市的内政
    /// </summary>
    public static void AIRebuildCity()
    {
        for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
        {
            City city = CityListCache.GetCityByCityId(cityId);

            // 如果城市的国王ID大于0且不属于玩家国家的国王
            if (city.cityBelongKing > 0 && city.cityBelongKing != (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId)).countryKingId)
            {
                byte curCountryCitys = CountryListCache.GetCountryByKingId(city.cityBelongKing).GetHaveCityNum();

                // 如果当前国家的城市数量小于等于10
                if (curCountryCitys <= 10)
                {
                    // 如果城市的资金少于600，则增加一定的资金
                    if (city.GetMoney() < 600)
                        city.AddMoney((short)(30 + UnityEngine.Random.Range(0, 6000 / curCountryCitys)));

                    // 如果城市的食物少于城市所有士兵数量的 6 * 60 倍
                    if (city.GetFood() < city.GetCityAllSoldierNum() / 1000 * 6 * 60)
                        city.SetFood((short)(150 + UnityEngine.Random.Range(0, 6000 / curCountryCitys)));
                }
                else if (city.GetMoney() < 30)
                {
                    // 如果城市的资金少于30，则增加一定的资金
                    city.AddMoney((short)(UnityEngine.Random.Range(20, 30)));
                }

                byte byte1 = 100; // 初始最小忠诚度
                short word0 = 0;  // 初始忠诚度最低的将军ID
                short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray();

                // 遍历城市中所有将军
                for (byte byte3 = 0; byte3 < city.getCityGeneralNum(); byte3 = (byte)(byte3 + 1))
                {
                    short generalId = officeGeneralIdArray[byte3];
                    General general = GeneralListCache.GetGeneral(generalId);

                    // 如果将军的忠诚度低于50，则提升忠诚度
                    if (general.getLoyalty() < 50)
                    {
                        general.AddLoyalty(true);
                    }
                    // 如果当前将军的忠诚度低于最小忠诚度，则更新最小忠诚度
                    else if (byte1 > general.getLoyalty())
                    {
                        word0 = generalId;
                        byte1 = general.getLoyalty();
                    }
                }
                // 如果最小忠诚度低于90，则提升对应将军的忠诚度
                if (byte1 < 90)
                    GeneralListCache.GetGeneral(word0).AddLoyalty(true);

                // 如果城中有学院，则调用AIDoLearn方法
                if (city.citySchool)
                    city.StudyOfAllGeneral();

                // 如果城中有医馆，则对每个将军进行体力恢复
                if (city.cityHospital)
                    for (byte byte5 = 0; byte5 < city.getCityGeneralNum(); byte5 = (byte)(byte5 + 1))
                    {
                        short word4 = officeGeneralIdArray[byte5];
                        General general = GeneralListCache.GetGeneral(word4);
                        if (general.getCurPhysical() < general.maxPhysical)
                        {
                            byte addPhysical = (byte)(general.maxPhysical - general.getCurPhysical());
                            general.addCurPhysical(addPhysical);
                        }
                    }

                // 如果城市的资金大于等于30
                if (CityListCache.GetCityByCityId(cityId).GetMoney() >= 30)
                {
                    short generalId = AiFindMostIQPoliticalGeneralInCity(cityId); // 获取将军ID
                                                                                 // 如果城市的防洪控制小于99，则调用AiTameOrder方法
                    if ((CityListCache.GetCityByCityId(cityId)).floodControl < 99)
                        AiReclaimOrder(cityId, generalId);

                    // 如果城市的资金仍然大于等于30
                    if (CityListCache.GetCityByCityId(cityId).GetMoney() >= 30)
                    {
                        if (curCountryCitys > 10)
                        {
                            if (city.getCityGeneralNum() > 1)
                            {
                                if (UnityEngine.Random.Range(0, city.getCityGeneralNum()) > 0)
                                    AutoInteriorLogic(cityId); // 调用AutoInteriorLogic方法
                            }
                            else if (UnityEngine.Random.Range(0, 60) > curCountryCitys * 2)
                            {
                                AutoInteriorLogic(cityId); // 调用AutoInteriorLogic方法
                            }
                        }
                        else if (city.getCityGeneralNum() > 1)
                        {
                            if (UnityEngine.Random.Range(0, city.getCityGeneralNum()) > 0)
                                AutoInteriorLogic(cityId); // 调用AutoInteriorLogic方法
                        }
                        else if (UnityEngine.Random.Range(0, 3) > 0)
                        {
                            AutoInteriorLogic(cityId); // 调用AutoInteriorLogic方法
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 自动对AI所有城市进行内政处理
    /// </summary>
    public static void AutoInteriorAllCity()
    {
        for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
        {
            City city = CityListCache.GetCityByCityId(cityId);
            if (city.cityBelongKing > 0 && city.cityBelongKing != (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId)).countryKingId)
            {
                byte CITY_NUM = CountryListCache.GetCountryByKingId(city.cityBelongKing).GetHaveCityNum();
                if (CITY_NUM > 0 && CITY_NUM <= 6)
                {
                    if (city.GetMoney() < 600)
                        city.AddMoney((short)(30 + UnityEngine.Random.Range(0, 600 / CITY_NUM)));
                    if (city.GetFood() < city.GetCityAllSoldierNum() / 1000 * 6 * 30)
                        city.AddFood((short)(150 + UnityEngine.Random.Range(0, 600 / CITY_NUM)));
                }
                else if (city.GetMoney() < 30)
                {
                    city.AddMoney((short)UnityEngine.Random.Range(20, 30));
                }

                short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray();
                byte minLoyalty = 100;
                short word0 = 0;
                for (byte byte3 = 0; byte3 < city.getCityGeneralNum(); byte3 = (byte)(byte3 + 1))
                {
                    short generalId = officeGeneralIdArray[byte3];
                    General general = GeneralListCache.GetGeneral(generalId);
                    if (general.getLoyalty() < 50)
                    {
                        general.AddLoyalty(false);
                    }
                    else if (minLoyalty > general.getLoyalty())
                    {
                        word0 = generalId;
                        minLoyalty = general.getLoyalty();
                    }
                }
                if (minLoyalty < 90)
                {
                    General general = GeneralListCache.GetGeneral(word0);
                    general.AddLoyalty(false);
                }

                if (city.citySchool)
                    city.StudyOfAllGeneral();

                if (city.cityHospital)
                    for (byte id = 0; id < city.getCityGeneralNum(); id = (byte)(id + 1))
                    {
                        short generalId = officeGeneralIdArray[id];
                        General general2 = GeneralListCache.GetGeneral(generalId);
                        if (general2.getCurPhysical() < general2.maxPhysical)
                        {
                            byte addPhysical = (byte)(general2.maxPhysical- general2.getCurPhysical());
                            general2.addCurPhysical(addPhysical);
                        }
                    }

                if (city.GetMoney() >= 30)
                {
                    short generalId = AiFindMostIQPoliticalGeneralInCity(cityId); // 获取将军ID
                                                                                 // 如果城市的防洪控制小于99，则调用AiTameOrder方法
                    if ((CityListCache.GetCityByCityId(cityId)).floodControl < 99)
                        AiReclaimOrder(cityId, generalId);
                    if (city.GetMoney() >= 30)
                        if (CITY_NUM > 6)
                        {
                            if (city.getCityGeneralNum() > 1)
                            {
                                if (UnityEngine.Random.Range(0, city.getCityGeneralNum()) > 0)
                                    AutoInteriorLogic(cityId);
                            }
                            else if (UnityEngine.Random.Range(0, 40) > CITY_NUM * 2)
                            {
                                AutoInteriorLogic(cityId);
                            }
                        }
                        else if (city.getCityGeneralNum() > 1)
                        {
                            if (UnityEngine.Random.Range(0, city.getCityGeneralNum()) > 0)
                                AutoInteriorLogic(cityId);
                        }
                        else if (UnityEngine.Random.Range(0, 3) > 1)
                        {
                            AutoInteriorLogic(cityId);
                        }
                }
            }
        }
    }

    /// <summary>
    /// AI自动选择内政策略
    /// </summary>
    /// <param name="cityId"></param>
    static void AutoInteriorLogic(byte cityId)
    {
        short generalId = AiFindMostIQPoliticalGeneralInCity(cityId);
        City city = CityListCache.GetCityByCityId(cityId);

        // 优先进行治水
        if (city.floodControl < 99)
        {
            AiTameOrder(cityId, generalId);
            return;
        }

        // 决定优先发展农业或贸易
        if (UnityEngine.Random.Range(0, 3) > 1)
        {
            if (city.agro < 999 && city.trade < 999)
            {
                if (GameInfo.month < 4 || GameInfo.month >= 10)
                {
                    AiMercantileOrder(cityId, generalId);
                }
                else
                {
                    AiReclaimOrder(cityId, generalId);
                }
                return;
            }
            if (city.agro < 999)
            {
                AiReclaimOrder(cityId, generalId);
                return;
            }
            if (city.trade < 999)
            {
                AiMercantileOrder(cityId, generalId);
                return;
            }
        }

        // 人口未达上限则优先发展人口
        if (city.population < 990000)
        {
            generalId = AIgetGenToXC(cityId);
            AiPatrolOrder(cityId, generalId);
            return;
        }

        // 否则继续治水
        AiTameOrder(cityId, generalId);
    }

    /// <summary>
    /// AI内政开垦查操作
    /// </summary>
    /// <param name="cityId"></param>
    /// <param name="generalId"></param>
    static void AiReclaimOrder(byte cityId, short generalId)
    {
        General general = GeneralListCache.GetGeneral(generalId);
        int needMoney = general.GetNeedMoneyOfInterior(TaskType.Reclaim); // 获取内政开垦需要的金钱
        UIExecutivePanel.Reclaim(cityId, generalId, needMoney);  // 执行相关操作
    }

    /// <summary>
    /// AI内政劝商操作
    /// </summary>
    /// <param name="cityId"></param>
    /// <param name="generalId"></param>
    static void AiMercantileOrder(byte cityId, short generalId)
    {
        General general = GeneralListCache.GetGeneral(generalId);
        int needMoney = general.GetNeedMoneyOfInterior(TaskType.Mercantile);  // 获取内政劝商需要的金钱
        UIExecutivePanel.Mercantile(cityId, generalId, needMoney);  // 执行相关操作
    }

    /// <summary>
    /// AI内政治水操作
    /// </summary>
    /// <param name="cityId"></param>
    /// <param name="generalId"></param>
    static void AiTameOrder(byte cityId, short generalId)
    {
        General general = GeneralListCache.GetGeneral(generalId);
        int needMoney = general.GetNeedMoneyOfInterior(TaskType.Tame);  // 获取内政治水需要的金钱
        UIExecutivePanel.Tame(cityId, generalId, needMoney);  // 执行相关操作
    }

    /// <summary>
    /// Ai内政巡查操作
    /// </summary>
    /// <param name="cityId"></param>
    /// <param name="generalId"></param>
    static void AiPatrolOrder(byte cityId, short generalId)
    {
        General general = GeneralListCache.GetGeneral(generalId);
        int needMoney = general.GetNeedMoneyOfInterior(TaskType.Patrol);  // 获取内政巡查需要的金钱
        UIExecutivePanel.Patrol(cityId, generalId, needMoney);  // 执行相关操作
    }

    // <summary>
    /// Ai判断笼络操作
    /// </summary>
    /// <returns></returns>
    bool AiJudgeBribe()
    {
        short gohireId = 0;
        short behireId = 0;
        byte behireCity = 0;
        byte gohireCity = 0;
        int val = 0;

        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);  // 获取当前回合国家
        byte[] cityIds = country.GetCities();  // 获取该国家的所有城市

        // 遍历所有城市
        for (byte i = 1; i < CityListCache.CITY_NUM; i++)
        {
            City otherCity = CityListCache.GetCityByCityId(i);

            if (otherCity.cityBelongKing != country.countryKingId)  // 判断是否为敌方城市
            {
                short[] otherOfficeGeneralIdArray = otherCity.GetCityOfficeGeneralIdArray();

                for (byte j = 0; j < cityIds.Length; j++)
                {
                    City thisCity = CityListCache.GetCityByCityId(cityIds[j]);

                    if (thisCity.getCityGeneralNum() <= 9)  // 如果城市的将领数量不超过9
                    {
                        short[] thisOfficeGeneralIdArray = thisCity.GetCityOfficeGeneralIdArray();

                        for (int k = 0; k < thisOfficeGeneralIdArray.Length; k++)
                        {
                            short thisGeneralId = thisOfficeGeneralIdArray[k];

                            for (int m = 0; m < otherOfficeGeneralIdArray.Length; m++)
                            {
                                short otherGeneralId = otherOfficeGeneralIdArray[m];
                                int per = AiBribeProbability(thisGeneralId, otherGeneralId);  // 计算招揽成功率

                                if (per > 0)
                                {
                                    General otherGeneral = GeneralListCache.GetGeneral(otherGeneralId);
                                    int curval = (otherGeneral.lead * 3 + otherGeneral.force + otherGeneral.IQ) * per;

                                    if (curval > val)
                                    {
                                        behireId = otherGeneralId;
                                        gohireId = thisGeneralId;
                                        behireCity = i;
                                        gohireCity = cityIds[j];
                                        val = curval;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (behireId == 0)
            return false;

        // 执行招揽操作
        if (behireId != 0 && gohireCity != 0 && gohireId != 0)
        {
            if (BribeRate(gohireId, behireId))  // 判断招揽成功
            {
                AiBribe(gohireCity, behireCity, gohireId, behireId);
                return true;
            }
            AiAlienate(behireCity, gohireId, behireId);  // 如果招揽失败，执行其他操作
            return true;
        }

        return false;
    }

    /// <summary>
    /// AI计算笼络将领的概率
    /// </summary>
    /// <param name="gohireId"></param>
    /// <param name="behireId"></param>
    /// <returns></returns>
    int AiBribeProbability(short gohireId, short behireId)
    {
        General beGeneral = GeneralListCache.GetGeneral(behireId);  // 获取被招揽的将领
        General goGeneral = GeneralListCache.GetGeneral(gohireId);  // 获取招揽的将领

        if (beGeneral.getLoyalty() == 100)  // 如果忠诚度为100，直接返回0
            return 0;

        short kingId = beGeneral.GetOfficeGenBelongKing();  // 获取被招揽将领所属的君主
        if (kingId <= 0)
        {
            Debug.Log("未找到" + beGeneral.generalName + "所属君主！");
            return 0;
        }

        short bepk = GeneralListCache.GetGeneral(kingId).phase;  // 被招揽将领的君主相性
        short pk = GeneralListCache.GetGeneral(goGeneral.GetOfficeGenBelongKing()).phase;  // 招揽将领的君主相性

        int d1 = GeneralListCache.GetdPhase(bepk, beGeneral.phase);  // 计算相性差距
        int d2 = GeneralListCache.GetdPhase(pk, beGeneral.phase);    // 计算相性差距
        int d3 = GeneralListCache.GetdPhase(goGeneral.phase, beGeneral.phase);  // 招揽将领与被招揽将领的相性差距

        if (d1 == 0)
            return 0;
        if (d2 == 0)
            return 1000;

        int val = d1 - d2 - d3 * 2 + 100 - beGeneral.getLoyalty();  // 计算招揽成功的几率
        if (val > 0)
            return val * 20;

        // 随机数判断是否招揽成功
        if (UnityEngine.Random.Range(0, 120) < goGeneral.IQ - beGeneral.IQ)
            return (goGeneral.IQ - beGeneral.IQ) / 2;

        return 0;
    }

    /// <summary>
    /// 检查将领是否可以被离间
    /// </summary>
    /// <param name="gohireId"></param>
    /// <param name="behireId"></param>
    /// <returns></returns>
    bool BribeRate(short gohireId, short behireId)
    {
        General goGeneral = GeneralListCache.GetGeneral(gohireId);
        General beGeneral = GeneralListCache.GetGeneral(behireId);
        if (beGeneral.getLoyalty() >= 90)
            return false;
        short pk1 = (GeneralListCache.GetGeneral(goGeneral.GetOfficeGenBelongKing())).phase;
        short pk2 = (GeneralListCache.GetGeneral(beGeneral.GetOfficeGenBelongKing())).phase;
        int d1 = GeneralListCache.GetdPhase(pk1, beGeneral.phase);
        int d2 = GeneralListCache.GetdPhase(pk2, beGeneral.phase);
        int d3 = GeneralListCache.GetdPhase((GeneralListCache.GetGeneral(gohireId)).phase, beGeneral.phase);
        if (d2 == 0)
            return false;
        if (d2 < 5)
        {
            int val = d2 - d1 * 2 - d3 * 2 + (100 - beGeneral.getLoyalty()) / 2;
            if (val > 0)
                return true;
        }
        else if (d2 <= 10)
        {
            int val = d2 - d1 * 3 / 2 - d3 * 2 + (100 - beGeneral.getLoyalty()) / 2;
            if (val > 0)
                return true;
        }
        else if (d2 <= 20)
        {
            int val = d2 - d1 - d3 * 2 + (100 - beGeneral.getLoyalty()) / 2;
            if (val > 0)
                return true;
        }
        else
        {
            int val = d2 - d1 - d3 * 2 + (100 - beGeneral.getLoyalty()) / 3;
            if (val > 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// AI笼络玩家将领
    /// </summary>
    /// <param name="goCity"></param>
    /// <param name="beCity"></param>
    /// <param name="word0"></param>
    /// <param name="word1"></param>
    IEnumerator AiBribe(byte goCity, byte beCity, short word0, short word1)
    {
        bool flag = BribeMovePossibility(goCity, beCity, word0, word1);

        // 如果雇佣成功且目标城市属于玩家的国家
        if (flag && CityListCache.GetCityByCityId(beCity).cityBelongKing == CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId)
        {
            AIUseCityId = goCity;  // 设置目标城市
            AITargetGeneralId = word1;  // 设置AI将领ID
            string text = GeneralListCache.GetGeneral(word1).generalName + "背弃了主公,从" + CityListCache.GetCityByCityId(beCity).cityName + $"离开,投靠了{kingName}";
            TurnManager.PlayingState = GameState.AIBribe;
            yield return UITurnTips.GetInstance().ShowTurnTips(text);
        }
    }

    /// <summary>
    /// 尝试笼络将领从一个城市转移到另一个城市
    /// </summary>
    /// <param name="byte0"></param>
    /// <param name="cityId"></param>
    /// <param name="word0"></param>
    /// <param name="word1"></param>
    /// <returns></returns>
    bool BribeMovePossibility(byte byte0, byte cityId, short word0, short word1)
    {
        if (BribeRate(word0, word1))
        {
            RandomSetGeneralLoyalty(word1); // 随机设置将领忠诚度
            GeneralListCache.addMoralExp(word0, (byte)5); // 增加将领的道德经验
            GeneralListCache.addIQExp(word0, (byte)2); // 增加将领的智力经验
            City city = CityListCache.GetCityByCityId(cityId);
            city.removeOfficeGeneralId(word1); // 从城市中移除将领
            City city2 = CityListCache.GetCityByCityId(byte0);
            city2.AddOfficeGeneralId(word1); // 将将领添加到另一个城市
            Debug.Log(city2.cityName + "的" + GeneralListCache.GetGeneral(word0).generalName + "招揽" + city.cityName + "的" + GeneralListCache.GetGeneral(word1).generalName);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 随机设置笼络来的将领忠诚度
    /// </summary>
    /// <param name="generalId"></param>
    public static void RandomSetGeneralLoyalty(short generalId)
    {
        GeneralListCache.GetGeneral(generalId).SetLoyalty((byte)UnityEngine.Random.Range(60, 75));
    }

    /// <summary>
    /// Ai离间玩家将领
    /// </summary>
    /// <param name="byte0"></param>
    /// <param name="gohireId"></param>
    /// <param name="behireId"></param>
    IEnumerator AiAlienate(byte byte0, short gohireId, short behireId)
    {
        // 如果成功且目标城市属于玩家的国家
        if (GeneralListCache.IsSuccessOfAlienate(gohireId, behireId))
        {
            if (CityListCache.GetCityByCityId(byte0).cityBelongKing == CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId)
            {
                AITargetGeneralId = behireId;  // 设置AI将领ID
                string text = GeneralListCache.GetGeneral(gohireId).generalName + "正与" + GeneralListCache.GetGeneral(behireId).generalName + "暗中密谈";
                TurnManager.PlayingState=GameState.AIAlienate;  // 设置事件ID
                yield return UITurnTips.GetInstance().ShowTurnTips(text);
            }
            GeneralListCache.addIQExp(gohireId, (byte)1);  // 增加智力经验
        }
    }

    
    // AI搜索并登用过程
    void AiDoSearchEmploy(byte cityId)
    {
        City city = CityListCache.GetCityByCityId(cityId);
        try
        {
            // 如果城市将领数量少于10，尝试进行招揽
            if (city.getCityGeneralNum() < 10 && city.GetReservedGeneralNum() > 0)
            {
                short behireId = city.GetReservedGeneralId((byte)UnityEngine.Random.Range(0, city.GetReservedGeneralNum()));
                if (behireId <= 0)
                    return;

                short generalId = city.GetMostMoralGeneralInCity();
                UIExecutivePanel.EmployKind(generalId, behireId);
            }

            short generalId2 = city.GetMostIQMoralGeneralInCity();
            UIExecutivePanel.Search(generalId2,cityId);

        }
        catch (Exception exception)
        {
            Debug.Log(exception); // 捕获异常，防止程序崩溃
        }
    }
   

    /// <summary>
    /// AI搜索将领招揽
    /// </summary>
    /// <param name="curTurnsCountryId"></param>
    void AiSearch()
    {
        // 随机50%几率进行搜索
        if (UnityEngine.Random.Range(0, 100) < 50)
            return;
        Debug.Log($"{kingName}正在搜罗人才");
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);

        // 遍历国家所有城市，寻找可以招揽的将领
        for (int index = 0; index < country.GetHaveCityNum(); index++)
        {
            byte curCityId = country.getCity(index);
            City city = CityListCache.GetCityByCityId(curCityId);
            if (city.getCityGeneralNum() < 10 && city.GetReservedGeneralNum() > 0)
            {
                short generalId = city.GetReservedGeneralId((byte)UnityEngine.Random.Range(0, city.GetReservedGeneralNum()));
                if (generalId > 0)
                {
                    short employGeneralId = city.GetDoSearchGen(generalId);
                    if (UIExecutivePanel.EmployKind(employGeneralId, generalId)==TaskType.EmploySuccess)
                    return;
                }
            }
        }

        // 再次遍历，处理剩余的招揽
        for (int byte1 = 0; byte1 < country.GetHaveCityNum(); byte1++)
        {
            byte curCityId = country.getCity(byte1);
            City city = CityListCache.GetCityByCityId(curCityId);
            if (city.GetReservedGeneralNum() > 0)
            {
                short generalId = city.GetMostIQMoralGeneralInCity();
                UIExecutivePanel.Search(generalId, curCityId);
            }
        }
    }

    // 必须钱粮交易
    private bool AiMustShopping()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        byte[] adjacentCity = country.GetEnemyAdjacentCityIdArray();
        bool flag = false;

        if (!country.haveGrainShop())
            return flag;

        for (int index = 0; index < adjacentCity.Length; index++)
        {
            byte curCity = adjacentCity[index];
            if (curCity <= 0)
                break;

            City city = CityListCache.GetCityByCityId(curCity);
            int needFood = city.needFoodToHarvest() + city.needFoodWarAMonth();

            if (city.GetFood() < needFood && city.GetMoney() > 200)
            {
                int buyNum = needFood - city.GetFood();
                buyNum = System.Math.Min(30000 - city.GetFood(), buyNum);

                if (buyNum >= 100 || city.GetFood() <= 100)
                {
                    if (buyNum > city.GetMoney() * 4 / 3)
                    {
                        short buy = (short)((city.GetMoney() - 50) * 4 / 3);
                        city.AddFood(buy);
                        city.SetMoney((short)50);
                        Debug.Log($"{city.cityName} -50黄金买入粮食+{buy} ");
                    }
                    else
                    {
                        city.AddFood((short)buyNum);
                        city.DecreaseMoney((short)(buyNum * 3 / 4));
                        Debug.Log($"{city.cityName} -{(buyNum * 3 / 4)}黄金买入粮食+{buyNum}");
                    }
                    flag = true;
                }
            }
        }

        for (int index1 = 0; index1 < adjacentCity.Length; index1++)
        {
            byte curCity = adjacentCity[index1];
            if (curCity <= 0)
                break;

            City city = CityListCache.GetCityByCityId(curCity);
            int needFood = city.needFoodToHarvest() + city.needFoodWarAMonth();

            if (city.GetFood() > needFood + 200)
            {
                int sellNum = city.GetFood() - needFood - 200;
                sellNum = System.Math.Min(30000 - city.GetMoney(), sellNum);

                if (sellNum >= 50)
                {
                    city.DecreaseFood((short)sellNum);
                    city.AddMoney((short)(sellNum * 3 / 4));
                    Debug.Log($"{city.cityName} -{sellNum}粮食售得黄金+{(sellNum * 3 / 4)}");
                    flag = true;
                }
            }
            else if (city.GetMoney() <= 0)
            {
                int needMoney = city.needAllSalariesMoney();
                int needSellFood = needMoney * 4 / 3;

                if (needSellFood >= city.GetFood() / 2)
                    return flag;

                city.AddMoney((short)needMoney);
                city.DecreaseFood((short)needSellFood);
                Debug.Log($"{city.cityName} -{needSellFood}粮食售得黄金+{needMoney}");
            }
        }

        byte[] noAdjacentCity = country.GetNoEnemyAdjacentCityIdArray();
        for (int b1 = 0; b1 < noAdjacentCity.Length; b1++)
        {
            byte curCity = noAdjacentCity[b1];
            if (curCity <= 0)
                break;

            City city = CityListCache.GetCityByCityId(curCity);
            int needFood = city.needFoodToHarvest();

            if (city.GetFood() > needFood + 50)
            {
                int sellNum = city.GetFood() - needFood - 50;
                sellNum = System.Math.Min(30000 - city.GetMoney(), sellNum);

                if (sellNum >= 50)
                {
                    city.DecreaseFood((short)sellNum);
                    city.AddMoney((short)(sellNum * 3 / 4));
                    Debug.Log($"{city.cityName} -{sellNum}粮食售得黄金+{(sellNum * 3 / 4)}");
                    flag = true;
                }
            }
            else if (city.GetMoney() <= 0)
            {
                int needMoney = city.needAllSalariesMoney();
                int needSellFood = needMoney * 4 / 3;

                if (needSellFood >= city.GetFood() / 2)
                    return flag;

                city.AddMoney((short)needMoney);
                city.DecreaseFood((short)needSellFood);
                Debug.Log($"{city.cityName} - {needSellFood}粮食售得黄金+{needMoney}");
            }
        }

        return flag;
    }

    // AI 自动征兵
    void AiConscription()
    {
        byte thecity = 0;  // 记录人口最多的城市 ID
        int maxpopulation = 0;  // 记录最大人口
        Country curCountry = CountryListCache.GetCountryByCountryId(curTurnsCountryId);  // 获取当前回合国家

        if (curCountry == null)  // 如果国家为空，则返回
            return;

        byte CITY_NUM = curCountry.GetHaveCityNum();  // 获取当前国家的城市数量

        if (CITY_NUM < 1)  // 如果没有城市，则返回
            return;

        // 遍历所有城市，找到人口最多的城市
        for (int i = 0; i < CITY_NUM; i++)
        {
            byte cityId = curCountry.getCity(i);  // 获取城市 ID
            City city = CityListCache.GetCityByCityId(cityId);  // 根据 ID 获取城市实例

            // 如果城市人口超过 1000 且大于当前记录的最大人口，更新最大人口和目标城市 ID
            if (city.population > 1000 && city.population > maxpopulation)
            {
                maxpopulation = city.population;
                thecity = cityId;
            }
        }

        // 如果没有符合条件的城市或者最大人口少于 1000，返回
        if (thecity == 0 || maxpopulation < 1000)
            return;

        // 遍历所有城市进行征兵操作
        for (int i = 0; i < CITY_NUM; i++)
        {
            byte cityId = curCountry.getCity(i);  // 获取当前城市 ID
            City city = CityListCache.GetCityByCityId(cityId);  // 获取当前城市实例

            int i1 = city.getAllSoldierNum();  // 获取城市的总士兵数
            int j1 = i1 - city.GetCityAllSoldierNum();  // 获取城市非驻守士兵数
            int needSoldierNum = j1 - city.cityReserveSoldier;  // 计算需要补充的士兵数

            // 如果需要补充士兵
            if (needSoldierNum > 0)
            {
                int needMoney = ((needSoldierNum - 1) / 100 + 1) * 20;  // 计算征兵所需的金钱
                City thisCity = CityListCache.GetCityByCityId(thecity);  // 获取人口最多的城市实例

                // 如果当前城市有足够的金钱进行征兵
                if (city.GetMoney() >= needMoney)
                {
                    city.DecreaseMoney((short)needMoney);  // 减少城市金钱
                    city.cityReserveSoldier += needMoney * 5;  // 增加城市的预备士兵
                    thisCity.population -= needSoldierNum;  // 减少人口最多城市的人口

                    // 如果人口减少到负数，置为 0
                    if (thisCity.population < 0)
                        thisCity.population = 0;
                }
                else  // 如果金钱不足
                {
                    int i2 = city.GetMoney() / 20 * 20;  // 根据金钱计算可征兵的数量
                    city.DecreaseMoney((short)i2);  // 减少城市金钱
                    city.cityReserveSoldier += i2 * 5;  // 增加城市的预备士兵
                    thisCity.population -= i2 * 5;  // 减少人口最多城市的人口

                    // 如果人口减少到负数，置为 0
                    if (thisCity.population < 0)
                        thisCity.population = 0;
                }
            }

            // 分配士兵到将领
            city.distributionSoldier();
        }
    }

    private void AiDefence()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        country.Defence();
    }

    


    private void AiTransportMoney()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        country.TransportMoney();
    }


    /// <summary>
    /// Ai检测低忠诚度将领方法
    /// </summary>
    /// <returns></returns>
    bool AiFindLowLoyaltyGeneral()
    {
        int maxGeneralScore = 0; // 初始化最大将领得分
        short tempGeneralId = 0; // 初始化临时将领ID
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId); // 获取当前国家
                                                                                            // 遍历国家中的城市
        for (int k = 0; k < country.GetHaveCityNum(); k++)
        {
            byte cityId = country.getCity(k); // 获取城市ID
            City city = CityListCache.GetCityByCityId(cityId); // 获取城市对象
                                                                // 检查城市金钱或宝物数量
            if (city.GetMoney() >= 50 || city.treasureNum != 0)
            {
                short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray(); // 获取城市办公厅将领ID数组
                                                                                    // 遍历城市中的将领
                for (byte i = 0; i < city.getCityGeneralNum(); i = (byte)(i + 1))
                {
                    short GeneralId = officeGeneralIdArray[i]; // 获取将领ID
                    General general = GeneralListCache.GetGeneral(GeneralId); // 获取将领对象
                    int generalScore = general.getGeneralScore(); // 获取将领得分
                                                                    // 检查将领忠诚度和城市的金钱/宝物条件
                    if (generalScore > maxGeneralScore)
                        if (general.getLoyalty() < 90 && city.GetMoney() > 50 && city.GetMoney() / 50 > UnityEngine.Random.Range(0, 4))
                        {
                            maxGeneralScore = generalScore; // 更新最大得分
                            tempGeneralId = GeneralId; // 更新临时将领ID
                            AIUseGeneralId = tempGeneralId; // 设置目标将领
                            AIUseCityId = cityId; // 设置目标城市
                        }
                        else if (general.getLoyalty() >= 90 && city.treasureNum > 0 && city.treasureNum > UnityEngine.Random.Range(0, 4))
                        {
                            maxGeneralScore = generalScore; // 更新最大得分
                            tempGeneralId = GeneralId; // 更新临时将领ID
                            AIUseGeneralId = tempGeneralId; // 设置目标将领
                            AIUseCityId = cityId; // 设置目标城市
                        }
                }
            }
        }
        if (tempGeneralId == 0) // 如果没有找到合适的将领
            return false;

        return true; // 返回true
    }

    /// <summary>
    /// Ai奖赏将领忠诚度或减少目标城市的宝藏或金钱
    /// </summary>
    void AiReward()
    {
        // 如果没有设置目标城市，直接返回
        if (AIUseCityId == 0)
            return;

        General general = GeneralListCache.GetGeneral(AIUseGeneralId);  // 获取当前将领
        City city = CityListCache.GetCityByCityId(AIUseCityId);  // 获取目标城市

        // 如果将领的忠诚度大于 90 且城市有宝藏
        if (general.getLoyalty() > 90)
        {
            if (city.treasureNum > 0)
            {
                general.AddLoyalty(false);  // 增加忠诚度但标记为不友好
                city.treasureNum = (byte)(city.treasureNum - 1);  // 减少城市的宝藏数量
                return;
            }
        }
        // 否则如果城市金钱大于 50
        else if (city.GetMoney() > 50)
        {
            general.AddLoyalty(true);  // 增加忠诚度且标记为友好
            city.DecreaseMoney((short)50);  // 减少城市的金钱
        }
        // 如果城市没有足够的金钱但有宝藏
        else if (city.treasureNum > 0)
        {
            general.AddLoyalty(false);  // 增加忠诚度但标记为不友好
            city.treasureNum = (byte)(city.treasureNum - 1);  // 减少城市的宝藏
        }
    }




    private IEnumerator AiAttack() 
    {  
        byte num = (orderNum - AIUseOrderNum > warNum) ? warNum : (byte)(orderNum - AIUseOrderNum);  // 计算当前战争次数
        if (AIThinkWar(curTurnsCountryId, num) && warRand(num))  // 如果AI决定战争且符合随机条件
        {
            warNum = (byte)(warNum - 1);  // 减少战争次数
            yield return startWar();  // 开始战争
            if (j_byte_fld == 3)
            {
                //WarManager.Instance.AIattackPlayer();  // AI攻击玩家

                TurnManager.Instance.CountryDieAfterWar();  // 执行战斗后的结算
                if (j_byte_fld == 99)
                {
                    j_byte_fld = 0;
                    yield break;  // 如果状态为99，结束函数
                }
            }
        }
    }

    /// <summary>
    /// AI判断是否发动战争
    /// </summary>
    /// <param name="curTurnsCountryId"></param>
    /// <param name="warNum"></param>
    /// <returns></returns>
    bool AIThinkWar(byte curTurnsCountryId, byte warNum)
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        int minMustPower = 0;
        bool result = false;
        for (int i = 0; i < country.GetHaveCityNum(); i++)
        {
            byte curCityId = country.getCity(i);
            City curUserCity = CityListCache.GetCityByCityId(curCityId);
            int gl = 100 * curUserCity.getAlreadyAllSoldierNum() / curUserCity.getAllSoldierNum();  // 计算士兵比例
            int gl2 = UnityEngine.Random.Range(0, 101);  // 随机获取一个数值用于判断
            if (gl2 <= gl)  // 根据概率判断是否发动战争
            {
                int maxAtkPower = (int)(curUserCity.getMaxAtkPower() * (0.5D + 0.5D * warNum));  // 计算最大攻击力
                int maxEnemyAtkPower = (int)(CountryListCache.GetEnemyAdjacentAtkPowerArray(curCityId) * 0.65D);  // 计算敌人的最大攻击力
                if (maxAtkPower > maxEnemyAtkPower)  // 如果我方攻击力大于敌方
                {
                    byte[] adjacentCity = CountryListCache.GetEnemyCityIdArray(curCityId);  // 获取敌方相邻城市
                    for (int enemyIndex = 0; enemyIndex < adjacentCity.Length; enemyIndex++)
                    {
                        byte curEnemyCityId = adjacentCity[enemyIndex];
                        if (GameInfo.isWatch)  // 如果当前为观察模式，跳过玩家所属城市
                        {
                            City enemyCity = CityListCache.GetCityByCityId(curEnemyCityId);
                            if (enemyCity.cityBelongKing != 0 && CountryListCache.GetCountryByKingId(enemyCity.cityBelongKing).countryId == GameInfo.playerCountryId)
                                continue;
                        }
                        int defenseAbility = CountryListCache.GetEnemyAdjacentCityDefenseAbility(curEnemyCityId, curCityId);  // 获取敌方防御能力
                        if (maxAtkPower - maxEnemyAtkPower > defenseAbility && minMustPower < maxAtkPower - maxEnemyAtkPower - defenseAbility)
                        {
                            minMustPower = maxAtkPower - maxEnemyAtkPower - defenseAbility;  // 更新最小必需攻击力
                            result = true;
                            AITargetCityId = curCityId;
                            AIUseCityId = curEnemyCityId;
                        }
                        continue;
                    }
                }
            }
        }
        return result;  // 返回是否发动战争的判断结果
    }

    /// <summary>
    /// 战争随机决策函数，根据战争能力计算是否可以胜利
    /// </summary>
    /// <param name="warNum"></param>
    /// <returns></returns>
    bool warRand(byte warNum)
    {
        City cCity = CityListCache.GetCityByCityId(AIUseCityId); // 获取当前城市信息
        int defenseAbility = cCity.GetDefenseAbility(); // 获取防御能力
        City tCity = CityListCache.GetCityByCityId(AITargetCityId); // 获取目标城市信息
        int atkPower = tCity.getMaxAtkPower(); // 获取目标城市的最大攻击能力
        double warAbility = (5 * warNum + getWarAbility(atkPower, defenseAbility)); // 计算战争能力
        int random = UnityEngine.Random.Range(0, 100); // 获取0到100的随机数
        if (random <= warAbility) // 如果随机数小于等于战争能力，战争胜利
            return true;

        if (cCity.cityBelongKing == 0) // 如果城市没有归属的国家，战争失败
            return false;

        if ((CountryListCache.GetCountryByKingId(cCity.cityBelongKing)).countryId == GameInfo.playerCountryId) // 如果城市所属国家是玩家国家
        {
            random = UnityEngine.Random.Range(0, 100); // 再次获取随机数
            if (random <= warAbility)
                return true; // 如果随机数仍然小于战争能力，战争胜利
        }
        return false; // 否则战争失败
    }

    /// <summary>
    /// 计算战争能力值，根据攻击力与防御力的比率得出战争能力
    /// </summary>
    /// <param name="atkPower"></param>
    /// <param name="defenseAbility"></param>
    /// <returns></returns>
    private byte getWarAbility(int atkPower, int defenseAbility)
    {
        byte ability = 0;
        if (atkPower >= 2 * defenseAbility) // 攻击力是防御力的两倍或以上
        {
            ability = 80;
        }
        else if (atkPower >= 1.67D * defenseAbility)
        {
            ability = 70;
        }
        else if (atkPower >= 1.33D * defenseAbility)
        {
            ability = 60;
        }
        else if (atkPower >= defenseAbility)
        {
            ability = 50;
        }
        else if (atkPower >= 0.84D * defenseAbility)
        {
            ability = 40;
        }
        else if (atkPower >= 0.68D * defenseAbility)
        {
            ability = 30;
        }
        else if (atkPower >= 0.52D * defenseAbility)
        {
            ability = 20;
        }
        else if (atkPower >= 0.36D * defenseAbility)
        {
            ability = 10;
        }
        return ability; // 返回战争能力值
    }

    // 发起战争
    IEnumerator startWar()
    {
        int needFood = 0;
        int needMoney = 0;

        // 获取目标城市的实例
        City tCity = CityListCache.GetCityByCityId(AITargetCityId);

        // 计算敌方临近城市的最大攻击力
        int enemyAdjacentAtkPower = CountryListCache.getOtherCityMaxAtkPower(AITargetCityId, AIUseCityId);
        enemyAdjacentAtkPower = (int)(enemyAdjacentAtkPower * 0.3D); // 取30%的值

        // 获取战事办公室中的将领 ID 数组
        short[] warOfficeGeneralIdArray = tCity.GetWarOfficeGeneralIdArray(enemyAdjacentAtkPower);

        // 选择的将领数量
        chooseGeneralNum = (byte)warOfficeGeneralIdArray.Length;

        // 如果没有将领可以参与战争，直接返回
        if (chooseGeneralNum == 0)
            yield break;

        // 移除战事办公室的将领
        for (int i = 0; i < warOfficeGeneralIdArray.Length; i++)
            tCity.removeOfficeGeneralId(warOfficeGeneralIdArray[i]);

        // 重新任命太守
        tCity.AppointmentPrefect();

        // 初始化选择的将领 ID 数组
        chooseGeneralIdArray = new short[10];

        // 将在任的将领 ID 填充到选择的将领数组中
        for (int i = 0; i < warOfficeGeneralIdArray.Length; i++)
            chooseGeneralIdArray[i] = warOfficeGeneralIdArray[i];

        // 计算所需粮食
        needFood = NeedFoodValue(tCity, chooseGeneralIdArray, chooseGeneralNum);

        // 获取当前城市实例
        City cCity = CityListCache.GetCityByCityId(AIUseCityId);

        // 获取当前城市所属国王的 ID
        short kingId = cCity.cityBelongKing;

        // 计算所需金钱，若金钱少于50，设为0，否则取一半
        if (tCity.GetMoney() < 50)
        {
            needMoney = 0;
        }
        else
        {
            needMoney = tCity.GetMoney() / 2;
        }

        // 如果当前城市没有国王，减少所需粮食和金钱
        if (kingId == 0)
        {
            needFood /= 5;
            needMoney /= 5;
        }

        // 减少目标城市的金钱和粮食
        tCity.DecreaseMoney((short)needMoney);
        tCity.DecreaseFood((short)needFood);

        // 垃圾回收
        GC.Collect();

        // 判断进攻方是否是玩家控制的国家
        if (kingId == (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId)).countryKingId)
        {
            // 更新显示信息
            TurnManager.PlayingState = GameState.PlayervsAI;
            string text = $"{GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(curTurnsCountryId)).countryKingId).generalName}军攻打 {cCity.cityName}...";
            yield return UITurnTips.GetInstance().ShowTurnTips(text);

            // 设置事件 ID
            eventId = 1;


            // 记录战争中的金钱和粮食
            aiMoney_inWar = (short)needMoney;
            aiGrain_inWar = (short)needFood;
            j_byte_fld = 3;
            yield break;
        }

        // 若当前城市无国王
        if (kingId == 0)
        {
            eventId = 2;
            OccupyCity(chooseGeneralIdArray, chooseGeneralNum, needFood, needMoney);
        }
        else
        {
            // 获取敌方国家 ID
            byte countryId = (CountryListCache.GetCountryByKingId(kingId)).countryId;

            // 构建显示信息的字符串
            string str = "[";
            for (int j = 0; j < chooseGeneralNum && j < 3; j++)
                str += $"{GeneralListCache.GetGeneral(chooseGeneralIdArray[j]).generalName}、";

            // 移除最后一个 "、"
            str = str.Substring(0, str.Length - 1);
            str += "]";

            if (chooseGeneralNum > 3)
                str += $"等{chooseGeneralNum}员大将";

            str = $"{GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(curTurnsCountryId)).countryKingId).generalName}军从{tCity.cityName}出动了{str}攻打{GeneralListCache.GetGeneral(kingId).generalName}的{cCity.cityName}！";

            // 输出信息到控制台
            Debug.Log(str);

            // 更新游戏显示信息
            TurnManager.PlayingState = GameState.AIvsAI;
            yield return UITurnTips.GetInstance().ShowTurnTips(str);

            // 判断是否能够占领城市
            if (isCapture(chooseGeneralIdArray, chooseGeneralNum, needFood, needMoney))
            {
                TurnManager.PlayingState = GameState.AIWinAI;
                string win = $"{GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(curTurnsCountryId)).countryKingId).generalName}军占领了{cCity.cityName}！";
                yield return UITurnTips.GetInstance().ShowTurnTips(win);
            }
            else
            {
                TurnManager.PlayingState = GameState.AILoseAI;
                string lose = $"{GeneralListCache.GetGeneral(kingId).generalName}军在{cCity.cityName}被{GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(curTurnsCountryId)).countryKingId).generalName}军所击败！";
                yield return UITurnTips.GetInstance().ShowTurnTips(lose);
            }

            // 检查敌方国家是否已经灭亡
            if (CountryListCache.GetCountryByCountryId(countryId) == null)
            {
                AIUseGeneralId = kingId;
                loseCountryId = countryId;
                TurnManager.PlayingState = GameState.AIFail;
                string fail = $"{GeneralListCache.GetGeneral(kingId).generalName}势力消灭了";
                yield return UITurnTips.GetInstance().ShowTurnTips(fail);
            }
            else if ((CountryListCache.GetCountryByCountryId(countryId)).countryKingId != kingId)
            {
                AIUseGeneralId = kingId;
                loseCountryId = countryId;
                TurnManager.PlayingState = GameState.AIInherit;
                string inherit = $"{GeneralListCache.GetGeneral((CountryListCache.GetCountryByCountryId(countryId)).countryKingId).generalName}继承了原{GeneralListCache.GetGeneral(kingId).generalName}的势力！";
                yield return UITurnTips.GetInstance().ShowTurnTips(inherit);
                Debug.Log(inherit);
            }
        }

        // 暂停500毫秒以模拟事件延迟
        try
        {
            System.Threading.Thread.Sleep(500);
        }
        catch (Exception exception) { }

        // 垃圾回收
        System.GC.Collect();
    }

    /// <summary>
    /// 计算所需食物的数量
    /// </summary>
    /// <param name="city"></param>
    /// <param name="chooseGeneralIdArray"></param>
    /// <param name="chooseGeneralNum"></param>
    /// <returns></returns>
    int NeedFoodValue(City city, short[] chooseGeneralIdArray, int chooseGeneralNum)
    {
        int allGeneralSoldier = 0; // 初始化所有将军的士兵数量
        allGeneralSoldier = CalculateTotalGeneralSoldierNum(chooseGeneralIdArray, (byte)chooseGeneralNum); // 计算所有将军的士兵数量
        allGeneralSoldier = allGeneralSoldier * 4 / 1000 + 1; // 计算修正后的士兵数量

        // 计算所需食物数量
        int needFood = UnityEngine.Random.Range(0, allGeneralSoldier * 30) + allGeneralSoldier * 30;
        if (needFood < city.GetFood() / 2)
        {
            needFood = city.GetFood() / 2; // 如果所需食物少于城市的一半食物，则取城市食物的一半
        }
        else if (needFood >= city.GetFood())
        {
            needFood = city.GetFood() * 2 / 3; // 如果所需食物大于等于城市食物，则取城市食物的三分之二
        }
        return needFood; // 返回所需食物数量
    }

    // 计算军中将领兵力
    short CalculateTotalGeneralSoldierNum(short[] generalIdArray, byte generalNum)
    {
        short word0 = 0;

        for (byte byte1 = 0; byte1 < generalNum; byte1++)
            word0 += GeneralListCache.GetGeneral(generalIdArray[byte1]).generalSoldier;

        return word0;
    }

    /// <summary>
    /// 执行占领城市方法
    /// </summary>
    /// <param name="generalIdArray"></param>
    /// <param name="generalNum"></param>
    /// <param name="food"></param>
    /// <param name="money"></param>
    void OccupyCity(short[] generalIdArray, byte generalNum, int food, int money)
    {
        CountryListCache.GetCountryByCountryId(curTurnsCountryId).AddCity(AIUseCityId); // 将当前城市添加到国家
        City city = CityListCache.GetCityByCityId(AIUseCityId); // 获取城市对象
        for (int k1 = 0; k1 < generalNum; k1++)
            city.AddOfficeGeneralId(generalIdArray[k1]); // 将将军 ID 添加到城市中
        city.cityBelongKing = CityListCache.GetCityByCityId(AITargetCityId).cityBelongKing; // 设置城市的国王
        city.prefectId = generalIdArray[0]; // 设置城市的 prefectId
        city.AddFood((short)food); // 添加食物
        city.AddMoney((short)money); // 添加资金
    }

    // 判断是否占领城市
    bool isCapture(short[] generalIdArray, byte generalNum, int food, int j1)
    {
        gffpjy(generalIdArray, generalNum); // 计算己方武将的战斗力
        City city = CityListCache.GetCityByCityId(AIUseCityId); // 获取当前城市
        fffpjy(city.GetCityOfficeGeneralIdArray()); // 计算敌方武将的战斗力

        // 若AI进行战争
        if (AIWar2(generalIdArray, generalNum, food))
        {
            IsDestroyed(generalIdArray, generalNum, food, j1); // 处理战后事宜
            return true; // 占领成功
        }

        // 战争失败，进行相应处理
        IsRetreat(generalIdArray, generalNum, (CityListCache.GetCityByCityId(AITargetCityId)).cityBelongKing, food, j1);
        return false;
    }


    // 根据武将数组计算己方阵列的战斗力
    void gffpjy(short[] generalIdArray, byte generalNum)
    {
        int[]gfZL = new int[generalNum]; // 己方总战斗力
        int[]gfzdl = new int[generalNum]; // 己方单个武将战斗力
        int gfZZL = 1; // 己方总战斗力初始值

        for (byte i = 0; i < generalNum; i = (byte)(i + 1))
        {
            General general = GeneralListCache.GetGeneral(generalIdArray[i]); // 获取武将
            byte zl = general.IQ; // 智力
            byte ts = general.lead; // 统帅
            byte dj = general.level; // 等级
            byte wl = general.force; // 武力

            // 计算武将战斗力 (多属性加成公式)
            int gjl = (int)(ts * 1.42D + zl * 0.25D + wl * 0.33D + ((ts * 2 + wl + zl) * (dj - 1)) * 0.04D);

            // 根据智力值的不同调整战斗力
            if (zl >= 100)
                gjl += 15;
            else if (zl >= 95)
                gjl += 12;
            else if (zl >= 88)
                gjl += 8;
            else if (zl >= 80)
                gjl += 5;
            else if (zl >= 70)
                gjl += 3;
            else if (zl >= 55)
                gjl -= 5;
            else if (zl >= 40)
                gjl -= 10;
            else
                gjl -= 20;

            // 最终战斗力计算 (考虑平方值与兵力的影响)
            long gjl2 = (gjl * gjl * gjl / 100000 + 1);
            gfzdl[i] = (int)gjl2;

            // 调整战斗力上限与下限
            if (general.generalSoldier < 500)
                gfzdl[i] = Math.Min(100, gfzdl[i]);

            if (gfzdl[i] < 20)
                gfzdl[i] = Math.Max(general.generalSoldier / 150, gfzdl[i]);

            // 总战斗力计算
            gfZL[i] = gfzdl[i];
            gfZL[i] = gfZL[i] * (general.generalSoldier + 1);
            gfZZL += gfZL[i];
        }
    }

    // 根据武将数组计算敌方阵列的战斗力
    void fffpjy(short[] generalIdArray)
    {
        int[]ffZL = new int[generalIdArray.Length]; // 敌方总战斗力
        int[]ffzdl = new int[generalIdArray.Length]; // 敌方单个武将战斗力
        int ffZZL = 1; // 敌方总战斗力初始值

        for (byte i = 0; i < generalIdArray.Length; i = (byte)(i + 1))
        {
            General general = GeneralListCache.GetGeneral(generalIdArray[i]); // 获取武将
            byte zl = general.IQ; // 智力
            byte ts = general.lead; // 统帅
            byte dj = general.level; // 等级
            byte wl = general.force; // 武力

            // 计算武将战斗力 (多属性加成公式)
            int gjl = (int)(ts * 1.42D + zl * 0.25D + wl * 0.33D + ((ts * 2 + wl + zl) * (dj - 1)) * 0.04D);

            if (i == 0)
                gjl = (int)(gjl * 1.3D); // 队长额外加成

            // 根据智力值的不同调整战斗力
            if (zl >= 100)
                gjl += 15;
            else if (zl >= 95)
                gjl += 12;
            else if (zl >= 88)
                gjl += 8;
            else if (zl >= 80)
                gjl += 5;
            else if (zl >= 70)
                gjl += 3;
            else if (zl >= 55)
                gjl -= 5;
            else if (zl >= 40)
                gjl -= 10;
            else
                gjl -= 20;

            // 最终战斗力计算
            long gjl2 = (gjl * gjl * gjl / 100000 + 1);
            ffzdl[i] = (int)gjl2;

            // 调整战斗力上限与下限
            if (general.generalSoldier < 500)
                ffzdl[i] = Math.Min(150, ffzdl[i]);

            if (ffzdl[i] < 20)
                ffzdl[i] = Math.Max(general.generalSoldier / 150, ffzdl[i]);

            // 总战斗力计算
            if (i == 0)
                ffZL[i] = ffZL[i] * (general.generalSoldier + 1);
            else
                ffZL[i] = ffZL[i] * (general.generalSoldier + 1);

            ffZZL += ffZL[i];
        }
    }

    // 计算进攻方的消耗粮草
    short atkEatFood(short[] genId, byte genNum)
    {
        short eatNum = 3; // 初始消耗为3
        short atkTotleSoldier = CalculateTotalGeneralSoldierNum(genId, genNum); // 计算进攻方的总兵力
        eatNum = (short)(eatNum + atkTotleSoldier * 4 / 1000); // 按兵力比例增加消耗
        return eatNum;
    }

    // 计算防守方的消耗粮草
    short defEatFood(short[] genId)
    {
        short eatNum = 1; // 初始消耗为1
        short defTotleSoldier = CalculateTotalGeneralSoldierNum(genId, (byte)genId.Length); // 计算防守方的总兵力
        eatNum = (short)(eatNum + defTotleSoldier * 4 / 1000); // 按兵力比例增加消耗
        return eatNum;
    }

    // AI进行战争模拟
    bool AIWar2(short[] genId, byte genNum, int atkfood)
    {
        bool occupy = false; // 是否占领
        bool atknot = false; // 进攻方是否无法进攻
        bool defnot = false; // 防守方是否无法防守
        byte day = 0; // 战斗持续天数
        City city = CityListCache.GetCityByCityId(AIUseCityId); // 获取当前城市

        // 战斗模拟循环
        while (true)
        {
            day = (byte)(day + 1); // 天数增加
            atkfood -= atkEatFood(genId, genNum); // 进攻方消耗粮草

            if (atkfood <= 0)
            {
                atkfood = 0; // 粮草耗尽
                break;
            }

            // 防守方消耗粮草
            city.DecreaseFood(defEatFood(city.GetCityOfficeGeneralIdArray()));

            // 若防守方粮草耗尽
            if (city.GetFood() <= 0)
            {
                city.SetFood((short)0); // 设置防守方粮草为0
                occupy = true; // 城市被占领
                break;
            }

            if (day < 3) // 战斗持续时间小于3天继续
                continue;

            if (day > 30) // 战斗持续时间超过30天结束
                break;

            // 寻找进攻方的攻击武将
            short attackGeneralId = -1;
            byte[] sequence1 = new byte[genNum];
            for (byte i = 0; i < genNum; i = (byte)(i + 1))
                sequence1[i] = i;
            sequence1 = RandomArray(sequence1); // 随机打乱进攻方武将顺序

            // 查找能够进攻的武将
            for (byte i = 0; i < sequence1.Length; i = (byte)(i + 1))
            {
                byte index1 = sequence1[i];
                int soldier = GeneralListCache.GetGeneral(genId[index1]).generalSoldier;
                byte phy = GeneralListCache.GetGeneral(genId[index1]).getCurPhysical();
                if (soldier > 0 || phy > 50)
                {
                    attackGeneralId = genId[index1]; // 找到攻击武将
                    atknot = false;
                    break;
                }
                atknot = true; // 无法找到攻击武将
            }

            // 随机跳过本次战斗
            if (UnityEngine.Random.Range(1, 101) < 15)
                continue;

            if (atknot)
                break; // 无法继续进攻

            // 寻找防守方的防守武将
            short preventGeneralId = -1;
            byte[] sequence2 = new byte[city.getCityGeneralNum()];
            for (byte b1 = 0; b1 < city.getCityGeneralNum(); b1 = (byte)(b1 + 1))
                sequence2[b1] = b1;
            sequence2 = RandomArray(sequence2); // 随机打乱防守方武将顺序
            short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray();

            // 查找能够防守的武将
            for (byte j = 0; j < sequence2.Length; j = (byte)(j + 1))
            {
                byte index2 = sequence2[j];
                short generalId = officeGeneralIdArray[index2];
                int soldier = GeneralListCache.GetGeneral(generalId).generalSoldier;
                byte phy = GeneralListCache.GetGeneral(generalId).getCurPhysical();
                if (soldier > 0 || phy > 50)
                {
                    preventGeneralId = generalId; // 找到防守武将
                    defnot = false;
                    break;
                }
                defnot = true; // 无法找到防守武将
            }

            if (defnot)
            {
                occupy = true; // 防守失败，占领城市
                break;
            }

            if (attackGeneralId > 0 && preventGeneralId > 0)
                moniAtk2(attackGeneralId, preventGeneralId); // 模拟攻防战
        }

        return occupy; // 返回是否占领成功
    }

    byte[] RandomArray(byte[] array) 
    {
        System.Random random = new System.Random();
        for (int i = 0; i < array.Length; i++) {
            int p = random.Next(array.Length); // 使用静态随机数生成器
            byte tmp = array[i];
            array[i] = array[p];
            array[p] = tmp; // 移除不必要的类型转换
        }
        return array;
    }
    
    
    
    
    /// <summary>
    /// 执行 DefenseTurnToAttack 方法
    /// </summary>
    void DefenseTurnToAttack()
    {
        Country attackCountry = CountryListCache.GetCountryByKingId(CityListCache.GetCityByCityId(AITargetCityId).cityBelongKing); // 获取攻击国
        Country defenseCountry = CountryListCache.GetCountryByKingId(CityListCache.GetCityByCityId(AIUseCityId).cityBelongKing); // 获取防御国
        attackCountry.AddCity(AIUseCityId); // 将城市添加到攻击国
        defenseCountry.RemoveCity(AIUseCityId); // 从防御国中移除城市
    }


    /// <summary>
    /// 处理势力灭亡逻辑
    /// </summary>
    /// <param name="attackGeneralArray"></param>
    /// <param name="attackGeneralNum"></param>
    /// <param name="carryFood"></param>
    /// <param name="money"></param>
    IEnumerator IsDestroyed(short[] attackGeneralArray, byte attackGeneralNum, int carryFood, int money)
    {
        City city = CityListCache.GetCityByCityId(AIUseCityId); // 获取当前城市
        byte defenseGeneralNum = city.getCityGeneralNum(); // 获取防御将军数量
        short kingId = city.cityBelongKing; // 获取城市所属国王
        Country country = CountryListCache.GetCountryByKingId(kingId); // 获取国家对象
        short[] defenseGeneralIdArray = city.GetCityOfficeGeneralIdArray(); // 获取城市中的防御将军ID数组

        city.ClearAllOfficeGeneral(); // 清空城市中的所有将军
        DefenseTurnToAttack(); // 处理城市所属国的变更

        for (int l1 = 0; l1 < attackGeneralNum; l1++)
            city.AddOfficeGeneralId(attackGeneralArray[l1]); // 将进攻方的将军ID添加到城市中

        city.prefectId = attackGeneralArray[0]; // 设置城市的 prefectId
        city.cityBelongKing = CityListCache.GetCityByCityId(AITargetCityId).cityBelongKing; // 更新城市的国王

        short takeThing = city.GetMoney(); // 获取城市当前金钱
        city.SetMoney((short)money); // 更新城市金钱
        money = takeThing; // 保存原有金钱

        takeThing = city.GetFood(); // 获取城市当前食物
        city.SetFood((short)carryFood); // 更新城市食物
        carryFood = takeThing; // 保存原有食物

        // 判断防御将军是否撤退成功
        bool flag = IsRetreat(defenseGeneralIdArray, defenseGeneralNum, kingId, carryFood, money);
        if (flag) // 如果撤退成功
        {
            if (country.GetHaveCityNum() > 0)
            {
                GameInfo.countryDieTips = 1;
                short newKingGeneralId = country.Inherit(); // 继承新国王
                string text = GeneralListCache.GetGeneral(defenseGeneralIdArray[0]).generalName + "死亡,新君主 " + GeneralListCache.GetGeneral(newKingGeneralId).generalName + " 继位!";
                TurnManager.PlayingState = GameState.AIInherit;
                yield return UITurnTips.GetInstance().ShowTurnTips(text);
                inheritGeneralId = newKingGeneralId; // 更新继位的将军ID
            }
            else
            {
                // 处理国家消亡后的逻辑
                byte[] tempCountryOrder = new byte[CountryListCache.countryOrder.Length - 1];
                int index = 0;
                for (int i = 0; i < CountryListCache.countryOrder.Length; i++)
                {
                    if (CountryListCache.countryOrder[i] != country.countryId)
                    {
                        if (index == CountryListCache.countryOrder.Length - 1)
                        {
                            Debug.LogError("系统错误!无法找到与countryId:" + country.countryId + "相同的势力编号");
                        }
                        else
                        {
                            tempCountryOrder[index] = CountryListCache.countryOrder[i];
                            index++;
                        }
                    }
                }
                chooseGeneralName = GeneralListCache.GetGeneral(country.countryKingId).generalName; // 设置消亡国家的国王名字
                CountryListCache.countryOrder = tempCountryOrder; // 更新国家顺序
                CountryListCache.removeCountry(country.countryId); // 移除国家
            }
        }
    }

    /// <summary>
    /// 处理将军撤退逻辑并判断是否成功撤退
    /// </summary>
    /// <param name="generalIdArray"></param>
    /// <param name="generalNum"></param>
    /// <param name="kingId"></param>
    /// <param name="food"></param>
    /// <param name="money"></param>
    /// <returns></returns>
    bool IsRetreat(short[] generalIdArray, byte generalNum, short kingId, int food, int money)
    {
        Country country = CountryListCache.GetCountryByKingId(kingId); // 根据国王ID获取国家
        City city = CityListCache.GetCityByCityId(AIUseCityId); // 获取当前城市
        short[] tempGeneralIdArray = new short[generalNum]; // 临时将军ID数组
        byte tempGeneralNum = 0; // 临时将军数量
        bool chiefGeneralCaptured = false; // 标记主将是否被俘获

        // 遍历所有将军ID
        for (int i = 0; i < generalNum; i++)
        {
            short generalId = generalIdArray[i];
            if (generalId == country.countryKingId) // 判断将军是否为国王
            {
                tempGeneralIdArray[tempGeneralNum] = generalId;
                tempGeneralNum = (byte)(tempGeneralNum + 1);
                continue; // 跳过后续操作
            }

            if (city.getCityGeneralNum() > 9) // 如果城市中的将军数量大于9
            {
                tempGeneralIdArray[tempGeneralNum] = generalId;
                tempGeneralNum = (byte)(tempGeneralNum + 1);
                continue; // 跳过后续操作
            }

            byte[] citys = country.GetCities(); // 获取国家的所有城市
            General general = GeneralListCache.GetGeneral(generalIdArray[i]); // 获取将军对象

            // 判断国家是否只剩下当前城市
            if (citys.Length < 1 || (citys.Length == 1 && citys[0] == AIUseCityId))
            {
                // 根据将军和国王的 phase 值计算是否俘获
                if (GeneralListCache.GetdPhase(GeneralListCache.GetGeneral(kingId).phase, general.phase) > 20)
                {
                    general.CapturedGeneralTo(AIUseCityId); // 将将军俘获
                    if (i == 0)
                    {
                        chiefGeneralCaptured = true; // 标记主将被俘获
                        Debug.Log("主将：" + general.generalName + "被俘获！！！！");
                    }
                    else
                    {
                        Debug.Log("武将：" + general.generalName + "被俘获！！！！");
                    }
                    continue;
                }
            }
            else if (general.getCurPhysical() <= 40 && general.generalSoldier <= 0 && GeneralListCache.GetdPhase(GeneralListCache.GetGeneral(kingId).phase, general.phase) > 15)
            {
                byte capturedProbability;

                // 根据将军的 IQ、force 和 lead 值计算被俘几率
                if (general.IQ >= 95 || general.force >= 95 || general.lead >= 95)
                {
                    capturedProbability = 5;
                }
                else if (general.IQ >= 90 || general.force >= 90 || general.lead >= 95)
                {
                    capturedProbability = 15;
                }
                else if (general.IQ >= 80 || general.force >= 80 || general.lead >= 95)
                {
                    capturedProbability = 25;
                }
                else
                {
                    capturedProbability = 40;
                }

                // 随机判断是否俘获将军
                if (UnityEngine.Random.Range(0, 100) <= capturedProbability)
                {
                    general.CapturedGeneralTo(AIUseCityId); // 将将军俘获
                    if (i == 0)
                    {
                        chiefGeneralCaptured = true; // 标记主将被俘获
                        Debug.Log("主将：" + general.generalName + "被俘获！！！！");
                    }
                    else
                    {
                        Debug.Log("武将：" + general.generalName + "被俘获！！！！");
                    }
                    continue;
                }
            }
            tempGeneralIdArray[tempGeneralNum] = generalId; // 将将军ID添加到临时数组
            tempGeneralNum = (byte)(tempGeneralNum + 1);
            continue;
        }

        bool masterRetreat = false; // 标记主将是否撤退成功
        if (tempGeneralNum > 0)
        {
            if (generalIdArray.Length > tempGeneralNum)
            {
                short[] tempOfficeGeneralId = new short[tempGeneralNum];
                Array.Copy(tempGeneralIdArray, 0, tempOfficeGeneralId, 0, tempGeneralNum); // 复制临时将军数组
                generalIdArray = tempOfficeGeneralId; // 更新将军数组
            }
            masterRetreat = country.RetreatGeneralToCity(generalIdArray, AIUseCityId, food, money, chiefGeneralCaptured); // 将将军撤退到城市
        }

        if (!masterRetreat) // 如果主将没有成功撤退
        {
            city.AddFood((short)food); // 给城市添加食物
            city.AddMoney((short)money); // 给城市添加金钱
            if (kingId == generalIdArray[0])
                return true; // 如果国王是第一个撤退的将军，返回true
        }

        return false; // 返回false
    }

    void moniAtk2(short attackGeneralId, short preventGeneralId)
    {
        City city = CityListCache.GetCityByCityId(AIUseCityId);
        bool isPrefectId = (city.prefectId == preventGeneralId);
        short soldier1 = (GeneralListCache.GetGeneral(attackGeneralId)).generalSoldier;
        short soldier2 = (GeneralListCache.GetGeneral(preventGeneralId)).generalSoldier;

        if (soldier1 > 0 && soldier2 > 0)
        {
            HandleBattle(attackGeneralId, preventGeneralId, isPrefectId, soldier1, soldier2);
        }
        else if (soldier1 > 0)
        {
            HandleAttackWithSoldiersLeft(attackGeneralId, preventGeneralId, isPrefectId, soldier1, 0);
        }
        else if (soldier2 > 0)
        {
            HandleAttackWithSoldiersLeft(preventGeneralId, attackGeneralId, isPrefectId, soldier2, 1);
        }
        else
        {
            HandleBothGeneralsNoSoldiers(attackGeneralId, preventGeneralId);
        }
    }

    void HandleBattle(short attackGeneralId, short preventGeneralId, bool isPrefectId, short soldier1, short soldier2)
    {
        int power1 = CalculatePower(attackGeneralId, isPrefectId);
        int power2 = CalculatePower(preventGeneralId, isPrefectId, true);
        int sword1 = power1 * soldier1;
        int sword2 = power2 * soldier2;

        if (sword1 > sword2)
        {
            ResolveBattleOutcome(attackGeneralId, preventGeneralId, sword2, power1, power2);
        }
        else
        {
            ResolveBattleOutcome(preventGeneralId, attackGeneralId, sword1, power2, power1);
        }
    }

    int CalculatePower(short generalId, bool isPrefectId, bool isPrevent = false)
    {
        General general = GeneralListCache.GetGeneral(generalId);
        int power = general.getSatrapValue();
        if (isPrefectId)
        {
            power = (int)(power * (isPrevent ? 1.33D : 1.0D));
        }
        return MoniAtkgetGenPower(power, !isPrevent, generalId);
    }

    void HandleAttackWithSoldiersLeft(short attackingGeneralId, short defendingGeneralId, bool isPrefectId, short soldiersLeft, int attackerType)
    {
        int power1 = CalculatePower(attackingGeneralId, isPrefectId);
        int power2 = GetGenSinglePower(defendingGeneralId);
        int sword1 = power1 * soldiersLeft;

        byte phy = Cangetphy(defendingGeneralId);
        int sword2 = (attackerType == 0 ? power2 * phy * 3 : power2 * phy * 2);

        if (sword1 > sword2)
        {
            int dea1 = sword2 / power1;
            ChangeSoldier(attackingGeneralId, (short)dea1);
            GeneralListCache.GetGeneral(defendingGeneralId).setCurPhysical((byte)UnityEngine.Random.Range(35, 41));
            GeneralListCache.addforceExp(defendingGeneralId, (byte)(dea1 / 50));
        }
        else
        {
            GeneralListCache.GetGeneral(attackingGeneralId).generalSoldier = 0;
            byte physical = (byte)(sword1 / power2 * (attackerType == 0 ? 3 : 2));
            GeneralListCache.GetGeneral(defendingGeneralId).decreaseCurPhysical(physical);
            if (GeneralListCache.GetGeneral(defendingGeneralId).getCurPhysical() < 1)
                GeneralListCache.GetGeneral(defendingGeneralId).setCurPhysical((byte)1);
            GeneralListCache.addforceExp(defendingGeneralId, (byte)(soldiersLeft / 50));
        }
    }

    void HandleBothGeneralsNoSoldiers(short attackGeneralId, short preventGeneralId)
    {
        int power1 = CalculateGeneralPower(attackGeneralId);
        int power2 = CalculateGeneralPower(preventGeneralId);

        byte phy1 = GeneralListCache.GetGeneral(attackGeneralId).getCurPhysical();
        byte phy2 = GeneralListCache.GetGeneral(preventGeneralId).getCurPhysical();

        if (power1 * phy1 > power2 * phy2)
        {
            ResolveGeneralNoSoldiersOutcome(attackGeneralId, preventGeneralId, power1, phy1, power2, phy2);
        }
        else if (power1 * phy1 == power2 * phy2)
        {
            GeneralListCache.GetGeneral(attackGeneralId).setCurPhysical((byte)UnityEngine.Random.Range(10, 15));
            GeneralListCache.GetGeneral(preventGeneralId).setCurPhysical((byte)UnityEngine.Random.Range(10, 15));
            GeneralListCache.addforceExp(attackGeneralId, (byte)5);
            GeneralListCache.addforceExp(preventGeneralId, (byte)5);
        }
        else
        {
            ResolveGeneralNoSoldiersOutcome(preventGeneralId, attackGeneralId, power2, phy2, power1, phy1);
        }
    }

    int CalculateGeneralPower(short generalId)
    {
        return (GeneralListCache.GetGeneral(generalId)).force +
               (GeneralListCache.GetGeneral(generalId)).force *
               ((WeaponListCache.GetWeapon((GeneralListCache.GetGeneral(generalId)).weapon)).weaponProperties +
               (WeaponListCache.GetWeapon((GeneralListCache.GetGeneral(generalId)).armor)).weaponProperties) / 100;
    }

    void ResolveBattleOutcome(short winnerId, short loserId, int swordLoss, int winnerPower, int loserPower)
    {
        int dea1 = swordLoss / winnerPower;
        ChangeSoldier(winnerId, (short)dea1);
        GeneralListCache.AddExp_P(winnerId, loserId, dea1);
        GeneralListCache.AIWarAddEXP2(winnerId, loserId, (short)dea1);
    }

    void ResolveGeneralNoSoldiersOutcome(short winnerId, short loserId, int winnerPower, byte winnerPhy, int loserPower, byte loserPhy)
    {
        GeneralListCache.GetGeneral(loserId).setCurPhysical((byte)10);
        byte x = (byte)((winnerPower * winnerPhy - loserPower * loserPhy) / winnerPower);
        GeneralListCache.GetGeneral(winnerId).decreaseCurPhysical(x);
        if (GeneralListCache.GetGeneral(winnerId).getCurPhysical() < 10)
            GeneralListCache.GetGeneral(winnerId).setCurPhysical((byte)10);
        GeneralListCache.addforceExp(winnerId, (byte)10);
        GeneralListCache.addforceExp(loserId, (byte)2);
    }

    int MoniAtkgetGenPower(int power, bool ists, short genId)
    {
        // 根据将领的士兵数量调整攻击力
        long gjl_jq = (1 + power * power * power / 100000);
        if (ists && GeneralListCache.GetGeneral(genId).generalSoldier <= 500)
            gjl_jq = Math.Min(100L, gjl_jq);

        return (int)gjl_jq;
    }

    int GetGenSinglePower(short id)
    {
        // 计算将领的单个战斗力
        int power = GeneralListCache.GetGeneral(id).force * 2 +
                    GeneralListCache.GetGeneral(id).force * WeaponListCache.GetWeapon(GeneralListCache.GetGeneral(id).weapon).weaponProperties / 100 +
                    GeneralListCache.GetGeneral(id).force * WeaponListCache.GetWeapon(GeneralListCache.GetGeneral(id).armor).weaponProperties / 100;
        long p = (1 + power * power * power / 100000);

        return (int)p;
    }

    byte Cangetphy(short id)
    {
        // 获取将领的当前体力
        byte phy;
        if (GeneralListCache.GetGeneral(id).getCurPhysical() > 35)
        {
            int ph = UnityEngine.Random.Range(0, GeneralListCache.GetGeneral(id).getCurPhysical() + 30);
            if (ph >= GeneralListCache.GetGeneral(id).getCurPhysical())
                ph = GeneralListCache.GetGeneral(id).getCurPhysical() - 35;

            phy = (byte)ph;
        }
        else
        {
            phy = 1;
        }

        return phy;
    }

    void ChangeSoldier(short genId, short num)
    {
        // 减少将领的士兵数量
        GeneralListCache.GetGeneral(genId).generalSoldier -= num;
        // 确保士兵数量不会为负数
        if (GeneralListCache.GetGeneral(genId).generalSoldier < 0)
            GeneralListCache.GetGeneral(genId).generalSoldier = 0;
    }
    

    private void AiStudy()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        if (country.AiFindCanStudyGeneral())
        {
            Debug.Log($"{kingName}太想进步了,于是组织了菁英大学习");
        }
    }










    private void AiTreat() 
    {
        if (AiFindLowHpGeneral()!=null)  // 如果可以进行治疗
        {
            AiTreat(AiFindLowHpGeneral());  // 执行治疗操作
            Debug.Log("执行治疗");
        }

        
    }

    /// <summary>
    /// 处理AiFindLowHpEnemyGeneral方法
    /// </summary>
    /// <returns></returns>
    short[] AiFindLowHpGeneral()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId); // 获取当前国家
        byte[] cities = country.cities.ToArray(); // 获取势力城市ID数组
                                                  // 遍历城市
        for (int i = 0; i < cities.Length; i++)
        {
            City city = CityListCache.GetCityByCityId(cities[i]); // 获取城市对象
            short[] officeGeneralIdArray = city.GetCityOfficeGeneralIdArray(); // 获取城市办公厅将领ID数组

            List<short> treatGeneralId = new List<short>();
            if (city.GetMoney() >= 100) // 如果城市金钱大于等于100
            {
                if (city.GetLowHPGeneralIds().Count > 0)
                    treatGeneralId.AddRange(city.GetLowHPGeneralIds()); // 获取将领ID
                if (treatGeneralId.Count > 0)
                {
                    return treatGeneralId.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }
        return null; // 如果找到了合适的将领，返回true
    }

    /// <summary>
    /// AI执行治疗操作
    /// </summary>
    void AiTreat(short[] treatGeneralIds)
    {
        for (int i = 0; i < treatGeneralIds.Length; i++)
        {
            byte physical = (byte)AiTreatValue();  // 获取随机治疗效果
            General general = GeneralListCache.GetGeneral(AIUseGeneralId);  // 获取当前操作的将领
            general.addCurPhysical(physical);  // 增加将领的当前体力
        }
        CityListCache.GetCityByCityId(AIUseCityId).DecreaseMoney(50);  // 从城市的金钱中扣除 50
    }

    /// <summary>
    /// 随机返回一个治疗效果值
    /// </summary>
    /// <returns></returns>
    int AiTreatValue()
    {
        return UnityEngine.Random.Range(35, 52);  // 返回 35 到 51 的随机值
    }


    private IEnumerator AiUprising()
    {
        Country country = CountryListCache.GetCountryByCountryId(curTurnsCountryId);
        if (country.RebelCity()!=0)
        {
            City city = CityListCache.GetCityByCityId(country.RebelCity());
            short prefectId = city.prefectId;  // 获取城市太守ID
            General general = GeneralListCache.GetGeneral(prefectId);  // 获取将领对象
            Country oldCountry = CountryListCache.GetCountryByKingId(city.cityBelongKing);  // 获取旧国家
            oldCountry.RemoveCity(city.cityId);  // 从旧国家移除该城市
            Country newCountry = new Country();  // 创建新国家
            newCountry.countryId = (byte)(CountryListCache.GetCountrySize() + 1);  // 设置新国家ID
            newCountry.countryKingId = general.generalId;  // 设置新国家的国王ID
            city.prefectId = general.generalId;  // 设置城市的太守为该将领
            newCountry.AddCity(city.cityId);  // 新国家添加城市
            AITargetGeneralId = general.generalId;  // 更新AI将领ID
            TurnManager.PlayingState= GameState.Rebel;
            TurnTipsInfo();  // 处理事件
            CountryListCache.AddCountry(newCountry);  // 将新国家添加到国家缓存
            byte[] tempCountryOrder = new byte[CountryListCache.countryOrder.Length + 1];  // 创建临时数组以包含新国家
            for (int j = 0; j < CountryListCache.countryOrder.Length; j++)
                tempCountryOrder[j] =  CountryListCache.countryOrder[j];  // 复制旧国家顺序
            tempCountryOrder[tempCountryOrder.Length - 1] = newCountry.countryId;  // 添加新国家
            CountryListCache.countryOrder = tempCountryOrder;  // 更新国家顺序
            string text = general.generalName + "在" + city.cityName + "起义！";
            yield return UITurnTips.GetInstance().ShowTurnTips(text);
            Debug.Log(text);  // 输出起义日志
        }
    }

    private void TurnTipsInfo()
    {
    }
}*/