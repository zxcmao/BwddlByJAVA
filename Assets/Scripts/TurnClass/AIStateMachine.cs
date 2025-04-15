using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClass;
using DataClass;
using UIClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using War;
using static DataClass.GameInfo;
using Random = UnityEngine.Random;

namespace TurnClass
{
    public class AIStateMachine
    {
        private AIState currentState;
        private byte curTurnsCountryId;
        private Country _curCountry; //当前AI的势力
        private byte _usedOrderNum;//当前AI消耗的命令数
        private byte orderNum;//当前AI总命令数
        private byte _warCount = 2;
        public bool AIFinish;
        private UITips uiTips;
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
            uiTips = GameObject.Find("Canvas").GetComponent<UIGlobe>().tips;
            currentState = AIState.Idle;
            curTurnsCountryId = countryId;
            _curCountry = CountryListCache.GetCountryByCountryId(countryId);
            orderNum = CountryListCache.GetAIOredrNum(countryId);
            AIFinish = false;
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
    }*/
        
        
        public IEnumerator AIDoOrder()
        {
            while (_usedOrderNum < orderNum)  // 当AI未执行完所有指令时
            {
                _usedOrderNum++;
                yield return AiAlliance();
                yield return AiInterior();
                AiSearch();
                AIStore();
                AiConscription();
                AiDefence();
                AiTransportMoney();
                if (AIJudgeReward(out General rewardGen, out City rewardCity))
                {
                    AIDoReward(rewardGen, rewardCity);
                }
                AiTreat();
                if (_usedOrderNum < orderNum && _warCount > 0)  // 如果AI还有指令未执行且还有战争次数
                {
                    yield return AiAttack();
                    AiConscription();  // AI进行额外的征兵
                    AiTreat();  // 执行战后治疗操作
                }
                AiStudy();  // 进行学习操作
            
                if (AIJudgeReward(out General rewardGen2, out City rewardCity2))
                {
                    AIDoReward(rewardGen2, rewardCity2);
                }
                //AiInterior(curTurnsCountryId);  // 继续进行内政处理
                Debug.Log($"{_curCountry.KingName()}执行第{_usedOrderNum}次命令");
            }
            AIFinish = true;
            Debug.Log($"{_curCountry.KingName()}执行完所有命令");
        } 

        private IEnumerator AiAlliance()
        {
            Debug.Log($"{_curCountry.KingName()}正分析天下形势");
            // 获取该国家拥有的城市数量
            byte haveCityNum = _curCountry.GetHaveCityNum();
            // 如果城市数量大于3，则无需结盟
            if (haveCityNum > 3)
                yield break;

            int threats = 0;  // 记录与玩家势力相邻的城池

            // 遍历所有城市
            foreach (var cityId in _curCountry.cityIDs)
            {
                // 获取城市对象
                City city = CityListCache.GetCityByCityId(cityId);
                // 获取该城市连接的城市ID
                byte[] connectCityId = city.connectCityId;

                // 遍历连接的城市
                if (connectCityId.Select(t => CityListCache.GetCityByCityId(t).cityBelongKing)
                    .Select(CountryListCache.GetCountryByKingId)
                    .Any(otherCountry => otherCountry != null && otherCountry.countryId == GameInfo.playerCountryId))
                {
                    threats++;  // 增加结盟计数
                }
            }

            // 如果没有找到玩家势力相邻的城池且随机值大于10，则无需结盟
            if (threats == 0 && Random.Range(0, 100) > 10)
                yield break;

            // 计算结盟阈值
            byte gl = (byte)(8 - Mathf.Pow(2, haveCityNum - 1) + CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).GetHaveCityNum());
            int k = Random.Range(0, 100);
            gl = (byte)(gl + threats);

            // 如果结盟阈值小于随机值，则无需结盟
            if (gl < k)
                yield break;

            // 获取没有结盟的国家ID数组
            byte[] noAllianceCountryId = _curCountry.GetNoCountryIdAllianceCountryIdArray();
            // 如果没有没有结盟的国家，则返回
            if (noAllianceCountryId.Length < 1)
                yield break;

            Country allianceCountry = null;  // 记录要结盟的国家
            int dPhase = 0;  // 记录最小的相位差
            // 获取当前国家国王
            General countryKing = GeneralListCache.GetGeneral(_curCountry.countryKingId);

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
            if (dPhase + allianceCountry.GetHaveCityNum() <= Random.Range(0, 75))
            {
                // 执行结盟操作
                _curCountry.AddAlliance(allianceCountry.countryId);
                // 输出结盟成功的日志
                string text = countryKing.generalName + "势力与" + allianceCountry.KingName() + "达成同盟！";
                Debug.Log(text);
                // 更新游戏信息显示
                yield return uiTips.ShowTurnTips(text, GameState.AITruce);
            }
        }

        //AI执行内政操作
        private IEnumerator AiInterior()
        {
            Debug.Log($"{_curCountry.KingName()}正在治理城池");
            // 随机选择一个城市进行内政计算
            byte cityId = _curCountry.cityIDs[Random.Range(0, _curCountry.cityIDs.Count)];
            int i = Random.Range(0, 13);
            City city = CityListCache.GetCityByCityId(cityId);
            General general = AiFindExecutiveOfficer(city);

            // 城市资金不足时，优先选择特定策略
            if (city.GetMoney() < 100)
                i = 7;

            // 根据随机结果选择内政行动
            switch (i)
            {
                case 0:
                    if (city.floodControl < 99)
                        AiTameOrder(city, general);
                    break;
                case 1:
                    if (city.agro < 999)
                        AiReclaimOrder(city, general);
                    break;
                case 2:
                    if (city.trade < 999)
                        AiMercantileOrder(city, general);
                    break;
                case 3:
                case 4:
                    if (city.population < 990000)
                    {
                        General friendlyGeneral = AIFindFriendlyOfficer(city);
                        AiPatrolOrder(city, friendlyGeneral);
                    }
                    break;
                case 5:
                case 6:
                    yield return AiJudgeBribe(); // 执行特定行为
                    break;
                case 7:
                case 8:
                    AiDoSearchEmploy(city); // 执行招揽行动
                    break;
            }
        }
        
        /// <summary>
        /// AI获取城市中智力和政治综合能力最高的将领
        /// </summary>
        /// <returns></returns>
        static General AiFindExecutiveOfficer(City city)
        {
            short[] officerIds = city.GetOfficerIds();
            int bestScore = 0;
            General best = GeneralListCache.GetGeneral(city.prefectId);
            foreach (var id in officerIds)
            {
                General general = GeneralListCache.GetGeneral(id);
                int score = general.IQ + general.political * 2;
                if (general.HasSkill(3, 2) || general.HasSkill(3, 3))
                {   // 如果将领具备特技屯田商才，能力值加强
                    score = (int)(score * 1.33f);
                }
                else if (general.HasSkill(3, 0))
                {   // 如果将领具备特技王佐
                    score = (int)(score * 1.25f);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = general;
                }
            }
            return best;
        }

        // AI获取最佳治理将领
        static General AIFindFriendlyOfficer(City city)
        {
            short[] officerIds = city.GetOfficerIds();
            int bestScore = 0;
            General best = GeneralListCache.GetGeneral(city.prefectId);
            foreach (var id in officerIds)
            {
                General general = GeneralListCache.GetGeneral(id);
                int score = general.IQ + general.political * 2 + general.moral * 2;
                if (general.HasSkill(3, 1))
                {   // 如果将领具备特技仁政，能力值加强
                    score = (int)(score * 1.33f);
                }
                else if (general.HasSkill(3, 0))
                {   // 如果将领具备特技王佐
                    score = (int)(score * 1.25f);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = general;
                }
            }
            return best;
        }






        /// <summary>
        /// 自动重建AI灾后所有城市的内政
        /// </summary>
        public static void AIRebuildCity()
        {
            foreach (var city in CityListCache.cityDictionary.Values)
            {
                // 如果城市的国王ID大于0且不属于玩家国家的国王
                if (city.cityBelongKing > 0 && city.cityBelongKing != (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId)).countryKingId)
                {
                    byte cityNum = CountryListCache.GetCountryByKingId(city.cityBelongKing).GetHaveCityNum();

                    // 如果当前国家的城市数量小于等于10
                    if (cityNum <= 10)
                    {
                        // 如果城市的资金少于600，则增加一定的资金
                        if (city.GetMoney() < 600)
                            city.AddGold((short)(30 + Random.Range(0, 6000 / cityNum)));

                        // 如果城市的食物少于城市所有士兵数量的 6 * 60 倍
                        if (city.GetFood() < city.GetCityAllSoldierNum() / 1000 * 6 * 60)
                            city.AddFood((short)(150 + Random.Range(0, 6000 / cityNum)));
                    }
                    else if (city.GetMoney() < 100)
                    {
                        // 如果城市的资金少于100，则增加一定的资金
                        city.AddGold((short)Random.Range(100, 300));
                    }

                    byte minLoyalty = 100; // 初始最大忠诚度
                    General minGeneral = null;  // 忠诚度最低的将军ID
                    short[] officerIds = city.GetOfficerIds();

                    // 遍历城市中所有将军
                    foreach (var id in officerIds)
                    {
                        General general = GeneralListCache.GetGeneral(id);

                        // 如果将军的忠诚度低于50，则提升忠诚度
                        if (general.GetLoyalty() < 50)
                        {
                            general.RewardAddLoyalty(true);
                        }
                        // 如果当前将军的忠诚度低于最小忠诚度，则更新最小忠诚度
                        else if (minLoyalty > general.GetLoyalty())
                        {
                            minGeneral = general;
                            minLoyalty = general.GetLoyalty();
                        }
                    }
                    // 如果最小忠诚度低于90，则提升对应将军的忠诚度
                    if (minGeneral != null && minLoyalty < 90)
                        minGeneral.RewardAddLoyalty(true);

                    // 如果城中有学院，则调用AIDoLearn方法
                    if (city.citySchool)
                        StudyOfAllGeneral(city);

                    // 如果城中有医馆，则对每个将军进行体力恢复
                    if (city.cityHospital)
                    {
                        foreach (var id in officerIds)
                        {
                            General general = GeneralListCache.GetGeneral(id);
                            if (general.GetCurPhysical() < general.maxPhysical)
                            {
                                byte addPhysical = (byte)(general.maxPhysical - general.GetCurPhysical());
                                general.AddCurPhysical(addPhysical);
                            }
                        }
                    }

                    // 如果城市的资金大于等于30
                    if (city.GetMoney() >= 30)
                    {
                        General general = AiFindExecutiveOfficer(city); // 获取将军ID
                        // 如果城市的防洪控制小于99，则调用AiTameOrder方法
                        if (city.floodControl < 99)
                        {
                            var useGlod = general.GetNeedMoneyOfInterior(TaskType.Tame);
                            city.Reclaim(general, useGlod);
                        }

                        // 如果城市的资金仍然大于等于30
                        if (city.GetMoney() >= 30)
                        {
                            if (cityNum > 10)
                            {
                                if (officerIds.Length > 1)
                                {
                                    if (Random.Range(0, city.GetCityOfficerNum()) > 0)
                                        AutoInteriorLogic(city); // 调用AutoInteriorLogic方法
                                }
                                else if (Random.Range(0, 60) > cityNum * 2)
                                {
                                    AutoInteriorLogic(city); // 调用AutoInteriorLogic方法
                                }
                            }
                            else if (officerIds.Length > 1)
                            {
                                if (Random.Range(0, city.GetCityOfficerNum()) > 0)
                                    AutoInteriorLogic(city); // 调用AutoInteriorLogic方法
                            }
                            else if (Random.Range(0, 3) > 0)
                            {
                                AutoInteriorLogic(city); // 调用AutoInteriorLogic方法
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
            foreach (var city in CityListCache.cityDictionary.Values)
            {
                if (city.cityBelongKing > 0 && city.cityBelongKing != (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId)).countryKingId)
                {
                    byte cityNum = CountryListCache.GetCountryByKingId(city.cityBelongKing).GetHaveCityNum();
                    if (cityNum is > 0 and <= 6)
                    {
                        if (city.GetMoney() < 600)
                            city.AddGold((short)(30 + Random.Range(0, 600 / cityNum)));
                        if (city.GetFood() < city.GetCityAllSoldierNum() / 1000 * 6 * 30)
                            city.AddFood((short)(150 + Random.Range(0, 600 / cityNum)));
                    }
                    else if (city.GetMoney() < 100)
                    {
                        city.AddGold((short)Random.Range(100, 300));
                    }

                    short[] officerIds = city.GetOfficerIds();
                    byte minLoyalty = 100;
                    General minGeneral = null;
                    foreach (var id in officerIds)
                    {
                        General general = GeneralListCache.GetGeneral(id);
                        if (general.GetLoyalty() < 50)
                        {
                            general.RewardAddLoyalty(false);
                        }
                        else if (minLoyalty > general.GetLoyalty())
                        {
                            minGeneral = general;
                            minLoyalty = general.GetLoyalty();
                        }
                    }
                    if (minGeneral != null && minLoyalty < 90)
                    {
                        minGeneral.RewardAddLoyalty(false);
                    }

                    if (city.citySchool)
                        StudyOfAllGeneral(city);

                    if (city.cityHospital)
                        foreach (var id in officerIds)
                        {
                            General general = GeneralListCache.GetGeneral(id);
                            if (general.GetCurPhysical() < general.maxPhysical)
                            {
                                byte addPhysical = (byte)(general.maxPhysical- general.GetCurPhysical());
                                general.AddCurPhysical(addPhysical);
                            }
                        }

                    if (city.GetMoney() >= 30)
                    {
                        General general = AiFindExecutiveOfficer(city); // 获取将军ID
                        // 如果城市的防洪控制小于99，则调用AiTameOrder方法
                        if (city.floodControl < 99)
                            AiReclaimOrder(city, general);
                        if (city.GetMoney() >= 30)
                            if (cityNum > 6)
                            {
                                if (officerIds.Length > 1)
                                {
                                    if (Random.Range(0, city.GetCityOfficerNum()) > 0)
                                        AutoInteriorLogic(city);
                                }
                                else if (Random.Range(0, 40) > cityNum * 2)
                                {
                                    AutoInteriorLogic(city);
                                }
                            }
                            else if (officerIds.Length > 1)
                            {
                                if (Random.Range(0, city.GetCityOfficerNum()) > 0)
                                    AutoInteriorLogic(city);
                            }
                            else if (Random.Range(0, 3) > 1)
                            {
                                AutoInteriorLogic(city);
                            }
                    }
                }
            }
        }

        /// <summary>
        /// AI自动选择内政策略
        /// </summary>
        static void AutoInteriorLogic(City city)
        {
            General general = AiFindExecutiveOfficer(city);
            // 优先进行治水
            if (city.floodControl < 99)
            {
                AiTameOrder(city, general);
                return;
            }

            // 决定优先发展农业或贸易
            if (Random.Range(0, 3) > 1)
            {
                if (city.agro < 999 && city.trade < 999)
                {
                    if (GameInfo.month < 4 || GameInfo.month >= 10)
                    {
                        AiMercantileOrder(city, general);
                    }
                    else
                    {
                        AiReclaimOrder(city, general);
                    }
                    return;
                }
                if (city.agro < 999)
                {
                    AiReclaimOrder(city, general);
                    return;
                }
                if (city.trade < 999)
                {
                    AiMercantileOrder(city, general);
                    return;
                }
            }

            // 人口未达上限则优先发展人口
            if (city.population < 990000)
            {
                general = AIFindFriendlyOfficer(city);
                AiPatrolOrder(city, general);
                return;
            }

            // 否则继续治水
            AiTameOrder(city, general);
        }

        /// <summary>
        /// AI内政开垦查操作
        /// </summary>
        /// <param name="city"></param>
        /// <param name="general"></param>
        static void AiReclaimOrder(City city, General general)
        {
            int needMoney = general.GetNeedMoneyOfInterior(TaskType.Reclaim); // 获取内政开垦需要的金钱
            city.Reclaim(general, needMoney);  // 执行相关操作
        }

        /// <summary>
        /// AI内政劝商操作
        /// </summary>
        /// <param name="city"></param>
        /// <param name="general"></param>
        static void AiMercantileOrder(City city, General general)
        {
            int needMoney = general.GetNeedMoneyOfInterior(TaskType.Mercantile);  // 获取内政劝商需要的金钱
            city.Mercantile(general, needMoney);  // 执行相关操作
        }

        /// <summary>
        /// AI内政治水操作
        /// </summary>
        /// <param name="city"></param>
        /// <param name="general"></param>
        static void AiTameOrder(City city, General general)
        {
            int needMoney = general.GetNeedMoneyOfInterior(TaskType.Tame);  // 获取内政治水需要的金钱
            city.Tame(general, needMoney);  // 执行相关操作
        }

        /// <summary>
        /// Ai内政巡查操作
        /// </summary>
        /// <param name="city"></param>
        /// <param name="general"></param>
        static void AiPatrolOrder(City city, General general)
        {
            int needMoney = general.GetNeedMoneyOfInterior(TaskType.Patrol);  // 获取内政巡查需要的金钱
            city.Patrol(general, needMoney);  // 执行相关操作
        }

        /// <summary>
        /// Ai判断笼络操作
        /// </summary>
        /// <returns></returns>
        IEnumerator AiJudgeBribe()
        {
            short doGenId = 0;
            short beGenId = 0;
            byte beCityId = 0;
            byte doCityId = 0;
            int val = 0;
            
            // 遍历所有城市
            foreach (var otherCity in CityListCache.cityDictionary.Values)
            {
                if (otherCity.cityBelongKing != _curCountry.countryKingId)  // 判断是否为敌方城市
                {
                    short[] otherOfficerIds = otherCity.GetOfficerIds();

                    foreach (var cityId in _curCountry.cityIDs)
                    {
                        City ownCity = CityListCache.GetCityByCityId(cityId);

                        if (ownCity.GetCityOfficerNum() <= 9)  // 如果城市的将领数量不超过9
                        {
                            short[] ownOfficerIds = ownCity.GetOfficerIds();

                            foreach (var id in ownOfficerIds)
                            {
                                foreach (var otherId in otherOfficerIds)
                                {
                                    int per = AiBribeProbability(id, otherId);  // 计算招揽成功率

                                    if (per > 0)
                                    {
                                        General otherGeneral = GeneralListCache.GetGeneral(otherId);
                                        int curval = (otherGeneral.lead * 3 + otherGeneral.force + otherGeneral.IQ) * per;

                                        if (curval > val)
                                        {
                                            beGenId = otherId;
                                            doGenId = id;
                                            beCityId = otherCity.cityId;
                                            doCityId = cityId;
                                            val = curval;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // 执行招揽操作
            if (beGenId != 0 && doCityId != 0 && doGenId != 0)
            {
                if (GeneralListCache.IsBribe(doCityId, beCityId, doGenId, beGenId))  // 判断招揽成功
                {
                    yield return AiBribe(doCityId, beCityId, doGenId, beGenId);
                    yield break;
                }
                yield return AiAlienate(beCityId, doGenId, beGenId);  // 如果招揽失败，执行离间操作
            }
        }

        /// <summary>
        /// AI计算笼络将领的概率
        /// </summary>
        /// <param name="doGenId"></param>
        /// <param name="beGenId"></param>
        /// <returns></returns>
        int AiBribeProbability(short doGenId, short beGenId)
        {
            General beGeneral = GeneralListCache.GetGeneral(beGenId);  // 获取被招揽的将领
            General goGeneral = GeneralListCache.GetGeneral(doGenId);  // 获取招揽的将领

            if (beGeneral.GetLoyalty() == 100)  // 如果忠诚度为100，直接返回0
                return 0;

            General beKing = GeneralListCache.GetGeneral(beGeneral.GetOfficeGenBelongKing());  // 被招揽将领的君主
            General doKing = GeneralListCache.GetGeneral(goGeneral.GetOfficeGenBelongKing());  // 招揽将领的君主相性

            int d1 = GeneralListCache.GetPhaseDifference(beKing, beGeneral);  // 计算相性差距
            int d2 = GeneralListCache.GetPhaseDifference(doKing, beGeneral);    // 计算相性差距
            int d3 = GeneralListCache.GetPhaseDifference(goGeneral, beGeneral);  // 招揽将领与被招揽将领的相性差距

            if (d1 == 0)
                return 0;
            if (d2 == 0)
                return 1000;

            int val = d1 - d2 - d3 * 2 + 100 - beGeneral.GetLoyalty();  // 计算招揽成功的几率
            if (val > 0)
                return val * 20;

            // 随机数判断是否招揽成功
            if (Random.Range(0, 120) < goGeneral.IQ - beGeneral.IQ)
                return (goGeneral.IQ - beGeneral.IQ) / 2;

            return 0;
        }
        

        /// <summary>
        /// AI笼络玩家将领
        /// </summary>
        /// <param name="doCityId"></param>
        /// <param name="beCityId"></param>
        /// <param name="doGenId"></param>
        /// <param name="beGenId"></param>
        IEnumerator AiBribe(byte doCityId, byte beCityId, short doGenId, short beGenId)
        {
            if (CityListCache.GetCityByCityId(beCityId).cityBelongKing == CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId)
            {
                string generalName = GeneralListCache.GetGeneral(beGenId).generalName;
                string cityName = CityListCache.GetCityByCityId(beCityId).cityName;
                string tarCityName = CityListCache.GetCityByCityId(doCityId).cityName;
                string text = $"{generalName}背信弃义,竟不惜从{cityName}逃往{tarCityName}向{_curCountry.KingName()}献媚";
                yield return uiTips.ShowTurnTips(text, GameState.AIBribe);
            }
        }

        /// <summary>
        /// Ai离间玩家将领
        /// </summary>
        /// <param name="beCityId">被离间者城池</param>
        /// <param name="doGenId">离间者ID</param>
        /// <param name="beGenId">被离间者ID</param>
        IEnumerator AiAlienate(byte beCityId, short doGenId, short beGenId)
        {
            // 如果成功且目标城市属于玩家的国家
            if (GeneralListCache.IsAlienate(doGenId, beGenId))
            {
                City beCity = CityListCache.GetCityByCityId(beCityId);
                if (beCity.cityBelongKing == CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId)
                {
                    string text = $"{GeneralListCache.GetGeneral(beGenId).generalName}正与{GeneralListCache.GetGeneral(doGenId)}在{beCity.cityName}暗中密谈";
                    yield return uiTips.ShowTurnTips(text, GameState.AIAlienate);
                }
            }
        }

    
        // AI某个城池搜索并登用过程
        void AiDoSearchEmploy(City city)
        {
            try
            {
                List<short> talentIds = city.GetTalentIds();
                // 如果城市将领数量少于10，尝试进行招揽
                if (city.GetCityOfficerNum() < 10 && talentIds.Count > 0)
                {
                    short id = talentIds[Random.Range(0, talentIds.Count)];
                    if (id <= 0)
                        return;

                    short generalId = city.GetMostMoralGeneralInCity();
                    city.IsEmploy(generalId, id);
                }

                short 回收武将 = city.GetMostIqMoralGeneralInCity();
                city.Search(回收武将);

            }
            catch (Exception exception)
            {
                Debug.Log(exception); // 捕获异常，防止程序崩溃
            }
        }
   

        /// <summary>
        /// AI全国范围登用搜索将领
        /// </summary>
        void AiSearch()
        {
            // 随机50%几率进行搜索
            if (Random.Range(0, 100) < 50)
                return;
            Debug.Log($"{_curCountry.KingName()}正在搜罗人才");

            // 遍历国家所有城市，寻找可以登用的将领
            foreach (var cityID in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityID);
                List<short> talentIds = city.GetTalentIds();
                if (city.GetCityOfficerNum() < 10 && talentIds.Count > 0)
                {
                    short generalId = talentIds[Random.Range(0, talentIds.Count)];
                    short employGeneralId = city.GetDoSearchGen(generalId);
                    if (city.IsEmploy(employGeneralId, generalId))
                        return;
                }
            }
            // 再次遍历，处理剩余的搜索
            foreach (var cityID in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityID);
                if (city.GetReservedGeneralNum() > 0)
                {
                    short generalId = city.GetMostIqMoralGeneralInCity();
                    city.Search(generalId);
                }
            }
        }

        // 必须钱粮交易
        private bool AIStore()
        {
            List<byte> adjacentCity = GetEnemyAdjacentCityIds();
            bool flag = false;

            if (!HaveGrainShop())
                return flag;

            foreach (var cityId in adjacentCity)
            {
                if (cityId <= 0)
                    break;

                City city = CityListCache.GetCityByCityId(cityId);
                int needFood = city.NeedFoodToHarvest() + city.NeedFoodWarAMonth();

                if (city.GetFood() < needFood && city.GetMoney() > 200)
                {
                    int buyNum = needFood - city.GetFood();
                    buyNum = Mathf.Min(30000 - city.GetFood(), buyNum);

                    if (buyNum >= 100 || city.GetFood() <= 100)
                    {
                        if (buyNum > city.GetMoney() * 4 / 3)
                        {
                            short buy = (short)((city.GetMoney() - 50) * 4 / 3);
                            city.AddFood(buy);
                            city.SetMoney(50);
                            Debug.Log($"{city.cityName}买入{buy}石粮食+{buy}只剩50两黄金");
                        }
                        else
                        {
                            city.AddFood(buyNum);
                            city.SubGold(buyNum * 3 / 4);
                            Debug.Log($"{city.cityName}花费{buyNum * 3 / 4}两黄金买入粮食{buyNum}石");
                        }
                        flag = true;
                    }
                }
            }

            foreach (var cityId in adjacentCity)
            {
                if (cityId <= 0)
                    break;

                City city = CityListCache.GetCityByCityId(cityId);
                int needFood = city.NeedFoodToHarvest() + city.NeedFoodWarAMonth();

                if (city.GetFood() > needFood + 200)
                {
                    int sellNum = city.GetFood() - needFood - 200;
                    sellNum = Mathf.Min(30000 - city.GetMoney(), sellNum);

                    if (sellNum >= 50)
                    {
                        city.SubFood(sellNum);
                        city.AddGold(sellNum * 3 / 4);
                        Debug.Log($"{city.cityName}卖出{sellNum}石粮食得到黄金{sellNum * 3 / 4}两");
                        flag = true;
                    }
                }
                else if (city.GetMoney() <= 100)
                {
                    int needMoney = city.NeedAllSalariesMoney();
                    int needSellFood = needMoney * 4 / 3;

                    if (needSellFood >= city.GetFood() / 2)
                        return flag;

                    city.AddGold(needMoney);
                    city.SubFood(needSellFood);
                    Debug.Log($"{city.cityName}卖出{needSellFood}石粮食得到黄金+{needMoney}两");
                }
            }

            List<byte> noAdjacentCity = GetNoEnemyAdjacentCityIds();
            foreach (var cityId in noAdjacentCity)
            {
                if (cityId <= 0)
                    break;

                City city = CityListCache.GetCityByCityId(cityId);
                int needFood = city.NeedFoodToHarvest();

                if (city.GetFood() > needFood + 50)
                {
                    int sellNum = city.GetFood() - needFood - 50;
                    sellNum = Mathf.Min(30000 - city.GetMoney(), sellNum);

                    if (sellNum >= 50)
                    {
                        city.SubFood((short)sellNum);
                        city.AddGold((short)(sellNum * 3 / 4));
                        Debug.Log($"{city.cityName} -{sellNum}粮食售得黄金+{(sellNum * 3 / 4)}");
                        flag = true;
                    }
                }
                else if (city.GetMoney() <= 100)
                {
                    int needMoney = city.NeedAllSalariesMoney();
                    int needSellFood = needMoney * 4 / 3;

                    if (needSellFood >= city.GetFood() / 2)
                        return flag;

                    city.AddGold(needMoney);
                    city.SubFood(needSellFood);
                    Debug.Log($"{city.cityName} - {needSellFood}粮食售得黄金+{needMoney}");
                }
            }

            return flag;
        }

        // 判断是否有粮店的方法
        public bool HaveGrainShop()
        {
            // 遍历势力内的所有城市
            foreach (byte cityId in _curCountry.cityIDs)
            {
                // 根据城市ID从缓存中获取城市对象
                City cityFromCache = CityListCache.GetCityByCityId(cityId);

                // 检查城市的k_byte_array1d_fld字段是否包含粮店标志（0x8）
                if (cityFromCache.cityGrainShop)
                {
                    return true;  // 如果有粮店，返回true
                }
            }
            return false;  // 如果没有粮店，返回false
        }
        
        /// <summary>
        /// 获取与敌对势力相邻的城市ID列表
        /// </summary>
        /// <returns></returns>
        List<byte> GetEnemyAdjacentCityIds()
        {
            // 将列表转换为数组并返回
            return _curCountry.cityIDs.Where(IsCityNearEnemy).ToList();
        }



        /// <summary>
        /// 获取不与敌对势力相邻的内部城市ID列表
        /// </summary>
        /// <returns></returns>
        List<byte> GetNoEnemyAdjacentCityIds()
        {
            // 获取与敌对势力相邻的城市ID数组
            List<byte> enemyAdjacentCityIdArray = GetEnemyAdjacentCityIds();

            // 将列表转换为数组并返回
            return _curCountry.cityIDs.Except(enemyAdjacentCityIdArray).ToList();;
        }
        
        // AI 自动征兵
        void AiConscription()
        {
            byte maxCityId = 0;  // 记录人口最多的城市 ID
            int maxPopulation = 0;  // 记录最大人口
        
            foreach (var cityID in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityID);  // 根据 ID 获取城市实例

                // 如果城市人口超过 1000 且大于当前记录的最大人口，更新最大人口和目标城市 ID
                if (city.population > 1000 && city.population > maxPopulation)
                {
                    maxPopulation = city.population;
                    maxCityId = cityID;
                }
            }

            // 如果没有符合条件的城市或者最大人口少于 1000，返回
            if (maxCityId == 0 || maxPopulation < 1000)
                return;

            foreach (var cityID in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityID);  // 获取当前城市实例

                int maxSoldierNum = city.GetMaxSoldierNum();  // 获取城市的总士兵数
                int cityAllSoldierNum = maxSoldierNum - city.GetCityAllSoldierNum();  // 获取城市非驻守士兵数
                int needSoldierNum = cityAllSoldierNum - city.cityReserveSoldier;  // 计算需要补充的士兵数

                // 如果需要补充士兵
                if (needSoldierNum > 0)
                {
                    int needMoney = needSoldierNum / 5 ;  // 计算征兵所需的金钱
                    City thisCity = CityListCache.GetCityByCityId(maxCityId);  // 获取人口最多的城市实例

                    // 如果当前城市有足够的金钱进行征兵
                    if (city.GetMoney() >= needMoney)
                    {
                        city.SubGold(needMoney);  // 减少城市金钱
                        city.cityReserveSoldier += needMoney * 5;  // 增加城市的预备士兵
                        thisCity.SubPopulation(needSoldierNum);  // 减少人口最多城市的人口
                    }
                    else  // 如果金钱不足
                    {
                        int maxConscript = city.GetMoney() * 5;  // 根据金钱计算可征兵的数量
                        city.SubGold(city.GetMoney());  // 减少城市金钱
                        city.cityReserveSoldier += maxConscript;  // 增加城市的预备士兵
                        thisCity.SubPopulation(maxConscript);  // 减少人口最多城市的人口
                    }
                }
                // 分配士兵到将领
                city.AssignSoldier();
            }
        }

        private bool AiDefence()
        {
            bool isMove = false;

            // 获取最需要战斗力的城市
            City maxNeedPowerCity = GetMostNeedPowerCity();
            // 获取敌方相邻城市的ID数组
            List<byte> adjacentCityIds = GetEnemyAdjacentCityIds();
            // 获取没有敌人相邻的城市ID数组
            List<byte> noEnemyAdjacentCityIds = GetNoEnemyAdjacentCityIds();
            // 如果没有找到这样一个城市
            if (maxNeedPowerCity == null)
            {
                if (adjacentCityIds.Count <= 0)
                    return isMove;
                
                // 遍历所有没有敌人相邻的城市
                foreach (byte cityId in noEnemyAdjacentCityIds)
                {
                    City city = CityListCache.GetCityByCityId(cityId);

                    // 遍历所有敌方相邻城市
                    foreach (byte otherCityId in adjacentCityIds)
                    {
                        City dangerousCity = CityListCache.GetCityByCityId(otherCityId);

                        // 如果城市将领数量 <= 1 或者与敌方相邻城市的将领数量 > 9
                        if (city.GetCityOfficerNum() <= 1 || dangerousCity.GetCityOfficerNum() > 9)
                        {
                            if (ChangeGeneral(city, dangerousCity))
                                isMove = true;
                        }
                        else
                        {
                            if (AddGeneralToOtherCity(city, dangerousCity))
                                isMove = true;
                        }
                    }
                }
                return isMove;
            }


            if (noEnemyAdjacentCityIds.Count > 0)
            {
                // 遍历没有敌人相邻的城市
                foreach (byte cityId in noEnemyAdjacentCityIds)
                {
                    City city = CityListCache.GetCityByCityId(cityId);

                    // 如果城市将领数量 <= 1 或者 maxNeedPowerCity 的将领数量 > 9
                    if (city.GetCityOfficerNum() <= 1 || maxNeedPowerCity.GetCityOfficerNum() > 9)
                    {
                        if (ChangeGeneral(city, maxNeedPowerCity))
                            isMove = true;
                    }
                    else
                    {
                        if (AddGeneralToOtherCity(city, maxNeedPowerCity))
                            isMove = true;
                    }
                }
            }
            else
            {
                foreach (byte adjacentCityId in adjacentCityIds)
                {
                    if (adjacentCityId != maxNeedPowerCity.cityId)
                    {
                        City adjacentCity = CityListCache.GetCityByCityId(adjacentCityId);
                        byte generalNum = adjacentCity.GetCityOfficerNum();
                        int defenseAbility = adjacentCity.GetDefenseAbility();
                        short minBattlePowerGeneralId = adjacentCity.GetMinBattlePowerGeneralId();
                        General minBattlePowerGeneral = GeneralListCache.GetGeneral(minBattlePowerGeneralId);
                        double battlePower = minBattlePowerGeneral.GetBattlePower();
                        int enemyAdjacentAtkPower = CountryListCache.GetEnemyAdjacentMaxAtkPower(adjacentCityId);

                        // 如果城市将领数量 > 1 且 maxNeedPowerCity 的将领数量 < 9
                        if (generalNum > 1 && maxNeedPowerCity.GetCityOfficerNum() < 9)
                        {
                            if (defenseAbility - battlePower >= enemyAdjacentAtkPower)
                            {
                                maxNeedPowerCity.AddOfficeGeneralId(minBattlePowerGeneralId);
                                maxNeedPowerCity.AppointmentPrefect();
                                adjacentCity.RemoveOfficerId(minBattlePowerGeneralId);
                                adjacentCity.AppointmentPrefect();
                                isMove = true;
                                Debug.Log($"{adjacentCity.cityName} 的战力过大, 将武将: {minBattlePowerGeneral.generalName} 移动至 {maxNeedPowerCity.cityName}");
                            }
                        }
                        else
                        {
                            short needMinBattlePowerGeneralId = maxNeedPowerCity.GetMinBattlePowerGeneralId();
                            General needMinBattlePowerGeneral = GeneralListCache.GetGeneral(needMinBattlePowerGeneralId);
                            double needBattlePower = needMinBattlePowerGeneral.GetBattlePower();

                            if (needBattlePower < battlePower && defenseAbility - battlePower + needBattlePower >= enemyAdjacentAtkPower)
                            {
                                ChangeGeneral(maxNeedPowerCity, adjacentCity, needMinBattlePowerGeneralId,  minBattlePowerGeneralId);
                                Debug.Log($"{maxNeedPowerCity.cityName} 的 {needMinBattlePowerGeneral.generalName} 与 {adjacentCity.cityName} 的 {minBattlePowerGeneral.generalName} 交换。");
                                isMove = true;
                            }
                        }
                    }
                }
            }

            return isMove;
        }

        // 获取最需要战斗力的城市
        City GetMostNeedPowerCity()
        {
            // 获取敌对邻近城市ID数组
            List<byte> enemyAdjacentCityIds = GetEnemyAdjacentCityIds();
            int maxNeedPower = 0;
            City moveCity = null;

            foreach (byte adjacentCityId in enemyAdjacentCityIds)
            {
                City adjacentCity = CityListCache.GetCityByCityId(adjacentCityId);
                if (adjacentCity.GetCityOfficerNum() <= 9)
                {
                    int defenseAbility = adjacentCity.GetDefenseAbility();
                    int enemyAdjacentAtkPower = CountryListCache.GetEnemyAdjacentMaxAtkPower(adjacentCityId);
                    int needPower = enemyAdjacentAtkPower - defenseAbility;
                    if (needPower > maxNeedPower)
                    {
                        maxNeedPower = needPower;
                        moveCity = adjacentCity;
                    }
                }
            }

            return moveCity;
        }
        
        /// <summary>
        ///  更换将领
        /// </summary>
        /// <param name="doCity">后备城池</param>
        /// <param name="beCity">危险城池</param>
        /// <returns></returns>
        private bool ChangeGeneral(City doCity, City beCity)
        {
            // 获取其他城市中战斗力最小的武将的ID
            short minBattlePowerGeneralId = beCity.GetMinBattlePowerGeneralId();
            // 根据ID从缓存中获取武将对象
            General minBattlePowerGeneral = GeneralListCache.GetGeneral(minBattlePowerGeneralId);
            // 获取该武将的战斗力
            double minBattlePower = minBattlePowerGeneral.GetBattlePower();

            // 获取当前城市中战斗力最大的武将的ID
            short maxBattlePowerGeneralId = doCity.GetMaxBattlePowerGeneralId();
            // 根据ID从缓存中获取武将对象
            General battlePowerGeneral = GeneralListCache.GetGeneral(maxBattlePowerGeneralId);
            // 获取该武将的战斗力
            double battlePower = battlePowerGeneral.GetBattlePower();

            // 如果当前城市中战斗力最大的武将的战斗力大于其他城市中战斗力最小的武将
            if (battlePower > minBattlePower)
            {
                // 调换武将，将当前城市的战斗力最大武将与其他城市的战斗力最小武将交换
                ChangeGeneral(doCity, beCity, maxBattlePowerGeneralId,  minBattlePowerGeneralId);

                // 输出日志，记录武将调换信息
                Debug.Log($"{doCity.cityName} 的 {battlePowerGeneral.generalName} 与 {beCity.cityName} 的 {minBattlePowerGeneral.generalName} 进行了交换。");

                // 返回 true，表示交换成功
                return true;
            }

            // 如果不满足交换条件，则返回 false
            return false;
        }

        /// <summary>
        /// 交换城池将军
        /// </summary>
        /// <param name="doCity">出发城池</param>
        /// <param name="beCity">目的城池</param>
        /// <param name="doGenId">出发武将</param>
        /// <param name="beGenId">回收武将</param>
        void ChangeGeneral(City doCity, City beCity, short doGenId, short beGenId)
        {

            // 如果目的城市的武将数量小于2
            if (beCity.GetCityOfficerNum() < 2)
            {
                // 将出发武将添加到目的城池
                beCity.AddOfficeGeneralId(doGenId);
                // 从目的城市移除回收武将留个位置
                beCity.RemoveOfficerId(beGenId);

                // 如果出发城池的武将数量小于10
                if (doCity.GetCityOfficerNum() < 10)
                {
                    // 将回收武将添加到出发城池
                    doCity.AddOfficeGeneralId(beGenId);
                    // 从出发城池移除出发武将
                    doCity.RemoveOfficerId(doGenId);
                }
                else// 如果出发城池的武将数量大于等于10
                {
                    // 先从出发城池移除出发武将
                    doCity.RemoveOfficerId(doGenId);
                    // 后将回收武将添加到出发城池
                    doCity.AddOfficeGeneralId(beGenId);
                }
            }
            else// 如果目的城市的武将数量大于等于2
            {
                // 从目的城池移除出发武将
                beCity.RemoveOfficerId(beGenId);
                // 将回收武将添加到目的城池
                beCity.AddOfficeGeneralId(doGenId);

                // 如果出发城池的武将数量小于10
                if (doCity.GetCityOfficerNum() < 10)
                {
                    // 将出发武将添加到出发城池
                    doCity.AddOfficeGeneralId(beGenId);
                    // 从出发城池移除回收武将
                    doCity.RemoveOfficerId(doGenId);
                }
                else
                {
                    // 从出发城池移除回收武将
                    doCity.RemoveOfficerId(doGenId);
                    // 将出发武将添加到出发城池
                    doCity.AddOfficeGeneralId(beGenId);
                }
            }

            // 任命目的城池的太守
            beCity.AppointmentPrefect();
            // 任命出发城池的太守
            doCity.AppointmentPrefect();
        }



        // 向其他城市添加将领
        private bool AddGeneralToOtherCity(City doCity, City beCity)
        {
            short minBattlePowerGeneralId = doCity.GetMinBattlePowerGeneralId();
            bool isMove = false;

            while (true)
            {
                short generalId = doCity.GetMaxBattlePowerGeneralId();

                if (doCity.GetCityOfficerNum() == 1 || generalId == minBattlePowerGeneralId || beCity.GetCityOfficerNum() >= 10)
                {
                    return isMove;
                }

                // 移除将领
                doCity.RemoveOfficerId(generalId);

                // 输出移动信息
                Debug.Log($"将{doCity.cityName}的{GeneralListCache.GetGeneral(generalId).generalName}移动至{beCity.cityName}");

                // 添加将领到其他城市
                beCity.AddOfficeGeneralId(generalId);

                isMove = true;
            }
        }

        // 检查城市是否邻近敌方城市
        bool IsCityNearEnemy(byte cityId)
        {
            // 获取城市对象
            City city = CityListCache.GetCityByCityId(cityId);
            // 初始化是否有敌方城市标志
            bool haveEnemyCity = false;

            foreach (byte connectCityId in city.connectCityId)
            {
                // 获取邻接城市的君主ID
                short belongKing = CityListCache.GetCityByCityId(connectCityId).cityBelongKing;
                // 如果邻接城市的君主ID与当前势力不同
                if (belongKing != _curCountry.countryKingId)
                {
                    // 获取邻接城市的所属势力
                    Country otherCountry = CountryListCache.GetCountryByKingId(belongKing);
                    // 如果邻接城市的所属势力存在且不是同盟国
                    if (otherCountry != null && otherCountry.GetAllianceById(otherCountry.countryId) == null)
                    {
                        // 设置有敌方城市标志并退出循环
                        haveEnemyCity = true;
                        break;
                    }
                }
            }

            return haveEnemyCity;
        }

        private bool AiTransportMoney()
        {
            if (_curCountry.IsEndangered()) return false;
            City bestNeedMoneyCity = null;
            int bestNeedMoney = 0;
            City bestRichMoneyCity = null;
            int bestRichMoney = 0;

            foreach (var cityId in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityId);
                int needFood = 0;
                int needMoney = 0;
                bool isNearEnemy = IsCityNearEnemy(cityId);

                if (isNearEnemy)
                {
                    needFood = city.NeedFoodToHarvest() + city.NeedFoodWarAMonth();
                    needMoney += (int)(needMoney + city.GetMaxSoldierNum() * 0.2 + (city.NeedAllSalariesMoney() * 12));
                }
                else
                {
                    needFood = city.NeedFoodToHarvest();
                    needMoney += city.NeedAllSalariesMoney() * 12;
                }

                if (city.GetFood() < needFood)
                {
                    needMoney = (needFood - city.GetFood()) * 3 / 4;
                }

                if (city.GetMoney() < needMoney)
                {
                    if (needMoney - city.GetMoney() > bestNeedMoney)
                    {
                        bestNeedMoney = needMoney - city.GetMoney();
                        bestNeedMoneyCity = city;
                    }
                }
                else if (city.GetMoney() - needMoney > bestRichMoney)
                {
                    bestRichMoney = city.GetMoney() - needMoney;
                    bestRichMoneyCity = city;
                }
            }

            if (bestNeedMoneyCity != null && bestRichMoneyCity != null)
            {
                int transportMoney = (bestRichMoney > bestNeedMoney) ? bestNeedMoney : bestRichMoney;
                bestRichMoneyCity.SubGold(transportMoney);
                bestNeedMoneyCity.AddGold(transportMoney);
                Debug.Log($"{bestRichMoneyCity.cityName} 运输 {transportMoney} 金钱到 {bestNeedMoneyCity.cityName}");
                return true;
            }

            return false;
        }


        /// <summary>
        /// Ai检测低忠诚度将领方法
        /// </summary>
        /// <param name="general">需要奖赏的将领</param>
        /// <param name="doCity">找到的将领所在城市</param>
        /// <returns>是否找到符合条件的将领</returns>
        bool AIJudgeReward(out General general, out City doCity)
        {
            int maxGeneralScore = 0; // 初始化最大将领得分
            general = null; // 初始化 out 参数
            doCity = null; // 初始化 out 参数

            foreach (var cityID in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityID); // 获取城市对象
                // 检查城市金钱或宝物数量
                if (city.GetMoney() >= 50 || city.treasureNum != 0)
                {
                    short[] officeGeneralIdArray = city.GetOfficerIds(); // 获取城市办公厅将领ID数组
                    // 遍历城市中的将领
                    foreach (var id in officeGeneralIdArray)
                    {
                        General tempGeneral = GeneralListCache.GetGeneral(id); // 获取将领对象
                        int generalScore = tempGeneral.AllStatus(); // 获取将领得分
                        // 检查将领忠诚度和城市的金钱/宝物条件
                        if (generalScore > maxGeneralScore)
                        {
                            if ((tempGeneral.GetLoyalty() < 90 && city.GetMoney() > 50 && city.GetMoney() / 50 > Random.Range(0, 4)) ||
                                (tempGeneral.GetLoyalty() >= 90 && city.treasureNum > 0 && city.treasureNum > Random.Range(0, 4)))
                            {
                                maxGeneralScore = generalScore; // 更新最大得分
                                general = tempGeneral; // 设置目标将领
                                doCity = city; // 设置目标城市
                            }
                        }
                    }
                }
            }
            if (general != null && doCity != null) // 如果没有找到合适的将领和城池
                return true;

            return false; // 返回true
        }

        /// <summary>
        /// Ai奖赏将领忠诚度或减少目标城市的宝藏或金钱
        /// </summary>
        void AIDoReward(General general, City city)
        {
            // 如果将领的忠诚度大于 90 且城市有宝藏
            if (general.GetLoyalty() > 90)
            {
                if (city.treasureNum > 0)
                {
                    general.RewardAddLoyalty(false);  // 增加忠诚度但标记为不友好
                    city.treasureNum = (byte)(city.treasureNum - 1);  // 减少城市的宝藏数量
                    return;
                }
            }
            // 否则如果城市金钱大于 50
            else if (city.GetMoney() > 50)
            {
                general.RewardAddLoyalty(true);  // 增加忠诚度且标记为友好
                city.SubGold((short)50);  // 减少城市的金钱
            }
            // 如果城市没有足够的金钱但有宝藏
            else if (city.treasureNum > 0)
            {
                general.RewardAddLoyalty(false);  // 增加忠诚度但标记为不友好
                city.treasureNum = (byte)(city.treasureNum - 1);  // 减少城市的宝藏
            }
        }




        private IEnumerator AiAttack() 
        {  
            byte num = (orderNum - _usedOrderNum > _warCount) ? _warCount : (byte)(orderNum - _usedOrderNum);  // 计算当前战争次数
            
            if (AIThinkWar(out City atkCity, out City defCity))  // 如果AI决定战争且符合随机条件
            {
                if (WarRandom(num, atkCity, defCity))
                {
                    _warCount = (byte)(_warCount - 1);  // 减少战争次数
                    yield return AIStartWar(atkCity, defCity);  // 开始战争
                }
               
            }
        }
        
        /// <summary>
        /// AI判断是否发动战争
        /// </summary>
        /// <param name="atkCity">AI自身城池</param>
        /// <param name="defCity">目标城池</param>
        /// <returns></returns>
        bool AIThinkWar(out City atkCity, out City defCity)
        {
            int minMustPower = 0;
            //bool result = false;
            atkCity = null;  // 初始化目标城市
            defCity = null; 
            List<byte> enemyAdjacentCity = GetEnemyAdjacentCityIds();
            foreach (var cityID in enemyAdjacentCity)
            {
                City ownCity = CityListCache.GetCityByCityId(cityID);
                int rate = 100 * ownCity.GetAlreadySoldierNum() / ownCity.GetMaxSoldierNum();  // 计算士兵比例
                int random = Random.Range(0, 101);  // 随机获取一个数值用于判断
                if (random <= rate)  // 根据概率判断是否发动战争
                {
                    int maxAtkPower = (int)(ownCity.GetMaxAtkPower() * (0.5D + 0.5D * _warCount));  // 计算最大攻击力
                    int maxEnemyAtkPower = (int)(CountryListCache.GetEnemyAdjacentMaxAtkPower(cityID) * 0.65D);  // 计算敌人的最大攻击力
                    if (maxAtkPower > maxEnemyAtkPower)  // 如果我方攻击力大于敌方
                    {
                        List<byte> enemyCityIds = CountryListCache.GetAdjacentEnemyCityIds(cityID);  // 获取敌方相邻城市
                        foreach (var enemyCityId in enemyCityIds)
                        {
                            City enemyCity = CityListCache.GetCityByCityId(enemyCityId);
                            if (GameInfo.isWatch)  // 如果当前为观察模式，跳过玩家所属城市
                            {
                                if (enemyCity.cityBelongKing != 0 && CountryListCache.GetCountryByKingId(enemyCity.cityBelongKing).countryId == GameInfo.playerCountryId)
                                    continue;
                            }
                            int defenseAbility = CountryListCache.GetEnemyAdjacentCityDefenseAbility(enemyCity, ownCity);  // 获取敌方防御能力
                            if (maxAtkPower - maxEnemyAtkPower > defenseAbility && minMustPower < maxAtkPower - maxEnemyAtkPower - defenseAbility)
                            {
                                minMustPower = maxAtkPower - maxEnemyAtkPower - defenseAbility;  // 更新最小必需攻击力
                                atkCity = ownCity;
                                defCity = enemyCity;
                                return true;
                            }
                            continue;
                        }
                    }
                }
            }
            return false;  // 返回是否发动战争的判断结果
        }

        /// <summary>
        /// 战争随机决策函数，根据战争能力计算是否可以胜利
        /// </summary>
        /// <param name="warNum"></param>
        /// <param name="atkCity"></param>
        /// <param name="defCity"></param>
        /// <returns></returns>
        bool WarRandom(byte warNum, City atkCity, City defCity)
        {
            if (atkCity == null || defCity == null) // 如果城池为空，直接返回
                return false;
            
            if (defCity.cityBelongKing == 0) // 如果是空城池，战争胜利
                return true;
            int defPower = defCity.GetDefenseAbility(); // 获取防御能力
            int atkPower = atkCity.GetMaxAtkPower(); // 获取自身城市的最大攻击能力
            double warAbility = (5 * warNum + GetWarAbility(atkPower, defPower)); // 计算战争能力
            int random = Random.Range(0, 100); // 获取0到100的随机数
            if (random <= warAbility) // 如果随机数小于等于战争能力，战争胜利
                return true;

            if ((CountryListCache.GetCountryByKingId(defCity.cityBelongKing)).countryId == GameInfo.playerCountryId) // 如果城市所属国家是玩家国家
            {
                random = Random.Range(0, 100); // 再次获取随机数
                if (random <= warAbility)
                    return true; // 如果随机数仍然小于战争能力，战争胜利
            }
            return false; // 否则战争失败
        }

        /// <summary>
        /// 计算战争能力值，根据攻击力与防御力的比率得出战争能力
        /// </summary>
        /// <param name="atkPower"></param>
        /// <param name="defPower"></param>
        /// <returns></returns>
        private byte GetWarAbility(int atkPower, int defPower)
        {
            byte ability = 0;
            if (atkPower >= 2 * defPower) // 攻击力是防御力的两倍或以上
            {
                ability = 80;
            }
            else if (atkPower >= 1.67D * defPower)
            {
                ability = 70;
            }
            else if (atkPower >= 1.33D * defPower)
            {
                ability = 60;
            }
            else if (atkPower >= defPower)
            {
                ability = 50;
            }
            else if (atkPower >= 0.84D * defPower)
            {
                ability = 40;
            }
            else if (atkPower >= 0.68D * defPower)
            {
                ability = 30;
            }
            else if (atkPower >= 0.52D * defPower)
            {
                ability = 20;
            }
            else if (atkPower >= 0.36D * defPower)
            {
                ability = 10;
            }
            return ability; // 返回战争能力值
        }

        // 发起战争
        IEnumerator AIStartWar(City atkCity, City defCity)
        {
            int needFood = 0;
            int needMoney = 0;
            
            // 计算目标敌方城市周边的敌方城池的最大攻击力
            int otherEnemyAdjacentAtkPower = CountryListCache.GetMaxAtkPowerInEnemyCities(defCity, atkCity);
            otherEnemyAdjacentAtkPower = (int)(otherEnemyAdjacentAtkPower * 0.3D); // 取30%的值

            // 获取战事办公室中的将领 ID 数组
            List<short> generalIds = atkCity.GetWarOfficeGeneralIds(otherEnemyAdjacentAtkPower);

            // 如果没有将领可以参与战争，直接返回
            if (generalIds.Count == 0)
                yield break;

            // 移除战事将领
            foreach (var t in generalIds)
                atkCity.RemoveOfficerId(t);

            // 重新任命太守
            atkCity.AppointmentPrefect();

            // 计算所需粮食
            needFood = NeedFoodValue(atkCity, generalIds);

            // 获取当前城市所属国王的 ID
            short defKingId = defCity.cityBelongKing;

            // 计算所需金钱，若金钱少于50，设为0，否则取一半
            if (atkCity.GetMoney() < 50)
            {
                needMoney = 0;
            }
            else
            {
                needMoney = atkCity.GetMoney() / 2;
            }

            // 如果当前城市没有国王，减少所需粮食和金钱
            if (defKingId == 0)
            {
                needFood /= 5;
                needMoney /= 5;
            }

            // 减少目标城市的金钱和粮食
            atkCity.SubGold(needMoney);
            atkCity.SubFood(needFood);

            // 垃圾回收
            GC.Collect();

           
            if (defKingId == 0)
            {
                AIOccupyEmptyCity(defCity, generalIds, needFood, needMoney);
                string text = $"{_curCountry.KingName()}军占领了空城{defCity.cityName}";// 更新显示信息
                yield return uiTips.ShowTurnTips(text, GameState.AIOccupy);
                UIGlobe.UpdateCityButtonColor(defCity.cityId, _curCountry.countryColor);
            }
            else if (defKingId == (CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId)).countryKingId)
            {   
                // 判断防守方是否是玩家控制的国家
                string text = $"{_curCountry.KingName()}军前来攻打{defCity.cityName}...";// 更新显示信息
                yield return uiTips.ShowTurnTips(text, GameState.AIvsPlayer);
                WarManager.Instance.OnWarOver += WarManagerOnOnWarOver;
                targetCityId = defCity.cityId;
                doCityId = atkCity.cityId;
                targetGeneralIds = generalIds;
                optionalGeneralIds[0] = (short)needFood;
                optionalGeneralIds[1] = (short)needMoney;
                SceneManager.LoadScene("WarScene");
                yield break;
            }
            else 
            {   // 防守方是否是AI
                Country defCountry = CountryListCache.GetCountryByKingId(defKingId); // 获取敌方势力

                // 构建显示信息的字符串
                string str = "【";
                for (int i = 0; i < generalIds.Count && i < 3; i++)
                    str += $"{GeneralListCache.GetGeneral(generalIds[i]).generalName}、";

                // 移除最后一个 "、"
                str = str.Substring(0, str.Length - 1);
                str += "】";

                if (generalIds.Count > 3)
                    str += $"等{generalIds.Count}员大将";

                str = $"{_curCountry.KingName()}军从{atkCity.cityName}出动了{str}攻打{defCountry.KingName()}的{defCity.cityName}！";

                // 输出信息到控制台
                Debug.Log(str);

                // 更新游戏显示信息
                yield return uiTips.ShowTurnTips(str, GameState.AIvsAI);
                
                // 判断是否能够占领城市
                if (AIWar2(defCity, generalIds, needFood))
                {
                    string win = $"{_curCountry.KingName()}军占领了{defCity.cityName}！";
                    yield return uiTips.ShowTurnTips(win, GameState.AIWinAI);
                    UIGlobe.UpdateCityButtonColor(defCity.cityId, _curCountry.countryColor);
                    yield return IsDestroyed(defCity, generalIds, needFood, needMoney); // 处理战后事宜
                }
                else
                {   // 战争失败，进行相应处理
                    string lose = $"{_curCountry.KingName()}在{defCity.cityName}被{defCountry.KingName()}击败了!";
                    yield return uiTips.ShowTurnTips(lose, GameState.AILoseAI);
                    IsRetreat(defCity, generalIds, _curCountry.countryKingId, needFood, needMoney);
                }

                // 检查敌方国家是否已经灭亡
                if (defCountry.GetHaveCityNum() == 0)
                {
                    string fail = $"{defCountry.KingName()}势力被消灭了";
                    yield return uiTips.ShowTurnTips(fail, GameState.AIFail);
                }
                else if (defCountry.countryKingId != defKingId)
                {
                    string inherit = $"{defCountry.KingName()}继承了原{GeneralListCache.GetGeneral(defKingId).generalName}的势力！";
                    yield return uiTips.ShowTurnTips(inherit, GameState.AIInherit);
                    Debug.Log(inherit);
                }
            }

            yield return new WaitForSeconds(0.5f);

            // 垃圾回收
            GC.Collect();
        }

        private void WarManagerOnOnWarOver()
        {
            WarManager.Instance.OnWarOver -= WarManagerOnOnWarOver;
            AiConscription();
        }

        /// <summary>
        /// 计算所需食物的数量
        /// </summary>
        /// <param name="city"></param>
        /// <param name="generalIds"></param>
        /// <returns></returns>
        int NeedFoodValue(City city, List<short> generalIds)
        {
            int allGeneralSoldier = 0; // 初始化所有将军的士兵数量
            allGeneralSoldier = GetTotalSoldierNum(generalIds); // 计算所有将军的士兵数量
            allGeneralSoldier = allGeneralSoldier / 250 + 1; // 计算修正后的士兵数量

            // 计算所需食物数量
            int needFood = Random.Range(0, allGeneralSoldier * 30) + allGeneralSoldier * 30;
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
        short GetTotalSoldierNum(List<short> generalIds)
        {
            return generalIds.Aggregate<short, short>(0, (current, id) => (short)(current + GeneralListCache.GetGeneral(id).generalSoldier));
        }

        /// <summary>
        /// AI执行占领城市方法
        /// </summary>
        /// <param name="city">被占领的城池</param>
        /// <param name="generalIds">将领列表</param>
        /// <param name="food">粮食</param>
        /// <param name="gold">金钱</param>
        void AIOccupyEmptyCity(City city, List<short> generalIds, int food, int gold)
        {
            _curCountry.AddCity(city.cityId); // 将当前城市添加到国家
            foreach (var id in generalIds)
                city.AddOfficeGeneralId(id); // 将将军 ID 添加到城市中
            city.cityBelongKing = _curCountry.countryKingId; // 设置城市的国王
            city.prefectId = generalIds[0]; // 设置城市的 prefectId
            city.AddFood(food); // 添加食物
            city.AddGold(gold); // 添加资金
        }

        // 判断AI是否战胜另一个AI城池
        bool CanAIWin(City defCity, List<short> attackerIds, int food, int gold)
        {
            //AttackerWarPowerSum(generalIdArray, generalNum); // 计算己方武将的战斗力
            //DefenderWarPowerArray(city.GetOfficerIds()); // 计算敌方武将的战斗力

            // 若AI进行战争
            if (AIWar2(defCity, attackerIds, food))
            {
                IsDestroyed(defCity, attackerIds, food, gold); // 处理战后事宜
                return true; // 占领成功
            }
            
            // 战争失败，进行相应处理
            IsRetreat(defCity, attackerIds, defCity.cityBelongKing, food, gold);
            return false;
        }


        // 根据武将数组计算己方阵列的战斗力
        void AttackerWarPowerSum(short[] generalIdArray, byte generalNum)
        {
            int[]gfZL = new int[generalNum]; // 己方总战斗力
            int[]gfzdl = new int[generalNum]; // 己方单个武将战斗力
            int gfZZL = 1; // 己方总战斗力初始值

            for (byte i = 0; i < generalNum; i = (byte)(i + 1))
            {
                General general = GeneralListCache.GetGeneral(generalIdArray[i]); // 获取武将
                byte zl = general.IQ; // 智力

                // 计算武将战斗力 (多属性加成公式)
                int gjl = general.GetWarValue();

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
                    gfzdl[i] = Mathf.Min(100, gfzdl[i]);

                if (gfzdl[i] < 20)
                    gfzdl[i] = Mathf.Max(general.generalSoldier / 150, gfzdl[i]);

                // 总战斗力计算
                gfZL[i] = gfzdl[i];
                gfZL[i] = gfZL[i] * (general.generalSoldier + 1);
                gfZZL += gfZL[i];
            }
        }

        // 根据武将数组计算敌方阵列的战斗力
        void DefenderWarPowerArray(short[] generalIdArray)
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
        short AttackerEatFood(List<short> generalIds)
        {
            short eatNum = 3; // 初始消耗为3
            short atkTotleSoldier = GetTotalSoldierNum(generalIds); // 计算进攻方的总兵力
            eatNum = (short)(eatNum + atkTotleSoldier / 250); // 按兵力比例增加消耗
            return eatNum;
        }

        // 计算防守方的消耗粮草
        short DefenderEatFood(List<short> generalIds)
        {
            short eatNum = 1; // 初始消耗为1
            short defTotleSoldier = GetTotalSoldierNum(generalIds); // 计算防守方的总兵力
            eatNum = (short)(eatNum + defTotleSoldier / 250); // 按兵力比例增加消耗
            return eatNum;
        }

        // AI进行战争模拟
        bool AIWar2(City defCity, List<short> attackerIds, int atkFood)
        {
            bool occupy = false; // 是否占领
            bool atknot = false; // 进攻方是否无法进攻
            bool defnot = false; // 防守方是否无法防守
            byte day = 0; // 战斗持续天数
            List<short> defenderIds = new List<short>(defCity.GetOfficerIds()); // 防守方武将列表
            // 战斗模拟循环
            while (true)
            {
                day = (byte)(day + 1); // 天数增加
                atkFood -= AttackerEatFood(attackerIds); // 进攻方消耗粮草

                if (atkFood <= 0)
                {
                    atkFood = 0; // 粮草耗尽
                    break;
                }

                // 防守方消耗粮草
                defCity.SubFood(DefenderEatFood(defenderIds));

                // 若防守方粮草耗尽
                if (defCity.GetFood() <= 0)
                {
                    defCity.SetFood(0); // 设置防守方粮草为0
                    occupy = true; // 城市被占领
                    break;
                }

                if (day < 3) // 战斗持续时间小于3天继续
                    continue;

                if (day > 30) // 战斗持续时间超过30天结束
                    break;

                // 寻找进攻方的攻击武将
                General attacker = null; // 进攻方攻击武将初始化
                List<short> atkSequence = new List<short>(attackerIds);
                Shuffle(atkSequence); // 随机打乱进攻方武将顺序

                // 查找能够进攻的武将
                for (byte i = 0; i < atkSequence.Count; i++)
                {
                    General general = GeneralListCache.GetGeneral(atkSequence[i]); // 获取武将
                    int soldier = general.generalSoldier;
                    byte phy =general.GetCurPhysical();
                    if (soldier > 0 || phy > 50)
                    {
                        attacker = general; // 找到攻击武将
                        atknot = false;
                        break;
                    }
                    atknot = true; // 无法找到攻击武将
                }

                // 随机跳过本次战斗
                if (Random.Range(1, 101) < 15)
                    continue;

                if (atknot)
                    break; // 无法继续进攻

                // 寻找防守方的防守武将
                General defender = null;
                List<short> defenseSequence = new List<short>(defenderIds);
                Shuffle(defenseSequence); // 随机打乱防守方武将顺序
                
                // 查找能够防守的武将
                for (byte i = 0; i < defenseSequence.Count; i = (byte)(i + 1))
                {

                    General general = GeneralListCache.GetGeneral(defenseSequence[i]);
                    int soldier = general.generalSoldier;
                    byte phy = general.GetCurPhysical();
                    if (soldier > 0 || phy > 50)
                    {
                        defender = general; // 找到防守武将
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

                if (attacker != null && defender != null)
                    AiMoniAtk2(defCity, attacker, defender); // 模拟攻防战
            }

            return occupy; // 返回是否占领成功
        }
    
        /// <summary>
        /// 使用 UnityEngine.Random 实现 Fisher-Yates 洗牌算法
        /// </summary>
        /// <typeparam name="T">列表元素的类型</typeparam>
        /// <param name="list">要洗牌的列表</param>
        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                // 使用 UnityEngine.Random.Range 生成随机索引
                int randomIndex = Random.Range(0, i + 1);

                // 交换元素
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
        
        /// <summary>
        /// 处理势力灭亡逻辑
        /// </summary>
        /// <param name="defCity"></param>
        /// <param name="attackerIds"></param>
        /// <param name="food"></param>
        /// <param name="gold"></param>
        IEnumerator IsDestroyed(City defCity, List<short> attackerIds, int food, int gold)
        {
            short kingId = defCity.cityBelongKing; // 获取城市所属国王
            Country defCountry = CountryListCache.GetCountryByKingId(kingId); // 获取国家对象
            List<short> defenderIds = new List<short>(defCity.GetOfficerIds()); // 获取城市中的防御将军ID

            defCity.ClearAllOfficeGeneral(); // 清空城市中的所有将军
            defCountry.RemoveCity(defCity.cityId); // 处理城市所属国的变更
            _curCountry.AddCity(defCity.cityId);

            for (int i = 0; i < attackerIds.Count; i++)
                defCity.AddOfficeGeneralId(attackerIds[i]); // 将进攻方的将军ID添加到城市中

            defCity.prefectId = attackerIds[0]; // 设置城市的 prefectId
            defCity.cityBelongKing = _curCountry.countryKingId; // 更新城市的国王

            short takeThing = defCity.GetMoney(); // 获取城市当前金钱
            defCity.SetMoney(gold); // 更新城市金钱
            gold = takeThing; // 保存原有金钱

            takeThing = defCity.GetFood(); // 获取城市当前食物
            defCity.SetFood(food); // 更新城市食物
            food = takeThing; // 保存原有食物

            // 判断防御将军是否撤退成功
            bool flag = IsRetreat(defCity, defenderIds, kingId, food, gold);
            if (flag) // 如果撤退成功
            {
                if (defCountry.GetHaveCityNum() > 0)
                {
                    GameInfo.countryDieTips = 1;
                    short newKingGeneralId = defCountry.Inherit(); // 继承新国王
                    string text = GeneralListCache.GetGeneral(defenderIds[0]).generalName + "死亡,新君主 " + GeneralListCache.GetGeneral(newKingGeneralId).generalName + " 继位!";
                    yield return uiTips.ShowTurnTips(text, GameState.AIInherit);
                }
                else
                {
                    // 处理国家消亡后的逻辑
                    CountryListCache.RemoveCountry(defCountry.countryId);
                    chooseGeneralName = GeneralListCache.GetGeneral(defCountry.countryKingId).generalName; // 设置消亡国家的国王名字
                    CountryListCache.RemoveSequence(defCountry.countryId); // 更新国家顺序
                }
            }
        }

        /// <summary>
        /// 处理将军撤退逻辑并判断是否成功撤退还是灭亡
        /// </summary>
        /// <param name="defCity"></param>
        /// <param name="generalIds"></param>
        /// <param name="kingId">敌方君主</param>
        /// <param name="food"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        bool IsRetreat(City defCity, List<short> generalIds, short kingId, int food, int money)
        {
            Country country = CountryListCache.GetCountryByKingId(kingId); // 根据国王ID获取国家
            General king = GeneralListCache.GetGeneral(kingId); // 获取国王对象
            List<short> tempGeneralIds = new List<short>(); // 待撤退将军ID数组
            bool commanderCaptured = false; // 标记主将是否被俘获

            // 遍历所有将军ID
            foreach (var generalId in generalIds)
            {
                if (generalId == country.countryKingId) // 判断将军是否为国王
                {
                    tempGeneralIds.Add(generalId);
                    continue; // 跳过后续操作
                }

                if (defCity.GetCityOfficerNum() > 9) // 如果城市中的将军数量大于9
                {
                    tempGeneralIds.Add(generalId);
                    continue; // 跳过后续操作
                }

                General general = GeneralListCache.GetGeneral(generalId); // 获取将军对象

                // 判断国家是否只剩下当前城市
                if (country.cityIDs.Count < 1 || (country.cityIDs.Count == 1 && country.cityIDs[0] == defCity.cityId))
                {
                    // 根据将军和国王的相性值计算是否俘获
                    if (GeneralListCache.GetPhaseDifference(king, general) > 20)
                    {
                        general.CapturedGeneralTo(defCity.cityId); // 将将军俘获
                        if (generalIds.IndexOf(generalId) == 0)
                        {
                            commanderCaptured = true; // 标记主将被俘获
                            Debug.Log("主将：" + general.generalName + "被俘获！！！！");
                        }
                        else
                        {
                            Debug.Log("武将：" + general.generalName + "被俘获！！！！");
                        }
                        continue;
                    }
                }
                else if (general.GetCurPhysical() <= 40 && general.generalSoldier <= 0 && GeneralListCache.GetPhaseDifference(king, general) > 15)
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
                    if (Random.Range(0, 100) <= capturedProbability)
                    {
                        general.CapturedGeneralTo(defCity.cityId); // 将将军俘获
                        if (generalIds.IndexOf(generalId) == 0)
                        {
                            commanderCaptured = true; // 标记主将被俘获
                            Debug.Log("主将：" + general.generalName + "被俘获！！！！");
                        }
                        else
                        {
                            Debug.Log("武将：" + general.generalName + "被俘获！！！！");
                        }
                        continue;
                    }
                }
                tempGeneralIds.Add(generalId);
                continue;
            }

            bool masterRetreat = false; // 标记主将是否撤退成功
            if (tempGeneralIds.Count > 0)
            {
                masterRetreat = country.AIRetreatGeneralToCity(tempGeneralIds.ToArray(), defCity.cityId, food, money, commanderCaptured); // 将将军撤退到城市
            }

            if (!masterRetreat) // 如果主将没有成功撤退
            {
                defCity.AddFood(food); // 给城市添加食物
                defCity.AddGold(money); // 给城市添加金钱
                if (kingId == generalIds[0])
                    return true; // 如果国王是第一个撤退的将军，返回true
            }

            return false; // 返回false
        }

        void AiMoniAtk2(City defCity, General attacker, General defender)
        {
            bool isPrefectId = (defCity.prefectId == defender.generalId);
            short soldier1 = attacker.generalSoldier;
            short soldier2 = defender.generalSoldier;

            if (soldier1 > 0 && soldier2 > 0)
            {
                HandleBattle(attacker, defender, isPrefectId, soldier1, soldier2);
            }
            else if (soldier1 > 0)
            {
                HandleAttackWithSoldiersLeft(attacker, defender, isPrefectId, soldier1, 0);
            }
            else if (soldier2 > 0)
            {
                HandleAttackWithSoldiersLeft(defender, attacker, isPrefectId, soldier2, 1);
            }
            else
            {
                HandleBothGeneralsNoSoldiers(attacker, defender);
            }
        }

        void HandleBattle(General attacker, General defender, bool isPrefectId, short soldier1, short soldier2)
        {
            int power1 = CalculatePower(attacker, isPrefectId);
            int power2 = CalculatePower(defender, isPrefectId, true);
            int sword1 = power1 * soldier1;
            int sword2 = power2 * soldier2;

            if (sword1 > sword2)
            {
                ResolveBattleOutcome(attacker, defender, sword2, power1, power2);
            }
            else
            {
                ResolveBattleOutcome(defender, attacker, sword1, power2, power1);
            }
        }

        int CalculatePower(General general, bool isPrefectId, bool isDef = false)
        {
            int power = general.GetWarValue();
            if (isPrefectId)
            {
                power = (int)(power * (isDef ? 1.33D : 1.0D));
            }
            return MoniAtkgetGenPower(power, !isDef, general);
        }

        void HandleAttackWithSoldiersLeft(General attacker, General defender, bool isPrefectId, short soldiersLeft, int attackerType)
        {
            int power1 = CalculatePower(attacker, isPrefectId);
            int power2 = GetGenSinglePower(defender);
            int sword1 = power1 * soldiersLeft;

            byte phy = CanGetHP(defender);
            int sword2 = (attackerType == 0 ? power2 * phy * 3 : power2 * phy * 2);

            if (sword1 > sword2)
            {
                int dea1 = sword2 / power1;
                attacker.SubSoldier(dea1);
                defender.SetCurPhysical((byte)Random.Range(35, 41));
                defender.AddForceExp(dea1 / 50);
            }
            else
            {
                attacker.generalSoldier = 0;
                byte physical = (byte)(sword1 / power2 * (attackerType == 0 ? 3 : 2));
                defender.SubHp(physical);
                if (defender.GetCurPhysical() < 1)
                    defender.SetCurPhysical(1);
                defender.AddForceExp(soldiersLeft / 50);
            }
        }

        void HandleBothGeneralsNoSoldiers(General attacker, General defender)
        {
            int power1 = CalculateGeneralPower(attacker);
            int power2 = CalculateGeneralPower(defender);

            byte phy1 = attacker.GetCurPhysical();
            byte phy2 = defender.GetCurPhysical();

            if (power1 * phy1 > power2 * phy2)
            {
                ResolveGeneralNoSoldiersOutcome(attacker, defender, power1, phy1, power2, phy2);
            }
            else if (power1 * phy1 == power2 * phy2)
            {
                attacker.SetCurPhysical((byte)Random.Range(10, 15));
                defender.SetCurPhysical((byte)Random.Range(10, 15));
                attacker.AddForceExp(5);
                defender.AddForceExp(5);
            }
            else
            {
                ResolveGeneralNoSoldiersOutcome(defender, attacker, power2, phy2, power1, phy1);
            }
        }

        int CalculateGeneralPower(General general)
        {
            return general.force + general.force *
                   (WeaponListCache.GetWeapon(general.weapon).weaponProperties +
                    WeaponListCache.GetWeapon(general.armor).weaponProperties) / 100;
        }

        void ResolveBattleOutcome(General winner, General loser, int swordLoss, int winnerPower, int loserPower)
        {
            int dea1 = swordLoss / winnerPower;
            winner.SubSoldier(dea1);
            GeneralListCache.AddExp_P(winner, loser, dea1);
            GeneralListCache.AIWarAddEXP2(winner, loser, (short)dea1);
        }

        void ResolveGeneralNoSoldiersOutcome(General winner, General loser, int winnerPower, byte winnerPhy, int loserPower, byte loserPhy)
        {
            loser.SetCurPhysical(10);
            byte x = (byte)((winnerPower * winnerPhy - loserPower * loserPhy) / winnerPower);
            winner.SubHp(x);
            if (winner.GetCurPhysical() < 10)
                winner.SetCurPhysical(10);
            winner.AddForceExp(10);
            loser.AddForceExp(2);
        }

        int MoniAtkgetGenPower(int power, bool ists, General general)
        {
            // 根据将领的士兵数量调整攻击力
            long gjl_jq = (1 + power * power * power / 100000);
            if (ists && general.generalSoldier <= 500)
                gjl_jq = (int)Mathf.Min(100L, gjl_jq);

            return (int)gjl_jq;
        }

        int GetGenSinglePower(General general)
        {
            // 计算将领的单个战斗力
            int power = general.force * 2 +
                        general.force * WeaponListCache.GetWeapon(general.weapon).weaponProperties / 100 +
                        general.force * WeaponListCache.GetWeapon(general.armor).weaponProperties / 100;
            long p = (1 + power * power * power / 100000);

            return (int)p;
        }

        byte CanGetHP(General general)
        {
            // 获取将领的当前体力
            byte phy;
            if (general.GetCurPhysical() > 35)
            {
                int ph = Random.Range(0, general.GetCurPhysical() + 30);
                if (ph >= general.GetCurPhysical())
                    ph = general.GetCurPhysical() - 35;

                phy = (byte)ph;
            }
            else
            {
                phy = 1;
            }

            return phy;
        }

        
        
        
        private void AiStudy()
        {
            if (AiFindCanStudyGeneral())
            {
                Debug.Log($"{_curCountry.KingName()}太想进步了,于是组织了菁英大学习");
            }
        }
        
        // 学习
        bool AiFindCanStudyGeneral()
        {
            short studentCount = 0;

            // 枚举所有的城市
            foreach (byte cityId in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityId);
                // 累加所有城市的总学习值
                studentCount += StudyOfAllGeneral(city);
            }

            // 如果学习值大于0，则返回true
            return studentCount > 0;
        }
        
        
        // 计算所有武将的学习进度
        static short StudyOfAllGeneral(City city)
        {
            short learnNum = 0;
            short[] officeGeneralIdArray = city.GetOfficerIds();
            for (byte i = 0; i < officeGeneralIdArray.Length; i++)
            {
                short generalId = officeGeneralIdArray[i];
                General general = GeneralListCache.GetGeneral(generalId);
                // 如果智力低于120
                if (general.IQ < 120)
                {
                    // 根据智力值除以10的结果进行不同的处理
                    switch (general.IQ / 10)
                    {
                        // 智力在0-40之间
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            // 如果经验值足够学习
                            if (general.experience >= general.GetSchoolNeedExp())
                            {
                                general.StudyUp();
                            }
                            // 增加学习次数
                            learnNum = (short)(learnNum + 1);
                            break;
                        // 智力为50
                        case 5:
                            // 如果经验值足够且等级大于等于2
                            if (general.experience >= general.GetSchoolNeedExp() && general.level >= 2)
                            {
                                general.StudyUp();
                            }
                            // 增加学习次数
                            learnNum = (short)(learnNum + 1);
                            break;
                        // 智力为60
                        case 6:
                            // 如果经验值足够且等级大于等于3
                            if (general.experience >= general.GetSchoolNeedExp() && general.level >= 3)
                            {
                                general.StudyUp();
                            }
                            // 增加学习次数
                            learnNum = (short)(learnNum + 1);
                            break;
                        // 智力为70
                        case 7:
                            // 如果经验值足够且等级大于等于4
                            if (general.experience >= general.GetSchoolNeedExp() && general.level >= 4)
                            {
                                general.StudyUp();
                            }
                            // 增加学习次数
                            learnNum = (short)(learnNum + 1);
                            break;
                        // 智力为80
                        case 8:
                            // 如果经验值足够且等级大于等于7
                            if (general.experience >= general.GetSchoolNeedExp() && general.level >= 7)
                            {
                                general.StudyUp();
                            }
                            break;
                        // 智力为90到120之间
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            // 如果经验值足够且等级等于8
                            if (general.experience >= general.GetSchoolNeedExp() && general.level == 8)
                            {
                                general.StudyUp();
                            }
                            // 增加学习次数
                            learnNum = (short)(learnNum + 1);
                            break;
                    }
                }
            }
            return learnNum;
        }

                /// <summary>
        /// 学习方法
        /// </summary>
        /// <param name="learnNum">初始学习次数</param>
        public void Study(short learnNum)
        {
            
        }






        private void AiTreat() 
        {
            if (AiFindLowHpGeneral(out City city, out short[] wounded))  // 如果可以进行治疗
            {
                AiTreat(city, wounded);  // 执行治疗操作
                Debug.Log("执行治疗");
            }

        
        }

        /// <summary>
        /// 处理AI检查低血量武将方法
        /// </summary>
        /// <returns></returns>
        bool AiFindLowHpGeneral(out City doCity, out short[] wounded)
        {
            doCity = null;
            wounded = null;
            // 遍历城市
            foreach (var cityId in _curCountry.cityIDs)
            {
                City city = CityListCache.GetCityByCityId(cityId); // 获取城市对象

                List<short> treatGeneralId = city.GetThresholdGeneralIds(60);
                if (city.GetMoney() >= 100) // 如果城市金钱大于等于100
                {
                    if (treatGeneralId.Count > 0)
                    {
                        doCity = city;  // 设置当前城市
                        wounded = treatGeneralId.ToArray();
                        return true;
                    }
                }
            }
            return false; // 如果找到了合适的将领，返回true
        }

        /// <summary>
        /// AI执行治疗操作
        /// </summary>
        void AiTreat(City city, short[] treatGeneralIds)
        {
            for (int i = 0; i < treatGeneralIds.Length; i++)
            {
                byte physical = (byte)AiTreatValue();  // 获取随机治疗效果
                General general = GeneralListCache.GetGeneral(treatGeneralIds[i]);  // 获取当前操作的将领
                general.AddCurPhysical(physical);  // 增加将领的当前体力
            }
            city.SubGold(50);  // 从城市的金钱中扣除 50
        }

        /// <summary>
        /// 随机返回一个治疗效果值
        /// </summary>
        /// <returns></returns>
        int AiTreatValue()
        {
            return Random.Range(33, 51);  // 返回 33 到 51 的随机值
        }
    }
}