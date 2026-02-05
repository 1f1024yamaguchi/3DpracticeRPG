using UnityEngine;
using UnityEngine.EventSystems;
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject audioSettingPanel;
    [SerializeField] private GameObject sensitivetyPanel;
    [SerializeField] private GameObject start_Button;
    [SerializeField] private GameObject Camera_backButton;
    [SerializeField] private GameObject BGM_SE_backButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMainMenu();
        //起動時はポーズメニューを表示
        
    }

    public void ShowMainMenu()
    {
        MainMenuPanel.SetActive(true);
        audioSettingPanel.SetActive(false);
        sensitivetyPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(start_Button);

    }

    public void ShowAudioSettings()
    {
        MainMenuPanel.SetActive(false);
        audioSettingPanel.SetActive(true);
        sensitivetyPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(BGM_SE_backButton);

    }


    public void ShowSensitivitySettings()
    {
        MainMenuPanel.SetActive(false);
        audioSettingPanel.SetActive(false);
        sensitivetyPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(Camera_backButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
