// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class OpenSE_BGMpanel : MonoBehaviour
// {
//     public Slider BGMvolumeSlider;  
//     public Slider SEvolumeSlider;

//         // 音量管理システムが開かれているかどうか
//     public bool Opened_Audio_Setting = false;

//     [SerializeField] private GameObject BGM_SE_Panel;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         if(BGM_SE_Panel != null)
//         {
//             BGM_SE_Panel.SetActive(false);
//         }
        
//     }

//     // Update is called once per frame
//     void Update()
//     {


        
//     }

//                 //音量調節画面を開く
//     public void Open_Audio_Setting()
//     {
//         if(!Opened_Audio_Setting)
//         {
//             if (BGM_SE_Panel != null) BGM_SE_Panel.SetActive(true);
                
//                 Opened_Audio_Setting = true;
//                 PauseGame();
//         }
//     }
// }
