using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Core;

namespace Raccoin.UI
{
    /// <summary>
    /// 游戏内 UI 管理器 - 复刻原版 GameplayUIManager
    /// </summary>
    public class GameplayUIManager : MonoSingleton<GameplayUIManager>
    {
        [SerializeField] private List<GameplayUISubSystem> _subSystems = new();
        private GameplayUIMediator _mediator;

        public GameplayUIMediator Mediator => _mediator;

        protected override void OnSingletonAwake()
        {
            _mediator = new GameplayUIMediator();
            InitializeSubSystems();
        }

        private void InitializeSubSystems()
        {
            foreach (var sub in _subSystems)
            {
                sub.Initialize(_mediator);
            }
        }

        public T GetSubSystem<T>() where T : GameplayUISubSystem
        {
            return _subSystems.Find(s => s is T) as T;
        }

        public IEnumerator IE_SaveDeal()
        {
            yield return null;
        }
    }

    /// <summary>
    /// 游戏内 UI 中介者 - 复刻原版 GameplayUIMediator
    /// </summary>
    public class GameplayUIMediator
    {
        public event System.Action OnShopOpen;
        public event System.Action OnShopClose;
        public event System.Action OnGameOver;
        public event System.Action<int> OnRoundEnd;
        public event System.Action<long> OnScoreUpdate;

        public void BroadcastShopOpen() => OnShopOpen?.Invoke();
        public void BroadcastShopClose() => OnShopClose?.Invoke();
        public void BroadcastGameOver() => OnGameOver?.Invoke();
        public void BroadcastRoundEnd(int round) => OnRoundEnd?.Invoke(round);
        public void BroadcastScoreUpdate(long score) => OnScoreUpdate?.Invoke(score);
    }

    /// <summary>
    /// 游戏内 UI 子系统基类 - 复刻原版 GameplayUISubSystem
    /// </summary>
    public abstract class GameplayUISubSystem : MonoBehaviour
    {
        protected GameplayUIMediator Mediator { get; private set; }
        protected GameplayUIManager UIManager => GameplayUIManager.Instance;

        public virtual void Initialize(GameplayUIMediator mediator)
        {
            Mediator = mediator;
        }

        public virtual void Show() { gameObject.SetActive(true); }
        public virtual void Hide() { gameObject.SetActive(false); }
        public virtual void Refresh() { }
    }

    // ===== 游戏内 UI 控制器 =====

    /// <summary>操作 UI - 复刻原版 ActionUIController</summary>
    public class ActionUIController : GameplayUISubSystem
    {
        public void ShowPrizeAction(PrizeActionInfo info) { }
        public void ShowGadgetAction(GadgetActionInfo info) { }
        public void ShowRobotAction(RobotActionInfo info) { }
    }

    public struct PrizeActionInfo { public int PrizeId; public int Amount; }
    public struct GadgetActionInfo { public int GadgetId; }
    public struct RobotActionInfo { public RobotActionType Type; }
    public enum RobotActionType { Push = 0, Collect = 1, Attack = 2 }

    /// <summary>属性信息 UI - 复刻原版 AttributeInfoUIController</summary>
    public class AttributeInfoUIController : GameplayUISubSystem { }

    /// <summary>Buff UI - 复刻原版 BuffUIController</summary>
    public class BuffUIController : GameplayUISubSystem
    {
        public void RefreshBuffView(BuffViewRefreshInfo info) { }
    }
    public struct BuffViewRefreshInfo { public int BuffId; public float Duration; }

    /// <summary>芯片 UI - 复刻原版 ChipUIController</summary>
    public class ChipUIController : GameplayUISubSystem
    {
        private IEnumerator IE_LoadChipView() { yield return null; }
    }

    /// <summary>夹币 UI - 复刻原版 CoinClipUIController</summary>
    public class CoinClipUIController : GameplayUISubSystem
    {
        private IEnumerator IE_LoadClipCoinView() { yield return null; }
        private IEnumerator IE_ClipShake_Buy() { yield return null; }
        private IEnumerator IE_ClipShake_Fail() { yield return null; }
    }

    /// <summary>游戏菜单 UI - 复刻原版 GameMenuUIController</summary>
    public class GameMenuUIController : GameplayUISubSystem
    {
        private IEnumerator IE_SaveRequest() { yield return null; }
    }

    /// <summary>游戏结束 UI - 复刻原版 GameOverUIController</summary>
    public class GameOverUIController : GameplayUISubSystem
    {
        private IEnumerator IE_Show() { yield return null; }
    }

    /// <summary>游戏结束里程碑 UI - 复刻原版 GameOverMilestoneUIController</summary>
    public class GameOverMilestoneUIController : GameplayUISubSystem
    {
        private IEnumerator IE_ProgressAnim_Show() { yield return null; }
        private IEnumerator IE_ProgressAnim_Move() { yield return null; }
    }

    /// <summary>游戏结束解锁 UI - 复刻原版 GameOverUnlockUIController</summary>
    public class GameOverUnlockUIController : GameplayUISubSystem
    {
        private IEnumerator IE_ShowUnlock() { yield return null; }
        private IEnumerator IE_ShowUnlockAll() { yield return null; }
    }

    /// <summary>游戏结束促销 UI - 复刻原版 GameOverSaleUIController</summary>
    public class GameOverSaleUIController : GameplayUISubSystem { }

    /// <summary>信息窗口 UI - 复刻原版 InfoWindowUIController</summary>
    public class InfoWindowUIController : GameplayUISubSystem { }

    /// <summary>钥匙扣 UI - 复刻原版 KeychainUIController</summary>
    public class KeychainUIController : GameplayUISubSystem
    {
        private IEnumerator IE_ShowPanel() { yield return null; }
    }

    /// <summary>幸运转盘 UI - 复刻原版 LuckyWheelUIController</summary>
    public class LuckyWheelUIController : GameplayUISubSystem
    {
        private IEnumerator IE_BecomeSmall() { yield return null; }
        private IEnumerator WheelPartEffect() { yield return null; }
    }

    /// <summary>监控 UI - 复刻原版 MonitorUIController</summary>
    public class MonitorUIController : GameplayUISubSystem
    {
        private IEnumerator IE_SaveDeal() { yield return null; }
    }

    /// <summary>奖品 UI - 复刻原版 PrizeUIController</summary>
    public class PrizeUIController : GameplayUISubSystem
    {
        private IEnumerator IE_LoadPrizeView() { yield return null; }
    }

    /// <summary>机器人 UI - 复刻原版 RobotUIController</summary>
    public class RobotUIController : GameplayUISubSystem { }

    /// <summary>回合结束 UI - 复刻原版 RoundEndUIController</summary>
    public class RoundEndUIController : GameplayUISubSystem
    {
        private IEnumerator IE_WaitSave() { yield return null; }
    }

    /// <summary>商店 UI - 复刻原版 ShopUIController</summary>
    public class ShopUIController : GameplayUISubSystem
    {
        private IEnumerator ShopInAnim() { yield return null; }
        private IEnumerator ShopOutAnim() { yield return null; }
        private IEnumerator IE_RerollWait() { yield return null; }
        private IEnumerator EatTicketAnim() { yield return null; }
    }

    /// <summary>票券 UI - 复刻原版 TicketUIController</summary>
    public class TicketUIController : GameplayUISubSystem
    {
        private IEnumerator IE_TicketEffect() { yield return null; }
    }

    /// <summary>画布特效 UI - 复刻原版 CanvasEffectUIController</summary>
    public class CanvasEffectUIController : GameplayUISubSystem { }

    /// <summary>世界特效 UI - 复刻原版 WorldEffectUIController</summary>
    public class WorldEffectUIController : GameplayUISubSystem { }

    /// <summary>输入 UI - 复刻原版 InputUIController</summary>
    public class InputUIController : GameplayUISubSystem { }

    /// <summary>游戏选项 UI - 复刻原版 GameOptionUIController</summary>
    public class GameOptionUIController : GameplayUISubSystem { }

    /// <summary>硬币积分信息 UI - 复刻原版 CoinPtInfoUIController</summary>
    public class CoinPtInfoUIController : GameplayUISubSystem
    {
        private IEnumerator IE_UpdateHeight() { yield return null; }
    }

    /// <summary>文本链接 UI - 复刻原版 TextLinkInfoUIController</summary>
    public class TextLinkInfoUIController : GameplayUISubSystem { }

    /// <summary>道具解锁 UI - 复刻原版 ItemUnlockUIController</summary>
    public class ItemUnlockUIController : GameplayUISubSystem
    {
        private IEnumerator IE_ShowUnlock() { yield return null; }
    }

    /// <summary>经典模式右上角 UI - 复刻原版 ClassicTopRightUIController</summary>
    public class ClassicTopRightUIController : GameplayUISubSystem { }

    /// <summary>无后处理画布特效 - 复刻原版 NoPPCanvasEffectUIController</summary>
    public class NoPPCanvasEffectUIController : GameplayUISubSystem { }
}
