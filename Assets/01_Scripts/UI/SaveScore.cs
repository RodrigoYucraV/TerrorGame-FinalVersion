using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveScore : MonoBehaviour
{
    public void SavePlayerScore(string playerName, int score)
    {
     
        int entryCount = PlayerPrefs.GetInt("LeaderboardEntryCount", 0);
        PlayerPrefs.SetString("PlayerName_" + entryCount, playerName);
        PlayerPrefs.SetInt("PlayerScore_" + entryCount, score);
        PlayerPrefs.SetInt("LeaderboardEntryCount", entryCount + 1);
        PlayerPrefs.Save();
    }
}
