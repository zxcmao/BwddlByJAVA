﻿using System.Collections.Generic;


namespace DataClass
{
    public static class TextLibrary
    {
        // 定义字符串数组
        public static readonly string[] GeneralGrade = new string[] { "", "愚钝", "普通", "精英", "天才" };

        public static readonly string[][] taskName = new string[][] {
            new string[] { "移动", "战争", "征兵", "输送" },
            new string[] { "搜索", "登用", "奖赏", "任命" },
            new string[] { "开垦", "劝商", "治水", "巡查" },
            new string[] { "同盟", "离间", "招揽", "情报" },
            new string[] { "金粮", "武器", "学院", "医馆" },
            new string[] { "存储", "读取", "环境", "退出", "结束" }
        };

        private static readonly Dictionary<TaskType, string> taskDescriptions = new Dictionary<TaskType, string>
        {
            { TaskType.None, "无" },
            { TaskType.Move, "移动" },
            { TaskType.OverMove, "移动过多" },
            { TaskType.Attack, "战争" },
            { TaskType.OverAttack,"攻城频繁" },
            { TaskType.Conscript, "征兵" },
            { TaskType.Assign,"分配" },
            { TaskType.Transport, "输送" },
            { TaskType.End, "结束" },
            { TaskType.Search, "搜索" },
            { TaskType.SearchMoney, "收缴金钱" },
            { TaskType.SearchFood, "征得粮食" },
            { TaskType.SearchTreasure, "慧眼识珠" },
            { TaskType.SearchGeneral, "慧眼识英" },
            { TaskType.SearchNothing, "一无所获" },
            { TaskType.Employ, "登用" },
            { TaskType.EmploySuccess,  "如鱼得水" },
            { TaskType.EmployDeny, "遗珠之憾" },
            { TaskType.EmployNothing, "野无遗贤" },
            { TaskType.OverEmploy, "置吏已满" },
            { TaskType.Reward, "奖赏" },
            { TaskType.RewardDeny, "众志成城" },
            { TaskType.Appoint, "任命" },
            { TaskType.AppointDeny, "坐镇中枢"},
            { TaskType.Lack, "财匮力绌"},
            { TaskType.Reclaim, "开垦" },
            { TaskType.OverReclaim, "五谷丰登" },
            { TaskType.Mercantile, "劝商" },
            { TaskType.OverMercantile, "金龙满仓" },
            { TaskType.Tame, "治水" },
            { TaskType.OverTame, "治水饱和" },
            { TaskType.Patrol, "巡查"},
            { TaskType.OverPatrol, "巡查饱和" },
            { TaskType.Truce, "议和" },
            { TaskType.OverTruce, "天下太平" },
            { TaskType.TruceSelect, "议和" },
            { TaskType.Alienate, "离间" },
            { TaskType.Intelligence, "情报" },
            { TaskType.Bribe, "招揽" },
            { TaskType.Shop, "购置金粮" },
            { TaskType.LackShop, "米市荒芜" },
            { TaskType.Smithy, "武器" },
            { TaskType.LackSmithy, "武备稀缺" },
            { TaskType.School, "书院" },
            { TaskType.LackSchool, "学海无舟" },
            { TaskType.SchoolDeny, "无将肯学" },
            { TaskType.Hospital, "医馆" },
            { TaskType.LackHospital, "医所难觅" },
            { TaskType.HospitalDeny, "龙精虎猛" },
            { TaskType.Save, "存储" },
            { TaskType.Load, "读取" },
            { TaskType.Settings, "环境" },
            { TaskType.Exit, "退出" },


        };

        public static string GetTaskDescription(this TaskType taskType)
        {
            return taskDescriptions[taskType];
        }

        public static readonly string[][] skillsName = new string[][] {
            new string[] { "沉着", "鬼谋", "百出", "军师", "火攻", "神算", "反计", "待伏", "袭粮", "内讧" },
            new string[] { "骑神", "骑将", "弓神", "弓将", "水将", "乱战", "连弩", "金刚", "不屈", "猛将" },
            new string[] { "单骑", "奇袭", "铁壁", "攻城", "守城", "神速", "攻心", "精兵", "军魂", "军神" },
            new string[] { "王佐", "仁政", "屯田", "商才", "名士", "风水", "义军", "内助", "仁义", "抢运" },
            new string[] { "统领", "掠夺", "恐吓", "一骑", "水练", "能吏", "练兵", "言教", "冷静", "束缚" }
        };

        public static readonly string[] skillNames = new string[] {
            "沉着", "鬼谋", "百出", "军师", "火攻", "神算", "反计", "待伏", "袭粮", "内讧",
            "骑神", "骑将", "弓神", "弓将", "水将", "乱战", "连弩", "金刚", "不屈", "猛将",
            "单骑", "奇袭", "铁壁", "攻城", "守城", "神速", "攻心", "精兵", "军魂", "军神",
            "王佐", "仁政", "屯田", "商才", "名士", "风水", "义军", "内助", "仁义", "抢运",
            "统领", "掠夺", "恐吓", "一骑", "水练", "能吏", "练兵", "言教", "冷静", "束缚"
        };

        public static readonly string[][] DoThingsResultInfo = new string[][] {
            new string[] { "末将即刻出发!", "移动将领过多!", "誓死拿下敌军城池!", "本月已攻城两次,将士都疲惫不堪", "" },
            new string[] { "吾等愿缔结秦晋之好,共图霸业", "道不同不相为谋", "财资不足", "今已天下无敌！", "" },
            new string[] { "此城置吏已足", "野无遗贤", "愿效犬马之劳 以报知遇之恩", "志不在此 恐负所托", "将士皆忠心不贰!", "承蒙厚恩 吾愿肝脑涂地!", "吾誓与此城同命!", "此城乃君主治所!" },
            new string[] { "此城农业水平提高了", "此城商业水平提高了",  "此城抗灾能力提高了", "此城秩序更加稳定人口增加了", "此城田土广辟", "此城人民丰赡", "此城无惧水旱", "此城户口殷盛，百姓安居乐业" },
            new string[] { "交易成功", "成功购得武器", "马匹", "武将智力得到提高", "吾已康复无恙" },
            new string[] { "离间成功", "离间失败", "招揽成功", "招揽失败", "破坏", },
            new string[] { "一无所获!", "征得粮草:", "获得黄金", "偶遇贤才", "寻得宝物"},
            new string[] { "此城中无粮店!", "此城中无工匠铺！", "此城中无书院！", "此城中无医馆！", "真乃神兵利器", "此器不合吾用", "学习？学个屁!", "将士均龙精虎猛"}
        };

        public static readonly string[] PlanName = new string[] {
            "火计", "陷阱", "虚兵", "要击", "乱水",
            "劫粮", "共杀", "伪击转杀", "烧粮", "落石", 
            "连环", "伏兵", "水攻", "连弩", "劫火",
            "奇门遁甲"
        };

        public static readonly string[] planResult = new string[] {
            "游戏开始", "游戏结束", "游戏胜利", "游戏失败"
        };

        public static readonly string[] g_java_lang_String_array1d_static_fld = new string[] {
            "前进", "待机", "撤退", "包围", "计策"
        };

        public static readonly string[] h = new string[] {
            "平", "山", "水"
        };

        public static readonly string[] h2 = new string[] {
            "C", "B", "A", "S"
        };

        public static readonly string[][] gfString = new string[][] {
            new string[] { "将攻", "骑攻", "弩攻", "步攻" },
            new string[] { "将防", "骑防", "弩防", "步防" }
        };

        public static readonly string[] i = new string[] {
            "春", "夏", "秋", "冬"
        };

        public static readonly string[] j = new string[] {
            "左", "右"
        };

        public static readonly string[] k = new string[] {
            "上", "下"
        };

        public static readonly string[] l = new string[] {
            " 经过多年征战,终于实现了宇内混一!", " 势力被消灭了,全国局势仍处于混乱之中..."
        };

        public static readonly string[] TacticNames = new string[] {
            "挑战", "咒缚", "连弩", "呐喊", "火矢", "爆炎"
        };

        public static readonly string[] n = new string[] {
            " ", "咒", " ", "弩", "士", "火"
        };

        public static readonly string[] o = new string[] {
            "", "击毙敌将!", "我方武将阵亡!", "擒获敌将!", "我方武将被俘!", "敌将逃窜!", "我方武将已撤退!"
        };

        public static readonly string[] p = new string[] {
            "音乐", "音效", "音量"
        };

        public static readonly string[] q = new string[] {
            "开启", "关闭"
        };

        public static readonly string[] r = new string[] {
            "微", "低", "中", "高", "强"
        };

        // 计划解释
        public static readonly string[] PlanExplain = new string[] {
            "用于平原、树林的敌军，烧伤敌兵。范围：5，需机动力：6",
            "用于山地、树林的敌军，减少敌将的体力。范围：5，需机动力：5",
            "用于山地、树林的敌军，限制敌军移动。范围：5，需机动力：4",
            "用于山地、树林的敌军，杀伤敌兵。范围：5，需机动力：6",
            "用于水中的敌军，淹没敌兵。范围：5，需机动力：7",
            "用于平原、山地、树林、村庄中的敌军主将，劫获敌军粮食。范围：2，需机动力：8",
            "用于山地的敌军，且至少有一支敌军与该敌军相邻，对该敌军及周围敌军造成重大伤害。范围：2，需机动力：8",
            "用于城中的敌军，且我军与城池相邻。范围：1，需机动力：8",
            "用于树林、森林的敌军主将，烧毁敌军粮草。范围：2，需机动力：10",
            "我军在山地或城中，用于非城中的敌军，杀伤敌兵，减少敌将体力。范围：2，需机动力：9",
            "用于水中的敌军，且至少有一支敌军与该敌军相邻，限制该敌军和周围敌军的行动。需机动力：10",
            "我军在山地或森林中，用于非城中的敌军，对敌军造成重大伤害，且敌军一日内无法行动。范围：2，需机动力：10",
            "用于水中的敌军，且至少有一支敌军与该敌军相邻。对该敌军及周围敌军造成重大伤害。范围：3,需机动力：12",
            "我军在城中或山地，用于非城中的敌军，对敌军造成重大伤害。范围：2，需机动力：10",
            "用于树林的敌军，且至少有两支敌军与该敌军相邻，对该敌军及周围敌军造成重大伤害。范围：3，需机动力：12",
            "用于森林、山地，施用此计,我军自损100兵,但如果敌军进入此地,则10日内无法行动且每日损失一定的士兵.需机动力:10"
        };

        public static readonly string[] PlanResult = new string[] {
            "计策失败",
            "士兵数减少",
            "军队中计,被困",
            "武将体力减少",
            "粮食减少",
            "奇门遁甲已布置完毕",
            "陷入奇门遁甲之计"
        };

        // 战术说明
        public static readonly string[] TacticExplain = new string[] {
            "向敌将发出单挑邀请.耗费2个计策点",
            "限制敌将行动.耗费5个计策点", 
            "箭手射程增加.耗费7个计策点", 
            "全军攻击力增加.耗费8个计策点", 
            "箭手攻击力大大增加.耗费10个计策点", 
            "我方兵力大于500.利用火器攻击敌军.耗费12个计策点"
        };

        // 获得装备
        public static readonly string[] u = new string[] {
            "获得兵器 青釭剑", "获得防具 龙鳞宝铠", "获得兵器 倚天剑", "获得兵器 七星剑", "获得兵器 龙牙刀"
        };

        // 战斗字符串
        public static readonly string[] SoloInfo = new string[] {
            "攻击敌方中...",
            "成功招降!",
            "招降失败!",
            "自军成功撤离!",
            "撤退失败...",
            "我方投降!",
            "我方武将阵亡!",
            "敌方武将行动中....",
            "若果真如此,吾等愿降",
            "招降失败!",
            "敌方武将逃窜!",
            "敌方遁逃失败!",
            "敌方武将归降!",
            "这等武艺也敢上阵为将?",
            "良禽择木而栖",
            "吾宁头断,安能降汝!",
            "吾受天命,岂可降人?"
        
        };

        // 游戏胜利评语
        public static readonly string[][] gameScore = new string[][] {
            new string[] {
                "极度贫瘠的土地, 根本无法生产出粮食,饿殍遍野,百姓苦不堪言",
                "土地资源非常匮乏,粮食收成非常低下,各地百姓饿死现象时有发生,无法继续生活下去",
                "农业发展较为落后,粮食产量仍无法满足百姓的生活,各地田里,地里一片菜色",
                "随着农业地逐步发展,粮食产量基本达到了百姓生活地需求",
                "随着土地地不断开发,粮食产量已较为富裕,百姓吃饭问题已完全解决了",
                "新的农业工具地不断出现,在土地作业中的逐渐普及,各地粮食已极为充裕",
                "农业发展空前,粮食年年丰产,人民衣食无忧"
            }, new string[] {
                "极度落后的商业,人们最基本的物品交换都无法进行.国家贫苦不堪",
                "商业资源非常贫乏,人们无法得到最基本的生活用品,生活非常艰辛",
                "商业发展仍较为落后,商务活动较少, 百姓生活水平非常低下",
                "随着商业地逐步发展,商务活动基本达到了百姓生活地需求",
                "随着官府的不断重视,商业发展已较为发达,国家经济状况有了很大改善",
                "与外国的经济往来,使得国家商务发展极度发达,百姓生活在了一个富裕的国家中",
                "国家的经济发展举世罕见, 新的领域的经济往来不断增强本国的经济实力"
            },
            new string[] {
                "瘟疫,蝗虫,旱涝,各种灾害遍地,民不聊生",
                "瘟疫等这种灾害仍时有发生,百姓灾难深重",
                "由于新国家官府的一定重视,灾害在一定程度上得到了遏制",
                "经过官府的不断努力,灾害防御措施逐渐实施,百姓基本远离了灾害痛苦",
                "大坝建成,各种治理灾害措施井然有序,百姓再无灾害之忧"
            },
            new string[] {
                "经过多年的战争,各地一片荒芜,各城市人丁稀少,百里难见一人",
                "战后恢复仍没有足够的进行,各城市劳动力资源仍稍显不足",
                "战争的硝烟逐渐远去,各城市人口资源已完全满足了劳动的需求",
                "国家开始重视了城市的建设,城市规模发展稳定,大型城市逐步兴起",
                "经过国家多方面努力,城市向都市化发展开始,各城市欣欣向荣",
                "各地发展平衡,都市化发展完成,国家发展空前,百姓安居乐业"
            },
            new string[] {
                "部下野心勃勃,叛变,反乱事件层出不穷,国家马上将被颠覆",
                "属下人心浮动,流言四起,某些武将的叛乱似乎已在眼前",
                "大多武将忠于君主,但奸臣当道,叛变的种子仍远未消失",
                "君主赏罚分明,恩威并施,下属武将对国家较为忠诚",
                "君主善于御人之道, 武将们对君主, 国家忠心耿耿",
                "以恩义著称的新君主,得到了属下誓死效忠,各武将绝无二心",
                "经过多年的同生共死,臣僚们用鲜血来发誓,子子孙孙誓死效忠君主"
            },
            new string[] {
                "新国家的所作所为,百姓深恶痛疾,各地烽烟四起,起义遍地,无法控制",
                "新国家并没有实施仁政,百姓对新国家非常不满, 起义的种子四处萌发",
                "新国家的仁政思想逐渐传播,逐渐使得百姓对新的国家基本上认可了",
                "君主一向以仁政而闻名,国家建立以来,得到了各地百姓的支持和欢迎",
                "君主贤名远播,管理国家政策得当,施以百姓的仁政深入人心",
                "全国百姓坚决拥护新的国家,君主已成为大众的偶像,和精神砥柱"
            },
            new string[] {
                "多年战争造成国家兵甲极度不足,远远无法抵御外敌侵略,随时有灭国危险",
                "战后兵甲仍然不足,无法抵御匈奴等外敌的大规模进攻,国家岌岌可危",
                "匈奴,外族虎视耽耽,全部兵丁已派往前线,与强大的敌人达成微弱的军事平衡",
                "在战争积累了足够军事经验,国家军事实力稳定,在随后的战争中数次战败外侵略者",
                "武器,阵法的更新,将领的培养,使得军事实力强大,征服了许多周围的小国家",
                "军事实力空前,对周边国家形成了强大的震慑作用.各国纷纷遣使进贡,从此强大的帝国已经产生"
            }
        };

        public static readonly string gameFail = 
            "将军莫哀，战事无常，胜败乃兵家常事。昔日高祖数败于项羽，终能乌江一战而定乾坤。" + "\n" + 
            "今汝虽有小挫，然英勇之志，不减当年。兵法云：知己知彼，百战不殆。愿将军反思此役之得失，汲取教训，待来日重整旗鼓，再图大业。" + "\n" + 
            "且莫忘，三国乱世，英雄辈出，谁人不曾历风雨？刘备三顾茅庐，方得卧龙相助；曹操赤壁虽败，犹能东山再起。汝之才智，岂会困于一时之失？" + "\n" + 
            "望汝勿坠青云之志，以坚韧不拔之心，继续前行。待那时，必将风云变色，天下震惊。吾等在此，静候将军凯旋之音，共饮庆功之酒。";
        
        // 静态字符串字段
        public static readonly string a_java_lang_String_static_fld = "君主不能投降!";
        public static readonly string not_java_lang_String_static_fld = "对方君主拒绝接受!";
        public static readonly string b_java_lang_String_static_fld = "请选择继位君主";
        public static readonly string c_java_lang_String_static_fld = "请选择登用武将";
        public static readonly string d_java_lang_String_static_fld = "选择自建武将";
        public static readonly string e_java_lang_String_static_fld = "任命自建君主";
        public static readonly string f_java_lang_String_static_fld = "删除自建武将";
        public static readonly string g_java_lang_String_static_fld = "选择一个空城建都";

        // 字符串数组
        public static readonly string[] v = new string[] {
            "(网)", "(网)", "(临)"
        };

        // 游戏剧本
        public static readonly string[] gameScript = new string[] {
            "《群雄起源》", "《十八路反董》"
        };

        // 文件路径
        public static readonly string[] w = new string[] {
            "lib200.jg", "lib201.jg"
        };

        // 等级标识
        public static readonly string[] nd = new string[] {
            "", "(初级)", "(中级)", "(超级)"
        };
    
        // 地形攻防系数
        public static readonly float[] TopInf = { 0.8f, 0.9f, 1.0f, 1.1f };

        // 地形骑、弓、步兵比例
        public static readonly byte[][][] CAI_Proportion =
        {
            // 平原
            new[]
            {
                new byte[] { 0, 4, 6 }, // 平C
                new byte[] { 2, 4, 4 }, // 平B
                new byte[] { 4, 3, 3 }, // 平A
                new byte[] { 6, 2, 2 }  // 平S
            },
            // 山地
            new[]
            {
                new byte[] { 0, 4, 6 }, // 山C
                new byte[] { 2, 4, 4 }, // 山B
                new byte[] { 4, 4, 2 }, // 山A
                new byte[] { 6, 2, 2 }  // 山S
            },
            // 水域
            new[]
            {
                new byte[] { 0, 2, 8 }, // 水C
                new byte[] { 0, 4, 6 }, // 水B
                new byte[] { 0, 6, 4 }, // 水A
                new byte[] { 0, 8, 2 }  // 水S
            },
            // 城战
            new[]
            {
                new byte[] { 0, 10, 0 }, // 守城7+
                new byte[] { 0, 8, 2 },  // 守城5+
                new byte[] { 0, 6, 4 },  // 守城3+
                new byte[] { 0, 4, 6 }   // 守城<3
            }
        };

        public static readonly float[] hj = new float[] { 1.0F, 1.01F, 1.01F, 1.01F, 1.02F, 1.02F, 1.02F, 1.03F, 1.03F, 1.04F, 1.04F, 1.04F, 1.05F, 1.05F,
            1.05F, 1.06F, 1.06F, 1.06F, 1.07F, 1.07F, 1.08F, 1.08F, 1.08F, 1.09F, 1.09F, 1.09F, 1.1F, 1.1F, 1.11F,
            1.11F, 1.11F, 1.12F, 1.12F, 1.13F, 1.13F, 1.13F, 1.14F, 1.14F, 1.14F, 1.15F, 1.15F, 1.16F, 1.16F, 1.16F,
            1.17F, 1.17F, 1.18F, 1.18F, 1.19F, 1.19F, 1.19F, 1.2F, 1.2F, 1.21F, 1.21F, 1.21F, 1.22F, 1.22F, 1.23F,
            1.23F, 1.24F, 1.24F, 1.24F, 1.25F, 1.25F, 1.26F, 1.26F, 1.27F, 1.27F, 1.27F, 1.28F, 1.28F, 1.29F, 1.29F,
            1.3F, 1.3F, 1.31F, 1.31F, 1.31F, 1.32F, 1.32F, 1.33F, 1.33F, 1.34F, 1.34F, 1.35F, 1.35F, 1.36F, 1.36F,
            1.37F, 1.37F, 1.38F, 1.38F, 1.39F, 1.39F, 1.39F, 1.4F, 1.4F, 1.41F, 1.41F, 1.42F, 1.42F, 1.43F, 1.43F,
            1.44F, 1.44F, 1.45F, 1.45F, 1.46F, 1.46F, 1.47F, 1.47F, 1.48F, 1.48F, 1.49F, 1.49F, 1.5F, 1.51F, 1.51F,
            1.52F, 1.52F, 1.53F, 1.53F, 1.54F, 1.54F, 1.55F, 1.55F, 1.56F, 1.56F, 1.57F, 1.57F, 1.58F, 1.59F, 1.59F,
            1.6F, 1.6F, 1.61F, 1.61F, 1.62F, 1.62F, 1.63F, 1.64F, 1.64F, 1.65F, 1.65F, 1.66F, 1.66F, 1.67F, 1.68F,
            1.68F, 1.69F, 1.69F, 1.7F, 1.71F, 1.71F, 1.72F, 1.72F, 1.73F, 1.74F, 1.74F, 1.75F, 1.75F, 1.76F, 1.77F,
            1.77F, 1.78F, 1.78F, 1.79F, 1.8F, 1.8F, 1.81F, 1.82F, 1.82F, 1.83F, 1.83F, 1.84F, 1.85F, 1.85F, 1.86F,
            1.87F, 1.87F, 1.88F, 1.89F, 1.89F, 1.9F, 1.91F, 1.91F, 1.92F, 1.93F, 1.93F, 1.94F, 1.95F, 1.95F, 1.96F,
            1.97F, 1.97F, 1.98F, 1.99F, 1.99F, 2.0F, 2.01F, 2.01F, 2.02F, 2.03F, 2.03F, 2.04F, 2.05F, 2.06F, 2.06F,
            2.07F, 2.08F, 2.08F, 2.09F, 2.1F, 2.11F, 2.11F, 2.12F, 2.13F, 2.14F, 2.14F, 2.15F, 2.16F, 2.17F, 2.17F,
            2.18F, 2.19F, 2.2F, 2.2F, 2.21F, 2.22F, 2.23F, 2.23F, 2.24F, 2.25F, 2.26F, 2.27F, 2.27F, 2.28F, 2.29F,
            2.3F, 2.31F, 2.31F, 2.32F, 2.33F, 2.34F, 2.35F, 2.35F, 2.36F, 2.37F, 2.38F, 2.39F, 2.39F, 2.4F, 2.41F,
            2.42F, 2.43F, 2.44F, 2.45F, 2.45F, 2.46F, 2.47F, 2.48F, 2.49F, 2.5F, 2.51F, 2.51F, 2.52F, 2.53F, 2.54F,
            2.55F, 2.56F, 2.57F, 2.58F, 2.58F, 2.59F, 2.6F, 2.61F, 2.62F, 2.63F, 2.64F, 2.65F, 2.66F, 2.67F, 2.68F,
            2.69F, 2.69F, 2.7F, 2.71F, 2.72F, 2.73F, 2.74F, 2.75F, 2.76F, 2.77F, 2.78F, 2.79F, 2.8F, 2.81F, 2.82F,
            2.83F, 2.84F, 2.85F, 2.86F, 2.87F, 2.88F, 2.89F, 2.9F, 2.91F, 2.92F, 2.93F, 2.94F, 2.95F, 2.96F, 2.97F,
            2.98F, 2.99F, 3.0F, 3.01F, 3.02F, 3.03F, 3.04F, 3.05F, 3.06F, 3.07F, 3.08F, 3.1F, 3.11F, 3.12F, 3.13F,
            3.14F, 3.15F, 3.16F, 3.17F, 3.18F, 3.19F, 3.2F, 3.22F, 3.23F, 3.24F, 3.25F, 3.26F, 3.27F, 3.28F, 3.29F,
            3.31F, 3.32F, 3.33F, 3.34F, 3.35F, 3.36F, 3.38F, 3.39F, 3.4F, 3.41F, 3.42F, 3.43F, 3.45F, 3.46F, 3.47F,
            3.48F, 3.49F, 3.51F, 3.52F, 3.53F, 3.54F, 3.56F, 3.57F, 3.58F, 3.59F, 3.61F, 3.62F, 3.63F, 3.64F, 3.66F,
            3.67F, 3.68F, 3.69F, 3.71F, 3.72F, 3.73F, 3.75F, 3.76F, 3.77F, 3.78F, 3.8F, 3.81F, 3.82F, 3.84F, 3.85F,
            3.86F, 3.88F, 3.89F, 3.9F, 3.92F, 3.93F, 3.94F, 3.96F, 3.97F, 3.99F, 4.0F, 4.01F, 4.03F, 4.04F, 4.06F,
            4.07F, 4.08F, 4.1F, 4.11F, 4.13F, 4.14F, 4.16F, 4.17F, 4.18F, 4.2F, 4.21F, 4.23F, 4.24F, 4.26F, 4.27F,
            4.29F, 4.3F, 4.32F, 4.33F, 4.35F, 4.36F, 4.38F, 4.39F, 4.41F, 4.42F, 4.44F, 4.45F, 4.47F, 4.48F, 4.5F,
            4.52F, 4.53F, 4.55F, 4.56F, 4.58F, 4.59F, 4.61F, 4.63F, 4.64F, 4.66F, 4.68F, 4.69F, 4.71F, 4.72F, 4.74F,
            4.76F, 4.77F, 4.79F, 4.81F, 4.82F, 4.84F, 4.86F, 4.87F, 4.89F, 4.91F, 4.92F, 4.94F, 4.96F, 4.98F, 4.99F,
            5.01F, 5.03F, 5.05F, 5.06F, 5.08F, 5.1F, 5.12F, 5.13F, 5.15F, 5.17F, 5.19F, 5.21F, 5.22F, 5.24F, 5.26F,
            5.28F, 5.3F, 5.31F, 5.33F, 5.35F, 5.37F, 5.39F, 5.41F, 5.43F, 5.45F, 5.46F, 5.48F, 5.5F, 5.52F, 5.54F,
            5.56F, 5.58F, 5.6F, 5.62F, 5.64F, 5.66F };
    }
}


