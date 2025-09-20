using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelsScroll : MonoBehaviour
{
    [Header("滑動元件")]
    public ScrollRect scrollRect;

    [Header("左右按鈕")]
    public Button LButton;
    public Button RButton;

    public GameObject LButtonView;
    public GameObject RButtonView;

    [Header("動畫速度")]
    public float moveSpeed = 10f;

    [Header("每次滑動距離")]
    public float stepSize = 1800f;

    [Header("圖片數量")]
    public int totalImages = 18;

    private RectTransform contentRect;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private int currentIndex = 0; // 現在是第幾張圖（0 ~ totalImages - 1）

    void Start()
    {
        if (scrollRect != null)
            contentRect = scrollRect.content;

        if (LButton != null)
            LButton.onClick.AddListener(OnLeftClick);
        if (RButton != null)
            RButton.onClick.AddListener(OnRightClick);

        // 初始位置
        targetPosition = contentRect.localPosition;
        UpdateButtonVisibility();
    }

    void Update()
    {
        if (isMoving && contentRect != null)
        {
            contentRect.localPosition = Vector3.Lerp(contentRect.localPosition, targetPosition, Time.deltaTime * moveSpeed);

            if (Vector3.Distance(contentRect.localPosition, targetPosition) < 0.1f)
            {
                contentRect.localPosition = targetPosition;
                isMoving = false;
            }
        }
    }

    void OnLeftClick()
    {
        if (currentIndex <= 0) return;

        currentIndex--;
        MoveToCurrentIndex();
    }

    void OnRightClick()
    {
        if (currentIndex >= totalImages - 1) return;

        currentIndex++;
        MoveToCurrentIndex();
    }

    void MoveToCurrentIndex()
    {
        // posX = currentIndex * -stepSize
        float newX = -currentIndex * stepSize;
        targetPosition = new Vector3(newX, contentRect.localPosition.y, contentRect.localPosition.z);
        isMoving = true;
        UpdateButtonVisibility();
    }

    void UpdateButtonVisibility()
    {
        LButtonView.SetActive(currentIndex > 0);
        RButtonView.SetActive(currentIndex < totalImages - 1);
    }
}
