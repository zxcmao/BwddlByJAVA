using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace GameClass
{
    public class SceneController : MonoBehaviour
    {
        
        private static SceneController _instance;
        private SceneController(){}
        
        private Scene originalScene;
        private Scene newScene;

        public static SceneController Instance => _instance;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this; // 确保在 Awake 中完成初始化
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject); // 防止多个实例
            }
        }
        
        // 加载新场景并激活
        public void LoadNewScene(string newSceneName)
        {
            // 保存当前活动场景
            originalScene = SceneManager.GetActiveScene();
            
            // 异步加载新场景
            SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);
            
            DisableOriginalSceneComponent();
            
            // 等待场景加载完成后设置为活动场景
            SceneManager.sceneLoaded += OnNewSceneLoaded;
        }

        // 卸载新场景并恢复原场景
        public void UnloadNewScene(string newSceneName)
        {
            if (newScene.isLoaded)
            {
                // 卸载新场景
                SceneManager.UnloadSceneAsync(newSceneName);

                // 恢复原场景为活动场景
                if (originalScene.isLoaded)
                {
                    SceneManager.SetActiveScene(originalScene);
                    EnableOriginalSceneComponent();
                }
            }
        }

        // 新场景加载完成后的回调
        private void OnNewSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 设置新场景为活动场景
            newScene = scene;
            SceneManager.SetActiveScene(newScene);

            // 解除事件绑定（防止重复调用）
            SceneManager.sceneLoaded -= OnNewSceneLoaded;
        }
        
        void DisableOriginalSceneComponent()
        {
            foreach (GameObject rootObj in originalScene.GetRootGameObjects())
            {
                var cam = rootObj.GetComponentInChildren<Camera>();
                if (cam!= null)
                {
                    cam.gameObject.SetActive(false); // 禁用原场景的相机
                }
                var globalLight = rootObj.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
                if (globalLight != null)
                {
                    globalLight.gameObject.SetActive(false); // 禁用全局光
                }

                var canvas = rootObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(false); // 禁用画布
                }
                var eventSystem = rootObj.GetComponentInChildren<EventSystem>();
                if (eventSystem != null)
                {
                    eventSystem.gameObject.SetActive(false); // 禁用输入
                }
            }
        }

        void EnableOriginalSceneComponent()
        {
            foreach (GameObject rootObj in originalScene.GetRootGameObjects())
            {
                var cam = rootObj.GetComponentInChildren<Camera>();
                if (cam!= null)
                {
                    cam.gameObject.SetActive(true); // 禁用原场景的相机
                }
                var globalLight = rootObj.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
                if (globalLight != null)
                {
                    globalLight.gameObject.SetActive(true); // 禁用全局光
                }

                var canvas = rootObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(true); // 禁用画布
                }
                var eventSystem = rootObj.GetComponentInChildren<EventSystem>();
                if (eventSystem != null)
                {
                    eventSystem.gameObject.SetActive(true); // 禁用输入
                }
            }
        }
        
        void DisableGlobalLights()
        {
            foreach (GameObject rootObj in originalScene.GetRootGameObjects())
            {
                var globalLight = rootObj.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
                if (globalLight != null)
                {
                    globalLight.enabled = false; // 禁用全局光
                }
            }
        }
    }
}