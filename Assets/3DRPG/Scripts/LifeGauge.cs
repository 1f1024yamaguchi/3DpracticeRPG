using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeGauge : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private RectTransform _parentRectTransform;
    private Camera _camera;
    private MobStatus _status;

    private void Update()
    {
        Refresh();
    }

    //ゲージを初期化する
    public void Initialize(RectTransform parentRectTransform, Camera camera, MobStatus status)
    {
        //座標の計算に使うパラメータを受け取り、保存しておく
        _parentRectTransform = parentRectTransform;
        _camera = camera;
        _status = status;
        Refresh();
    }

    //ゲージを更新する
    private void Refresh()
    {
        if (_status == null)
        {
            return;
        }
        //残りライフを表示する
        fillImage.fillAmount = _status.Life / _status.LifeMax;

        //対象mobの場所にゲージを移動する。world座標やLocal座標を変換するといはRectTransformUtilityを使う
        var screenPoint = _camera.WorldToScreenPoint(_status.transform.position);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, screenPoint, null, out localPoint);
        //ゲージがキャラに重なるので少し上にずらす
        transform.localPosition = localPoint + new Vector2(0,80);

    }

}
