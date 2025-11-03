using UnityEngine;
using UnityEngine.UI;
using System.Collections; 

public class LLM_Tester_UI : MonoBehaviour
{
    // --- 【基礎設定】---
    public InputField questionInput; 
    public Button sendButton; 
    public LLM_Client llmClient; 

    // --- 【聊天室 UI 設定】---
    public Transform chatContent;        
    public GameObject questionPrefab;    
    public GameObject answerPrefab;      
    public float typingSpeed = 0.05f;

    // --- 【新】捲動視窗設定 ---
    public ScrollRect chatScrollRect; // << 請將您場景中的 "ChatScrollView" 物件拖曳到這裡

    // --- 【邏輯變數】---
    private Coroutine userTypingCoroutine; 
    private Coroutine aiTypingCoroutine;   
    private Text currentAITextComponent;   

    void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnAskButtonClick);
        }
        if (llmClient != null)
        {
            llmClient.OnResponseReceived.AddListener(OnAIResponse);
        }
        else
        {
            Debug.LogError("LLM Client 沒有被指定！");
        }

        if (chatScrollRect == null)
        {
            Debug.LogError("Chat Scroll Rect 沒有被指定！");
        }
    }

    private void OnAskButtonClick()
    {
        string question = questionInput.text;
        if (string.IsNullOrEmpty(question) || llmClient == null)
        {
            return;
        }

        // 1. 停止上一次的打字
        if (userTypingCoroutine != null) StopCoroutine(userTypingCoroutine);
        if (aiTypingCoroutine != null) StopCoroutine(aiTypingCoroutine);

        // 2. 產生問題
        GameObject userBubble = Instantiate(questionPrefab, chatContent);
        Text userTextComponent = userBubble.GetComponent<Text>();
        userTypingCoroutine = StartCoroutine(TypewriterEffect(userTextComponent, "你: ", question, false));

        // 3. 產生思考中
        GameObject aiBubble = Instantiate(answerPrefab, chatContent);
        currentAITextComponent = aiBubble.GetComponent<Text>(); 
        currentAITextComponent.text = "音樂精靈: 正在思考...";

        // 4. 【新】強制捲動到底部
        // 我們使用 Coroutine 來確保捲動發生在 UI 佈局更新 *之後*
        StartCoroutine(ScrollToBottomAfterFrame());

        // 5. 呼叫 AI
        llmClient.AskQuestion(question);

        // 6. 清空
        questionInput.text = "";
    }

    // 當 LLM_Client 收到完整回覆時
    private void OnAIResponse(string fullAnswer)
    {
        if (currentAITextComponent != null)
        {
            if (aiTypingCoroutine != null) StopCoroutine(aiTypingCoroutine);
            aiTypingCoroutine = StartCoroutine(TypewriterEffect(currentAITextComponent, "音樂精靈: ", fullAnswer, true));
        }
        else
        {
            GameObject aiBubble = Instantiate(answerPrefab, chatContent);
            Text aiText = aiBubble.GetComponent<Text>();
            aiTypingCoroutine = StartCoroutine(TypewriterEffect(aiText, "音樂精靈: ", fullAnswer, true));
        }
    }

    // 【新函式】在影格結束時強制捲動
    IEnumerator ScrollToBottomAfterFrame()
    {
        // 等待這一影格的 UI 佈局 (VerticalLayoutGroup) 計算完成
        yield return new WaitForEndOfFrame();
        
        // 將 ScrollView 的垂直位置設為 0 (0 = 底部, 1 = 頂部)
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    // 通用的打字機效果 Coroutine
    IEnumerator TypewriterEffect(Text textComponent, string prefix, string fullMessage, bool isAI)
    {
        textComponent.text = prefix; 

        foreach (char letter in fullMessage)
        {
            textComponent.text += letter; 

            // 【新】如果 AI 正在打字，且內容很長，持續將捲軸保持在底部
            if (isAI)
            {
                chatScrollRect.verticalNormalizedPosition = 0f;
            }

            yield return new WaitForSeconds(typingSpeed); 
        }

        if (isAI)
        {
            currentAITextComponent = null; 
        }
    }

    void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnAskButtonClick);
        }
        if (llmClient != null)
        {
            llmClient.OnResponseReceived.RemoveListener(OnAIResponse);
        }
    }
}