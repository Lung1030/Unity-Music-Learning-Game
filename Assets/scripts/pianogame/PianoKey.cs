using UnityEngine;
using UnityEngine.EventSystems;

public class PianoKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Note currentNote;
    private float keyDownTime;
    private bool isKeyHeld = false;

    public void SetNote(Note note)
    {
        currentNote = note;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GameManager.Instance.isGameStarted) return;

        keyDownTime = Time.time;
        isKeyHeld = true;

        if (currentNote != null)
        {
            currentNote.OnKeyPressed();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GameManager.Instance.isGameStarted || !isKeyHeld) return;

        isKeyHeld = false;

        if (currentNote != null)
        {
            currentNote.OnKeyReleased();
        }
        else
        {
            GameManager.Instance.RegisterNoteHit("Miss");
        }
    }

    public float GetKeyDownTime()
    {
        return keyDownTime;
    }

    // ✅ 加這個讓 Note 能查詢目前是否按著
    public bool IsKeyHeld => isKeyHeld;
}
