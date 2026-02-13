using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip sceneBGM; // インスペクターで設定

    void Start()
    {
        // シーン開始時に SoundManager に曲を渡す
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayBGM(sceneBGM);
        }
    }
}