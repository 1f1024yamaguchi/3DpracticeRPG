using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを使うために必要
using UnityEngine.EventSystems; // UIのフォーカス制御に必要


public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingCanvas; //表示・非表示するcanvas
    [SerializeField] private GameObject firstSelectedButton; //最初に選択しているボタン;
    [SerializeField] private GameObject mainPanel; //メインパネル
    private PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
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
        Debug.Log("ToggleMenuが呼ばれました！");
        //現在の状態を反映させる
        bool isActive = !settingCanvas.activeSelf;
        settingCanvas.SetActive(isActive);

        if (isActive)
        {
            if(mainPanel != null) mainPanel.SetActive(true);
            Cursor.visible = true; // カーソルを表示する
            Cursor.lockState = CursorLockMode.None; // カーソルのロックを解除する

            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            // ゲームを一時停止させる
            Time.timeScale =0;

            playerInput.SwitchCurrentActionMap("UI"); // Input Action Mapを「UI」に切り替え
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // カーソルを画面中央に固定する
            EventSystem.current.SetSelectedGameObject(null);
            // 再開させる
            Time.timeScale = 1;
            playerInput.SwitchCurrentActionMap("Player"); // Input Action Mapを「Player」に切り替え
        }
    }
}
