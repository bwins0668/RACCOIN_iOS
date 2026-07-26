using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Raccoin.Effects
{
    /// <summary>
    /// 硬币效果基类 - 复刻原版 CoinEffectBase
    /// 所有 CE_* 类的父类
    /// </summary>
    public abstract class CoinEffectBase : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected float _effectDuration = 5f;
        [SerializeField] protected float _effectRadius = 2f;
        [SerializeField] protected int _effectPriority = 0;

        public int CoinId { get; protected set; }
        public bool IsActive { get; protected set; }
        public float RemainingTime { get; protected set; }

        protected CoinViewBase CoinView { get; private set; }

        public virtual void Initialize(int coinId, CoinViewBase coinView)
        {
            CoinId = coinId;
            CoinView = coinView;
            RemainingTime = _effectDuration;
        }

        public virtual void OnSpawn()
        {
            IsActive = true;
            OnSpawnEffect();
        }

        public virtual void OnSettle()
        {
            OnSettleEffect();
        }

        public virtual void OnDestroy_Coin()
        {
            IsActive = false;
            OnDestroyEffect();
        }

        protected virtual void OnSpawnEffect() { }
        protected virtual void OnSettleEffect() { }
        protected virtual void OnDestroyEffect() { }

        protected virtual void Update()
        {
            if (!IsActive) return;
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0 && _effectDuration > 0)
            {
                OnExpire();
            }
        }

        protected virtual void OnExpire()
        {
            IsActive = false;
        }

        /// <summary>
        /// 出生时更新缩放动画
        /// </summary>
        protected IEnumerator BornUpdateScale(float targetScale, float duration = 0.3f)
        {
            float elapsed = 0;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
            transform.localScale = endScale;
        }
    }

    /// <summary>
    /// 硬币效果修改器基类 - 复刻原版 CoinEffectMod
    /// </summary>
    public abstract class CoinEffectMod : CoinEffectBase
    {
        protected List<CoinEffectBase> AffectedCoins { get; } = new();

        public virtual void ApplyTo(CoinEffectBase target)
        {
            AffectedCoins.Add(target);
        }

        public virtual void RemoveFrom(CoinEffectBase target)
        {
            AffectedCoins.Remove(target);
        }
    }

    /// <summary>
    /// 硬币视图基类 - 复刻原版 CoinViewBase
    /// </summary>
    public class CoinViewBase : MonoBehaviour
    {
        [SerializeField] protected MeshRenderer _meshRenderer;
        [SerializeField] protected Collider _collider;

        public int CoinId { get; set; }
        public int CoinTypeId { get; set; }
        public long PointValue { get; set; }
        public bool IsSettled { get; set; }
        public CoinEffectBase CurrentEffect { get; set; }

        public virtual void Initialize(int coinId, int typeId, long pointValue)
        {
            CoinId = coinId;
            CoinTypeId = typeId;
            PointValue = pointValue;
        }

        public virtual void Settle()
        {
            IsSettled = true;
            CurrentEffect?.OnSettle();
        }

        public virtual void DestroyCoin()
        {
            CurrentEffect?.OnDestroy_Coin();
            Destroy(gameObject);
        }
    }

    // ===== 枚举定义 =====

    /// <summary>
    /// 硬币效果类型枚举
    /// </summary>
    public enum CoinEffectType
    {
        None = 0,
        Basic,
        Gold,
        Diamond,
        Ruby,
        // 动物
        Fox, Frog, Rabbit, Hen, Lion, Tiger, Wolf, Monkey, Dog, Pigeon, Rat, Rooster,
        // 天文
        Sun, Moon, Mars, Jupiter, Saturn, Neptune, Uranus, Venus, Pluto, Earth,
        // 食物
        Rice, Sushi, Pizza, Onigiri, Omurice, EggWaffle,
        // 机械
        Gear, Bolt, Nut, WindingKey, DrillBit, Clock,
        // 元素
        FireBall, Ice, Water, Electricity, Tornado, Quake,
        // 数学
        Multiply, Division, Factorial, Sum, Percent, Equal,
        // RPG
        RPG_Attack, RPG_Defend, RPG_Heal, RPG_Doom,
        // 外星
        BagAlien, BallAlien, SlotAlien
    }

    /// <summary>
    /// 硬币板效果类型枚举 (17种)
    /// </summary>
    public enum CoinPlateType
    {
        None = 0,
        Bigger,
        Demon,
        Electric,
        ExtraSettle,
        Forever,
        Fungus,
        Golden,
        GunPowder,
        Ice,
        Return,
        SleepWalk,
        Smaller,
        Source,
        Swallow,
        Time,
        Zero,
        Zombie
    }

    // ===== 接口定义 =====

    public interface ICoinBeAttract
    {
        void BeAttracted(Vector3 direction, float force);
    }

    public interface ICoinBeShit
    {
        void BeShit();
    }

    public interface ICoinBeWater
    {
        void BeWatered();
    }

    public interface ICoinBeCook
    {
        void BeCooked();
    }

    public interface ICoinAstro
    {
        void AstroFly(Vector3 target);
    }
}
