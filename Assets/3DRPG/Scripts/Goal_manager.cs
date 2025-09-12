using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal_manager : MonoBehaviour
{
    public TimerController timerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {

            if (timerController != null)
            {
                timerController.Finish();
            }
            
            Debug.Log("ゴールしました");
            SceneManager.LoadScene("end"); 
        }
    }
}
