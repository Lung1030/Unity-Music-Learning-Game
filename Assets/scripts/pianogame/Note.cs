using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    private bool canBePressed = false;
    private PianoKey currentKey;
    
    

    public float moveSpeed = 1f;
    public float perfectRange = 0.6f;
    private bool isPressed = false;
    private bool hasBeenDestroyed = false;

    public float holdDuration;
    private float enterTime = 0f;
    private bool isHolding = false;
    private bool isInHoldState = false; // 是否正在 Hold

    // 🆕 新增：計時器相關
    public Text holdTimerText; // 音符上顯示時間的文字
    private float holdTimer = 0f; // 實際按壓持續時間

    void Update()
    {
        if (!GameManager.Instance.isGameStarted) return;

        // 音符掉落
        float fallSpeed = moveSpeed * 200f;
        transform.localPosition -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
        perfectRange = 0.7f * GameManager.Instance.noteSpeed;

        // 正在長按中，計時並更新文字
        if (isInHoldState)
        {
            holdTimer += Time.deltaTime;

            if (holdTimerText != null)
            {
                holdTimerText.text = holdTimer.ToString("F2") + "s";

                // 🆕 如果長按達標，變成綠色字
                if (holdTimer >= holdDuration)
                {
                    holdTimerText.color = Color.green;
                }
            }
        }

        // 沒按到且掉出畫面，判為 Miss
        if (transform.localPosition.y < -450f && !hasBeenDestroyed)
        {
            if (currentKey != null && currentKey.IsKeyHeld)
            {
                // 長按中，不做 Miss 判定
                return;
            }

            // 沒有在長按，才算 Miss
            GameManager.Instance.RegisterNoteHit("Miss");
            hasBeenDestroyed = true;
            DestroyNote();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PianoKey"))
        {
            canBePressed = true;
            enterTime = Time.time;
            currentKey = other.GetComponent<PianoKey>();
            currentKey.SetNote(this);

            // 🆕 如果進入時已經在長按，馬上開始
            if (currentKey.IsKeyHeld)
            {
                OnKeyPressed();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PianoKey"))
        {
            canBePressed = false;

            if (isPressed && !hasBeenDestroyed)
            {
                GameManager.Instance.RegisterNoteHit("Miss");
                hasBeenDestroyed = true;
                DestroyNote();
            }

            if (currentKey != null)
            {
                currentKey.SetNote(null);
                currentKey = null;
            }
        }
    }

    public void OnKeyPressed()
    {
        if (hasBeenDestroyed) return;

        isPressed = true;
        isHolding = true;
        isInHoldState = true; // 進入 Hold 狀態
        holdTimer = 0f; // 🆕 每次按下重新開始計時

        if (holdTimerText != null)
        {
            holdTimerText.text = "0.00s"; // 初始化文字
            holdTimerText.color = Color.white; // 重設顏色
        }
    }

    public void OnKeyReleased()
    {
        if (!isHolding || hasBeenDestroyed || currentKey == null) return;

        float pressDuration = holdTimer; // 🆕 用 holdTimer 作為最終按壓時間
        bool isCorrectHold = Mathf.Abs(pressDuration - holdDuration) <= perfectRange;

        string result = isCorrectHold ? "Perfect" : "Miss";
        GameManager.Instance.RegisterNoteHit(result);

        isHolding = false;
        isInHoldState = false;
        hasBeenDestroyed = true;

        // 🆕 清除文字
        if (holdTimerText != null)
        {
            holdTimerText.text = "";
            holdTimerText.color = Color.white;
        }

        DestroyNote();
    }

    public void InitializeNote(float holdDuration)
    {
        this.holdDuration = holdDuration;
    }

    private void DestroyNote()
    {
        Invoke("DestroyObject", 0.1f);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
