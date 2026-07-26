using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Core;
using Raccoin.FX;
using Raccoin.Effects;
using Raccoin.CoinPusher;

namespace Raccoin.CoinPusher.View
{
    /// <summary>
    /// 硬币行为控制器 - 复刻原版 GameCoinActionController
    /// 管理硬币的所有行为动作：投掷、移动、碰撞、结算
    /// </summary>
    public class GameCoinActionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CoinViewBase _coinView;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;

        [Header("Throw Settings")]
        [SerializeField] private float _throwForce = 5f;
        [SerializeField] private float _throwTorque = 2f;
        [SerializeField] private Vector3 _throwDirection = Vector3.forward;

        [Header("State")]
        [SerializeField] private CoinActionState _currentState = CoinActionState.Idle;

        public CoinActionState CurrentState => _currentState;
        public CoinViewBase CoinView => _coinView;
        public bool IsSettled { get; private set; }
        public float LifeTime { get; private set; }

        public enum CoinActionState
        {
            Idle,
            Throwing,
            Flying,
            Landing,
            Settling,
            Settled,
            Collecting,
            Destroying
        }

        private void Awake()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_coinView == null) _coinView = GetComponent<CoinViewBase>();
        }

        /// <summary>
        /// 投掷硬币
        /// </summary>
        public void ThrowCoin(Vector3 direction, float forceMultiplier = 1f)
        {
            if (_currentState != CoinActionState.Idle) return;

            _currentState = CoinActionState.Throwing;
            IsSettled = false;

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.AddForce(direction.normalized * _throwForce * forceMultiplier, ForceMode.Impulse);
                _rb.AddTorque(Random.insideUnitSphere * _throwTorque, ForceMode.Impulse);
            }

            StartCoroutine(IE_ThrowSequence());
        }

        private IEnumerator IE_ThrowSequence()
        {
            // 投掷动画
            _currentState = CoinActionState.Flying;
            FX_Manager.Instance.PlayFX(FX_Manager.FXType.CoinDrop, transform.position);

            // 等待落地
            yield return new WaitUntil(() => _rb != null && _rb.linearVelocity.magnitude < 0.1f);

            _currentState = CoinActionState.Landing;
            yield return new WaitForSeconds(0.2f);

            _currentState = CoinActionState.Settling;
            yield return new WaitForSeconds(0.5f);

            Settle();
        }

        /// <summary>
        /// 硬币结算
        /// </summary>
        public void Settle()
        {
            _currentState = CoinActionState.Settled;
            IsSettled = true;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            // 触发结算事件
            CoinPusherManager.Instance?.Mediator?.BroadcastCoinSettled(_coinView.CoinId);
        }

        /// <summary>
        /// 收集硬币
        /// </summary>
        public void Collect()
        {
            if (_currentState == CoinActionState.Collecting) return;
            _currentState = CoinActionState.Collecting;

            StartCoroutine(IE_CollectSequence());
        }

        private IEnumerator IE_CollectSequence()
        {
            FX_Manager.Instance.PlayFX(FX_Manager.FXType.CoinCollect, transform.position);

            // 收集动画 - 缩小消失
            float elapsed = 0;
            Vector3 startScale = transform.localScale;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / 0.3f);
                yield return null;
            }

            // 回收到对象池
            ObjectPool.Instance.Return(gameObject);
        }

        /// <summary>
        /// 销毁硬币
        /// </summary>
        public void Destroy()
        {
            _currentState = CoinActionState.Destroying;
            FX_Manager.Instance.PlayFX(FX_Manager.FXType.CoinDestroy, transform.position);
            ObjectPool.Instance.Return(gameObject);
        }

        /// <summary>
        /// 重置状态(对象池复用)
        /// </summary>
        public void ResetState()
        {
            _currentState = CoinActionState.Idle;
            IsSettled = false;
            LifeTime = 0;
            transform.localScale = Vector3.one;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = false;
            }
        }

        private void Update()
        {
            LifeTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// 夹取硬币视图 - 复刻原版 ClipCoinView
    /// 用于夹娃娃机模式的硬币视图
    /// </summary>
    public class ClipCoinView : CoinViewBase
    {
        [Header("Clip Settings")]
        [SerializeField] private bool _isClipped;
        [SerializeField] private Transform _clipParent;
        [SerializeField] private float _clipStrength = 0.8f;

        public bool IsClipped => _isClipped;

        /// <summary>
        /// 尝试夹取
        /// </summary>
        public bool TryClip(Transform clipper)
        {
            if (_isClipped) return false;

            // 根据夹取强度判断是否成功
            if (Random.value > _clipStrength)
            {
                // 夹取失败 - 滑落
                StartCoroutine(IE_SlipOff());
                return false;
            }

            _isClipped = true;
            _clipParent = clipper;
            transform.SetParent(clipper);
            return true;
        }

        /// <summary>
        /// 释放硬币
        /// </summary>
        public void Release()
        {
            if (!_isClipped) return;
            _isClipped = false;
            transform.SetParent(null);

            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
            }
        }

        private IEnumerator IE_SlipOff()
        {
            // 滑落动画
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Random.onUnitSphere * 2f, ForceMode.Impulse);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 硬币无效检测器 - 复刻原版 CoinInvalidDetector
    /// 检测硬币是否处于无效状态(卡住、出界等)
    /// </summary>
    public class CoinInvalidDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float _stuckThreshold = 0.01f;
        [SerializeField] private float _stuckDuration = 5f;
        [SerializeField] private Bounds _validBounds = new Bounds(Vector3.zero, new Vector3(20, 10, 20));
        [SerializeField] private float _checkInterval = 0.5f;

        private Vector3 _lastPosition;
        private float _stuckTimer;
        private float _checkTimer;

        public event System.Action<CoinViewBase> OnCoinInvalid;

        private void Update()
        {
            _checkTimer += Time.deltaTime;
            if (_checkTimer < _checkInterval) return;
            _checkTimer = 0;

            CheckAllCoins();
        }

        private void CheckAllCoins()
        {
            var coins = FindObjectsByType<CoinViewBase>(FindObjectsSortMode.None);
            foreach (var coin in coins)
            {
                if (IsCoinInvalid(coin))
                {
                    OnCoinInvalid?.Invoke(coin);
                    HandleInvalidCoin(coin);
                }
            }
        }

        /// <summary>
        /// 检测硬币是否无效
        /// </summary>
        public bool IsCoinInvalid(CoinViewBase coin)
        {
            if (coin == null) return true;

            // 出界检测
            if (!_validBounds.Contains(coin.transform.position))
            {
                return true;
            }

            // 卡住检测
            var rb = coin.GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.magnitude < _stuckThreshold)
            {
                _stuckTimer += _checkInterval;
                if (_stuckTimer > _stuckDuration)
                {
                    return true;
                }
            }
            else
            {
                _stuckTimer = 0;
            }

            return false;
        }

        private void HandleInvalidCoin(CoinViewBase coin)
        {
            // 重置或销毁无效硬币
            var action = coin.GetComponent<GameCoinActionController>();
            if (action != null)
            {
                action.Destroy();
            }
            else
            {
                coin.DestroyCoin();
            }
        }
    }

    /// <summary>
    /// 特殊硬币投掷预测器 - 复刻原版 SpecialCoinThrowPredictor
    /// 预测特殊硬币的投掷轨迹和落点
    /// </summary>
    public class SpecialCoinThrowPredictor : MonoBehaviour
    {
        [Header("Prediction Settings")]
        [SerializeField] private int _predictionSteps = 50;
        [SerializeField] private float _timeStep = 0.02f;
        [SerializeField] private float _gravity = -9.81f;

        [Header("Visualization")]
        [SerializeField] private LineRenderer _trajectoryLine;
        [SerializeField] private GameObject _landingIndicator;

        private List<Vector3> _trajectoryPoints = new();

        /// <summary>
        /// 预测投掷轨迹
        /// </summary>
        public List<Vector3> PredictTrajectory(Vector3 startPos, Vector3 velocity)
        {
            _trajectoryPoints.Clear();
            Vector3 pos = startPos;
            Vector3 vel = velocity;

            for (int i = 0; i < _predictionSteps; i++)
            {
                _trajectoryPoints.Add(pos);
                vel += Vector3.up * (_gravity * _timeStep);
                pos += vel * _timeStep;

                // 地面碰撞检测
                if (pos.y <= 0)
                {
                    _trajectoryPoints.Add(new Vector3(pos.x, 0, pos.z));
                    break;
                }
            }

            return _trajectoryPoints;
        }

        /// <summary>
        /// 预测落点
        /// </summary>
        public Vector3 PredictLandingPoint(Vector3 startPos, Vector3 velocity)
        {
            var trajectory = PredictTrajectory(startPos, velocity);
            return trajectory.Count > 0 ? trajectory[^1] : startPos;
        }

        /// <summary>
        /// 显示预测轨迹
        /// </summary>
        public void ShowTrajectory(Vector3 startPos, Vector3 velocity)
        {
            var trajectory = PredictTrajectory(startPos, velocity);

            if (_trajectoryLine != null)
            {
                _trajectoryLine.enabled = true;
                _trajectoryLine.positionCount = trajectory.Count;
                _trajectoryLine.SetPositions(trajectory.ToArray());
            }

            if (_landingIndicator != null && trajectory.Count > 0)
            {
                _landingIndicator.SetActive(true);
                _landingIndicator.transform.position = trajectory[^1];
            }
        }

        /// <summary>
        /// 隐藏预测轨迹
        /// </summary>
        public void HideTrajectory()
        {
            if (_trajectoryLine != null) _trajectoryLine.enabled = false;
            if (_landingIndicator != null) _landingIndicator.SetActive(false);
        }

        /// <summary>
        /// 计算最佳投掷角度
        /// </summary>
        public float CalculateOptimalAngle(Vector3 start, Vector3 target, float speed)
        {
            float distance = Vector3.Distance(start, target);
            float heightDiff = target.y - start.y;

            // 抛体运动公式
            float v2 = speed * speed;
            float v4 = v2 * v2;
            float g = Mathf.Abs(_gravity);

            float discriminant = v4 - g * (g * distance * distance + 2 * heightDiff * v2);
            if (discriminant < 0) return 45f; // 无法到达，返回默认角度

            float angle = Mathf.Atan2(v2 - Mathf.Sqrt(discriminant), g * distance);
            return angle * Mathf.Rad2Deg;
        }
    }

    /// <summary>
    /// 硬币投掷控制器 - 管理投币口和投掷逻辑
    /// </summary>
    public class CoinThrowController : MonoBehaviour
    {
        [Header("Throw Points")]
        [SerializeField] private Transform[] _throwPoints;
        [SerializeField] private int _currentThrowPointIndex;

        [Header("Settings")]
        [SerializeField] private float _throwCooldown = 0.3f;
        [SerializeField] private GameObject _coinPrefab;

        private float _lastThrowTime;
        private Queue<GameObject> _coinQueue = new();

        public bool CanThrow => Time.time - _lastThrowTime >= _throwCooldown;

        /// <summary>
        /// 投掷硬币
        /// </summary>
        public bool ThrowCoin(float forceMultiplier = 1f)
        {
            if (!CanThrow) return false;

            var throwPoint = GetCurrentThrowPoint();
            if (throwPoint == null) return false;

            var coin = ObjectPool.Instance.Get("Coin", _coinPrefab, throwPoint.position, throwPoint.rotation);
            var action = coin.GetComponent<GameCoinActionController>();
            if (action != null)
            {
                action.ResetState();
                action.ThrowCoin(throwPoint.forward, forceMultiplier);
            }

            _lastThrowTime = Time.time;
            return true;
        }

        /// <summary>
        /// 切换投币点
        /// </summary>
        public void SwitchThrowPoint(int direction)
        {
            if (_throwPoints == null || _throwPoints.Length == 0) return;
            _currentThrowPointIndex = (_currentThrowPointIndex + direction + _throwPoints.Length) % _throwPoints.Length;
        }

        public Transform GetCurrentThrowPoint()
        {
            if (_throwPoints == null || _throwPoints.Length == 0) return transform;
            return _throwPoints[_currentThrowPointIndex];
        }
    }

    /// <summary>
    /// 硬币动画控制器 - 管理硬币的视觉动画
    /// </summary>
    public class CoinAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Material[] _tierMaterials;

        private static readonly int HashSpin = Animator.StringToHash("Spin");
        private static readonly int HashCollect = Animator.StringToHash("Collect");
        private static readonly int HashDestroy = Animator.StringToHash("Destroy");
        private static readonly int HashTier = Animator.StringToHash("Tier");

        public void PlaySpinAnimation()
        {
            if (_animator != null) _animator.SetTrigger(HashSpin);
        }

        public void PlayCollectAnimation()
        {
            if (_animator != null) _animator.SetTrigger(HashCollect);
        }

        public void PlayDestroyAnimation()
        {
            if (_animator != null) _animator.SetTrigger(HashDestroy);
        }

        public void SetTier(int tier)
        {
            if (_animator != null) _animator.SetInteger(HashTier, tier);
            if (_renderer != null && _tierMaterials != null && tier < _tierMaterials.Length)
            {
                _renderer.material = _tierMaterials[tier];
            }
        }

        /// <summary>
        /// 闪烁效果
        /// </summary>
        public IEnumerator IE_Flash(Color flashColor, float duration = 0.5f)
        {
            if (_renderer == null) yield break;

            var originalColor = _renderer.material.color;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 10f, 1f);
                _renderer.material.color = Color.Lerp(originalColor, flashColor, t);
                yield return null;
            }
            _renderer.material.color = originalColor;
        }

        /// <summary>
        /// 缩放脉冲
        /// </summary>
        public IEnumerator IE_ScalePulse(float maxScale = 1.3f, float duration = 0.3f)
        {
            Vector3 originalScale = transform.localScale;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin(elapsed / duration * Mathf.PI);
                transform.localScale = originalScale * Mathf.Lerp(1f, maxScale, t);
                yield return null;
            }
            transform.localScale = originalScale;
        }
    }
}
