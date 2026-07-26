using UnityEngine;
using System.Collections;
using Raccoin.Core;

namespace Raccoin.UI.Title
{
    /// <summary>
    /// 标题主 UI - 复刻原版 TitleUI
    /// </summary>
    public class TitleUI : MonoSingleton<TitleUI>
    {
        [SerializeField] private TitlePanelType _currentPanel = TitlePanelType.Lobby;
        [SerializeField] private GameObject[] _panels;

        public TitlePanelType CurrentPanel => _currentPanel;

        protected override void OnSingletonAwake()
        {
            StartCoroutine(IE_Init());
        }

        private IEnumerator IE_Init()
        {
            // 初始化标题界面
            SwitchPanel(TitlePanelType.Lobby);
            yield return null;
        }

        public void SwitchPanel(TitlePanelType panel)
        {
            _currentPanel = panel;
            int index = (int)panel;
            if (_panels != null)
            {
                for (int i = 0; i < _panels.Length; i++)
                {
                    if (_panels[i] != null)
                        _panels[i].SetActive(i == index);
                }
            }
        }
    }

    public enum TitlePanelType
    {
        Lobby = 0,
        GameMode = 1,
        Setup = 2,
        SkinCustom = 3,
        Challenge = 4,
        CharacterSelect = 5,
        Profile = 6,
        Codex = 7,
        GiftShop = 8,
        Milestone = 9,
        LuckyWheel = 10,
        Options = 11
    }

    /// <summary>标题大厅 - 复刻原版 TitleLobbyUIController</summary>
    public class TitleLobbyUIController : MonoBehaviour
    {
        public void OnPlayButtonClicked() { }
        public void OnProfileButtonClicked() { }
        public void OnSettingsButtonClicked() { }
    }

    /// <summary>标题游戏模式 - 复刻原版 TitleGameModeUIController</summary>
    public class TitleGameModeUIController : MonoBehaviour
    {
        public void SelectMode(GameMode mode) { }
    }

    /// <summary>标题设置 - 复刻原版 TitleSetupUIController</summary>
    public class TitleSetupUIController : MonoBehaviour { }

    /// <summary>标题新游戏设置 - 复刻原版 TitleSetupNewGameUIController</summary>
    public class TitleSetupNewGameUIController : MonoBehaviour
    {
        public void StartNewGame(NewGameRequestInfo info) { }
    }

    public class NewGameRequestInfo
    {
        public GameMode Mode;
        public string CharacterId;
        public int Difficulty;
    }

    /// <summary>标题继续游戏 - 复刻原版 TitleSetupContinueUIController</summary>
    public class TitleSetupContinueUIController : MonoBehaviour { }

    /// <summary>标题皮肤自定义 - 复刻原版 TitleSkinCustomUIController</summary>
    public class TitleSkinCustomUIController : MonoBehaviour { }

    /// <summary>标题皮肤选择 - 复刻原版 TitleSkinUIController</summary>
    public class TitleSkinUIController : MonoBehaviour { }

    /// <summary>标题挑战 - 复刻原版 TitleChallengeUIController</summary>
    public class TitleChallengeUIController : MonoBehaviour { }

    /// <summary>标题挑战设置 - 复刻原版 TitleSetupChallengeUIController</summary>
    public class TitleSetupChallengeUIController : MonoBehaviour { }

    /// <summary>角色选择 - 复刻原版 CharacterSelectUIController</summary>
    public class CharacterSelectUIController : MonoBehaviour
    {
        private IEnumerator ResetRebuild() { yield return null; }
    }

    /// <summary>神选择主硬币 - 复刻原版 GodSelectMasterCoinController</summary>
    public class GodSelectMasterCoinController : MonoBehaviour { }

    /// <summary>神选择道具 - 复刻原版 GodSelectPropController</summary>
    public class GodSelectPropController : MonoBehaviour { }

    /// <summary>标题幸运转盘 - 复刻原版 TitleLuckyWheelUIController</summary>
    public class TitleLuckyWheelUIController : MonoBehaviour { }

    /// <summary>玩家档案 - 复刻原版 ProfileUIController</summary>
    public class ProfileUIController : MonoBehaviour { }

    /// <summary>玩家档案统计 - 复刻原版 ProfileStatsUIController</summary>
    public class ProfileStatsUIController : MonoBehaviour { }

    /// <summary>玩家档案角色 - 复刻原版 ProfileCharacterUIController</summary>
    public class ProfileCharacterUIController : MonoBehaviour { }

    /// <summary>玩家档案历史 - 复刻原版 ProfileHistoryUIController</summary>
    public class ProfileHistoryUIController : MonoBehaviour { }

    /// <summary>图鉴 - 复刻原版 CodexUIController</summary>
    public class CodexUIController : MonoBehaviour { }

    /// <summary>图鉴收集 - 复刻原版 CodexCollectionUIController</summary>
    public class CodexCollectionUIController : MonoBehaviour { }

    /// <summary>礼品商店 - 复刻原版 GiftShopUIController</summary>
    public class GiftShopUIController : MonoBehaviour { }

    /// <summary>里程碑 - 复刻原版 MilestoneUIController</summary>
    public class MilestoneUIController : MonoBehaviour { }

    /// <summary>标题语言设置 - 复刻原版 TitleLanguageUIController</summary>
    public class TitleLanguageUIController : MonoBehaviour { }

    /// <summary>标题隐私协议 - 复刻原版 TitlePrivacyAgreeUIController</summary>
    public class TitlePrivacyAgreeUIController : MonoBehaviour { }

    /// <summary>标题注册 - 复刻原版 TitleSignUpUIController</summary>
    public class TitleSignUpUIController : MonoBehaviour
    {
        private IEnumerator IE_SubmitRequest() { yield return null; }
    }

    /// <summary>标题经典设置 - 复刻原版 TitleClassicSetupUIController</summary>
    public class TitleClassicSetupUIController : MonoBehaviour { }

    /// <summary>标题预览机器 - 复刻原版 TitlePreviewMachine</summary>
    public class TitlePreviewMachine : MonoBehaviour
    {
        private IEnumerator InitMachineSkin() { yield return null; }
    }

    /// <summary>标题模型控制器 - 复刻原版 TitleModelController</summary>
    public class TitleModelController : MonoBehaviour
    {
        private IEnumerator IE_ShowCamera() { yield return null; }
        private IEnumerator IE_HideCamera() { yield return null; }
    }

    /// <summary>标题相机管理器 - 复刻原版 TitleCameraManager</summary>
    public class TitleCameraManager : MonoBehaviour { }

    /// <summary>加载 UI - 复刻原版 LoadingUIController</summary>
    public class LoadingUIController : MonoBehaviour
    {
        private IEnumerator FadeOut() { yield return null; }
    }
}
