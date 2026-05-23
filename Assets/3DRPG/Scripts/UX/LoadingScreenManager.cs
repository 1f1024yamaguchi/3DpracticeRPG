using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("ロード画面全体を囲うオブジェクト(CanvasやPanelなど)")]
    [SerializeField] private GameObject loadingScreenUI;
    [Tooltip("進捗表示用のテキスト(右下に配置してください)")]
    [SerializeField] private TextMeshProUGUI progressText;
    [Tooltip("Tips画像を表示するImage")]
    [SerializeField] private Image tipsImage;

    [Header("Tips Data")]
    [Tooltip("表示したいTips画像のリスト")]
    [SerializeField] private List<Sprite> tipsList = new List<Sprite>();

    [Header("Settings")]
    [Tooltip("フェードイン・フェードアウトにかかる時間(秒)")]
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("最低限ローディング画面を表示する時間(秒)。Tipsを読ませるために長めに設定できます。")]
    [SerializeField] private float minLoadingTime = 2.0f;
    
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // シングルトンパターンの確実な実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時もUIを維持する
            
            if (loadingScreenUI != null)
            {
                // シーン遷移時にUIオブジェクトが破棄されるのを防ぐため、このマネージャーの子オブジェクトにする
                loadingScreenUI.transform.SetParent(transform);

                // CanvasGroupを取得、なければアタッチ
                canvasGroup = loadingScreenUI.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = loadingScreenUI.AddComponent<CanvasGroup>();
                }
                // 初期状態は非表示にする
                loadingScreenUI.SetActive(false);
                if (progressText != null)
                {
                    progressText.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("LoadingScreenManager: loadingScreenUI が割り当てられていません！");
            }
        }
        else
        {
            // 既にInstanceが存在する場合は重複オブジェクトを破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定されたシーン名のシーンを非同期でロードする
    /// </summary>
    /// <param name="sceneName">読み込むシーンの名前</param>
    /// <param name="showTips">Tipsや進捗を表示するかどうか（falseで真っ暗な画面になり、待機もスキップされます）</param>
    public void LoadScene(string sceneName, bool showTips = true)
    {
        Debug.Log(sceneName + " へのロードを開始します！"); // これを追加
        if (loadingScreenUI == null)
        {
            Debug.LogError("LoadingScreenUIが設定されていません。通常のシーンロードを実行します。");
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(LoadSceneAsyncCoroutine(sceneName, showTips));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName, bool showTips)
    {
        // 1. UIの表示準備
        loadingScreenUI.SetActive(true);
        canvasGroup.alpha = 0f;

        if (showTips)
        {
            // Tips画像をランダムに設定
            if (tipsList.Count > 0 && tipsImage != null)
            {
                int randomIndex = Random.Range(0, tipsList.Count);
                tipsImage.sprite = tipsList[randomIndex];
                tipsImage.gameObject.SetActive(true); // 念のためアクティブにする
            }

            // 進捗テキストをリセット
            if (progressText != null)
            {
                progressText.text = "0%";
                progressText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Tipsと進捗テキストを非表示にする
            if (tipsImage != null) tipsImage.gameObject.SetActive(false);
            if (progressText != null) progressText.gameObject.SetActive(false);
        }

        // 2. 画面をフェードイン
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 3. 次のシーンの非同期ロード開始
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        // 読み込みが完了してもすぐにはシーン遷移させない
        operation.allowSceneActivation = false; 

        // 読み込み中の処理
        float loadTimer = 0f;
        float currentMinLoadTime = showTips ? minLoadingTime : 0f; // Tips非表示時は待機時間を0にする

        while (!operation.isDone)
        {
            loadTimer += Time.deltaTime;

            // Unityの仕様で、progressは0.9でロード完了を意味するため、0〜1に補正する
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (showTips && progressText != null)
            {
                // 進捗テキストの更新
                progressText.text = Mathf.FloorToInt(progress * 100f).ToString() + "%";
            }

            // progressが0.9以上（=実際のロードがほぼ完了）かつ、最低表示時間が経過したら遷移
            if (operation.progress >= 0.9f && loadTimer >= currentMinLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 4. ロード完了後のフェードアウト
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        // ロード画面を非表示に戻す
        loadingScreenUI.SetActive(false);
        canvasGroup.alpha = 0f;
        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }

        
    }

    public void LoadSceneFromEditor(string sceneName)
    {
        //本体のロードシーンを呼び出す
        LoadScene(sceneName, true);
    }
}
