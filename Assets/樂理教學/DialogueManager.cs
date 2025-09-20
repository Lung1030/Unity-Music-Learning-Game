using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Unity.VectorGraphics;
// 如果你使用 TextMeshPro，改用以下一行並把 dialogueText 類型改為 TMP_Text
// using TMPro;

[System.Serializable]
public class DialogueEntry
{
    [TextArea(2,6)]
    public string text;           // 要顯示的文字
    public Sprite portrait;       // 講者頭像（可 null）
    public AudioClip voice;       // 對話語音（可 null）
    public bool autoAdvance = false; // 是否自動前進
    public float autoDelay = 0.5f;   // 自動前進的額外延遲（秒）
}

public class DialogueManager : MonoBehaviour, IPointerClickHandler
{
    [Header("對話資料")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    [Header("UI 元件")]
    public Text dialogueText;      // 若使用 TextMeshPro，請改為 TMP_Text 並修改程式
    public SVGImage portraitImage;    // 顯示講者頭像（可留空）
    public GameObject panel;       // 接收點擊的 Panel（可留空，若留空會用本 GameObject）

    [Header("行為參數")]
    public float typeSpeed = 0.02f;           // 打字機速度（每字秒數）
    public bool clickAnywhereToAdvance = true; // 點擊 panel 任意地方前進
    public bool startOnAwake = false;          // 啟動時即開始對話（否則手動呼 StartDialogue）

    [Header("事件回調")]
    public UnityEvent onDialogueComplete;     // 對話結束時觸發

    // 內部狀態
    private int currentIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private AudioSource audioSource;
    private bool isAutoAdvancing = false;

    void Awake()
    {
        if (panel == null) panel = this.gameObject;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // 如果要啟動就開始
        if (startOnAwake && dialogueEntries.Count > 0)
            StartDialogue();
        
    }

    /// <summary>
    /// 外部呼叫開始對話
    /// </summary>
    public void StartDialogue()
    {
        currentIndex = 0;
        ShowEntry(currentIndex);
    }

    /// <summary>
    /// 顯示指定索引的對話
    /// </summary>
    void ShowEntry(int index)
    {
        // 清除自動狀態
        if (isAutoAdvancing) StopAllCoroutines(); 
        isAutoAdvancing = false;

        if (index < 0 || index >= dialogueEntries.Count)
        {
            EndDialogue();
            return;
        }

        DialogueEntry e = dialogueEntries[index];

        // portrait
        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(e.portrait != null);
            if (e.portrait != null)
                portraitImage.sprite = e.portrait;
        }

        // 播放語音（如有）
        if (e.voice != null)
        {
            audioSource.Stop();
            audioSource.clip = e.voice;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        // 啟動打字機
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(e.text));

        // 若設定 autoAdvance，當文字與音訊播放完時自動進下一句
        if (e.autoAdvance)
        {
            isAutoAdvancing = true;
            float voiceLen = (e.voice != null) ? e.voice.length : 0f;
            float delay = Mathf.Max(0f, voiceLen) + e.autoDelay;
            StartCoroutine(AutoAdvanceAfter(delay));
        }
    }

    IEnumerator AutoAdvanceAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isAutoAdvancing)
        {
            Advance();
        }
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";
        if (typeSpeed <= 0f)
        {
            dialogueText.text = fullText;
            isTyping = false;
            yield break;
        }

        for (int i = 0; i < fullText.Length; i++)
        {
            dialogueText.text += fullText[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        // 完成
        isTyping = false;
    }

    /// <summary>
    /// 被 Panel 點擊時會呼叫（或可以由其他 UI 呼叫 Advance())
    /// 判斷：如果還在打字 -> 立即完成打字；若已完成 -> 移到下一句
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 若你只希望點 Panel 空白區域才觸發，可以檢查 raycast 的目標是否為 panel 本身
        if (!clickAnywhereToAdvance)
        {
            // pointerCurrentRaycast.gameObject 會是被點的最上層 UI 元件
            if (eventData.pointerCurrentRaycast.gameObject != panel &&
                eventData.pointerCurrentRaycast.gameObject != this.gameObject)
            {
                // 點到其他互動物件：不處理
                return;
            }
        }

        // 如果正在打字 -> 立即完成
        if (isTyping)
        {
            FinishTypingImmediately();
            return;
        }

        // 如果正在自動前進中，忽略點擊
        if (isAutoAdvancing) return;

        Advance();
    }

    /// <summary>
    /// 立即把文字顯示完整（打字效果跳過）
    /// </summary>
    void FinishTypingImmediately()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        DialogueEntry e = dialogueEntries[currentIndex];
        dialogueText.text = e.text;
        isTyping = false;
    }

    /// <summary>
    /// 前進到下一句
    /// </summary>
    public void Advance()
    {
        // 停掉語音（視需求可保留）
        if (audioSource.isPlaying) audioSource.Stop();

        currentIndex++;
        if (currentIndex >= dialogueEntries.Count)
            EndDialogue();
        else
            ShowEntry(currentIndex);
    }

    void EndDialogue()
    {
        // 做結束處理
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
        isAutoAdvancing = false;
        dialogueText.text = "";
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);
        audioSource.Stop();

        // 觸發 callback（可在 Inspector 加函式）
        if (onDialogueComplete != null) onDialogueComplete.Invoke();
    }

    // 你也可以提供外部方法跳到指定索引
    public void JumpToIndex(int idx)
    {
        if (idx < 0 || idx >= dialogueEntries.Count) return;
        currentIndex = idx;
        ShowEntry(currentIndex);
    }
}
