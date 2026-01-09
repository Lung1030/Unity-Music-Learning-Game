using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.Events;

/// <summary>
/// 負責處理與 LLM 後端伺服器通訊的客戶端元件。
/// 使用 UnityWebRequest 發送 POST 請求並處理 JSON 回應。
/// </summary>
public class LLM_Client : MonoBehaviour
{
    #region 資料傳輸物件 (DTOs)

    /// <summary>
    /// 定義發送給 API 的請求資料結構。
    /// </summary>
    [System.Serializable]
    private class PostData
    {
        public string question;
        public string qa_type;
        // 若未來需要調整參數，可在此擴充
        // public float temperature;
        // public int max_tokens;
    }

    /// <summary>
    /// 定義從 API 接收的回應資料結構。
    /// </summary>
    [System.Serializable]
    private class ResponseData
    {
        public string answer;
    }

    #endregion

    [Header("伺服器設定 (Server Settings)")]
    [Tooltip("後端 API 的完整網址，包含 /ask 路徑 (例如 ngrok 網址)")]
    [SerializeField]
    // 【修改處】這裡換回您的 ngrok 預設網址範例
    private string apiUrl = "https://your-ngrok-id.ngrok-free.dev/ask";

    [Header("事件回調 (Events)")]
    [Tooltip("當成功收到 AI 回覆時觸發")]
    public UnityEvent<string> OnResponseReceived;

    /// <summary>
    /// 公開方法：發送問題至 LLM 伺服器。
    /// </summary>
    /// <param name="questionText">使用者的問題內容</param>
    public void AskQuestion(string questionText)
    {
        StartCoroutine(SendRequestRoutine(questionText));
    }

    /// <summary>
    /// 執行非同步網路請求的 Coroutine。
    /// </summary>
    private IEnumerator SendRequestRoutine(string questionText)
    {
        // 1. 建構請求資料 (Payload)
        PostData data = new PostData
        {
            question = questionText,
            qa_type = "rag" // 預設使用 RAG 檢索模式
        };

        // 將資料序列化為 JSON 格式
        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // 2. 建立並發送 WebRequest
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 設定標準標頭
            request.SetRequestHeader("Content-Type", "application/json");

            // 【重要】針對 ngrok 環境，設定跳過瀏覽器警告頁面
            // 若不設定此項，Unity 會抓到 ngrok 的 HTML 警告網頁而非 JSON 資料
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");

            // 設定 User-Agent，保留您的 MusicAI 識別
            request.SetRequestHeader("User-Agent", "MusicAI/1.0 (lnu)");

            // 開發除錯用 Log
            Debug.Log($"[LLM_Client] 正在發送請求至 (ngrok 模式): {apiUrl}");

            // 等待請求完成
            yield return request.SendWebRequest();

            // 3. 處理請求結果
            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleSuccess(request.downloadHandler.text);
            }
            else
            {
                HandleError(request);
            }
        }
    }

    /// <summary>
    /// 處理成功的 API 回應。
    /// </summary>
    private void HandleSuccess(string jsonResponse)
    {
        try
        {
            // 反序列化 JSON 回應
            ResponseData response = JsonUtility.FromJson<ResponseData>(jsonResponse);

            if (!string.IsNullOrEmpty(response.answer))
            {
                Debug.Log($"[LLM_Client] 接收成功，回應長度: {response.answer.Length}");
                OnResponseReceived?.Invoke(response.answer);
            }
            else
            {
                Debug.LogWarning("[LLM_Client] 伺服器回應內容為空。");
            }
        }
        catch (System.Exception ex)
        {
            // 修改錯誤訊息提示，提醒可能是抓到非 JSON 內容
            Debug.LogError($"[LLM_Client] JSON 解析錯誤 (請檢查網址或 ngrok 狀態): {ex.Message}");
            OnResponseReceived?.Invoke("系統錯誤：無法解析伺服器回應。");
        }
    }

    /// <summary>
    /// 處理失敗的 API 請求。
    /// </summary>
    private void HandleError(UnityWebRequest request)
    {
        Debug.LogError($"[LLM_Client] 請求失敗。代碼: {request.responseCode}, 錯誤: {request.error}");
        
        // 嘗試印出伺服器回傳的錯誤詳情（若有的話）
        if (request.downloadHandler != null)
        {
            Debug.LogError($"[LLM_Client] 伺服器訊息: {request.downloadHandler.text}");
        }

        OnResponseReceived?.Invoke($"連線錯誤: {request.error}");
    }
}
