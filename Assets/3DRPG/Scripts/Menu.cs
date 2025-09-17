using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    [SerializeField] private ItemsDialog itemsDialog;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;

    [SerializeField] private Button itemsButton;
    [SerializeField] private PlayerInput playerInput;

    private InputAction _inventoryAction; //Inventoryアクションを保持す変数数



    
    

    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //ポーズのパネルは初期状態では非表示にしておく

        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        itemsButton.onClick.AddListener(ToggleItemsDialog);
        
        //Inventoryという名前のアクションを取得する
        _inventoryAction = playerInput.actions["Inventory"];
        Cursor.visible = false; //マウスカーソル非表示
        Cursor.lockState = CursorLockMode.Locked;//カーソルロック
        
    
    }

    void Update()
    {
        if (_inventoryAction.WasPressedThisFrame())
        {
            ToggleItemsDialog();
        }
    }
    //ゲームを一時停止する
    private void Pause()
    {
        Time.timeScale = 0; //Time.timeScaleで時間の流れの速さを決める。0だと時間が停止する。
        pausePanel.SetActive(true);

    }
    //ゲームを再開する
    private void Resume()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    
    private void ToggleItemsDialog()
    {
        itemsDialog.Toggle();

        if(itemsDialog.gameObject.activeSelf) //アイテムが開いている
        {
            Cursor.visible = true; //マウスカーソルがあらわれる
            Cursor.lockState = CursorLockMode.None;//カーソルロック解除
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }
    
}
