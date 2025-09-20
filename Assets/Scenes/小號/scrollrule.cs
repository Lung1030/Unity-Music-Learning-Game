using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class scrollrule : MonoBehaviour
{
    [Header("滑動元件")]
    public RectTransform contentRect; // 要移動的內容

    [Header("左右按鈕")]
    public Button LButton;
    public Button RButton;

    public GameObject LButtonView;
    public GameObject RButtonView;

    [Header("每頁寬度")]
    public float step = 1800f;         // 每頁寬度（視 UI 而定）

    private int currentIndex = 0;      // 當前頁面索引
    private int maxIndex = 0;          // 最大索引（將依據內容長度自動設定）

    void Start()
    {
        if (contentRect == null)
        {
            Debug.LogError("Content Rect 未設定！");
            return;
        }

        // 計算最大索引
        float totalWidth = contentRect.rect.width;
        float viewWidth = ((RectTransform)contentRect.parent).rect.width;
        maxIndex = Mathf.Max(0, Mathf.FloorToInt((totalWidth - viewWidth) / step));

        // 初始化位置
        contentRect.anchoredPosition = new Vector2(0f, contentRect.anchoredPosition.y);

        if (LButton != null)
            LButton.onClick.AddListener(OnLeftClick);

        if (RButton != null)
            RButton.onClick.AddListener(OnRightClick);

        UpdateButtonVisibility();
    }

    void OnLeftClick()
    {
        Debug.Log("點擊 ← 左鍵");
        if (currentIndex <= 0) return;

        currentIndex--;
        contentRect.anchoredPosition = new Vector2(-step * currentIndex, contentRect.anchoredPosition.y);
        UpdateButtonVisibility();
    }

    void OnRightClick()
    {
        Debug.Log("點擊 → 右鍵");
        if (currentIndex >= maxIndex) return;

        currentIndex++;
        contentRect.anchoredPosition = new Vector2(-step * currentIndex, contentRect.anchoredPosition.y);
        UpdateButtonVisibility();
    }

    void UpdateButtonVisibility()
    {
        if (LButtonView != null)
            LButtonView.SetActive(currentIndex > 0);

        if (RButtonView != null)
            RButtonView.SetActive(currentIndex < maxIndex);

        Debug.Log($"目前索引：{currentIndex} / 最大索引：{maxIndex}");
    }
}
