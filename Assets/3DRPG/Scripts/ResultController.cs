using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; //LoadSceneを使うために必要
using unityroom.Api;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    private bool canInput;
    [SerializeField] private TextMeshProUGUI resultTimeText;
    [SerializeField] private Image rankImage; //ランク画像表示
    [SerializeField] private Sprite ssSprite;
    [SerializeField] private Sprite sSprite;
    [SerializeField] private Sprite aplusSprite;
    [SerializeField] private Sprite aSprite;
    [SerializeField] private Sprite bSprite;
    [SerializeField] private Sprite cSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canInput =false; //最初は入力を受け付けない
        StartCoroutine(EnableInputAfterDelay(3f));
        float finalTime = GameManager.Instance.finalTime;

        float minutes = Mathf.FloorToInt(finalTime / 60);
        float seconds = Mathf.FloorToInt(finalTime % 60);
        float milliseconds = (finalTime % 1) * 100;
        resultTimeText.text = string.Format("Time: {0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);

        string rank = RankManager.GetRank(finalTime);
        //rankText.text = "Rank:" + rank;

        ShowRankImage(rank); //ランク画像を表示



        
    }
    

    // Update is called once per frame
    void Update()
    {
        if (canInput && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("Start");
        }
    }
    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); //指定した秒数待つ
        canInput = true;//入力を許可する
    }

    private void ShowRankImage(string rank)
    {
        switch (rank)
        {
            case "SS": rankImage.sprite = ssSprite; break;
            case "S": rankImage.sprite = sSprite; break;
            case "A+": rankImage.sprite = aplusSprite; break;
            case "A": rankImage.sprite = aSprite; break;
            case "B": rankImage.sprite = bSprite; break;
            case "C": rankImage.sprite = cSprite; break;
            default: rankImage.sprite = null; break;
        }
    }
    
}
