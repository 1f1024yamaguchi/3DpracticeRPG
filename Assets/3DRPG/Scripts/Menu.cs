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

    // Update is called once per frame
    private void ToggleItemsDialog()
    {
        itemsDialog.Toggle();
        
    }
    
}
