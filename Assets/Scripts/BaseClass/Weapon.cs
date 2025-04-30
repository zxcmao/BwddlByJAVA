namespace BaseClass
{
    [System.Serializable]// 武器实体类
    public class Weapon
    {
        public byte weaponID;     // 武器ID
        public string weaponName; // 武器名称
        public short price;       // 武器价格
        public byte property;     // 武器属性
        public byte weight;       // 武器重量
        public byte kind;         // 武器类型
        public bool isUnique;     // 武器唯一性
        public short smithy;      // 武器所在城池武器铺
        
        
        //单挑会掉落武器的检测方法
        public bool CanSeized()
        {
            // 检查是否符合特定ID并设置相关标志
            if (weaponID == 5 || weaponID == 6 || weaponID == 7 || weaponID == 14 || weaponID == 15 || weaponID == 21 || weaponID == 23
                || weaponID == 30 || weaponID == 31)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}