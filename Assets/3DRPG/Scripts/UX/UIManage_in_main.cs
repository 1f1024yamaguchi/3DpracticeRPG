using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIManager_in_main : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject audioSettingPanel;
    [SerializeField] private GameObject sensitivetyPanel;
    [SerializeField] private GameObject start_Button;
    [SerializeField] private GameObject Camera_backButton;
    [SerializeField] private GameObject BGM_SE_backButton;
    [SerializeField] private GameObject settingCanvas;

    public bool IsMenuVisible() => settingCanvas.activeSelf;


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
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectAfterFrame(start_Button));

    }

    public void ShowAudioSettings()
    {
        Debug.Log("ボタンが押されました！");
        MainMenuPanel.SetActive(false);
        audioSettingPanel.SetActive(true);
        sensitivetyPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectAfterFrame(BGM_SE_backButton));

    }


    public void ShowSensitivitySettings()
    {
        Debug.Log("ボタンが押されました！");
        MainMenuPanel.SetActive(false);
        audioSettingPanel.SetActive(false);
        sensitivetyPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        
        StartCoroutine(SelectAfterFrame(Camera_backButton));;
    }

    public void OpenMenu()
    {
        settingCanvas.SetActive(true);
        ShowMainMenu(); //既存のメインメニュー表示処理を呼ぶ

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseMenu()
    {
        settingCanvas.SetActive(false);
        HideAllPanels();

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    private System.Collections.IEnumerator SelectAfterFrame(GameObject obj)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; //ここで1フレーム待つ
        EventSystem.current.SetSelectedGameObject(obj);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
