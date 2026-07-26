using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Raccoin.Effects
{
    /// <summary>
    /// 硬币板效果基类 - 复刻原版 CoinPlateBase
    /// 硬币板是放置在推币机平台上的特殊效果板
    /// </summary>
    public abstract class CoinPlateBase : MonoBehaviour
    {
        [Header("Plate Settings")]
        [SerializeField] protected float _plateDuration = 10f;
        [SerializeField] protected float _plateRadius = 1.5f;
        [SerializeField] protected int _maxTriggerCount = -1; // -1 = infinite

        public int PlateId { get; protected set; }
        public bool IsActive { get; protected set; }
        public float RemainingTime { get; protected set; }
        public int TriggerCount { get; protected set; }

        protected List<CoinViewBase> _affectedCoins = new();

        public virtual void Initialize(int plateId)
        {
            PlateId = plateId;
            RemainingTime = _plateDuration;
            IsActive = true;
            TriggerCount = 0;
        }

        protected virtual void Update()
        {
            if (!IsActive) return;
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0 && _plateDuration > 0)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// 当硬币经过板上方时触发
        /// </summary>
        public virtual void OnCoinPassOver(CoinViewBase coin)
        {
            if (!IsActive) return;
            if (_maxTriggerCount >= 0 && TriggerCount >= _maxTriggerCount)
            {
                Deactivate();
                return;
            }
            TriggerCount++;
            ApplyPlateEffect(coin);
        }

        protected abstract void ApplyPlateEffect(CoinViewBase coin);

        public virtual void Deactivate()
        {
            IsActive = false;
            OnDeactivate();
        }

        protected virtual void OnDeactivate() { }

        protected IEnumerator BornUpdateScale(float targetScale, float duration = 0.3f)
        {
            float elapsed = 0;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
                yield return null;
            }
            transform.localScale = endScale;
        }
    }

    // ===== 17 种硬币板效果 =====

    /// <summary>变大板 - 经过的硬币变大</summary>
    public class CP_Bigger : CoinPlateBase
    {
        [SerializeField] private float _scaleMultiplier = 1.5f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            coin.transform.localScale *= _scaleMultiplier;
        }

        protected override void OnDeactivate() { }
    }

    /// <summary>恶魔板 - 随机销毁硬币</summary>
    public class CP_Demon : CoinPlateBase
    {
        [SerializeField] private float _destroyChance = 0.2f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            if (Random.value < _destroyChance)
            {
                coin.DestroyCoin();
            }
        }
    }

    /// <summary>电击板 - 电击附近硬币</summary>
    public class CP_Electric : CoinPlateBase
    {
        [SerializeField] private float _chainRadius = 2f;
        [SerializeField] private int _chainCount = 3;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            // 链式电击效果
            Collider[] hits = Physics.OverlapSphere(transform.position, _chainRadius);
            int chained = 0;
            foreach (var hit in hits)
            {
                if (chained >= _chainCount) break;
                var otherCoin = hit.GetComponent<CoinViewBase>();
                if (otherCoin != null && otherCoin != coin)
                {
                    chained++;
                    // 电击伤害/效果
                }
            }
        }
    }

    /// <summary>额外结算板 - 硬币经过时额外获得分数</summary>
    public class CP_ExtraSettle : CoinPlateBase
    {
        [SerializeField] private long _bonusPoints = 10;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            coin.PointValue += _bonusPoints;
        }
    }

    /// <summary>永恒板 - 永不过期</summary>
    public class CP_Forever : CoinPlateBase
    {
        public CP_Forever()
        {
            _plateDuration = 0; // 0 = 永不过期
        }

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            // 永恒板：给硬币添加永久增益
        }
    }

    /// <summary>真菌板 - 感染附近硬币</summary>
    public class CP_Fungus : CoinPlateBase
    {
        [SerializeField] private float _spreadRadius = 1.5f;
        [SerializeField] private float _spreadDelay = 2f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            StartCoroutine(SpreadFungus());
        }

        private IEnumerator SpreadFungus()
        {
            yield return new WaitForSeconds(_spreadDelay);
            Collider[] hits = Physics.OverlapSphere(transform.position, _spreadRadius);
            foreach (var hit in hits)
            {
                var otherCoin = hit.GetComponent<CoinViewBase>();
                if (otherCoin != null)
                {
                    // 感染效果
                }
            }
        }
    }

    /// <summary>黄金板 - 将硬币变为金色</summary>
    public class CP_Golden : CoinPlateBase
    {
        [SerializeField] private float _valueMultiplier = 3f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            coin.PointValue = (long)(coin.PointValue * _valueMultiplier);
        }
    }

    /// <summary>火药板 - 延迟爆炸</summary>
    public class CP_GunPowder : CoinPlateBase
    {
        [SerializeField] private float _explosionDelay = 3f;
        [SerializeField] private float _explosionRadius = 3f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            StartCoroutine(IE_WaitBoom());
        }

        private IEnumerator IE_WaitBoom()
        {
            yield return new WaitForSeconds(_explosionDelay);
            // 爆炸效果
            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (var hit in hits)
            {
                var otherCoin = hit.GetComponent<CoinViewBase>();
                if (otherCoin != null)
                {
                    otherCoin.GetComponent<Rigidbody>()?.AddExplosionForce(10f, transform.position, _explosionRadius);
                }
            }
            Deactivate();
        }
    }

    /// <summary>冰冻板 - 冻结硬币</summary>
    public class CP_Ice : CoinPlateBase
    {
        [SerializeField] private float _freezeDuration = 5f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            var rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                StartCoroutine(Unfreeze(rb, _freezeDuration));
            }
        }

        private IEnumerator Unfreeze(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rb != null) rb.isKinematic = false;
        }
    }

    /// <summary>返回板 - 将硬币弹回</summary>
    public class CP_Return : CoinPlateBase
    {
        [SerializeField] private float _returnForce = 5f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            var rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(-transform.forward * _returnForce, ForceMode.Impulse);
            }
        }
    }

    /// <summary>梦游板 - 硬币随机移动</summary>
    public class CP_SleepWalk : CoinPlateBase
    {
        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            var rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = Random.onUnitSphere;
                randomDir.y = 0;
                rb.AddForce(randomDir * 2f, ForceMode.Impulse);
            }
        }
    }

    /// <summary>缩小板 - 经过的硬币变小</summary>
    public class CP_Smaller : CoinPlateBase
    {
        [SerializeField] private float _scaleMultiplier = 0.6f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            coin.transform.localScale *= _scaleMultiplier;
        }
    }

    /// <summary>源头板 - 持续生成硬币</summary>
    public class CP_Source : CoinPlateBase
    {
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private int _maxSpawns = 5;
        private int _spawned;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            if (_spawned < _maxSpawns)
            {
                _spawned++;
                // 生成新硬币
            }
        }
    }

    /// <summary>吞噬板 - 吞噬硬币并变大</summary>
    public class CP_Swallow : CoinPlateBase
    {
        [SerializeField] private float _growPerCoin = 0.1f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            StartCoroutine(IE_Eat(coin));
        }

        private IEnumerator IE_Eat(CoinViewBase coin)
        {
            // 吞噬动画
            coin.transform.localScale = Vector3.Lerp(coin.transform.localScale, Vector3.zero, 0.5f);
            yield return new WaitForSeconds(0.3f);
            coin.DestroyCoin();
            transform.localScale += Vector3.one * _growPerCoin;
        }
    }

    /// <summary>时间板 - 减缓附近硬币速度</summary>
    public class CP_Time : CoinPlateBase
    {
        [SerializeField] private float _slowFactor = 0.3f;
        [SerializeField] private float _effectRadius = 2f;

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            var rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity *= _slowFactor;
            }
        }
    }

    /// <summary>归零板 - 将硬币价值归零</summary>
    public class CP_Zero : CoinPlateBase
    {
        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            coin.PointValue = 0;
        }
    }

    /// <summary>僵尸板 - 被销毁的硬币会复活</summary>
    public class CP_Zombie : CoinPlateBase
    {
        [SerializeField] private float _reviveDelay = 3f;
        private Queue<Vector3> _deadPositions = new();

        protected override void ApplyPlateEffect(CoinViewBase coin)
        {
            _deadPositions.Enqueue(coin.transform.position);
            StartCoroutine(ReviveCoin());
        }

        private IEnumerator ReviveCoin()
        {
            yield return new WaitForSeconds(_reviveDelay);
            if (_deadPositions.Count > 0)
            {
                Vector3 pos = _deadPositions.Dequeue();
                // 复活硬币
            }
        }
    }
}
