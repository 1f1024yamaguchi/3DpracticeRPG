using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputActionTest : MonoBehaviour
{
    private InputAction _move;
    private InputAction _jump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var input = GetComponent<PlayerInput>();

        input.currentActionMap.Enable();
        _move = input.currentActionMap.FindAction("Move");
        _jump = input.currentActionMap.FindAction("Jump");
        
    }

    // Update is called once per frame
    void Update()
    {
        var moveValue = _move.ReadValue<Vector2>();
        if (moveValue.magnitude > 0f)
        {
            Debug.Log($"Moveアクションの値: {moveValue}");
        }
        if (_jump.WasPressedThisFrame())
        {
            Debug.Log("Jumpアクションが実行されたよ");
        }
    }
}
