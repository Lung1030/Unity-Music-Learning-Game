using UnityEngine;
using UnityEngine.UI;

public class CircularScroll : MonoBehaviour
{
    [Header("滑動元件")]
    public ScrollRect scrollRect;

    [Header("左右按鈕")]
    public Button LButton;
    public Button RButton;

    [Header("動畫速度")]
    public float lerpSpeed = 5f;

    private float targetPosition = 0f;
    private bool isLerping = false;

    void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            targetPosition = 0f;
        }

        if (LButton != null)
            LButton.onClick.AddListener(OnLeftClick);
        if (RButton != null)
            RButton.onClick.AddListener(OnRightClick);
    }

    void Update()
    {
        if (isLerping && scrollRect != null)
        {
            float current = scrollRect.horizontalNormalizedPosition;
            float next = Mathf.Lerp(current, targetPosition, Time.deltaTime * lerpSpeed);
            scrollRect.horizontalNormalizedPosition = next;

            if (Mathf.Abs(current - targetPosition) < 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = targetPosition;
                isLerping = false;

                // 滑動結束時恢復按鈕可點
                SetButtonInteractable(true);
            }
        }
    }

    void OnLeftClick()
    {
        if (scrollRect == null || isLerping) return; // 正在滑動時禁止重複觸發

        float pos = scrollRect.horizontalNormalizedPosition;

        if (pos - 0.2f <= 0f)
            targetPosition = 0.99f;
        else
            targetPosition = Mathf.Clamp01(pos - 0.33f);

        isLerping = true;
        SetButtonInteractable(false); // 禁止按鈕在動畫中被按
    }

    void OnRightClick()
    {
        if (scrollRect == null || isLerping) return; // 正在滑動時禁止重複觸發

        float pos = scrollRect.horizontalNormalizedPosition;

        if (pos + 0.2f >= 1f)
            targetPosition = 0f;
        else
            targetPosition = Mathf.Clamp01(pos + 0.33f);

        isLerping = true;
        SetButtonInteractable(false); // 禁止按鈕在動畫中被按
    }

    // 控制左右按鈕是否可互動
    private void SetButtonInteractable(bool interactable)
    {
        if (LButton != null) LButton.interactable = interactable;
        if (RButton != null) RButton.interactable = interactable;
    }
}
