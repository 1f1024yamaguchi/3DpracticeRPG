using UnityEngine;
using System.Collections;
public class PlayerEffectManager : MonoBehaviour
{

    private PlayerController _playerController;
    private MobStatus _mobStatus;
    private float baseSpeed;
    private float baseJump;
    private PlayerStatus _playerStatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _mobStatus = GetComponent<MobStatus>();
        baseSpeed = _playerController.moveSpeed;
        baseJump = _playerController.jumpPower;
        _playerStatus = GetComponent<PlayerStatus>(); //playerStatusを直接取得
        
        
    }

    public void ApplyItemEffect(Item.ItemType itemType)
    {
        switch (itemType)
        {
            case Item.ItemType.SpeedUp:
            //10秒間、移動速度とジャンプ力を1.5倍にするコルーチンを開始
                StartCoroutine(SpeedUpCoroutine(10f,1.5f));
                break;
            
            case Item.ItemType.Attack_Power:
            //PlayerStatusの強化メソッドを呼び出す
                
                if(_playerStatus != null)
                {
                    _playerStatus.ApplyAttackBuff(30f,2); //30秒間攻撃力二倍
                }
                break;

            
        }
    }

    private IEnumerator SpeedUpCoroutine(float duration, float multipliter)
    {
        
        

        //速度を上げる
        _playerController.moveSpeed =baseSpeed* multipliter;
        _playerController.jumpPower =baseJump* multipliter;
        Debug.Log("現在の速度" + _playerController.moveSpeed);

        //指定された時間待つ
        yield return new WaitForSeconds(duration);

        //速度を元に戻す
        _playerController.moveSpeed = baseSpeed;
        _playerController.jumpPower = baseJump;
        Debug.Log("スピードアップ効果終了。");

    }
}
