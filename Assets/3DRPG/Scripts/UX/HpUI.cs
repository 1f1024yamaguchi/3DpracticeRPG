// using UnityEngine;
// using TMPro;

// public class HpUI : MonoBehaviour
// {
//     [Header("UIコンポーネントの参照")]
//     [SerializeField] private LevelSystem levelSystem; // レベルシステムへの参照
//     [SerializeField] private PlayerStatus playerStatus; // プレイヤーステータスへの参照 hp取得


//     [Header("UIテキストコンポーネントの参照")] 
//     [SerializeField] private TextMeshProUGUI hpText; // HP表示用のテキストコンポーネント
    
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // HPのテキストを更新
//         hpText.text = $"HP: {playerStatus.currentHP} / {playerStatus.maxLife}";
        
//     }
// }
