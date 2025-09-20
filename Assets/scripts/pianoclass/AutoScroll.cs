using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 0.03f;
    public Button settingButton;
    public Button closeButton;

    public static bool isScrolling = true;

    void Start()
    {
        settingButton.onClick.AddListener(PauseScroll);
        closeButton.onClick.AddListener(ResumeScroll);
    }

    void Update()
    {
        if (isScrolling && scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition += scrollSpeed * Time.deltaTime;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
        }
    }

    public void PauseScroll()
    {
        isScrolling = false;
        JudgeLine.isPaused = true; // ✅ 同步暫停判斷時間
    }

    public void ResumeScroll()
    {
        isScrolling = true;
        JudgeLine.isPaused = false; // ✅ 同步恢復時間
    }
}
