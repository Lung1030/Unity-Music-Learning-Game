using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PianoImageClick : MonoBehaviour
{
    public RectTransform pianoImage;  // 設定你的Image物件
    public float scaleDuration = 0.3f;
    public float targetScale = 1.5f;
    public ScenceChange sceneChange;  // 你的原本腳本

    public void OnPianoClick()
    {
        StartCoroutine(ScaleAndGo());
    }

    private IEnumerator ScaleAndGo()
    {
        Vector3 startScale = pianoImage.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        // scale動畫
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            pianoImage.localScale = Vector3.Lerp(startScale, endScale, elapsed / scaleDuration);
            yield return null;
        }
        pianoImage.localScale = endScale;

        // 鏡頭縮放 & 換場
        sceneChange.GoToPianoChoose();
    }
}