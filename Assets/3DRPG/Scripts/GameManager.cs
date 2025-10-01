using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public float finalTime;
    
    private void Awake()
    {
        //GameManagerがまだ存在しない場合
        if (Instance == null)
        {
            //このインスタンスを唯一のものとして設定
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        //すでにgamemanagerが存在する場合
        else
        {
            Destroy(gameObject);
        }
    }
}
