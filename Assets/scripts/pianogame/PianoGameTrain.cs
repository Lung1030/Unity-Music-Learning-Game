using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoGameTrain : MonoBehaviour
{
    public GameObject[] notePrefabs; // 五種音符 Prefab
    public bool isRandom = true;
    private GameManager acc;
    private PianGameLog sbee;

    // 音符時間、索引、類型與持續時間的設定
    public List<(float time, int index, int noteType, float holdDuration)> noteTimings = new List<(float, int, int, float)>
{
        // 火車快飛 (1-2小節)
    
    (0.1f, 2, 3, 0.5f),   // 火 (Re)
    (0.8f, 2, 3, 0.5f),   // 車 (Re)
    (1.5f, 4, 3, 0.5f),   // 快 (Mi)
    (2.2f, 7, 3, 0.5f),   // 飛 (Sol)
    
    // 火車快飛 (3-4小節)
    (2.9f, 2, 3, 0.5f),   // 火 (Re)
    (3.6f, 2, 3, 0.5f),   // 車 (Re)
    (4.3f, 4, 3, 0.5f),   // 快 (Mi)
    (5.0f, 7, 4, 1.0f),   // 飛 (Sol) - 延長
    
    // 穿過高山 (5-6小節)
    (6.1f, 9, 3, 0.5f),   // 穿 (La)
    (6.8f, 7, 3, 0.5f),   // 過 (Sol)
    (7.5f, 4, 3, 0.5f),   // 高 (Mi)
    (8.2f, 7, 3, 0.5f),   // 山 (Sol)
    
    // 越過小溪 (7-8小節)
    (8.9f, 9, 3, 0.5f),   // 越 (La)
    (9.6f, 7, 3, 0.5f),   // 過 (Sol)
    (10.3f, 4, 3, 0.5f),   // 小 (Mi)
    (11.0f, 2, 4, 1.0f),   // 溪 (Re) - 延長
    
    // 不知過了 (9-10小節)
    (12.1f, 4, 3, 0.5f),   // 不 (Mi)
    (12.8f, 4, 3, 0.5f),   // 知 (Mi)
    (13.5f, 7, 3, 0.5f),  // 過 (Sol)
    (14.2f, 7, 3, 0.5f),  // 了 (Sol)
    
    // 幾千里 (11-12小節)
    (14.9f, 9, 3, 0.5f),  // 幾 (La)
    (15.6f, 7, 3, 0.5f),  // 千 (Sol)
    (16.3f, 4, 4, 1.0f),  // 里 (Mi) - 延長
    
    // 快到家裡 (13-14小節)
    (17.4f, 2, 3, 0.5f),  // 快 (Re)
    (18.1f, 2, 3, 0.5f),  // 到 (Re)
    (18.8f, 4, 3, 0.5f),  // 家 (Mi)
    (19.5f, 7, 3, 0.5f),  // 裡 (Sol)
    
    // 快到家裡 (15-16小節)
    (20.2f, 2, 3, 0.5f),  // 快 (Re)
    (20.9f, 2, 3, 0.5f),  // 到 (Re)
    (21.6f, 4, 3, 0.5f),  // 家 (Mi)
    (22.3f, 7, 4, 1.0f),  // 裡 (Sol) - 延長
    
    // 媽媽見了 (17-18小節)
    (23.4f, 9, 3, 0.5f),  // 媽 (La)
    (24.1f, 7, 3, 0.5f),  // 媽 (Sol)
    (24.8f, 4, 3, 0.5f),  // 見 (Mi)
    (25.5f, 7, 3, 0.5f),  // 了 (Sol)
    
    // 真歡喜 (19-20小節)
    (26.2f, 9, 3, 0.5f),  // 真 (La)
    (26.9f, 7, 3, 0.5f),  // 歡 (Sol)
    (27.6f, 4, 3, 0.5f),  // 喜 (Mi)
    
    
    
    
    
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
            if (!PianGameLog.gametrain_isclear)
            {
                PianGameLog.clearcount += 1;
            }
            PianGameLog.gametrain_isclear = true;
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
