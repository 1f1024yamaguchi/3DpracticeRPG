using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterController))]

public class Magic : MonoBehaviour
{
    [SerializeField] private GameObject magicPrefab; //発射する魔法のプレハブ
    [SerializeField] private Transform firePoint; //魔法の発射地点


    private CharacterController _characterController; //CharacterControllerのキャッシュ
    private InputAction _magicAction; //マジックアクションのキャッシュ
    private Camera _mainCamera; //メインカメラのキャッシュ
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        var input = GetComponent<PlayerInput>();

        

        input.currentActionMap.Enable();
        _magicAction = input.currentActionMap.FindAction("Magic");
        _mainCamera = Camera.main; //メインカメラをキャッシュ
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_magicAction.WasPressedThisFrame())
        {
            Shoot();
        }
        
    }

    private void Shoot()
    {
        //プレハブや発射地点が設定されてなければ処理を中断
        if(magicPrefab == null || firePoint == null || _mainCamera == null)
        {
            Debug.Log("設定の不足");
            return;
        }

        //画面中央からレイを飛ばす
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f,0));
        Vector3 targetPoint;

        //レイが何かに当たったらそこが目標地点
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else //当たらなかったら100m先が目標地点
        {
            targetPoint = ray.GetPoint(100);
        }

        //発射地点から目標地点への方向を計算
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        //計算した方向に向けて魔法のプレハブ生成
        Quaternion rotation = Quaternion.LookRotation(direction);
        Instantiate(magicPrefab, firePoint.position, rotation);
    }
}
