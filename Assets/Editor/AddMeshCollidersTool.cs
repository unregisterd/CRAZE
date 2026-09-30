#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class AddMeshCollidersTool
{
    [MenuItem("Tools/环境/给选中物体及子物体添加 MeshCollider")]
    public static void AddMeshColliders()
    {
        foreach (var go in Selection.gameObjects)
            AddRecursive(go);
    }

    private static void AddRecursive(GameObject go)
    {
        var mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null && go.GetComponent<Collider>() == null)
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            mc.convex = false; // 静态环境用 false；移动物体改成 true 或换基本碰撞体
        }

        foreach (Transform child in go.transform)
            AddRecursive(child.gameObject);
    }
}
#endif