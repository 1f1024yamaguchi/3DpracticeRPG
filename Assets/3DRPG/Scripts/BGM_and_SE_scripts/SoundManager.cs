using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    //[SerializeField] private AudioSource bgmAudioSource;

    [Header("Start Scene BGM")]
    [Tooltip("Startシーンで自動再生するBGMクリップ（未設定なら再生しない）")]
    
    [SerializeField] private AudioClip startSceneBGM;

    // GenericMenuItemのSlider最大値と合わせること（Inspector上のmaxValue）
    [SerializeField] private int sliderMaxValue = 10;

    [Header("Audio Resources")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmAudioSource;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SE_VOLUME_KEY = "SEVolume";

    // MixerのExposed Parametersの名前と一致させる
    private const string MIXER_BGM = "BGM";
    private const string MIXER_SE = "SE";



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 保存された音量を復元
            LoadAndApplyVolumes();

            // StartシーンBGMを自動再生
            if (startSceneBGM != null)
            {
                PlayBGM(startSceneBGM);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAndApplyVolumes()
    {
        float savedBGM = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
        SetBGMVolume(savedBGM);

        float savedSE = PlayerPrefs.GetFloat(SE_VOLUME_KEY, 0.5f);
        SetSEVolume(savedSE);
    }

    public void SetBGMVolume(float volume)
    {
        ApplyVolumeToMixer(MIXER_BGM, volume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
    }



    public void SetSEVolume(float volume)
    {
        ApplyVolumeToMixer(MIXER_SE, volume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, volume);
        
    }



    /// <summary>
    /// AutoMenuGenerator の GenericMenuItem (Slider, OnValueChanged<int>) から呼ぶ用。
    /// int値(0〜sliderMaxValue) を float(0.0〜1.0) に変換して適用・保存する。
    /// </summary>
    public void SetBGMVolumeFromMenu(int value)
    {
        float volume = (float)value / sliderMaxValue;
        SetBGMVolume(volume);
    }

    public void SetSEVolumeFromMenu(int value)
    {
        float volume = (float)value / sliderMaxValue;
        SetSEVolume(volume);
    }

    // --- 共通処理：Linear(0-1) を Decibel(-80-20) に変換してMixerに適用 ---
    private void ApplyVolumeToMixer(string parameterName, float volume)
    {
        float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;

        audioMixer.SetFloat(parameterName, db);

    }

    // ★ここを追加：BGMを切り替えて再生するメソッド
    public void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource.clip == clip)
        {
            if (!bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Play();
            }
            return;

        }
        // すでに同じ曲が流れている場合は何もしない（ループ再生対策）
        if (bgmAudioSource.clip == clip) return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }
}