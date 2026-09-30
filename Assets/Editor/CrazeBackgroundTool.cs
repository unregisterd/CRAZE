using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Craze.EditorTools
{
    /// <summary>
    /// 把 Resources/Pictures 下的图片一键设为 Scene_0 菜单背景。
    /// 用法：打开 Scene_0，菜单 Tools > Craze > Apply Menu Background Image。
    /// 会自动把图片导入类型改为 Sprite，并作为最底层铺满屏幕。
    /// </summary>
    public static class CrazeBackgroundTool
    {
        const string ImagePath = "Assets/Resources/Pictures/one.jpeg";

        [MenuItem("Tools/Craze/Apply Menu Background Image", false, 30)]
        public static void ApplyBackground()
        {
            // ① 确保图片以 Sprite 方式导入
            var importer = AssetImporter.GetAtPath(ImagePath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[CRAZE] 找不到图片: {ImagePath}");
                return;
            }

            if (importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ImagePath);
            if (sprite == null)
            {
                Debug.LogError($"[CRAZE] 无法加载为 Sprite: {ImagePath}");
                return;
            }

            // ② 找到当前场景的 Canvas / MainMenu
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[CRAZE] 当前场景找不到 Canvas，请先在 Scene_0 里运行。");
                return;
            }

            Transform menu = canvas.transform.Find("MainMenu");
            Transform parent = menu != null ? menu : canvas.transform;

            // ③ 创建（或复用）背景 Image，置于最底层
            Transform bg = parent.Find("BG_Image");
            GameObject go;
            if (bg == null)
            {
                go = new GameObject("BG_Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = bg.gameObject;
            }
            go.transform.SetSiblingIndex(0);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;   // 铺满；想保持比例可改 true
            img.raycastTarget = false;
            img.color = Color.white;

            // ④ 把原来的纯黑底调成半透明压暗层，让图片透出来
            Transform black = parent.Find("BG_Black");
            if (black != null)
            {
                var bi = black.GetComponent<Image>();
                if (bi != null) bi.color = new Color(0f, 0f, 0f, 0.35f);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[CRAZE] 背景图已应用并保存场景。");
        }
    }
}
