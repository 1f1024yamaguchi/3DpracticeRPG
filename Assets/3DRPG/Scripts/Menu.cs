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
    
    private InputAction _inventoryAction; 
    [SerializeField] private string gamepadSchemeName ="Gamepad";

    void Start()
    {
        pausePanel.SetActive(false); 

        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        itemsButton.onClick.AddListener(ToggleItemsDialog);
        
        _inventoryAction = playerInput.actions["Inventory"];
        
        playerInput.onControlsChanged += OnControlsChanged;
        playerInput.actions.FindActionMap("UI").Disable();
        UpdateCursorState(); 
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
        // ★修正：プレイヤーの操作が維持される仕様になったため、
        // 「開く」のも「閉じる」のもこの一つの処理だけで完璧に動きます！
        if (_inventoryAction != null && _inventoryAction.WasPressedThisFrame())
        {
            // ポーズ画面が開いていない時だけ許可
            if (!pausePanel.activeSelf)
            {
                ToggleItemsDialog();
            }
        }
    }

    private void Pause()
    {
        pausePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);

        if (TO_UI_OnlyManager.Instance != null)
        {
            TO_UI_OnlyManager.Instance.SetUIMode(true, true, false); 
        }
    }

    private void Resume()
    {
        pausePanel.SetActive(false);

        if (TO_UI_OnlyManager.Instance != null)
        {
            TO_UI_OnlyManager.Instance.SetUIMode(false);
        }
    }

    private void ToggleItemsDialog()
    {
        itemsDialog.Toggle();

        if(itemsDialog.gameObject.activeSelf) 
        {
            if (TO_UI_OnlyManager.Instance != null) 
            {
                // インベントリは時間止めない(false)、プレイヤー操作残す(true)
                TO_UI_OnlyManager.Instance.SetUIMode(true, false, true);
            }
        }
        else
        {
            if (TO_UI_OnlyManager.Instance != null)
            {
                TO_UI_OnlyManager.Instance.SetUIMode(false);
            } 
        }
    }

    private void UpdateCursorState()
    {
        bool isUiOpen = itemsDialog.gameObject.activeSelf || pausePanel.activeSelf;
        bool isGamepad = playerInput.currentControlScheme == gamepadSchemeName;

        if (isUiOpen)
        {
            if (isGamepad)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
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