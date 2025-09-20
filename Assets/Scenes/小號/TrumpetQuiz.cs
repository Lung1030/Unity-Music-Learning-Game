using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TrumpetQuiz : MonoBehaviour
{
    public enum QuestionType { Normal, Sharp, Flat }

    [System.Serializable]
    public class QuestionData
    {
        public QuestionType type;
        public int positionIndex;    // 對應 positions 陣列的索引
        public int correctAnswer;    // 正確答案 (bit 編碼)
    }

    [Header("題目設定")]
    public Transform noteParent;                           // 音符父物件 (放置音符的位置)
    public GameObject normalNotePrefab;                    // 正常音符 Prefab
    public GameObject sharpNotePrefab;                     // 升記號音符 Prefab
    public GameObject flatNotePrefab;                      // 降記號音符 Prefab
    public QuestionData[] questions;                       // 所有題目資料

    private GameObject currentNoteInstance;                // 當前顯示的音符實例

    // 題庫座標
    private readonly int[] sharpPositions = { -52, -26, 26, 52, 104, 130, 156, 208, 234, 286, 312, 338 };
    private int[] flatPositions;     // 由 sharpPositions + 26 自動產生
    private int[] normalPositions;   // 從 -52 起，每 26 一格，<= 388

    private List<int> questionOrder; // 隨機題目順序
    private int currentIndex = 0;


    [Header("五線譜輔助線")]
    public GameObject topline1;
    public GameObject topline2;
    public GameObject downline1;
    public GameObject downline2;


    [Header("UI 元件")]
    public Button confirmButton;        
    public Text timeText;               
    public GameObject wrongPanel;       
    public Text correctAnswerText;      
    public Button closePanelButton;     

    [Header("結束面板")]
    public GameObject endPanel;         
    public Button restartButton;        
              

    [Header("活塞按鍵設定")]
    public GameObject[] valves;         // 三個活塞物件 (Button 或 Image)
    private bool[] valveStates;         // 活塞狀態 (true=按下)

    [Header("題目參數")]
    private float timeLimit = 5f;
    private float timer;

    void Awake()
    {
        // 產生 normalPositions：-52 起，每 +26，直到 <= 388（實際最後會是 364）
        List<int> normals = new List<int>();
        for (int y = -52; y <= 390; y += 26)
            normals.Add(y);
        normalPositions = normals.ToArray();

        // 產生 flatPositions：sharp + 26
        flatPositions = new int[sharpPositions.Length];
        for (int i = 0; i < sharpPositions.Length; i++)
            flatPositions[i] = sharpPositions[i] + 26;
    }

    void Start()
    {
        // 初始化活塞狀態
        valveStates = new bool[valves.Length];

        // 加入 PointerDown / PointerUp 事件
        for (int i = 0; i < valves.Length; i++)
        {
            int index = i;
            var trigger = valves[i].GetComponent<EventTrigger>();
            if (trigger == null) trigger = valves[i].AddComponent<EventTrigger>();

            var press = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            press.callback.AddListener((_) => SetValveState(index, true));
            trigger.triggers.Add(press);

            var release = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            release.callback.AddListener((_) => SetValveState(index, false));
            trigger.triggers.Add(release);
        }

        // 綁定 UI
        if (confirmButton) confirmButton.onClick.AddListener(CheckAnswer);
        if (closePanelButton) closePanelButton.onClick.AddListener(ClosePanel);
        if (restartButton) restartButton.onClick.AddListener(RestartQuiz);
        

        if (wrongPanel) wrongPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);

        StartQuiz();
    }

    void Update()
    {
        if (endPanel != null && endPanel.activeSelf) return;

        timer -= Time.deltaTime;
        if (timeText) timeText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0f)
        {
            ShowWrongAnswer();
        }
    }

    void StartQuiz()
    {
        // 建立隨機題目順序（依 questions 長度）
        questionOrder = new List<int>();
        for (int i = 0; i < (questions?.Length ?? 0); i++)
            questionOrder.Add(i);

        Shuffle(questionOrder);
        LoadQuestion(0);
    }

    void LoadQuestion(int index)
    {
        if (questionOrder == null || questions == null || questions.Length == 0)
        {
            Debug.LogWarning("沒有題目資料設定。");
            ShowEndPanel();
            return;
        }

        if (index >= questionOrder.Count)
        {
            ShowEndPanel();
            return;
        }

        currentIndex = index;
        timer = timeLimit;

        int qIndex = questionOrder[index];
        QuestionData currentQuestion = questions[qIndex];

        // 銷毀舊的音符實例
        if (currentNoteInstance != null)
        {
            DestroyImmediate(currentNoteInstance);
        }

        // 根據題目類型創建對應的音符 Prefab
        GameObject prefabToUse = GetPrefabForType(currentQuestion.type);
        if (prefabToUse != null && noteParent != null)
        {
            currentNoteInstance = Instantiate(prefabToUse, noteParent);
        }
        else
        {
            Debug.LogWarning("找不到對應的音符 Prefab 或 noteParent 未設定！");
            return;
        }

        // 取得對應座標集和位置
        int[] posArray = GetPositionsForType(currentQuestion.type);
        if (posArray == null || posArray.Length == 0)
        {
            Debug.LogWarning("座標陣列為空。");
            return;
        }

        // 取得座標位置
        int posY = (currentQuestion.positionIndex < posArray.Length) ? 
                   posArray[currentQuestion.positionIndex] : 
                   posArray[currentQuestion.positionIndex % posArray.Length];

        // 設定音符位置
        if (currentNoteInstance != null)
        {
            RectTransform noteRect = currentNoteInstance.GetComponent<RectTransform>();
            if (noteRect != null)
            {
                noteRect.anchoredPosition = new Vector2(noteRect.anchoredPosition.x, posY);
            }
        }
        
        // 控制輔助線顯示
        if (downline2) downline2.SetActive(posY == -52 || posY == -26);
        if (downline1) downline1.SetActive(posY == 0 || posY == 26);
        if (topline1) topline1.SetActive(posY == 338 || posY == 364);
        if (topline2) topline2.SetActive(posY == 390);

        // 重置活塞狀態
        for (int i = 0; i < valveStates.Length; i++)
            valveStates[i] = false;
    }

    GameObject GetPrefabForType(QuestionType type)
    {
        switch (type)
        {
            case QuestionType.Normal: return normalNotePrefab;
            case QuestionType.Sharp:  return sharpNotePrefab;
            case QuestionType.Flat:   return flatNotePrefab;
        }
        return normalNotePrefab;
    }

    int[] GetPositionsForType(QuestionType type)
    {
        switch (type)
        {
            case QuestionType.Normal: return normalPositions;
            case QuestionType.Sharp:  return sharpPositions;
            case QuestionType.Flat:   return flatPositions;
        }
        return normalPositions;
    }

    void CheckAnswer()
    {
        if (endPanel != null && endPanel.activeSelf) return;

        int qIndex = questionOrder[currentIndex];
        QuestionData currentQuestion = questions[qIndex];
        int userInput = GetValveCode();

        if (userInput == currentQuestion.correctAnswer)
        {
            LoadQuestion(currentIndex + 1);
        }
        else
        {
            ShowWrongAnswer();
        }
    }

    void ShowWrongAnswer()
    {
        int qIndex = questionOrder[currentIndex];
        QuestionData currentQuestion = questions[qIndex];
        
        if (wrongPanel) wrongPanel.SetActive(true);
        if (correctAnswerText)
            correctAnswerText.text = "正解是: " + ConvertToBinary(currentQuestion.correctAnswer) + " (1=按下, 0=放開)";
    }

    void ClosePanel()
    {
        if (wrongPanel) wrongPanel.SetActive(false);
        LoadQuestion(currentIndex + 1);
    }

    public void SetValveState(int index, bool isPressed)
    {
        if (index >= 0 && index < valveStates.Length)
        {
            valveStates[index] = isPressed;
            // Debug.Log($"Valve {index} 狀態: {valveStates[index]}");
        }
    }

    int GetValveCode()
    {
        int code = 0;
        for (int i = 0; i < valveStates.Length; i++)
        {
            if (valveStates[i])
                code |= (1 << (valveStates.Length - 1 - i)); // 左到右 = 高位到低位
        }
        return code;
    }

    string ConvertToBinary(int value)
    {
        return System.Convert.ToString(value, 2).PadLeft(valves.Length, '0');
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    void ShowEndPanel()
    {
        if (endPanel) endPanel.SetActive(true);
        if (timeText) timeText.text = "";
    }

    void RestartQuiz()
    {
        if (endPanel) endPanel.SetActive(false);
        StartQuiz();
    }

    

    void OnDestroy()
    {
        // 清理音符實例
        if (currentNoteInstance != null)
        {
            DestroyImmediate(currentNoteInstance);
        }
    }

    // 輔助方法：自動生成題目資料（可在 Editor 中呼叫）
    [ContextMenu("Generate All Questions")]
    void GenerateAllQuestions()
    {
        List<QuestionData> allQuestions = new List<QuestionData>();
        
        // 生成所有 Normal 題目
        for (int i = 0; i < normalPositions.Length; i++)
        {
            allQuestions.Add(new QuestionData
            {
                type = QuestionType.Normal,
                positionIndex = i,
                correctAnswer = 0 // 請根據實際需求設定正確答案
            });
        }
        
        // 生成所有 Sharp 題目
        for (int i = 0; i < sharpPositions.Length; i++)
        {
            allQuestions.Add(new QuestionData
            {
                type = QuestionType.Sharp,
                positionIndex = i,
                correctAnswer = 0 // 請根據實際需求設定正確答案
            });
        }
        
        // 生成所有 Flat 題目
        for (int i = 0; i < flatPositions.Length; i++)
        {
            allQuestions.Add(new QuestionData
            {
                type = QuestionType.Flat,
                positionIndex = i,
                correctAnswer = 0 // 請根據實際需求設定正確答案
            });
        }
        
        questions = allQuestions.ToArray();
        Debug.Log($"已生成 {questions.Length} 道題目");
    }
}