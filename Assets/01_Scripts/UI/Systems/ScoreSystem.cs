using System;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private int _currentScore;
    public int CurrentScore
    {
        get => _currentScore;
        set
        {
            _currentScore = value;
            UpdateUI();
            SaveScore();
        }
    }

    public Action<int> OnScoreChanged { get; internal set; }

    private void Start() => LoadScore();

    public void AddScore(int points) => CurrentScore += points;
    public void ResetScore() => CurrentScore = 0;

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = _currentScore.ToString();
    }
    private void SaveScore() => PlayerPrefs.SetInt("PlayerScore", _currentScore);
    private void LoadScore() => CurrentScore = PlayerPrefs.GetInt("PlayerScore", 0);
}
