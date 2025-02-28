using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3; //移動速度
    [SerializeField] private float jumpPower = 3; //ジャンプ力
    private CharacterController _characterController; 
    private Transform _transform; 
    private Vector3 _moveVelocity;
    private InputAction _move;
    private InputAction _jump;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _transform = transform;

        var input = GetComponent<PlayerInput>();
        input.currentActionMap.Enable();
        //アクションマップからアクションを取得するにはFindAction()を使う
        _move = input.currentActionMap.FindAction("Move");
        _jump = input.currentActionMap.FindAction("Jump");

    }


    // Update is called once per frame
    void Update()
    {
        Debug.Log(_characterController.isGrounded ? "地面にいます" : "空中です");

        //moveアクションを使った移動処理（完成を無視しているのでキビキビ動く）
        var moveValue = _move.ReadValue<Vector2>();
        _moveVelocity.x = moveValue.x * moveSpeed;
        _moveVelocity.z = moveValue.y * moveSpeed;

        //移動方向に向く
        _transform.LookAt(_transform.position + new Vector3(_moveVelocity.x, 0 , _moveVelocity.z));

        if (_characterController.isGrounded)
        {
            if (_jump.WasPressedThisFrame())
            {
                //ジャンプ処理
                Debug.Log("ジャンプ!");
                _moveVelocity.y = jumpPower; //ジャンプの際は上方向に移動させる
            }
        }
        else
        {
            //重力による加速
            _moveVelocity.y += Physics.gravity.y * Time.deltaTime;

        }

        //オブジェクトを動かす
        _characterController.Move(_moveVelocity * Time.deltaTime);
    }
}
