using UnityEngine;
using Unity.Cinemachine;

public class ApplySensitivityMain : MonoBehaviour
{
    public CinemachineInputAxisController axisController;
    [SerializeField] private float baseGainX = 1.0f;
    [SerializeField] private float baseGainY = 1.0f;

    // 前回の設定画面の状態を覚えておくための変数
    // (UIManager等の Opened_Audio_Setting を参照できるとより効率的です)

    void Update()
    {
        // 常にPlayerPrefsから最新の値を読み込んで適用する
        // ※毎フレームの読み込み負荷が気になる場合は、
        // 「設定画面が開いている時だけ実行する」という条件を加えるとベストです
        ApplyCurrentSensitivity();
    }

    private void ApplyCurrentSensitivity()
    {
        
        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 1.0f);
        
        if (axisController != null)
        {
            axisController.Controllers[0].Input.Gain = baseGainX * savedSensitivity;
            axisController.Controllers[1].Input.Gain = baseGainY * savedSensitivity;
        }
    }
}