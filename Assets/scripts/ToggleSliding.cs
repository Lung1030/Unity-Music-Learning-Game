using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空間

public class ToggleSliding : MonoBehaviour
{
    public Sprite toggleOffSprite;
    public Sprite toggleOnSprite;
    public ScrollRect pianoScrollView;
    public TextMeshProUGUI statusText;

    private Image buttonImage;
    private bool isOn = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = toggleOffSprite;
        UpdateScrollView(false);
    }

    public void Toggle()
    {
        isOn = !isOn;
        buttonImage.sprite = isOn ? toggleOnSprite : toggleOffSprite;
        UpdateScrollView(isOn);
    }

    private void UpdateScrollView(bool isOn)
    {
        if (pianoScrollView != null)
        {
            // 根據是否開啟 Unrestricted 模式來設定 ScrollRect 的移動類型
            if (isOn)
            {
                pianoScrollView.horizontal = isOn;
            }
            else
            {
                pianoScrollView.horizontal = isOn;
            }
        }

        if (statusText != null)
        {
            // 根據開啟的模式更新 UI 狀態文字
            statusText.text = isOn ? "啟用" : "關閉";
        }

        // 根據切換狀態設置是否允許滑動觸發音符
        btnkey.allowSliding = !isOn; // 當開啟 Unrestricted 模式時禁用滑動觸發音符
    }
}
