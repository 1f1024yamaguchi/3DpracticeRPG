using UnityEngine;

// Flaps each child "wing pivot" up and down by rotating it about its own span (local X) axis.
[ExecuteAlways]
public class SeraphimWingFlap : MonoBehaviour
{
    public float amplitude = 22f;     // degrees of flap
    public float speed = 2.5f;        // flap speed
    public float phasePerWing = 0.6f; // offset so wings don't move in unison

    Transform[] wings;
    Quaternion[] baseRot;

    void OnEnable() { Capture(); }

    void Capture()
    {
        int n = transform.childCount;
        wings = new Transform[n];
        baseRot = new Quaternion[n];
        for (int i = 0; i < n; i++)
        {
            wings[i] = transform.GetChild(i);
            baseRot[i] = wings[i].localRotation;
        }
    }

    void Update()
    {
        if (wings == null || wings.Length != transform.childCount) Capture();

        float t;
#if UNITY_EDITOR
        t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
        t = Time.time;
#endif
        for (int i = 0; i < wings.Length; i++)
        {
            if (wings[i] == null) continue;
            float a = Mathf.Sin(t * speed + i * phasePerWing) * amplitude;
            wings[i].localRotation = baseRot[i] * Quaternion.AngleAxis(a, Vector3.right);
        }
    }
}
