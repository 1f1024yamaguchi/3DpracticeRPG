using UnityEngine;
using System.Linq;
using System;

public class MobStatus : MonoBehaviour
{
    // Hpが変化したことを通知するイベント
    //引数：現在のhp,最大hp
    public event Action<float, float> OnLifeChanged;
    public enum StateEnum
    {
        Normal,//通常
        Attack,//攻撃中
        Die, //死亡
        Guard, //ガード中 
        Knockback //ノックバック中
    }

    //移動可能かどうか
    public bool IsMovable => StateEnum.Normal == _state;

    //攻撃可能かどうか
    public bool IsAttackable => StateEnum.Normal == _state;
    // "Normal"状態の時のみ移動・攻撃を許可する

    //ライフの最大値を返す
    public float LifeMax => lifeMax; 

    //ライフの値を返す
    public float Life => _life;

    public bool IsGuarding => _state ==StateEnum.Guard;
    public StateEnum State => _state; // 現在の状態を外部に公開するためのプロパティ

    [SerializeField] private float lifeMax =10; //ライフ最大値
    [SerializeField] protected float knockbackResistance = 1f; // この値が高いほど吹っ飛ばされにくい
    [SerializeField] protected int attackPower = 2; //キャラクターの基本攻撃力

    
    public int AttackPower => attackPower; //外部から攻撃力を読み取るための窓口

    protected Animator _animator;
    protected StateEnum _state = StateEnum.Normal; //Mob状態
    private float _life; //現在のライフ値(ヒットポイント)
    
    


    protected virtual void Start()
    {
        

        _life = lifeMax;
        
        _animator = GetComponentInChildren<Animator>();
        

        //ゲーム開始時にhp情報を通知
        OnLifeChanged?.Invoke(_life, lifeMax);     
    }

    //キャラクターが倒れた時の処理を記述する
    protected virtual void OnDie()
    {
        //ライフゲージの表示を終了する
        //LifeGaugeContainer.Instance.Remove(this);
    }

    //指定値のダメージを受ける
    public virtual void Damage(int damage)
    {
        if (_state == StateEnum.Die || _state == StateEnum.Guard) return;
        // if (_state == StateEnum.Die) return;
        // Debug.Log($"現在のステート: {_state}, IsGuarding: {IsGuarding}");

        // if (IsGuarding)
        // {
        //     Debug.Log("ガード成功");
        //     damage =0; //攻撃を完全に無効化
        //     return; //ガード中ならダメージを受けない
        // }

        _life -= damage;
        //Debug.Log($"ダメージを受けた: {damage}, 残りライフ: {_life}");

        //ダメージを受けたことを通知
        OnLifeChanged?.Invoke(_life, lifeMax);


        if (_life > 0) return;

        _state = StateEnum.Die;
        _animator.SetTrigger("Die");

        OnDie();
    }
    public virtual void Damage(int damage, Vector3 attackDirection, float baseKnockbackPower)
    {
        // 中身は、引数1つのDamageを呼び出す形がシンプルで良いです
        Damage(damage);
    }

   
    

    //可能であれば攻撃中の状態に移行する
    public void GoToAttackStateIfPossible()
    {
        if (!IsAttackable) return;

        _state = StateEnum.Attack;

        _animator.SetTrigger("Attack");
    }

    //可能であればNormalの状態に移行する
    public void GoToNormalStateIfPossible()
    {
        if (_state == StateEnum.Die  ) return;
        _state = StateEnum.Normal;
        //Debug.Log("Normal状態に移行しました。現在のステート: " + _state);

        //MobはIsGuardingを持たないので設定しないようにする。

        if (_animator.parameters.Any(p => p.name =="IsGuarding"))
        {
            _animator.SetBool("IsGuarding" , false); 
            //Debug.Log("ガード解除！現在のステート: " + _state);
        }

        
    }

    public void CancelGuard()
    {
        if (_state == StateEnum.Guard)
        {
            GoToNormalStateIfPossible();
        }
    }

    public void GoToGuardStateIfPossible()
    {
        if (_state ==StateEnum.Die) 
        {
            return;//死亡中ならガードできない
        }
        _state = StateEnum.Guard;
        //_animator.SetTrigger("Guard"); //ガードアニメーション再生
        _animator.SetBool("IsGuarding", true);
        //Debug.Log("ガード状態になった！ 現在のステート: " + _state);
    }

    public void Heal(int amount)
    {
        //死んでる場合は回復しない
        if(_state == StateEnum.Die) return;

        _life += amount;

        //最大HPは越えない
        _life = Mathf.Min(_life, lifeMax);

        OnLifeChanged?.Invoke(_life, lifeMax);
    }





}
