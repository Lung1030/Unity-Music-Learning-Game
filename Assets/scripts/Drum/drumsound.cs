using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class drumsound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioSource originalAudioSource; // 原始音源
    private float touchStartTime;
    private float maxPlayTime;
    private bool isTouching;
    private AudioSource currentAudio;

    public float startAt = 0.65f;

    void Start()
    {
        if (originalAudioSource == null)
        {
            originalAudioSource = GetComponent<AudioSource>();
        }

        if (originalAudioSource == null || originalAudioSource.clip == null)
        {
            Debug.LogError("drumsound: AudioSource 或 Clip 未設定！");
            return;
        }

        maxPlayTime = originalAudioSource.clip.length;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchStartTime = Time.time;
        isTouching = true;
        PlaySound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouching = false;

        if (currentAudio != null)
        {
            float releaseTime = 0.8f;
            StartCoroutine(StopAudioAfterTime(currentAudio, releaseTime));
        }
    }

    private void PlaySound()
{
    if (originalAudioSource == null || originalAudioSource.clip == null)
    {
        Debug.LogError("drumsound: 無法播放音效，AudioSource 或 Clip 未設定！");
        return;
    }

    
    float touchDuration = Time.time - touchStartTime;
    float playTime = Mathf.Min(originalAudioSource.clip.length - startAt, originalAudioSource.clip.length);

    currentAudio = gameObject.AddComponent<AudioSource>();
    currentAudio.clip = originalAudioSource.clip;
    currentAudio.volume = originalAudioSource.volume;
    currentAudio.pitch = originalAudioSource.pitch;

    currentAudio.time = startAt; // ✅ 播放從 0.5 秒開始
    currentAudio.Play();

    StartCoroutine(StopAudioAfterTime(currentAudio, playTime));
}

    private IEnumerator StopAudioAfterTime(AudioSource audio, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(audio);
    }
}
