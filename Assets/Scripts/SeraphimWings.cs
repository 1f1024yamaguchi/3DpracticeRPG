using UnityEngine;

// Arranges the wings as tilted bands that wrap AROUND the eye (an armillary embrace),
// and slowly rotates the whole cage so it keeps enveloping the eye.
[ExecuteAlways]
public class SeraphimWings : MonoBehaviour
{
    [Header("Slow rotation of the whole embrace (deg/sec)")]
    public float orbitSpeedX = 10f;
    public float orbitSpeedY = 16f;
    public float orbitSpeedZ = 8f;

    [Header("Spread of the wing bands")]
    public float tiltPerWing = 60f;   // each wing's band is tilted this much more than the last

    Transform[] pivots;
    float prevT;

    void OnEnable() { Gather(); prevT = Now(); }

    void Gather()
    {
        int n = transform.childCount;
        pivots = new Transform[n];
        for (int i = 0; i < n; i++)
        {
            pivots[i] = transform.GetChild(i);
            float ang = 30f + i * 60f;
            // spin the band around the view axis, then tilt it into 3D so the bands weave around the eye
            pivots[i].localRotation =
                Quaternion.AngleAxis(ang, Vector3.forward) *
                Quaternion.AngleAxis(i * tiltPerWing, Vector3.right);
        }
    }

    float Now()
    {
#if UNITY_EDITOR
        return Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
        return Time.time;
#endif
    }

    void Update()
    {
        if (pivots == null || pivots.Length != transform.childCount) Gather();

        float t = Now();
        float dt = Mathf.Clamp(t - prevT, 0f, 0.1f);
        prevT = t;

        transform.Rotate(orbitSpeedX * dt, orbitSpeedY * dt, orbitSpeedZ * dt, Space.Self);
    }
}
