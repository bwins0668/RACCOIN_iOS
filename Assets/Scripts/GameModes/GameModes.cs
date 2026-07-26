using UnityEngine;
using System.Collections;
using Raccoin.Core;

namespace Raccoin.GameModes
{
    /// <summary>
    /// 经典模式管理器 - 复刻原版 ClassicGameManager
    /// </summary>
    public class ClassicGameManager : MonoSingleton<ClassicGameManager>
    {
        [SerializeField] private ClassicFactoryManager _factory;
        [SerializeField] private ClassicPusherManager _pusher;
        [SerializeField] private ClassicToolManager _tool;

        public ClassicData GameData { get; private set; }

        protected override void OnSingletonAwake()
        {
            StartCoroutine(IE_Init());
        }

        private IEnumerator IE_Init()
        {
            GameData = new ClassicData();
            yield return StartCoroutine(IE_InitClassic());
        }

        private IEnumerator IE_InitClassic()
        {
            _factory?.Initialize();
            _pusher?.Initialize();
            _tool?.Initialize();
            yield return null;
        }

        private IEnumerator IE_Save()
        {
            DataPersistentManager.Instance.SaveAll();
            yield return null;
        }

        private IEnumerator IE_Shake()
        {
            // 机器震动效果
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// 经典工厂管理器 - 复刻原版 ClassicFactoryManager
    /// </summary>
    public class ClassicFactoryManager : MonoBehaviour
    {
        public void Initialize() { StartCoroutine(IE_Init()); }

        private IEnumerator IE_Init()
        {
            yield return null;
        }

        private IEnumerator IE_SaveDeal()
        {
            yield return null;
        }
    }

    /// <summary>
    /// 经典推板管理器 - 复刻原版 ClassicPusherManager
    /// </summary>
    public class ClassicPusherManager : MonoBehaviour
    {
        public void Initialize() { StartCoroutine(IE_Init()); }

        private IEnumerator IE_Init()
        {
            yield return StartCoroutine(IE_InitDetecter());
        }

        private IEnumerator IE_InitDetecter()
        {
            yield return null;
        }

        private IEnumerator IE_NewGameSave()
        {
            yield return null;
        }

        private IEnumerator SpawnGiftCoin()
        {
            yield return null;
        }
    }

    /// <summary>
    /// 经典工具管理器 - 复刻原版 ClassicToolManager
    /// </summary>
    public class ClassicToolManager : MonoBehaviour
    {
        public void Initialize() { StartCoroutine(IE_Init()); }

        private IEnumerator IE_Init() { yield return null; }
        private IEnumerator IE_SaveDeal() { yield return null; }
    }

    /// <summary>
    /// Lab 模式管理器 - 复刻原版 LabGameManager
    /// </summary>
    public class LabGameManager : MonoSingleton<LabGameManager>
    {
        [SerializeField] private LabPusherManager _pusher;
        [SerializeField] private LabScoreBoardController _scoreBoard;

        protected override void OnSingletonAwake()
        {
            StartCoroutine(IE_Init());
        }

        private IEnumerator IE_Init()
        {
            yield return StartCoroutine(IE_InitGameplay());
        }

        private IEnumerator IE_InitGameplay()
        {
            yield return null;
        }

        private IEnumerator IE_Shake()
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Lab 推板管理器 - 复刻原版 LabPusherManager
    /// </summary>
    public class LabPusherManager : MonoBehaviour
    {
        private IEnumerator IE_Init() { yield return null; }
        private IEnumerator IE_InitDetecter() { yield return null; }
        private IEnumerator IE_NewRound() { yield return null; }
        private IEnumerator IE_NewRoundBroadcast() { yield return null; }
        private IEnumerator SpawnGiftCoin() { yield return null; }
    }

    /// <summary>
    /// Lab 计分板 - 复刻原版 LabScoreBoardController
    /// </summary>
    public class LabScoreBoardController : MonoBehaviour { }

    /// <summary>
    /// Kindle 小游戏系统 - 复刻原版 Kindle_* 系列
    /// </summary>
    public class Kindle_BonusSlotController : MonoBehaviour
    {
        private IEnumerator IE_Init() { yield return null; }
    }

    public class Kindle_BulbController : MonoBehaviour
    {
        private IEnumerator IE_ClearBulb() { yield return null; }
        private IEnumerator IE_KindleEvent() { yield return null; }
    }

    public class Kindle_CoinEntryController : MonoBehaviour
    {
        private IEnumerator IE_InitEntity() { yield return null; }
        private IEnumerator SpawnGiftCoin() { yield return null; }
    }

    public class Kindle_WormController : MonoBehaviour { }

    // ===== 数据类 =====

    /// <summary>
    /// 经典模式数据 - 复刻原版 ClassicData
    /// </summary>
    [System.Serializable]
    public class ClassicData : InGameData
    {
        public int TotalRoundsPlayed;
        public long BestScore;
        public int BestRound;
    }

    /// <summary>
    /// 经典 Kindle 数据 - 复刻原版 ClassicKindleData
    /// </summary>
    [System.Serializable]
    public class ClassicKindleData : ClassicData
    {
        public int BulbLitCount;
        public int BonusSlotHits;
    }

    /// <summary>
    /// 经典 RPG 数据 - 复刻原版 ClassicRPGData
    /// </summary>
    [System.Serializable]
    public class ClassicRPGData : ClassicData
    {
        public int EnemiesDefeated;
        public int PlayerDeaths;
        public long TotalDamageDealt;
    }
}
