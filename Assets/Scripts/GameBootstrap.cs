using UnityEngine;
using System.Collections;
using Raccoin.Core;
using Raccoin.Data;
using Raccoin.Audio;
using Raccoin.Platform;

namespace Raccoin
{
    /// <summary>
    /// 强制加载初始化场景 - 复刻原版 ForceLoadInitScene
    /// 游戏入口点，负责初始化所有子系统
    /// </summary>
    public class ForceLoadInitScene : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            // 创建全局管理器 GameObject
            var go = new GameObject("[GameBootstrap]");
            go.AddComponent<GameBootstrap>();
            DontDestroyOnLoad(go);
        }
    }

    /// <summary>
    /// 游戏引导程序 - 初始化所有管理器
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private IEnumerator Start()
        {
            Debug.Log("[GameBootstrap] Starting RACCOIN iOS...");

            // 1. 初始化全局管理器
            var globalManager = gameObject.AddComponent<GlobalManager>();

            // 2. 初始化设置
            SettingManager.Instance.LoadSettings();
            SettingManager.Instance.ApplySettings();

            // 3. 初始化存档系统
            DataPersistentManager.Instance.Initialize();

            // 4. 初始化数据配置
            ExcelDataManager.Instance.Initialize();

            // 5. 初始化音频
            var audioObj = new GameObject("[AudioManager]");
            audioObj.transform.SetParent(transform);
            AudioManager.Instance.Initialize(audioObj);
            DontDestroyOnLoad(audioObj);

            // 6. iOS 平台初始化
#if UNITY_IOS
            var iosManager = gameObject.AddComponent<IOSPlatformManager>();
            var touchInput = gameObject.AddComponent<TouchInputManager>();
            GameCenterManager.Instance.Authenticate();
            
            // 性能优化
            PerformanceManager.Instance.OptimizeForMobile();
#else
            PerformanceManager.Instance.OptimizeForHighEnd();
#endif

            // 7. 初始化资源管理器
            yield return StartCoroutine(ResManager.Instance.IE_InitRes());

            // 8. 加载标题场景
            Debug.Log("[GameBootstrap] Initialization complete. Loading Title...");
            yield return StartCoroutine(SceneLoader.Instance.LoadNewScene(SceneLoader.SceneName.Title));
        }
    }

    /// <summary>
    /// 构建版本配置 - 复刻原版 BuildVersionConfig (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "BuildVersionConfig", menuName = "RACCOIN/BuildVersionConfig")]
    public class BuildVersionConfig : ScriptableObject
    {
        public string Version = "1.0.0";
        public int BuildNumber = 1;
        public string CommitHash = "";
        public bool IsRelease = false;
    }

    /// <summary>
    /// 浣熊构建版本 - 复刻原版 RabbitBuildVersion
    /// </summary>
    public static class RabbitBuildVersion
    {
        public const string VERSION = "1.0.0";
        public const int BUILD = 1;
        public const string ENGINE = "Unity 6000.3.0f1";
        public const string PLATFORM = "iOS";
    }

    /// <summary>
    /// 路径定义 - 复刻原版 PathDefine
    /// </summary>
    public static class PathDefine
    {
        public const string PREFAB_PATH = "Assets/Resources/Prefabs/";
        public const string SPRITE_PATH = "Assets/Resources/Sprites/";
        public const string CONFIG_PATH = "Assets/Resources/Config/";
        public const string AUDIO_PATH = "Assets/Resources/Audio/";
        public const string FX_PATH = "Assets/Resources/FX/";
        public const string MATERIAL_PATH = "Assets/Resources/Materials/";
    }

    /// <summary>
    /// 规则定义 - 复刻原版 RuleDefine
    /// </summary>
    public static class RuleDefine
    {
        public const int MAX_COIN_ON_SCREEN = 500;
        public const int MAX_COIN_ON_SCREEN_MOBILE = 300;
        public const float COIN_SPAWN_INTERVAL = 0.3f;
        public const float PUSHER_SPEED = 0.5f;
        public const float PUSHER_DISTANCE = 1.5f;
        public const int DEFAULT_ROUND_COUNT = 50;
        public const float SETTLE_DELAY = 1.0f;
        public const int SHOP_EVERY_N_ROUNDS = 3;
    }

    /// <summary>
    /// 存档定义 - 复刻原版 SaveDefine
    /// </summary>
    public static class SaveDefine
    {
        public const string SAVE_FOLDER = "saves";
        public const string COMMON_SAVE = "common_save";
        public const string SETTING_SAVE = "common_setting";
        public const string PROFILE_SAVE = "profile";
        public const string GAME_SAVE = "game_data";
        public const int MAX_SAVE_SLOTS = 3;
    }
}
