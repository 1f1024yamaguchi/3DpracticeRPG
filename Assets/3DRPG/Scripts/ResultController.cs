using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; //LoadSceneを使うために必要

public class ResultController : MonoBehaviour
{
    private bool canInput;
    [SerializeField] private TextMeshProUGUI resultTimeText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canInput =false; //最初は入力を受け付けない
        StartCoroutine(EnableInputAfterDelay(3f));
        float finalTime = TimerController.finalTime;

        float minutes = Mathf.FloorToInt(finalTime / 60);
        float seconds = Mathf.FloorToInt(finalTime % 60);
        float milliseconds = (finalTime % 1) * 100;
        resultTimeText.text = string.Format("Time: {0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
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
    
}
