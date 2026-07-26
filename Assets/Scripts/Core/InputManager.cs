using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Raccoin.Core
{
    /// <summary>
    /// 输入管理器 - 复刻原版 InputManager
    /// 统一管理所有输入：触摸、手柄、键盘
    /// </summary>
    public class InputManager : MonoSingleton<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] private float _tapThreshold = 0.2f;
        [SerializeField] private float _swipeThreshold = 50f;
        [SerializeField] private float _pinchZoomSpeed = 0.01f;
        [SerializeField] private float _dragSensitivity = 0.5f;

        public UserInput CurrentInput { get; private set; } = new UserInput();
        public bool IsInputEnabled { get; set; } = true;

        // 输入事件
        public event System.Action<Vector2> OnTap;
        public event System.Action<Vector2, Vector2> OnSwipe; // start, end
        public event System.Action<Vector2> OnDrag;
        public event System.Action<float> OnPinchZoom;
        public event System.Action OnPausePressed;
        public event System.Action OnThrowCoin;

        private Vector2 _touchStartPos;
        private float _touchStartTime;
        private bool _isTouching;
        private Vector2 _lastTouchPos;

        protected override void Awake()
        {
            base.Awake();
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            if (!IsInputEnabled) return;
            ProcessTouchInput();
            ProcessGamepadInput();
        }

        private void ProcessTouchInput()
        {
            var touches = Touch.activeTouches;

            if (touches.Count == 0)
            {
                if (_isTouching)
                {
                    // 触摸结束
                    float duration = Time.time - _touchStartTime;
                    Vector2 delta = _lastTouchPos - _touchStartPos;

                    if (duration < _tapThreshold && delta.magnitude < _swipeThreshold)
                    {
                        // 点击
                        OnTap?.Invoke(_touchStartPos);
                        CurrentInput.LastTapPosition = _touchStartPos;
                    }
                    else if (delta.magnitude >= _swipeThreshold)
                    {
                        // 滑动
                        OnSwipe?.Invoke(_touchStartPos, _lastTouchPos);
                        CurrentInput.LastSwipeDirection = delta.normalized;
                    }
                    _isTouching = false;
                }
                return;
            }

            if (touches.Count == 1)
            {
                var touch = touches[0];
                Vector2 pos = touch.screenPosition;

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    _isTouching = true;
                    _touchStartPos = pos;
                    _touchStartTime = Time.time;
                    _lastTouchPos = pos;
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved && _isTouching)
                {
                    Vector2 delta = pos - _lastTouchPos;
                    OnDrag?.Invoke(delta);
                    CurrentInput.DragDelta = delta;
                    _lastTouchPos = pos;
                }
            }
            else if (touches.Count == 2)
            {
                // 双指缩放
                var touch0 = touches[0];
                var touch1 = touches[1];

                float currentDist = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
                float prevDist = Vector2.Distance(
                    touch0.screenPosition - touch0.delta,
                    touch1.screenPosition - touch1.delta
                );

                float zoomDelta = (currentDist - prevDist) * _pinchZoomSpeed;
                OnPinchZoom?.Invoke(zoomDelta);
                CurrentInput.PinchZoomDelta = zoomDelta;
            }
        }

        private void ProcessGamepadInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            // A 键 - 投币
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                OnThrowCoin?.Invoke();
            }

            // Start 键 - 暂停
            if (gamepad.startButton.wasPressedThisFrame)
            {
                OnPausePressed?.Invoke();
            }

            // 摇杆 - 相机控制
            Vector2 stick = gamepad.leftStick.ReadValue();
            if (stick.magnitude > 0.1f)
            {
                CurrentInput.CameraRotation = stick;
            }

            // 扳机 - 缩放
            float triggerDelta = gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue();
            if (Mathf.Abs(triggerDelta) > 0.1f)
            {
                OnPinchZoom?.Invoke(-triggerDelta * 0.1f);
            }
        }

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        public Vector3 ScreenToWorld(Vector2 screenPos, Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return Vector3.zero;

            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane + 10f));
            return worldPos;
        }

        /// <summary>
        /// 射线检测
        /// </summary>
        public bool RaycastFromScreen(Vector2 screenPos, out RaycastHit hit, LayerMask layerMask, Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null)
            {
                hit = default;
                return false;
            }

            Ray ray = cam.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out hit, 100f, layerMask);
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }
    }

    /// <summary>
    /// 用户输入数据 - 复刻原版 UserInput
    /// </summary>
    public class UserInput
    {
        public Vector2 LastTapPosition { get; set; }
        public Vector2 LastSwipeDirection { get; set; }
        public Vector2 DragDelta { get; set; }
        public float PinchZoomDelta { get; set; }
        public Vector2 CameraRotation { get; set; }

        public void Reset()
        {
            LastTapPosition = Vector2.zero;
            LastSwipeDirection = Vector2.zero;
            DragDelta = Vector2.zero;
            PinchZoomDelta = 0;
            CameraRotation = Vector2.zero;
        }
    }

    /// <summary>
    /// 事件队列管理器 - 复刻原版 EventQueueManager
    /// 管理游戏事件的队列处理，确保事件按顺序执行
    /// </summary>
    public class EventQueueManager : MonoSingleton<EventQueueManager>
    {
        private Queue<GameEvent> _eventQueue = new();
        private Queue<GameEvent> _nextFrameQueue = new();
        private bool _isProcessing;

        [SerializeField] private int _maxEventsPerFrame = 100;

        public int PendingEventCount => _eventQueue.Count;

        /// <summary>
        /// 添加事件到队列
        /// </summary>
        public void Enqueue(GameEvent evt)
        {
            _eventQueue.Enqueue(evt);
        }

        /// <summary>
        /// 添加延迟事件(下一帧处理)
        /// </summary>
        public void EnqueueNextFrame(GameEvent evt)
        {
            _nextFrameQueue.Enqueue(evt);
        }

        /// <summary>
        /// 添加延迟事件(指定延迟)
        /// </summary>
        public void EnqueueDelayed(GameEvent evt, float delay)
        {
            StartCoroutine(IE_DelayedEnqueue(evt, delay));
        }

        private IEnumerator IE_DelayedEnqueue(GameEvent evt, float delay)
        {
            yield return new WaitForSeconds(delay);
            _eventQueue.Enqueue(evt);
        }

        private void Update()
        {
            // 处理上一帧的延迟事件
            while (_nextFrameQueue.Count > 0)
            {
                _eventQueue.Enqueue(_nextFrameQueue.Dequeue());
            }

            // 处理事件队列
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            int processed = 0;
            while (_eventQueue.Count > 0 && processed < _maxEventsPerFrame)
            {
                var evt = _eventQueue.Dequeue();
                try
                {
                    evt.Execute();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[EventQueue] Error processing event {evt.EventType}: {e}");
                }
                processed++;
            }

            _isProcessing = false;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void ClearQueue()
        {
            _eventQueue.Clear();
            _nextFrameQueue.Clear();
        }
    }

    /// <summary>
    /// 游戏事件基类
    /// </summary>
    public abstract class GameEvent
    {
        public abstract GameEventType EventType { get; }
        public float Timestamp { get; } = Time.time;

        public abstract void Execute();
    }

    public enum GameEventType
    {
        None,
        CoinSpawn,
        CoinCollect,
        CoinDestroy,
        CoinSettle,
        RoundStart,
        RoundEnd,
        ScoreChange,
        EffectTrigger,
        UIOpen,
        UIClose,
        GamePause,
        GameResume,
        ShopOpen,
        LevelUp,
        AchievementUnlock
    }

    /// <summary>
    /// 通用委托事件
    /// </summary>
    public class DelegateEvent : GameEvent
    {
        private readonly GameEventType _type;
        private readonly System.Action _action;

        public DelegateEvent(GameEventType type, System.Action action)
        {
            _type = type;
            _action = action;
        }

        public override GameEventType EventType => _type;

        public override void Execute()
        {
            _action?.Invoke();
        }
    }

    /// <summary>
    /// 游戏相机管理器 - 复刻原版 GameCameraManager
    /// 管理推币机视角相机
    /// </summary>
    public class GameCameraManager : MonoSingleton<GameCameraManager>
    {
        [Header("Camera References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _uiCamera;

        [Header("Position Settings")]
        [SerializeField] private Vector3 _defaultPosition = new Vector3(0, 10, -8);
        [SerializeField] private Vector3 _defaultRotation = new Vector3(45, 0, 0);
        [SerializeField] private Transform _lookTarget;

        [Header("Zoom Settings")]
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 20f;
        [SerializeField] private float _currentZoom = 10f;
        [SerializeField] private float _zoomSpeed = 2f;

        [Header("Rotation Settings")]
        [SerializeField] private float _rotationSpeed = 50f;
        [SerializeField] private float _minPitch = 20f;
        [SerializeField] private float _maxPitch = 80f;
        [SerializeField] private float _currentPitch = 45f;
        [SerializeField] private float _currentYaw;

        [Header("Smoothing")]
        [SerializeField] private float _positionSmoothTime = 0.1f;
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        private Vector3 _positionVelocity;
        private float _pitchVelocity;
        private float _yawVelocity;
        private bool _isDragging;

        public Camera MainCamera => _mainCamera;
        public float CurrentZoom => _currentZoom;

        protected override void Awake()
        {
            base.Awake();
            if (_mainCamera == null) _mainCamera = Camera.main;
        }

        private void Start()
        {
            // 注册输入事件
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPinchZoom += HandleZoom;
                InputManager.Instance.OnDrag += HandleDrag;
            }

            ResetCamera();
        }

        private void LateUpdate()
        {
            UpdateCameraPosition();
        }

        private void HandleZoom(float zoomDelta)
        {
            _currentZoom = Mathf.Clamp(_currentZoom - zoomDelta * _zoomSpeed, _minZoom, _maxZoom);
        }

        private void HandleDrag(Vector2 delta)
        {
            // 水平拖动旋转
            _currentYaw += delta.x * _rotationSpeed * Time.deltaTime;
            // 垂直拖动调整俯仰
            _currentPitch = Mathf.Clamp(_currentPitch - delta.y * _rotationSpeed * Time.deltaTime, _minPitch, _maxPitch);
        }

        private void UpdateCameraPosition()
        {
            if (_lookTarget == null) return;

            // 计算目标位置
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -_currentZoom);
            Vector3 targetPosition = _lookTarget.position + offset;

            // 平滑移动
            _mainCamera.transform.position = Vector3.SmoothDamp(
                _mainCamera.transform.position,
                targetPosition,
                ref _positionVelocity,
                _positionSmoothTime
            );

            // 看向目标
            _mainCamera.transform.LookAt(_lookTarget);
        }

        /// <summary>
        /// 重置相机到默认位置
        /// </summary>
        public void ResetCamera()
        {
            _currentZoom = 10f;
            _currentPitch = _defaultRotation.x;
            _currentYaw = _defaultRotation.y;

            if (_mainCamera != null)
            {
                _mainCamera.transform.position = _defaultPosition;
                _mainCamera.transform.rotation = Quaternion.Euler(_defaultRotation);
            }
        }

        /// <summary>
        /// 聚焦到指定位置
        /// </summary>
        public void FocusOn(Vector3 position, float zoom = -1)
        {
            if (_lookTarget != null)
            {
                _lookTarget.position = position;
            }
            if (zoom > 0)
            {
                _currentZoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);
            }
        }

        /// <summary>
        /// 震动效果
        /// </summary>
        public void Shake(float intensity = 0.5f, float duration = 0.3f)
        {
            StartCoroutine(IE_Shake(intensity, duration));
        }

        private IEnumerator IE_Shake(float intensity, float duration)
        {
            float elapsed = 0;
            Vector3 originalPos = _mainCamera.transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - (elapsed / duration);
                Vector3 offset = Random.insideUnitSphere * intensity * decay;
                _mainCamera.transform.localPosition = originalPos + offset;
                yield return null;
            }

            _mainCamera.transform.localPosition = originalPos;
        }

        /// <summary>
        /// 屏幕坐标转世界坐标(在指定平面上)
        /// </summary>
        public Vector3 ScreenToWorldOnPlane(Vector2 screenPos, float planeY = 0)
        {
            if (_mainCamera == null) return Vector3.zero;

            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            Plane plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPinchZoom -= HandleZoom;
                InputManager.Instance.OnDrag -= HandleDrag;
            }
        }
    }

    /// <summary>
    /// ProPixelizer 相机缩放 - 复刻原版 ProPixelizerCameraZoom
    /// 像素化渲染的相机控制
    /// </summary>
    public class ProPixelizerCameraZoom : MonoBehaviour
    {
        [Header("Pixel Settings")]
        [SerializeField] private int _basePixelSize = 3;
        [SerializeField] private int _minPixelSize = 1;
        [SerializeField] private int _maxPixelSize = 8;
        [SerializeField] private float _zoomSensitivity = 0.1f;

        private Camera _camera;
        private int _currentPixelSize;

        public int CurrentPixelSize => _currentPixelSize;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _currentPixelSize = _basePixelSize;
        }

        /// <summary>
        /// 设置像素大小
        /// </summary>
        public void SetPixelSize(int size)
        {
            _currentPixelSize = Mathf.Clamp(size, _minPixelSize, _maxPixelSize);
            // ProPixelizer 会通过相机组件读取此值
        }

        /// <summary>
        /// 根据缩放调整像素大小
        /// </summary>
        public void AdjustPixelSize(float zoomDelta)
        {
            float newSize = _currentPixelSize - zoomDelta * _zoomSensitivity;
            SetPixelSize(Mathf.RoundToInt(newSize));
        }
    }
}
