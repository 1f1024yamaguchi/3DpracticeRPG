using UnityEngine;
using UnityEngine.EventSystems;
public class UIManager_in_main : MonoBehaviour
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
        HideAllPanels();
        
    }

        public void HideAllPanels()
    {
        MainMenuPanel.SetActive(false);
        audioSettingPanel.SetActive(false);
        sensitivetyPanel.SetActive(false);
        // マウスカーソルを隠す設定などをここに入れても良いです
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
        Debug.Log("ボタンが押されました！");
        MainMenuPanel.SetActive(false);
        audioSettingPanel.SetActive(true);
        sensitivetyPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(BGM_SE_backButton);

    }


    public void ShowSensitivitySettings()
    {
        Debug.Log("ボタンが押されました！");
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
