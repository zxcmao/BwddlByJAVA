using UnityEngine;
using UnityEditor;

public class ObjectFlagChecker 
{/*
    [MenuItem("Tools/Check HideFlags of All Objects")]
    public static void CheckAllHideFlags()
    {
        // 获取所有场景对象（包括Prefab实例和场景中的对象）
        var allObjects = FindObjectsOfType<UnityEngine.Object>();

        foreach (var obj in allObjects)
        {
            // 输出对象的名称和 HideFlags
            CheckHideFlags(obj);
        }
    }

    // 检查一个对象的 HideFlags
    public static void CheckHideFlags(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            // 输出对象的名称和 HideFlags
            if ((obj.hideFlags & HideFlags.DontSaveInEditor) != 0)
            {
                Debug.LogWarning($"Object '{obj.name}' has 'DontSaveInEditor' flag. This might cause issues when trying to save it as an asset.");
            }
            else
            {
                Debug.Log($"Object '{obj.name}' has HideFlags: {obj.hideFlags}");
            }
        }
    }

    // 查找并显示哪些对象被错误地添加到资源中
    public static void CheckObjectsInAssetDatabase()
    {
        // 你可以进一步查找和验证哪些对象正在被添加到 AssetDatabase 中
        // 例如检查FontAsset或其他资源的处理

        // 这是一个示例，验证 FontAsset 是否被错误处理
        var fontAssets = Resources.FindObjectsOfTypeAll<UnityEngine.TextCore.Text.FontAsset>();

        foreach (var fontAsset in fontAssets)
        {
            if (fontAsset != null && (fontAsset.hideFlags & HideFlags.DontSaveInEditor) != 0)
            {
                Debug.LogWarning($"FontAsset '{fontAsset.name}' has 'DontSaveInEditor' flag. This might cause issues when added to asset.");
            }
        }
    }

    // 在场景加载时检查所有对象
    [InitializeOnLoadMethod]
    static void OnSceneLoaded()
    {
        Debug.Log("Checking all objects in scene for HideFlags...");
        CheckAllHideFlags();
        CheckObjectsInAssetDatabase();
    }*/
}