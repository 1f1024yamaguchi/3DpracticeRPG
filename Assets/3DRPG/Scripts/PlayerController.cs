using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(MobAttack))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3; //移動速度
    [SerializeField] private float jumpPower = 3; //ジャンプ力
    [SerializeField] private Transform cameraTransform; //カメラのTransform
    [SerializeField] private float rotationSpeed = 1f; //回転速度
    [SerializeField] private Animator animator;
    private CharacterController _characterController; 
    private Transform _transform; 
    private Vector3 _moveVelocity; //キャラの移動速度情報
    private InputAction _move;
    private InputAction _jump;
    private InputAction _attack;
    private InputAction _guard;
    private PlayerStatus _status;
    private MobAttack _mobAttack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _transform = transform;
        _status = GetComponent<PlayerStatus>();
        _mobAttack = GetComponent<MobAttack>();
        animator = GetComponent<Animator>();

        var input = GetComponent<PlayerInput>();
        input.currentActionMap.Enable();
        //アクションマップからアクションを取得するにはFindAction()を使う
        _move = input.currentActionMap.FindAction("Move");
        _jump = input.currentActionMap.FindAction("Jump");
        _attack = input.currentActionMap.FindAction("Attack");
        _guard = input.currentActionMap.FindAction("Guard");

        if (_guard == null)
        {
            Debug.LogError("Guard アクションが見つかりません。InputActionMap を確認してください。");
        }

    }


    // Update is called once per frame
    void Update()
    {
        bool isGuarding = _guard.IsPressed();
        Debug.Log("IsGuarding: " + isGuarding);

        if (isGuarding)
        {
            _status.GoToGuardStateIfPossible();
        }

        if (!isGuarding)
        {
            animator.SetBool("IsGuarding" , false);
        }
        else
        {
            _status.GoToNormalStateIfPossible();
            
        }

        animator.SetBool("IsGuarding", isGuarding); //Animatorに反映

        Debug.Log(_characterController.isGrounded ? "地面にいます" : "空中です");

        if (_attack.WasPressedThisFrame())
        {
            //Attackアクション(マウス左クリックなどで)攻撃をする
            _mobAttack.AttackIfPossible();
            _moveVelocity.x =0f;
            _moveVelocity.z = 0f;
        }

        if (_status.IsMovable && !isGuarding) //移動可能な状態であればユーザー入力を移動に反映する、ガード注出ない場合のみ移動
        {
            var moveValue = _move.ReadValue<Vector2>(); // 移動の入力値を取得
            Vector3 forward = cameraTransform.forward;  // カメラの前方向
            Vector3 right = cameraTransform.right;      // カメラの右方向
            
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
        }
        else if (isGuarding) //ガード中のときは移動速度を0.01に固定（アニメーションをIdelにしない)
        
        {
            _moveVelocity.x = 0f;
            _moveVelocity.z = 0f;
        }
            //else
            //{
                //_moveVelocity.x =0;
                //_moveVelocity.z =0;
            //}
        

        

        

        if (_characterController.isGrounded)
        {
            if (_jump.WasPressedThisFrame() && !isGuarding) //ガード中はジャンプできない
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

        //移動スピードをanimatorに反映する(ガード中は0.01にする)
        
        //animator.SetFloat("MoveSpeed", new Vector3(_moveVelocity.x, 0, _moveVelocity.z).magnitude);
        float moveSpeedValue = new Vector3(_moveVelocity.x, 0, _moveVelocity.z).magnitude;
        Debug.Log("MoveSpeed: " + moveSpeedValue);
        animator.SetFloat("MoveSpeed", moveSpeedValue);
    }
}
