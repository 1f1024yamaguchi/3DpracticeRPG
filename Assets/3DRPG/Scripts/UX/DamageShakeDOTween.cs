using UnityEngine;
using DG.Tweening;

public class DamageShakeDOTween : MonoBehaviour
{
    [Header("References")]
    [Tooltip("揺らす対象（子オブジェクトの3Dモデルを指定）")]
    [SerializeField] private Transform modelTransform;

    [Header("shake setting")]

    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float strength = 0.5f;


    private Vector3 initialPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (modelTransform != null)
        {
            initialPosition = modelTransform.localPosition;
        }
        

    }

    public void PlayShakeEffect()
    {
        if(modelTransform == null)
        {
            return;
        }
        // 1. 進行中のTweenを完全に破棄（Completeだと座標がズレたまま終わるリスクがあるためKill）
        modelTransform.DOKill(); 

        // 2. 揺らす前に、必ず初期のローカル座標にリセットする（連続ヒット時の位置ズレ防止）
        modelTransform.localPosition = initialPosition;

        //３ローカル座標に対して揺れを適用する
        modelTransform.DOShakePosition(duration, strength, vibrato:10, randomness:90, snapping:false, fadeOut:true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
