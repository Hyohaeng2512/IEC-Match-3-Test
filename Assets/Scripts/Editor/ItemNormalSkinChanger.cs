#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class ItemNormalSkinChanger
{
    [MenuItem("Tools/Change ItemNormal Skins")]
    public static void ChangeSkins()
    {
        string prefabsFolderPath = Constants.PREFAB_LOCATION_PATH;
        string spritesFolderPath = Constants.NEW_SKIN_LOCATION_PATH;

        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesFolderPath });
        Sprite[] sprites = spriteGuids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(s => s.name, System.StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("Spirte Not Found In " + spritesFolderPath);
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("itemNormal t:Prefab", new[] { prefabsFolderPath });
        GameObject[] prefabs = prefabGuids
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(go => go.name, System.StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (prefabs.Length == 0)
        {
            Debug.LogError("Spirte Not Found In  " + prefabsFolderPath);
            return;
        }

        int count = Mathf.Min(sprites.Length, prefabs.Length);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[i];
            Sprite sprite = sprites[i];

            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);

                Debug.Log($"Apply {sprite.name} to {prefab.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Done, Reskin for {count} prefab itemNormal.");
    }
}
#endif
