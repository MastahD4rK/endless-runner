using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class CleanupMissingScripts : Editor
{
    [MenuItem("Tools/Clean Up Missing Scripts")]
    public static void CleanupMissingScriptsInSceneAndPrefabs()
    {
        // 1. Limpiar todas las escenas del Build Settings
        string currentScenePath = SceneManager.GetActiveScene().path;
        int totalSceneRemoved = 0;
        int scenesModified = 0;

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;

            Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Additive);
            var rootObjects = scene.GetRootGameObjects();
            int removedInThisScene = 0;

            foreach (var go in rootObjects)
            {
                removedInThisScene += CleanupMissingScriptsRecursive(go);
            }

            if (removedInThisScene > 0)
            {
                totalSceneRemoved += removedInThisScene;
                scenesModified++;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            // Si no era la escena originalmente activa, la cerramos
            if (scene.path != currentScenePath)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        if (totalSceneRemoved > 0)
        {
            Debug.Log($"[Cleanup] Se eliminaron {totalSceneRemoved} scripts faltantes en {scenesModified} escenas.");
        }

        // 2. Limpiar todos los prefabs del proyecto
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int totalPrefabRemoved = 0;
        int prefabsModified = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                int removed = CleanupMissingScriptsRecursive(prefab);
                if (removed > 0)
                {
                    totalPrefabRemoved += removed;
                    prefabsModified++;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        if (totalPrefabRemoved > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"[Cleanup] Se eliminaron {totalPrefabRemoved} scripts faltantes en {prefabsModified} prefabs.");
        }

        if (totalSceneRemoved == 0 && totalPrefabRemoved == 0)
        {
            Debug.Log("[Cleanup] No se encontraron scripts faltantes ni en la escena ni en los prefabs.");
        }
        else
        {
            Debug.Log("[Cleanup] ¡Limpieza total completada!");
        }
    }

    private static int CleanupMissingScriptsRecursive(GameObject go)
    {
        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        
        foreach (Transform child in go.transform)
        {
            removedCount += CleanupMissingScriptsRecursive(child.gameObject);
        }
        
        return removedCount;
    }
}
