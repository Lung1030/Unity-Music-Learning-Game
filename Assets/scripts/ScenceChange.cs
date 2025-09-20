using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
public class ScenceChange : MonoBehaviour
{
    [Header("淡出過場設定")]
    public CanvasGroup fadeCanvas;      // 指向一個覆蓋全螢幕的 CanvasGroup（alpha 0 → 1）

    public GameObject panel;
    public float fadeDuration = 0.5f;   // 淡出所需時間

    void Start()
    {
        // 一開始確保畫面不會是全黑
        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;
        
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // PianoChoose 按鈕
    public void GoToPianoChoose()
    {
        StartCoroutine(FadeAndLoad("PianoChoose"));
    }

    // DrumChoose 按鈕
    public void GoToDrumChoose()
    {
        StartCoroutine(FadeAndLoad("DrumChoose"));
    }

    // TrumpetChoose 按鈕
    public void GoToTrumpetChoose()
    {
        StartCoroutine(FadeAndLoad("Trumpet Choose"));
    }

    // MusicTheoryChoose 按鈕
    public void GoToMusicTheoryChoose()
    {
        StartCoroutine(FadeAndLoad("MusicTheoryChoose"));
    }
    public void OpenWebsite()
    {
        // SceneManager.LoadScene("webview1");
        Application.OpenURL("https://lnu.nttu.edu.tw/"); 
    }
    // HomeButton 事件：切換到 FirstScene 場景
    public void GoToFirstScene()
    {
        SceneManager.LoadScene("FirstScene");
    }
    public void GoToFirstScene123()
    {
        StartCoroutine(FadeAndLoad("FirstScene"));
    }
    //鋼琴專區
    public void GoToPlayPiano()
    {
        StartCoroutine(DelayedSceneChange("PianoPlay"));
    }
    public void GoToPianoGameChoose()
    {
        StartCoroutine(DelayedSceneChange("PianoGameChoose"));
    }  
    public void GoToPianoClassChoose()
    {
        StartCoroutine(DelayedSceneChange("PianoClassChoose"));
        
    } 
    

    public void PianoGamesmallbee()
    {
        SceneManager.LoadScene("PianoGamesmallbee");
        
    }
    public void PianoGamePianoGamesmallstart()
    {
        SceneManager.LoadScene("PianoGamesmallstart");
        
    }
    public void PianoGamePianoGametwotigers()
    {
        SceneManager.LoadScene("PianoGame2tigers");
        
    }
    public void PianoGamePianoGametrain()
    {
        SceneManager.LoadScene("PianoGameTrain");
        
    }
    public void PianoGamePianoGameBridge()
    {
        SceneManager.LoadScene("PianoGameBridge");
        
    }
    
    public void GoToPianoclasssmallbee()
    {
        SceneManager.LoadScene("Piano class small bee");

    }
    public void GoToPianoclasslittlestar()
    {
        SceneManager.LoadScene("Piano class little star");
        
    }
    public void GoToPianoclassbridge()
    {
        SceneManager.LoadScene("Piano class bridge");
        
    }
    public void GoToPianoclasstrain()
    {
        SceneManager.LoadScene("Piano class train");

    }
    //爵士鼓專區
    public void GoToPlayDrum()
    {
        StartCoroutine(DelayedSceneChange("DrumPlay"));
    }
    //樂理專區
    public void GoToExam()
    {
        SceneManager.LoadScene("MusicExam");
    }
    
    //小號
    public void GoToTrumpetExamChoose()
    {
        SceneManager.LoadScene("trumpettest");
    }  
    public void GoToTrumpetClassChoose()
    {
        SceneManager.LoadScene("TrumpetTeachchoose"); 
    } 
    public void GoToTrumpetClass1Choose()
    {
        SceneManager.LoadScene("trumpetteach"); 
    } 
    public void GoToTrumpetClass2Choose()
    {
        SceneManager.LoadScene("trumpetteachupdown"); 
    } 

    // 延遲場景切換的方法
    private IEnumerator DelayedSceneChange(string sceneName)
    {
        yield return new WaitForSeconds(0.5f); // 等待 0.5 秒
        SceneManager.LoadScene(sceneName); // 切換場景
    }
    // 通用淡出後載入場景
    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeCanvas == null)
        {
            // 沒有指定淡出 CanvasGroup，就直接切場景
            SceneManager.LoadScene(sceneName);
            yield break;    
        }
        if (panel != null)
        {
            panel.SetActive(true);
        }
        
        // 漸變 alpha 0 → 1
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // 切換場景
        SceneManager.LoadScene(sceneName);
    }
}