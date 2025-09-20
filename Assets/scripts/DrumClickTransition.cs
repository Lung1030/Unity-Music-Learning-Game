using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DrumClickTransition : MonoBehaviour
{
    [Header("過場設定")]
    public string nextSceneName = "NextScene";  // 下一個場景的名稱
    public float zoomDuration   = 1f;           // 鏡頭放大總時間
    public float targetFOV      = 30f;          // 目標鏡頭視野 (原本預設約 60)
    
    private Camera mainCam;
    private float originalFOV;
    private bool  isTransitioning = false;

    void Start()
    {
        mainCam      = Camera.main;
        originalFOV  = mainCam.fieldOfView;
    }

    // 只要是非 UI、非 EventSystem 的 3D Collider，就可以用 OnMouseDown
    void OnMouseDown()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(DoZoomAndLoad());
    }

    IEnumerator DoZoomAndLoad()
    {
        // 1. 鏡頭放大（FOV 由 originalFOV → targetFOV）
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            mainCam.fieldOfView = Mathf.Lerp(
                originalFOV, targetFOV, elapsed / zoomDuration
            );
            yield return null;
        }
        mainCam.fieldOfView = targetFOV;

        // 2. 可做短暫停頓，或 Fade to black

        // 3. 載入下一個場景
        SceneManager.LoadScene(nextSceneName);
    }
}