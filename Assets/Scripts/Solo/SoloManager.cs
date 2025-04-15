using System;
using BaseClass;
using Battle;
using DataClass;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SoloAction
{
    Pin,  // 牵制
    Attack,   // 攻击
    Whack, // 全力一击
    Persuade, // 劝降
    Retreat,  // 逃跑
    Surrender // 投降
}

public class SoloManager
{
    private General hmGeneral;
    private General aiGeneral;
    private short _hmInitialHp;
    private short _aiInitialHp;
    public bool seizedWeapon;
    
    public SoloManager(General hmGeneral, General aiGeneral)
    {
        this.hmGeneral = hmGeneral;
        this.aiGeneral = aiGeneral;
        _hmInitialHp = hmGeneral.GetCurPhysical();
        _aiInitialHp = aiGeneral.GetCurPhysical();
    }
    
    //单挑会掉落武器的检测方法
    public void CheckWeaponDrop()
    {
        seizedWeapon = false;
        short weaponId = aiGeneral.weapon;
        short armorId = aiGeneral.armor;
        // 检查AI将领是否符合特定ID并设置相关标志
        if (weaponId == 5 || weaponId == 6 || weaponId == 7 || weaponId == 14 || weaponId == 15 || weaponId == 21 || weaponId == 23
            && armorId == 30 || armorId == 31)
        {
            seizedWeapon = true;
        }
    }

    public string ObtainWeaponOrArmor()
    {
        if (!seizedWeapon) return null;
        // 如果为true，根据AI将领武器ID分配武器或防具
        Weapon weapon = WeaponListCache.GetWeapon(aiGeneral.weapon);
        Weapon armor = WeaponListCache.GetWeapon(aiGeneral.armor);
        if (weapon.weaponId == 6)
        {
            hmGeneral.weapon = 6;
            return weapon.weaponName;
        }
        if (armor.weaponId == 30)
        {
            hmGeneral.armor = 30;
            return armor.weaponName;
        }
        if (armor.weaponId == 31)
        {
            hmGeneral.armor = 31;
            return armor.weaponName;
        }
        if (weapon.weaponId == 7)
        {
            hmGeneral.weapon = 7;
            return weapon.weaponName;
        }
        if (weapon.weaponId == 5)
        {
            hmGeneral.weapon = 5;
            return weapon.weaponName;
        }
        if (weapon.weaponId == 14)
        {
            hmGeneral.weapon = 14;
            return weapon.weaponName;
        }
        return null;
    }
    
    // 单挑连击方法
    public bool ComboTrigger(General atkGeneral)
    {
        Weapon weapon = WeaponListCache.GetWeapon(atkGeneral.weapon);
        Weapon armor = WeaponListCache.GetWeapon(atkGeneral.armor);
        if (weapon.weaponId == 15 || weapon.weaponId == 21 || weapon.weaponId == 22 || weapon.weaponId == 23)
        {
            if (Random.Range(0,6)<1)
            {
                Debug.Log(atkGeneral.generalName+"连击");
                return true;
            }
        }
        if (atkGeneral.generalName == "吕布")
        {
            if (Random.Range(0,6)<1)
            {
                Debug.Log(atkGeneral.generalName+"连击");
                return true;
            }
        }
        if (atkGeneral.generalName == "关羽" || atkGeneral.generalName == "张飞" || atkGeneral.generalName == "赵云" || 
            atkGeneral.generalName == "许褚" || atkGeneral.generalName == "马超") 
        {
            if (Random.Range(0,10)<1)
            {
                Debug.Log(atkGeneral.generalName+"连击");
                return true;
            }
        }
        byte totalWeight = (byte) (weapon.weaponWeight + armor.weaponWeight);
        if (Random.Range(0,101) - totalWeight >= 70)
        {
            Debug.Log(atkGeneral.generalName+"连击");
            return true;
        }
        return false;
    }

    // 单人战斗中增加战斗经验的方法
    public void HandleSoloExp()
    {
        int hmCurPhysical = 0;
        if (hmGeneral != null)
            hmCurPhysical = hmGeneral.GetCurPhysical(); // 获取玩家当前体力

        int aiCurPhysical = 0;
        if (aiGeneral != null)
            aiCurPhysical = aiGeneral.GetCurPhysical(); // 获取AI当前体力

        // 计算经验值
        int aiExp = (int)((_hmInitialHp - hmCurPhysical) * 1.2D);
        int hmExp = _aiInitialHp - aiCurPhysical;

        // 设置经验值的下限和上限
        if (aiExp <= 0)
            aiExp = 5;
        if (hmExp < 0)
            hmExp = 2;
        if (aiExp > 100)
            aiExp = 100;
        if (hmExp > 100)
            hmExp = 100;

        // 添加将领经验
        aiGeneral.AddForceExp(aiExp);
        hmGeneral.AddForceExp(hmExp);

        // 重置初始体力
        _hmInitialHp = 0;
        _aiInitialHp = 0;
    }
    
    // 判断玩家逃跑结果
    public bool Escape(General hmGeneral, General aiGeneral)
    {
        return true;
        // 如果条件玩家单挑逃跑为true，直接返回false，不执行后续逻辑
        if (BattleManager.Instance.aiTroops[0].GetAllAttackableDirection(BattleManager.Instance.aiTroops[0].arrayPos.x,BattleManager.Instance.aiTroops[0].arrayPos.y).Count == 4)
        {
            if (hmGeneral.generalName == "赵云")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 获取玩家与AI将领的力量差值，除以20用于计算概率
        int i1 = (hmGeneral.force - aiGeneral.force) / 20;

        // 使用随机数生成器，判断是否满足指定概率条件
        return (Random.Range(0,10) < 4 + i1);
    }

    // 判断AI逃跑结果
    public bool AIEscape(General hmGeneral, General aiGeneral)
    {
        // 获取双方将领的武力差并除以15作为影响因素
        int i1 = (aiGeneral.force - hmGeneral.force) / 15;

        // 通过随机数来判断结果
        return (Random.Range(0,10) < 10 + i1);
    }
    
    // 原低难度AI单挑指令
    byte byte_ss_a(short word0, short word1)
    {
        // 当前体力低于20且随机数为偶数，返回4
        if (GeneralListCache.GetGeneral(word0).GetCurPhysical() < 20 && Random.Range(0, 3) == 0)
            return 4;

        // 当前体力低于最大体力的一半且对方体力比自身高10，随机数小于3时返回2
        if (GeneralListCache.GetGeneral(word0).GetCurPhysical() < GeneralListCache.GetGeneral(word0).maxPhysical / 2 &&
            GeneralListCache.GetGeneral(word1).GetCurPhysical() - GeneralListCache.GetGeneral(word0).GetCurPhysical() > 10 &&
            Random.Range(0, 11) < 3)
            return 2;

        // 当前体力低于对方体力时，根据随机数返回0或1
        if (GeneralListCache.GetGeneral(word0).GetCurPhysical() < GeneralListCache.GetGeneral(word1).GetCurPhysical())
            return (byte)((Random.Range(0, 11) >= 6) ? 0 : 1);

        // 否则根据随机数返回1或0
        return (byte)((Random.Range(0, 11) >= 6) ? 1 : 0);
    }
    

    static byte WeaponEffect(General atkGeneral, General defGeneral, byte sc, bool whack)
    {
        // 获取进攻方的武器类型和防守方的防具类型
        byte a = atkGeneral.weapon;
        byte b = defGeneral.armor;

        // 根据武器和防具的类型调整sc的值
        if (a == 5 && whack)
            sc = (byte)(sc + 20);
        if ((a == 6 || a == 7 || a == 14) && !whack)
            sc = (byte)(sc + 15);
        if (b == 30 && !whack)
            sc = (byte)(sc - 15);
        if (b == 31 && !whack)
            sc = (byte)(sc - 20);
        else if (b == 31 && whack)
            sc = 0;

        // 返回调整后的sc
        return sc;
    }

    // 武器命中率
    public static byte GetPercentage(General atkGeneral, General defGeneral, bool whack, bool hmAtk)
    {
        byte sc = 0;
        byte atkGenWeaponType = WeaponListCache.GetWeapon(atkGeneral.weapon).weaponType;
        byte defGenWeaponType = WeaponListCache.GetWeapon(defGeneral.weapon).weaponType;

        // 根据双方的武器类型设置基础命中率
        if (atkGenWeaponType == 0 && defGenWeaponType == 0)
            sc = 70;
        else if (atkGenWeaponType == 0 && defGenWeaponType == 1)
            sc = 75;
        else if (atkGenWeaponType == 0 && defGenWeaponType == 2)
            sc = 60;
        else if (atkGenWeaponType == 1 && defGenWeaponType == 0)
            sc = 55;
        else if (atkGenWeaponType == 1 && defGenWeaponType == 1)
            sc = 60;
        else if (atkGenWeaponType == 1 && defGenWeaponType == 2)
            sc = 70;
        else if (atkGenWeaponType == 2 && defGenWeaponType == 0)
            sc = 80;
        else if (atkGenWeaponType == 2 && defGenWeaponType == 1)
            sc = 50;
        else if (atkGenWeaponType == 2 && defGenWeaponType == 2)
            sc = 60;

        // 根据攻击方式调整命中率
        if (hmAtk)
            sc = (byte)(sc - 20);
        if (whack && hmAtk)
            sc = (byte)(sc / 3);
        else if (whack && !hmAtk)
            sc = (byte)(sc / 2);

        // 使用weaponEffect方法进一步调整命中率
        sc = WeaponEffect(atkGeneral, defGeneral, sc, whack);
        Debug.Log("命中率：" + sc);
        // 返回最终的命中率
        return sc;
    }

    
    
    // 我方是否招降AI将领？
    public bool Persuade(General doGeneral, General beGeneral)
    {
        if (beGeneral.GetLoyalty() > 99)
            return false;  // 如果将领忠诚度大于99，不招降

        int i1 = (doGeneral.moral * 7 + doGeneral.IQ * 3) * 10 / 99;  // 计算奖励值
        i1 = (i1 - 70) / 5;  // 调整招降值
        if (i1 == 4)
        {
            i1 = 6;  // 招降值为6
        }
        else if (i1 == 5)
        {
            i1 = 8;  // 招降值为8
        }
        i1 = beGeneral.GetCurPhysical() + beGeneral.GetLoyalty() - i1 + Random.Range(0, 5);  // 计算最终值
        return (i1 < 100);  // 如果最终值小于100，则返回true
    }

    /// <summary>
    /// AI是否招降我方将领？
    /// </summary>
    /// <param name="aiGenId"></param>
    /// <param name="hmGenId"></param>
    /// <returns></returns>
    public bool AIPersuade(General doGeneral, General beGeneral)
    {
        if (beGeneral.GetLoyalty() > 99)
            return false;  // 如果将领忠诚度大于99，不招降

        int i1 = (doGeneral.moral * 7 + doGeneral.IQ * 3) * 10 / 99;  // 计算奖励值
        i1 = (i1 - 70) / 5;  // 调整招降值
        if (i1 == 4)
        {
            i1 = 6;  // 招降值为6
        }
        else if (i1 == 5)
        {
            i1 = 8;  // 招降值为8
        }
        i1 = beGeneral.GetCurPhysical() + beGeneral.GetLoyalty() - i1 + Random.Range(0, 5);  // 计算最终值
        return (i1 < 110);  // 如果最终值小于110，则返回true
    }

    /// <summary>
    /// 计算基于将军武力值的某种攻击力或效果
    /// </summary>
    /// <param name="general"></param>
    /// <param name="atkPower"></param>
    /// <param name="defPower"></param>
    /// <returns></returns>
    static int GeneralAtkValue(General general, short atkPower, short defPower)
    {
        int i = general.force;  // 获取将军武力值
        i *= atkPower;  // 乘以参数word1
        i /= defPower;  // 除以参数word2
        if (i > 2 * general.force)
            i = 2 * general.force;  // 限制最大值
        if (i < general.force / 3)
            i = general.force / 3;  // 限制最小值
        i += i / 5;  // 增加5%的计算值
        return i;
    }

    /// <summary>
    /// 获得攻击伤害数
    /// </summary>
    /// <param name="atkGeneral"></param>
    /// <param name="atk"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public static byte GetAtkDea(General atkGeneral, short atk, short def)
    {
        byte dea = 1;  // 初始化伤害数
        dea = (byte)(GeneralAtkValue(atkGeneral, atk, def) / 8);  // 根据攻击力计算伤害数
        if (dea < 0)
            dea = 100;  // 确保伤害数不小于0
        return dea;
    }

    /// <summary>
    /// 获得全力一击伤害数
    /// </summary>
    /// <param name="atkGeneral"></param>
    /// <param name="atk"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    byte GetAllAtkDea(General atkGeneral, short atk, short def)
    {
        byte dea = 1;  // 初始化伤害数
        dea = (byte)(GeneralAtkValue(atkGeneral, atk, def) / 2);  // 根据攻击力计算伤害数
        if (dea < 0)
            dea = 100;  // 确保伤害数不小于0
        return dea;
    }

    /// <summary>
    /// 牵制攻击结果
    /// </summary>
    /// <param name="atkGeneral"></param>
    /// <param name="atk"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public byte SinglePin(General atkGeneral, short atk, short def)
    {
        byte byte0 = (byte)(GeneralAtkValue(atkGeneral, atk, def) / 20);  // 计算牵制效果
        if (byte0 > 0)
            return byte0;  // 如果效果大于0，返回效果值
        return 1;  // 否则返回1
    }

    /// <summary>
    /// 普通攻击
    /// </summary>
    /// <param name="atkGeneral"></param>
    /// <param name="defGeneral"></param>
    /// <param name="atk"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public byte SingleAtk(General atkGeneral, General defGeneral, short atk, short def)
    {
        bool hmatk = false;  // 标志是否为主要攻击
        byte hurt = 0;  // 伤害值初始化为0
        if (atkGeneral == hmGeneral)
            hmatk = true;  // 如果攻击者是HMGeneral，设为主要攻击
        if (GetPercentage(atkGeneral, defGeneral, false, hmatk) > Random.Range(0, 101))
            hurt = GetAtkDea(atkGeneral, atk, def);  // 如果命中率计算通过，计算伤害值
        return hurt;  // 返回伤害值
    }

    /// <summary>
    /// 全力一击攻击
    /// </summary>
    /// <param name="atkGeneral"></param>
    /// <param name="defGeneral"></param>
    /// <param name="atk"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public short SingleAllAtk(General atkGeneral, General defGeneral, short atk, short def)
    {
        bool hmatk = false;  // 标志是否为玩家攻击
        short hurt = 0;  // 伤害值初始化为0
        if (atkGeneral == hmGeneral)
            hmatk = true;  // 如果攻击者是玩家
        if (GetPercentage(atkGeneral, defGeneral, true, hmatk) > Random.Range(0, 101))
        {
            hurt = GetAllAtkDea(atkGeneral, atk, def);  // 如果命中率计算通过，计算全力伤害
        }
        else
        {
            hurt = (short)Random.Range(-20, -15);  // 否则计算随机伤害值
        }
        return hurt;  // 返回伤害值
    }
    
    
    
    
    // 获取 AI 当前的状态
    public SoloAction GetAISoloAction()
    {
        int aiCurPhysical = aiGeneral.GetCurPhysical();
        int hmCurPhysical = hmGeneral.GetCurPhysical();
        short aiAtkPower = aiGeneral.GetAttackPower();
        short aiDefPower = aiGeneral.GetDefendPower();
        short hmAtkPower = hmGeneral.GetAttackPower();
        short hmDefPower = hmGeneral.GetDefendPower();

        // 条件判断
        if (AIPersuade(aiGeneral, hmGeneral))
        {
            return SoloAction.Persuade; // 招降
        }
        if (SinglePin(aiGeneral, aiAtkPower, hmDefPower) >= hmCurPhysical)
        {
            return SoloAction.Pin; // 牵制
        }
        if (GetAtkDea(aiGeneral, aiAtkPower, hmDefPower) >= hmCurPhysical)
        {
            return SoloAction.Attack; // 攻击
        }
        if (aiCurPhysical < 20 || GetAtkDea(hmGeneral, hmAtkPower, aiDefPower) >= aiCurPhysical)
        {
            return SoloAction.Retreat; // 逃跑
        }
        if (aiCurPhysical > 35 &&
            GetPercentage(aiGeneral, hmGeneral, true, false) >= 30 &&
            GetAllAtkDea(aiGeneral, aiAtkPower, hmDefPower) >= hmCurPhysical &&
            GetAtkDea(aiGeneral, aiAtkPower, hmDefPower) + 5 < GetAtkDea(hmGeneral, hmAtkPower, aiDefPower) &&
            aiCurPhysical + 10 < hmCurPhysical &&
            GetPercentage(aiGeneral, hmGeneral, true, false) >= Random.Range(0, 41))
        {
            return SoloAction.Whack; // 全力一击
        }
        int qw = GetPercentage(aiGeneral, hmGeneral, false, false) * GetAtkDea(aiGeneral, aiAtkPower, hmDefPower) / 100;
        if (qw > SinglePin(aiGeneral, aiAtkPower, hmDefPower) + 1)
        {
            return SoloAction.Attack; // 攻击
        }

        return SoloAction.Pin; // 默认牵制
    }
    
    
    
    

    
}
