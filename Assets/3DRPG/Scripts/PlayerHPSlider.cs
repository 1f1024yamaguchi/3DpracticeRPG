using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使うために必要
using TMPro; // TextMeshProを使うために必要
using DG.Tweening; //DoTweenを動かすために必要

public class PlayerHPSlider : MonoBehaviour
{
    [SerializeField] private Slider hpSlider; // InspectorからSliderコンポーネントを設定
    [SerializeField] private TextMeshProUGUI hpText; // HPのテキスト表示用
    private PlayerStatus _playerStatus;
    //private Color _originalTextColor; // 起動時の元の色を保持する

    void Start()
    {
        // Inspectorで設定した元の色を保存しておく
        // if (hpText != null)
        // {
        //     _originalTextColor = hpText.color;
        // }

        // シーン内にいるPlayerStatusを探してくる
        _playerStatus = FindObjectOfType<PlayerStatus>();

        if (_playerStatus != null)
        {
            // PlayerStatusのHP変更通知（OnLifeChanged）を受け取ったら、
            // UpdateSliderメソッドを実行するように予約する
            _playerStatus.OnLifeChanged += UpdateSlider;
            UpdateSlider(_playerStatus.DamagePercent, 999f); // 初期表示の更新

            
        }
        else
        {
            Debug.LogError("シーンにPlayerStatusが見つかりません！");
        }
    }

    private void OnDestroy()
    {
        // このオブジェクトが破壊されるときに、予約を解除する
        if (_playerStatus != null)
        {
            _playerStatus.OnLifeChanged -= UpdateSlider;
        }   
    }

    // HPの変更通知を受け取ったときに実行されるメソッド
    private void UpdateSlider(float currentLife, float maxLife)
    {
        // currentLifeは今はDamagePercent
        // スマブラ風に0〜300%などにする
        hpSlider.maxValue = 300f; // とりあえず最大300%までゲージが伸びるようにする
        hpSlider.value = currentLife;
        hpText.transform.DOShakePosition(0.5f, 10f, 30, 1, false); //ゲージ変化時少し揺れる

        hpText.text = $"{Mathf.FloorToInt(currentLife)} %";
        if (currentLife > 100f) 
        {
            hpText.color = Color.red; // 100%超えで赤くする
        }
        else if(currentLife >50f )
        {
            hpText.color = Color.yellow;
        }
        else 
        {
            hpText.color = Color.white;
            //hpText.color = _originalTextColor; // 元のテキスト色（黄色など）に戻す
        }
    }
}