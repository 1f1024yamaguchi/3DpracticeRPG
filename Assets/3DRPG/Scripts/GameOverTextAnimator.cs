using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameOverTextAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var transformCache = transform;
        //終点として使用するため、初期座標を保持する
        var defaultPosition = transformCache.localPosition;

        transformCache.localPosition = new Vector3(0, 300f);

        transformCache.DOLocalMove(defaultPosition, 1f)
            .SetEase(Ease.Linear)
            .OnComplete(()=>
            {
                Debug.Log("GAME OVER");
                transformCache.DOShakePosition(1.5f, 100);
            });
        DOVirtual.DelayedCall(10, () =>
        {
            SceneManager.LoadScene("Start");
        });
    }


}
