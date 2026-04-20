using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BeautifyGameOver
{
    [MenuItem("Tools/Embellecer Game Over")]
    public static void Beautify()
    {
        // Buscar el UI Canvas en la escena activa
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        Transform gameOverPanel = null;
        
        foreach (Canvas c in canvases)
        {
            gameOverPanel = c.transform.Find("Panel_GamerOver") ?? c.transform.Find("Panel_GameOver");
            if (gameOverPanel != null) break;
        }

        if (gameOverPanel == null)
        {
            Debug.LogError("No se encontró Panel_GamerOver en la escena activa.");
            return;
        }

        // 1. Configurar fondo oscuro translúcido
        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bgImage = gameOverPanel.GetComponent<Image>();
        if (bgImage == null) bgImage = gameOverPanel.gameObject.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.12f, 0.15f, 0.95f); // Fondo gris muy oscuro

        // 2. Darle orden vertical con espaciado
        VerticalLayoutGroup layout = gameOverPanel.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = gameOverPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 150, 50);
        layout.spacing = 35f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // 3. Arreglar todos los textos y botones internos
        TMP_Text[] texts = gameOverPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in texts)
        {
            // Evitar que la fuente 8-bit se rompa en dos lineas
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;
            t.alignment = TextAlignmentOptions.Center;
            
            RectTransform trt = t.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(800, 80);

            // Evitar el error amarillo del caracter 'ú'
            if (t.text.Contains("ú"))
            {
                t.text = t.text.Replace("ú", "u").Replace("Ú", "U");
                EditorUtility.SetDirty(t);
            }

            // Colorear y cambiar tamaños estéticamente
            string upperText = t.text.ToUpper();
            if (upperText.Contains("GAME OVER") || upperText.Contains("GAMEOVER"))
            {
                t.fontSize = 80;
                t.color = new Color(0.9f, 0.2f, 0.2f); // Rojo retro
                trt.sizeDelta = new Vector2(1000, 120);
            }
            else if (upperText.Contains("PUNTAJE") || upperText.Contains("TIEMPO"))
            {
                t.fontSize = 32;
                t.color = new Color(1f, 0.9f, 0.4f); // Amarillo
            }
            else if (t.gameObject.GetComponentInParent<Button>() != null)
            {
                t.fontSize = 36;
                t.color = Color.white;
                
                Button btn = t.gameObject.GetComponentInParent<Button>();
                if (btn != null)
                {
                    RectTransform btnRt = btn.GetComponent<RectTransform>();
                    btnRt.sizeDelta = new Vector2(600, 80);
                    
                    Image btnImg = btn.GetComponent<Image>();
                    if (btnImg == null) btnImg = btn.gameObject.AddComponent<Image>();
                    
                    // Fondo tipo botón de la vieja escuela
                    btnImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
                    btn.targetGraphic = btnImg;
                    EditorUtility.SetDirty(btn);
                }
            }
            else 
            {
                t.fontSize = 48; // Números de score
                t.color = Color.white;
            }
            EditorUtility.SetDirty(t);
        }

        EditorUtility.SetDirty(gameOverPanel.gameObject);
        Debug.Log("¡Panel de Game Over rediseñado con éxito!");
    }
}
