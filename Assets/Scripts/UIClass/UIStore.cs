using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIClass
{
    public class UIStore : MonoBehaviour
    {
        [SerializeField] private Toggle buyFood;
        [SerializeField] private Toggle sellFood;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_InputField needFoodNum;
        [SerializeField] private TMP_InputField needGoldNum;
        [SerializeField] private TextMeshProUGUI restoreInfo;
        [SerializeField] private TextMeshProUGUI assetInfo;

        private short _food;//传入粮食数
        private short _gold;//传入黄金数
        public event Action<short, short> GoStore;
        
        public void Store(short food, short gold)
        {
            _food = food;
            _gold = gold;
            assetInfo.text = $"拥有粮食:{food}  黄金:{gold}";
            needFoodNum.text = "0000";
            needGoldNum.text = "0000"; 
            gameObject.SetActive(true);
            // 设置第一组Toggle和InputField的关系
            buyFood.onValueChanged.AddListener(delegate {
                ToggleValueChanged(buyFood.isOn);
            });
            needFoodNum.onValueChanged.AddListener(delegate{
                OnInputFieldSubmit(needFoodNum.ToString());
            });

            // 设置第二组Toggle和InputField的关系
            sellFood.onValueChanged.AddListener(delegate {
                ToggleValueChanged(sellFood.isOn);
            });
            needGoldNum.onValueChanged.AddListener(delegate{
                OnInputFieldSubmit(needGoldNum.ToString());
            });
            
            // 设置按钮的点击事件
            confirmButton.gameObject.SetActive(true);
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        void ToggleValueChanged(bool isOn)
        {
            if (buyFood.isOn)
            {
                needFoodNum.interactable = isOn;
                needGoldNum.interactable = !isOn;
                needFoodNum.onSubmit.AddListener(OnInputFieldSubmit);
                needGoldNum.onSubmit.RemoveAllListeners();
            }
            else if (sellFood.isOn)
            {
                needFoodNum.interactable = !isOn;
                needGoldNum.interactable = isOn;
                needFoodNum.onSubmit.RemoveAllListeners();
                needGoldNum.onSubmit.AddListener(OnInputFieldSubmit);
            }
        }
        
        
        void OnInputFieldSubmit(string value)
        {
            if (int.TryParse(value, out int intValue))
            {
                if (buyFood.isOn)
                {
                    if (intValue < 1)
                    {
                        // 如果输入值超出范围，设置为默认值或清空
                        needFoodNum.text = "0000"; // 
                        restoreInfo.text = "请确认实际数量";
                    }
                    else if (intValue > 10000)
                    {
                        // 如果输入值超出范围，设置为默认值或清空
                        needFoodNum.text = "9999"; 
                        restoreInfo.text = "请确认实际数量";
                    }

                    intValue = int.Parse(needFoodNum.text);
                    int payNum = 3 * intValue / 4;
                    if (payNum > _gold)
                    {
                        // 如果应当支付的值超出范围，设置为最大值
                        needFoodNum.text = (4 * _gold / 3).ToString();
                        needGoldNum.text = _gold.ToString();
                        restoreInfo.text = $"将花费黄金:{payNum}";
                    }
                    else
                    {
                        needGoldNum.text = payNum.ToString();
                        restoreInfo.text = $"将花费黄金:{payNum}";
                    }
                    Debug.Log(needFoodNum.text);
                }
                else if (sellFood.isOn)
                {
                    if (intValue < 1)
                    {
                        // 如果输入值超出范围，设置为默认值或清空
                        needGoldNum.text = "0000"; 
                        restoreInfo.text = "请确认实际数量";
                    }
                    else if (intValue > 10000)
                    {
                        // 如果输入值超出范围，设置为默认值或清空
                        needGoldNum.text = "9999"; 
                        restoreInfo.text = "请确认实际数量";
                    }
                    
                    intValue = int.Parse(needGoldNum.text);
                    int payNum = 4 * intValue / 3;
                    if (payNum > _food)
                    {
                        needGoldNum.text = (4 * _food / 3).ToString();
                        needFoodNum.text = _food.ToString();
                        restoreInfo.text = $"将卖出粮食:{payNum}";
                    }
                    else
                    {
                        needFoodNum.text = payNum.ToString();
                        restoreInfo.text = $"将卖出粮食:{payNum}";
                    }
                    Debug.Log(needGoldNum.text);
                }
            }
        }
        
        private void OnConfirmButtonClicked()
        {
            if (buyFood.isOn)
            {
                var buy = short.Parse(needFoodNum.text);
                if (_food + buy >= 30000)
                {
                    _food = 30000;
                }
                else
                {
                    _food += buy;
                }
                _gold -= (short)(buy * 3 / 4);
            }
            else if (sellFood.isOn)
            {
                var sell = short.Parse(needGoldNum.text);
                if (_gold + sell > 30000)
                {
                    _gold = 30000;
                }
                else
                {
                    _gold += sell;
                }
                _food -= (short)(sell * 4 / 3);
            }
            GoStore?.Invoke(_food, _gold);
            gameObject.SetActive(false);
        }
    }
    
    
}