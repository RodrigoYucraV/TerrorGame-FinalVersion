using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class GameStateUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private float returnToMenuDelay = 5f;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private HealthSystem playerHealth;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Buscar jugador y suscribirse a su muerte
        if (PlayerManager.Instance != null)
        {
            playerHealth = PlayerManager.Instance.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += OnPlayerDeath;
            }
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }

    private void OnPlayerDeath()
    {
        if (gameOverText != null) gameOverText.text = "Has muerto...";
        OnGameOver();
    }

    public void OnSanityDepleted()
    {
        if (gameOverText != null) gameOverText.text = "Tu cordura colapsó...";
        OnGameOver();
    }

    private void OnGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        StartCoroutine(ReturnToMenu());
    }

    public void ShowWinPanel()
    {
        if (winPanel != null) winPanel.SetActive(true);
        StartCoroutine(ReturnToMenu());
    }

    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(returnToMenuDelay);
        SceneManager.LoadScene("Menu");
    }
}
