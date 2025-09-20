using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour
{
    public GameObject recordMenuPanel; // 拖曳 recordmenupanel 到這裡
    public GameObject menuButton; // 拖曳 MenuButton 到這裡

    private bool isPanelVisible = false; // 用來追蹤面板顯示狀態

    void Start()
    {
        // 初始時隱藏 recordMenuPanel
        if (recordMenuPanel != null)
        {
            recordMenuPanel.SetActive(false);
        }

        // 為 MenuButton 添加點擊事件
        if (menuButton != null)
        {
            menuButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleRecordMenuPanel);
        }
    }

    // 顯示或隱藏 recordMenuPanel
    void ToggleRecordMenuPanel()
    {
        isPanelVisible = !isPanelVisible;
        
        // 根據 isPanelVisible 顯示或隱藏 recordMenuPanel
        if (recordMenuPanel != null)
        {
            recordMenuPanel.SetActive(isPanelVisible);
        }
    }
}
