using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Core;

namespace Raccoin.UI.Lab
{
    /// <summary>
    /// Lab UI 管理器 - 复刻原版 LabUIManager
    /// 管理实验室模式的所有 UI
    /// </summary>
    public class LabUIManager : MonoSingleton<LabUIManager>
    {
        [Header("Panels")]
        [SerializeField] private LabFunctionPanel _functionPanel;
        [SerializeField] private LabItemPanel _itemPanel;
        [SerializeField] private LabPropertyPanel _propertyPanel;
        [SerializeField] private LabScoreBoardPanel _scoreBoardPanel;

        [Header("Controllers")]
        [SerializeField] private LabUIController _mainController;

        private LabPanelType _currentPanel = LabPanelType.None;

        public enum LabPanelType
        {
            None,
            Function,
            Item,
            Property,
            ScoreBoard
        }

        public void Initialize()
        {
            HideAllPanels();
        }

        public void ShowPanel(LabPanelType panelType)
        {
            HideAllPanels();
            _currentPanel = panelType;

            switch (panelType)
            {
                case LabPanelType.Function:
                    _functionPanel?.Show();
                    break;
                case LabPanelType.Item:
                    _itemPanel?.Show();
                    break;
                case LabPanelType.Property:
                    _propertyPanel?.Show();
                    break;
                case LabPanelType.ScoreBoard:
                    _scoreBoardPanel?.Show();
                    break;
            }
        }

        public void HideAllPanels()
        {
            _functionPanel?.Hide();
            _itemPanel?.Hide();
            _propertyPanel?.Hide();
            _scoreBoardPanel?.Hide();
            _currentPanel = LabPanelType.None;
        }

        public LabPanelType CurrentPanel => _currentPanel;
    }

    /// <summary>
    /// Lab UI 控制器 - 复刻原版 LabUIController
    /// </summary>
    public class LabUIController : MonoBehaviour
    {
        [SerializeField] private LabUIManager _manager;

        public void OnFunctionButtonClicked()
        {
            _manager.ShowPanel(LabUIManager.LabPanelType.Function);
        }

        public void OnItemButtonClicked()
        {
            _manager.ShowPanel(LabUIManager.LabPanelType.Item);
        }

        public void OnPropertyButtonClicked()
        {
            _manager.ShowPanel(LabUIManager.LabPanelType.Property);
        }

        public void OnCloseButtonClicked()
        {
            _manager.HideAllPanels();
        }
    }

    /// <summary>
    /// Lab 功能面板 - 复刻原版 LabFunctionPanel
    /// </summary>
    public class LabFunctionPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button[] _functionButtons;
        [SerializeField] private Text _titleLabel;

        public void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
            }
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
        }

        public void SetFunctions(List<LabFunctionData> functions)
        {
            for (int i = 0; i < _functionButtons.Length && i < functions.Count; i++)
            {
                var func = functions[i];
                _functionButtons[i].gameObject.SetActive(true);
                _functionButtons[i].GetComponentInChildren<Text>().text = func.Name;
            }
        }
    }

    /// <summary>
    /// Lab 物品面板 - 复刻原版 LabItemPanel
    /// </summary>
    public class LabItemPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private GameObject _itemSlotPrefab;

        private List<LabItemSlot> _slots = new();

        public void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
            }
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
        }

        public void SetItems(List<LabItemData> items)
        {
            // 清空现有
            foreach (var slot in _slots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _slots.Clear();

            // 创建新槽位
            foreach (var item in items)
            {
                var slotObj = Instantiate(_itemSlotPrefab, _itemContainer);
                var slot = slotObj.GetComponent<LabItemSlot>();
                slot.Setup(item);
                _slots.Add(slot);
            }
        }
    }

    /// <summary>
    /// Lab 属性面板 - 复刻原版 LabPropertyPanel
    /// </summary>
    public class LabPropertyPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _descriptionLabel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Slider[] _propertySliders;
        [SerializeField] private Text[] _propertyLabels;

        public void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
            }
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
        }

        public void ShowProperties(LabPropertyData data)
        {
            _nameLabel.text = data.Name;
            _descriptionLabel.text = data.Description;

            for (int i = 0; i < _propertySliders.Length && i < data.Values.Length; i++)
            {
                _propertySliders[i].value = data.Values[i];
                _propertyLabels[i].text = $"{data.PropertyNames[i]}: {data.Values[i]:F1}";
            }
        }
    }

    /// <summary>
    /// Lab 计分板面板
    /// </summary>
    public class LabScoreBoardPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _scoreLabel;
        [SerializeField] private Text _comboLabel;
        [SerializeField] private Text _multiplierLabel;

        public void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null) _canvasGroup.alpha = 1;
        }

        public void Hide()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        public void UpdateScore(long score, int combo, float multiplier)
        {
            _scoreLabel.text = score.ToString("N0");
            _comboLabel.text = $"Combo x{combo}";
            _multiplierLabel.text = $"x{multiplier:F1}";
        }
    }

    /// <summary>
    /// Lab 物品槽
    /// </summary>
    public class LabItemSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _countLabel;
        [SerializeField] private Button _button;

        private LabItemData _data;

        public void Setup(LabItemData data)
        {
            _data = data;
            _nameLabel.text = data.Name;
            _countLabel.text = $"x{data.Count}";
            if (data.Icon != null) _icon.sprite = data.Icon;
        }

        private void OnEnable()
        {
            _button?.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            // 使用物品
            Debug.Log($"[Lab] Using item: {_data?.Name}");
        }
    }

    // Lab 数据结构
    public class LabFunctionData
    {
        public int Id;
        public string Name;
        public string Description;
        public int Cost;
    }

    public class LabItemData
    {
        public int Id;
        public string Name;
        public int Count;
        public Sprite Icon;
    }

    public class LabPropertyData
    {
        public string Name;
        public string Description;
        public string[] PropertyNames;
        public float[] Values;
    }
}

namespace Raccoin.UI.Classic
{
    /// <summary>
    /// 经典模式 UI 管理器 - 复刻原版 ClassicUIManager
    /// </summary>
    public class ClassicUIManager : MonoSingleton<ClassicUIManager>
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject _mainHUD;
        [SerializeField] private GameObject _scorePanel;
        [SerializeField] private GameObject _coinCountPanel;
        [SerializeField] private GameObject _roundInfoPanel;

        [Header("Sub Controllers")]
        [SerializeField] private ClassicScoreController _scoreController;
        [SerializeField] private ClassicCoinCounter _coinCounter;
        [SerializeField] private ClassicRoundDisplay _roundDisplay;
        [SerializeField] private ClassicToolPanel _toolPanel;

        public void Initialize()
        {
            ShowMainHUD();
        }

        public void ShowMainHUD()
        {
            _mainHUD?.SetActive(true);
            _scorePanel?.SetActive(true);
            _coinCountPanel?.SetActive(true);
            _roundInfoPanel?.SetActive(true);
        }

        public void HideMainHUD()
        {
            _mainHUD?.SetActive(false);
        }

        public void UpdateScore(long score)
        {
            _scoreController?.SetScore(score);
        }

        public void UpdateCoinCount(int current, int max)
        {
            _coinCounter?.SetCount(current, max);
        }

        public void UpdateRound(int current, int total)
        {
            _roundDisplay?.SetRound(current, total);
        }
    }

    /// <summary>经典模式分数控制器</summary>
    public class ClassicScoreController : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _highScoreText;
        [SerializeField] private Animator _animator;

        private long _currentScore;
        private long _displayScore;

        public void SetScore(long score)
        {
            _currentScore = score;
            StartCoroutine(IE_AnimateScore());
        }

        private IEnumerator IE_AnimateScore()
        {
            while (_displayScore < _currentScore)
            {
                _displayScore += Mathf.Max(1, (_currentScore - _displayScore) / 10);
                _scoreText.text = _displayScore.ToString("N0");
                yield return null;
            }
            _displayScore = _currentScore;
            _scoreText.text = _currentScore.ToString("N0");
        }

        public void SetHighScore(long highScore)
        {
            _highScoreText.text = $"Best: {highScore:N0}";
        }

        public void PlayScorePopup()
        {
            _animator?.SetTrigger("Popup");
        }
    }

    /// <summary>经典模式硬币计数器</summary>
    public class ClassicCoinCounter : MonoBehaviour
    {
        [SerializeField] private Text _countText;
        [SerializeField] private Slider _countSlider;
        [SerializeField] private Image _fillImage;

        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _dangerColor = Color.red;

        public void SetCount(int current, int max)
        {
            _countText.text = $"{current}/{max}";
            _countSlider.maxValue = max;
            _countSlider.value = current;

            float ratio = (float)current / max;
            _fillImage.color = ratio > 0.9f ? _dangerColor : ratio > 0.7f ? _warningColor : _normalColor;
        }
    }

    /// <summary>经典模式回合显示</summary>
    public class ClassicRoundDisplay : MonoBehaviour
    {
        [SerializeField] private Text _roundText;
        [SerializeField] private Text _phaseText;
        [SerializeField] private GameObject _roundBanner;

        public void SetRound(int current, int total)
        {
            _roundText.text = $"Round {current}/{total}";
        }

        public void SetPhase(string phaseName)
        {
            _phaseText.text = phaseName;
        }

        public void ShowRoundBanner(int roundNumber)
        {
            StartCoroutine(IE_ShowBanner(roundNumber));
        }

        private IEnumerator IE_ShowBanner(int roundNumber)
        {
            _roundBanner.SetActive(true);
            _roundBanner.GetComponentInChildren<Text>().text = $"ROUND {roundNumber}";
            yield return new WaitForSeconds(2f);
            _roundBanner.SetActive(false);
        }
    }

    /// <summary>经典模式工具面板</summary>
    public class ClassicToolPanel : MonoBehaviour
    {
        [SerializeField] private Transform _toolContainer;
        [SerializeField] private GameObject _toolSlotPrefab;

        private List<ClassicToolSlot> _tools = new();

        public void SetTools(List<ClassicToolData> tools)
        {
            foreach (var slot in _tools)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _tools.Clear();

            foreach (var tool in tools)
            {
                var slotObj = Instantiate(_toolSlotPrefab, _toolContainer);
                var slot = slotObj.GetComponent<ClassicToolSlot>();
                slot.Setup(tool);
                _tools.Add(slot);
            }
        }
    }

    public class ClassicToolSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Button _button;

        public void Setup(ClassicToolData data)
        {
            _nameLabel.text = data.Name;
            if (data.Icon != null) _icon.sprite = data.Icon;
        }
    }

    public class ClassicToolData
    {
        public int Id;
        public string Name;
        public Sprite Icon;
        public int Count;
    }
}

namespace Raccoin.Visual
{
    /// <summary>
    /// 背景颜色控制器 - 复刻原版 BackgroundColorController
    /// 管理游戏背景颜色变化
    /// </summary>
    public class BackgroundColorController : MonoSingleton<BackgroundColorController>
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Gradient _dayNightGradient;
        [SerializeField] private float _transitionSpeed = 1f;

        private Color _currentColor;
        private Color _targetColor;

        public void SetBackgroundColor(Color color, bool instant = false)
        {
            _targetColor = color;
            if (instant)
            {
                _currentColor = color;
                ApplyColor();
            }
        }

        public void SetTheme(BackgroundTheme theme)
        {
            _targetColor = theme switch
            {
                BackgroundTheme.Classic => new Color(0.2f, 0.2f, 0.3f),
                BackgroundTheme.Night => new Color(0.05f, 0.05f, 0.15f),
                BackgroundTheme.Sunset => new Color(0.4f, 0.2f, 0.3f),
                BackgroundTheme.Ocean => new Color(0.1f, 0.3f, 0.4f),
                BackgroundTheme.Forest => new Color(0.1f, 0.3f, 0.1f),
                _ => Color.black
            };
        }

        private void Update()
        {
            if (_currentColor != _targetColor)
            {
                _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * _transitionSpeed);
                ApplyColor();
            }
        }

        private void ApplyColor()
        {
            if (_mainCamera != null)
            {
                _mainCamera.backgroundColor = _currentColor;
            }
            RenderSettings.ambientLight = _currentColor * 0.5f;
        }
    }

    public enum BackgroundTheme
    {
        Classic,
        Night,
        Sunset,
        Ocean,
        Forest
    }

    /// <summary>
    /// 机器皮肤管理器 - 复刻原版 MachineSkinManager
    /// 管理推币机的外观皮肤
    /// </summary>
    public class MachineSkinManager : MonoSingleton<MachineSkinManager>
    {
        [SerializeField] private MeshRenderer[] _machineRenderers;
        [SerializeField] private Material[] _skinMaterials;

        private int _currentSkinIndex;

        public int CurrentSkinIndex => _currentSkinIndex;

        public void SetSkin(int skinIndex)
        {
            if (skinIndex < 0 || skinIndex >= _skinMaterials.Length) return;

            _currentSkinIndex = skinIndex;
            var mat = _skinMaterials[skinIndex];

            foreach (var renderer in _machineRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = mat;
                }
            }
        }

        public void SetSkin(string skinName)
        {
            for (int i = 0; i < _skinMaterials.Length; i++)
            {
                if (_skinMaterials[i].name == skinName)
                {
                    SetSkin(i);
                    return;
                }
            }
        }

        public void UnlockSkin(int skinIndex)
        {
            // 解锁皮肤逻辑
            Debug.Log($"[MachineSkin] Unlocked skin {skinIndex}");
        }
    }

    /// <summary>
    /// 装饰管理器 - 复刻原版 DecoManager
    /// 管理游戏场景装饰物
    /// </summary>
    public class DecoManager : MonoSingleton<DecoManager>
    {
        [SerializeField] private Transform _decoContainer;
        [SerializeField] private GameObject[] _decoPrefabs;

        private List<GameObject> _activeDecos = new();

        public void AddDeco(int decoId, Vector3 position, Quaternion rotation)
        {
            if (decoId < 0 || decoId >= _decoPrefabs.Length) return;

            var deco = Instantiate(_decoPrefabs[decoId], position, rotation, _decoContainer);
            _activeDecos.Add(deco);
        }

        public void RemoveDeco(GameObject deco)
        {
            _activeDecos.Remove(deco);
            Destroy(deco);
        }

        public void ClearAllDecos()
        {
            foreach (var deco in _activeDecos)
            {
                if (deco != null) Destroy(deco);
            }
            _activeDecos.Clear();
        }

        public void SetDecoTheme(DecoTheme theme)
        {
            ClearAllDecos();
            // 根据主题加载装饰
            Debug.Log($"[Deco] Set theme: {theme}");
        }
    }

    public enum DecoTheme
    {
        Default,
        Christmas,
        Halloween,
        NewYear,
        Spring
    }

    /// <summary>
    /// 灯光控制器 - 管理游戏灯光效果
    /// </summary>
    public class LightingController : MonoSingleton<LightingController>
    {
        [SerializeField] private Light _mainLight;
        [SerializeField] private Light[] _accentLights;
        [SerializeField] private float _pulseSpeed = 2f;

        private bool _isPulsing;

        public void SetMainLightColor(Color color)
        {
            if (_mainLight != null) _mainLight.color = color;
        }

        public void SetMainLightIntensity(float intensity)
        {
            if (_mainLight != null) _mainLight.intensity = intensity;
        }

        public void StartPulse(Color color1, Color color2)
        {
            _isPulsing = true;
            StartCoroutine(IE_Pulse(color1, color2));
        }

        public void StopPulse()
        {
            _isPulsing = false;
        }

        private IEnumerator IE_Pulse(Color color1, Color color2)
        {
            float t = 0;
            while (_isPulsing)
            {
                t += Time.deltaTime * _pulseSpeed;
                Color color = Color.Lerp(color1, color2, (Mathf.Sin(t) + 1) / 2);
                SetMainLightColor(color);
                yield return null;
            }
        }
    }
}
