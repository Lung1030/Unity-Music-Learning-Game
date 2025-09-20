using UnityEngine;
using UnityEngine.UI; // 如果用 TextMeshPro 改成 using TMPro;

public class TextFadeIn : MonoBehaviour
{
    public Text textUI;               // Legacy Text
    public float fadeDuration = 2f;   // 淡入時間
    public bool instantSwitch = false;// 勾選後啟用忽然顯示模式（循環顯示/隱藏）
    public float instantDelay = 1f;   // 顯示與隱藏的延遲秒數

    private void Start()
    {
        if (instantSwitch)
        {
            // 循環切換透明/顯示
            StartCoroutine(InstantBlink());
        }
        else
        {
            // 從透明開始淡入
            Color color = textUI.color;
            color.a = 0f;
            textUI.color = color;
            StartCoroutine(FadeInText());
        }
    }

    private System.Collections.IEnumerator FadeInText()
    {
        float time = 0f;
        Color color = textUI.color;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            textUI.color = color;
            yield return null;
        }
        color.a = 1f;
        textUI.color = color;
    }

    private System.Collections.IEnumerator InstantBlink()
    {
        while (true)
        {
            // 顯示
            SetAlpha(1f);
            yield return new WaitForSeconds(instantDelay);

            // 透明
            SetAlpha(0f);
            yield return new WaitForSeconds(instantDelay);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = textUI.color;
        color.a = alpha;
        textUI.color = color;
    }
}
