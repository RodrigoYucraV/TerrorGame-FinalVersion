using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ObjectiveSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;

    private HashSet<string> _completedObjectives = new();
    private Dictionary<string, string> _currentObjectives = new();

    public void AddObjective(string id, string description)
    {
        _currentObjectives[id] = description;
        UpdateUI();
    }

    public void CompleteObjective(string id)
    {
        if (_currentObjectives.ContainsKey(id))
        { 
            _completedObjectives.Add(id);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        var sb = new StringBuilder();
        foreach (var obj in _currentObjectives)
        {
            sb.AppendLine(_completedObjectives.Contains(obj.Key)
                ? $"<color=green>✓ {obj.Value}</color>"
                : $"○ {obj.Value}");
        }
        objectiveText.text = sb.ToString();
    }

}
