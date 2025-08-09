using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;  
    public string homePageSceneName = "HomePage"; 

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false); 
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void SaveAndExit()
    {
        Debug.Log("Save successful!");
        SceneManager.LoadScene(homePageSceneName);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
