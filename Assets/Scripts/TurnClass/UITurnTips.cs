/*
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class UITurnTips : MonoBehaviour
{
    
    [SerializeField] GameObject TurnTips;
    [SerializeField] RawImage turnImage;
    private static UITurnTips instance;
    private bool isClicked = false;

    private UITurnTips() { }

    public static UITurnTips GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<UITurnTips>();
        }

        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    public IEnumerator ShowTurnTips(string turntips)
    {
        if (TurnTips == null)
        {
            Debug.LogError("检查器中没有设置TurnTips !");
            yield break; // 退出协程
        }

        TurnTips.SetActive(true);

        RawImage image = TurnTips.transform.Find("Image").GetComponent<RawImage>();
        if (image == null)
        {
            Debug.LogError("无法在TurnTips中找到“Image”RawImage组件。");
            yield break; // 退出协程
        }

        image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips");

        switch (TurnManager.PlayingState)
        {
            case GameState.AITruce:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Truce");
                break;
            case GameState.AIvsAI:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiAttack");
                break;
            case GameState.AIWinAI:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiWin");
                break;
            case GameState.AILoseAI:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiLose");
                break;
            case GameState.AIvsPlayer:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AivsPlayer");
                break;
            case GameState.AIWinPlayer:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiWinPlayer");
                break;
            case GameState.AILosePlayer:
                //image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiLosePlayer");
                break;
            case GameState.AIInherit:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Inherit");
                break;
            case GameState.AIAlienate:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiAlienate");
                break;
            case GameState.AIBribe:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AiBribe");
                break;
            case GameState.Rebel:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Rebel");
                break;
            case GameState.MoneyTax:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/MoneyTax");
                break;
            case GameState.FoodTax:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/FoodTax");
                break;
            case GameState.AllianceEnd:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/AllianceEnd");
                break;
            case GameState.Drought:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Drought");
                break;
            case GameState.Flood:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Flood");
                break;
            case GameState.LocustPlague:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/LocustPlague");
                break;
            case GameState.Plague:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Plague");
                break;
            case GameState.Turmoil:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Turmoil");
                break;
            case GameState.Plunder:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/Plunder");
                break;
            case GameState.GameOver:
                image.texture = Resources.Load<Texture2D>("UI/TipsImage/TurnTips/GameOver");
                break;
        }

        // 假设TurnTips对象下有一个名为"Tip"的子对象，并且该子对象上有TextMeshProUGUI组件
        TextMeshProUGUI tip = TurnTips.transform.Find("Tip").GetComponent<TextMeshProUGUI>();
        if (tip == null)
        {
            Debug.LogError("无法在TurnTips中找到“Tip”TextMeshProUGUI组件。");
            yield break; // 退出协程
        }

        tip.text = turntips;

        // 等待玩家点击按钮来关闭提示
        Button button = TurnTips.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // 确保按钮事件不会重复添加
            button.onClick.AddListener(OnClickButton);
        }
        else
        {
            Debug.LogWarning("在TurnTips及其子组件中找不到Button组件。单击事件将不会被添加。");
        }

        //// 等待显示持续时间
        //yield return new WaitForSeconds(5f);

        //// 如果在此期间用户点击了按钮，则提前结束
        //if (TurnTips.activeSelf)
        //{
        //    yield return SwitchOff(); // 关闭提示
        //}

        yield return new WaitUntil(() => isClicked);
        if (TurnTips != null)
        {
            TurnTips.SetActive(false);
        }
        isClicked = false;
    }

    private void OnClickButton()
    {
        isClicked = true;
    }
}
*/
