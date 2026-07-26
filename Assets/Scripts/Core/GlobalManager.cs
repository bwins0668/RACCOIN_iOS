using UnityEngine;
using System.Collections;

namespace Raccoin.Core
{
    /// <summary>
    /// 全局管理器 - 复刻原版 GlobalManager
    /// 负责全局初始化、平台检测、退出逻辑
    /// </summary>
    public class GlobalManager : MonoSingleton<GlobalManager>
    {
        [Header("Platform")]
        [SerializeField] private RuntimePlatform _currentPlatform;
        [SerializeField] private bool _isInitialized;

        public RuntimePlatform CurrentPlatform => _currentPlatform;
        public bool IsInitialized => _isInitialized;
        public GameMode CurrentGameMode { get; private set; }

        protected override void OnSingletonAwake()
        {
            _currentPlatform = Application.platform;
            StartCoroutine(IE_Init());
        }

        private IEnumerator IE_Init()
        {
            // 平台检测
            yield return StartCoroutine(IE_PlatformCheck());

            // 初始化各子系统
            yield return StartCoroutine(IE_ReadAllStartCoinPattern());

            _isInitialized = true;
            Debug.Log("[GlobalManager] Initialization complete.");
        }

        private IEnumerator IE_PlatformCheck()
        {
#if UNITY_IOS
            Debug.Log("[GlobalManager] Platform: iOS");
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#elif UNITY_ANDROID
            Debug.Log("[GlobalManager] Platform: Android");
            Application.targetFrameRate = 60;
#else
            Debug.Log("[GlobalManager] Platform: Standalone");
            Application.targetFrameRate = 120;
#endif
            yield return null;
        }

        private IEnumerator IE_ReadAllStartCoinPattern()
        {
            // 读取初始硬币排列模式
            yield return null;
        }

        public void SetGameMode(GameMode mode)
        {
            CurrentGameMode = mode;
        }

        public IEnumerator IE_Quit()
        {
            // 保存数据
            if (GameInterfaceManager.HasInstance)
            {
                yield return StartCoroutine(GameInterfaceManager.Instance.IE_Save(SaveSource.Quit));
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public IEnumerator IE_RebuildRect()
        {
            // 重建游戏区域（分辨率变化时）
            yield return null;
        }
    }

    public enum GameMode
    {
        None = 0,
        Classic = 1,
        Lab = 2,
        Challenge = 3
    }

    public enum SaveSource
    {
        Auto = 0,
        Manual = 1,
        Quit = 2,
        RoundEnd = 3,
        SceneChange = 4
    }
}
