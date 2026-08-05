using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private Transform leaderboardContent; 
    [SerializeField] private GameObject entryPrefab; 

    private void Start()
    {
        LoadLeaderboard();
    }

    private void LoadLeaderboard()
    {
        int entryCount = PlayerPrefs.GetInt("LeaderboardEntryCount", 0);
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        for (int i = 0; i < entryCount; i++)
        {
            string playerName = PlayerPrefs.GetString("PlayerName_" + i, "Unknown");
            int playerScore = PlayerPrefs.GetInt("PlayerScore_" + i, 0);

            entries.Add(new LeaderboardEntry(playerName, playerScore));
        }
        entries.Sort((a, b) => b.Score.CompareTo(a.Score));

        foreach (var entry in entries)
        {
            GameObject entryObject = Instantiate(entryPrefab, leaderboardContent);
            entryObject.GetComponent<TextMeshProUGUI>().text = $"{entry.PlayerName}: {entry.Score}";
        }
    }
    private class LeaderboardEntry
    {
        public string PlayerName { get; }
        public int Score { get; }

        public LeaderboardEntry(string playerName, int score)
        {
            PlayerName = playerName;
            Score = score;
        }
    }
}
