using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting_Script : MonoBehaviour
{
    
    // スライダー
    public Slider BGMvolumeSlider;  
    public Slider SEvolumeSlider;  
    
    // オーディオソース
    public AudioSource BGMaudioSource;
    public AudioSource SEaudioSource;

    // BGM・SEリスト
    public List<AudioClip> BGMs;
    public List<AudioClip> SEs;

    // 音量管理システムが開かれているかどうか
    public bool Opened_Audio_Setting = false;

    [SerializeField] private GameObject BGM_SE_Panel;
    

    void Start()
    {
        if (BGM_SE_Panel != null)
        {
            BGM_SE_Panel.SetActive(false);
        }
       
        


        // 保存された音量を反映
        BGMvolumeSlider.value = Update_Volume.BGMsliderValue;
        SEvolumeSlider.value = Update_Volume.SEsliderValue;

        // 音量を適用
        ChangeVolumeBGM(Update_Volume.BGMsliderValue);
        ChangeVolumeSE(Update_Volume.SEsliderValue);

         // スライダーの値が変更された時の処理を登録
        BGMvolumeSlider.onValueChanged.AddListener(ChangeVolumeBGM);
        SEvolumeSlider.onValueChanged.AddListener(ChangeVolumeSE);


    }

    void Update()
    {
        // スライダーの値を取得
        Update_Volume.BGMsliderValue = BGMvolumeSlider.value;
        Update_Volume.SEsliderValue = SEvolumeSlider.value;
        // オーディオの音量を設定
        BGMaudioSource.GetComponent<AudioSource>().volume = Update_Volume.BGMsliderValue;
        SEaudioSource.GetComponent<AudioSource>().volume = Update_Volume.SEsliderValue;
        

        //エスケープキーが押されたときの処理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!Opened_Audio_Setting)
            {
                Open_Audio_Setting();
            }
            else
            {
                Close_Audio_Setting();
            }
        }
    }
    void ChangeVolumeBGM(float newVolume)
    {
        // スライダーの値によって音量を変更
        BGMaudioSource.GetComponent<AudioSource>().volume = newVolume;
    }
    void ChangeVolumeSE(float newVolume)
    {
        // スライダーの値によって音量を変更
        SEaudioSource.GetComponent<AudioSource>().volume = newVolume;
    }

    void PauseGame()
    {
        Time.timeScale = 0f; // ゲームの時間を停止
    }
    void ResumeGame()
    {
        Time.timeScale = 1f; // ゲームの時間を再開
    }

    //音量調節画面を開く
    public void Open_Audio_Setting(){
        if(!Opened_Audio_Setting){
            if (BGM_SE_Panel != null) BGM_SE_Panel.SetActive(true);
            
            Opened_Audio_Setting = true;
            PauseGame();
        }
    }

    //音量調節画面を閉じる
    public void Close_Audio_Setting(){
        if(Opened_Audio_Setting){
            if (BGM_SE_Panel != null) BGM_SE_Panel.SetActive(false);
            Opened_Audio_Setting = false;
            ResumeGame();
        }
    }

    //SEを鳴らす
    public void Play_SE(int seIndex){
        SEaudioSource.GetComponent<AudioSource>().clip = SEs[seIndex];
        SEaudioSource.GetComponent<AudioSource>().Play();
    }

    //BGMを鳴らす
    public void Play_BGM(int bgmIndex){
        BGMaudioSource.GetComponent<AudioSource>().clip = BGMs[bgmIndex];
        BGMaudioSource.GetComponent<AudioSource>().Play();
    }
}
