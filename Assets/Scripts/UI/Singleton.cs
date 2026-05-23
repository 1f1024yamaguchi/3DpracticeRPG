using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object(); // スレッドセーフ用

    public static T Instance
    {
        get
        {
            // スレッドセーフなアクセスを保証
            lock (_lock)
            {
                // インスタンスがまだ存在しない場合
                if (_instance == null)
                {
                    // まずシーン内に既存のインスタンスがないか探す
                    _instance = FindFirstObjectByType<T>();

                    // シーンにも存在しない場合、Resourcesフォルダからプレハブを探して生成する
                    if (_instance == null)
                    {
                        // クラス名と同じ名前のプレハブを探す
                        var prefab = Resources.Load<T>(typeof(T).Name);
                        if (prefab != null)
                        {
                            _instance = Instantiate(prefab);
                        }
                        else
                        {
                            Debug.LogError(typeof(T).Name + " のインスタンスも、Resourcesフォルダ内のプレハブも見つかりません。");
                        }
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this as T)
        {
            Debug.LogWarning(typeof(T).Name + " のインスタンスが重複しています。後から生成された方を破棄します。");
            Destroy(this.gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(this.gameObject);
    }
}