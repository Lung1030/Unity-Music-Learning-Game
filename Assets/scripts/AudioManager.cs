using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("所有 BGM 曲目 (拖到這裡)")]
    public List<AudioClip> bgmClips;

    [Header("要播放 BGM 的場景名稱 (拖場景名稱字串)")]
    public List<string> scenesWithBGM;

    [Header("是否隨機播放？")]
    public bool shuffle = true;

    private AudioSource audioSource;
    private int currentIndex = 0;
    private bool isBgmEnabled = false;  // 目前場景是否啟用 BGM

    void Awake()
    {
        // 單例 & 不銷毀
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = null;
        audioSource.volume = 0.4f; // ← 預設音量（防止每次不同）

        // 一開始就訂閱場景切換
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 解訂閱
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 場景載入後呼叫
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool wantBgm = scenesWithBGM.Contains(scene.name);

        if (wantBgm)
        {
            // 只在「剛從靜音場景或第一次啟動，有空 clip」才啟動或換歌
            if (!isBgmEnabled || audioSource.clip == null)
            {
                PlayNextBGM();
            }
            isBgmEnabled = true;
        }
        else
        {
            isBgmEnabled = false;
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    void Update()
    {
        // 只有在「此場景要播放」的時候，且曲目播完才換下一首
        if (isBgmEnabled && !audioSource.isPlaying && audioSource.clip != null)
        {
            PlayNextBGM();
        }
    }

    /// <summary>
    /// 播放下一首（隨機或依序）
    /// </summary>
    public void PlayNextBGM()
    {
        if (bgmClips == null || bgmClips.Count == 0) return;

        int nextIndex;
        if (shuffle)
        {
            do
            {
                nextIndex = Random.Range(0, bgmClips.Count);
            }
            while (bgmClips.Count > 1 && nextIndex == currentIndex);

            currentIndex = nextIndex;
        }
        else
        {
            currentIndex = (currentIndex + 1) % bgmClips.Count;
        }

        audioSource.clip = bgmClips[currentIndex];
        audioSource.volume = 0.4f;
        audioSource.Play();
    }

    /// <summary>
    /// 直接播放指定曲目
    /// </summary>
    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgmClips.Count) return;
        currentIndex = index;
        isBgmEnabled = true;
        audioSource.clip = bgmClips[currentIndex];
        audioSource.Play();
    }

    /// <summary>
    /// 調整音量
    /// </summary>
    public void SetVolume(float vol)
    {
        audioSource.volume = Mathf.Clamp01(vol);
    }
}
