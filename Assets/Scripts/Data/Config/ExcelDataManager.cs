using UnityEngine;
using System;
using System.Collections.Generic;

namespace Raccoin.Data
{
    /// <summary>
    /// Excel 数据项基类 - 复刻原版 ExcelItemBase
    /// </summary>
    [Serializable]
    public abstract class ExcelItemBase
    {
        public string Id;
        public string Name;
        public abstract void ParseFromJson(string json);
    }

    /// <summary>
    /// Excel 数据管理器 - 复刻原版 ExcelDataManager
    /// 使用 ScriptableObject/JSON 替代原版 Excel 读取
    /// </summary>
    public class ExcelDataManager : Core.Singleton<ExcelDataManager>
    {
        private Dictionary<Type, object> _dataTables = new();

        public void Initialize()
        {
            LoadAllTables();
        }

        private void LoadAllTables()
        {
            _dataTables[typeof(CoinExcelData)] = LoadTable<CoinExcelItem>("Config/Coins");
            _dataTables[typeof(ChipExcelData)] = LoadTable<ChipExcelItem>("Config/Chips");
            _dataTables[typeof(CharacterExcelData)] = LoadTable<CharacterExcelItem>("Config/Characters");
            _dataTables[typeof(GameModeExcelData)] = LoadTable<GameModeExcelItem>("Config/GameModes");
            _dataTables[typeof(ChallengeExcelData)] = LoadTable<ChallengeExcelItem>("Config/Challenges");
            _dataTables[typeof(DifficultyExcelData)] = LoadTable<DifficultyExcelItem>("Config/Difficulty");
            _dataTables[typeof(GiftExcelData)] = LoadTable<GiftExcelItem>("Config/Gifts");
            _dataTables[typeof(PrizeExcelData)] = LoadTable<PrizeExcelItem>("Config/Prizes");
            _dataTables[typeof(LuckyWheelExcelData)] = LoadTable<LuckyWheelExcelItem>("Config/LuckyWheel");
            _dataTables[typeof(MilestoneExcelData)] = LoadTable<MilestoneExcelItem>("Config/Milestones");
            _dataTables[typeof(RoundExcelData)] = LoadTable<RoundExcelItem>("Config/Rounds");
            _dataTables[typeof(SkinExcelData)] = LoadTable<SkinExcelItem>("Config/Skins");
            _dataTables[typeof(TutorialExcelData)] = LoadTable<TutorialExcelItem>("Config/Tutorial");
            _dataTables[typeof(CoinBuffExcelData)] = LoadTable<CoinBuffExcelItem>("Config/CoinBuffs");
            _dataTables[typeof(CoinPlateExcelData)] = LoadTable<CoinPlateExcelItem>("Config/CoinPlates");
            _dataTables[typeof(GadgetExcelData)] = LoadTable<GadgetExcelItem>("Config/Gadgets");
            _dataTables[typeof(RobotSkillExcelData)] = LoadTable<RobotSkillExcelItem>("Config/RobotSkills");
            _dataTables[typeof(KeychainExcelData)] = LoadTable<KeychainExcelItem>("Config/Keychains");
            _dataTables[typeof(MarkExcelData)] = LoadTable<MarkExcelItem>("Config/Marks");
            _dataTables[typeof(FXExcelData)] = LoadTable<FXExcelItem>("Config/FX");
            _dataTables[typeof(AudioExcelData)] = LoadTable<AudioExcelItem>("Config/Audio");
            _dataTables[typeof(CardExcelData)] = LoadTable<CardExcelItem>("Config/Cards");
            _dataTables[typeof(CookExcelData)] = LoadTable<CookExcelItem>("Config/Cook");
            _dataTables[typeof(DoomExcelData)] = LoadTable<DoomExcelItem>("Config/Doom");
            _dataTables[typeof(GameplayBuffExcelData)] = LoadTable<GameplayBuffExcelItem>("Config/GameplayBuffs");
            _dataTables[typeof(PlatformExcelData)] = LoadTable<PlatformExcelItem>("Config/Platforms");
            _dataTables[typeof(QueueEventExcelData)] = LoadTable<QueueEventExcelItem>("Config/QueueEvents");
        }

        private List<T> LoadTable<T>(string resourcePath) where T : ExcelItemBase
        {
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogWarning($"[ExcelDataManager] Config not found: {resourcePath}");
                return new List<T>();
            }
            var wrapper = JsonUtility.FromJson<TableWrapper<T>>(textAsset.text);
            return wrapper?.Items ?? new List<T>();
        }

        public List<T> GetTable<T>() where T : ExcelItemBase
        {
            if (_dataTables.TryGetValue(typeof(T), out var table))
            {
                return table as List<T>;
            }
            return new List<T>();
        }

        [Serializable]
        private class TableWrapper<T>
        {
            public List<T> Items = new();
        }
    }

    // ===== 配置表数据定义 =====

    [Serializable]
    public class CoinExcelItem : ExcelItemBase
    {
        public CoinType CoinType;
        public CommonRarity Rarity;
        public int BaseValue;
        public float Weight;
        public float Scale;
        public string EffectClass;
        public string PrefabPath;
        public string IconPath;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class CoinExcelData { }

    [Serializable]
    public class ChipExcelItem : ExcelItemBase
    {
        public ChipType Type;
        public ChipConditionType ConditionType;
        public ChipModifyType ModifyType;
        public float Value;
        public CommonRarity Rarity;
        public string DescKey;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class ChipExcelData { }

    [Serializable]
    public class CharacterExcelItem : ExcelItemBase
    {
        public CharacterWeight Weight;
        public string SkillId;
        public string ModelPath;
        public string PortraitPath;
        public int UnlockCondition;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class CharacterExcelData { }

    [Serializable]
    public class GameModeExcelItem : ExcelItemBase
    {
        public int ModeType;
        public int MaxRound;
        public float DifficultyMultiplier;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class GameModeExcelData { }

    [Serializable]
    public class ChallengeExcelItem : ExcelItemBase
    {
        public ChallengeType Type;
        public ChallengeEffectType EffectType;
        public float EffectValue;
        public int TargetScore;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class ChallengeExcelData { }

    [Serializable]
    public class DifficultyExcelItem : ExcelItemBase
    {
        public float CoinSpawnRate;
        public float EnemyHpMultiplier;
        public float ScoreMultiplier;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class DifficultyExcelData { }

    [Serializable]
    public class GiftExcelItem : ExcelItemBase
    {
        public GiftType Type;
        public int Amount;
        public float Probability;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class GiftExcelData { }

    [Serializable]
    public class PrizeExcelItem : ExcelItemBase
    {
        public PrizeType Type;
        public int Value;
        public CommonRarity Rarity;
        public string ModelPath;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class PrizeExcelData { }

    [Serializable]
    public class LuckyWheelExcelItem : ExcelItemBase
    {
        public LuckyWheelRewardType RewardType;
        public LuckyWheelWeight Weight;
        public int Amount;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class LuckyWheelExcelData { }

    [Serializable]
    public class MilestoneExcelItem : ExcelItemBase
    {
        public MilestoneRewardType RewardType;
        public long RequiredScore;
        public int RewardAmount;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class MilestoneExcelData { }

    [Serializable]
    public class RoundExcelItem : ExcelItemBase
    {
        public int RoundNumber;
        public int CoinBudget;
        public float TimeLimit;
        public string SpecialEventId;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class RoundExcelData { }

    [Serializable]
    public class SkinExcelItem : ExcelItemBase
    {
        public string SkinPath;
        public int UnlockType;
        public int Price;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class SkinExcelData { }

    [Serializable]
    public class TutorialExcelItem : ExcelItemBase
    {
        public int StepIndex;
        public string DialogueKey;
        public string HighlightTarget;
        public int WaitAction;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class TutorialExcelData { }

    [Serializable]
    public class CoinBuffExcelItem : ExcelItemBase
    {
        public CoinBuffAttribute Attribute;
        public BuffRefreshType RefreshType;
        public BuffCostType CostType;
        public float Duration;
        public float Value;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class CoinBuffExcelData { }

    [Serializable]
    public class CoinPlateExcelItem : ExcelItemBase
    {
        public string PlateEffectClass;
        public float Duration;
        public CommonRarity Rarity;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class CoinPlateExcelData { }

    [Serializable]
    public class GadgetExcelItem : ExcelItemBase
    {
        public string PrefabPath;
        public float SpawnWeight;
        public int MaxCount;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class GadgetExcelData { }

    [Serializable]
    public class RobotSkillExcelItem : ExcelItemBase
    {
        public string SkillClass;
        public float Cooldown;
        public float Value;
        public int Level;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class RobotSkillExcelData { }

    [Serializable]
    public class KeychainExcelItem : ExcelItemBase
    {
        public KeychainType Type;
        public float EffectValue;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class KeychainExcelData { }

    [Serializable]
    public class MarkExcelItem : ExcelItemBase
    {
        public MarkType Type;
        public float Value;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class MarkExcelData { }

    [Serializable]
    public class FXExcelItem : ExcelItemBase
    {
        public string PrefabPath;
        public float Lifetime;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class FXExcelData { }

    [Serializable]
    public class AudioExcelItem : ExcelItemBase
    {
        public string WwiseEventName;
        public AudioName AudioName;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class AudioExcelData { }

    [Serializable]
    public class CardExcelItem : ExcelItemBase
    {
        public int CardLevel;
        public float EffectValue;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class CardExcelData { }

    [Serializable]
    public class CookExcelItem : ExcelItemBase
    {
        public string RecipeId;
        public int CookTime;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class CookExcelData { }

    [Serializable]
    public class DoomExcelItem : ExcelItemBase
    {
        public float DoomRate;
        public int TowerHeight;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class DoomExcelData { }

    [Serializable]
    public class GameplayBuffExcelItem : ExcelItemBase
    {
        public BuffableAttributeEffectType EffectType;
        public float Value;
        public float Duration;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class GameplayBuffExcelData { }

    [Serializable]
    public class PlatformExcelItem : ExcelItemBase
    {
        public float Width;
        public float Speed;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class PlatformExcelData { }

    [Serializable]
    public class QueueEventExcelItem : ExcelItemBase
    {
        public IncidentType EventType;
        public float TriggerProbability;
        public int MinRound;
        public override void ParseFromJson(string json) { JsonUtility.FromJsonOverwrite(json, this); }
    }
    [Serializable] public class QueueEventExcelData { }

    // ===== 枚举定义 =====
    public enum CoinType { Normal = 0, Special = 1, Gift = 2, Doom = 3, RPG = 4 }
    public enum CommonRarity { Common = 0, Uncommon = 1, Rare = 2, Epic = 3, Legendary = 4 }
    public enum ChipType { Passive = 0, Active = 1, Trigger = 2 }
    public enum ChipConditionType { None = 0, OnSpawn = 1, OnSettle = 2, OnDestroy = 3, OnRound = 4 }
    public enum ChipModifyType { Add = 0, Multiply = 1, Override = 2 }
    public enum CharacterWeight { Light = 0, Medium = 1, Heavy = 2 }
    public enum ChallengeType { Score = 0, Survival = 1, Speed = 2, Special = 3 }
    public enum ChallengeEffectType { None = 0, Modifier = 1, Restriction = 2 }
    public enum GiftType { Coin = 0, Ticket = 1, Item = 2, Character = 3 }
    public enum PrizeType { Ball = 0, Figure = 1, Ticket = 2 }
    public enum LuckyWheelRewardType { Coin = 0, Ticket = 1, Item = 2, Character = 3, Skin = 4 }
    public enum LuckyWheelWeight { Low = 0, Medium = 1, High = 2 }
    public enum MilestoneRewardType { Coin = 0, Ticket = 1, Unlock = 2 }
    public enum CoinBuffAttribute { Speed = 0, Value = 1, Size = 2, Luck = 3 }
    public enum BuffRefreshType { Permanent = 0, PerRound = 1, Timed = 2 }
    public enum BuffCostType { Free = 0, Coin = 1, Ticket = 2 }
    public enum KeychainType { Score = 0, Coin = 1, Luck = 2, Special = 3 }
    public enum MarkType { Gold = 0, Silver = 1, Bronze = 2 }
    public enum AudioName { CoinDrop = 0, CoinSettle = 1, PusherMove = 2, UIClick = 3, RoundStart = 4, RoundEnd = 5 }
    public enum IncidentType { None = 0, CoinRain = 1, Earthquake = 2, Bonus = 3, Doom = 4 }
    public enum BuffableAttributeEffectType { Add = 0, Multiply = 1, Set = 2 }
}
