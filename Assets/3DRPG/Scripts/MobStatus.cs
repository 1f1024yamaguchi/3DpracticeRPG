using UnityEngine;
using System.Linq;

public class MobStatus : MonoBehaviour
{
    protected enum StateEnum
    {
        Normal,//通常
        Attack,//攻撃中
        Die, //死亡
        Guard //ガード中 
    }

    //移動可能かどうか
    public bool IsMovable => StateEnum.Normal == _state;

    //攻撃可能かどうか
    public bool IsAttackable => StateEnum.Normal == _state;

    //ライフの最大値を返す
    public float LifeMax => lifeMax; 

    //ライフの値を返す
    public float Life => _life;

    public bool IsGuarding => _state ==StateEnum.Guard;

    [SerializeField] private float lifeMax =10; //ライフ最大値
    protected Animator _animator;
    protected StateEnum _state = StateEnum.Normal; //Mob状態
    private float _life; //現在のライフ値(ヒットポイント)

    protected virtual void Start()
    {
        _life = lifeMax;
        _animator = GetComponentInChildren<Animator>();
    }

    //キャラクターが倒れた時の処理を記述する
    protected virtual void OnDie()
    {

    }

    //指定値のダメージを受ける
    public void Damage(int damage)
    {
        if (_state == StateEnum.Die) return;
        Debug.Log($"現在のステート: {_state}, IsGuarding: {IsGuarding}");

        if (IsGuarding)
        {
            Debug.Log("ガード成功");
            return; //ガード中ならダメージを受けない
        }

        _life -= damage;
        if (_life > 0) return;

        _state = StateEnum.Die;
        _animator.SetTrigger("Die");

        OnDie();
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

        //MobはIsGuardingを持たないので設定しないようにする。

        if (_animator.parameters.Any(p => p.name =="IsGuarding"))
        {
            _animator.SetBool("IsGuarding" , false); 
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
        Debug.Log("ガード状態になった！ 現在のステート: " + _state);
    }



}
