using UnityEngine;
using UnityEngine.SceneManagement;

public class change_scene_button : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Change_scene_button()
    {
        SceneManager.LoadScene("Main");
    }
}
