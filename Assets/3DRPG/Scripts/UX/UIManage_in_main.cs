using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIManager_in_main : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject StatusPanel;
    [SerializeField] private GameObject audioSettingPanel;
    [SerializeField] private GameObject sensitivetyPanel;

    [SerializeField] private GameObject start_Button;
    [SerializeField] private GameObject status_backButton;
    [SerializeField] private GameObject Camera_backButton;
    [SerializeField] private GameObject BGM_SE_backButton;

    [SerializeField] private GameObject settingCanvas;
    [SerializeField] private GameObject HPCanvas; // HPスライダークラスへの参照
    [SerializeField] private GameObject TimerCanvas; // タイマークラスへの参照
    [SerializeField] private GameObject LevelUp_tipsCanvas; // レベルアップのヒントを表示するキャンバスへの参照

    private GameObject _currentFocusButton; // 現在選択されているボタンを追跡する変数

    public bool IsMenuVisible() => settingCanvas.activeSelf;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideAllPanels();
        
    }

    void Update()
    {
        //メニューが開いている時だけ「磁石機能」を動かす
        if(settingCanvas.activeSelf)
        {
            if(LevelUp_tipsCanvas != null && LevelUp_tipsCanvas.activeSelf)
            {
                return; // レベルアップのヒントが表示されている場合は、他のUI要素の選択を変更しない
            }

            if(EventSystem.current.currentSelectedGameObject ==null && _currentFocusButton != null)
            {
                EventSystem.current.SetSelectedGameObject(_currentFocusButton);
            }
        }
    }

        public void HideAllPanels()
    {
        MainMenuPanel.SetActive(false);
        StatusPanel.SetActive(false);
        audioSettingPanel.SetActive(false);
        sensitivetyPanel.SetActive(false);
        // マウスカーソルを隠す設定などをここに入れても良いです
    }

    private void SwitchPanel(GameObject targetPanel, GameObject focusButton)
    {
        HideAllPanels();
        targetPanel.SetActive(true);
        _currentFocusButton = focusButton;
        StartCoroutine(SelectAfterFrame(focusButton));
    }



    public void ShowMainMenu()
    {
        SwitchPanel(MainMenuPanel, start_Button);

    }

        public void ShowStatusPanel()
    {
        HPCanvas.SetActive(false);
        TimerCanvas.SetActive(false);
        SwitchPanel(StatusPanel, status_backButton);
    }

    public void ShowAudioSettings()
    {
        SwitchPanel(audioSettingPanel, BGM_SE_backButton);

    }


    public void ShowSensitivitySettings()
    {
        SwitchPanel(sensitivetyPanel, Camera_backButton);
    }

    public void OpenMenu()
    {
        settingCanvas.SetActive(true);
        HPCanvas.SetActive(false);
        TimerCanvas.SetActive(false);

        //新しいマネージャーに操作の切り替えを依頼
        if(TO_UI_OnlyManager.Instance != null)
        {
            TO_UI_OnlyManager.Instance.SetUIMode(true);
        }

        ShowMainMenu();
    }

    public void CloseMenu()
    {
        settingCanvas.SetActive(false);
        HideAllPanels();
        HPCanvas.SetActive(true);
        TimerCanvas.SetActive(true);

        if(TO_UI_OnlyManager.Instance !=null)
        {
            TO_UI_OnlyManager.Instance.SetUIMode(false);
        }

    }

    public void CloseLevelUpTips()
    {
        Debug.Log("OKボタンが正しく押されました！"); // これが出るか確認
        LevelUp_tipsCanvas.SetActive(false);
        if(TO_UI_OnlyManager.Instance !=null)
        {
            TO_UI_OnlyManager.Instance.SetUIMode(false);
        }

       
    }

    private System.Collections.IEnumerator SelectAfterFrame(GameObject obj)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; //ここで1フレーム待つ
        EventSystem.current.SetSelectedGameObject(obj);

    }


}
