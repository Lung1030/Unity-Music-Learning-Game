using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    public GameObject settingPanel; // 設定面板物件
    public Button settingButton; // 設定按鈕
    public Button closeButton; // 關閉按鈕

    public Slider volumeSlider; // 音量調整滑動條
    public Text volumeText; // 顯示音量的文字

    private void Start()
    {
        // 設定按鈕點擊事件
        settingButton.onClick.AddListener(OpenSettings);
        closeButton.onClick.AddListener(CloseSettings);

        // 讀取並設置音量
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f); // 預設音量為 1
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // 音量滑動條的變動事件
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        UpdateVolumeText();
    }

    // 顯示設定面板
    private void OpenSettings()
    {
        settingPanel.SetActive(true);
        
    }

    // 關閉設定面板
    private void CloseSettings()
    {
        settingPanel.SetActive(false);
    }

    // 當音量變更時
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // 設置音效的全局音量
        PlayerPrefs.SetFloat("GameVolume", value); // 存儲音量設定
        PlayerPrefs.Save(); // 儲存到設備
        UpdateVolumeText();
    }

    // 更新音量顯示文字
    private void UpdateVolumeText()
    {
        volumeText.text = Mathf.RoundToInt(volumeSlider.value * 100) + "%";
    }
}
