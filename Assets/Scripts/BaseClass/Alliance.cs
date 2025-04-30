namespace BaseClass
{
    [System.Serializable] // 此属性允许Unity序列化此类，以便在Inspector中使用
    public class Alliance
    {
        // 存储联盟所属势力ID的字段
        public byte countryId;

        // 存储联盟持续月份的字段，已在构造函数中初始化
        public byte months;

        // Alliance类的构造函数，接受势力ID和月份作为参数
        public Alliance(byte countryId, byte months)
        {
            this.countryId = countryId; // 初始化势力ID
            this.months = months;      // 初始化月份
        }

        // 联盟持续月份的属性，提供getter和setter方法
        public byte Months
        {
            get { return months; }
            set { months = value; }
        }
    }
}