using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseClass;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DataClass
{
    public class DataManagement : MonoBehaviour
    {
        private static DataManagement _instance;

        public static DataManagement Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<DataManagement>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("DataManagement").AddComponent<DataManagement>();
                    }
                }
                return _instance;
            }
        }
        private DataManagement(){}
        
        private static bool isLoadAndInitializeData = false;

        private const int MAX_SAVE_SLOTS = 4;

        private static string[] saveFilePaths;
        
        public static Dictionary<byte, byte[,]> maps = new Dictionary<byte, byte[,]>();
        
        public static Dictionary<string, byte[,]> formations = new Dictionary<string, byte[,]>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadAndInitializeData()
        {
            if (!isLoadAndInitializeData)
            {
                LoadData();
                Initialize();
                isLoadAndInitializeData = true;
            }

        }

        private void LoadData()
        {
            StartCoroutine(ReadGeneralData());
            StartCoroutine(ReadCityData());
            StartCoroutine(ReadCountryData());
            StartCoroutine(ReadWeaponData());
        }

        private static void Initialize()
        {
            InitCities();
            InitGeneralsInCities();
            InitCityPrefects();
        }

        // 读取势力数据的方法
        private static IEnumerator ReadCountryData()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Json/CountryData.json");
            string json = string.Empty;

            // 根据平台判断读取方式
            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest 读取 StreamingAssets
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"读取 Android 文件失败: {www.error}");
                        yield break;
                    }
                    else
                    {
                        json = www.downloadHandler.text;
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows 平台直接读取文件
                if (File.Exists(filePath))
                {
                    try
                    {
                        json = File.ReadAllText(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"在 Windows 平台读取文件时出错: {ex.Message}");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError($"文件在 Windows 平台不存在: {filePath}");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("当前平台不支持读取 CountryData.json 文件。");
                yield break;
            }

            // 解析 JSON 数据
            try
            {
                List<Country> countryList = JsonConvert.DeserializeObject<List<Country>>(json);

                if (countryList == null || countryList.Count == 0)
                {
                    Debug.LogError("读取的势力数据为空。");
                }
                else
                {
                    foreach (var country in countryList)
                    {
                        CountryListCache.AddCountry(country); // 加入缓存
                    }
                    Debug.Log($"成功加载了 {CountryListCache.countryDictionary.Count} 个势力数据。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 反序列化出错: {ex.Message}");
            }
        }
        
        // 读取城市数据的方法
        private static IEnumerator ReadCityData()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Json/CityData.json");
            string json = string.Empty;

            // 根据不同平台选择合适的读取方式
            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest 从 APK 包内读取数据
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"读取 Android 文件失败: {www.error}");
                        yield break;
                    }
                    else
                    {
                        json = www.downloadHandler.text;
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows 平台直接读取文件
                if (File.Exists(filePath))
                {
                    try
                    {
                        json = File.ReadAllText(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"在 Windows 平台读取城市数据时出错: {ex.Message}");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError($"CityData.json 文件不存在于 {filePath}");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("当前平台不支持读取 CityData.json 文件。");
                yield break;
            }

            // 解析 JSON 数据
            try
            {
                List<City> cityList = JsonConvert.DeserializeObject<List<City>>(json);

                if (cityList == null || cityList.Count == 0)
                {
                    Debug.LogError("城市数据为空或加载失败。");
                }
                else
                {
                    foreach (var city in cityList)
                    {
                        // 将所有城市对象添加到静态字典中
                        CityListCache.AddCity(city);
                    }

                    Debug.Log($"成功加载了 {CityListCache.cityDictionary.Count} 座城市数据！");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 反序列化失败: {ex.Message}");
            }
        
        }
        
        // 读取武将数据的方法
        private static IEnumerator ReadGeneralData()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Json/GeneralData.json");
            string json = string.Empty;

            // 根据平台判断读取方式
            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest 从 APK 内读取
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"读取 Android 文件失败: {www.error}");
                        yield break;
                    }
                    else
                    {
                        json = www.downloadHandler.text;
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows 平台直接从文件系统读取
                if (File.Exists(filePath))
                {
                    try
                    {
                        json = File.ReadAllText(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"在 Windows 读取 GeneralData.json 时出错: {ex.Message}");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError($"GeneralData.json 不存在于 {filePath}");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("当前平台不支持读取 GeneralData.json 文件。");
                yield break;
            }

            // 解析 JSON 数据
            try
            {
                List<General> totalGeneralList = JsonConvert.DeserializeObject<List<General>>(json);

                if (totalGeneralList == null || totalGeneralList.Count == 0)
                {
                    Debug.LogError("武将数据为空或加载失败。");
                }
                else
                {
                    // 清空缓存，避免重复加载
                    GeneralListCache.clearAllGenerals();

                    // 遍历武将列表并根据登场年份分类
                    foreach (var general in totalGeneralList)
                    {
                        GeneralListCache.AddGeneral(general);

                        if (general.debutYears <= GameInfo.years)
                        {
                            // 已登场武将
                            GeneralListCache.AddDebutedGeneral(general);
                        }
                        else
                        {
                            // 未登场武将
                            GeneralListCache.AddNoDebutGeneral(general);
                        }
                    }

                    // 打印加载结果
                    Debug.Log($"{GeneralListCache.generalList.Count} 位武将在 {GameInfo.years} 年已登场," +
                              $"{GeneralListCache.noDebutGeneralList.Count} 位武将尚未登场," +
                              $"总共成功加载了 {GeneralListCache.GetTotalGeneralNum()} 个武将数据。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 反序列化失败: {ex.Message}");
            }
        }


        // 读取武器数据的方法
        private static IEnumerator ReadWeaponData()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Json/WeaponData.json");
            string json = string.Empty;

            // 根据平台判断读取方式
            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest 从 APK 中读取
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"读取 Android 文件失败: {www.error}");
                        yield break;
                    }
                    else
                    {
                        json = www.downloadHandler.text;
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows 平台直接读取文件
                if (File.Exists(filePath))
                {
                    try
                    {
                        json = File.ReadAllText(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"在 Windows 读取 WeaponData.json 时出错: {ex.Message}");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError($"WeaponData.json 不存在于 {filePath}");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("当前平台不支持读取 WeaponData.json 文件。");
                yield break;
            }

            // 解析 JSON 数据
            try
            {
                List<Weapon> weaponList = JsonConvert.DeserializeObject<List<Weapon>>(json);

                if (weaponList == null || weaponList.Count == 0)
                {
                    Debug.LogError("武器数据为空或加载失败。");
                }
                else
                {
                    // 清空之前缓存，避免重复加载
                    WeaponListCache.ClearAllWeapons();

                    // 遍历武器列表并存储
                    foreach (var weapon in weaponList)
                    {
                        WeaponListCache.AddWeapon(weapon);
                    }

                    Debug.Log($"成功加载了 {WeaponListCache.weaponDictionary.Count} 件武器数据！");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 反序列化失败: {ex.Message}");
            }
        }

        //初始化国内城市
        private static void InitCities()
        {
            // 遍历城市列表  
            foreach (var city in CityListCache.cityDictionary)
            {
                // 如果城市属于某个国家（即cityBelongKing不为0）  
                if (city.Value.cityBelongKing != 0)
                {
                    // 遍历国家列表查找对应的国家  
                    var country = CountryListCache.GetCountryByKingId(city.Value.cityBelongKing);
                    if (country != null)
                    {
                        // 检查是否已经包含该城市的ID，避免重复添加  
                        if (!country.cityIDs.Contains(city.Key))
                        {
                            // 将城市添加到对应国家的Cities列表中  
                            country.cityIDs.Add(city.Key);
                        }
                        else
                        {
                            Debug.LogWarning("城池" + city.Value.cityName + "已归属于势力" + country.KingName() + "跳过添加。");
                        }
                    }
                }
            }
            Debug.Log($"当今天下共有{CityListCache.cityDictionary.Count}座城池");

            // 此时，每个国家的Cities列表都应该填充了正确的城市，且不会有重复项  
        }

        // 初始化第一年武将在城市中的状态
        private static void InitGeneralsInCities()
        {
            int addoffice = 0;
            int addnotfound = 0;
            // 遍历所有武将
            foreach (var general in GeneralListCache.generalList)
            {          
                if (general.generalId != 0)
                {   // 获取武将的登场城市ID
                    byte debutCityId = general.debutCity;
                    // 根据城市ID查找对应城市
                    City city = CityListCache.GetCityByCityId(debutCityId);

                    if (city != null)
                    { 
                        if (general.isOffice == 1)
                        {     // 将武将添加到有职务的武将列表
                            city.AddOfficeGeneralId(general.generalId);
                            addoffice++; 
                        }
                        else
                        {
                            city.AddNotFoundGeneralId(general.generalId);
                            addnotfound++;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"未能找到ID为 {debutCityId} 的城市，武将 {general.generalId} 无法被分配。");                  
                    }
                }
            }

            // 此时，每个城市的未任职和任职武将列表都应该正确填充
            Debug.Log($"共有{addoffice}位武将已经入城，其中{addnotfound}未做官。");
        }


        // 遍历所有城市，并对归属于某个国家的城市执行任命操作
        private static void InitCityPrefects()
        {
            // 从ID为1的城市开始遍历，直到城市总数
            for (byte cityId = 1; cityId < CityListCache.CITY_NUM; cityId = (byte)(cityId + 1))
            {
                // 根据城市ID获取城市对象
                City city = CityListCache.GetCityByCityId(cityId);

                // 判断城市是否属于某个国家（即 cityBelongKing > 0）
                if (city != null && city.cityBelongKing > 0)
                {
                    // 为城市任命太守
                    city.AppointmentPrefect();
                    //Debug.Log($"城市 {city.cityName} 属于君主 {city.cityBelongKing}，已任命太守。");
                }
                else
                {
                    // 如果城市不存在或没有归属国家，输出警告信息
                    Debug.Log($"城池 {city.cityName} 是一座空城。");
                }
            }

            // 任命所有符合条件的城市太守操作完成
            Debug.Log("诸侯们已在各自城池任命太守。");
        }


        private static string[] GetSaveFilePaths()
        {
            if (saveFilePaths == null)
            {
                // ✅ 使用 persistentDataPath 保证跨平台兼容
                string savePath = Path.Combine(Application.persistentDataPath, "Saves");

                // ✅ 检查并创建存档文件夹
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                    Debug.Log($"创建存档文件夹: {savePath}");
                }

                // ✅ 生成存档路径
                saveFilePaths = new string[MAX_SAVE_SLOTS];
                for (int i = 0; i < MAX_SAVE_SLOTS; i++)
                {
                    saveFilePaths[i] = Path.Combine(savePath, $"save{i}.json");
                }

                Debug.Log("存档路径已成功生成。");
            }

            return saveFilePaths;
        }

        public static void GetRecordInfo()
        {
            // 获取存档文件路径
            GetSaveFilePaths();

            for (int i = 0; i < MAX_SAVE_SLOTS; i++)
            {
                try
                {
                    if (File.Exists(saveFilePaths[i]))
                    {
                        // 读取存档数据
                        string jsonData = File.ReadAllText(saveFilePaths[i]);

                        if (string.IsNullOrEmpty(jsonData))
                        {
                            Debug.LogError($"存档 {i} 的 JSON 数据无效或为空！");
                            GameInfo.recordInfo[i] = "存档损坏";
                            continue;
                        }

                        // 反序列化 JSON 数据
                        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);

                        if (saveData != null && !string.IsNullOrEmpty(saveData.recordInfo))
                        {
                            GameInfo.recordInfo[i] = saveData.recordInfo; // 存储记录信息
                        }
                        else
                        {
                            Debug.LogWarning($"存档 {i} 数据无效或 recordInfo 为空。");
                            GameInfo.recordInfo[i] = "存档数据无效";
                        }
                    }
                    else
                    {
                        GameInfo.recordInfo[i] = "尚无存档"; // 如果文件不存在，设置默认文本
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"读取存档 {i} 时出错: {ex.Message}");
                    GameInfo.recordInfo[i] = "读取失败";
                }
            }
        }

        /// <summary>
        /// 存档保存方法
        /// </summary>
        /// <param name="index"></param>
        public static void SaveGame(int index)
        {
            GetSaveFilePaths();
            SaveData saveData = new SaveData();

            // 填充游戏数据
            saveData.recordInfo = GeneralListCache.GetGeneral(CountryListCache.GetCountryByCountryId(GameInfo.playerCountryId).countryKingId).generalName +
                                  $"({GameInfo.years}年{GameInfo.month}月)";
            saveData.playerCountryId = GameInfo.playerCountryId;
            saveData.doCityId = GameInfo.doCityId;
            saveData.playerOrderNum = GameInfo.playerOrderNum;
            saveData.month = GameInfo.month;
            saveData.years = GameInfo.years;
            saveData.difficult = GameInfo.difficult;
            saveData.attackCount = GameInfo.attackCount;
            saveData.countrySequence = CountryListCache.countrySequence;
        
            // 添加将领信息
            saveData.generalList = new List<General>();
            foreach (var pair in GeneralListCache.generalDictionary)
            {
                saveData.generalList.Add(pair.Value);
            }

            // 添加城市信息
            saveData.cityList = new List<City>();
            foreach (var pair in CityListCache.cityDictionary)
            {
                saveData.cityList.Add(pair.Value);
            }

            // 添加国家信息
            saveData.countryList = new List<Country>();
            foreach (var pair in CountryListCache.countryDictionary)
            {
                saveData.countryList.Add(pair.Value);
            }

            // 将游戏数据序列化为JSON
            string jsonData = JsonConvert.SerializeObject(saveData, Formatting.Indented);

            // 保存到本地文件
            try
            {
                File.WriteAllText(saveFilePaths[index], jsonData);
                Debug.Log("游戏成功保存!");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存过程中发生错误: {e.Message}");
            }
        }




        /// <summary>
        /// 存档加载方法
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public void LoadGame(int index)
        {
            GetSaveFilePaths();

            // 检查保存文件是否存在
            if (!File.Exists(saveFilePaths[index]))
            {
                Debug.LogError("保存文件不存在!");
                return;
            }

            // 从文件中读取 JSON 数据
            string jsonData;
            try
            {
                jsonData = File.ReadAllText(saveFilePaths[index]);
            }
            catch (Exception e)
            {
                Debug.LogError($"读取保存文件时发生错误: {e.Message}");
                return;
            }

            // 反序列化 JSON 数据
            SaveData saveData;
            try
            {
                saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError($"反序列化保存数据时发生错误: {e.Message}");
                return;
            }

            // 校验加载的数据
            if (saveData == null)
            {
                Debug.LogError("加载的数据无效!");
                return;
            }

            // 还原游戏数据
            GameInfo.playerCountryId = saveData.playerCountryId;
            GameInfo.doCityId = saveData.doCityId;
            GameInfo.playerOrderNum = saveData.playerOrderNum;
            GameInfo.month = saveData.month;
            GameInfo.years = saveData.years;
            GameInfo.difficult = saveData.difficult;
            GameInfo.attackCount = saveData.attackCount;
            CountryListCache.countrySequence = saveData.countrySequence;

            // 还原将领信息
            GeneralListCache.ClearAllTotalGenerals(); // 假设有一个方法清空当前将领数据
            foreach (var generalData in saveData.generalList)
            {
                GeneralListCache.AddGeneral(generalData);
                if (generalData.debutYears <= GameInfo.years)
                {
                    // debutYears 小于等于当前年份的武将加入 generals
                    GeneralListCache.AddDebutedGeneral(generalData);
                }
                else
                {
                    // debutYears 大于当前年份的武将加入 noDebutGenerals
                    GeneralListCache.AddNoDebutGeneral(generalData);

                }
            }

       

            // 还原城市信息
            CityListCache.ClearAllCities(); // 假设有一个方法清空当前城市数据
            foreach (var cityData in saveData.cityList)
            {
                CityListCache.AddCity(cityData); // 假设有一个方法添加城市
            }

            // 还原国家信息
            CountryListCache.ClearAllCountries(); // 假设有一个方法清空当前国家数据
            foreach (var countryData in saveData.countryList)
            {
                CountryListCache.AddCountry(countryData); // 假设有一个方法添加国家
            }
            InitCities();
            InitGeneralsInCities();
            InitCityPrefects();
            StartCoroutine(ReadWeaponData());
            GC.Collect();
            Debug.Log("游戏成功加载!");
        }

        // 读取自定义武将列表
        public static void ReadCustomGeneralList()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Custom");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, "Custom.json");
            List<General> customList = new List<General>();
            if (!File.Exists(filePath))
            {
                Debug.Log("自定义武将数据文件不存在！");
            }
            else
            {
                try
                {
                    string json = File.ReadAllText(filePath);

                    // 使用 Newtonsoft.Json 进行反序列化
                    customList = JsonConvert.DeserializeObject<List<General>>(json);

                    if (customList == null || customList.Count == 0)
                    {
                        Debug.Log("自定义武将数据加载失败或为空。");
                    }
                    else
                    {
                        // 遍历 customList，缓存到字典中
                        foreach (var general in customList)
                        {
                            GeneralListCache.AddGeneral(general);
                        }
                        Debug.Log($"自定义武将数据加载成功！加载了 {customList.Count} 位武将数据。");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载武将数据时出错: {ex.Message}");
                }
            }
        
            GameInfo.customGeneralList = customList;
        }

        public static void AddCustomGeneral(General general, short imageId)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Custom");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, "Custom.json");
            // 源图像路径（相对于项目的Assets文件夹）
            string sourceImagePath = $"Assets/Resources/HeadImage/SelfHead/{imageId}.png";
    
            // 目标头像文件夹路径（相对于项目的Assets文件夹）
            string destinationFolderPath = "Assets/Resources/HeadImage/";
            if (!File.Exists(filePath))//自定义将领文件不存在
            {
                general.generalId = 10000;
                // 确保目标头像文件夹存在
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }
    
                // 目标头像文件路径（包括新名称）
                string destinationFilePath = Path.Combine(destinationFolderPath, $"{general.generalId}.png");
                GameInfo.customGeneralList.Add(general);
                try
                {
                    // 复制头像文件
                    File.Copy(sourceImagePath, destinationFilePath, true); // 第三个参数为true表示如果目标文件已存在，则覆盖它
        
                    Debug.Log("头像文件复制成功！");
                }
                catch (Exception e)
                {
                    Debug.LogError("头像文件操作失败：" + e.Message);
                }
            
                // 将游戏数据序列化为JSON
                string jsonData = JsonConvert.SerializeObject(GameInfo.customGeneralList, Formatting.Indented);

                // 保存到本地文件
                try
                {
                    File.WriteAllText(filePath, jsonData);
                    Debug.Log("自定义武将成功保存!");
                }
                catch (Exception e)
                {
                    Debug.LogError($"保存过程中发生错误: {e.Message}");
                }
            }
            else// 自定义武将文件已经存在
            {
                // 读取JSON数据
                ReadCustomGeneralList();
                general.generalId = (short)(GameInfo.customGeneralList.Count + 10000);
                // 确保目标头像文件夹存在
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }
    
                // 目标头像文件路径（包括新名称）
                string destinationFilePath = Path.Combine(destinationFolderPath, $"{general.generalId}.png");
                GameInfo.customGeneralList.Add(general);
                try
                {
                    // 复制头像文件
                    File.Copy(sourceImagePath, destinationFilePath, true); // 第三个参数为true表示如果目标文件已存在，则覆盖它
        
                    Debug.Log("头像文件复制成功！");
                }
                catch (Exception e)
                {
                    Debug.LogError("头像文件操作失败：" + e.Message);
                }
                // 将游戏数据序列化为JSON
                string jsonData2 = JsonConvert.SerializeObject(GameInfo.customGeneralList, Formatting.Indented);
                // 保存到本地文件
                try
                {
                    File.WriteAllText(filePath, jsonData2);
                    Debug.Log("自定义武将成功保存!");
                }
                catch (Exception e)
                {
                    Debug.LogError($"保存过程中发生错误: {e.Message}");
                }
            }
        }

        public static void RemoveCustomGeneral(short generalId)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Custom");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, "Custom.json");
        
            if (!File.Exists(filePath))
            {
                Debug.Log("自定义武将数据文件不存在！");
            }
            else
            {
                // 读取JSON数据
                ReadCustomGeneralList();
                // 目标头像文件路径（包括新名称）
                string imageName = $"Assets/Resources/HeadImage/SelfHead/{generalId}.png";
                if (File.Exists(imageName))
                {
                    File.Delete(imageName);
                }
                General general = GameInfo.customGeneralList.FirstOrDefault(t => t.generalId == generalId);
            
                GeneralListCache.RemoveGeneral(generalId);
                GameInfo.customGeneralList.Remove(general);
                // 将游戏数据序列化为JSON
                string jsonData = JsonConvert.SerializeObject(GameInfo.customGeneralList, Formatting.Indented);
                // 保存到本地文件
                try
                {
                    File.WriteAllText(filePath, jsonData);
                    Debug.Log("自定义武将成功删除!");
                }
                catch (Exception e)
                {
                    Debug.LogError($"删除过程中发生错误: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 读取游戏说明文件
        /// </summary>
        /// <returns></returns>
        public static IEnumerator ReadIntroduction(Action<string> onComplete)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "readMe.txt");
            string introduction = string.Empty;

            // ✅ 根据平台选择读取方式
            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest 读取 APK 内文件
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"在 Android 读取说明文件失败: {www.error}");
                        onComplete?.Invoke(string.Empty);
                        yield break;
                    }
                    else
                    {
                        introduction = www.downloadHandler.text;
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows 平台直接读取文件
                if (File.Exists(filePath))
                {
                    try
                    {
                        introduction = File.ReadAllText(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"在 Windows 读取说明文件时出错: {ex.Message}");
                        onComplete?.Invoke(string.Empty);
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError($"说明文件不存在于: {filePath}");
                    onComplete?.Invoke(string.Empty);
                    yield break;
                }
            }
            else
            {
                Debug.LogError("当前平台不支持读取说明文件。");
                onComplete?.Invoke(string.Empty);
                yield break;
            }

            // ✅ 调用回调函数返回读取结果
            onComplete?.Invoke(introduction);
        }
        
        
        
        /// <summary>
        /// 加载地图数据并返回 WarMap。
        /// </summary>
        /// <param name="cityId">城池 ID</param>
        /// <param name="onResult">加载完成的回调，返回 WarMap</param>
        public static IEnumerator LoadMapAsync(int cityId, Action<byte[,]> onResult)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Map", cityId + ".CSV");
            byte[,] warMap = new byte[19, 32]; // 初始化地图数组

            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest
                UnityWebRequest request = UnityWebRequest.Get(filePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string fileContent = request.downloadHandler.text;
                    ParseMapData(fileContent, warMap);
                    onResult?.Invoke(warMap); // 返回结果
                }
                else
                {
                    Debug.LogError($"加载地图文件失败: {filePath}. 错误: {request.error}");
                    onResult?.Invoke(null); // 返回空值
                }
            }
            else
            {
                // 非安卓平台直接读取文件
                if (File.Exists(filePath))
                {
                    string fileContent = File.ReadAllText(filePath);
                    ParseMapData(fileContent, warMap);
                    onResult?.Invoke(warMap); // 返回结果
                }
                else
                {
                    Debug.LogError($"加载地图文件失败: {filePath}");
                    onResult?.Invoke(null); // 返回空值
                }
            }
        }

        /// <summary>
        /// 解析地图数据并填充 WarMap 数组。
        /// </summary>
        /// <param name="fileContent">CSV 文件内容</param>
        /// <param name="warMap">要填充的地图数组</param>
        private static void ParseMapData(string fileContent, byte[,] warMap)
        {
            string[] lines = fileContent.Split('\n'); // 按行分割文件内容

            for (byte row = 0; row < 19 && row < lines.Length; row++)
            {
                string[] values = lines[row].Split(',');
                for (byte col = 0; col < 32 && col < values.Length; col++)
                {
                    if (byte.TryParse(values[col], out byte tileValue))
                    {
                        warMap[row, col] = tileValue;
                    }
                }
            }
        }
       
        

        /// <summary>
        /// 遍历 Formation 文件夹下的所有 .csv 文件，并加载到字典中
        /// </summary>
        public static IEnumerator LoadAllFormations()
        {
            if (formations != null)
            {
                Debug.Log("字典已存在，无需重新加载。");
                yield break;
            }
            
            string formationPath = Path.Combine(Application.streamingAssetsPath, "Formation");
            List<string> csvFileNames = new List<string>();

            // 根据平台遍历 Formation 文件夹
            if (Application.platform == RuntimePlatform.Android)
            {
                csvFileNames.Add("a0");
                csvFileNames.Add("a1");
                csvFileNames.Add("a2");
                csvFileNames.Add("a3");
                csvFileNames.Add("a4");
                csvFileNames.Add("ac");
                csvFileNames.Add("ac0");
                csvFileNames.Add("ac1");
                csvFileNames.Add("ac2");
                csvFileNames.Add("h0");
                csvFileNames.Add("h1");
                csvFileNames.Add("h2");
                csvFileNames.Add("h3");
                csvFileNames.Add("h4");
                csvFileNames.Add("hc");
                csvFileNames.Add("hc0");
                Debug.Log("当前为安卓平台");
                /*// 安卓平台通过 UnityWebRequest 读取文件列表（需打包时生成 file_list.txt）
                string fileListPath = Path.Combine(formationPath, "file_list.txt");
                using (UnityWebRequest request = UnityWebRequest.Get(fileListPath))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string[] files = request.downloadHandler.text.Split('\n');
                        foreach (string file in files)
                        {
                            if (file.EndsWith(".csv"))
                            {
                                csvFileNames.Add(file.Trim());
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"无法加载文件列表: {fileListPath}, 错误: {request.error}");
                        yield break;
                    }
                }*/
            }
            else
            {
                // Windows / macOS 等平台直接使用 Directory.GetFiles
                if (Directory.Exists(formationPath))
                {
                    string[] files = Directory.GetFiles(formationPath, "*.csv");
                    foreach (string filePath in files)
                    {
                        csvFileNames.Add(Path.GetFileName(filePath)); // 仅获取文件名
                    }
                }
                else
                {
                    Debug.LogError($"Formation 文件夹不存在: {formationPath}");
                    yield break;
                }
            }

            // 遍历所有文件名并加载内容
            foreach (string fileName in csvFileNames)
            {
                string formationName = Path.GetFileNameWithoutExtension(fileName); // 文件名去掉扩展名

                yield return LoadFormationAsync(formationName, (byte[,] array) =>
                {
                    if (array != null)
                    {
                        formations[formationName] = array;
                        Debug.Log($"成功加载阵型: {formationName}");
                    }
                    else
                    {
                        Debug.LogError($"加载阵型失败: {formationName}");
                    }
                });
            }

            Debug.Log($"加载完成，共加载了 {formations.Count} 个阵型文件。");
        }

        
        /// <summary>
        /// 加载单个阵型数据并返回 WarMap。
        /// </summary>
        /// <param name="formation">阵型名称</param>
        /// <param name="onResult">加载完成的回调，返回 Formation</param>
        public static IEnumerator LoadFormationAsync(string formation, System.Action<byte[,]> onResult)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Formation", formation + ".csv");
            byte[,] array = new byte[7, 16]; // 初始化地图数组

            if (Application.platform == RuntimePlatform.Android)
            {
                // 安卓平台使用 UnityWebRequest
                UnityWebRequest request = UnityWebRequest.Get(filePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string fileContent = request.downloadHandler.text;
                    ParseFormationData(fileContent, array);
                    onResult?.Invoke(array); // 返回结果
                }
                else
                {
                    Debug.LogError($"加载阵型文件失败: {filePath}. 错误: {request.error}");
                    onResult?.Invoke(null); // 返回空值
                }
            }
            else
            {
                // 非安卓平台直接读取文件
                if (File.Exists(filePath))
                {
                    string fileContent = File.ReadAllText(filePath);
                    ParseFormationData(fileContent, array);
                    onResult?.Invoke(array); // 返回结果
                }
                else
                {
                    Debug.LogError($"加载阵型文件失败: {filePath}");
                    onResult?.Invoke(null); // 返回空值
                }
            }
        }

        /// <summary>
        /// 解析阵型数据并填充阵型数组。
        /// </summary>
        /// <param name="fileContent">CSV 文件内容</param>
        /// <param name="array">要填充的阵型数组</param>
        private static void ParseFormationData(string fileContent, byte[,] array)
        {
            string[] lines = fileContent.Split('\n'); // 按行分割文件内容

            for (byte row = 0; row < 7 && row < lines.Length; row++)
            {
                string[] values = lines[row].Split(',');
                for (byte col = 0; col < 16 && col < values.Length; col++)
                {
                    if (byte.TryParse(values[col], out byte tileValue))
                    {
                        array[row, col] = tileValue;
                    }
                }
            }
        }

        public static byte[,] GetFormation(string formationName)
        {
            if(formations.TryGetValue(formationName, out byte[,] array))
            {
                return array;
            }
            Instance.StartCoroutine(LoadFormationAsync(formationName, (byte[,] fileArray) =>
            {
                if (fileArray != null)
                {
                    formations.Add(formationName, fileArray);
                    Debug.Log($"成功加载阵型: {formationName}");
                }
                else
                {
                    Debug.LogError($"加载阵型失败: {formationName},尝试重新加载...");
                    Instance.StartCoroutine(LoadAllFormations());
                }
            }));
            return formations[formationName];
        }
        









    }
}
