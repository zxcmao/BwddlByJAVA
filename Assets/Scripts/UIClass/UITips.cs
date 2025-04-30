using System;
using System.Collections;
using DataClass;
using GameClass;
using TMPro;
using TurnClass;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UIClass
{
    public class UITips : MonoBehaviour
    {
        [SerializeField] Image tipsImage;
        [SerializeField] TextMeshProUGUI tipsText;
        [SerializeField] Button closeButton;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        
        // 自动确认的协程：等待1秒后自动确认
        private IEnumerator AutoConfirmCoroutine(Action onConfirm)
        {
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
            onConfirm?.Invoke();
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
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.gameObject.SetActive(false);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.gameObject.SetActive(false);
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
        /// <param name="isConfirm">确认回调</param>
        /// <returns></returns>
        public void ShowOptionalTips(string text, Action<bool> isConfirm) 
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
                isConfirm?.Invoke(true);
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
                isConfirm?.Invoke(false);
            });

            // 等待显示持续时间
            Debug.Log($"等待玩家选择:{text}");
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
            
            if (tipsText == null)
            {
                Debug.LogError("检查器中没有设置TextMeshProUGUI组件。");
                gameObject.SetActive(false);
                return; 
            }
            tipsText.gameObject.SetActive(true);
            tipsText.text = msg;
            
            if (tipsImage == null)
            {
                Debug.LogError("检查器中没有设置TipsImage!");
                gameObject.SetActive(false);
                return; 
            }
            tipsText.gameObject.SetActive(true);
            DataManager.LoadSpriteToImage($"Assets/Image/Plan/{planID}.jpg", tipsImage, sprite =>
            {
                gameObject.SetActive(true);
            });
            
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
                DataManager.Release(tipsImage.sprite);
                gameObject.SetActive(false);
                onConfirm?.Invoke();
            });
        }
        
    
        public void ShowTurnTipsWithConfirm(string msg, GameState gameState, Action onConfirm)
        {
            Debug.Log("显示回合提示信息：" + msg);

            if (tipsText == null || tipsImage == null || closeButton == null)
            {
                Debug.LogError("TipsPanel缺少必要组件！");
                return;
            }
            
            // 一开始不要激活面板，等待资源加载
            gameObject.SetActive(false);

            if (FindAnyObjectByType<UIGlobe>().cachedEventSprites.TryGetValue(gameState, out var sprite))
            {
                tipsImage.sprite = sprite;
                tipsText.text = msg;
                gameObject.SetActive(true);
                
                closeButton.onClick.RemoveAllListeners();

                // 判断自动确认还是手动确认
                if (true) // 替换为 GameSettings.AutoConfirmEvent 之类的判断
                {
                    StartCoroutine(AutoConfirmCoroutine(onConfirm));
                }
                else
                {
                    closeButton.gameObject.SetActive(true);
                    closeButton.onClick.AddListener(() =>
                    {
                        gameObject.SetActive(false);
                        onConfirm?.Invoke();
                    });
                }
            }
            else
            {
                Debug.LogError($"未找到对应事件图片: {gameState}");
            }
        }
    

        /// <summary>
        /// 显示任务命令结果提示框
        /// </summary>
        /// <param name="taskResult"></param>
        /// <param name="taskType"></param>
        public IEnumerator ShowTaskTips(string taskResult, TaskType taskType)
        {
            Debug.Log("显示任务提示信息：" + taskResult);

            if (tipsText == null || tipsImage == null || closeButton == null)
            {
                Debug.LogError("TipsPanel缺少必要组件！");
                yield break;
            }
            
            gameObject.SetActive(false);
            
            string path = String.Empty;
            switch (taskType)
            {
                case TaskType.Search:
                case TaskType.SearchFood:
                case TaskType.SearchMoney:
                case TaskType.SearchGeneral:
                    path = $"Assets/Image/Event/Search.png";
                    break;
                default:
                    path = $"Assets/Image/Event/{taskType}.png";
                    break;
            }
            
            DataManager.LoadSpriteToImage(path, tipsImage, sprite =>
            {
                tipsImage.sprite = sprite;
                tipsText.text = taskResult;
                gameObject.SetActive(true);
            });
            bool isClosed = false;
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => isClosed = true);
           
            // 判断自动确认还是手动确认
            if (false) // 替换为 GameSettings.AutoConfirmEvent 之类的判断
            {
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
               yield return new WaitUntil(() => isClosed);
            }
            
            yield return null;
            UIExecutivePanel.Instance.GoToNextScene();
            gameObject.SetActive(false);
        }

        
        /// <summary>
        /// 显示带头像的任务命令结果提示框
        /// </summary>
        /// <param name="generalId">显示的将领头像ID</param>
        /// <param name="taskResult">显示的任务结果文本</param>
        public IEnumerator ShowHeadTips(short generalId, string taskResult)
        {
            gameObject.SetActive(true);
            bool isClosed = false;
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => isClosed = true);
            tipsText.text = taskResult;
            tipsImage.rectTransform.sizeDelta = new Vector2(275, 340);
            DataManager.LoadSpriteToImage($"Assets/Image/Head/{generalId}.jpg", tipsImage);
        
            Debug.Log($"等待玩家确认:{taskResult}");
            yield return new WaitUntil(() => isClosed);
            DataManager.Release(tipsImage.sprite);
            isClosed = false;
            tipsImage.rectTransform.sizeDelta = new Vector2(576, 432);
            
            if (GameInfo.Task != TaskType.Attack)
            {
                UIExecutivePanel.Instance.GoToNextScene();
            }
            gameObject.SetActive(false);
        }

    }
}
