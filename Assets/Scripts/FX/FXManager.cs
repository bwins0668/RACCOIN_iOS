using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Effects;

namespace Raccoin.Core
{
    /// <summary>
    /// 通用对象池 - 复刻原版 ObjectPool
    /// 用于特效、硬币等频繁创建/销毁的对象
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private static ObjectPool _instance;
        public static ObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[ObjectPool]");
                    _instance = go.AddComponent<ObjectPool>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<string, Queue<GameObject>> _pools = new();
        private Dictionary<string, Transform> _poolParents = new();

        [SerializeField] private int _defaultPoolSize = 20;
        [SerializeField] private int _maxPoolSize = 200;

        /// <summary>
        /// 预热对象池
        /// </summary>
        public void WarmUpPool(string poolName, GameObject prefab, int count)
        {
            if (!_pools.ContainsKey(poolName))
            {
                _pools[poolName] = new Queue<GameObject>();
                var parent = new GameObject($"Pool_{poolName}");
                parent.transform.SetParent(transform);
                _poolParents[poolName] = parent.transform;
            }

            for (int i = _pools[poolName].Count; i < count; i++)
            {
                var obj = Instantiate(prefab, _poolParents[poolName]);
                obj.SetActive(false);
                _pools[poolName].Enqueue(obj);
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public GameObject Get(string poolName, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj;

            if (_pools.ContainsKey(poolName) && _pools[poolName].Count > 0)
            {
                obj = _pools[poolName].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            else
            {
                obj = Instantiate(prefab, position, rotation);
                obj.AddComponent<PooledObject>().PoolName = poolName;
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 从池中获取对象(无位置)
        /// </summary>
        public GameObject Get(string poolName, GameObject prefab)
        {
            return Get(poolName, prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// 归还对象到池
        /// </summary>
        public void Return(GameObject obj)
        {
            var pooled = obj.GetComponent<PooledObject>();
            if (pooled == null)
            {
                Destroy(obj);
                return;
            }

            string poolName = pooled.PoolName;
            if (!_pools.ContainsKey(poolName))
            {
                _pools[poolName] = new Queue<GameObject>();
                var parent = new GameObject($"Pool_{poolName}");
                parent.transform.SetParent(transform);
                _poolParents[poolName] = parent.transform;
            }

            if (_pools[poolName].Count >= _maxPoolSize)
            {
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(_poolParents[poolName]);
            _pools[poolName].Enqueue(obj);
        }

        /// <summary>
        /// 延迟归还
        /// </summary>
        public void ReturnDelayed(GameObject obj, float delay)
        {
            StartCoroutine(IE_ReturnDelayed(obj, delay));
        }

        private IEnumerator IE_ReturnDelayed(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null) Return(obj);
        }

        /// <summary>
        /// 清空指定池
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (_pools.ContainsKey(poolName))
            {
                while (_pools[poolName].Count > 0)
                {
                    var obj = _pools[poolName].Dequeue();
                    if (obj != null) Destroy(obj);
                }
            }
        }

        /// <summary>
        /// 清空所有池
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _pools)
            {
                while (pool.Value.Count > 0)
                {
                    var obj = pool.Value.Dequeue();
                    if (obj != null) Destroy(obj);
                }
            }
            _pools.Clear();
        }

        public int GetPoolCount(string poolName)
        {
            return _pools.ContainsKey(poolName) ? _pools[poolName].Count : 0;
        }
    }

    /// <summary>
    /// 池化对象标记组件
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public string PoolName { get; set; }
        public float AutoReturnTime { get; set; } = -1f;

        private void OnEnable()
        {
            if (AutoReturnTime > 0)
            {
                Invoke(nameof(ReturnToPool), AutoReturnTime);
            }
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void ReturnToPool()
        {
            ObjectPool.Instance.Return(gameObject);
        }
    }
}

namespace Raccoin.FX
{
    using Raccoin.Core;

    /// <summary>
    /// 特效管理器 - 复刻原版 FX_Manager
    /// 管理所有游戏特效的创建、池化和生命周期
    /// </summary>
    public class FX_Manager : MonoSingleton<FX_Manager>
    {
        [Header("FX Prefabs")]
        [SerializeField] private GameObject _coinDropFX;
        [SerializeField] private GameObject _coinCollectFX;
        [SerializeField] private GameObject _coinDestroyFX;
        [SerializeField] private GameObject _explosionFX;
        [SerializeField] private GameObject _electricFX;
        [SerializeField] private GameObject _fireFX;
        [SerializeField] private GameObject _iceFX;
        [SerializeField] private GameObject _healFX;
        [SerializeField] private GameObject _levelUpFX;
        [SerializeField] private GameObject _comboFX;

        [Header("Settings")]
        [SerializeField] private int _fxPoolSize = 30;
        [SerializeField] private float _defaultFXLifetime = 3f;

        private Dictionary<FXType, GameObject> _fxPrefabs = new();
        private bool _initialized;

        public enum FXType
        {
            None = 0,
            CoinDrop,
            CoinCollect,
            CoinDestroy,
            Explosion,
            Electric,
            Fire,
            Ice,
            Heal,
            LevelUp,
            Combo,
            // CET 特效
            EyeLaser,
            GiraffeNeck,
            Jupiter,
            Killer,
            Magnet,
            Rocket,
            SlotAlien,
            Tornado,
            Venus,
            // 通用
            Sparkle,
            Smoke,
            Bubble,
            Star,
            Ring,
            Shockwave
        }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // 注册默认特效预制体映射
            RegisterFXPrefab(FXType.CoinDrop, _coinDropFX);
            RegisterFXPrefab(FXType.CoinCollect, _coinCollectFX);
            RegisterFXPrefab(FXType.CoinDestroy, _coinDestroyFX);
            RegisterFXPrefab(FXType.Explosion, _explosionFX);
            RegisterFXPrefab(FXType.Electric, _electricFX);
            RegisterFXPrefab(FXType.Fire, _fireFX);
            RegisterFXPrefab(FXType.Ice, _iceFX);
            RegisterFXPrefab(FXType.Heal, _healFX);
            RegisterFXPrefab(FXType.LevelUp, _levelUpFX);
            RegisterFXPrefab(FXType.Combo, _comboFX);

            // 预热对象池
            foreach (var kvp in _fxPrefabs)
            {
                if (kvp.Value != null)
                {
                    ObjectPool.Instance.WarmUpPool($"FX_{kvp.Key}", kvp.Value, _fxPoolSize);
                }
            }
        }

        public void RegisterFXPrefab(FXType type, GameObject prefab)
        {
            if (prefab != null)
            {
                _fxPrefabs[type] = prefab;
            }
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        public GameObject PlayFX(FXType type, Vector3 position, Quaternion rotation = default)
        {
            if (!_fxPrefabs.ContainsKey(type) || _fxPrefabs[type] == null)
            {
                Debug.LogWarning($"[FX_Manager] No prefab for FX type: {type}");
                return null;
            }

            var fx = ObjectPool.Instance.Get($"FX_{type}", _fxPrefabs[type], position, rotation);
            if (fx != null)
            {
                var pooled = fx.GetComponent<PooledObject>();
                if (pooled != null) pooled.AutoReturnTime = _defaultFXLifetime;

                // 播放粒子系统
                var ps = fx.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
            return fx;
        }

        /// <summary>
        /// 播放特效(跟随目标)
        /// </summary>
        public GameObject PlayFXAttached(FXType type, Transform parent, Vector3 localPos = default)
        {
            var fx = PlayFX(type, parent.position, parent.rotation);
            if (fx != null)
            {
                fx.transform.SetParent(parent);
                fx.transform.localPosition = localPos;
            }
            return fx;
        }

        /// <summary>
        /// 停止特效
        /// </summary>
        public void StopFX(GameObject fx)
        {
            if (fx == null) return;
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            ObjectPool.Instance.Return(fx);
        }

        /// <summary>
        /// 延迟停止
        /// </summary>
        public void StopFXDelayed(GameObject fx, float delay)
        {
            if (fx == null) return;
            ObjectPool.Instance.ReturnDelayed(fx, delay);
        }

        /// <summary>
        /// 停止所有特效
        /// </summary>
        public void StopAllFX()
        {
            ObjectPool.Instance.ClearAll();
        }
    }

    /// <summary>
    /// 硬币通用特效 - 复刻原版 CommonCoinFX
    /// 附加在硬币上的通用特效组件
    /// </summary>
    public class CommonCoinFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _trailPS;
        [SerializeField] private ParticleSystem _glowPS;
        [SerializeField] private ParticleSystem _impactPS;
        [SerializeField] private Light _glowLight;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _goldenColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color _diamondColor = new Color(0.7f, 0.9f, 1f);
        [SerializeField] private Color _specialColor = new Color(1f, 0.4f, 0.7f);

        public void SetCoinTier(int tier)
        {
            Color color = tier switch
            {
                0 => _normalColor,
                1 => _goldenColor,
                2 => _diamondColor,
                _ => _specialColor
            };

            if (_glowLight != null) _glowLight.color = color;
            SetParticleColor(color);
        }

        public void SetParticleColor(Color color)
        {
            if (_trailPS != null)
            {
                var main = _trailPS.main;
                main.startColor = color;
            }
            if (_glowPS != null)
            {
                var main = _glowPS.main;
                main.startColor = color;
            }
        }

        public void PlayTrail() => _trailPS?.Play();
        public void StopTrail() => _trailPS?.Stop();
        public void PlayGlow() => _glowPS?.Play();
        public void StopGlow() => _glowPS?.Stop();
        public void PlayImpact() => _impactPS?.Play();

        public void EnableGlowLight(bool enable)
        {
            if (_glowLight != null) _glowLight.enabled = enable;
        }
    }

    // ===== CET_* 自定义特效组件系列 =====

    /// <summary>
    /// CET 特效基类 - Coin Effect Trigger
    /// </summary>
    public abstract class CET_Base : MonoBehaviour
    {
        [SerializeField] protected float _effectDuration = 3f;
        [SerializeField] protected float _effectRadius = 2f;
        [SerializeField] protected GameObject _effectPrefab;

        public bool IsActive { get; protected set; }
        protected GameObject _fxInstance;

        public virtual void Trigger(Vector3 position)
        {
            IsActive = true;
            _fxInstance = FX_Manager.Instance.PlayFX(GetFXType(), position);
            StartCoroutine(IE_EffectCoroutine(position));
        }

        protected abstract FX_Manager.FXType GetFXType();
        protected abstract IEnumerator IE_EffectCoroutine(Vector3 position);

        protected virtual void EndEffect()
        {
            IsActive = false;
            if (_fxInstance != null)
            {
                FX_Manager.Instance.StopFX(_fxInstance);
                _fxInstance = null;
            }
        }

        protected virtual void OnDestroy()
        {
            EndEffect();
        }
    }

    /// <summary>眼睛激光特效 - 从硬币射出激光</summary>
    public class CET_EyeLaser : CET_Base
    {
        [SerializeField] private float _laserDamage = 10f;
        [SerializeField] private float _laserRange = 10f;
        [SerializeField] private LineRenderer _laserLine;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.EyeLaser;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            float elapsed = 0;
            while (elapsed < _effectDuration)
            {
                elapsed += Time.deltaTime;
                // 激光扫射逻辑
                if (_laserLine != null)
                {
                    _laserLine.enabled = true;
                    _laserLine.SetPosition(0, transform.position);
                    Vector3 dir = Quaternion.Euler(0, elapsed * 180f, 0) * Vector3.forward;
                    _laserLine.SetPosition(1, transform.position + dir * _laserRange);
                }
                yield return null;
            }
            if (_laserLine != null) _laserLine.enabled = false;
            EndEffect();
        }
    }

    /// <summary>长颈鹿脖子特效 - 伸缩攻击</summary>
    public class CET_GiraffeNeck : CET_Base
    {
        [SerializeField] private float _extendSpeed = 5f;
        [SerializeField] private float _maxLength = 8f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.GiraffeNeck;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            // 伸出
            float length = 0;
            while (length < _maxLength)
            {
                length += _extendSpeed * Time.deltaTime;
                transform.localScale = new Vector3(1, length, 1);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            // 缩回
            while (length > 0)
            {
                length -= _extendSpeed * Time.deltaTime;
                transform.localScale = new Vector3(1, Mathf.Max(0.1f, length), 1);
                yield return null;
            }
            EndEffect();
        }
    }

    /// <summary>木星特效 - 巨大引力场</summary>
    public class CET_Jupiter : CET_Base
    {
        [SerializeField] private float _gravityStrength = 20f;
        [SerializeField] private float _pullRadius = 5f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.Jupiter;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            float elapsed = 0;
            while (elapsed < _effectDuration)
            {
                elapsed += Time.deltaTime;
                // 吸引附近硬币
                Collider[] hits = Physics.OverlapSphere(position, _pullRadius);
                foreach (var hit in hits)
                {
                    var rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (position - hit.transform.position).normalized;
                        rb.AddForce(dir * _gravityStrength * Time.deltaTime);
                    }
                }
                yield return null;
            }
            EndEffect();
        }
    }

    /// <summary>杀手特效 - 消灭附近硬币</summary>
    public class CET_Killer : CET_Base
    {
        [SerializeField] private int _killCount = 3;
        [SerializeField] private float _killInterval = 0.5f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.Killer;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            int killed = 0;
            while (killed < _killCount)
            {
                Collider[] hits = Physics.OverlapSphere(position, _effectRadius);
                foreach (var hit in hits)
                {
                    var coin = hit.GetComponent<CoinViewBase>();
                    if (coin != null)
                    {
                        coin.DestroyCoin();
                        killed++;
                        FX_Manager.Instance.PlayFX(FX_Manager.FXType.CoinDestroy, hit.transform.position);
                        break;
                    }
                }
                yield return new WaitForSeconds(_killInterval);
            }
            EndEffect();
        }
    }

    /// <summary>磁铁特效 - 吸引硬币</summary>
    public class CET_Magnet : CET_Base
    {
        [SerializeField] private float _attractForce = 15f;
        [SerializeField] private float _attractRadius = 4f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.Magnet;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            float elapsed = 0;
            while (elapsed < _effectDuration)
            {
                elapsed += Time.deltaTime;
                Collider[] hits = Physics.OverlapSphere(position, _attractRadius);
                foreach (var hit in hits)
                {
                    var rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (position - hit.transform.position).normalized;
                        float dist = Vector3.Distance(position, hit.transform.position);
                        float force = _attractForce * (1f - dist / _attractRadius);
                        rb.AddForce(dir * force * Time.deltaTime);
                    }
                }
                yield return null;
            }
            EndEffect();
        }
    }

    /// <summary>火箭特效 - 向上发射</summary>
    public class CET_Rocket : CET_Base
    {
        [SerializeField] private float _launchForce = 30f;
        [SerializeField] private float _explosionRadius = 3f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.Rocket;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            // 发射阶段
            float elapsed = 0;
            float launchDuration = 1f;
            Vector3 startPos = position;
            while (elapsed < launchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / launchDuration;
                transform.position = startPos + Vector3.up * (t * 10f);
                yield return null;
            }

            // 爆炸阶段
            FX_Manager.Instance.PlayFX(FX_Manager.FXType.Explosion, transform.position);
            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (var hit in hits)
            {
                var rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(_launchForce, transform.position, _explosionRadius);
                }
            }
            EndEffect();
        }
    }

    /// <summary>外星老虎机特效</summary>
    public class CET_SlotAlien : CET_Base
    {
        [SerializeField] private int _slotCount = 3;
        [SerializeField] private float _spinDuration = 2f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.SlotAlien;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            // 转盘动画
            float elapsed = 0;
            while (elapsed < _spinDuration)
            {
                elapsed += Time.deltaTime;
                transform.Rotate(Vector3.up, 720f * Time.deltaTime);
                yield return null;
            }

            // 结算奖励
            int result = Random.Range(0, _slotCount);
            if (result == 0)
            {
                // 大奖 - 生成额外硬币
                FX_Manager.Instance.PlayFX(FX_Manager.FXType.Combo, position);
            }
            EndEffect();
        }
    }

    /// <summary>龙卷风特效 - 卷起硬币</summary>
    public class CET_Tornado : CET_Base
    {
        [SerializeField] private float _rotateSpeed = 360f;
        [SerializeField] private float _liftForce = 10f;
        [SerializeField] private float _tornadoRadius = 3f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.Tornado;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            float elapsed = 0;
            while (elapsed < _effectDuration)
            {
                elapsed += Time.deltaTime;
                transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);

                Collider[] hits = Physics.OverlapSphere(position, _tornadoRadius);
                foreach (var hit in hits)
                {
                    var rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // 旋转力 + 上升力
                        Vector3 toCenter = (position - hit.transform.position).normalized;
                        Vector3 tangent = Vector3.Cross(Vector3.up, toCenter);
                        rb.AddForce(tangent * _rotateSpeed * 0.01f + Vector3.up * _liftForce * Time.deltaTime);
                    }
                }
                yield return null;
            }
            EndEffect();
        }
    }

    /// <summary>金星特效 - 腐蚀效果</summary>
    public class CET_Venus : CET_Base
    {
        [SerializeField] private float _corrosionRate = 0.1f;
        [SerializeField] private float _corrosionRadius = 2.5f;

        protected override FX_Manager.FXType GetFXType() => FX_Manager.FXType.Venus;

        protected override IEnumerator IE_EffectCoroutine(Vector3 position)
        {
            float elapsed = 0;
            while (elapsed < _effectDuration)
            {
                elapsed += Time.deltaTime;
                Collider[] hits = Physics.OverlapSphere(position, _corrosionRadius);
                foreach (var hit in hits)
                {
                    var coin = hit.GetComponent<CoinViewBase>();
                    if (coin != null)
                    {
                        // 逐渐缩小(腐蚀)
                        coin.transform.localScale *= (1f - _corrosionRate * Time.deltaTime);
                        if (coin.transform.localScale.magnitude < 0.1f)
                        {
                            coin.DestroyCoin();
                        }
                    }
                }
                yield return null;
            }
            EndEffect();
        }
    }
}
