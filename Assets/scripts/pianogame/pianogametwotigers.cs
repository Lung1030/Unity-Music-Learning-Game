using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pianogametwotigers : MonoBehaviour
{
    public GameObject[] notePrefabs; // 五種音符 Prefab
    public bool isRandom = true;
    private GameManager acc;
    private PianGameLog sbee;

    // 音符時間、索引、類型與持續時間的設定
    public List<(float time, int index, int noteType, float holdDuration)> noteTimings = new List<(float, int, int, float)>
    {
        (0.1f, 0, 3, 0.5f), (0.8f, 2, 3, 0.5f), (1.5f, 4, 3, 0.5f), (2.2f, 0, 3, 0.5f),
        (2.9f, 0, 3, 0.5f), (3.6f, 2, 3, 0.5f), (4.3f, 4, 3, 0.5f), (5.0f, 0, 3, 0.5f),

        (5.7f, 4, 3, 0.5f), (6.4f, 5, 3, 0.5f), (7.1f, 7, 4, 1f),
        (8.2f, 4, 3, 0.5f), (8.9f, 5, 3, 0.5f), (9.6f, 7, 4, 1f),

        (10.7f, 7, 2, 0.25f), (11.2f, 9, 2, 0.25f), (11.7f, 7, 2, 0.25f), (12.2f, 5, 2, 0.25f),
        (12.7f, 4, 3, 0.5f), (13.2f, 0, 3, 0.5f),

        (14.3f, 7, 2, 0.25f), (14.8f, 9, 2, 0.25f), (15.3f, 7, 2, 0.25f), (15.8f, 5, 2, 0.25f),
        (16.3f, 4, 3, 0.5f), (16.8f, 0, 3, 0.5f),

        (17.9f, 0, 3, 0.5f), (18.6f, 25, 3, 0.5f), (19.3f, 0, 4, 1f),
        (20.4f, 0, 3, 0.5f), (21.1f, 25, 3, 0.5f), (21.8f, 0, 4, 1f),
    };

    private bool isSpawning = true;
    private int nextNoteIndex = 0;
    private float songTimer = 0f;

    void Start()
    {
        // 根據 isRandom 來決定使用隨機音符生成還是定時音符生成
        if (isRandom)
        {
            StartCoroutine(SpawnRandomNotes());
        }
        else
        {
            StartCoroutine(SpawnScriptedNotes());
        }
    }

    void Update()
    {
        if (!isRandom && GameManager.Instance.isGameStarted)
        {
            songTimer += Time.deltaTime;
        }
        if (GameManager.Instance != null && GameManager.Instance.accuracy >= 90)
        {
            if (!PianGameLog.gametwotiger_isclear)
            {
                PianGameLog.clearcount += 1;
            }
            PianGameLog.gametwotiger_isclear = true;
            PianGameLog.SaveClearStatus(); // ✅ 儲存
        }
    }

    // 隨機生成音符
    IEnumerator SpawnRandomNotes()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(1f);
            if (GameManager.Instance.isGameStarted)
            {
                int spawnIndex = Random.Range(0, GameManager.Instance.spawnPoints.Length);
                int noteType = Random.Range(0, notePrefabs.Length);
                Transform spawnPoint = GameManager.Instance.spawnPoints[spawnIndex];

                GameObject noteObj = Instantiate(notePrefabs[noteType], spawnPoint.position, Quaternion.identity);
                noteObj.transform.SetParent(spawnPoint); // 設定為該 spawnPoint 的子物件

                Note note = noteObj.GetComponent<Note>();
                note.moveSpeed = GameManager.Instance.noteSpeed;
            }
        }
    }

    // 根據 script 時間表生成音符
    IEnumerator SpawnScriptedNotes()
    {
        while (nextNoteIndex < noteTimings.Count)
        {
            // 等待音符的時間
            yield return new WaitUntil(() => songTimer >= noteTimings[nextNoteIndex].time);

            var (time, index, noteType, holdDuration) = noteTimings[nextNoteIndex];
            Transform spawnPoint = GameManager.Instance.spawnPoints[index];

            // 創建音符並設定位置
            GameObject noteObj = Instantiate(notePrefabs[noteType], spawnPoint.position, Quaternion.identity);
            noteObj.transform.SetParent(spawnPoint); // 設定為該 spawnPoint 的子物件

            Note note = noteObj.GetComponent<Note>();
            note.moveSpeed = GameManager.Instance.noteSpeed;
            note.holdDuration = holdDuration; // 傳遞 holdDuration

            nextNoteIndex++; // 移動到下一個音符
        }
    }
}
