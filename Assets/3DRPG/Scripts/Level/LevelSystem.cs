using UnityEngine;
using System;
using TMPro;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.Playables;

[Serializable]
public class LevelSystem : MonoBehaviour
{
    // 外部から数値を読み取るためのプロパティ
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExp = 0;

    [SerializeField] private GameObject levelUpPrefab; // レベルアップ時に表示するUIのプレハブ
    [SerializeField] private Transform uiParent; // レベルアップUIの親オブジェクト（Canvasなど）

    private PlayableDirector _director;


    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; } = 0;
    public int ExpToNextLevel { get; private set; } = 50;
    public int SkillPoints { get; private set; } = 0;



    //private TextMeshProUGUI _levelText; // レベル表示用のテキストコンポーネント

    // レベルアップ時にUIなどに通知するためのイベント
    public event Action OnLevelUp;

    private Coroutine _hideCoroutine;



    private void Awake()
    {
        
        InitializeNewGame();
        //LoadData(); // 起動時に保存データを読み込む
        SaveData(); //初期状態を保存
        Debug.Log($"初期化{Level},{CurrentExp},{ExpToNextLevel},{SkillPoints}" );
    }

    private void InitializeNewGame()
    {
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = 50;
        SkillPoints = 0;
    }



    public void AddExp(int amount)
    {
        CurrentExp += amount;
        while (CurrentExp >= ExpToNextLevel)
        {
            LevelUp();
        }
        SaveData(); // 経験値が入るたびに保存
    }

    private void LevelUp()
    {
        CurrentExp -= ExpToNextLevel;
        Level++;
        SkillPoints += 3; // 1レベルにつき3ポイント付与
        ExpToNextLevel = Mathf.RoundToInt(ExpToNextLevel * 1.3f); // 次の必要量を増やす



        
        OnLevelUp?.Invoke(); // レベルアップイベントを呼び出す

        if(levelUpPrefab != null )
        {
            //UIを生成

            GameObject go = Instantiate(levelUpPrefab, uiParent);

            go.SetActive(true);

            PlayableDirector prefabDirector = go.GetComponent<PlayableDirector>();

            

            prefabDirector.Play();

            var textComponent = go.GetComponentInChildren<TextMeshProUGUI>();

            Destroy(go, 2f); // 2秒後にUIを消す

            if(textComponent != null)
            {
                textComponent.text = "Level Up!";
            }
            

            
        }
        Debug.Log($"Level Up! Now Level: {Level}");


        
        
    }



    public bool ConsumeSkillPoint()
    {
        if (SkillPoints > 0)
        {
            SkillPoints--;
            SaveData();
            return true;
        }
        return false;
    }

    // --- 保存と読み込み (PlayerPrefsを使用) ---
    public void SaveData()
    {
        PlayerPrefs.SetInt("PlayerLevel", Level);
        PlayerPrefs.SetInt("PlayerExp", CurrentExp);
        PlayerPrefs.SetInt("NextExp", ExpToNextLevel);
        PlayerPrefs.SetInt("SkillPoints", SkillPoints);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        Level = PlayerPrefs.GetInt("PlayerLevel", 1);
        CurrentExp = PlayerPrefs.GetInt("PlayerExp", 0);
        ExpToNextLevel = PlayerPrefs.GetInt("NextExp", 100);
        SkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
    }

    public void AddSkillPoints(int amount =1)
    {
        SkillPoints += amount;
        SaveData();
        Debug.Log($"スキルポイントが返却されました。現在: {SkillPoints}");
    }
}