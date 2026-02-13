using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] AudioSource bgmAudioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmAudioSource.volume = volume;
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