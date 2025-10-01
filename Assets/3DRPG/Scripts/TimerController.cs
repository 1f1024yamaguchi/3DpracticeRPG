using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using unityroom.Api;

public class TimerController : MonoBehaviour
{
    private float timer = 0.0f;
    private bool isRunning = true;

    [SerializeField] private TextMeshProUGUI timerText;

    public static float finalTime;


    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            DisplayTime(timer);
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay <0)
        {
            timeToDisplay =0;
        }

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliseconds = (timeToDisplay % 1) * 100;

        // 指定したフォーマットでテキストに表示
        timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public void Finish()
    {
        Debug.Log("Finishメソッドが呼ばれました。現在のタイムは: " + timer);
        isRunning = false; //タイマーを止める
        GameManager.Instance.finalTime = timer; //GameManagerに最終タイムを保存
        Debug.Log("GameManagerにタイムを保存しました: " + GameManager.Instance.finalTime);
        //float scoreInMilliseconds = timer * 1000f;

        //finalTimeの値を送信する
        UnityroomApiClient.Instance.SendScore(1, timer , ScoreboardWriteMode.HighScoreAsc);

    }

}
