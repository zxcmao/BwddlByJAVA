using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace War
{
    public class CameraController : MonoBehaviour
    {
        /*[SerializeField] private float moveSpeed = 200f;
        [SerializeField] private float followSpeed = 50f;
        [SerializeField] private Vector2 minBounds = new Vector2(340, 648);
        [SerializeField] private Vector2 maxBounds = new Vector2(1964, 648);
        
        private Vector2 lastTouchPosition; // 记录上一次触摸位置
        private bool isTouching = false; // 是否正在触摸
        private CameraInput _playerInput;
        private InputAction _mouseDeltaAction;
        private InputAction _touchDeltaAction;

        private Vector3 _moveDirection = Vector3.zero;
        
        void Awake()
        {
            _playerInput = new CameraInput();
            _mouseDeltaAction = _playerInput.Camera.MouseDelta;
            _touchDeltaAction = _playerInput.Camera.TouchDelta;

            // 订阅输入事件
            _mouseDeltaAction.performed += OnMouseDelta;
            _mouseDeltaAction.canceled += StopMovement;

            _touchDeltaAction.performed += OnTouchDelta;
            _touchDeltaAction.canceled += StopMovement;
        }

        void OnEnable()
        {
            _mouseDeltaAction.Enable();
            _touchDeltaAction.Enable();
            WarManager.Instance.cameraPosition.z = -10;
            transform.position = WarManager.Instance.cameraPosition;
            ClampCameraPosition();
        }

        void OnDisable()
        {
            _mouseDeltaAction.Disable();
            _touchDeltaAction.Disable();
            WarManager.Instance.cameraPosition = transform.position;
            ClampCameraPosition();
        }

        // 鼠标移动时调用
        private void OnMouseDelta(InputAction.CallbackContext context)
        {
            // 如果计谋面板处于活动状态，禁止摄像头移动
            if (UIWar.Instance.uiPlanPanel.gameObject.activeInHierarchy)
                return;
            if (Mouse.current.leftButton.isPressed && WarManager.Instance.warState == WarState.PlayerTurn)
            {
                Vector2 delta = context.ReadValue<Vector2>();
                _moveDirection = new Vector3(-delta.x, -delta.y, 0) * moveSpeed * Time.deltaTime;
                MoveCamera();
            }
        }

        // 触控移动时调用
        private void OnTouchDelta(InputAction.CallbackContext context)
        {
            // 如果计谋面板处于活动状态，禁止摄像头移动
            if (UIWar.Instance.uiPlanPanel.gameObject.activeInHierarchy)
                return;

            if (Touchscreen.current == null || WarManager.Instance.warState != WarState.PlayerTurn)
                return;

            // 获取当前触摸点
            var touch = Touchscreen.current.touches[0];

            // 处理触摸按下
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                isTouching = true;
                lastTouchPosition = touch.position.ReadValue(); // 记录初始位置
            }
            // 处理触摸滑动（仅在 Moved 状态移动摄像机）
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && isTouching)
            {
                Vector2 touchPosition = touch.position.ReadValue(); // 读取当前手指位置
                Vector2 delta = touchPosition - lastTouchPosition; // 计算 delta
                lastTouchPosition = touchPosition; // 更新 lastTouchPosition

                // **增加触控灵敏度**
                float touchSensitivity = 2.0f;
                _moveDirection = new Vector3(-delta.x, -delta.y, 0) * moveSpeed * Time.deltaTime * touchSensitivity;
                MoveCamera();
            }
            // 处理触摸结束
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || 
                     touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                isTouching = false; // 触摸结束，停止移动
            }
        }

        // 停止移动
        private void StopMovement(InputAction.CallbackContext context)
        {
            _moveDirection = Vector3.zero;
        }

        // 执行移动
        private void MoveCamera()
        {
            transform.position += _moveDirection;
            ClampCameraPosition();
        }

        private void ClampCameraPosition()
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
            transform.position = clampedPosition;
        }
        
        
        public void StartAIUnitFollow()
        {
            StartCoroutine(FollowAIUnit());
        }

        public void StopAIUnitFollow()
        {
            StopCoroutine(FollowAIUnit());
            Debug.Log("停止摄像机跟随");
        }
        
        private IEnumerator FollowAIUnit()
        {
            while (WarManager.Instance.warState != WarState.PlayerTurn)
            {
                if (WarManager.Instance.aiUnitObj != null)
                {
                    Vector3 targetPosition = WarManager.Instance.aiUnitObj.transform.position;
                    targetPosition.z = transform.position.z;
                    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                    ClampCameraPosition();
                }
                yield return new WaitForSeconds(0.05f); // 每 0.1 秒更新一次
            }
        }*/
        [SerializeField] private float moveSpeed = 200f;
        [SerializeField] private float followSpeed = 50f;
        
        // 瓦片地图信息
        [SerializeField] private int mapRows = 19;
        [SerializeField] private int mapColumns = 32;
        [SerializeField] private float tileSize = 72f; // 单个瓦片的大小
        
        // 摄像机边界 - 这些将被自动计算
        private float minBoundX;
        private float maxBoundX;
        private float minBoundY;
        private float maxBoundY;
        
        // 默认固定Y位置（如果地图是水平滚动的）
        [SerializeField] private float fixedYPosition = 648f;
        
        private Vector2 lastTouchPosition;
        private bool isTouching = false;
        private CameraInput _playerInput;
        private InputAction _mouseDeltaAction;
        private InputAction _touchDeltaAction;
        private Vector3 _moveDirection = Vector3.zero;
        private Camera _camera;
        
        void Awake()
        {
            _camera = GetComponent<Camera>();
            _playerInput = new CameraInput();
            _mouseDeltaAction = _playerInput.Camera.MouseDelta;
            _touchDeltaAction = _playerInput.Camera.TouchDelta;

            // 订阅输入事件
            _mouseDeltaAction.performed += OnMouseDelta;
            _mouseDeltaAction.canceled += StopMovement;

            _touchDeltaAction.performed += OnTouchDelta;
            _touchDeltaAction.canceled += StopMovement;
        }

        void OnEnable()
        {
            _mouseDeltaAction.Enable();
            _touchDeltaAction.Enable();
            CalculateCameraBounds();
            
            if (WarManager.Instance != null)
            {
                WarManager.Instance.cameraPosition.z = -10;
                transform.position = WarManager.Instance.cameraPosition;
            }
            ClampCameraPosition();
        }

        void OnDisable()
        {
            _mouseDeltaAction.Disable();
            _touchDeltaAction.Disable();
            if (WarManager.Instance != null)
            {
                WarManager.Instance.cameraPosition = transform.position;
            }
        }
        
        /*void Start()
        {
            CalculateCameraBounds();
            ClampCameraPosition();
        }*/
        
        /*// 当屏幕尺寸或方向改变时重新计算
        void OnRectTransformDimensionsChange()
        {
            CalculateCameraBounds();
            ClampCameraPosition();
        }*/
        
        // 根据瓦片地图大小和当前摄像机视野计算边界
        private void CalculateCameraBounds()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();
                
            // 计算地图的实际世界尺寸
            float mapWidth = mapColumns * tileSize;
            float mapHeight = mapRows * tileSize;
            
            // 计算地图的中心点
            float mapCenterX = mapWidth / 2;
            float mapCenterY = mapHeight / 2;
            
            // 获取摄像机的可视宽度和高度（以世界坐标单位）
            float cameraHeight = 2 * _camera.orthographicSize;
            float cameraWidth = cameraHeight * _camera.aspect;
            
            // 计算最小和最大边界，确保摄像机视野不会超出地图范围
            
            // X边界 - 根据水平方向计算
            float halfCameraWidth = cameraWidth / 2;
            
            // 如果摄像机视野宽度小于地图宽度，则需要设置边界
            if (cameraWidth < mapWidth)
            {
                // 摄像机可以在地图范围内左右移动
                minBoundX = halfCameraWidth;
                maxBoundX = mapWidth - halfCameraWidth;
            }
            else
            {
                // 摄像机视野比地图宽，只需要让地图居中
                minBoundX = mapCenterX;
                maxBoundX = mapCenterX;
            }
            
            // Y边界 - 对于水平滚动地图，通常保持固定Y位置
            minBoundY = fixedYPosition;
            maxBoundY = fixedYPosition;
            
            // 如果需要垂直滚动，可以像X那样计算Y边界
            /* 
            float halfCameraHeight = cameraHeight / 2;
            if (cameraHeight < mapHeight) {
                minBoundY = halfCameraHeight;
                maxBoundY = mapHeight - halfCameraHeight;
            } else {
                minBoundY = mapCenterY;
                maxBoundY = mapCenterY;
            }
            */
            
            Debug.Log($"计算的摄像机边界：X ({minBoundX} - {maxBoundX}), Y ({minBoundY} - {maxBoundY})");
            Debug.Log($"摄像机信息：正交大小 = {_camera.orthographicSize}, 宽高比 = {_camera.aspect}, 视野宽度 = {cameraWidth}");
        }

        // 鼠标移动时调用
        private void OnMouseDelta(InputAction.CallbackContext context)
        {
            // 如果计谋面板处于活动状态，禁止摄像头移动
            if (UIWar.Instance != null && UIWar.Instance.uiPlanPanel.gameObject.activeInHierarchy)
                return;
            if (Mouse.current.leftButton.isPressed && WarManager.Instance != null && WarManager.Instance.warState == WarState.PlayerTurn)
            {
                Vector2 delta = context.ReadValue<Vector2>();
                _moveDirection = new Vector3(-delta.x, -delta.y, 0) * moveSpeed * Time.deltaTime;
                MoveCamera();
            }
        }

        // 触控移动时调用
        private void OnTouchDelta(InputAction.CallbackContext context)
        {
            // 如果计谋面板处于活动状态，禁止摄像头移动
            if (UIWar.Instance != null && UIWar.Instance.uiPlanPanel.gameObject.activeInHierarchy)
                return;

            if (Touchscreen.current == null || WarManager.Instance == null || WarManager.Instance.warState != WarState.PlayerTurn)
                return;

            // 获取当前触摸点
            var touch = Touchscreen.current.touches[0];

            // 处理触摸按下
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                isTouching = true;
                lastTouchPosition = touch.position.ReadValue(); // 记录初始位置
            }
            // 处理触摸滑动（仅在 Moved 状态移动摄像机）
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && isTouching)
            {
                Vector2 touchPosition = touch.position.ReadValue(); // 读取当前手指位置
                Vector2 delta = touchPosition - lastTouchPosition; // 计算 delta
                lastTouchPosition = touchPosition; // 更新 lastTouchPosition

                // **增加触控灵敏度**
                float touchSensitivity = 2.0f;
                _moveDirection = new Vector3(-delta.x, -delta.y, 0) * moveSpeed * Time.deltaTime * touchSensitivity;
                MoveCamera();
            }
            // 处理触摸结束
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || 
                     touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                isTouching = false; // 触摸结束，停止移动
            }
        }

        // 停止移动
        private void StopMovement(InputAction.CallbackContext context)
        {
            _moveDirection = Vector3.zero;
        }

        // 执行移动
        private void MoveCamera()
        {
            transform.position += _moveDirection;
            ClampCameraPosition();
        }

        private void ClampCameraPosition()
        {
            Vector3 clampedPosition = transform.position;
            
            // 限制X轴移动
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBoundX, maxBoundX);
            
            // 对于水平滚动地图，Y保持固定
            clampedPosition.y = fixedYPosition;
            
            // 如果需要垂直滚动，则使用计算出的Y边界
            // clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBoundY, maxBoundY);
            
            transform.position = clampedPosition;
        }
        
        public void StartAIUnitFollow()
        {
            StartCoroutine(FollowAIUnit());
        }

        public void StopAIUnitFollow()
        {
            StopCoroutine(FollowAIUnit());
            Debug.Log("停止摄像机跟随");
        }
        
        private IEnumerator FollowAIUnit()
        {
            while (WarManager.Instance != null && WarManager.Instance.warState != WarState.PlayerTurn)
            {
                if (WarManager.Instance.aiUnitObj != null)
                {
                    Vector3 targetPosition = WarManager.Instance.aiUnitObj.transform.position;
                    targetPosition.z = transform.position.z;
                    
                    // 确保目标位置在有效范围内
                    targetPosition.x = Mathf.Clamp(targetPosition.x, minBoundX, maxBoundX);
                    targetPosition.y = fixedYPosition;
                    
                    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                }
                yield return new WaitForSeconds(0.01f); // 每 0.05 秒更新一次
            }
        }
        
        /*// 公共方法，当瓦片地图或摄像机设置改变时手动重新计算边界
        public void RecalculateBounds()
        {
            CalculateCameraBounds();
            ClampCameraPosition();
        }*/

        void Update()
        {
            if (WarManager.Instance.warState == WarState.AITurn)
            {
                // AI 回合时摄像机跟随单位
                if (WarManager.Instance.aiUnitObj != null)
                {
                    Vector3 targetPosition = WarManager.Instance.aiUnitObj.transform.position;
                    targetPosition.z = transform.position.z;
                    
                    // 确保目标位置在有效范围内
                    targetPosition.x = Mathf.Clamp(targetPosition.x, minBoundX, maxBoundX);
                    targetPosition.y = fixedYPosition;
                    
                    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                }
            }
        }
    }
}
