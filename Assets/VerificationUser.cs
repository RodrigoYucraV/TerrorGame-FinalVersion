using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class VerificationUser : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField; 
    [SerializeField] private Button continueButton; 
    [SerializeField] private Text errorText; 

    private void Start()
    {
        continueButton.interactable = false;
        errorText.gameObject.SetActive(false);
        inputField.onValueChanged.AddListener(OnInputFieldChanged);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void OnInputFieldChanged(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            continueButton.interactable = true;
            errorText.gameObject.SetActive(false);
        }
        else
        {
            continueButton.interactable = false;
            errorText.gameObject.SetActive(true);
        }
    }

    private void OnContinueButtonClicked()
    {
        PlayerPrefs.SetString("UserName", inputField.text);
        PlayerPrefs.Save();
    }
}
