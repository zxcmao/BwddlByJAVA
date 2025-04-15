using System.Collections.Generic;
using UnityEngine;

namespace GameClass
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; } // 单例模式

        [Header("音量设置")]
        [Range(0f, 1f)] public float bgmVolume = 1f; // 背景音乐音量
        [Range(0f, 1f)] public float sfxVolume = 1f; // 音效音量

        [Header("音频源")]
        [SerializeField] private AudioSource bgmSource; // 背景音乐音源
        [SerializeField] private AudioSource sfxSource; // 全局音效音源
        [SerializeField] private AudioSource uiSource; // UI音效音源

        private Dictionary<string, AudioClip> audioClips; // 音频资源缓存

        private void Awake()
        {
            // 实现单例模式
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // 声音管理器在场景切换时不被销毁

            audioClips = new Dictionary<string, AudioClip>(); // 初始化缓存
        }

        #region 背景音乐管理
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clipName">音频剪辑名称</param>
        public void PlayBGM(string clipName)
        {
            AudioClip clip = GetAudioClip(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"BGM 音频剪辑 {clipName} 未找到！");
                return;
            }

            // 检查是否已经是当前播放的音频，并且正在播放
            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {   // 如果已经是当前音频且正在播放，则不执行任何操作
                return;// 防止重复播放同一音频
            } 
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            bgmSource.Stop();
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseBGM()
        {
            bgmSource.Pause();
        }

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void ResumeBGM()
        {
            bgmSource.UnPause();
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="volume">音量（0~1）</param>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("bgmVolume", bgmVolume);
            bgmSource.volume = bgmVolume;
        }
        #endregion

        #region 音效管理
        /// <summary>
        /// 播放一次性音效
        /// </summary>
        /// <param name="clipName">音频剪辑名称</param>
        public void PlaySFX(string clipName)
        {
            AudioClip clip = GetAudioClip(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"SFX 音频剪辑 {clipName} 未找到！");
                return;
            }

            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// 播放绑定到对象的音效
        /// </summary>
        /// <param name="clipName">音频剪辑名称</param>
        /// <param name="targetObject">目标对象</param>
        public void PlaySFXAtObject(string clipName, GameObject targetObject)
        {
            AudioClip clip = GetAudioClip(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"SFX 音频剪辑 {clipName} 未找到！");
                return;
            }

            AudioSource source = targetObject.GetComponent<AudioSource>();
            if (source == null)
            {
                source = targetObject.AddComponent<AudioSource>();
                source.spatialBlend = 1f; // 3D 音效
            }

            source.clip = clip;
            source.volume = sfxVolume;
            source.Play();
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="volume">音量（0~1）</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
        #endregion

        #region UI音效管理
        /// <summary>
        /// 播放UI音效
        /// </summary>
        /// <param name="clipName">音频剪辑名称</param>
        public void PlayUI(string clipName)
        {
            AudioClip clip = GetAudioClip(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"UI 音频剪辑 {clipName} 未找到！");
                return;
            }

            uiSource.PlayOneShot(clip, sfxVolume);
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 获取音频剪辑
        /// </summary>
        /// <param name="clipName">音频名称</param>
        /// <returns>音频剪辑</returns>
        private AudioClip GetAudioClip(string clipName)
        {
            if (audioClips.TryGetValue(clipName, out AudioClip clip))
            {
                return clip;
            }

            // 如果缓存中没有，尝试从资源加载
            clip = Resources.Load<AudioClip>($"Audio/Music/{clipName}");
            if (clip != null)
            {
                audioClips[clipName] = clip;
            }

            return clip;
        }
        #endregion
    }
}
