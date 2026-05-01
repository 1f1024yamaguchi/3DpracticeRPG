using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使うために必要
using TMPro; // TextMeshProを使うために必要

public class PlayerHPSlider : MonoBehaviour
{
    [SerializeField] private Slider hpSlider; // InspectorからSliderコンポーネントを設定
    [SerializeField] private TextMeshProUGUI hpText; // HPのテキスト表示用
    private PlayerStatus _playerStatus;

    void Start()
    {
        // シーン内にいるPlayerStatusを探してくる
        _playerStatus = FindObjectOfType<PlayerStatus>();

        if (_playerStatus != null)
        {
            // PlayerStatusのHP変更通知（OnLifeChanged）を受け取ったら、
            // UpdateSliderメソッドを実行するように予約する
            _playerStatus.OnLifeChanged += UpdateSlider;
            UpdateSlider(_playerStatus._life, _playerStatus.lifeMax); // 初期表示の更新

            
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
        // Sliderの最大値をキャラクターの最大HPに設定
        hpSlider.maxValue = maxLife;
        // Sliderの現在の値をキャラクターの現在HPに設定
        hpSlider.value = currentLife;

        hpText.text = $"HP: {currentLife} / {maxLife}";
    }
}