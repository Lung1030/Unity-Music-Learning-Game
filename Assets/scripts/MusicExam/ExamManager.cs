using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExamManager : MonoBehaviour
{
    public GameObject startchoosepanel;
    public Button startbutton;
    public Button exitbutton;

    [Header("滑動元件")]
    public ScrollRect LimitscrollRect;
    public ScrollRect DiffscrollRect;
    public ScrollRect NumscrollRect;

    [Header("左右按鈕")]
    public Button LimitLButton;
    public Button LimitRButton;
    public Button DiffLButton;
    public Button DiffRButton;
    public Button NumLButton;
    public Button NumRButton;

    [Header("出題腳本")]
    public QandA qAndA;

    // 可供其他腳本存取的靜態值
    public static int limitValue = 50;
    public static int diffValue = 0;
    public static int numValue = 10;

    private int[] LimitData = { 50, 10, 5, 1 };
    private int[] DiffData = { 0, 1, 2 };
    private int[] NumData = { 10, 20, 25, 50 };

    void Start()
    {
        startchoosepanel.SetActive(true);
        startbutton.onClick.AddListener(StartExam);
        exitbutton.onClick.AddListener(ExitExam);

        LimitLButton.onClick.AddListener(() => AdjustScroll(LimitscrollRect, 400, 0, 1200));
        LimitRButton.onClick.AddListener(() => AdjustScroll(LimitscrollRect, -400, 0, 1200));
        DiffLButton.onClick.AddListener(() => AdjustScroll(DiffscrollRect, 400, 0, 800));
        DiffRButton.onClick.AddListener(() => AdjustScroll(DiffscrollRect, -400, 0, 800));
        NumLButton.onClick.AddListener(() => AdjustScroll(NumscrollRect, 400, 0, 1200));
        NumRButton.onClick.AddListener(() => AdjustScroll(NumscrollRect, -400, 0, 1200));
    }

    void Update()
    {
        int limitIndex = Mathf.Clamp(Mathf.RoundToInt(-LimitscrollRect.content.anchoredPosition.x / 400f), 0, LimitData.Length - 1);
        int diffIndex = Mathf.Clamp(Mathf.RoundToInt(-DiffscrollRect.content.anchoredPosition.x / 400f), 0, DiffData.Length - 1);
        int numIndex = Mathf.Clamp(Mathf.RoundToInt(-NumscrollRect.content.anchoredPosition.x / 400f), 0, NumData.Length - 1);

        limitValue = LimitData[limitIndex];
        diffValue = DiffData[diffIndex];
        numValue = NumData[numIndex];
    }

    void StartExam()
    {
        startchoosepanel.SetActive(false);

        if (qAndA != null)
        {
            qAndA.StartExam();
        }
        else
        {
            Debug.LogWarning("QandA 參考尚未指派！");
        }
    }

    void ExitExam()
    {
        SceneManager.LoadScene("MusicTheoryChoose");
    }

    void AdjustScroll(ScrollRect scroll, float delta, float min, float max)
    {
        float x = scroll.content.anchoredPosition.x;
        float newX = Mathf.Clamp(x + delta, -max, -min); // 注意：ScrollRect 往右滑是負值
        scroll.content.anchoredPosition = new Vector2(newX, scroll.content.anchoredPosition.y);
    }
}
