using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Raccoin.Core;
using Raccoin.CoinPusher;

namespace Raccoin.UI
{
    /// <summary>
    /// 经典推币机玩法桥接器
    /// 负责把 输入 / 推币机子系统 / UI 三者连接起来。
    /// 由 SceneBuilder 在构建 GameClassic 场景时自动挂载并连线。
    /// </summary>
    public class ClassicGameBridge : MonoBehaviour
    {
        [Header("推币机子系统 (由 SceneBuilder 赋值)")]
        public CoinEntryController CoinEntry;
        public ScoreBoardController ScoreBoard;
        public GameObject CoinPrefab;

        [Header("UI 引用 (由 SceneBuilder 赋值)")]
        public Text ScoreText;
        public Text CoinCountText;
        public Text RoundText;
        public Button BackButton;

        [Header("初始硬币")]
        public int InitialCoinCount = 55;
        public long ScorePerCoin = 10;

        private long _score;

        private void Start()
        {
            // 返回按钮
            if (BackButton != null)
                BackButton.onClick.AddListener(() => SceneManager.LoadScene(1)); // 回到 Title

            // 订阅投币机结算事件
            var mediator = CoinPusherManager.Instance != null ? CoinPusherManager.Instance.Mediator : null;
            if (mediator != null)
                mediator.OnCoinSettled += OnCoinSettled;

            // 订阅输入 (触摸/点击 -> 投币)
            if (InputManager.Instance != null)
                InputManager.Instance.OnTap += OnTap;

            // 开始新游戏并铺设初始硬币床
            if (CoinPusherManager.Instance != null)
                CoinPusherManager.Instance.StartNewGame();

            StartCoroutine(SpawnInitialCoins());
            RefreshUI();

            Debug.Log("[ClassicGameBridge] Classic coin pusher ready. Tap to drop coins!");
        }

        /// <summary>触摸 -> 根据触摸位置选择投币口投币</summary>
        private void OnTap(Vector2 screenPos)
        {
            if (CoinEntry == null) return;

            // 根据屏幕 X 坐标选择左/中/右投币口
            float nx = screenPos.x / Mathf.Max(1f, Screen.width);
            SpawnCoinPos pos = nx < 0.38f ? SpawnCoinPos.Left
                             : nx > 0.62f ? SpawnCoinPos.Right
                             : SpawnCoinPos.Center;

            CoinEntry.SpawnCoin(pos);
            RefreshUI();
        }

        /// <summary>硬币结算 -> 加分</summary>
        private void OnCoinSettled(int coinId)
        {
            _score += ScorePerCoin;
            if (ScoreBoard != null) ScoreBoard.AddScore(ScorePerCoin);
            RefreshUI();
        }

        /// <summary>铺设初始硬币床 (随机散布在推板前方, 分批生成避免物理爆炸)</summary>
        private IEnumerator SpawnInitialCoins()
        {
            if (CoinPrefab == null) yield break;

            for (int i = 0; i < InitialCoinCount; i++)
            {
                float x = Random.Range(-2.5f, 2.5f);
                float z = Random.Range(0.5f, 3.5f);
                float y = 0.4f + (i % 8) * 0.25f; // 分层高度, 像下雨一样落下
                Vector3 pos = new Vector3(x, y, z);
                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                Instantiate(CoinPrefab, pos, rot);

                if (i % 4 == 0) yield return new WaitForSeconds(0.04f);
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (ScoreText != null) ScoreText.text = _score.ToString("D6");
            if (CoinCountText != null)
                CoinCountText.text = CoinEntry != null ? CoinEntry.CoinsRemaining.ToString() : "0";
            if (RoundText != null && CoinPusherManager.Instance != null)
                RoundText.text = $"ROUND {CoinPusherManager.Instance.CurrentRound}";
        }

        private void Update()
        {
            // 硬币数量随消耗实时刷新
            if (CoinCountText != null && CoinEntry != null)
                CoinCountText.text = CoinEntry.CoinsRemaining.ToString();
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnTap -= OnTap;
        }
    }
}
