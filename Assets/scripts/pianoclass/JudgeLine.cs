using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JudgeLine : MonoBehaviour
{
    [Header("結束與顯示")]
    public GameObject endPanel;
    public Text summaryText;
    
    public Transform errorListContent;
    public GameObject errorTextPrefab;
    public GameObject countdownPanel;
    public Text countdownText;

    [Header("教學總時間")]
    public float time = 10f;
    private float timer = 0f;
    private float countdownTime = 4f;

    [Header("判定設定")]
    public float inputToleranceTime = 0.2f;
    public float holdTimeToleranceRatio = 0.2f;

    public static bool isPaused = false;

    private List<NoteInfo> currentNotes = new List<NoteInfo>();
    private Dictionary<string, bool> currentPressedNotes = new Dictionary<string, bool>();
    private Dictionary<NoteInfo, float> pressStartTimes = new Dictionary<NoteInfo, float>();
    private HashSet<NoteInfo> judgedNotes = new HashSet<NoteInfo>();

    private int noteCounter = 0;
    private int wrongCount = 0;
    private List<string> wrongNotes = new List<string>();
    private bool isLessonEnded = false;

    [Header("其他設定")]
    public Button changemodebutton1;
    public Button changemodebutton2;
    public GameObject notePanel;
    public GameObject easyPanel;
    public GameObject startPanel;

    void Start()
    {
        
        changemodebutton1.onClick.AddListener(changemode);
        changemodebutton2.onClick.AddListener(nochangemode);
        startPanel.SetActive(true);
        AutoScroll.isScrolling = false;
        isPaused = true;
    }

    void Update()
    {
        if (isLessonEnded || isPaused) return;

        timer += Time.deltaTime;
        if (timer >= time)
        {
            EndLesson();
            return;
        }

        UpdateVisualFeedback();
    }

    private void UpdateVisualFeedback()
    {
        foreach (var note in currentNotes)
        {
            if (judgedNotes.Contains(note)) continue;

            string noteName = note.noteName;
            if (currentPressedNotes.ContainsKey(noteName) && currentPressedNotes[noteName])
            {
                float holdTime = Time.time - pressStartTimes[note];
                float requiredHold = note.requiredHoldTime;
                float tolerance = requiredHold * holdTimeToleranceRatio;

                if (holdTime >= requiredHold - tolerance && holdTime <= requiredHold + tolerance)
                    note.SetColor(Color.green);
                else if (holdTime > requiredHold + tolerance)
                    note.SetColor(Color.red);
                else
                    note.SetColor(Color.yellow);
            }
            else
            {
                note.SetColor(Color.white);
            }
        }
    }

    public void changemode()
    {
        notePanel.SetActive(false);
        easyPanel.SetActive(true);
        startPanel.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    public void nochangemode()
    {
        notePanel.SetActive(true);
        easyPanel.SetActive(false);
        startPanel.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownPanel.SetActive(true);
        countdownText.gameObject.SetActive(true);
        countdownTime = 4f;

        while (countdownTime > 0)
        {
            countdownTime -= 1;
            countdownText.text = countdownTime > 0 ? countdownTime.ToString() : "Start!";
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownText.gameObject.SetActive(false);
        countdownPanel.SetActive(false);
        isPaused = false;
        AutoScroll.isScrolling = true;
    }

    public void SetInput(string noteName, bool isHeld)
    {
        currentPressedNotes[noteName] = isHeld;

        var activeNotes = currentNotes.Where(n => n.noteName == noteName && !judgedNotes.Contains(n)).ToList();

        foreach (var note in activeNotes)
        {
            if (isHeld)
            {
                if (!pressStartTimes.ContainsKey(note))
                    pressStartTimes[note] = Time.time;
            }
            else
            {
                if (pressStartTimes.TryGetValue(note, out float startTime))
                {
                    float actualHold = Time.time - startTime;
                    float requiredHold = note.requiredHoldTime;
                    float tolerance = requiredHold * holdTimeToleranceRatio;

                    if (Mathf.Abs(actualHold - requiredHold) <= tolerance)
                    {
                        note.SetColor(Color.green);
                        note.HideImage();
                    }
                    else
                    {
                        note.SetColor(Color.red);
                        RecordMistake(noteCounter, note.noteName, actualHold, requiredHold);
                    }

                    judgedNotes.Add(note);
                    pressStartTimes.Remove(note);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLessonEnded || isPaused) return;

        if (other.TryGetComponent(out NoteInfo note))
        {
            if (!currentNotes.Contains(note))
            {
                currentNotes.Add(note);
                note.SetColor(Color.white);
                noteCounter++;

                StartCoroutine(JudgeNoteCoroutine(note));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isLessonEnded || isPaused) return;

        if (other.TryGetComponent(out NoteInfo note))
        {
            if (!judgedNotes.Contains(note))
            {
                if (pressStartTimes.TryGetValue(note, out float startTime))
                {
                    float holdTime = Time.time - startTime;
                    float required = note.requiredHoldTime;
                    float tolerance = required * holdTimeToleranceRatio;

                    if (Mathf.Abs(holdTime - required) <= tolerance)
                    {
                        note.SetColor(Color.green);
                        note.HideImage();
                    }
                    else
                    {
                        note.SetColor(Color.red);
                        RecordMistake(noteCounter, note.noteName, holdTime, required);
                    }

                    pressStartTimes.Remove(note);
                }
                else
                {
                    RecordMistake(noteCounter, note.noteName, 0f, note.requiredHoldTime);
                    note.SetColor(Color.red);
                }

                judgedNotes.Add(note);
            }

            currentNotes.Remove(note);
        }
    }

    IEnumerator JudgeNoteCoroutine(NoteInfo note)
    {
        float waitTime = note.requiredHoldTime + (note.requiredHoldTime * holdTimeToleranceRatio) + inputToleranceTime;
        yield return new WaitForSeconds(waitTime);

        if (!judgedNotes.Contains(note))
        {
            RecordMistake(noteCounter, note.noteName, 0f, note.requiredHoldTime);
            note.SetColor(Color.red);
            judgedNotes.Add(note);
        }
    }

    private void RecordMistake(int index, string noteName, float actualTime = -1f, float requiredTime = -1f)
    {
        wrongCount++;
        if (actualTime > 0f && requiredTime > 0f)
        {
            float diff = Mathf.Abs(actualTime - requiredTime);
            wrongNotes.Add($"第 {index} 個音為 {noteName}，按住時間錯誤 (實際: {actualTime:F2}s, 要求: {requiredTime:F2}s, 差距: {diff:F2}s)");
        }
        else
        {
            wrongNotes.Add($"第 {index} 個音為 {noteName}，沒有按到");
        }
    }

    public void EndLesson()
    {
        isLessonEnded = true;
        endPanel.SetActive(true);
        summaryText.text = $"總共音符：{noteCounter} 個 \n錯誤次數：{wrongCount} 次";

        foreach (var err in wrongNotes)
        {
            GameObject item = Instantiate(errorTextPrefab, errorListContent);
            item.GetComponent<Text>().text = err;
        }
    }

    
}
