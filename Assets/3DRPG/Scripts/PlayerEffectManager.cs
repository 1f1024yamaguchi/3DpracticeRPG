using UnityEngine;
using System.Collections;
public class PlayerEffectManager : MonoBehaviour
{
    [Header("効果音クリップ")]
    [SerializeField] private AudioClip powerUpSound;
    [SerializeField] private AudioClip healSound;
    private AudioSource _audioSource;

    [SerializeField] private ParticleSystem speedBuffParticles; 
    [SerializeField] private ParticleSystem attackBuffParticles;

    private Coroutine _speedUpCoroutine; // スピードアップのコルーチンを保持
    private Coroutine _attackUpCoroutine; // ★★★ 攻撃力アップ用コルーチンを追加

    private PlayerController _playerController;
    private MobStatus _mobStatus;
    private float baseSpeed;
    private float baseJump;
    private float baseRun;
    private PlayerStatus _playerStatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
        

        
        _playerController = GetComponent<PlayerController>();
        _mobStatus = GetComponent<MobStatus>();
        baseSpeed = _playerController.moveSpeed;
        baseJump = _playerController.jumpPower;
        baseRun = _playerController.runSpeed;
        _playerStatus = GetComponent<PlayerStatus>(); //playerStatusを直接取得

        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (speedBuffParticles != null) speedBuffParticles.Stop();
        if (attackBuffParticles != null) attackBuffParticles.Stop();
        
        
    }

    public void ApplyItemEffect(Item.ItemType itemType)
    {
        switch (itemType)
        {
            case Item.ItemType.SpeedUp:
                if (_speedUpCoroutine != null)
                {
                    StopCoroutine(_speedUpCoroutine);
          
                    
                }

            
            //10秒間、移動速度とジャンプ力を1.5倍にするコルーチンを開始
                _speedUpCoroutine = StartCoroutine(SpeedUpCoroutine(30f,1.5f));
                
                
                break;
            
            case Item.ItemType.Attack_Power:
            //PlayerStatusの強化メソッドを呼び出す
                
                
               
                if (_attackUpCoroutine != null)
                {
                    StopCoroutine(_attackUpCoroutine);
                } 
                _attackUpCoroutine = StartCoroutine(AttackUpCoroutine(30f, 2));
                break;

            case Item.ItemType.Heal_Potion:

                _mobStatus.Heal(25);
                PlaySound(healSound);
                break;

            
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator SpeedUpCoroutine(float duration, float multipliter)
    {
        PlaySound(powerUpSound); // 音を鳴らす
        if (speedBuffParticles != null) speedBuffParticles.Play(); // パーティクル再生

        
        //速度を上げる
        _playerController.moveSpeed =baseSpeed* multipliter;
        _playerController.jumpPower =baseJump* multipliter;
        _playerController.runSpeed = baseRun * multipliter;
        Debug.Log("現在の速度" + _playerController.moveSpeed);

        //指定された時間待つ
        yield return new WaitForSeconds(duration);

        //速度を元に戻す
        _playerController.moveSpeed = baseSpeed;
        _playerController.jumpPower = baseJump;
        _playerController.runSpeed = baseRun;
        Debug.Log("スピードアップ効果終了。");
        if (speedBuffParticles != null) speedBuffParticles.Stop();
        _speedUpCoroutine = null; // コルーチンが終了したことを示す

    }
    private IEnumerator AttackUpCoroutine(float duration, int multiplier)
    {
        PlaySound(powerUpSound);
        if (attackBuffParticles != null) attackBuffParticles.Play();
        _playerStatus.BuffAttackPower(multiplier);
        //指定時間待つ
        yield return new WaitForSeconds(duration);

        // PlayerStatusの元に戻す命令を呼び出す
        _playerStatus.ResetAttackPower();

        if (attackBuffParticles != null) attackBuffParticles.Stop();
        _attackUpCoroutine = null;
    }
}
