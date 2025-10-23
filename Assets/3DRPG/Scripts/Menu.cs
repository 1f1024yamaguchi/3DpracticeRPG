using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [SerializeField] private ItemsDialog itemsDialog;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;

    [SerializeField] private Button itemsButton;
    [SerializeField] private PlayerInput playerInput;

    private InputAction _inventoryAction; //Inventoryアクションを保持す変数数
    [SerializeField] private string gamepadSchemeName ="Gamepad";



    
    

    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //ポーズのパネルは初期状態では非表示にしておく

        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        itemsButton.onClick.AddListener(ToggleItemsDialog);
        
        //Inventoryという名前のアクションを取得する
        _inventoryAction = playerInput.actions["Inventory"];

        //コントロール変更イベントを登録
        playerInput.onControlsChanged += OnControlsChanged;

        //UIマップを無効化
        playerInput.actions.FindActionMap("UI").Disable();

        //ゲーム開始時にコンソール状態を更新
        UpdateCursorState(); 


        //Cursor.visible = false; //マウスカーソル非表示
        //Cursor.lockState = CursorLockMode.Locked;//カーソルロック
        
    
    }
    
    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    private void OnControlsChanged(PlayerInput input)
    {
        UpdateCursorState();
    }

    void Update()
    {
        if (_inventoryAction.WasPressedThisFrame())
        {
            if (!pausePanel.activeSelf)
            {
                ToggleItemsDialog();
            }
            
        }
    }

    //ゲームを一時停止する
    private void Pause()
    {
        Time.timeScale = 0; //Time.timeScaleで時間の流れの速さを決める。0だと時間が停止する。
        pausePanel.SetActive(true);

        playerInput.SwitchCurrentActionMap("UI");
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);

        UpdateCursorState();

    }
    //ゲームを再開する
    private void Resume()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);

        playerInput.SwitchCurrentActionMap("Player");
        UpdateCursorState();

        
    }

    
    private void ToggleItemsDialog()
    {
        itemsDialog.Toggle();

        if(itemsDialog.gameObject.activeSelf) //アイテムが開いている
        {
            //UIマップを有効にする
            playerInput.actions.FindActionMap("UI").Enable();
            
        }
        else
        {
            //UIマップのみ無効化
            playerInput.actions.FindActionMap("UI").Disable();
        }

        //どちらの場合もカーソル状態を更新
        UpdateCursorState();        
    }

    //カーソルの表示・非表示を更新
    private void UpdateCursorState()
    {
        bool isUiOpen = itemsDialog.gameObject.activeSelf || pausePanel.activeSelf;
        bool isGamepad = playerInput.currentControlScheme == gamepadSchemeName;

        if (isUiOpen)
        {

            if (isGamepad)
            {
                //UIが開き、かつゲームパッド使用中はカーソル非表示
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

            }
            else
            {
                // UIが開き、かつキーボードマウス使用中 -> カーソル表示
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;


            }

        }

        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
}
