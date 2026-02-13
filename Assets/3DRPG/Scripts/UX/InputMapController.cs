using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputMapController : MonoBehaviour
{
    [SerializeField] private UIManager_in_main uiManager;
    private PlayerInput _playerInput;

    private InputAction _playerMenuAction;
    private InputAction _uiMenuAction;
    private InputAction _menuAction;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        _playerMenuAction = _playerInput.actions.FindAction("Player/Menu");
        if(_playerMenuAction != null)
        {
            _playerMenuAction.performed += OnMenuPerformed;
        }
        _uiMenuAction = _playerInput.actions.FindAction("UI/Menu");

        if(_uiMenuAction != null)
        {
            _uiMenuAction.performed += OnMenuPerformed;
        }
        _menuAction = _playerInput.actions.FindAction("Menu");


    }

    void OnDisable()
    {
        //イベント解除
        if(_playerMenuAction != null)
        {
            _playerMenuAction.performed -= OnMenuPerformed;
        }
        if(_uiMenuAction != null)
        {
            _uiMenuAction.performed -= OnMenuPerformed;
        }
    }

 

    private void OnMenuPerformed(InputAction.CallbackContext context)
    {
        if (uiManager == null) return;

        if (!uiManager.IsMenuVisible())
        {
            uiManager.OpenMenu();
            _playerInput.SwitchCurrentActionMap("UI");
        }
        else
        {
            uiManager.CloseMenu();
            _playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
