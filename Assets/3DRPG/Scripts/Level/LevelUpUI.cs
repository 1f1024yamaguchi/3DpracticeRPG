using UnityEngine;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private LevelSystem levelSystem; // レベルシステムへの参照
    [SerializeField] private TextMeshProUGUI levelText; // レベルアップのテキスト表示用
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI pointText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // レベルアップのUIを更新
        levelText.text = $"Level: {levelSystem.Level}";
        expText.text = $"EXP: {levelSystem.CurrentExp} / {levelSystem.ExpToNextLevel}";
        pointText.text = $"Skill Points: {levelSystem.SkillPoints}";
        
    }
}
