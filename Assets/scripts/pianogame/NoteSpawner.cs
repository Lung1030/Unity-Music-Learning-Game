using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class NoteSpawner : MonoBehaviour
{
    public GameObject[] notePrefabs; // 五種音符 Prefab
    public bool isRandom = true;

    // 音符時間、索引、類型與持續時間的設定
    public List<(float time, int index, int noteType, float holdDuration)> noteTimings = new List<(float, int, int, float)>
{
    (0.5f, 7, 3, 0.5f), (1.2f, 4, 3, 0.5f), (2.1f, 4, 4, 1f),
    (3.2f, 5, 3, 0.5f), (3.9f, 2, 3, 0.5f), (4.8f, 2, 4, 1f),
    (5.9f, 0, 3, 0.5f), (6.6f, 2, 3, 0.5f), (7.3f, 4, 3, 0.5f), (8f, 5, 3, 0.5f), (8.7f, 7, 3, 0.5f), (9.4f, 7, 3, 0.5f), (10.3f, 7, 4, 1f),

    (11.5f, 7, 3, 0.5f), (12.2f, 4, 3, 0.5f), (13.1f, 4, 4, 1f),
    (14.2f, 5, 3, 0.5f), (14.9f, 2, 3, 0.5f), (15.8f, 2, 4, 1f),
    (16.9f, 0, 3, 0.5f), (17.6f, 4, 3, 0.5f), (18.3f, 7, 3, 0.5f), (19f, 7, 3, 0.5f), (20.1f, 4, 5, 2f),

    (23f, 2, 3, 0.5f), (23.7f, 2, 3, 0.5f), (24.4f, 2, 3, 0.5f), (25.1f, 2, 3, 0.5f), (25.8f, 2, 3, 0.5f),
    (26.5f, 4, 3, 0.5f), (27.2f, 5, 3, 0.5f),
    (28.3f, 4, 3, 0.5f), (29f, 4, 3, 0.5f), (29.7f, 4, 3, 0.5f), (30.4f, 4, 3, 0.5f), (31.1f, 4, 3, 0.5f),
    (31.8f, 5, 3, 0.5f), (32.5f, 7, 3, 0.5f),

    (33.6f, 7, 3, 0.5f), (34.3f, 4, 3, 0.5f), (35.2f, 4, 4, 1f),
    (36.3f, 5, 3, 0.5f), (37f, 2, 3, 0.5f), (37.9f, 2, 4, 1f),
    (39f, 0, 3, 0.5f), (39.7f, 4, 3, 0.5f), (40.4f, 7, 3, 0.5f),
    (41.1f, 7, 3, 0.5f), (42.2f, 0, 5, 2f)
};


    private bool isSpawning = true;
    private int nextNoteIndex = 0;
    private float songTimer = 0f;

    private GameManager acc;
    private PianGameLog sbee;

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
            if (!PianGameLog.gamesmallbee_isclear)
            {
                PianGameLog.clearcount += 1;
            }
            PianGameLog.gamesmallbee_isclear = true;
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
