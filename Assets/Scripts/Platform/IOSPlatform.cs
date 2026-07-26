using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;
using Raccoin.Core;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Raccoin.Platform
{
    /// <summary>
    /// iOS 平台管理器 - 替代原版 SteamManager/SteamInterface
    /// </summary>
    public class IOSPlatformManager : MonoSingleton<IOSPlatformManager>
    {
        public bool IsInitialized { get; private set; }
        public string PlayerId { get; private set; }

        protected override void OnSingletonAwake()
        {
            InitializePlatform();
        }

        private void InitializePlatform()
        {
#if UNITY_IOS && !UNITY_EDITOR
            // iOS 平台初始化
            UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
            
            // 安全区域适配
            ApplySafeArea();
#endif
            IsInitialized = true;
            PlayerId = SystemInfo.deviceUniqueIdentifier;
            Debug.Log($"[IOSPlatformManager] Initialized. Device: {SystemInfo.deviceModel}");
        }

        private void ApplySafeArea()
        {
            // 由 SafeAreaController 处理
        }

        public void RequestReview()
        {
#if UNITY_IOS && !UNITY_EDITOR
            // 请求 App Store 评价
            UnityEngine.iOS.Device.RequestStoreReview();
#endif
        }

        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }

    /// <summary>
    /// 安全区域控制器 - iPad 刘海/圆角适配
    /// </summary>
    public class SafeAreaController : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (_rectTransform == null) return;

            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _lastSafeArea = safeArea;
        }
    }

    /// <summary>
    /// 触摸输入管理器 - 替代原版键鼠输入
    /// 复刻原版 InputManager + TouchscreenUIController
    /// </summary>
    public class TouchInputManager : MonoSingleton<TouchInputManager>
    {
        [Header("Coin Throw Settings")]
        [SerializeField] private float _swipeThreshold = 50f;
        [SerializeField] private float _tapRadius = 30f;

        [Header("Camera Settings")]
        [SerializeField] private float _pinchZoomSpeed = 0.01f;
        [SerializeField] private float _rotateSpeed = 0.5f;
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 15f;

        private Camera _mainCamera;
        private float _currentZoom = 8f;
        private Vector2 _lastTouchPos;
        private bool _isDragging;

        public event System.Action<Vector2> OnCoinThrow;
        public event System.Action<Vector2> OnTap;
        public event System.Action OnPinchZoom;

        protected override void OnSingletonAwake()
        {
            _mainCamera = Camera.main;
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            var touches = Touch.activeTouches;

            if (touches.Count == 1)
            {
                HandleSingleTouch(touches[0]);
            }
            else if (touches.Count == 2)
            {
                HandlePinchZoom(touches[0], touches[1]);
            }
        }

        private void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    _lastTouchPos = touch.screenPosition;
                    _isDragging = false;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    float delta = Vector2.Distance(touch.screenPosition, _lastTouchPos);
                    if (delta > _tapRadius)
                    {
                        _isDragging = true;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                    if (!_isDragging)
                    {
                        // 点击 - 投币
                        OnTap?.Invoke(touch.screenPosition);
                        OnCoinThrow?.Invoke(touch.screenPosition);
                    }
                    else
                    {
                        // 滑动 - 相机旋转
                        Vector2 swipeDelta = touch.screenPosition - _lastTouchPos;
                        if (swipeDelta.magnitude > _swipeThreshold)
                        {
                            RotateCamera(swipeDelta);
                        }
                    }
                    break;
            }
        }

        private void HandlePinchZoom(Touch touch0, Touch touch1)
        {
            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
            float previousDistance = Vector2.Distance(
                touch0.screenPosition - touch0.delta,
                touch1.screenPosition - touch1.delta
            );

            float zoomDelta = currentDistance - previousDistance;
            _currentZoom -= zoomDelta * _pinchZoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);

            if (_mainCamera != null)
            {
                _mainCamera.orthographicSize = _currentZoom;
            }

            OnPinchZoom?.Invoke();
        }

        private void RotateCamera(Vector2 delta)
        {
            if (_mainCamera != null)
            {
                Transform camTransform = _mainCamera.transform;
                camTransform.RotateAround(Vector3.zero, Vector3.up, delta.x * _rotateSpeed);
            }
        }

        private void OnDestroy()
        {
            if (EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Disable();
            }
        }
    }

    /// <summary>
    /// 触觉反馈管理器 - iOS Haptics
    /// </summary>
    public class HapticsManager : Singleton<HapticsManager>
    {
        public bool IsEnabled { get; set; } = true;

        public void CoinDrop()
        {
            if (!IsEnabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            // 轻触觉反馈
            Handheld.Vibrate();
#endif
        }

        public void CoinSettle()
        {
            if (!IsEnabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public void BigWin()
        {
            if (!IsEnabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            // 强触觉反馈序列
            Handheld.Vibrate();
#endif
        }

        public void UIButton()
        {
            if (!IsEnabled) return;
            // 轻微触觉
        }
    }

    /// <summary>
    /// Game Center 管理器 - 替代 Steam 成就/排行榜
    /// </summary>
    public class GameCenterManager : Singleton<GameCenterManager>
    {
        public bool IsAuthenticated { get; private set; }

        public void Authenticate()
        {
#if UNITY_IOS && !UNITY_EDITOR
            // 使用 Unity Social API 或原生 Game Center
            Social.localUser.Authenticate(success => {
                IsAuthenticated = success;
                Debug.Log($"[GameCenter] Auth: {success}");
            });
#endif
        }

        public void ReportScore(long score, string leaderboardId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Social.ReportScore(score, leaderboardId, success => {
                Debug.Log($"[GameCenter] Score reported: {success}");
            });
#endif
        }

        public void UnlockAchievement(string achievementId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Social.ReportProgress(achievementId, 100.0, success => {
                Debug.Log($"[GameCenter] Achievement: {success}");
            });
#endif
        }

        public void ShowLeaderboard()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Social.ShowLeaderboardUI();
#endif
        }

        public void ShowAchievements()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Social.ShowAchievementsUI();
#endif
        }
    }

    /// <summary>
    /// 成就管理器 - 复刻原版 AchieveManager
    /// </summary>
    public class AchieveManager : Singleton<AchieveManager>
    {
        public void UnlockAchievement(string id)
        {
            GameCenterManager.Instance.UnlockAchievement(id);
        }

        public void ReportProgress(string id, float progress)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Social.ReportProgress(id, progress, _ => { });
#endif
        }
    }
}
