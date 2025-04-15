using System;
using System.Linq;
using DataClass;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIClass
{
    public class UIRecord : MonoBehaviour
    {
        [SerializeField] Toggle save1;
        [SerializeField] Toggle save2;
        [SerializeField] Toggle save3;
        [SerializeField] Toggle save4;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        
        private byte _recordIndex;
        public event Action<byte> RecordIndex; 
        
        // 存档
        public void Save()
        {
            gameObject.SetActive(true);
            DataManagement.GetRecordInfo();
            save1.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[0];
            save2.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[1];
            save3.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[2];
            save4.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[3];
            save1.onValueChanged.AddListener(delegate {
                _recordIndex = 1;
                confirmButton.gameObject.SetActive(true);
                confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            });
            save2.onValueChanged.AddListener(delegate {
                _recordIndex = 2;
                confirmButton.gameObject.SetActive(true);
                confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            });
            save3.onValueChanged.AddListener(delegate {
                _recordIndex = 3;
                confirmButton.gameObject.SetActive(true);
                confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            });
            save4.onValueChanged.AddListener(delegate {
                _recordIndex = 4;
                confirmButton.gameObject.SetActive(true);
                confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
            });
            cancelButton.gameObject.SetActive(true);
            cancelButton.GetComponent<Button>().onClick.AddListener(OnCancelButtonClicked);
        }
        
        //载入存档方法
        public void Load()
        {
            gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            cancelButton.GetComponent<Button>().onClick.AddListener(OnCancelButtonClicked);
            DataManagement.GetRecordInfo();
            save1.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[0];
            save2.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[1];
            save3.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[2];
            save4.GetComponent<TextMeshProUGUI>().text = GameInfo.recordInfo[3];
            
            if(save1.GetComponent<TextMeshProUGUI>().text != "尚无存档")
            { 
                save1.onValueChanged.RemoveAllListeners();
                save1.onValueChanged.AddListener(delegate
                {
                    _recordIndex = 1;
                    Debug.Log("1");
                    confirmButton.gameObject.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                });
            }
            else
            {
                save1.interactable = false;
            }
            
            if (save2.GetComponent<TextMeshProUGUI>().text != "尚无存档")
            {
                save2.onValueChanged.RemoveAllListeners();
                save2.onValueChanged.AddListener(delegate
                {
                    _recordIndex = 2;
                    Debug.Log("2");
                    confirmButton.gameObject.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                });
            }
            else
            {
                save2.interactable = false;
            }
            
            if (save3.GetComponent<TextMeshProUGUI>().text != "尚无存档")
            {
                save3.onValueChanged.RemoveAllListeners();
                save3.onValueChanged.AddListener(delegate
                {
                    _recordIndex = 3;
                    confirmButton.gameObject.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                });
            }
            else
            {
                save3.interactable = false;
            }
            
            if (save4.GetComponent<TextMeshProUGUI>().text != "尚无存档")
            {
                save4.onValueChanged.RemoveAllListeners();
                save4.onValueChanged.AddListener(delegate
                {
                    _recordIndex = 4;
                    confirmButton.gameObject.SetActive(true);
                    confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
                });
            }
            else
            {
                save4.interactable = false;
            }
        }

        private void OnConfirmButtonClicked()
        {
            gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            RecordIndex?.Invoke(_recordIndex);
        }
        
        private void OnCancelButtonClicked()
        {
            gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            RecordIndex?.Invoke(0);
        }
    }
}
