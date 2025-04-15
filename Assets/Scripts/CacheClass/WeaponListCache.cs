using System.Collections.Generic;
using System.Linq;
using BaseClass;
using UnityEngine;
using Newtonsoft.Json; 



// 武器缓存类
public static class WeaponListCache
{
    // 静态列表，用于存储武器数据
    //public static List<Weapon> weaponList;
    public static Dictionary<byte, Weapon> weaponDictionary = new Dictionary<byte, Weapon>();


    /*
    // 初始化方法
    public static void Init(System.Random random, byte[] data)
    {
        int index = 0;
        // 获取武器的数量
        byte weaponNum = CommonUtils.byte_bR_a(data[index++], random);

        // 输出初始化开始的日志
        Debug.Log($"初始化武器信息开始，总数: {weaponNum}");

        // 遍历每个武器
        for (int i = 0; i < weaponNum; i++)
        {
            Weapon weapon = new Weapon();

            // 设置武器ID
            weapon.weaponId = CommonUtils.byte_bR_a(data[index++], random);
            // 获取武器名称的长度
            byte weaponNameLength = CommonUtils.byte_bR_a(data[index++], random);

            // 读取武器名称
            byte[] weaponNameBytes = new byte[weaponNameLength];
            for (int j = 0; j < weaponNameLength; j++)
            {
                weaponNameBytes[j] = CommonUtils.byte_bR_a(data[index++], random);
            }
            weapon.weaponName = Encoding.UTF8.GetString(weaponNameBytes);

            // 读取武器价格
            byte byte0 = CommonUtils.byte_bR_a(data[index++], random);
            byte byte1 = CommonUtils.byte_bR_a(data[index++], random);
            weapon.weaponPrice = (short)((byte1 << 8) | (byte0 & 0xFF));

            // 读取武器属性
            weapon.weaponProperties = CommonUtils.byte_bR_a(data[index++], random);
            // 读取武器重量
            weapon.weaponWeight = CommonUtils.byte_bR_a(data[index++], random);
            // 读取武器类型
            weapon.weaponType = CommonUtils.byte_bR_a(data[index++], random);

            // 将武器添加到列表中
            addWeapon(weapon);

            // 输出武器添加的日志
            Debug.Log($"武器ID: {weapon.weaponId}, 名称: {weapon.weaponName}, 类型: {weapon.weaponType} 添加到缓存");
        }

        // 输出初始化完成的日志
        Debug.Log($"初始化武器信息完成，武器数量: {getWeaponSize()}");
    }
    */
    // 添加武器到列表
    public static void AddWeapon(Weapon weapon)
    {
        weaponDictionary.TryAdd(weapon.weaponId, weapon);
    }

    // 获取武器列表的大小
    public static byte getWeaponSize()
    {
        return (byte)weaponDictionary.Count;
    }

    // 根据武器ID获取武器
    public static Weapon GetWeapon(byte weaponId)
    {
        if (weaponDictionary.TryGetValue(weaponId, out var weapon))
        {
            return weapon;
        }
        
        // 输出未找到武器的日志
        Debug.LogWarning($"未找到武器ID: {weaponId}");
        return null;
    }
    
    /// <summary>
    /// 根据城池商店值筛选 Weapon 对象
    /// </summary>
    /// <param name="citySmithy">输入的城池商店值 (0 到 7)</param>
    /// <returns>符合条件的 Weapon 对象列表</returns>
    public static List<Weapon> GetWeaponsBySmithy(byte citySmithy)
    {
        // 如果输入为 0，返回空列表
        if (citySmithy == 0)
        {
            Debug.Log("城中无武器店");
            return null;
        }

        // 计算目标位的标识值 (1 << (citySmithy - 1))
        short bitMask = (short)(1 << (citySmithy - 1));

        // 筛选符合条件的武器
        List<Weapon> filteredWeapons = weaponDictionary
            .Where(kv => (kv.Value.smithy & bitMask) != 0) // 使用位运算检测指定位是否为 1
            .Select(kv => kv.Value)                      // 提取 Weapon 对象
            .ToList();

        return filteredWeapons;
    }

    // 清空所有武器
    public static void ClearAllWeapons()
    {
        int count = weaponDictionary.Count;
        weaponDictionary.Clear();
        // 输出清空武器的日志
        Debug.Log($"所有武器已清空，共清理 {count} 件武器");
    }
}

