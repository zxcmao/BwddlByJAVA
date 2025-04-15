/*using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameInfo;
using static TextLibrary;

public class UIResultTips : MonoBehaviour
{
    [SerializeField] GameObject TipsPanel;  // 显示任务结果的面板
    [SerializeField] RawImage SelectedHead;
    [SerializeField] TextMeshProUGUI TipsInfo;
    [SerializeField] Button CloseTipsButton;
    private static UIResultTips instance;

    private UIResultTips() { }

    public static UIResultTips GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<UIResultTips>();
        }
        return instance;
    }

    public void Start()
    {
        instance = this;
    }


    /// <summary>
    /// 显示任务命令结果提示框
    /// </summary>
    /// <param name="taskResult"></param>
    /// <param name="taskType"></param>
    public void ShowResultTips(short generalId, string taskResult, TaskType taskType)
    {
        TipsPanel.SetActive(true);
        CloseTipsButton.onClick.AddListener(CloseTipsPanel);
        UIExecutivePanel.Instance.title.text = $"{TextLibrary.GetTaskDescription(taskType)}";
        SelectedHead.texture = Resources.Load<Texture2D>($"HeadImage/{generalId}");

        // 根据 TaskType 选择显示的内容
        string resultMessage = string.Empty;

        switch (taskType)
        {
            case TaskType.Reward:
                resultMessage = DoThingsResultInfo[2][5]; // 奖赏武将提升忠诚度
                break;
            case TaskType.Reclaim:
                resultMessage = DoThingsResultInfo[3][0]; // 本城农业水平提高了
                break;
            case TaskType.Mercantile:
                resultMessage = DoThingsResultInfo[3][1]; // 本城商业水平提高了
                break;
            case TaskType.Tame:
                resultMessage = DoThingsResultInfo[3][2]; // 本城抗灾能力提高了
                break;
            case TaskType.Patrol:
                resultMessage = DoThingsResultInfo[3][3]; // 城市秩序更加稳定人口增加了
                break;
            case TaskType.Shop:
                resultMessage = DoThingsResultInfo[4][0]; // 交易成功
                break;
            case TaskType.Smithy:
                resultMessage = DoThingsResultInfo[4][1]; // 成功购买武器
                break;
            case TaskType.School:
                resultMessage = DoThingsResultInfo[4][3]; // 武将智力提升了
                break;
            case TaskType.Hospital:
                resultMessage = DoThingsResultInfo[4][4]; // 武将智力提升了
                break;


            default:
                resultMessage = taskResult; // 默认显示任务
                break;
        }

        if (UIExecutivePanel.increment == 0)  // 0表示没有执行内政任务
        {
            TipsInfo.text = $"{resultMessage}";
            return;
        }
        TipsInfo.text = $"{resultMessage}{UIExecutivePanel.increment}";
    }


    private void CloseTipsPanel()
    {
        TipsPanel.SetActive(false);
        SceneManager.LoadScene("CityPanel");
    }
}*/