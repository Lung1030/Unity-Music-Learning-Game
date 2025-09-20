using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;
using UnityEngine.Networking;

public class DrumRecordButtonController : MonoBehaviour
{
    public Button recordButton;
    public Sprite recordOnSprite;
    public Sprite recordOffSprite;
    public Text recordButtonText;

    public GameObject saveDialog;
    public Button saveButton;
    public Button cancelButton;

    public GameObject recordingItemPrefab;
    public Transform content;

    public Sprite playSprite;
    public Sprite pauseSprite;

    private AudioClip recording;
    private string microphone;
    private bool isRecording = false;
    private float startRecordingTime;

    void Start()
    {
        recordButton.onClick.AddListener(ToggleRecording);
        saveButton.onClick.AddListener(SaveRecording);
        cancelButton.onClick.AddListener(CancelRecording);
        saveDialog.SetActive(false);

        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("未找到麥克風設備！");
        }

        LoadExistingRecordings();  // **啟動時讀取已有錄音**
    }

    private void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    private void StartRecording()
    {
        recording = Microphone.Start(microphone, false, 300, 44100);
        startRecordingTime = Time.time;
        isRecording = true;
        recordButton.image.sprite = recordOnSprite;
        recordButtonText.text = "錄音中";
        Debug.Log("開始錄音...");
    }

    private void StopRecording()
    {
        float recordingDuration = Time.time - startRecordingTime;
        Microphone.End(microphone);
        isRecording = false;
        recordButton.image.sprite = recordOffSprite;
        recordButtonText.text = " ";
        Debug.Log("停止錄音。錄音長度：" + recordingDuration + " 秒");

        saveDialog.SetActive(true);
    }

    private void SaveRecording()
    {
        float recordingDuration = Time.time - startRecordingTime;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(Application.persistentDataPath, "drum_" + timestamp + ".wav");

        SaveRecordingToWav(filePath, recordingDuration);
        saveDialog.SetActive(false);
        Debug.Log("錄音已儲存至: " + filePath);

        AddRecordingToList(filePath);
    }

    private void LoadExistingRecordings()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.wav");
        foreach (string filePath in files)
        {
            AddRecordingToList(filePath);
        }
    }

    private void AddRecordingToList(string filePath)
    {
        GameObject newItem = Instantiate(recordingItemPrefab, content);
        Text fileNameText = newItem.transform.Find("FileNameText").GetComponent<Text>();
        Button playButton = newItem.transform.Find("PlayButton").GetComponent<Button>();
        Button deleteButton = newItem.transform.Find("DelButton").GetComponent<Button>();
        Image playButtonImage = playButton.GetComponent<Image>();

        string fileName = Path.GetFileName(filePath);
        fileNameText.text = fileName;

        AudioSource audioSource = newItem.AddComponent<AudioSource>();
        bool isPlaying = false;

        playButton.onClick.AddListener(() =>
        {
            if (isPlaying)
            {
                audioSource.Stop();
                playButtonImage.sprite = playSprite;
                isPlaying = false;
            }
            else
            {
                StartCoroutine(LoadAndPlay(filePath, audioSource, playButtonImage, () => isPlaying = false));
                isPlaying = true;
            }
        });

        deleteButton.onClick.AddListener(() => DeleteRecording(newItem, filePath));
    }

    private void DeleteRecording(GameObject item, string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("錄音已刪除: " + filePath);
        }

        Destroy(item);
    }

    private IEnumerator LoadAndPlay(string filePath, AudioSource audioSource, Image playButtonImage, Action onComplete)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.Play();
                playButtonImage.sprite = pauseSprite;

                yield return new WaitForSeconds(audioSource.clip.length);

                playButtonImage.sprite = playSprite;
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError("載入音檔失敗: " + www.error);
            }
        }
    }

    private void CancelRecording()
    {
        saveDialog.SetActive(false);
        Debug.Log("取消儲存錄音。");
    }

    private void SaveRecordingToWav(string filePath, float duration)
    {
        int samples = (int)(recording.frequency * duration) * recording.channels;
        float[] data = new float[samples];
        recording.GetData(data, 0);

        byte[] wavData = ConvertToWav(data, recording.frequency);
        File.WriteAllBytes(filePath, wavData);
        Debug.Log("錄音已儲存至: " + filePath);
    }

    private byte[] ConvertToWav(float[] audioData, int frequency)
    {
        int headerSize = 44;
        int fileSize = headerSize + audioData.Length * 2;
        byte[] wavFile = new byte[fileSize];

        WriteString(wavFile, 0, "RIFF");
        BitConverter.GetBytes(fileSize - 8).CopyTo(wavFile, 4);
        WriteString(wavFile, 8, "WAVE");
        WriteString(wavFile, 12, "fmt ");
        BitConverter.GetBytes(16).CopyTo(wavFile, 16);
        BitConverter.GetBytes((short)1).CopyTo(wavFile, 20);
        BitConverter.GetBytes((short)1).CopyTo(wavFile, 22);
        BitConverter.GetBytes(frequency).CopyTo(wavFile, 24);
        BitConverter.GetBytes(frequency * 2).CopyTo(wavFile, 28);
        BitConverter.GetBytes((short)2).CopyTo(wavFile, 32);
        BitConverter.GetBytes((short)16).CopyTo(wavFile, 34);
        WriteString(wavFile, 36, "data");
        BitConverter.GetBytes(audioData.Length * 2).CopyTo(wavFile, 40);

        int offset = headerSize;
        foreach (float sample in audioData)
        {
            short shortSample = (short)(sample * short.MaxValue);
            BitConverter.GetBytes(shortSample).CopyTo(wavFile, offset);
            offset += 2;
        }

        return wavFile;
    }

    private void WriteString(byte[] buffer, int offset, string value)
    {
        foreach (char c in value)
        {
            buffer[offset++] = (byte)c;
        }
    }
}
