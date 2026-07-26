using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Raccoin.Data;

namespace Raccoin.Core
{
    /// <summary>
    /// 场景加载器 - 复刻原版 SceneLoader
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        public enum SceneName
        {
            Init = 0,
            Title = 1,
            Gameplay = 2,
            Lab = 3,
            GameplayChallenge = 4,
            Credits = 5
        }

        public SceneName CurrentScene { get; private set; } = SceneName.Init;
        public bool IsLoading { get; private set; }

        public IEnumerator LoadNewScene(SceneName targetScene, bool showLoading = true)
        {
            if (IsLoading) yield break;
            IsLoading = true;

            // 卸载当前场景
            yield return LoadNewScene_UnloadPhase();

            // 加载新场景
            var asyncOp = SceneManager.LoadSceneAsync((int)targetScene, LoadSceneMode.Single);
            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                yield return null;
            }

            asyncOp.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOp.isDone);

            CurrentScene = targetScene;
            IsLoading = false;
        }

        private IEnumerator LoadNewScene_UnloadPhase()
        {
            // 清理资源
            Resources.UnloadUnusedAssets();
            yield return null;
            System.GC.Collect();
        }

        public IEnumerator LoadSceneQuit()
        {
            yield return LoadNewScene(SceneName.Title);
        }
    }

    /// <summary>
    /// 游戏接口管理器 - 复刻原版 GameInterfaceManager
    /// </summary>
    public class GameInterfaceManager : Singleton<GameInterfaceManager>
    {
        public InGameData CurrentGameData { get; private set; }
        public bool IsInGame { get; private set; }

        public void EnterGame(GameMode mode)
        {
            IsInGame = true;
            CurrentGameData = new InGameData();
        }

        public void ExitGame()
        {
            IsInGame = false;
            CurrentGameData = null;
        }

        public IEnumerator IE_Save(SaveSource source)
        {
            // 触发存档
            DataPersistentManager.Instance.SaveAll();
            yield return null;
        }
    }

    /// <summary>
    /// 标题界面接口管理器 - 复刻原版 TitleInterfaceManager
    /// </summary>
    public class TitleInterfaceManager : Singleton<TitleInterfaceManager>
    {
        public bool IsInTitle { get; private set; }

        public void EnterTitle()
        {
            IsInTitle = true;
        }

        public void ExitTitle()
        {
            IsInTitle = false;
        }
    }

    /// <summary>
    /// 游戏内数据基类 - 复刻原版 InGameData
    /// </summary>
    public class InGameData
    {
        public int CurrentRound { get; set; }
        public long TotalScore { get; set; }
        public long CurrentCoins { get; set; }
        public GameMode Mode { get; set; }
    }
}
