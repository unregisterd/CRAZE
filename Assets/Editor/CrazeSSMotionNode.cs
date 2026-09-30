using UnityEditor;
using UnityEngine;

namespace Craze.EditorTools
{
    /// <summary>
    /// 剑盾动画（SS_）默认没设置 motionNodeName，导致 root motion 位移不生效、移动卡住。
    /// 本工具把 SwordAndShield 目录下所有动画的 motionNodeName 设为 "root"。
    /// 菜单：Tools > Craze > Set SS Animations Root Motion
    /// </summary>
    public static class CrazeSSMotionNode
    {
        const string SSAnimDir = "Assets/AmplifyAnimationPack/Animations/SwordAndShield";

        [MenuItem("Tools/Craze/Set SS Animations Root Motion", false, 52)]
        public static void SetRootMotion()
        {
            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { SSAnimDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                bool changed = false;

                // ① 设置 root motion 节点
                if (string.IsNullOrEmpty(importer.motionNodeName))
                {
                    importer.motionNodeName = "root";
                    changed = true;
                }

                // ② 设置循环播放
                var clips = importer.defaultClipAnimations;
                if (clips != null && clips.Length > 0)
                {
                    foreach (var clip in clips)
                    {
                        if (!clip.loopTime)
                        {
                            clip.loopTime = true;
                            changed = true;
                        }
                    }
                    if (changed) importer.clipAnimations = clips;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }

            Debug.Log($"[CRAZE] 已为 {count} 个剑盾动画设置 root motion 节点 + 循环播放。" +
                      "等导入完成后 Play 测试移动。");
        }
    }
}
