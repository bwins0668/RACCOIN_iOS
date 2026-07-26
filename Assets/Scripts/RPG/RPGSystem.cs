using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Core;

namespace Raccoin.RPG
{
    /// <summary>
    /// RPG 玩家控制器 - 复刻原版 RPG_PlayerController
    /// </summary>
    public class RPG_PlayerController : MonoBehaviour
    {
        [SerializeField] private RPG_PlayerData _playerData;

        public RPG_PlayerData Data => _playerData;
        public bool IsAlive => _playerData != null && _playerData.HP > 0;

        public void Initialize(RPG_PlayerData data)
        {
            _playerData = data;
        }

        public IEnumerator IE_PlayerAttack(RPG_EnemyController target)
        {
            if (!IsAlive) yield break;

            int damage = CalculateDamage(_playerData.Attack, target.Data.Defense);
            target.TakeDamage(damage);

            // 攻击动画
            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator IE_PlayerDefend()
        {
            if (!IsAlive) yield break;

            _playerData.IsDefending = true;
            yield return new WaitForSeconds(0.3f);
        }

        public IEnumerator IE_PlayerHeal()
        {
            if (!IsAlive) yield break;

            int healAmount = _playerData.HealPower;
            _playerData.HP = Mathf.Min(_playerData.HP + healAmount, _playerData.MaxHP);

            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator IE_PlayerDeadDeal()
        {
            // 玩家死亡处理
            yield return new WaitForSeconds(1f);
        }

        public IEnumerator IE_WaitPlayerRevive()
        {
            // 等待复活
            yield return new WaitForSeconds(3f);
            _playerData.HP = _playerData.MaxHP / 2;
        }

        public void ResetDefense()
        {
            _playerData.IsDefending = false;
        }

        private int CalculateDamage(int attack, int defense)
        {
            int baseDamage = Mathf.Max(1, attack - defense / 2);
            return baseDamage + Random.Range(0, attack / 4);
        }
    }

    /// <summary>
    /// RPG 敌人控制器 - 复刻原版 RPG_EnemyController
    /// </summary>
    public class RPG_EnemyController : MonoBehaviour
    {
        [SerializeField] private RPG_EnemyData _enemyData;
        [SerializeField] private RPG_EnemyIntentIcon _intentIcon;

        public RPG_EnemyData Data => _enemyData;
        public bool IsAlive => _enemyData != null && _enemyData.HP > 0;
        public RPG_EnemyIntent CurrentIntent { get; private set; }

        public void Initialize(RPG_EnemyData data)
        {
            _enemyData = data;
            DecideNextIntent();
        }

        public void TakeDamage(int damage)
        {
            _enemyData.HP -= damage;
            if (_enemyData.HP <= 0)
            {
                _enemyData.HP = 0;
            }
        }

        public IEnumerator IE_EnemyAction(RPG_PlayerController player)
        {
            switch (CurrentIntent)
            {
                case RPG_EnemyIntent.Attack:
                    int damage = Mathf.Max(1, _enemyData.Attack - player.Data.Defense / 2);
                    if (player.Data.IsDefending) damage /= 2;
                    player.Data.HP -= damage;
                    break;

                case RPG_EnemyIntent.Defend:
                    _enemyData.IsDefending = true;
                    break;

                case RPG_EnemyIntent.Heal:
                    _enemyData.HP = Mathf.Min(_enemyData.HP + _enemyData.HealPower, _enemyData.MaxHP);
                    break;

                case RPG_EnemyIntent.Special:
                    // 特殊技能
                    break;
            }

            yield return new WaitForSeconds(0.5f);
            DecideNextIntent();
        }

        public IEnumerator IE_EnemyDeadDeal()
        {
            // 敌人死亡动画和奖励
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        public IEnumerator IE_EnemyRecreate()
        {
            // 重新生成敌人
            yield return null;
        }

        private void DecideNextIntent()
        {
            // 基于 AI 决定下一步行动
            float hpRatio = (float)_enemyData.HP / _enemyData.MaxHP;
            if (hpRatio < 0.3f && Random.value < 0.5f)
            {
                CurrentIntent = RPG_EnemyIntent.Heal;
            }
            else if (Random.value < 0.2f)
            {
                CurrentIntent = RPG_EnemyIntent.Defend;
            }
            else
            {
                CurrentIntent = RPG_EnemyIntent.Attack;
            }

            _intentIcon?.ShowIntent(CurrentIntent);
        }
    }

    /// <summary>
    /// RPG 计分板控制器 - 复刻原版 RPG_ScoreBoardController
    /// </summary>
    public class RPG_ScoreBoardController : MonoBehaviour
    {
        [SerializeField] private RPG_PlayerController _player;
        [SerializeField] private RPG_EnemyController _enemy;

        public long TotalDamageDealt { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public int CurrentWave { get; private set; }

        public IEnumerator ActionEndDeal()
        {
            // 行动结束处理
            yield return null;
        }

        public IEnumerator IE_Cheat_PlayerKill()
        {
            // 作弊：直接击杀玩家（调试用）
            yield return null;
        }

        public void RecordDamage(int damage)
        {
            TotalDamageDealt += damage;
        }

        public void RecordEnemyDefeated()
        {
            EnemiesDefeated++;
        }
    }

    /// <summary>
    /// RPG 攻击伤害文字 - 复刻原版 RPG_AttackDamageText
    /// </summary>
    public class RPG_AttackDamageText : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshPro _textMesh;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _lifetime = 1f;

        public void Show(int damage, bool isCritical = false)
        {
            if (_textMesh != null)
            {
                _textMesh.text = damage.ToString();
                _textMesh.color = isCritical ? Color.red : Color.white;
                _textMesh.fontSize = isCritical ? 4f : 3f;
            }
            StartCoroutine(FloatAndFade());
        }

        private IEnumerator FloatAndFade()
        {
            float elapsed = 0;
            Vector3 startPos = transform.position;

            while (elapsed < _lifetime)
            {
                elapsed += Time.deltaTime;
                transform.position = startPos + Vector3.up * (_floatSpeed * elapsed);

                if (_textMesh != null)
                {
                    var color = _textMesh.color;
                    color.a = 1f - (elapsed / _lifetime);
                    _textMesh.color = color;
                }
                yield return null;
            }

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// RPG 敌人意图图标 - 复刻原版 RPG_EnemyIntentIcon
    /// </summary>
    public class RPG_EnemyIntentIcon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _iconRenderer;
        [SerializeField] private Sprite _attackIcon;
        [SerializeField] private Sprite _defendIcon;
        [SerializeField] private Sprite _healIcon;
        [SerializeField] private Sprite _specialIcon;

        public void ShowIntent(RPG_EnemyIntent intent)
        {
            if (_iconRenderer == null) return;

            _iconRenderer.sprite = intent switch
            {
                RPG_EnemyIntent.Attack => _attackIcon,
                RPG_EnemyIntent.Defend => _defendIcon,
                RPG_EnemyIntent.Heal => _healIcon,
                RPG_EnemyIntent.Special => _specialIcon,
                _ => null
            };
        }
    }

    /// <summary>
    /// RPG 计分板硬币图标 - 复刻原版 RPG_ScoreboardCoinIcon
    /// </summary>
    public class RPG_ScoreboardCoinIcon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        public void SetCoinType(int typeId) { }
    }

    // ===== 数据类 =====

    /// <summary>
    /// RPG 单位数据基类 - 复刻原版 RPG_UnitData
    /// </summary>
    [System.Serializable]
    public class RPG_UnitData
    {
        public string UnitName;
        public int MaxHP = 100;
        public int HP = 100;
        public int Attack = 10;
        public int Defense = 5;
        public int HealPower = 15;
        public int Speed = 5;
        public bool IsDefending;
    }

    /// <summary>
    /// RPG 玩家数据 - 复刻原版 RPG_PlayerData
    /// </summary>
    [System.Serializable]
    public class RPG_PlayerData : RPG_UnitData
    {
        public int Level = 1;
        public int Experience;
        public int CoinsCollected;
        public List<string> Skills = new();
    }

    /// <summary>
    /// RPG 敌人数据 - 复刻原版 RPG_EnemyData
    /// </summary>
    [System.Serializable]
    public class RPG_EnemyData : RPG_UnitData
    {
        public int EnemyTypeId;
        public int RewardCoins;
        public int RewardExp;
        public float Aggressiveness = 0.7f;
    }

    public enum RPG_EnemyIntent
    {
        None = 0,
        Attack = 1,
        Defend = 2,
        Heal = 3,
        Special = 4
    }
}
