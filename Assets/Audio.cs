using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    [Header("UI References")]
    public Slider BGMvolumeSlider;  
    public Slider SEvolumeSlider;

            // 音量管理システムが開かれているかどうか
    public bool Opened_Audio_Setting = false;

    [Header("Settings")]
    //Audioミキサーを入れるとこです
    [SerializeField] AudioMixer audioMixer;

    //それぞれのスライダーをいれるところ。多い場合は配列でもいい
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;

    [SerializeField] private GameObject BGM_SE_Panel;

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", -20);
        float savedSE = PlayerPrefs.GetFloat("SEVolume", -20);

        BGMSlider.value = savedBGM;
        SESlider.value = savedSE;

        audioMixer.SetFloat("BGM", savedBGM);
        
        audioMixer.SetFloat("SE", savedSE);

        // if(BGM_SE_Panel != null)
        // {
        //     BGM_SE_Panel.SetActive(false);
        // }


        //ミキサーのvolumeにスライダーのvloumeをいれる。

        //bgm
        // audioMixer.GetFloat("BGM", out float bgmVolume);
        // BGMSlider.value = bgmVolume;

        //se
        // audioMixer.GetFloat("SE", out float seVolume);
        // SESlider.value = seVolume;

        
    }



    // Update is called once per frame
    void Update()
    {

        // if(Input.GetKeyDown(KeyCode.Escape))
        // {
        //     if(!Opened_Audio_Setting)
        //     {
        //         Open_Audio_Setting();
        //     }
        //     else
        //     {
        //         Close_Audio_Setting();
        //     }


        // }

        
    }

    
        //             //音量調節画面を開く
        // public void Open_Audio_Setting()
        // {
        //     if(!Opened_Audio_Setting)
        //     {
        //         if (BGM_SE_Panel != null) BGM_SE_Panel.SetActive(true);
                
        //         Opened_Audio_Setting = true;
        //         PauseGame();
        //     }
        // }

        //     //音量調節画面を閉じる
        // public void Close_Audio_Setting()
        // {
        //     if(Opened_Audio_Setting)
        //     {
        //         if (BGM_SE_Panel != null) BGM_SE_Panel.SetActive(false);
        //         Opened_Audio_Setting = false;
        //         ResumeGame();
        //     }
        // }

        public void SetBGM(float volume)
        {
            audioMixer.SetFloat("BGM", volume);
            PlayerPrefs.SetFloat("BGMVolume", volume);
            PlayerPrefs.Save();
        }

        public void SetSE(float volume)
        {
            audioMixer.SetFloat("SE", volume);
            PlayerPrefs.SetFloat("SEVolume", volume);
            PlayerPrefs.Save();
        }   

        // private void PauseGame()
        // {
        //     Time.timeScale = 0f;// 時間の流れをゼロにする


        // }

        // private void ResumeGame()
        // {
        //     Time.timeScale = 1f;
        // }
}
