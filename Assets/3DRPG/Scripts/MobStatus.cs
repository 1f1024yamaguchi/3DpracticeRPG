using UnityEngine;

public class MobStatus : MonoBehaviour
{
    protected enum StateEnum
    {
        Normal,//通常
        Attack,//攻撃中
        Die //死亡
    }

    //移動可能かどうか
    public bool IsMovable => StateEnum.Normal == _state;

    //攻撃可能かどうか
    public bool IsAttackable => StateEnum.Normal == _state;

    //ライフの最大値を返す
    public float LifeMax => lifeMax; 

    //ライフの値を返す
    public float Life => _life;

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
        if (_state == StateEnum.Die) return;
        _state = StateEnum.Normal;
    }



}
