using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class GameStateUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private float returnToMenuDelay = 5f;
    [SerializeField] private ObjectiveSystem _objectiveSystem;
    //private ScoreSystem _scoreSystem;
    [SerializeField] private TextMeshProUGUI gameOverText;
    public void Initialize(IDamageable damageable)
    {
        //damageable = damageable;
        //_scoreSystem = scoreSystem;

       // _damageable.OnDeath += OnGameOver;
        //_scoreSystem.OnScoreChanged += CheckWinCondition;
    }

    private void CheckWinCondition()
    {
        //if (_objectiveSystem.AllObjectivesCompleted())
        //{
        //    ShowWinPanel();
        //}
    }

    public void OnSanityDepleted()
    {
        gameOverText.text = "Tu cordura colapsó...";
        OnGameOver();
    }

    private void OnGameOver()
    {
        gameOverPanel.SetActive(true);
        StartCoroutine(ReturnToMenu());
    }

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);
        StartCoroutine(ReturnToMenu());
    }

    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(returnToMenuDelay);
        SceneManager.LoadScene("Menu");
    }
}