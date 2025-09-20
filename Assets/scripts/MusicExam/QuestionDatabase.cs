using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Question
{
    public string questionText;
    public Sprite questionImage;
    public AudioClip questionAudio;
    public string[] options = new string[4];
    public int correctIndex;
    public int difficulty; // 0: 基礎, 1: 進階
}

[CreateAssetMenu(fileName = "QuestionDatabase", menuName = "ScriptableObjects/QuestionDatabase", order = 1)]
public class QuestionDatabase : ScriptableObject
{
    public Question[] questions;
}
