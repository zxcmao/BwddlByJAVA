using GameClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIClass
{
    public class UISetting : MonoBehaviour
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Slider musicSlider;

        private void Start()
        {
            gameObject.SetActive(true);
            if (PlayerPrefs.GetFloat("BGMVolume") > 0)
            {
                musicToggle.isOn = true;
                musicSlider.value = PlayerPrefs.GetFloat("BGMVolume");
            }
            musicToggle.onValueChanged.AddListener(OnMusicToggleValueChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        }

        private void OnMusicToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                OnMusicSliderValueChanged(0.5f);
                SoundManager.Instance.SetBGMVolume(0.5f);
                SoundManager.Instance.PlayBGM("2");
            }
            else
            {
                PlayerPrefs.SetFloat("BGMVolume", 0);
                SoundManager.Instance.SetBGMVolume(0);
                SoundManager.Instance.StopBGM();
            }
        }

        private void OnMusicSliderValueChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            SoundManager.Instance.SetBGMVolume(value);
        }
    }
}
