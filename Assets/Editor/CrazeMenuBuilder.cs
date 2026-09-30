using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;
using Craze.UI;

namespace Craze.EditorTools
{
    /// <summary>
    /// 一键生成 P5 风格主菜单 UI。
    /// 菜单入口：Tools > Craze > Build Main Menu
    /// 会在当前场景自动创建 / 复用 Canvas 与 EventSystem，并生成红黑白的斜切卡片层级。
    /// </summary>
    public static class CrazeMenuBuilder
    {
        const string MENU = "Tools/Craze/Main Menu";

        [MenuItem(MENU, false, 1)]
        public static void Build()
        {
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();

            // 用生成器创建整棵层级
            GameObject root = CreateMenuRoot(canvas.transform);

            // 挂控制器并填充字段
            AttachController(root);

            Undo.RegisterCreatedObjectUndo(root, "Build Craze Main Menu");
            Selection.activeGameObject = root;
            Debug.Log("[CRAZE] 主菜单已生成: " + root.name);
        }

        // ---------------- 基础组件 ----------------
        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "EventSystem");
        }

        static Canvas EnsureCanvas()
        {
            Canvas existing = Object.FindObjectOfType<Canvas>();
            if (existing != null) return existing;

            var go = new GameObject("CrazeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Undo.RegisterCreatedObjectUndo(go, "CrazeCanvas");
            return canvas;
        }

        // ---------------- 生成主菜单层级 ----------------
        static GameObject CreateMenuRoot(Transform parent)
        {
            GameObject root = new GameObject("MainMenu",
                typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);

            // 背景大色块（黑）
            GameObject bg = CreatePanel(root.transform, "BG_Black",
                new Vector2(1920, 1080), new Vector2(0, 0), Color.black, 0.95f);
            // 右上红色斜切撞色块
            GameObject redBlock = CreatePanel(root.transform, "Red_Block",
                new Vector2(900, 900), new Vector2(520, 520),
                new Color(0.85f, 0.05f, 0.1f, 1f), 1f, 0.12f);
            // 左侧浅色斜切块（放标题）
            GameObject titleBlock = CreatePanel(root.transform, "Title_Block",
                new Vector2(1000, 380), new Vector2(-430, 180),
                new Color(0.95f, 0.95f, 0.95f, 0.92f), 1f, 0.10f);

            // 标题文字
            GameObject title = CreateText(titleBlock.transform, "Title",
                "CRAZE", 150f, Color.black, FontStyles.Bold | FontStyles.Italic);
            RectTransform titleRt = title.GetComponent<RectTransform>();
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = Vector3.zero;
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.5f);

            // 副标题
            GameObject sub = CreateText(titleBlock.transform, "SubTitle",
                "A Stylized UI Demo", 42f, new Color(0.2f, 0.2f, 0.2f, 1f), FontStyles.Italic);
            RectTransform subRt = sub.GetComponent<RectTransform>();
            subRt.anchorMin = subRt.anchorMax = new Vector2(0.5f, 0.5f);
            subRt.anchoredPosition = new Vector2(0f, -110f);

            // 按钮组（右下）
            string[] names = { "Btn_NewGame", "Btn_Continue", "Btn_Settings", "Btn_Quit" };
            Button[] buttons = new Button[names.Length];
            CanvasGroup[] groups = new CanvasGroup[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                GameObject btn = CreateButton(root.transform, names[i],
                    new Vector2(360, 84), new Vector2(420, -170 - i * 110),
                    new Color(0.9f, 0.05f, 0.1f, 1f), 0.08f);
                buttons[i] = btn.GetComponent<Button>();
                groups[i] = btn.GetComponent<CanvasGroup>();
            }

            // 返回根节点部件引用
            var rootRect = root.GetComponent<RectTransform>();
            widgetRefs = new WidgetRefs
            {
                panelMain = rootRect,
                titleBlock = titleBlock.GetComponent<RectTransform>(),
                titleText = title.GetComponent<TextMeshProUGUI>(),
                titleGroup = title.AddComponent<CanvasGroup>(),
                buttons = buttons,
                buttonGroups = groups,
            };

            return root;
        }

        static WidgetRefs widgetRefs;

        struct WidgetRefs
        {
            public RectTransform panelMain;
            public RectTransform titleBlock;
            public TextMeshProUGUI titleText;
            public CanvasGroup titleGroup;
            public Button[] buttons;
            public CanvasGroup[] buttonGroups;
        }

        static void AttachController(GameObject root)
        {
            var ctrl = root.AddComponent<CrazeMenuController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("panelMain").objectReferenceValue = widgetRefs.panelMain;
            so.FindProperty("titleBlock").objectReferenceValue = widgetRefs.titleBlock;
            so.FindProperty("titleText").objectReferenceValue = widgetRefs.titleText;
            so.FindProperty("titleGroup").objectReferenceValue = widgetRefs.titleGroup;
            so.FindProperty("buttons").arraySize = widgetRefs.buttons.Length;
            so.FindProperty("buttonGroups").arraySize = widgetRefs.buttonGroups.Length;
            for (int i = 0; i < widgetRefs.buttons.Length; i++)
            {
                so.FindProperty("buttons").GetArrayElementAtIndex(i).objectReferenceValue = widgetRefs.buttons[i];
                so.FindProperty("buttonGroups").GetArrayElementAtIndex(i).objectReferenceValue = widgetRefs.buttonGroups[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ctrl);
        }

        // ---------------- 通用创建辅助 ----------------
        static Shader DiagonalShader =>
            Shader.Find("Craze/UI/Diagonal") ?? Shader.Find("UI/Default");

        static Material MakeDiagMaterial(float bevel)
        {
            var mat = new Material(DiagonalShader) { name = "CrazeDiagMat" };
            if (mat.HasProperty("_Bevel")) mat.SetFloat("_Bevel", bevel);
            if (mat.HasProperty("_CutAmount")) mat.SetFloat("_CutAmount", 0f);
            return mat;
        }

        static GameObject CreatePanel(Transform parent, string name, Vector2 size,
            Vector2 anchoredPos, Color color, float alpha, float bevel = 0f)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image img = go.AddComponent<Image>();
            img.color = new Color(color.r, color.g, color.b, alpha);
            img.raycastTarget = false;
            if (bevel > 0f)
                img.material = MakeDiagMaterial(bevel);
            return go;
        }

        static GameObject CreateText(Transform parent, string name, string text,
            float size, Color color, FontStyles style = FontStyles.Normal)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 150);
            rt.pivot = new Vector2(0.5f, 0.5f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = false;
            return go;
        }

        static GameObject CreateButton(Transform parent, string name, Vector2 size,
            Vector2 anchoredPos, Color color, float bevel)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image img = go.AddComponent<Image>();
            img.color = new Color(color.r, color.g, color.b, 0.92f);
            img.raycastTarget = true;
            if (bevel > 0f)
                img.material = MakeDiagMaterial(0.08f);

            Button btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // 子文本
            GameObject label = CreateText(go.transform, "Label", name.Replace("Btn_", ""),
                46f, Color.white, FontStyles.Bold);
            RectTransform labelRt = label.GetComponent<RectTransform>();
            labelRt.sizeDelta = new Vector2(size.x, size.y);
            labelRt.anchorMin = labelRt.anchorMax = new Vector2(0.5f, 0.5f);
            labelRt.anchoredPosition = Vector3.zero;
            return go;
        }
    }
}
