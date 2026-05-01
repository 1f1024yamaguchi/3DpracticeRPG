using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Level_TipsManager : MonoBehaviour
{

    [SerializeField] private GameObject tipsPanel; // ヒントパネルのプレハブ
    [SerializeField] private LevelSystem levelSystem; // レベルシステムへの参照
    [SerializeField] private Button button ;    // UIマネージャーへの参照


    private bool _hasShownTips = false; // ヒントを表示したかどうかのフラグ

    
    private void Start()
    {
        
    }


    void OnEnable()
    {
        levelSystem.OnLevelUp += CheckLevelForTips_2; // レベルアップイベントにShowTipsメソッドを登録
    }

    void OnDisable()
    {
        levelSystem.OnLevelUp -= CheckLevelForTips_2; // レベルアップイベントからShowTipsメソッドを解除
    }

    private void CheckLevelForTips_2()
    {
        if (levelSystem.Level == 2 && !_hasShownTips) // レベルが2でまだヒントを表示していない場合
        {
            ShowLevel2Tips();
            
        }
    }

    void Update()
    {
        if(tipsPanel != null && tipsPanel.activeSelf)
        {
            if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject); // ボタンを選択状態にする
            }
        }
    }

    private void ShowLevel2Tips()
    {
        _hasShownTips = true; // ヒントを表示したフラグを立てる

        //新しいマネージャーにuiモードへの切り替えを依頼する
        if(TO_UI_OnlyManager.Instance != null)
        {
            TO_UI_OnlyManager.Instance.SetUIMode(true); // UIモードに切り替える
        }
       
        if(tipsPanel != null)
        {

            tipsPanel.SetActive(true); // ヒントパネルを表示
            StartCoroutine(FindButtonCoroutine());
            
            

            Debug.Log(  "tipsパネルの表示."); // デバッグログにヒント表示のメッセージを出力
        }
    }

    private IEnumerator FindButtonCoroutine()
    {
        yield return null ; // 1フレーム待つ

        if(EventSystem.current != null && button != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // 現在選択されているGameObjectをクリア
            EventSystem.current.SetSelectedGameObject(button.gameObject); // ボタンを選択状態にする

        }
        
    }




}
