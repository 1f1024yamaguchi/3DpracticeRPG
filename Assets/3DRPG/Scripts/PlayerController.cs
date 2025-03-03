using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3; //移動速度
    [SerializeField] private float jumpPower = 3; //ジャンプ力
    [SerializeField] private Transform cameraTransform; //カメラのTransform
    [SerializeField] private float rotationSpeed = 1f; //回転速度
    [SerializeField] private Animator animator;
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
        animator = GetComponent<Animator>();

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
        //カメラの向きを考慮して移動方向を決定
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        //上方向への影響を除外(x,y平面のみで計算)
        forward.y =0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        //入力値をカメラの向きに合わせた移動ベクトルに変換
        Vector3 moveDirection = (forward * moveValue.y + right * moveValue.x).normalized;
        _moveVelocity.x = moveDirection.x * moveSpeed;
        _moveVelocity.z = moveDirection.z * moveSpeed;


 
        //キャラクターの向きを移動方向に向ける
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, Time.deltaTime  * rotationSpeed);
        }

        if (_characterController.isGrounded)
        {
            if (_jump.WasPressedThisFrame())
            {
                //ジャンプ処理
                Debug.Log("ジャンプ!");
                _moveVelocity.y = jumpPower; //ジャンプの際は上方向に移動させる
                animator.SetBool("IsJumping", true); //ジャンプアニメーションを開始
            }
            else
            {
                animator.SetBool("IsJumping", false); //着地時にジャンプアニメーションを終了する
            }
        }
        else
        {
            //重力による加速
            _moveVelocity.y += Physics.gravity.y * Time.deltaTime;

        }

        //オブジェクトを動かす
        _characterController.Move(_moveVelocity * Time.deltaTime);

        //移動スピードをanimatorに反映する
        animator.SetFloat("MoveSpeed", new Vector3(_moveVelocity.x, 0, _moveVelocity.z).magnitude);
        float moveSpeedValue = new Vector3(_moveVelocity.x, 0, _moveVelocity.z).magnitude;
        Debug.Log("MoveSpeed: " + moveSpeedValue);
        animator.SetFloat("MoveSpeed", moveSpeedValue);
    }
}
