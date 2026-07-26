using NUnit.Framework;
using UnityEngine;

namespace Raccoin.Tests
{
    /// <summary>
    /// 核心系统单元测试
    /// </summary>
    [TestFixture]
    public class CoreSystemTests
    {
        [Test]
        public void Singleton_InstanceCreation_Works()
        {
            // 测试纯 C# 单例模式
            var instance = Core.SettingManager.Instance;
            Assert.IsNotNull(instance);
            Assert.AreSame(instance, Core.SettingManager.Instance);
        }

        [Test]
        public void GameTimeManager_DefaultTimeScale_IsOne()
        {
            var timeManager = Core.GameTimeManager.Instance;
            Assert.IsNotNull(timeManager);
        }

        [Test]
        public void PerformanceManager_InstanceCreation_Works()
        {
            var perfManager = Core.PerformanceManager.Instance;
            Assert.IsNotNull(perfManager);
        }
    }

    /// <summary>
    /// 数据系统测试
    /// </summary>
    [TestFixture]
    public class DataSystemTests
    {
        [Test]
        public void ExcelDataManager_InstanceCreation_Works()
        {
            var manager = Data.ExcelDataManager.Instance;
            Assert.IsNotNull(manager);
        }

        [Test]
        public void DataPersistentManager_InstanceCreation_Works()
        {
            var manager = Core.DataPersistentManager.Instance;
            Assert.IsNotNull(manager);
        }

        [Test]
        public void SaveDefine_Constants_AreCorrect()
        {
            Assert.AreEqual("saves", SaveDefine.SAVE_FOLDER);
            Assert.AreEqual(3, SaveDefine.MAX_SAVE_SLOTS);
        }

        [Test]
        public void RuleDefine_Constants_AreCorrect()
        {
            Assert.AreEqual(500, RuleDefine.MAX_COIN_ON_SCREEN);
            Assert.AreEqual(300, RuleDefine.MAX_COIN_ON_SCREEN_MOBILE);
            Assert.IsTrue(RuleDefine.COIN_SPAWN_INTERVAL > 0);
        }
    }

    /// <summary>
    /// 效果系统测试
    /// </summary>
    [TestFixture]
    public class EffectSystemTests
    {
        [Test]
        public void CoinEffectType_HasExpectedValues()
        {
            // 验证枚举值存在
            Assert.IsTrue(System.Enum.IsDefined(typeof(Effects.CoinEffectType), Effects.CoinEffectType.Basic));
            Assert.IsTrue(System.Enum.IsDefined(typeof(Effects.CoinEffectType), Effects.CoinEffectType.Gold));
            Assert.IsTrue(System.Enum.IsDefined(typeof(Effects.CoinEffectType), Effects.CoinEffectType.Diamond));
        }

        [Test]
        public void CoinPlateType_Has17Types()
        {
            var values = System.Enum.GetValues(typeof(Effects.CoinPlateType));
            Assert.GreaterOrEqual(values.Length, 17, "Should have at least 17 plate types");
        }
    }

    /// <summary>
    /// 构建配置测试
    /// </summary>
    [TestFixture]
    public class BuildConfigTests
    {
        [Test]
        public void RabbitBuildVersion_HasCorrectPlatform()
        {
            Assert.AreEqual("iOS", RabbitBuildVersion.PLATFORM);
            Assert.AreEqual("Unity 6000.3.0f1", RabbitBuildVersion.ENGINE);
        }

        [Test]
        public void PathDefine_HasRequiredPaths()
        {
            Assert.IsNotEmpty(PathDefine.PREFAB_PATH);
            Assert.IsNotEmpty(PathDefine.CONFIG_PATH);
            Assert.IsNotEmpty(PathDefine.FX_PATH);
        }
    }

    /// <summary>
    /// 场景配置测试
    /// </summary>
    [TestFixture]
    public class SceneConfigTests
    {
        [Test]
        public void SceneLoader_HasAllScenes()
        {
            var scenes = System.Enum.GetValues(typeof(Core.SceneLoader.SceneName));
            Assert.GreaterOrEqual(scenes.Length, 6, "Should have at least 6 scenes");
        }

        [Test]
        public void EditorBuildSettings_HasEnabledScenes()
        {
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            Assert.GreaterOrEqual(scenes.Length, 6, "Should have at least 6 scenes in build settings");
            
            foreach (var scene in scenes)
            {
                Assert.IsTrue(scene.enabled, $"Scene {scene.path} should be enabled");
            }
        }
    }
}
