using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;


public class SensitivityController : MonoBehaviour
{

    public Slider sensitivitySlider;
    public CinemachineInputAxisController axisController; //Cinemachineの設定に合わせて変更

    [Header("Speed settings")]
    [SerializeField] private float baseGainX = 1.0f; // X基本の感度設定
    [SerializeField] private float baseGainY = 1.0f; // Y基本の感度設定
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Additiveでシーンを読み込む（既に読み込まれている場合は注意が必要）
        //SceneManager.LoadScene("Main", LoadSceneMode.Additive);
        //sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        //保存された感度を読み込み
        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 1.0f);
        sensitivitySlider.value = savedSensitivity;

        ApplySensitivity(savedSensitivity);

        //スライダー変更時のイベント登録
        sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);

        
    }

    public void ApplySensitivity(float value)
    {

        Debug.Log($"感度適用中: {value} (BaseX: {baseGainX})");
        if(axisController != null)
        {
            axisController.Controllers[0].Input.Gain = baseGainX * value;
            axisController.Controllers[1].Input.Gain = baseGainY * value;

        }

        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
