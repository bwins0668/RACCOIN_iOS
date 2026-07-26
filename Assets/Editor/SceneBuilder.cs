using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.IO;
using Raccoin.CoinPusher;
using Raccoin.Core;
using Raccoin.UI;
using Raccoin.UI.Title;

/// <summary>
/// 场景构建器 - 在 CI 构建时用 Unity API 程序化搭建所有游戏场景。
/// 由于无法在本地手工编辑场景, 所有场景内容(推币机桌面/硬币/UI)都在构建时生成,
/// 并自动把提取出的原版美术资源(精灵/字体)连线到对应组件上。
/// </summary>
public static class SceneBuilder
{
    private const string SpriteDir = "Assets/Resources/Sprites";
    private const string FontDir = "Assets/Resources/Fonts";
    private const string MatDir = "Assets/Resources/Materials";
    private const string PrefabDir = "Assets/Resources/Prefabs";

    // ===================== 主入口 =====================

    /// <summary>构建全部场景 (由 BuildScript.BuildIOS 在打包前调用)</summary>
    public static void BuildAllScenes()
    {
        Debug.Log("[SceneBuilder] ===== Building all scenes =====");

        EnsureCoinTag();
        EnsureFolders();
        AssetDatabase.Refresh();

        GameObject coinPrefab = CreateCoinPrefab();

        BuildInitScene();
        BuildTitleScene();
        BuildClassicScene(coinPrefab);
        BuildPlaceholderScene("Assets/Scenes/GameLab.unity", "LAB MODE", "Coming Soon");
        BuildPlaceholderScene("Assets/Scenes/GameRPG.unity", "RPG MODE", "Coming Soon");
        BuildPlaceholderScene("Assets/Scenes/GameKindle.unity", "KINDLE MODE", "Coming Soon");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SceneBuilder] ===== All scenes built =====");
    }

    // ===================== 基础设施 =====================

    private static void EnsureFolders()
    {
        foreach (var d in new[] { MatDir, PrefabDir })
            if (!AssetDatabase.IsValidFolder(d))
            {
                string parent = Path.GetDirectoryName(d).Replace('\\', '/');
                string name = Path.GetFileName(d);
                if (!AssetDatabase.IsValidFolder(parent)) Directory.CreateDirectory(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
    }

    /// <summary>确保 "Coin" 标签存在 (推币机物理检测依赖)</summary>
    private static void EnsureCoinTag()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == "Coin") return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Coin";
        tagManager.ApplyModifiedProperties();
        Debug.Log("[SceneBuilder] Added 'Coin' tag.");
    }

    /// <summary>加载精灵, 若导入类型不是 Sprite 则强制修正并重新导入</summary>
    private static Sprite LoadSprite(string name)
    {
        string path = $"{SpriteDir}/{name}.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// <summary>加载中文字体, 失败则回退内置 Arial</summary>
    private static Font LoadFont()
    {
        foreach (var name in new[] { "CoinPusherFont_CN", "LiberationSans", "CoinPusherFont_Latin" })
        {
            var f = AssetDatabase.LoadAssetAtPath<Font>($"{FontDir}/{name}.ttf");
            if (f != null) return f;
        }
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static Material CreateMaterial(string name, Color color, float metallic = 0f, float gloss = 0.5f)
    {
        string path = $"{MatDir}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        var mat = new Material(Shader.Find("Standard")) { color = color };
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", gloss);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // ===================== 硬币预制体 =====================

    private static GameObject CreateCoinPrefab()
    {
        string prefabPath = $"{PrefabDir}/Coin.prefab";

        var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.tag = "Coin";
        coin.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f); // 半径0.35 厚0.1

        coin.GetComponent<MeshRenderer>().sharedMaterial =
            CreateMaterial("CoinGold", new Color(1f, 0.82f, 0.25f), 0.85f, 0.55f);

        var col = coin.GetComponent<MeshCollider>();
        col.convex = true;
        var phys = new PhysicsMaterial("CoinPhys")
        {
            dynamicFriction = 0.15f,
            staticFriction = 0.15f,
            bounciness = 0.05f,
            frictionCombine = PhysicsMaterialCombine.Average,
            bounceCombine = PhysicsMaterialCombine.Average
        };
        col.sharedMaterial = phys;

        var rb = coin.AddComponent<Rigidbody>();
        rb.mass = 0.3f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.5f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        var prefab = PrefabUtility.SaveAsPrefabAsset(coin, prefabPath);
        Object.DestroyImmediate(coin);
        Debug.Log($"[SceneBuilder] Coin prefab created: {prefabPath}");
        return prefab;
    }

    // ===================== Init 场景 =====================

    private static void BuildInitScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateCamera("Main Camera", new Vector3(0, 1, -10), Quaternion.identity,
            new Color(0.02f, 0.02f, 0.05f));
        cam.tag = "MainCamera";
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Init.unity");
        Debug.Log("[SceneBuilder] Init scene built.");
    }

    // ===================== Title 场景 =====================

    private static void BuildTitleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateCamera("Main Camera", new Vector3(0, 1, -10), Quaternion.identity,
            new Color(0.05f, 0.05f, 0.12f));
        cam.tag = "MainCamera";

        Font font = LoadFont();
        var canvas = CreateCanvas("TitleCanvas");
        CreateEventSystem();

        // 背景 (全屏拉伸)
        var bg = CreateUI("Background", canvas.transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var bgImg = bg.gameObject.AddComponent<Image>();
        var bgSprite = LoadSprite("UI_BG_2");
        if (bgSprite != null) { bgImg.sprite = bgSprite; bgImg.type = Image.Type.Simple; }
        bgImg.color = new Color(0.10f, 0.12f, 0.25f);

        // LOGO
        var logo = CreateUI("Logo", canvas.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 180), Vector2.zero);
        var logoText = AddText(logo, "RACCOIN", font, 140, new Color(1f, 0.84f, 0.2f), TextAnchor.MiddleCenter);
        logoText.fontStyle = FontStyle.Bold;

        // 副标题
        var sub = CreateUI("Subtitle", canvas.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 70), Vector2.zero);
        AddText(sub, "浣 熊 推 币 机", font, 44, Color.white, TextAnchor.MiddleCenter);

        // 按钮
        var controllerGo = new GameObject("TitleScreenController");
        var controller = controllerGo.AddComponent<TitleScreenController>();

        var playBtn = CreateButton("PlayButton", canvas.transform, font, "开始游戏",
            new Vector2(0, -80), new Vector2(420, 100));
        var labBtn = CreateButton("LabButton", canvas.transform, font, "实验室模式",
            new Vector2(0, -210), new Vector2(420, 90));

        // 版本
        var ver = CreateUI("Version", canvas.transform,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(300, 50));
        var verText = AddText(ver, "v1.0.0", font, 28, new Color(1, 1, 1, 0.5f), TextAnchor.LowerRight);

        // 连线
        controller.PlayButton = playBtn;
        controller.LabButton = labBtn;
        controller.VersionText = verText;
        EditorUtility.SetDirty(controller);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Title.unity");
        Debug.Log("[SceneBuilder] Title scene built.");
    }

    // ===================== GameClassic 场景 (核心推币机) =====================

    private static void BuildClassicScene(GameObject coinPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 相机: 俯视推币桌面
        var cam = CreateCamera("Main Camera", new Vector3(0, 8.5f, -7.5f), Quaternion.identity,
            new Color(0.06f, 0.06f, 0.10f));
        cam.tag = "MainCamera";
        cam.transform.LookAt(new Vector3(0, 0, 1.5f));
        cam.fieldOfView = 55f;

        // 灯光
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.color = new Color(1f, 0.96f, 0.88f);
        lightGo.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

        // ---- 桌面 ----
        var tableMat = CreateMaterial("TableFelt", new Color(0.09f, 0.35f, 0.22f), 0f, 0.3f);
        var wallMat = CreateMaterial("TableWood", new Color(0.32f, 0.20f, 0.11f), 0f, 0.4f);
        var pusherMat = CreateMaterial("PusherMetal", new Color(0.85f, 0.72f, 0.35f), 0.9f, 0.6f);

        CreateStaticBox("TableFloor", new Vector3(0, -0.1f, 0), new Vector3(6.4f, 0.2f, 9f), tableMat);
        CreateStaticBox("WallLeft", new Vector3(-3.3f, 0.7f, 0), new Vector3(0.2f, 1.6f, 9f), wallMat);
        CreateStaticBox("WallRight", new Vector3(3.3f, 0.7f, 0), new Vector3(0.2f, 1.6f, 9f), wallMat);
        CreateStaticBox("WallBack", new Vector3(0, 0.7f, -4.6f), new Vector3(6.8f, 1.6f, 0.2f), wallMat);

        // ---- 推板 (往复推动硬币) ----
        var pusher = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pusher.name = "Pusher";
        pusher.transform.localScale = new Vector3(5.8f, 1.0f, 0.35f);
        pusher.transform.localPosition = new Vector3(0, 0.5f, -1.2f);
        pusher.GetComponent<MeshRenderer>().sharedMaterial = pusherMat;
        var pusherRb = pusher.AddComponent<Rigidbody>();
        pusherRb.isKinematic = true;
        var pusherController = pusher.AddComponent<PusherController>();
        SetSerialized(pusherController, so =>
        {
            so.FindProperty("_pusherTransform").objectReferenceValue = pusher.transform;
            so.FindProperty("_pushDistance").floatValue = 1.9f;
            so.FindProperty("_pushSpeed").floatValue = 0.42f;
        });

        // ---- 投币口 (3个落点, 位于推板前方的桌面上方) ----
        var coinEntryGo = new GameObject("CoinEntry");
        coinEntryGo.transform.localPosition = new Vector3(0, 3.5f, 0.3f);
        var coinEntry = coinEntryGo.AddComponent<CoinEntryController>();
        var spawnPoints = new Transform[3];
        float[] xs = { -1.6f, 0f, 1.6f };
        for (int i = 0; i < 3; i++)
        {
            var sp = new GameObject($"SpawnPoint_{i}").transform;
            sp.SetParent(coinEntryGo.transform, false);
            sp.localPosition = new Vector3(xs[i], 0f, 0f);
            spawnPoints[i] = sp;
        }
        SetSerialized(coinEntry, so =>
        {
            var arr = so.FindProperty("_spawnPoints");
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            so.FindProperty("_coinPrefab").objectReferenceValue = coinPrefab;
            so.FindProperty("_spawnCooldown").floatValue = 0.22f;
        });

        // ---- 结算区 (桌前沿下方, 顶面低于桌面避免误触发, 硬币掉落即得分) ----
        var settleGo = new GameObject("SettleArea");
        settleGo.transform.localPosition = new Vector3(0, -1.5f, 5.0f);
        var settleCol = settleGo.AddComponent<BoxCollider>();
        settleCol.isTrigger = true;
        settleCol.size = new Vector3(6.4f, 2.0f, 3.0f);
        settleGo.AddComponent<SettleAreaController>();

        // ---- 计分板 / 推币机主管理器 ----
        var scoreGo = new GameObject("ScoreBoard");
        var scoreBoard = scoreGo.AddComponent<ScoreBoardController>();

        var managerGo = new GameObject("CoinPusherManager");
        var manager = managerGo.AddComponent<CoinPusherManager>();
        SetSerialized(manager, so =>
        {
            so.FindProperty("_pusherController").objectReferenceValue = pusherController;
            so.FindProperty("_coinEntry").objectReferenceValue = coinEntry;
            so.FindProperty("_scoreBoard").objectReferenceValue = scoreBoard;
            so.FindProperty("_settleArea").objectReferenceValue = settleGo.GetComponent<SettleAreaController>();
        });

        // ---- 输入管理器 ----
        var inputGo = new GameObject("InputManager");
        inputGo.AddComponent<InputManager>();

        // ---- HUD ----
        Font font = LoadFont();
        var canvas = CreateCanvas("GameCanvas");
        CreateEventSystem();

        var topBar = CreateUI("TopBar", canvas.transform,
            new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 110));
        var topBarImg = topBar.gameObject.AddComponent<Image>();
        topBarImg.color = new Color(0, 0, 0, 0.45f);

        var scoreRt = CreateUI("ScoreText", topBar,
            new Vector2(0, 0), new Vector2(0.4f, 1), Vector2.zero, Vector2.zero);
        var scoreText = AddText(scoreRt, "000000", font, 52, new Color(1f, 0.9f, 0.3f), TextAnchor.MiddleCenter);

        var roundRt = CreateUI("RoundText", topBar,
            new Vector2(0.4f, 0), new Vector2(0.6f, 1), Vector2.zero, Vector2.zero);
        var roundText = AddText(roundRt, "ROUND 1", font, 40, Color.white, TextAnchor.MiddleCenter);

        var coinRt = CreateUI("CoinCountText", topBar,
            new Vector2(0.6f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var coinText = AddText(coinRt, "100", font, 52, new Color(0.4f, 1f, 0.6f), TextAnchor.MiddleCenter);

        var backBtn = CreateButton("BackButton", canvas.transform, font, "< 返回",
            new Vector2(30, -30), new Vector2(160, 70),
            new Vector2(0, 1), new Vector2(0, 1));

        // 提示文字
        var hintRt = CreateUI("Hint", canvas.transform,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 40), Vector2.zero);
        AddText(hintRt, "点击屏幕投币", font, 36, new Color(1, 1, 1, 0.7f), TextAnchor.MiddleCenter);

        // ---- 桥接器: 连接 输入/玩法/UI ----
        var bridgeGo = new GameObject("ClassicGameBridge");
        var bridge = bridgeGo.AddComponent<ClassicGameBridge>();
        bridge.CoinEntry = coinEntry;
        bridge.ScoreBoard = scoreBoard;
        bridge.CoinPrefab = coinPrefab;
        bridge.ScoreText = scoreText;
        bridge.CoinCountText = coinText;
        bridge.RoundText = roundText;
        bridge.BackButton = backBtn;
        EditorUtility.SetDirty(bridge);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameClassic.unity");
        Debug.Log("[SceneBuilder] GameClassic scene built.");
    }

    // ===================== 占位场景 =====================

    private static void BuildPlaceholderScene(string path, string title, string subtitle)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateCamera("Main Camera", new Vector3(0, 1, -10), Quaternion.identity,
            new Color(0.05f, 0.05f, 0.12f));
        cam.tag = "MainCamera";

        Font font = LoadFont();
        var canvas = CreateCanvas("Canvas");
        CreateEventSystem();
        var t = CreateUI("Title", canvas.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 80), Vector2.zero);
        AddText(t, title, font, 90, Color.white, TextAnchor.MiddleCenter);
        var s = CreateUI("Sub", canvas.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, -20), Vector2.zero);
        AddText(s, subtitle, font, 44, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleCenter);
        var back = CreateButton("BackButton", canvas.transform, font, "< 返回标题",
            new Vector2(0, -160), new Vector2(320, 90));

        var go = new GameObject("PlaceholderController");
        var ctrl = go.AddComponent<PlaceholderSceneController>();
        ctrl.BackButton = back;
        EditorUtility.SetDirty(ctrl);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[SceneBuilder] Placeholder scene built: {path}");
    }

    // ===================== 通用构件 =====================

    private static Camera CreateCamera(string name, Vector3 pos, Quaternion rot, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetPositionAndRotation(pos, rot);
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bgColor;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        go.AddComponent<AudioListener>();
        return cam;
    }

    private static GameObject CreateStaticBox(string name, Vector3 pos, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    /// <summary>创建 EventSystem (UI 按钮响应触摸的必要条件) - 使用新输入系统模块</summary>
    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static RectTransform CreateUI(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private static Text AddText(RectTransform rt, string content, Font font, int size,
        Color color, TextAnchor alignment)
    {
        var text = rt.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>创建带按钮 (默认居中锚点, 可自定义)</summary>
    private static Button CreateButton(string name, Transform parent, Font font, string label,
        Vector2 anchoredPos, Vector2 size, Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        var rt = CreateUI(name, parent, anchorMin ?? new Vector2(0.5f, 0.5f), anchorMax ?? new Vector2(0.5f, 0.5f),
            anchoredPos, size);
        var img = rt.gameObject.AddComponent<Image>();

        var normal = LoadSprite("UI_Button_OldLarge_Normal");
        var highlighted = LoadSprite("UI_Button_OldLarge_Highlighted");
        var pressed = LoadSprite("UI_Button_OldLarge_Pressed");
        if (normal != null) { img.sprite = normal; img.type = Image.Type.Sliced; }
        else img.color = new Color(0.2f, 0.45f, 0.8f);

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        if (highlighted != null && pressed != null)
        {
            var state = new SpriteState
            {
                highlightedSprite = highlighted,
                pressedSprite = pressed
            };
            btn.spriteState = state;
        }

        var labelRt = CreateUI("Label", rt, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        AddText(labelRt, label, font, 40, Color.white, TextAnchor.MiddleCenter);
        return btn;
    }

    /// <summary>通过 SerializedObject 设置私有 [SerializeField] 字段</summary>
    private static void SetSerialized(Object target, System.Action<SerializedObject> mutate)
    {
        var so = new SerializedObject(target);
        mutate(so);
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}

/// <summary>占位场景控制器 - 返回标题</summary>
public class PlaceholderSceneController : MonoBehaviour
{
    public Button BackButton;
    private void Start()
    {
        if (BackButton != null)
            BackButton.onClick.AddListener(() => SceneManager.LoadScene(1));
    }
}
