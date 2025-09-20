using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 加入 UI 命名空間

public class btnkey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    public AudioSource originalAudioSource; // 原始音源
    private float touchStartTime;
    private float maxPlayTime;
    private bool isTouching;
    private btnkey lastPlayedKey;
    private AudioSource currentAudio;

    public static bool allowSliding = true;

    private Image keyImage; // 儲存按鍵的 Image 組件
    private Color originalColor; // 原本的顏色
    public Color pressedColor = Color.green; // 按下時變綠色

    void Start()
    {
        if (originalAudioSource == null)
        {
            originalAudioSource = GetComponent<AudioSource>();
        }

        if (originalAudioSource == null || originalAudioSource.clip == null)
        {
            Debug.LogError("btnkey: AudioSource 或 Clip 未設定！");
            return;
        }

        maxPlayTime = originalAudioSource.clip.length;

        // 嘗試獲取 Image 組件
        keyImage = GetComponent<Image>();
        if (keyImage != null)
        {
            originalColor = keyImage.color; // 記錄原本的顏色
        }
        else
        {
            Debug.LogWarning("btnkey: 無法找到 Image 組件，無法更改顏色");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchStartTime = Time.time;
        isTouching = true;
        PlayKey();

        // **確保舊琴鍵恢復顏色**
        if (lastPlayedKey != null && lastPlayedKey != this)
        {
            lastPlayedKey.ResetKeyColor();
        }
        lastPlayedKey = this;

        // **改變按鍵顏色**
        if (keyImage != null)
        {
            keyImage.color = pressedColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouching = false;

        if (currentAudio != null)
        {
            float releaseTime = 0.8f;
            StartCoroutine(StopAudioAfterTime(currentAudio, releaseTime));
        }

        // **恢復原本的顏色**
        ResetKeyColor();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!allowSliding) return;

        GameObject target = eventData.pointerCurrentRaycast.gameObject;
        if (target != null)
        {
            btnkey newKey = target.GetComponent<btnkey>();
            if (newKey != null && newKey != lastPlayedKey)
            {
                // **確保舊琴鍵恢復原本的顏色**
                if (lastPlayedKey != null)
                {
                    lastPlayedKey.ResetKeyColor();
                }

                lastPlayedKey = newKey;
                newKey.OnPointerDown(eventData);
            }
        }
    }

    // **解決滑動最後一個琴鍵不恢復顏色**
    public void OnEndDrag(PointerEventData eventData)
    {
        if (lastPlayedKey != null)
        {
            lastPlayedKey.ResetKeyColor();
        }
    }

    private void PlayKey()
    {
        if (originalAudioSource == null || originalAudioSource.clip == null)
        {
            Debug.LogError("btnkey: 無法播放音效，AudioSource 或 Clip 未設定！");
            return;
        }

        float touchDuration = Time.time - touchStartTime;
        float playTime = Mathf.Min(maxPlayTime - touchDuration, maxPlayTime);

        currentAudio = gameObject.AddComponent<AudioSource>();
        currentAudio.clip = originalAudioSource.clip;
        currentAudio.volume = originalAudioSource.volume;
        currentAudio.pitch = originalAudioSource.pitch;
        currentAudio.Play();

        StartCoroutine(StopAudioAfterTime(currentAudio, playTime));
    }

    private IEnumerator StopAudioAfterTime(AudioSource audio, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(audio);
    }

    // **恢復鍵的顏色**
    private void ResetKeyColor()
    {
        if (keyImage != null)
        {
            keyImage.color = originalColor;
        }
    }
}
