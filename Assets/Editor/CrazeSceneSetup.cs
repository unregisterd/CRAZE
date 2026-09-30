using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Craze.EditorTools
{
    /// <summary>
    /// 场景辅助工具：
    ///   1. Tools > Craze > Create Test Scene (Scene_1) —— 创建带相机/平行光的测试场景并注册
    ///   2. Tools > Craze > Add All Scenes To Build Settings —— 把 Assets/Scenes 下场景全部注册
    /// </summary>
    public static class CrazeSceneSetup
    {
        const string ScenesFolder = "Assets/Scenes";
        const string TestScenePath = "Assets/Scenes/Scene_1.unity";

        [MenuItem("Tools/Craze/Create Test Scene (Scene_1)", false, 20)]
        public static void CreateTestScene()
        {
            if (File.Exists(TestScenePath))
            {
                if (!EditorUtility.DisplayDialog("CRAZE",
                    "Scene_1 已存在，是否覆盖重建？（会丢失其中内容）", "覆盖", "取消"))
                    return;
            }

            // 提示保存当前场景，避免未保存改动丢失
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // 创建含 Main Camera + Directional Light 的默认场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, TestScenePath);
            AssetDatabase.Refresh();

            RegisterAllScenes();
            Debug.Log("[CRAZE] 已创建测试场景: " + TestScenePath + "，并注册到 Build Settings。");
        }

        [MenuItem("Tools/Craze/Add All Scenes To Build Settings", false, 21)]
        public static void RegisterAllScenes()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                Debug.LogWarning("[CRAZE] 找不到目录: " + ScenesFolder);
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder });
            var paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".unity"))
                .OrderBy(p => p.EndsWith("Scene_0.unity") ? 0 : 1) // Scene_0 置顶作为启动场景
                .ToList();

            var list = paths.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            EditorBuildSettings.scenes = list;

            Debug.Log($"[CRAZE] 已注册 {list.Length} 个场景到 Build Settings：\n" + string.Join("\n", paths));
        }
    }
}
