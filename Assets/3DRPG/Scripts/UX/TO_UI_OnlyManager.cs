using UnityEngine;
using UnityEngine.InputSystem;

public class TO_UI_OnlyManager : MonoBehaviour
{
    
    public static TO_UI_OnlyManager Instance;

    [Header("Player scripts to disable")]
    [SerializeField] private MonoBehaviour playerMovement; //移動用スクリプト
    [SerializeField] private MonoBehaviour playerAttack; //攻撃用スクリプト

    private PlayerInput _playerInput;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //シーンを跨いでこのオブジェクトを保持する （どこからでもTO_UI_OnlyManagerにアクセス可能）
        }

        // プレイヤーにアタッチされているPlayerInputを取得する
        _playerInput = GetComponent<PlayerInput>();
    }

    //uiのみの操作モードを切り替える 
    //trueならuiのみ、falseならゲーム復帰

    public void SetUIMode(bool isUIOnly)
    {
        if(isUIOnly)
        {
            Time.timeScale = 0f; //ゲーム時間を止める
            Cursor.visible = true; //マウスカーソルを表示
            Cursor.lockState = CursorLockMode.None; //カーソル固定解除

            // ★追加：入力をUI操作モードに切り替える（これでボタンが押せるようになる！）
            if (_playerInput != null)
            {
                _playerInput.SwitchCurrentActionMap("UI");
            }

            //プレイヤーの操作を無効にする

            if(playerMovement != null)
            {
                playerMovement.enabled = false; //移動の無効化
            } 
            if(playerAttack != null )
            {
                playerAttack.enabled = false; //攻撃の無効化
            } 
        }
        else
        {
            //ゲームプレイモード
            Time.timeScale = 1f;
            Cursor.visible = false; //マウスカーソルを非表示
            Cursor.lockState = CursorLockMode.Locked; //カーソルを中央固定

            // ★追加：入力をゲーム操作モードに戻す
            if (_playerInput != null)
            {
                _playerInput.SwitchCurrentActionMap("Player");
            }

            //プレイヤーの操作を有効に戻す
            if(playerMovement != null)
            {
                playerMovement.enabled = true; //移動の有効化
            }
            if(playerAttack != null)
            {
                playerAttack.enabled = true; //攻撃の有効化
            }

        }
    }

}
