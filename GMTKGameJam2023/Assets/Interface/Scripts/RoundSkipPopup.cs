using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundSkipPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingIndicator;

    [Header("Saved Game Popup UI References")]
    [SerializeField] private GameObject loadSavedGamePopupUI;
    [SerializeField] public Button resumeSavedGameButton = null;
    [SerializeField] public Button newGameButton = null;

    [Header("Checkpoint UI References")]
    [SerializeField] private GameObject popupUI;
    [SerializeField] private TextMeshProUGUI headerText = null;
    [SerializeField] public Button skipRoundButton = null;
    [SerializeField] public Button roundOneButton = null;
    [SerializeField] private TextMeshProUGUI skipRoundButtonText = null;

    [Header("Text Settings")]
    [SerializeField] private string headerTextTemplate = "You have already made it to <color=#3DDD7D>round $</color>.";
    [SerializeField] private string skipButtonTextTemplate = "Skip to round $";
    [SerializeField] private char numberKey = '$';

    private void Start()
    {
        // Hide UI by default
        HidePopupUI();
        HideSavegamePopupUI();
        loadingIndicator.SetActive(false);
    }

    public void OpenPopupUI(int roundToSkip)
    {
        // Update text with round number.
        headerText.text = headerTextTemplate.Replace(numberKey.ToString(), roundToSkip.ToString());
        skipRoundButtonText.text = skipButtonTextTemplate.Replace(numberKey.ToString(), roundToSkip.ToString());

        popupUI.SetActive(true);
    }

    public void OpenSavedGamePopupUI()
    {
        loadSavedGamePopupUI.SetActive(true);
    }

    public void ShowLoadingIndicator()
    {
        loadingIndicator.SetActive(true);
        // Hide it after 1s
        Invoke(nameof(HideLoadingIndicator), 3.5f);
    }

    public void HidePopupUI()
    {
        popupUI.SetActive(false);
    }

    public void HideSavegamePopupUI()
    {
        loadSavedGamePopupUI.SetActive(false);
    }

    private void HideLoadingIndicator()
    {
        loadingIndicator.SetActive(false);
    }
}
