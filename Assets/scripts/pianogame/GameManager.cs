using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 元件")]
    public GameObject startPanel;
    public GameObject countdownPanel;
    public GameObject exitPanel;
    public Text speedText;
    public Button speedUpButton;
    public Button speedDownButton;
    public Button startButton;
    public Button CancelButton;
    public Button settingButton;
    public Button backButton;
    public Button YButton;
    public Button NButton;
    public Text timerText;
    public Text StausText;
    public GameObject endPanel;
    public Text accuracyText;
    public Text resultText;
    public Text countdownText;
    public Text feedbackStatusText;

    [Header("遊戲設定")]
    public float gameDuration = 15f;
    private float gameTime;
    public bool isGameStarted = false;
    public bool isPaused = false;
    private float countdownTime = 4f;
    private float pausedTime;
    public float accuracy = 0f;

    [Header("音符速度")]
    public float noteSpeed = 1f;
    private const float speedStep = 0.2f;
    private const float minSpeed = 1f;
    private const float maxSpeed = 2.0f;

    private int totalNotes = 0;
    private int perfectCount = 0;
    private int missCount = 0;

    [Header("音符生成點")]
    public Transform[] spawnPoints;

    [Header("音效 & 震動")]
    public AudioSource errorAudioSource;
    public AudioClip errorClip;
    public AudioSource bgmSource;
    public AudioClip bgmExcellent;
    public AudioClip bgmGreat;
    public AudioClip bgmGood;
    public AudioClip bgmTryAgain;

    [Header("錯誤回饋設定")]
    public Button feedbackToggleButton;
    public Image feedbackToggleImage;
    public Sprite soundModeSprite;
    public Sprite vibrationModeSprite;
    private bool isVibrationMode = false;

    void Awake() => Instance = this;

    void Start()
    {
        gameTime = gameDuration;
        speedUpButton.onClick.AddListener(IncreaseSpeed);
        speedDownButton.onClick.AddListener(DecreaseSpeed);
        startButton.onClick.AddListener(StartGame);
        feedbackToggleButton.onClick.AddListener(ToggleFeedbackMode);
        backButton.onClick.AddListener(exitgame);
        
        UpdateSpeedText();
        startPanel.SetActive(true);
        endPanel.SetActive(false);

        feedbackStatusText.text = "關閉";
        feedbackToggleImage.sprite = vibrationModeSprite;
    }

    void Update()
    {
        if (!isGameStarted) return;

        if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            EndGame();
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void IncreaseSpeed()
    {
        if (noteSpeed < maxSpeed)
        {
            noteSpeed += speedStep;
            UpdateSpeedText();
        }
    }

    private void DecreaseSpeed()
    {
        if (noteSpeed > minSpeed)
        {
            noteSpeed -= speedStep;
            UpdateSpeedText();
        }
    }

    private void UpdateSpeedText() => speedText.text = $"{noteSpeed:F1}x";

    public void exitgame(){
        exitPanel.SetActive(true);
        TogglePause();
        YButton.onClick.AddListener(GoToPianoGameChoose);
        NButton.onClick.AddListener(ResumeGame);
    }
    public void GoToPianoGameChoose()
    {
        exitPanel.SetActive(false);
        SceneManager.LoadScene("PianoGameChoose");
    }  
    
    private void StartGame()
    {
        StartCoroutine(StartCountdown());
        settingButton.onClick.AddListener(TogglePause);
        CancelButton.onClick.AddListener(ResumeGame);

        startPanel.SetActive(false);
        
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
        isGameStarted = true;
        Time.timeScale = 1;
    }

    public void RegisterNoteHit(string result)
    {
        if (result == "Perfect")
        {
            perfectCount++;
            StausText.text = "Perfect";
        }
        else if (result == "Miss")
        {
            missCount++;
            StausText.text = "Miss";
            if (isVibrationMode) PlayErrorSound();
        }

        StartCoroutine(ClearStatusText());
    }

    private void PlayErrorSound()
    {
        if (errorAudioSource && errorClip)
            errorAudioSource.PlayOneShot(errorClip);
    }

    IEnumerator ClearStatusText()
    {
        yield return new WaitForSeconds(0.5f);
        StausText.text = "";
    }

    private void ToggleFeedbackMode()
    {
        isVibrationMode = !isVibrationMode;
        feedbackStatusText.text = isVibrationMode ? "啟動" : "關閉";
        feedbackToggleImage.sprite = isVibrationMode ? soundModeSprite : vibrationModeSprite;
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            isPaused = true;
            pausedTime = gameTime;
            Time.timeScale = 0;
        }
    }

    public void ResumeGame()
    {
        exitPanel.SetActive(false);
        isPaused = false;
        StartCoroutine(StartCountdown());
    }

    public void EndGame()
    {
        isGameStarted = false;
        timerText.text = "Time's Up!";
        endPanel.SetActive(true);

        accuracy = (perfectCount+missCount > 0) ? ((float)perfectCount / (perfectCount+missCount)) * 100f : 0f;

        accuracyText.text = $"準確率: {accuracy:F1}%  {perfectCount}/{perfectCount+missCount}";

        if (accuracy >= 90)
        {
            resultText.text = "Excellent!";
            PlayEndBGM(bgmExcellent);
        }
        else if (accuracy >= 70)
        {
            resultText.text = "Great!";
            PlayEndBGM(bgmGreat);
        }
        else if (accuracy >= 50)
        {
            resultText.text = "Good!";
            PlayEndBGM(bgmGood);
        }
        else
        {
            resultText.text = "Try Again!";
            PlayEndBGM(bgmTryAgain);
        }
    }

    private void PlayEndBGM(AudioClip clip)
    {
        if (bgmSource && clip)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }
}
