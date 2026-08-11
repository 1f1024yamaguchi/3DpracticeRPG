using UnityEngine;

/// <summary>
/// パーティクルをターゲット(コア)へ半径方向に引き寄せる吸い込みアトラクター。
/// 回転(渦)はこのスクリプトでは行わず、パーティクルシステムの
/// Velocity over Lifetime > Orbital 側で制御する。
/// ここでは「中心へ寄せる」半径方向の移動と、中心到達時の吸収のみを担当する。
/// 編集プレビュー/Playの両方で動作。
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor : MonoBehaviour
{
    [Tooltip("吸い込み先。未設定なら親のTransformを使用する")]
    public Transform target;

    [Tooltip("中心へ吸い込む速さ(単位/秒)")]
    public float inwardSpeed = 90f;

    [Tooltip("中心に近づくほど加速する割合(0で一定速度)")]
    [Range(0f, 4f)] public float accelerateNearCenter = 2f;

    [Tooltip("この距離まで来たら吸収して消滅させる")]
    public float killRadius = 2.5f;

    ParticleSystem _ps;
    ParticleSystem.Particle[] _buf;

    void OnEnable() { _ps = GetComponent<ParticleSystem>(); }

    void LateUpdate()
    {
        if (_ps == null) _ps = GetComponent<ParticleSystem>();
        if (_ps == null) return;

        Transform t = target != null
            ? target
            : (transform.parent != null ? transform.parent : transform);

        int max = _ps.main.maxParticles;
        if (_buf == null || _buf.Length < max) _buf = new ParticleSystem.Particle[max];
        int count = _ps.GetParticles(_buf);
        if (count == 0) return;

        bool local = _ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        Vector3 center = local ? transform.InverseTransformPoint(t.position) : t.position;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = _buf[i].position - center;
            float dist = offset.magnitude;

            if (dist <= killRadius)
            {
                _buf[i].remainingLifetime = 0f;
                continue;
            }

            // 回転は加えず、現在の角度を保ったまま半径だけ縮めて中心へ寄せる。
            // (角度=渦回転はパーティクルシステムのOrbitalが決める)
            float speed = inwardSpeed * (1f + accelerateNearCenter * (1f - Mathf.Clamp01(dist / 60f)));
            float newDist = Mathf.MoveTowards(dist, 0f, speed * dt);

            _buf[i].position = center + offset.normalized * newDist;
        }

        _ps.SetParticles(_buf, count);
    }
}
