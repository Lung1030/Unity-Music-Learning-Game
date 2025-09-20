using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class QandA : MonoBehaviour
{
    public QuestionDatabase database;
    public Text questionText;
    public Image questionImage;
    public GameObject panel2;          // 管理 questionImage 的容器
    public AudioSource audioSource;
    public Button[] optionButtons;
    public GameObject questionPanel;
    public Text numberLabel;

    [Header("計時器")]
    public Text timeText;          // 倒數顯示
    private float timeRemaining;   
    private bool isTiming;

    [Header("答題結果顯示")]
    public GameObject endPanel;
    public Transform resultContent;
    public GameObject resultItemPrefab;
    public Text scoreText;
    public Button leavebutton;

    private List<Question> selectedQuestions = new List<Question>();
    private List<(Question, int)> wrongAnswers = new List<(Question, int)>();
    private int currentIndex = 0;
    private bool examStarted = false;

    void Start()
    {
        questionPanel.SetActive(false);
        endPanel.SetActive(false);
        if (timeText != null)
            timeText.text = "";
        if (panel2 != null)
            panel2.SetActive(false);
    }

    public void StartExam()
    {
        currentIndex = 0;
        wrongAnswers.Clear();
        questionPanel.SetActive(true);
        endPanel.SetActive(false);
        PrepareQuestions();
        DisplayQuestion();
        examStarted = true;
    }

    void PrepareQuestions()
    {
        List<Question> baseQuestions = new List<Question>();
        List<Question> advancedQuestions = new List<Question>();

        foreach (var q in database.questions)
        {
            if (q.difficulty == 0) baseQuestions.Add(q);
            else if (q.difficulty == 1) advancedQuestions.Add(q);
        }

        int total = ExamManager.numValue;
        if (ExamManager.diffValue == 0)
            selectedQuestions = GetRandomQuestions(baseQuestions, total);
        else if (ExamManager.diffValue == 1)
            selectedQuestions = GetRandomQuestions(advancedQuestions, total);
        else
        {
            int baseCount = Mathf.RoundToInt(total * 0.4f);
            int advCount = total - baseCount;
            var combined = new List<Question>();
            combined.AddRange(GetRandomQuestions(baseQuestions, baseCount));
            combined.AddRange(GetRandomQuestions(advancedQuestions, advCount));
            selectedQuestions = Shuffle(combined);
        }
    }

    void DisplayQuestion()
    {
        if (currentIndex >= selectedQuestions.Count || wrongAnswers.Count >= ExamManager.limitValue)
        {
            EndExam();
            return;
        }

        Question q = selectedQuestions[currentIndex];
        numberLabel.text = $"Q{currentIndex + 1}";
        questionText.text = q.questionText;

        bool hasImage = q.questionImage != null;
        if (panel2 != null)
            panel2.SetActive(hasImage);
        if (hasImage)
            questionImage.sprite = q.questionImage;

        if (q.questionAudio != null)
        {
            audioSource.clip = q.questionAudio;
            audioSource.Play();
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btnText = optionButtons[i].GetComponentInChildren<Text>();
            btnText.text = q.options[i];
            optionButtons[i].interactable = true;
            int capturedIndex = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIndex));
        }

        timeRemaining = 10f;
        isTiming = true;
        UpdateTimeText();
    }

    void Update()
    {
        if (!examStarted) return;

        if (isTiming)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                isTiming = false;
                OnTimeOut();
            }
            UpdateTimeText();
        }
    }

    void UpdateTimeText()
    {
        if (timeText != null)
            timeText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    void OnTimeOut()
    {
        Question q = selectedQuestions[currentIndex];
        wrongAnswers.Add((q, -1));
        ShowAnswerResult(-1);
        Invoke("NextQuestion", 1.5f);
    }

    void OnAnswerSelected(int index)
    {
        isTiming = false;
        Question q = selectedQuestions[currentIndex];
        if (index != q.correctIndex)
            wrongAnswers.Add((q, index));

        ShowAnswerResult(index);
        Invoke("NextQuestion", 1.5f);
    }

    void ShowAnswerResult(int selectedIndex)
    {
        Question q = selectedQuestions[currentIndex];

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btnText = optionButtons[i].GetComponentInChildren<Text>();

            if (i == q.correctIndex)
                btnText.text += " O";
            else if (i == selectedIndex)
                btnText.text += " X";

            optionButtons[i].interactable = false;
        }
    }

    void NextQuestion()
    {
        currentIndex++;
        DisplayQuestion();
    }

    void EndExam()
    {
        examStarted = false;
        questionPanel.SetActive(false);
        endPanel.SetActive(true);

        foreach (Transform child in resultContent)
            Destroy(child.gameObject);

        int total = selectedQuestions.Count;
        int correct = total - wrongAnswers.Count;
        float score = ((float)correct / total) * 100f;
        if (scoreText != null)
            scoreText.text = $"成績：{score:F1} 分";

        for (int i = 0; i < selectedQuestions.Count; i++)
        {
            var qq = selectedQuestions[i];
            GameObject item = Instantiate(resultItemPrefab, resultContent);
            var texts = item.GetComponentsInChildren<Text>();

            texts[0].text = $"Q{i + 1}. {qq.questionText}";
            texts[2].text = "正確答案：" + qq.options[qq.correctIndex];
            texts[3].text = "-----------------------------------------";

            if (wrongAnswers.Exists(pair => pair.Item1 == qq))
            {
                int userChoice = wrongAnswers.Find(pair => pair.Item1 == qq).Item2;
                texts[1].text = userChoice >= 0
                    ? "你的答案：" + qq.options[userChoice]
                    : "你的答案： 未作答";
                foreach (var t in texts) t.color = Color.red;
            }
            else
            {
                texts[1].text = "你的答案：" + qq.options[qq.correctIndex] + " ✔";
                foreach (var t in texts) t.color = Color.white;
            }
        }

        leavebutton.onClick.RemoveAllListeners();
        leavebutton.onClick.AddListener(Leave);
    }

    private void Leave()
    {
        SceneManager.LoadScene("MusicTheoryChoose");
    }

    List<Question> GetRandomQuestions(List<Question> source, int count)
    {
        var shuffled = Shuffle(new List<Question>(source));
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }

    List<Question> Shuffle(List<Question> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
