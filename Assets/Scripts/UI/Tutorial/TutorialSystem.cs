using UnityEngine;
using System.Collections;
using Raccoin.Core;

namespace Raccoin.UI.Tutorial
{
    /// <summary>
    /// 教程基类 - 复刻原版 TutorialBase
    /// </summary>
    public abstract class TutorialBase : MonoBehaviour
    {
        [SerializeField] protected TutorialGroup _group;
        protected bool _isCompleted;
        protected int _currentStep;

        public bool IsCompleted => _isCompleted;
        public TutorialGroup Group => _group;

        public virtual void StartTutorial()
        {
            _currentStep = 0;
            _isCompleted = false;
        }

        protected IEnumerator IE_CompleteTask()
        {
            _isCompleted = true;
            OnTutorialComplete();
            yield return null;
        }

        protected virtual void OnTutorialComplete() { }
        protected virtual void OnStepComplete(int step) { }
    }

    /// <summary>
    /// 游戏内教程控制器 - 复刻原版 TutorialUIController
    /// </summary>
    public class TutorialUIController : TutorialBase
    {
        [SerializeField] private TutorialTaskUIController _taskUI;
        [SerializeField] private TutorialHighLightFrame _highlightFrame;
        [SerializeField] private DialogueUIView _dialogueView;

        public override void StartTutorial()
        {
            base.StartTutorial();
            StartCoroutine(RunTutorialSequence());
        }

        private IEnumerator RunTutorialSequence()
        {
            // Step 1: 等待贴纸触发
            yield return StartCoroutine(Custom_WaitStickerTrigger());

            // Step 2: 等待盘子浇水
            yield return StartCoroutine(Custom_WaitPlateWater());

            // Step 3: 等待插入浇水
            yield return StartCoroutine(Custom_WaitInsertWater());

            // Step 4: 等待盘子商店
            yield return StartCoroutine(Custom_WaitPlateShop());

            // Step 5: 等待插入烹饪
            yield return StartCoroutine(Custom_WaitInsertCook());

            // Step 6: 生成两个硬币
            yield return StartCoroutine(Custom_SpawnTwoCoin());

            // Step 7: 生成左右硬币
            yield return StartCoroutine(Custom_SpawnLeftRightCoin());

            // Step 8: 等待回合结束
            yield return StartCoroutine(Custom_WaitRoundEnd());

            // Step 9: 等待硬币结算
            yield return StartCoroutine(IE_WaitCoinSettle());

            // Step 10: 介绍失败处理
            yield return StartCoroutine(IE_IntroFail());

            // Step 11: 等待回合结束
            yield return StartCoroutine(IE_WaitRoundEnd());

            // Step 12: 等待第一个商店
            yield return StartCoroutine(IE_WaitFirstShop());

            // Step 13: 等待交换
            yield return StartCoroutine(Custom_WaitExchange());

            // Step 14: 等待特殊硬币
            yield return StartCoroutine(IE_WaitSpecialCoin());

            // Step 15: 等待奖励
            yield return StartCoroutine(IE_WaitBonus());

            yield return StartCoroutine(IE_CompleteTask());
        }

        // ===== 自定义等待协程 =====
        private IEnumerator Custom_WaitStickerTrigger() { yield return null; }
        private IEnumerator Custom_WaitPlateWater() { yield return null; }
        private IEnumerator Custom_WaitInsertWater() { yield return null; }
        private IEnumerator Custom_WaitPlateShop() { yield return null; }
        private IEnumerator Custom_WaitInsertCook() { yield return null; }
        private IEnumerator Custom_SpawnTwoCoin() { yield return null; }
        private IEnumerator Custom_SpawnLeftRightCoin() { yield return null; }
        private IEnumerator Custom_WaitRoundEnd() { yield return null; }
        private IEnumerator Custom_WaitExchange() { yield return null; }
        private IEnumerator Custom_WaitSpecialCoin() { yield return null; }
        private IEnumerator Custom_WaitCoinSetlle_Combo() { yield return null; }
        private IEnumerator Custom_WaitRainPrize() { yield return null; }
        private IEnumerator Custom_FifthRoundTask() { yield return null; }
        private IEnumerator Custom_FirstShopWaitBuy() { yield return null; }
        private IEnumerator Custom_FirstShopWaitBuyAll() { yield return null; }
        private IEnumerator Custom_FirstShopWaitExpand() { yield return null; }
        private IEnumerator Custom_FirstShopWaitFinish() { yield return null; }
        private IEnumerator Custom_SecondShopWaitBuyChip() { yield return null; }
        private IEnumerator Custom_SecondShopWaitBuyCoin() { yield return null; }
        private IEnumerator Custom_SecondShopWaitFinish() { yield return null; }
        private IEnumerator Custom_ThirdShopWaitBuyChip() { yield return null; }
        private IEnumerator Custom_ThirdShopWaitBuyCoin() { yield return null; }
        private IEnumerator Custom_ThirdShopWaitBuyPrize() { yield return null; }
        private IEnumerator Custom_ThirdShopWaitExpand() { yield return null; }
        private IEnumerator Custom_ThirdShopWaitFinish() { yield return null; }
        private IEnumerator Custom_WaitKiller() { yield return null; }
        private IEnumerator Custom_WaitThirdShop() { yield return null; }

        // ===== IE 等待协程 =====
        private IEnumerator IE_WaitCoinSettle() { yield return null; }
        private IEnumerator IE_IntroFail() { yield return null; }
        private IEnumerator IE_WaitRoundEnd() { yield return null; }
        private IEnumerator IE_WaitFirstShop() { yield return null; }
        private IEnumerator IE_WaitExchangeCoin() { yield return null; }
        private IEnumerator IE_WaitExchangeFail() { yield return null; }
        private IEnumerator IE_WaitExchangeFail_Round3() { yield return null; }
        private IEnumerator IE_WaitSpecialCoin() { yield return null; }
        private IEnumerator IE_WaitSecondShop() { yield return null; }
        private IEnumerator IE_WaitBadCoin() { yield return null; }
        private IEnumerator IE_WaitClearPoor() { yield return null; }
        private IEnumerator IE_FailKillerCoin() { yield return null; }
        private IEnumerator IE_WaitKeychain() { yield return null; }
        private IEnumerator IE_PlayerAttack() { yield return null; }
        private IEnumerator IE_WaitKiller() { yield return null; }
        private IEnumerator IE_WaitBonus() { yield return null; }
        private IEnumerator IE_WaitLuckyWheel() { yield return null; }
        private IEnumerator IE_WaitCombo() { yield return null; }
        private IEnumerator IE_WaitStartRain() { yield return null; }
        private IEnumerator IE_WaitFourthShop() { yield return null; }
        private IEnumerator IE_WaitFourthShopFinish() { yield return null; }
        private IEnumerator IE_FirstShopFinish() { yield return null; }
        private IEnumerator IE_FailFifth() { yield return null; }
        private IEnumerator SetWaitRoundTaskClear() { yield return null; }
    }

    /// <summary>
    /// 标题教程控制器 - 复刻原版 TitleTutorialUIController
    /// </summary>
    public class TitleTutorialUIController : TutorialBase
    {
        private IEnumerator Custom_ShowWheelModel() { yield return null; }
        private IEnumerator Custom_WaitWheelOpen() { yield return null; }
        private IEnumerator IE_Skip() { yield return null; }
    }

    /// <summary>
    /// 教程任务 UI 控制器 - 复刻原版 TutorialTaskUIController
    /// </summary>
    public class TutorialTaskUIController : MonoBehaviour
    {
        [SerializeField] private TutorialTaskUIView _view;

        public void ShowTask(string taskText)
        {
            _view?.SetText(taskText);
        }

        public void HideTask()
        {
            _view?.Hide();
        }
    }

    /// <summary>
    /// 教程任务 UI 视图 - 复刻原版 TutorialTaskUIView
    /// </summary>
    public class TutorialTaskUIView : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _taskText;
        [SerializeField] private GameObject _panel;

        public void SetText(string text)
        {
            if (_taskText != null) _taskText.text = text;
            _panel?.SetActive(true);
        }

        public void Hide()
        {
            _panel?.SetActive(false);
        }
    }

    /// <summary>
    /// 教程高亮框 - 复刻原版 TutorialHighLightFrame
    /// </summary>
    public class TutorialHighLightFrame : MonoBehaviour
    {
        [SerializeField] private RectTransform _frameRect;
        [SerializeField] private float _padding = 10f;

        public void Highlight(RectTransform target)
        {
            if (_frameRect == null || target == null) return;

            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Vector3 center = (corners[0] + corners[2]) / 2f;
            Vector3 size = corners[2] - corners[0] + Vector3.one * _padding * 2;

            _frameRect.position = center;
            _frameRect.sizeDelta = size;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 对话 UI 视图 - 复刻原版 DialogueUIView
    /// </summary>
    public class DialogueUIView : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _dialogueText;
        [SerializeField] private float _typeSpeed = 0.03f;

        public IEnumerator IE_TypeNext(string fullText)
        {
            _dialogueText.text = "";
            foreach (char c in fullText)
            {
                _dialogueText.text += c;
                yield return new WaitForSeconds(_typeSpeed);
            }
        }

        public void ShowImmediate(string text)
        {
            _dialogueText.text = text;
        }
    }

    /// <summary>
    /// 浣熊洞遮罩 - 复刻原版 RabbitHoleMask
    /// </summary>
    public class RabbitHoleMask : MonoBehaviour { }

    // ===== 枚举 =====
    public enum TutorialGroup
    {
        Basic = 0,
        Shop = 1,
        Combat = 2,
        Special = 3,
        Wheel = 4
    }

    public enum TutorialAction
    {
        Wait = 0,
        Highlight = 1,
        Dialogue = 2,
        Spawn = 3,
        Complete = 4
    }

    public enum TutorialTaskType
    {
        SpawnCoin = 0,
        SettleCoin = 1,
        OpenShop = 2,
        BuyItem = 3,
        UseSkill = 4,
        WinRound = 5
    }
}
