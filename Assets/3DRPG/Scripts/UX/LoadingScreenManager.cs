using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("ロード画面全体を囲うオブジェクト(CanvasやPanelなど)")]
    [SerializeField] private GameObject loadingScreenUI;
    [Tooltip("進捗表示用のスライダー")]
    [SerializeField] private Slider progressBar;
    [Tooltip("Tips画像を表示するImage")]
    [SerializeField] private Image tipsImage;

    [Header("Tips Data")]
    [Tooltip("表示したいTips画像のリスト")]
    [SerializeField] private List<Sprite> tipsList = new List<Sprite>();

    [Header("Settings")]
    [Tooltip("フェードイン・フェードアウトにかかる時間(秒)")]
    [SerializeField] private float fadeDuration = 0.5f;
    
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
                // CanvasGroupを取得、なければアタッチ
                canvasGroup = loadingScreenUI.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = loadingScreenUI.AddComponent<CanvasGroup>();
                }
                // 初期状態は非表示にする
                loadingScreenUI.SetActive(false);
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
    public void LoadScene(string sceneName)
    {
        if (loadingScreenUI == null)
        {
            Debug.LogError("LoadingScreenUIが設定されていません。通常のシーンロードを実行します。");
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        // 1. UIの表示準備
        loadingScreenUI.SetActive(true);
        canvasGroup.alpha = 0f;

        // Tips画像をランダムに設定
        if (tipsList.Count > 0 && tipsImage != null)
        {
            int randomIndex = Random.Range(0, tipsList.Count);
            tipsImage.sprite = tipsList[randomIndex];
        }

        // プログレスバーをリセット
        if (progressBar != null)
        {
            progressBar.value = 0f;
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
        while (!operation.isDone)
        {
            // Unityの仕様で、progressは0.9でロード完了を意味するため、0〜1に補正する
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // プログレスバー（スライダー）の更新
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            // progressが0.9以上（=実際のロードがほぼ完了）になったら
            if (operation.progress >= 0.9f)
            {
                // 少し待ってから（今回はすぐ）シーンをアクティブ化し切り替える
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
    }
}
