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
        // 進行度の永続化は AutoSaveManager に一元化した。
        // ここではセーブが無い場合(＝初めから)のための初期値のみ設定し、保存はしない。
        // セーブがある場合は、この後 AutoSaveManager.RestoreProgress() が上書きする。
        InitializeNewGame();
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

    // --- 保存/復元 ---
    // 進行度の永続化は AutoSaveManager に一元化した。
    // SaveData() は既存の呼び出し互換のため残すが、個別のPlayerPrefs書き込みは行わない
    // （AutoSaveManager が定期的に現在値をまとめて保存する）。
    public void SaveData()
    {
        // no-op: 永続化は AutoSaveManager 側で実施
    }

    /// <summary>現在の進行度を取得する（AutoSaveManagerの保存用）。</summary>
    public void GetProgress(out int level, out int exp, out int expToNext, out int skillPoints)
    {
        level = Level;
        exp = CurrentExp;
        expToNext = ExpToNextLevel;
        skillPoints = SkillPoints;
    }

    /// <summary>セーブデータから進行度を復元する（AutoSaveManager用）。</summary>
    public void RestoreProgress(int level, int exp, int expToNext, int skillPoints)
    {
        Level = Mathf.Max(1, level);
        CurrentExp = Mathf.Max(0, exp);
        ExpToNextLevel = Mathf.Max(1, expToNext);
        SkillPoints = Mathf.Max(0, skillPoints);
    }

    public void AddSkillPoints(int amount =1)
    {
        SkillPoints += amount;
        SaveData();
        Debug.Log($"スキルポイントが返却されました。現在: {SkillPoints}");
    }
}