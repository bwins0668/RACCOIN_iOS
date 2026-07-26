using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Core;

namespace Raccoin.CoinPusher
{
    /// <summary>
    /// 推币机主管理器 - 复刻原版 CoinPusherManager
    /// </summary>
    public class CoinPusherManager : MonoSingleton<CoinPusherManager>
    {
        [Header("References")]
        [SerializeField] private PusherController _pusherController;
        [SerializeField] private CoinEntryController _coinEntry;
        [SerializeField] private CoinMachineController _machineController;
        [SerializeField] private ScoreBoardController _scoreBoard;
        [SerializeField] private LuckyWheelController _luckyWheel;
        [SerializeField] private SettleAreaController _settleArea;

        [Header("Round Settings")]
        [SerializeField] private int _currentRound = 1;
        [SerializeField] private int _maxRound = 50;

        public int CurrentRound => _currentRound;
        public bool IsRoundActive { get; private set; }
        public CoinPusherMediator Mediator { get; private set; }

        protected override void OnSingletonAwake()
        {
            Mediator = new CoinPusherMediator();
            StartCoroutine(IE_Init());
        }

        private IEnumerator IE_Init()
        {
            yield return StartCoroutine(IE_InitDetecter());
            Debug.Log("[CoinPusherManager] Initialized.");
        }

        private IEnumerator IE_InitDetecter()
        {
            // 初始化硬币检测器
            yield return null;
        }

        public void StartNewGame()
        {
            _currentRound = 1;
            StartCoroutine(IE_NewRound());
        }

        private IEnumerator IE_NewRound()
        {
            IsRoundActive = true;

            // 广播新回合
            yield return StartCoroutine(IE_NewRoundBroadcast());

            // 保存
            yield return StartCoroutine(IE_NewRoundSave());

            Debug.Log($"[CoinPusherManager] Round {_currentRound} started.");
        }

        private IEnumerator IE_NewRoundBroadcast()
        {
            Mediator?.BroadcastRoundStart(_currentRound);
            yield return null;
        }

        private IEnumerator IE_NewRoundSave()
        {
            yield return null;
        }

        public void EndRound()
        {
            IsRoundActive = false;
            Mediator?.BroadcastRoundEnd(_currentRound);
        }

        public void NextRound()
        {
            if (_currentRound < _maxRound)
            {
                _currentRound++;
                StartCoroutine(IE_NewRound());
            }
            else
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            Debug.Log("[CoinPusherManager] Game Over!");
            Mediator?.BroadcastGameOver();
        }

        public IEnumerator SpawnGiftCoin(int giftCoinId)
        {
            // 生成礼物硬币
            yield return null;
        }
    }

    /// <summary>
    /// 推币机中介者 - 复刻原版 CoinPusherMediator
    /// </summary>
    public class CoinPusherMediator
    {
        public event System.Action<int> OnRoundStart;
        public event System.Action<int> OnRoundEnd;
        public event System.Action OnGameOver;
        public event System.Action<long> OnScoreChanged;
        public event System.Action<int> OnCoinSettled;

        public void BroadcastRoundStart(int round) => OnRoundStart?.Invoke(round);
        public void BroadcastRoundEnd(int round) => OnRoundEnd?.Invoke(round);
        public void BroadcastGameOver() => OnGameOver?.Invoke();
        public void BroadcastScoreChanged(long score) => OnScoreChanged?.Invoke(score);
        public void BroadcastCoinSettled(int coinId) => OnCoinSettled?.Invoke(coinId);
    }

    /// <summary>
    /// 推币机子系统基类 - 复刻原版 CoinPusherSubSystem
    /// </summary>
    public abstract class CoinPusherSubSystem : MonoBehaviour
    {
        protected CoinPusherManager Manager => CoinPusherManager.Instance;
        protected CoinPusherMediator Mediator => CoinPusherManager.Instance?.Mediator;

        public virtual void Initialize() { }
        public virtual void OnRoundStart(int round) { }
        public virtual void OnRoundEnd(int round) { }
        public virtual void Cleanup() { }
    }

    /// <summary>
    /// 推板控制器 - 复刻原版 PusherController
    /// </summary>
    public class PusherController : CoinPusherSubSystem
    {
        [SerializeField] private float _pushSpeed = 0.5f;
        [SerializeField] private float _pushDistance = 1.5f;
        [SerializeField] private Transform _pusherTransform;

        private PusherState _state = PusherState.Idle;
        private float _progress;
        private Vector3 _startPos;
        private Vector3 _endPos;

        public PusherState State => _state;

        public override void Initialize()
        {
            if (_pusherTransform != null)
            {
                _startPos = _pusherTransform.localPosition;
                _endPos = _startPos + Vector3.forward * _pushDistance;
            }
            _state = PusherState.MovingForward;
        }

        private void FixedUpdate()
        {
            if (_state == PusherState.Idle || _state == PusherState.Paused) return;

            switch (_state)
            {
                case PusherState.MovingForward:
                    _progress += _pushSpeed * Time.fixedDeltaTime;
                    if (_progress >= 1f)
                    {
                        _progress = 1f;
                        _state = PusherState.MovingBackward;
                    }
                    break;

                case PusherState.MovingBackward:
                    _progress -= _pushSpeed * Time.fixedDeltaTime;
                    if (_progress <= 0f)
                    {
                        _progress = 0f;
                        _state = PusherState.MovingForward;
                    }
                    break;
            }

            if (_pusherTransform != null)
            {
                _pusherTransform.localPosition = Vector3.Lerp(_startPos, _endPos, _progress);
            }
        }

        public void Pause() => _state = PusherState.Paused;
        public void Resume() => _state = PusherState.MovingForward;
    }

    public enum PusherState
    {
        Idle = 0,
        MovingForward = 1,
        MovingBackward = 2,
        Paused = 3
    }

    /// <summary>
    /// 硬币投币口控制器 - 复刻原版 CoinEntryController
    /// </summary>
    public class CoinEntryController : CoinPusherSubSystem
    {
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private float _spawnCooldown = 0.3f;

        private float _lastSpawnTime;
        private int _coinsRemaining = 100;

        public int CoinsRemaining => _coinsRemaining;
        public bool CanSpawn => Time.time - _lastSpawnTime >= _spawnCooldown && _coinsRemaining > 0;

        public void SpawnCoin(SpawnCoinPos pos = SpawnCoinPos.Center)
        {
            if (!CanSpawn) return;

            _lastSpawnTime = Time.time;
            _coinsRemaining--;

            int index = (int)pos;
            Transform spawnPoint = _spawnPoints != null && index < _spawnPoints.Length
                ? _spawnPoints[index]
                : transform;

            if (_coinPrefab != null)
            {
                Instantiate(_coinPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }

        public IEnumerator IE_SpawnCoinRain(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnCoin((SpawnCoinPos)Random.Range(0, 3));
                yield return new WaitForSeconds(0.1f);
            }
        }

        public IEnumerator IE_SpawnDoomCoin()
        {
            SpawnCoin(SpawnCoinPos.Center);
            yield return null;
        }

        public void AddCoins(int amount) => _coinsRemaining += amount;
    }

    public enum SpawnCoinPos
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    /// <summary>
    /// 硬币机器控制器 - 复刻原版 CoinMachineController
    /// </summary>
    public class CoinMachineController : CoinPusherSubSystem
    {
        [SerializeField] private CoinNumDetecter[] _detecters;

        public override void Initialize()
        {
            StartCoroutine(IE_Init());
        }

        private IEnumerator IE_Init()
        {
            yield return StartCoroutine(IE_InitEntity());
            yield return StartCoroutine(IE_InitDetecter());
        }

        private IEnumerator IE_InitEntity()
        {
            yield return null;
        }

        private IEnumerator IE_InitDetecter()
        {
            yield return null;
        }

        private IEnumerator IE_DetecterInvoke()
        {
            // 检测器触发逻辑
            yield return null;
        }
    }

    /// <summary>
    /// 硬币数量检测器 - 复刻原版 CoinNumDetecter
    /// </summary>
    public class CoinNumDetecter : MonoBehaviour
    {
        [SerializeField] private DetecterPos _position;
        [SerializeField] private float _detectRadius = 1.0f;

        public int DetectedCoinCount { get; private set; }

        public DetecterPos Position => _position;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Coin"))
            {
                DetectedCoinCount++;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Coin"))
            {
                DetectedCoinCount = Mathf.Max(0, DetectedCoinCount - 1);
            }
        }
    }

    public enum DetecterPos
    {
        Left = 0,
        Center = 1,
        Right = 2,
        SettleArea = 3
    }

    /// <summary>
    /// 计分板控制器 - 复刻原版 ScoreBoardController
    /// </summary>
    public class ScoreBoardController : CoinPusherSubSystem
    {
        public long CurrentScore { get; private set; }
        public long HighestScore { get; private set; }
        public int DoomCoinCount { get; private set; }
        public int LeftCoinCount { get; private set; }
        public float PtRate { get; private set; }
        public float TicketRate { get; private set; }

        public void AddScore(long amount)
        {
            CurrentScore += amount;
            if (CurrentScore > HighestScore)
            {
                HighestScore = CurrentScore;
            }
            Mediator?.BroadcastScoreChanged(CurrentScore);
        }

        public void ResetRound()
        {
            CurrentScore = 0;
            DoomCoinCount = 0;
        }
    }

    /// <summary>
    /// 幸运转盘控制器(游戏内) - 复刻原版 LuckyWheelController
    /// </summary>
    public class LuckyWheelController : CoinPusherSubSystem
    {
        [SerializeField] private float _spinDuration = 3.0f;
        [SerializeField] private float _minRotations = 3.0f;

        public bool IsSpinning { get; private set; }
        public int SpinChargeCount { get; private set; }

        public void StartSpin()
        {
            if (IsSpinning) return;
            StartCoroutine(IE_WaitSpinChargeDeal());
        }

        private IEnumerator IE_WaitSpinChargeDeal()
        {
            IsSpinning = true;
            yield return new WaitForSeconds(_spinDuration);
            IsSpinning = false;
            // 确定奖励
        }
    }

    /// <summary>
    /// 结算区域控制器 - 复刻原版 SettleAreaController
    /// </summary>
    public class SettleAreaController : MonoBehaviour
    {
        [SerializeField] private float _settleDelay = 1.0f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Coin"))
            {
                StartCoroutine(SettleCoin(other.gameObject));
            }
        }

        private IEnumerator SettleCoin(GameObject coin)
        {
            yield return new WaitForSeconds(_settleDelay);
            // 硬币结算逻辑
            CoinPusherManager.Instance?.Mediator?.BroadcastCoinSettled(coin.GetInstanceID());
            Destroy(coin);
        }
    }

    /// <summary>
    /// 推板标记控制器 - 复刻原版 PusherMarkController
    /// </summary>
    public class PusherMarkController : MonoBehaviour
    {
        [SerializeField] private MarkType _markType;
        public MarkType Mark => _markType;
    }

    public enum MarkType
    {
        Gold = 0,
        Silver = 1,
        Bronze = 2
    }
}
