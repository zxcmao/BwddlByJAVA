namespace BaseClass
{
    public enum SmithyResult
    {
        Success,
        Fail,
        NotEnoughMoney,
        Cancel
    }

    [System.Serializable]// 武器实体类
    public class Weapon
    {
        // 武器ID
        public byte weaponId { get; set; }
        // 武器名称
        public string weaponName { get; set; }
        // 武器价格
        public short weaponPrice { get; set; }
        // 武器属性
        public byte weaponProperties { get; set; }
        // 武器重量
        public byte weaponWeight { get; set; }
        // 武器类型
        public byte weaponType { get; set; }
        // 武器唯一性
        public bool weaponUnique{ get; set; }
        // 武器所在城池武器铺
        public short smithy { get; set; }
        // 默认构造函数
        public Weapon() { }

        // 参数化构造函数
        public Weapon(byte weaponId, string weaponName, short weaponPrice, byte weaponProperties, byte weaponWeight, byte weaponType)
        {
            this.weaponId = weaponId;
            this.weaponName = weaponName;
            this.weaponPrice = weaponPrice;
            this.weaponProperties = weaponProperties;
            this.weaponWeight = weaponWeight;
            this.weaponType = weaponType;
        }
        
        //单挑会掉落武器的检测方法
        public bool CanSeized()
        {
            // 检查是否符合特定ID并设置相关标志
            if (weaponId == 5 || weaponId == 6 || weaponId == 7 || weaponId == 14 || weaponId == 15 || weaponId == 21 || weaponId == 23
                || weaponId == 30 || weaponId == 31)
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