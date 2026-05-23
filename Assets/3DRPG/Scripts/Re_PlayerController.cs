using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(MobAttack))]
public class Re_PlayerController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 3f; 
    [SerializeField] public float jumpPower = 3f; 
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private float rotationSpeed = 1f; 
    [SerializeField] private Animator animator;
    [SerializeField] public float runSpeed = 6f; 
    [SerializeField] private float _knockbackDrag = 4f; 

    [Header("Jump Settings (Long Press Variable)")]
    [SerializeField] private float maxJumpPower = 15f; 
    [SerializeField] private float minJumpPower = 5f;  
    [SerializeField] private float forwardJumpSpeed = 5f; 
    [SerializeField] private float chargeMaxTime = 2.0f; 

    [Header("斬撃エフェクト設定")]
    [SerializeField] public GameObject[] slashEffectPrefabs; 
    [SerializeField] private Transform effectSpawnPoint;      
    [SerializeField] private float effectDuration = 1.0f;  

    [Header("UI設定")]
    [SerializeField] public TextMeshProUGUI chargeText; 

    private CharacterController _characterController; 
    private Transform _transform; 
    private Vector3 _moveVelocity; 
    private InputAction _move;
    private InputAction _jump;       
    private InputAction _attack;
    private InputAction _guard;
    private InputAction _run;

    private PlayerStatus _status;
    private MobAttack _mobAttack;
    private MobStatus mobStatus;

    private PlayerInput _playerInput; 

    public AudioClip sound1;
    AudioSource audioSource;

    public bool isRunning { get; private set; }
    private Vector3 _knockbackVector; 

    private float _currentChargedJumpPower = 0f;
    private float _chargeTimer = 0f;
    private bool _isChargingJump = false;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _transform = transform;
        _status = GetComponent<PlayerStatus>();
        _mobAttack = GetComponent<MobAttack>();
        animator = GetComponent<Animator>();

        _playerInput = GetComponent<PlayerInput>(); 
        _playerInput.currentActionMap.Enable();
        
        _move = _playerInput.currentActionMap.FindAction("Move");
        _jump = _playerInput.currentActionMap.FindAction("Jump");
        _attack = _playerInput.currentActionMap.FindAction("Attack");
        _guard = _playerInput.currentActionMap.FindAction("Guard");
        _run = _playerInput.currentActionMap.FindAction("Run");
        mobStatus = GetComponent<MobStatus>(); 

        audioSource = GetComponent<AudioSource>();
    }

    public void ApplyKnockback(Vector3 direction, float power)
    {
        _knockbackVector = direction * power;
        _knockbackVector.y += power / 2; 
    }

    void Update()
    {
        if(_playerInput.currentActionMap.name == "UI")
        {
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        if (!_characterController.enabled) return;

        // ★【変更箇所】CharacterController標準の接地判定のみを使用
        bool isGrounded = _characterController.isGrounded;

        isRunning = false;

        // --- すっ飛ばし処理 ---
        if (_status.State == MobStatus.StateEnum.Knockback)
        {
            _knockbackVector = Vector3.Lerp(_knockbackVector, Vector3.zero, _knockbackDrag * Time.deltaTime);
            _knockbackVector.y += Physics.gravity.y * Time.deltaTime;
            _characterController.Move(_knockbackVector * Time.deltaTime);

            if(_knockbackVector.magnitude < 2f)
            {
                _status.GoToNormalStateIfPossible();
            }
            return; 
        }
        
        // --- ガード判定 ---
        bool isGuarding = _guard.IsPressed();

        if (isGuarding)
        {
            _status.GoToGuardStateIfPossible();
        }
        else
        {
            _status.GoToNormalStateIfPossible();
            animator.SetBool("IsGuarding", false); 
        }
        animator.SetBool("IsGuarding", isGuarding); 

        // --- 攻撃処理 ---
        if (_attack.WasPressedThisFrame())
        {
            _mobAttack.AttackIfPossible();
            _moveVelocity.x = 0f;
            _moveVelocity.z = 0f;
            audioSource.PlayOneShot(sound1);
        }

        // --- 長押しジャンプのチャージ・発動ロジック ---
        bool shouldExecuteJump = false;

        if (isGrounded && !isGuarding && _status.IsMovable)
        {
            if (_jump.WasPressedThisFrame())
            {
                _isChargingJump = true;
                _chargeTimer = 0f;
                _currentChargedJumpPower = minJumpPower;
            }

            if (_isChargingJump)
            {
                _chargeTimer += Time.deltaTime;
                
                float chargeRatio = Mathf.Clamp01(_chargeTimer / chargeMaxTime);
                _currentChargedJumpPower = Mathf.Lerp(minJumpPower, maxJumpPower, chargeRatio);
                
                if (chargeText != null) chargeText.text = _chargeTimer.ToString("F2");

                if (_chargeTimer >= chargeMaxTime)
                {
                    shouldExecuteJump = true;
                }
                else if (_jump.WasReleasedThisFrame())
                {
                    shouldExecuteJump = true;
                }
            }
        }
        else
        {
            if (_isChargingJump)
            {
                _isChargingJump = false;
                if (chargeText != null) chargeText.text = "0.00"; 
            }
        }

        // --- 水平方向の移動（歩き・走り・チャージ中の停止） ---
        if (_status.IsMovable && !isGuarding && isGrounded) 
        {
            if (_isChargingJump)
            {
                _moveVelocity.x = 0f;
                _moveVelocity.z = 0f;
            }
            else
            {
                isRunning = _run.IsPressed();      
                float currentSpeed = isRunning ? moveSpeed * 2f : moveSpeed;

                var moveValue = _move.ReadValue<Vector2>(); 
                Vector3 forward = cameraTransform.forward;  
                Vector3 right = cameraTransform.right;      
                
                forward.y = 0; right.y = 0;
                forward.Normalize(); right.Normalize();

                Vector3 moveDirection = (forward * moveValue.y + right * moveValue.x).normalized;
                _moveVelocity.x = moveDirection.x * currentSpeed;
                _moveVelocity.z = moveDirection.z * currentSpeed;

                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
        }
        else if (isGuarding) 
        {
            _moveVelocity.x = 0f;
            _moveVelocity.z = 0f;
        }

        // --- 垂直方向の処理（重力・ジャンプ実行） ---
        if (isGrounded)
        {
            // 地面にいる間、isGroundedの判定を安定させるための微小な下向きの力
            if (_moveVelocity.y < 0f)
            {
                _moveVelocity.y = -2f; 
            }

            if (shouldExecuteJump) 
            {
                _moveVelocity.y = _currentChargedJumpPower; 

                var moveValue = _move.ReadValue<Vector2>();
                float runMultiplier = _run.IsPressed() ? 1.5f : 1.0f;
                float forwardForce = moveValue.magnitude * forwardJumpSpeed * runMultiplier;

                if (moveValue.magnitude > 0.1f)
                {
                    Vector3 cameraForward = cameraTransform.forward;
                    Vector3 cameraRight = cameraTransform.right;

                    cameraForward.y = 0; cameraRight.y = 0;
                    cameraForward.Normalize(); cameraRight.Normalize();

                    Vector3 jumpDirection = (cameraForward * moveValue.y + cameraRight * moveValue.x).normalized;

                    _moveVelocity.x = jumpDirection.x * forwardForce;
                    _moveVelocity.z = jumpDirection.z * forwardForce;

                    _transform.rotation = Quaternion.LookRotation(jumpDirection);
                }
                
                animator.SetBool("IsJumping", true); 
                _isChargingJump = false; 
                if (chargeText != null) chargeText.text = "0.00"; 
            }
            else
            {
                animator.SetBool("IsJumping", false); 
            }
        }
        else
        {
            // 通常の重力加速
            if (_moveVelocity.y > -20f)
            {
                _moveVelocity.y += Physics.gravity.y * Time.deltaTime;
            }
            else
            {
                _moveVelocity.y = -20f;
            }
        }

        // 移動の確定
        _characterController.Move(_moveVelocity * Time.deltaTime);

        float moveSpeedValue = new Vector3(_moveVelocity.x, 0, _moveVelocity.z).magnitude;
        animator.SetFloat("MoveSpeed", moveSpeedValue);
    }

    void PlaySlashEffect(int effectIndex)
    {
        Transform spawnPoint = (effectSpawnPoint != null) ? effectSpawnPoint : this.transform;
        GameObject effectInstance = Instantiate(slashEffectPrefabs[effectIndex], spawnPoint.position, spawnPoint.rotation);
        Destroy(effectInstance, effectDuration);
    }

    public void PlaySlashEffect_Attack1() => PlaySlashEffect(0);
    public void PlayeSlashEffect_Attack2() => PlaySlashEffect(1);

    public void AddBaseSpeed(float amount) { moveSpeed += amount; }
    public void RemoveBaseSpeed(float amount) { moveSpeed -= amount; }
    public void AddBaseJump(float amount) { jumpPower += amount; }
    public void RemoveBaseJump(float amount) { jumpPower -= amount; }
}