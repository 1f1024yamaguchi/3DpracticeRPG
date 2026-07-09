using UnityEngine;
using TMPro;

public class Status_UI_for_character : MonoBehaviour
{

    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private Re_PlayerController re_playerController;

    [Header("status 数値化")]

    [SerializeField] private TextMeshProUGUI powerValueText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI jumpValueText;
    [SerializeField] private TextMeshProUGUI hpValueText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerStatus == null || re_playerController == null)
        {
            Debug.LogWarning("PlayerStatus または PlayerController がアサインされていません！");
            return;
        }

        powerValueText.text = playerStatus.AttackPower.ToString();
        speedValueText.text = re_playerController.moveSpeed.ToString("F1"); //小数点以下1桁まで表示
        jumpValueText.text = re_playerController.jumpPower.ToString("F1"); //小数点以下1桁まで表示
        hpValueText.text = playerStatus.lifeMax.ToString();     //HPの最大値を表示
        
    }
}
