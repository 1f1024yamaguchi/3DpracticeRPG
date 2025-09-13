using UnityEngine;

public class BgmTrigger : MonoBehaviour
{
    // インスペクターからBGMを再生するAudioSourceを割り当てる
    public AudioSource bgmSource;

    // このトリガー範囲に入った時に呼ばれるメソッド
    private void OnTriggerEnter(Collider other)
    {
        // 侵入したオブジェクトがプレイヤーだったら
        if (other.CompareTag("Player"))
        {
            // BGMが再生中でなければ再生する
            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }
    }

    // このトリガー範囲から出た時に呼ばれるメソッド
    private void OnTriggerExit(Collider other)
    {
        // 出ていったオブジェクトがプレイヤーだったら
        if (other.CompareTag("Player"))
        {
            // BGMが再生中であれば停止する
            if (bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
        }
    }
}