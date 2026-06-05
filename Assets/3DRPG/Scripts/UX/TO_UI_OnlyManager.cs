using UnityEngine;
using UnityEngine.InputSystem;

public class TO_UI_OnlyManager : MonoBehaviour
{
    public static TO_UI_OnlyManager Instance;
    private PlayerInput _playerInput;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        _playerInput = GetComponent<PlayerInput>();
    }

    public void SetUIMode(bool isUIOnly, bool stopTime = true, bool keepPlayerControl = false)
    {
        if(isUIOnly)
        {
            if (stopTime) Time.timeScale = 0f;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_playerInput != null)
            {
                if (keepPlayerControl)
                {
                    // ★修正：Switchを使わず、UIマップを「追加で有効化」するだけにする
                    // （これで移動キーがリセットされず、走り続けられます！）
                    _playerInput.actions.FindActionMap("UI").Enable();
                }
                else
                {
                    // ポーズ画面などは今まで通り操作を完全に奪う
                    _playerInput.SwitchCurrentActionMap("UI"); 
                }
            }
        }
        else
        {
            Time.timeScale = 1f; 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (_playerInput != null)
            {
                // UI操作を無効化し、操作をPlayerに完全に戻す
                _playerInput.actions.FindActionMap("UI").Disable();

                if(_playerInput.currentActionMap.name != "Player")
                {
                    _playerInput.SwitchCurrentActionMap("Player");
                }
                
            }
        }
    }
}