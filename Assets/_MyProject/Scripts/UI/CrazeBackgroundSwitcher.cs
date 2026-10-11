using UnityEngine;
using UnityEngine.UI;

namespace Craze.UI
{
    /// <summary>
    /// 菜单背景切换器：根据按钮悬停切换背景图。
    /// 图片按原始宽高比显示在屏幕左侧（高度撑满屏幕），右侧纯黑背景始终保持常显。
    /// 转场为 P5 风格：新图从「冲击缩放」快速回弹（OutBack）+ 交叉淡入。
    /// 渲染顺序（MainMenu 内）：BG_Black(纯黑常显) → 背景图 → Red_Block/按钮 等。
    /// </summary>
    public class CrazeBackgroundSwitcher : MonoBehaviour
    {
        [Header("转场参数")]
        public float fadeTime = 0.28f;     // 淡入时长
        public float scaleTime = 0.5f;     // 缩放回弹时长
        public float punchScale = 1.12f;   // 初始冲击缩放

        [Header("资源路径")]
        public string resourceFolder = "Pictures";  // Resources 下的图片文件夹

        Image baseImage;
        Image frontImage;
        CanvasGroup baseCG;
        CanvasGroup frontCG;
        RectTransform baseRT;
        RectTransform frontRT;
        Sprite[] sprites;
        int current = -1;

        /// <summary>初始化：传入底层 Image（可为 null 则自动创建）、图片名数组和可选的文件夹路径。</summary>
        public void Setup(Image baseImg, string[] resourceNames, string folder = null)
        {
            if (!string.IsNullOrEmpty(folder)) resourceFolder = folder;

            // 黑底引用（只用于定位父节点和层级；黑底本身始终保持常显，不再压暗）
            Transform black = transform.Find("BG_Black");

            // 背景图挂到 BG_Black 的同一父节点（MainMenu）并紧跟在 BG_Black 之后，
            // 这样纯黑背景在底层常显，背景图盖在它上面，按钮/标题又在最上层。
            Transform imgParent = black != null ? black.parent : transform;
            int insertIndex = black != null ? black.GetSiblingIndex() + 1 : 0;

            // 底层（显示当前图）
            baseImage = baseImg != null ? baseImg : CreateLeftImage("BG_Image", imgParent, insertIndex);
            baseCG = EnsureCG(baseImage.gameObject);
            baseRT = baseImage.rectTransform;
            baseCG.alpha = 0f;

            // 前景层（转场用，覆盖在底层之上）
            frontImage = CreateLeftImage("BG_Image_Front", imgParent, insertIndex + 1);
            frontCG = EnsureCG(frontImage.gameObject);
            frontRT = frontImage.rectTransform;
            frontCG.alpha = 0f;

            // 加载图片
            sprites = new Sprite[resourceNames.Length];
            for (int i = 0; i < resourceNames.Length; i++)
                sprites[i] = LoadSprite(resourceNames[i]);
        }

        /// <summary>切换到第 index 张背景图（带转场）。</summary>
        public void Show(int index)
        {
            if (sprites == null || index < 0 || index >= sprites.Length) return;
            if (index == current) return;
            if (sprites[index] == null) return;
            current = index;

            // 前景层载入新图，从「冲击缩放 + 透明」开始
            frontImage.sprite = sprites[index];
            ApplyLeftLayout(frontImage, sprites[index]);
            frontCG.alpha = 0f;
            frontRT.localScale = new Vector3(punchScale, punchScale * 0.96f, 1f);

            CrazeTween.Fade(frontCG, 1f, fadeTime, Easing.OutCubic);
            CrazeTween.ScaleTo(frontRT, Vector3.one, scaleTime, Easing.OutBack, 0f, Commit);
        }

        // 转场结束：把新图固化到底层，前景层复位，保证下一次转场可用
        void Commit()
        {
            baseImage.sprite = frontImage.sprite;
            ApplyLeftLayout(baseImage, baseImage.sprite);
            baseCG.alpha = 1f;
            frontCG.alpha = 0f;
            frontRT.localScale = Vector3.one;
        }

        // 左侧布局：高度撑满屏幕，宽度 = 高度 × 图片原始宽高比，图片左边缘贴屏幕左边缘
        void ApplyLeftLayout(Image img, Sprite sp)
        {
            if (img == null || sp == null) return;
            var canvas = img.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Vector2 canvasSize = ((RectTransform)canvas.transform).rect.size;
            float aspect = sp.rect.width / sp.rect.height;
            float h = canvasSize.y;
            float w = h * aspect;

            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(-canvasSize.x * 0.5f + w * 0.5f, 0f);
            rt.localScale = Vector3.one;
        }

        Image CreateLeftImage(string name, Transform parent, int siblingIndex)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(siblingIndex);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;   // 图片不拉伸变形（尺寸已由 ApplyLeftLayout 按比例设定）
            return img;
        }

        static CanvasGroup EnsureCG(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
            return cg;
        }

        Sprite LoadSprite(string name)
        {
            string path = resourceFolder + "/" + name;

            // 优先按 Sprite 加载；若导入类型不是 Sprite，则用 Texture2D 动态创建
            var s = Resources.Load<Sprite>(path);
            if (s != null) return s;

            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);

            return null;
        }
    }
}
