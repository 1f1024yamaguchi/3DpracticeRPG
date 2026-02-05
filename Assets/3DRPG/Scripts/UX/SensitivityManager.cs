using UnityEngine;
using UnityEngine.UI;

public class SensitivityManager : MonoBehaviour
{
    [SerializeField] private Slider sensitivitySlider;


    public static float Sensitivity {get; private set; } = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("sensitivity", 1.0f);
        sensitivitySlider.value = savedSensitivity;
        Sensitivity = savedSensitivity;

        //スライダーの値が変更されたときのイベントを登録
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        
    }

    public void SetSensitivity(float value)
    {
        Sensitivity = value;
        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
