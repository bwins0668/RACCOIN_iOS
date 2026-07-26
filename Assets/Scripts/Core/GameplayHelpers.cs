using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Raccoin.Core;
using Raccoin.Effects;

namespace Raccoin.Analytics
{
    /// <summary>
    /// 数据分析管理器 - 复刻原版 AnalyticsManager
    /// 收集游戏数据用于平衡性调整
    /// </summary>
    public class AnalyticsManager : MonoSingleton<AnalyticsManager>
    {
        [Header("Settings")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private float _flushInterval = 60f;

        private Dictionary<string, int> _eventCounts = new();
        private Dictionary<string, float> _eventTimers = new();
        private List<AnalyticsEvent> _eventQueue = new();
        private float _lastFlushTime;

        // 游戏统计数据
        public GameSessionStats SessionStats { get; private set; } = new();

        protected override void Awake()
        {
            base.Awake();
            _lastFlushTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - _lastFlushTime >= _flushInterval)
            {
                FlushEvents();
                _lastFlushTime = Time.time;
            }
        }

        /// <summary>
        /// 记录事件
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_enableAnalytics) return;

            if (!_eventCounts.ContainsKey(eventName))
            {
                _eventCounts[eventName] = 0;
            }
            _eventCounts[eventName]++;

            var evt = new AnalyticsEvent
            {
                Name = eventName,
                Timestamp = System.DateTime.UtcNow,
                Parameters = parameters ?? new Dictionary<string, object>()
            };
            _eventQueue.Add(evt);

            // 更新会话统计
            UpdateSessionStats(eventName);
        }

        /// <summary>
        /// 记录硬币事件
        /// </summary>
        public void TrackCoinEvent(CoinEventType type, int coinId, long value)
        {
            TrackEvent($"coin_{type}", new Dictionary<string, object>
            {
                { "coin_id", coinId },
                { "value", value }
            });

            switch (type)
            {
                case CoinEventType.Spawn:
                    SessionStats.CoinsSpawned++;
                    break;
                case CoinEventType.Collect:
                    SessionStats.CoinsCollected++;
                    SessionStats.TotalValueCollected += value;
                    break;
                case CoinEventType.Destroy:
                    SessionStats.CoinsDestroyed++;
                    break;
            }
        }

        /// <summary>
        /// 记录回合事件
        /// </summary>
        public void TrackRoundEvent(int roundNumber, long score, float duration)
        {
            TrackEvent("round_complete", new Dictionary<string, object>
            {
                { "round", roundNumber },
                { "score", score },
                { "duration", duration }
            });

            SessionStats.RoundsCompleted++;
            SessionStats.TotalPlayTime += duration;
        }

        /// <summary>
        /// 记录效果触发
        /// </summary>
        public void TrackEffectTrigger(string effectName)
        {
            TrackEvent("effect_trigger", new Dictionary<string, object>
            {
                { "effect", effectName }
            });
        }

        private void UpdateSessionStats(string eventName)
        {
            // 根据事件类型更新统计
        }

        /// <summary>
        /// 刷新事件队列(发送到服务器或保存)
        /// </summary>
        public void FlushEvents()
        {
            if (_eventQueue.Count == 0) return;

            // iOS: 可以发送到 Game Center 或自定义服务器
            Debug.Log($"[Analytics] Flushing {_eventQueue.Count} events");

            // 保存到本地
            SaveEventsLocally();

            _eventQueue.Clear();
        }

        private void SaveEventsLocally()
        {
            // 本地存储逻辑
        }

        /// <summary>
        /// 获取事件计数
        /// </summary>
        public int GetEventCount(string eventName)
        {
            return _eventCounts.ContainsKey(eventName) ? _eventCounts[eventName] : 0;
        }

        /// <summary>
        /// 重置会话统计
        /// </summary>
        public void ResetSession()
        {
            SessionStats = new GameSessionStats();
            _eventCounts.Clear();
            _eventQueue.Clear();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) FlushEvents();
        }

        private void OnApplicationQuit()
        {
            FlushEvents();
        }
    }

    public enum CoinEventType
    {
        Spawn,
        Collect,
        Destroy,
        Settle,
        EffectTrigger
    }

    public class AnalyticsEvent
    {
        public string Name;
        public System.DateTime Timestamp;
        public Dictionary<string, object> Parameters;
    }

    public class GameSessionStats
    {
        public int CoinsSpawned;
        public int CoinsCollected;
        public int CoinsDestroyed;
        public long TotalValueCollected;
        public int RoundsCompleted;
        public float TotalPlayTime;
        public int EffectsTriggered;
        public long HighestScore;
        public int MaxCombo;
    }
}

namespace Raccoin.Gameplay
{
    /// <summary>
    /// 游戏计时器 - 复刻原版 GameplayTimer
    /// 管理游戏内各种计时
    /// </summary>
    public class GameplayTimer : MonoSingleton<GameplayTimer>
    {
        private Dictionary<string, TimerData> _timers = new();

        public class TimerData
        {
            public float Duration;
            public float Elapsed;
            public bool IsRunning;
            public bool IsLoop;
            public System.Action OnComplete;
            public System.Action<float> OnTick;
        }

        /// <summary>
        /// 创建计时器
        /// </summary>
        public void CreateTimer(string id, float duration, System.Action onComplete = null, bool loop = false)
        {
            _timers[id] = new TimerData
            {
                Duration = duration,
                Elapsed = 0,
                IsRunning = true,
                IsLoop = loop,
                OnComplete = onComplete
            };
        }

        /// <summary>
        /// 创建带回调的计时器
        /// </summary>
        public void CreateTimer(string id, float duration, System.Action<float> onTick, System.Action onComplete = null)
        {
            _timers[id] = new TimerData
            {
                Duration = duration,
                Elapsed = 0,
                IsRunning = true,
                OnTick = onTick,
                OnComplete = onComplete
            };
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void StopTimer(string id)
        {
            if (_timers.ContainsKey(id))
            {
                _timers[id].IsRunning = false;
            }
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        public void ResetTimer(string id)
        {
            if (_timers.ContainsKey(id))
            {
                _timers[id].Elapsed = 0;
                _timers[id].IsRunning = true;
            }
        }

        /// <summary>
        /// 删除计时器
        /// </summary>
        public void RemoveTimer(string id)
        {
            _timers.Remove(id);
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        public float GetRemainingTime(string id)
        {
            if (_timers.ContainsKey(id))
            {
                return Mathf.Max(0, _timers[id].Duration - _timers[id].Elapsed);
            }
            return 0;
        }

        /// <summary>
        /// 获取进度 (0-1)
        /// </summary>
        public float GetProgress(string id)
        {
            if (_timers.ContainsKey(id))
            {
                return Mathf.Clamp01(_timers[id].Elapsed / _timers[id].Duration);
            }
            return 0;
        }

        private void Update()
        {
            var toRemove = new List<string>();

            foreach (var kvp in _timers)
            {
                var timer = kvp.Value;
                if (!timer.IsRunning) continue;

                timer.Elapsed += Time.deltaTime;
                timer.OnTick?.Invoke(timer.Elapsed / timer.Duration);

                if (timer.Elapsed >= timer.Duration)
                {
                    timer.OnComplete?.Invoke();

                    if (timer.IsLoop)
                    {
                        timer.Elapsed = 0;
                    }
                    else
                    {
                        timer.IsRunning = false;
                        toRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var id in toRemove)
            {
                _timers.Remove(id);
            }
        }

        /// <summary>
        /// 暂停所有计时器
        /// </summary>
        public void PauseAll()
        {
            foreach (var timer in _timers.Values)
            {
                timer.IsRunning = false;
            }
        }

        /// <summary>
        /// 恢复所有计时器
        /// </summary>
        public void ResumeAll()
        {
            foreach (var timer in _timers.Values)
            {
                timer.IsRunning = true;
            }
        }
    }

    /// <summary>
    /// 游戏刷新器 - 复刻原版 GameplayRefresher
    /// 管理周期性刷新逻辑
    /// </summary>
    public class GameplayRefresher : MonoSingleton<GameplayRefresher>
    {
        private List<RefreshData> _refreshers = new();

        public class RefreshData
        {
            public string Id;
            public float Interval;
            public float Timer;
            public System.Action OnRefresh;
            public bool IsActive;
        }

        /// <summary>
        /// 注册刷新器
        /// </summary>
        public void Register(string id, float interval, System.Action onRefresh)
        {
            _refreshers.Add(new RefreshData
            {
                Id = id,
                Interval = interval,
                Timer = 0,
                OnRefresh = onRefresh,
                IsActive = true
            });
        }

        /// <summary>
        /// 注销刷新器
        /// </summary>
        public void Unregister(string id)
        {
            _refreshers.RemoveAll(r => r.Id == id);
        }

        /// <summary>
        /// 设置激活状态
        /// </summary>
        public void SetActive(string id, bool active)
        {
            var refresher = _refreshers.Find(r => r.Id == id);
            if (refresher != null) refresher.IsActive = active;
        }

        private void Update()
        {
            foreach (var refresher in _refreshers)
            {
                if (!refresher.IsActive) continue;

                refresher.Timer += Time.deltaTime;
                if (refresher.Timer >= refresher.Interval)
                {
                    refresher.Timer = 0;
                    refresher.OnRefresh?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// 游戏网格绘制器 - 复刻原版 GameplayMeshDrawer
    /// 用于绘制调试网格和辅助线
    /// </summary>
    public class GameplayMeshDrawer : MonoBehaviour
    {
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private bool _drawGrid = true;
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private int _gridCount = 20;
        [SerializeField] private Color _gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        private void OnDrawGizmos()
        {
            if (!_drawGrid) return;

            Gizmos.color = _gridColor;
            float half = _gridCount * _gridSize / 2;

            for (int i = 0; i <= _gridCount; i++)
            {
                float pos = -half + i * _gridSize;
                Gizmos.DrawLine(new Vector3(pos, 0, -half), new Vector3(pos, 0, half));
                Gizmos.DrawLine(new Vector3(-half, 0, pos), new Vector3(half, 0, pos));
            }
        }

        /// <summary>
        /// 绘制边界框
        /// </summary>
        public void DrawBounds(Bounds bounds, Color color, float duration = 0.1f)
        {
            Debug.DrawLine(bounds.min, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), color, duration);
            Debug.DrawLine(bounds.min, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), color, duration);
            Debug.DrawLine(bounds.min, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), color, duration);
            // ... 其他边
        }

        /// <summary>
        /// 绘制路径
        /// </summary>
        public void DrawPath(List<Vector3> points, Color color, float duration = 0.1f)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Debug.DrawLine(points[i], points[i + 1], color, duration);
            }
        }
    }

    /// <summary>
    /// 游戏工具管理器 - 复刻原版 GameplayToolManager
    /// 管理游戏内道具/工具
    /// </summary>
    public class GameplayToolManager : MonoSingleton<GameplayToolManager>
    {
        [SerializeField] private GameObject[] _toolPrefabs;

        private Dictionary<int, ToolData> _ownedTools = new();
        private int _selectedToolId = -1;

        public class ToolData
        {
            public int Id;
            public string Name;
            public int Count;
            public float Cooldown;
            public float LastUsedTime;
            public GameObject Prefab;
        }

        public int SelectedToolId => _selectedToolId;

        /// <summary>
        /// 添加工具
        /// </summary>
        public void AddTool(int toolId, int count = 1)
        {
            if (!_ownedTools.ContainsKey(toolId))
            {
                _ownedTools[toolId] = new ToolData
                {
                    Id = toolId,
                    Count = 0,
                    Prefab = toolId < _toolPrefabs.Length ? _toolPrefabs[toolId] : null
                };
            }
            _ownedTools[toolId].Count += count;
        }

        /// <summary>
        /// 使用工具
        /// </summary>
        public bool UseTool(int toolId, Vector3 position)
        {
            if (!_ownedTools.ContainsKey(toolId)) return false;

            var tool = _ownedTools[toolId];
            if (tool.Count <= 0) return false;
            if (Time.time - tool.LastUsedTime < tool.Cooldown) return false;

            tool.Count--;
            tool.LastUsedTime = Time.time;

            // 实例化工具效果
            if (tool.Prefab != null)
            {
                Instantiate(tool.Prefab, position, Quaternion.identity);
            }

            return true;
        }

        /// <summary>
        /// 选择工具
        /// </summary>
        public void SelectTool(int toolId)
        {
            _selectedToolId = toolId;
        }

        /// <summary>
        /// 获取工具数量
        /// </summary>
        public int GetToolCount(int toolId)
        {
            return _ownedTools.ContainsKey(toolId) ? _ownedTools[toolId].Count : 0;
        }

        /// <summary>
        /// 获取所有工具
        /// </summary>
        public List<ToolData> GetAllTools()
        {
            return new List<ToolData>(_ownedTools.Values);
        }
    }

    /// <summary>
    /// 游戏射线检测器 - 复刻原版 GameplayRaycaster
    /// 处理游戏内的射线检测逻辑
    /// </summary>
    public class GameplayRaycaster : MonoSingleton<GameplayRaycaster>
    {
        [SerializeField] private LayerMask _coinLayer;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private float _maxDistance = 100f;

        /// <summary>
        /// 从屏幕坐标射线检测硬币
        /// </summary>
        public bool RaycastCoin(Vector2 screenPos, out CoinViewBase coin, Camera cam = null)
        {
            coin = null;
            if (cam == null) cam = Camera.main;
            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _coinLayer))
            {
                coin = hit.collider.GetComponent<CoinViewBase>();
                return coin != null;
            }
            return false;
        }

        /// <summary>
        /// 从屏幕坐标射线检测可交互物体
        /// </summary>
        public bool RaycastInteractable(Vector2 screenPos, out GameObject obj, Camera cam = null)
        {
            obj = null;
            if (cam == null) cam = Camera.main;
            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _interactableLayer))
            {
                obj = hit.collider.gameObject;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 范围检测硬币
        /// </summary>
        public CoinViewBase[] OverlapCoins(Vector3 center, float radius)
        {
            var colliders = Physics.OverlapSphere(center, radius, _coinLayer);
            var coins = new List<CoinViewBase>();

            foreach (var col in colliders)
            {
                var coin = col.GetComponent<CoinViewBase>();
                if (coin != null) coins.Add(coin);
            }

            return coins.ToArray();
        }

        /// <summary>
        /// 射线检测所有命中
        /// </summary>
        public RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float distance = -1)
        {
            if (distance < 0) distance = _maxDistance;
            return Physics.RaycastAll(origin, direction, distance);
        }
    }

    /// <summary>
    /// 连击系统 - 管理连击计数和奖励
    /// </summary>
    public class ComboSystem : MonoSingleton<ComboSystem>
    {
        [SerializeField] private float _comboWindow = 2f;
        [SerializeField] private int _maxCombo = 99;

        public int CurrentCombo { get; private set; }
        public float ComboMultiplier => 1f + CurrentCombo * 0.1f;

        private float _lastComboTime;

        public event System.Action<int> OnComboChanged;
        public event System.Action<int> OnComboMilestone; // 达到里程碑

        private void Update()
        {
            if (CurrentCombo > 0 && Time.time - _lastComboTime > _comboWindow)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// 增加连击
        /// </summary>
        public void AddCombo()
        {
            CurrentCombo = Mathf.Min(CurrentCombo + 1, _maxCombo);
            _lastComboTime = Time.time;
            OnComboChanged?.Invoke(CurrentCombo);

            // 里程碑检查
            if (CurrentCombo % 10 == 0)
            {
                OnComboMilestone?.Invoke(CurrentCombo);
            }
        }

        /// <summary>
        /// 重置连击
        /// </summary>
        public void ResetCombo()
        {
            if (CurrentCombo > 0)
            {
                CurrentCombo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        /// <summary>
        /// 计算带连击加成的分数
        /// </summary>
        public long CalculateScore(long baseScore)
        {
            return (long)(baseScore * ComboMultiplier);
        }
    }
}
