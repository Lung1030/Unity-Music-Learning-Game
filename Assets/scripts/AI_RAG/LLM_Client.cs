using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.Events;

public class LLM_Client : MonoBehaviour
{
    // --- 【修正點】將這兩個類別移到 LLM_Client 內部 ---
    // 用於將 Unity 的資料序列化為 JSON
    [System.Serializable]
    private class PostData
    {
        public string question;
        public string qa_type;
        // public float temperature;
        // public int max_tokens;
    }

    // 用於將伺服器回傳的 JSON 反序列化
    [System.Serializable]
    private class ResponseData
    {
        public string answer;
    }
    // --- 修正結束 ---


    // 【警告】這個 URL 每次重啟 ngrok 都會改變！
    private string apiUrl = "https://cb85be395ce2.ngrok-free.app/ask";

    // 當收到回覆時觸發的事件，可以將答案傳遞給 UI
    public UnityEvent<string> OnResponseReceived;
    
    // 公開的函式，讓其他腳本 (例如 UI 按鈕) 呼叫
    public void AskQuestion(string questionText)
    {
        // 啟動 Coroutine 來處理非同步的網路請求
        StartCoroutine(SendRequestToLLM(questionText));
    }

    private IEnumerator SendRequestToLLM(string questionText)
    {
        Debug.Log("正在準備向 LLM 發送問題...");

        // 1. 準備要發送的 JSON 資料
        PostData data = new PostData
        {
            question = questionText,
            qa_type = "rag" // 根據您的 app.py，使用 'rag' 模式
        };
        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // 2. 建立 UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // 3. 設定 Body (UploadHandler)
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            // 4. 設定回傳 (DownloadHandler)
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // 5. 設定必要的標頭 (Headers)
            request.SetRequestHeader("Content-Type", "application/json");
            
            // 【關鍵】繞過 ngrok 的瀏覽器警告頁面
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");
            request.SetRequestHeader("User-Agent", "Unity-Client");

            Debug.Log($"發送請求至 {apiUrl}...");

            // 6. 發送請求並等待回應
            yield return request.SendWebRequest();

            // 7. 處理回應
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("伺服器回應 JSON: " + responseJson);

                // 解析 JSON 並取得答案
                ResponseData response = JsonUtility.FromJson<ResponseData>(responseJson);
                string llmAnswer = response.answer;

                Debug.Log("解析後的答案: " + llmAnswer);

                // 透過事件將答案傳出去
                OnResponseReceived?.Invoke(llmAnswer);
            }
            else
            {
                Debug.LogError("請求失敗: " + request.error);
                // 顯示伺服器可能回傳的錯誤訊息
                Debug.LogError("錯誤詳情: " + request.downloadHandler.text);
                
                OnResponseReceived?.Invoke("錯誤：" + request.error);
            }
        }
    }
}