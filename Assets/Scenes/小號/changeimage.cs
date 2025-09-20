using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class changeimage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite originalImage;     // 原始圖片
    public Sprite pressedImage;      // 按下圖片

    private Image buttonImage;       // Image 元件
    private RectTransform rectTrans; // RectTransform 組件

    // 儲存原始位置與尺寸
    private Vector2 originalAnchoredPos;
    private Vector2 originalSize;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        rectTrans = GetComponent<RectTransform>();

        if (buttonImage == null || rectTrans == null)
        {
            Debug.LogError("需要 Image 與 RectTransform 組件！");
            return;
        }

        // 儲存原始狀態
        originalAnchoredPos = rectTrans.anchoredPosition;
        originalSize = rectTrans.sizeDelta;

        // 設定原始圖片
        if (originalImage != null)
            buttonImage.sprite = originalImage;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 更換圖片
        if (pressedImage != null)
            buttonImage.sprite = pressedImage;

        // 修改位置與大小
        rectTrans.anchoredPosition = new Vector2(rectTrans.anchoredPosition.x, -470);
        rectTrans.sizeDelta = new Vector2(408, 306);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 還原圖片
        if (originalImage != null)
            buttonImage.sprite = originalImage;

        // 還原位置與大小
        rectTrans.anchoredPosition = originalAnchoredPos;
        rectTrans.sizeDelta = originalSize;
    }
}
