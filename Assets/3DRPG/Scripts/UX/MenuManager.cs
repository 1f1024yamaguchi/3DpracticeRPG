using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを使うために必要
using UnityEngine.EventSystems; // UIのフォーカス制御に必要


public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingCanvas; //表示・非表示するcanvas
    [SerializeField] private GameObject firstSelectedButton; //最初に選択しているボタン;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnToggleMenu(InputAction.CallbackContext context)
    {
        // キーが「押された瞬間」だけ実行する
        if (context.performed)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        //現在の状態を反映させる
        bool isActive = !settingCanvas.activeSelf;
        settingCanvas.SetActive(isActive);

        if (isActive)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            // ゲームを一時停止させる
            Time.timeScale =0;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            // 再開させる
            Time.timeScale = 1;
        }
    }
}
