using System;
using System.Collections;
using DataClass;
using TMPro;
using TurnClass;
using UnityEngine;
using UnityEngine.UI;
using War;

namespace UIClass
{
    public class UITips : MonoBehaviour
    {
        [SerializeField] RawImage tipsImage;
        [SerializeField] TextMeshProUGUI tipsText;
        [SerializeField] Button closeButton;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
    
        public bool _isClosed = false;
        
        public event Action<bool> OnOptionSelected;
    
        private void OnClickCloseButton() // 点击关闭按钮
        {
            _isClosed = true;
        }

        /// <summary>
        /// 显示仅可确认的触发事件消息面板
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <returns></returns>
        public IEnumerator ShowNoticeTips(string text) 
        {
            gameObject.SetActive(true);
            _isClosed = false;
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
                confirmButton.onClick.RemoveAllListeners();
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
                cancelButton.onClick.RemoveAllListeners();
            }
        
            if (tipsText == null)
            {
                Debug.LogError("检查器中没有设置TipsText!");
                gameObject.SetActive(false);
                yield break; // 退出协程
            }
            tipsText.text = text;
        
            if (closeButton == null)
            {
                Debug.LogError("检查器中没有设置closeButton!");
                gameObject.SetActive(false);
                yield break; // 退出协程
            }
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => { _isClosed = true; });

            // 等待显示持续时间
            Debug.Log($"等待玩家确认:{text}");
            yield return new WaitUntil(() =>_isClosed);
        
            Debug.Log("玩家已确认");
        
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "WarScene")
            {
                UIWar.Instance.DisplayWarMenu();
            }
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 显示确认后回调的触发事件消息面板
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <param name="onConfirm">确认回调</param>
        /// <returns></returns>
        public void ShowNoticeTipsWithConfirm(string text, Action onConfirm) 
        {
            gameObject.SetActive(true);
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
                confirmButton.onClick.RemoveAllListeners();
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
                cancelButton.onClick.RemoveAllListeners();
            }
        
            if (tipsText == null)
            {
                Debug.LogError("检查器中没有设置TipsText!");
                gameObject.SetActive(false);
                return; // 退出协程
            }
            tipsText.text = text;
        
            if (closeButton == null)
            {
                Debug.LogError("检查器中没有设置closeButton!");
                gameObject.SetActive(false);
                return; // 退出协程
            }
            closeButton.gameObject.SetActive(true);
            Debug.Log($"等待玩家确认:{text}");
            
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
                {
                    gameObject.SetActive(false);
                    onConfirm?.Invoke();
                });
        }
        
        /// <summary>
        /// 显示带选项的提示信息
        /// </summary>
        /// <param name="text">信息文本</param>
        /// <returns></returns>
        public void ShowOptionalTips(string text) 
        {
            gameObject.SetActive(true);

            if (tipsText == null)
            {
                Debug.LogError("检查器中没有设置TipsText!");
                gameObject.SetActive(false);
            }
            tipsText.text = text;
        
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
                closeButton.onClick.RemoveAllListeners();
            }
        
            if (confirmButton == null)
            {
                Debug.LogError("检查器中没有设置confirmButton!");
                gameObject.SetActive(false);
            }
            confirmButton.gameObject.SetActive(true);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(delegate
            {
                gameObject.SetActive(false);
                OnOptionSelected?.Invoke(true);
            });

            if (cancelButton == null)
            {
                Debug.LogError("检查器中没有设置cancelButton!");
                gameObject.SetActive(false);
            }
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(delegate
            {
                gameObject.SetActive(false);
                OnOptionSelected?.Invoke(false);
            });

            // 等待显示持续时间
            Debug.Log($"等待玩家选择:{text}");
        }
    
    
        /// <summary>
        /// 显示计策结果
        /// </summary>
        /// <param name="planID">计策ID</param>
        /// <param name="text">计谋结果文本</param>
        /// <returns></returns>
        public IEnumerator ShowPlanResult(byte planID, string text)
        {
            gameObject.SetActive(true);
            if (tipsImage == null)
            {
                Debug.LogError("检查器中没有设置TipsImage!");
                gameObject.SetActive(false);
                yield break; // 退出协程
            }
            tipsImage.texture = Resources.Load<Texture2D>($"War/PlanImage/{planID}");

            /*if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
            confirmButton.onClick.RemoveAllListeners();
        }

        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(false);
            cancelButton.onClick.RemoveAllListeners();
        }*/
        
            if (closeButton == null)
            {
                Debug.LogError("检查器中没有设置confirmButton!");
                gameObject.SetActive(false);
                yield break; // 退出协程
            }
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.AddListener(OnClickCloseButton);
        
            if (tipsText == null)
            {
                Debug.LogError("检查器中没有设置TextMeshProUGUI组件。");
                gameObject.SetActive(false);
                yield break; // 退出协程
            }
            tipsText.text = text;

            //// 等待显示持续时间
            yield return new WaitUntil(() => _isClosed);
            _isClosed = false;
            gameObject.SetActive(false);
        }
    
        /// <summary>
        /// 显示计策结果回调
        /// </summary>
        /// <param name="planID">计策ID</param>
        /// <param name="msg">计谋结果文本</param>
        /// <param name="onConfirm">确认回调</param>
        /// <returns></returns>
        public void ShowPlanResultWithConfirm(byte planID, string msg, Action onConfirm)
        {
            gameObject.SetActive(true);
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
                confirmButton.onClick.RemoveAllListeners();
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
                cancelButton.onClick.RemoveAllListeners();
            }
            
            if (tipsImage == null)
            {
                Debug.LogError("检查器中没有设置TipsImage!");
                gameObject.SetActive(false);
                return; 
            }
            tipsImage.texture = Resources.Load<Texture2D>($"War/PlanImage/{planID}");
           
            if (tipsText == null)
            {
                Debug.LogError("检查器中没有设置TextMeshProUGUI组件。");
                gameObject.SetActive(false);
                return; 
            }
            tipsText.text = msg;
            
            if (closeButton == null)
            {
                Debug.LogError("检查器中没有设置confirmButton!");
                gameObject.SetActive(false);
                return; // 退出协程
            }
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                onConfirm?.Invoke();
            });
        }
    
        public IEnumerator ShowTurnTips(string text, GameState gameState)
        {
            gameObject.SetActive(true);
        
            if (tipsImage == null)
            {
                Debug.LogError("无法在找到“Image”RawImage组件!");
                yield break; // 退出协程
            }

            switch (gameState)
            {
                case GameState.AITruce:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Truce");
                    break;
                case GameState.AIvsAI:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AIAttack");
                    break;
                case GameState.AIWinAI:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AIWin");
                    break;
                case GameState.AILoseAI:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AILose");
                    break;
                case GameState.AIvsPlayer:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AIAttack");
                    break;
                case GameState.AIWinPlayer:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AiWinPlayer");
                    break;
                case GameState.AIOccupy:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Occupy");
                    break;
                case GameState.PlayerInherit:
                case GameState.AIInherit:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Inherit");
                    break;
                case GameState.AIAlienate:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Alienate");
                    break;
                case GameState.AIBribe:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Bribe");
                    break;
                case GameState.Rebel:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Rebel");
                    break;
                case GameState.MoneyTax:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Tax");
                    break;
                case GameState.FoodTax:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Harvest");
                    break;
                case GameState.AllianceEnd:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AllianceEnd");
                    break;
                case GameState.Famine:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Famine");
                    break;
                case GameState.Drought:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Drought");
                    break;
                case GameState.Flood:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Flood");
                    break;
                case GameState.LocustPlague:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/LocustPlague");
                    break;
                case GameState.Plague:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Plague");
                    break;
                case GameState.Turmoil:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Turmoil");
                    break;
                case GameState.Plunder:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Plunder");
                    break;
                case GameState.GameOver:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/GameOver");
                    break;
            }
        
            if (tipsText == null)
            {
                Debug.LogError("无法在TipsPanel中找到“Tip”TextMeshProUGUI组件。");
                yield break; // 退出协程
            }

            tipsText.text = text;

            // 等待玩家点击按钮来关闭提示
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners(); // 确保按钮事件不会重复添加
                closeButton.onClick.AddListener(OnClickCloseButton);
            }
            else
            {
                Debug.LogWarning("在Tips及其子组件中找不到Button组件。单击事件将不会被添加。");
            }

            // 等待显示持续时间
            Debug.Log($"等待玩家确认:{text}");
            _isClosed = false;
            yield return new WaitUntil(() => _isClosed);
            gameObject.SetActive(false);
        }
    
        public void ShowTurnTipsWithConfirm(string msg,GameState gameState, Action onConfirm)
        {
            // 激活面板并显示消息
            gameObject.SetActive(true);
            if (tipsText == null)
            {
                Debug.LogError("无法在TipsPanel中找到“Tip”TextMeshProUGUI组件。");
                return ; // 退出协程
            }
            tipsText.text = msg;

            if (tipsImage == null)
            {
                Debug.LogError("无法在找到“Image”RawImage组件!");
                return; // 退出协程
            }

            switch (gameState)
            {
                case GameState.AITruce:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Truce");
                    break;
                case GameState.AIvsAI:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AIAttack");
                    break;
                case GameState.AIWinAI:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AIWin");
                    break;
                case GameState.AILoseAI:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AILose");
                    break;
                case GameState.AIvsPlayer:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AIAttack");
                    break;
                case GameState.AIWinPlayer:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AiWinPlayer");
                    break;
                case GameState.AIOccupy:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Occupy");
                    break;
                case GameState.PlayerInherit:
                case GameState.AIInherit:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Inherit");
                    break;
                case GameState.AIAlienate:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Alienate");
                    break;
                case GameState.AIBribe:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Bribe");
                    break;
                case GameState.Rebel:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Rebel");
                    break;
                case GameState.MoneyTax:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Tax");
                    break;
                case GameState.FoodTax:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Harvest");
                    break;
                case GameState.AllianceEnd:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/AllianceEnd");
                    break;
                case GameState.Famine:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Famine");
                    break;
                case GameState.Drought:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Drought");
                    break;
                case GameState.Flood:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Flood");
                    break;
                case GameState.LocustPlague:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/LocustPlague");
                    break;
                case GameState.Plague:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Plague");
                    break;
                case GameState.Turmoil:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Turmoil");
                    break;
                case GameState.Plunder:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/Plunder");
                    break;
                case GameState.AIFail:
                case GameState.GameOver:
                    tipsImage.texture = Resources.Load<Texture2D>("Event/GameOver");
                    break;
            }
        
            
            // 清除之前的所有事件
            closeButton.onClick.RemoveAllListeners();

            // 当玩家点击确认时执行回调
            closeButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false); // 隐藏面板
                onConfirm?.Invoke();    // 执行回调
            });
        }
    

        /// <summary>
        /// 显示任务命令结果提示框
        /// </summary>
        /// <param name="generalId"></param>
        /// <param name="taskResult"></param>
        /// <param name="taskType"></param>
        public IEnumerator ShowTaskTips(string taskResult, TaskType taskType)
        {
            gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickCloseButton);
            tipsImage.texture = Resources.Load<Texture2D>($"Event/{taskType}");

            // 根据 TaskType 选择显示的内容
            string resultMessage = string.Empty;

            switch (taskType)
            {
                case TaskType.Search:
                case TaskType.SearchFood:
                case TaskType.SearchMoney:
                case TaskType.SearchGeneral:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Search");
                    break;
                case TaskType.Reward:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Reward");
                    break;
                case TaskType.Reclaim:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Reclaim");
                    resultMessage = TextLibrary.DoThingsResultInfo[3][0]; // 本城农业水平提高了
                    break;
                case TaskType.Mercantile:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Mercantile");
                    resultMessage = TextLibrary.DoThingsResultInfo[3][1]; // 本城商业水平提高了
                    break;
                case TaskType.Tame:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Tame");
                    resultMessage = TextLibrary.DoThingsResultInfo[3][2]; // 本城抗灾能力提高了
                    break;
                case TaskType.Patrol:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Patrol");
                    resultMessage = TextLibrary.DoThingsResultInfo[3][3]; // 城市秩序更加稳定人口增加了
                    break;
                case TaskType.Shop:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Shop");
                    resultMessage = TextLibrary.DoThingsResultInfo[4][0]; // 交易成功
                    break;
                case TaskType.Smithy:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Smithy");
                    resultMessage = TextLibrary.DoThingsResultInfo[4][1]; // 成功购买武器
                    break;
                case TaskType.School:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/School");
                    resultMessage = TextLibrary.DoThingsResultInfo[4][3]; // 武将智力提升了
                    break;
                case TaskType.Hospital:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Hospital");
                    resultMessage = TextLibrary.DoThingsResultInfo[4][4]; // 武将生命提升了
                    break;


                default:
                    tipsImage.texture = Resources.Load<Texture2D>($"Event/Retire");
                    resultMessage = taskResult; // 默认显示任务
                    break;
            }
        
            tipsText.text = taskResult;
        
            Debug.Log($"等待玩家确认:{taskResult}");
            yield return new WaitUntil(() => _isClosed);
            _isClosed = false;
            Debug.Log("关闭任务提示");
            UIExecutivePanel.Instance.GoToNextScene();
            gameObject.SetActive(false);
        }

        
        /// <summary>
        /// 显示带头像的任务命令结果提示框
        /// </summary>
        /// <param name="generalId">显示的将领头像ID</param>
        /// <param name="taskResult">显示的任务结果文本</param>
        public IEnumerator ShowHeadTips(short generalId,string taskResult)
        {
            gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickCloseButton);
            tipsText.text = taskResult;
            tipsImage.rectTransform.sizeDelta = new Vector2(275, 340);
            tipsImage.texture = Resources.Load<Texture2D>($"HeadImage/{generalId}");
        
            Debug.Log($"等待玩家确认:{taskResult}");
            yield return new WaitUntil(() => _isClosed);
            _isClosed = false;
            tipsImage.rectTransform.sizeDelta = new Vector2(576, 432);
            Debug.Log("关闭任务提示");
            if (GameInfo.Task != TaskType.Attack)
            {
                UIExecutivePanel.Instance.GoToNextScene();
            }
            gameObject.SetActive(false);
        }

    }
}
