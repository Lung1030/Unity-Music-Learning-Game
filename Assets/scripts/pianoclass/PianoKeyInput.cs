using UnityEngine;
using UnityEngine.EventSystems;

public class PianoKeyInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("該按鍵對應的音名，例如 Do")]
    public string noteName;

    [Tooltip("連結至 JudgeLine")]
    public JudgeLine judgeLine;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (judgeLine != null)
        {
            judgeLine.SetInput(noteName, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (judgeLine != null)
        {
            judgeLine.SetInput(noteName, false);
        }
    }
}
