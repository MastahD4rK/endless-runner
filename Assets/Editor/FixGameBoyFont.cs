using UnityEngine;
using UnityEditor;
using TMPro;

public class FixGameBoyFont : EditorWindow
{
    [MenuItem("Tools/Corregir Fuente GameBoy Automáticamente")]
    public static void FixEverything()
    {
        // ¡Cambiado! El archivo correcto que tú generaste se llama "Early GameBoy.asset", 
        // no el "Early GameBoy SDF.asset" corrupto que estaba seleccionado antes.
        string fontAssetPath = "Assets/Art/Early GameBoy.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        
        if (fontAsset != null)
        {
            if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
            {
                foreach (Texture2D tex in fontAsset.atlasTextures)
                {
                    if (tex != null)
                    {
                        tex.filterMode = FilterMode.Point;
                        EditorUtility.SetDirty(tex);
                    }
                }
            }
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();

            TMP_Text[] allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
            int updatedCount = 0;
            foreach (TMP_Text txt in allTexts)
            {
                if ((txt.hideFlags & HideFlags.NotEditable) != 0 || (txt.hideFlags & HideFlags.HideAndDontSave) != 0)
                    continue;

                // Enlazamos la fuente y el material CORRECTAMENTE
                txt.font = fontAsset;
                txt.fontSharedMaterial = fontAsset.material;

                txt.ForceMeshUpdate(true, true);
                txt.SetAllDirty();
                EditorUtility.SetDirty(txt);
                updatedCount++;
            }
            
            Debug.Log("[Éxito] Se conectó la fuente correcta a " + updatedCount + " textos.");
            EditorUtility.DisplayDialog("¡Ahora SÍ!", "El problema era que tenías dos archivos de fuente generados y mi script estaba enlazando el corrupto.\n\nYa enlacé el bueno ('Early GameBoy.asset'). Revisa tus menús.", "¡A jugar!");
        }
        else
        {
            Debug.LogError("No se encontró 'Early GameBoy.asset' en la carpeta Assets/Art/.");
            EditorUtility.DisplayDialog("Error", "No encontré el archivo 'Early GameBoy.asset'.", "Ok");
        }
    }
}
