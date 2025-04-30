using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameClass
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("音量设置")]
        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        [Header("音频源")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        // 音频资源缓存
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AsyncOperationHandle<AudioClip>> audioHandles = new Dictionary<string, AsyncOperationHandle<AudioClip>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region 背景音乐管理
        public void PlayBGM(string clipAddress)
        {
            if (audioClips.TryGetValue(clipAddress, out AudioClip cachedClip))
            {
                if (bgmSource.clip == cachedClip && bgmSource.isPlaying)
                    return; // 已经在播放了

                PlayBGMInternal(cachedClip);
                return;
            }

            // 加载并播放
            var handle = Addressables.LoadAssetAsync<AudioClip>(clipAddress);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    audioClips[clipAddress] = op.Result;
                    audioHandles[clipAddress] = handle;
                    PlayBGMInternal(op.Result);
                }
                else
                {
                    Debug.LogError($"加载BGM失败: {clipAddress}");
                }
            };
        }

        private void PlayBGMInternal(AudioClip clip)
        {
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM() => bgmSource.Stop();
        public void PauseBGM() => bgmSource.Pause();
        public void ResumeBGM() => bgmSource.UnPause();

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("bgmVolume", bgmVolume);
            bgmSource.volume = bgmVolume;
        }
        #endregion

        #region 音效管理
        public void PlaySFX(string clipAddress)
        {
            LoadAndPlayClip(clipAddress, clip =>
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            });
        }

        public void PlaySFXAtObject(string clipAddress, GameObject targetObject)
        {
            LoadAndPlayClip(clipAddress, clip =>
            {
                AudioSource source = targetObject.GetComponent<AudioSource>();
                if (source == null)
                {
                    source = targetObject.AddComponent<AudioSource>();
                    source.spatialBlend = 1f; // 3D音效
                }
                source.clip = clip;
                source.volume = sfxVolume;
                source.Play();
            });
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
        #endregion

        #region UI音效管理
        public void PlayUI(string clipAddress)
        {
            LoadAndPlayClip(clipAddress, clip =>
            {
                uiSource.PlayOneShot(clip, sfxVolume);
            });
        }
        #endregion

        #region 工具方法
        private void LoadAndPlayClip(string clipAddress, Action<AudioClip> onLoaded)
        {
            if (audioClips.TryGetValue(clipAddress, out AudioClip cachedClip))
            {
                onLoaded?.Invoke(cachedClip);
                return;
            }

            var handle = Addressables.LoadAssetAsync<AudioClip>(clipAddress);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    audioClips[clipAddress] = op.Result;
                    audioHandles[clipAddress] = handle;
                    onLoaded?.Invoke(op.Result);
                }
                else
                {
                    Debug.LogError($"加载音频失败: {clipAddress}");
                }
            };
        }

        /// <summary>
        /// 释放所有已加载的音频资源（切场景时调用）
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var handle in audioHandles.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            audioClips.Clear();
            audioHandles.Clear();
            Debug.Log("释放了所有音频资源！");
        }
        #endregion
    }

}
