using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NoteInfo : MonoBehaviour
{
    public string noteName; // 音符名稱
    public float requiredHoldTime = 1.0f; // 要求按住的時間

    public Image image; // UI Image，用來改變顏色與透明度

    /// <summary>
    /// 設定音符顏色
    /// </summary>
    public void SetColor(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }

    /// <summary>
    /// 呼叫這個方法會啟動淡出動畫
    /// </summary>
    public void HideImage()
    {
        if (image != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutImage(0.5f)); // 可自訂淡出時間（秒）
        }
    }

    /// <summary>
    /// 淡出協程
    /// </summary>
    private IEnumerator FadeOutImage(float duration)
    {
        float elapsed = 0f;
        Color originalColor = image.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 確保完全透明
    }
}