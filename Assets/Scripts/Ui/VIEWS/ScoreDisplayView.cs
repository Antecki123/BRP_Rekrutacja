using TMPro;
using UnityEngine;

public class ScoreDisplayView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void OnEnable()
    {
        GameEvents.ScoreUpdated += UpdateScore;
        UpdateScore();
    }
    private void OnDisable()
    {
        GameEvents.ScoreUpdated -= UpdateScore;
    }

    private void Start()
    {
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = GameControlller.Instance.Score.ToString();
    }
}
