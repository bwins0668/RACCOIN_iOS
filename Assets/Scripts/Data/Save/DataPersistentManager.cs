using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace Raccoin.Core
{
    /// <summary>
    /// 数据持久化管理器 - 复刻原版 DataPersistentManager
    /// </summary>
    public class DataPersistentManager : Singleton<DataPersistentManager>
    {
        private FileDataHandler _dataHandler;
        private CommonSaveData _commonSaveData;
        private CommonSettingData _settingData;
        private ProfileData _profileData;

        public CommonSaveData CommonSave => _commonSaveData;
        public CommonSettingData Settings => _settingData;
        public ProfileData Profile => _profileData;

        public void Initialize()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "saves");
            _dataHandler = new FileDataHandler(savePath);
            LoadAll();
        }

        public void LoadAll()
        {
            _commonSaveData = _dataHandler.Load<CommonSaveData>("common_save") ?? new CommonSaveData();
            _settingData = _dataHandler.Load<CommonSettingData>("common_setting") ?? new CommonSettingData();
            _profileData = _dataHandler.Load<ProfileData>("profile") ?? new ProfileData();
        }

        public void SaveAll()
        {
            _dataHandler.Save("common_save", _commonSaveData);
            _dataHandler.Save("common_setting", _settingData);
            _dataHandler.Save("profile", _profileData);
        }

        public void DeleteAll()
        {
            _dataHandler.Delete("common_save");
            _dataHandler.Delete("common_setting");
            _dataHandler.Delete("profile");
            _commonSaveData = new CommonSaveData();
            _settingData = new CommonSettingData();
            _profileData = new ProfileData();
        }
    }

    /// <summary>
    /// 文件数据处理器 - 复刻原版 FileDataHandler
    /// iOS 使用 Application.persistentDataPath 沙盒目录
    /// </summary>
    public class FileDataHandler
    {
        private readonly string _saveDir;

        public FileDataHandler(string saveDir)
        {
            _saveDir = saveDir;
            if (!Directory.Exists(_saveDir))
            {
                Directory.CreateDirectory(_saveDir);
            }
        }

        public T Load<T>(string fileName) where T : class
        {
            string path = Path.Combine(_saveDir, $"{fileName}.json");
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileDataHandler] Load failed: {fileName}, {e.Message}");
                return null;
            }
        }

        public void Save<T>(string fileName, T data) where T : class
        {
            string path = Path.Combine(_saveDir, $"{fileName}.json");
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileDataHandler] Save failed: {fileName}, {e.Message}");
            }
        }

        public void Delete(string fileName)
        {
            string path = Path.Combine(_saveDir, $"{fileName}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 通用存档数据 - 复刻原版 CommonSaveData
    /// </summary>
    [Serializable]
    public class CommonSaveData
    {
        public int TotalPlayTimeSeconds;
        public long TotalCoinsEarned;
        public int TotalGamesPlayed;
        public int HighestRound;
        public long HighestScore;
        public List<string> UnlockedCharacters = new();
        public List<string> UnlockedSkins = new();
        public List<string> UnlockedCoins = new();
        public List<string> UnlockedChips = new();
        public List<string> UnlockedPrizes = new();
        public Dictionary<string, int> CharacterUsageCount = new();
        public int LuckyWheelFreeSpinsRemaining = 1;
        public long LastLoginTimestamp;
    }

    /// <summary>
    /// 通用设置数据 - 复刻原版 CommonSettingData
    /// </summary>
    [Serializable]
    public class CommonSettingData
    {
        public float MasterVolume = 1.0f;
        public float SFXVolume = 1.0f;
        public float MusicVolume = 1.0f;
        public int Language = 0;
        public int QualityLevel = 2;
        public bool VibrationEnabled = true;
        public bool ShowFPS = false;
        public int TargetFPS = 60;
    }

    /// <summary>
    /// 玩家档案数据 - 复刻原版 ProfileData
    /// </summary>
    [Serializable]
    public class ProfileData
    {
        public string PlayerName = "Player";
        public string SelectedCharacter = "raccoon_default";
        public string SelectedMachineSkin = "machine_default";
        public ProfileSkinSetting SkinSetting = new();
        public ProfileWheelSetting WheelSetting = new();
        public List<ProfileHistoryGameRecord> HistoryGames = new();
        public List<ActionBestRecord> BestRecords = new();
        public List<ActionCountRecord> CountRecords = new();
    }

    [Serializable]
    public class ProfileSkinSetting
    {
        public string MachineSkin = "default";
        public CustomDecoSetting DecoSetting = new();
    }

    [Serializable]
    public class CustomDecoSetting
    {
        public int ColorIndex;
        public List<int> DecoSlots = new();
    }

    [Serializable]
    public class ProfileWheelSetting
    {
        public List<string> WheelParts = new();
    }

    [Serializable]
    public class ProfileHistoryGameRecord
    {
        public long Timestamp;
        public int Round;
        public long Score;
        public string Character;
        public int GameMode;
    }

    [Serializable]
    public class ActionBestRecord
    {
        public int RecordType;
        public long Value;
    }

    [Serializable]
    public class ActionCountRecord
    {
        public int RecordType;
        public int Count;
    }
}
