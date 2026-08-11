using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Managers;

namespace UI.MultiMedia
{
    [System.Serializable]
    public class MultiMediaEntryData
    {
        public string itemName = "New Item";
        public List<MediaPageData> pages = new List<MediaPageData>();
    }

    /// <summary>
    /// MultiMediaContentを自動生成するクラス。
    /// インスペクターで設定したリストに基づいて、子オブジェクトとして項目を生成する。
    /// 生成された項目間で上下のナビゲーションリンクを設定します。
    ///
    /// AutoMenuGenerator との違い:
    ///   ・こちらはギャラリー専用シーン向け（MenuManager を使わない）
    ///   ・Bボタンで前のシーンへ戻るシーン遷移処理を内蔵
    ///   ・ページインジケータ（●○）を Generator 側で一括生成する
    /// 使い方は Inspector の「Generate Menu」ボタンで Editor 生成 → シーン保存。
    /// </summary>
    public class MultiMediaGenerator : MonoBehaviour
    {
        [Header("Prefab & Container")]
        [SerializeField] private MultiMediaContent itemPrefab;
        [SerializeField] private Transform container;

        [Header("Presenter")]
        [SerializeField] private MultiMediaPresenter presenter;

        [Header("Menu Definition")]
        [SerializeField] private List<MultiMediaEntryData> items = new List<MultiMediaEntryData>();

        [Header("Page Indicator Settings")]
        [SerializeField] private GameObject horizontalLayoutGroupObj;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("Scene Transition Settings")]
        [SerializeField] private string cancelTargetScene = "Menu";

        [Header("Runtime References")]
        [SerializeField, HideInInspector] private List<MultiMediaContent> _generatedItems = new List<MultiMediaContent>();
        [SerializeField, HideInInspector] private List<GameObject> _generatedPageIndicators = new List<GameObject>();

        private void Awake()
        {
            // プレイ開始時にシリアライズされたアイテムのイベント再登録を行う
            if (_generatedItems != null)
            {
                foreach (var item in _generatedItems)
                {
                    if (item != null)
                    {
                        item.OnIndicatorUpdate -= UpdateIndicator;
                        item.OnIndicatorUpdate += UpdateIndicator;
                    }
                }

                if (_generatedItems.Count > 0 && _generatedItems[0] != null)
                {
                    UpdateIndicator(_generatedItems[0].PageCount, 0);
                }
            }
        }

        /// <summary>
        /// Bボタン（キャンセル）で前のシーンへフェード遷移します。
        /// 前のシーン名が記録されていなければ cancelTargetScene へ戻ります。
        /// </summary>
        private void Update()
        {
            if (InputManager.Instance != null && InputManager.Instance.ButtonBDown)
            {
                if (FadeManager.Instance != null)
                {
                    string prevScene = AppSessionManager.Instance.previousSceneName;
                    if (!string.IsNullOrEmpty(prevScene))
                    {
                        FadeManager.Instance.FadeToScene(prevScene);
                    }
                    else
                    {
                        FadeManager.Instance.FadeToScene(cancelTargetScene);
                    }
                }
            }
        }

        private void OnEnable()
        {
            // オブジェクトがアクティブになった際、自動的に最初の項目にフォーカスを合わせる
            StartCoroutine(FocusFirstItemCoroutine());
        }

        private System.Collections.IEnumerator FocusFirstItemCoroutine()
        {
            yield return null; // 他のUI初期化を待つために1フレーム待機

            if (_generatedItems != null && _generatedItems.Count > 0 && _generatedItems[0] != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_generatedItems[0].gameObject);
                }
            }
        }

        /// <summary>
        /// items の定義に従って項目を生成し、ナビゲーションリンクと
        /// ページインジケータ（最大ページ数分）を設定します。
        /// インスペクターの右クリックメニュー、またはボタン（Editor拡張が必要）から実行可能
        /// </summary>
        [ContextMenu("Generate Menu")]
        public void GenerateMenu()
        {
            ClearMenu();

            if (itemPrefab == null)
            {
                Debug.LogError("[MultiMediaGenerator] Item Prefab is not set.");
                return;
            }

            if (container == null) container = transform;

            int maxPageCount = 0;

            foreach (var data in items)
            {
                if (data.pages.Count > maxPageCount)
                {
                    maxPageCount = data.pages.Count;
                }

                MultiMediaContent newItem = Instantiate(itemPrefab, container);
                newItem.gameObject.name = $"Item_{data.itemName}";

                newItem.Initialize(data.itemName, data.pages, presenter);
                newItem.OnIndicatorUpdate = UpdateIndicator;

                _generatedItems.Add(newItem);
            }

            LinkNavigation();

            Debug.Log($"[MultiMediaGenerator] Successfully generated {items.Count} items. Max page count is {maxPageCount}.");

            GeneratePageIndicators(maxPageCount);
        }

        /// <summary>
        /// インジケータの表示を更新します。
        /// itemCount = 現在の項目のページ総数（それ以外のインジケータは非表示）、
        /// currentIndex = 現在のページ（アクティブ表示にする）。
        /// MultiMediaContent.OnIndicatorUpdate から呼ばれます。
        /// </summary>
        public void UpdateIndicator(int itemCount, int currentIndex)
        {
            if (_generatedPageIndicators == null) return;

            for (int i = 0; i < _generatedPageIndicators.Count; i++)
            {
                if (_generatedPageIndicators[i] != null)
                {
                    bool isVisible = i < itemCount;
                    _generatedPageIndicators[i].SetActive(isVisible);

                    if (isVisible)
                    {
                        Image img = _generatedPageIndicators[i].GetComponent<Image>();
                        if (img != null)
                        {
                            img.sprite = (i == currentIndex) ? activeSprite : inactiveSprite;
                            img.color = (i == currentIndex) ? activeColor : inactiveColor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 全項目中の最大ページ数分のインジケータを生成します。
        /// （項目ごとのページ数の差は UpdateIndicator の表示/非表示で吸収）
        /// </summary>
        private void GeneratePageIndicators(int maxPageCount)
        {
            if (horizontalLayoutGroupObj == null || activeSprite == null || inactiveSprite == null) return;

            for (int i = 0; i < maxPageCount; i++)
            {
                GameObject indicator = new GameObject($"PageIndicator_{i}");
                indicator.transform.SetParent(horizontalLayoutGroupObj.transform, false);

                Image img = indicator.AddComponent<Image>();
                img.sprite = inactiveSprite;
                img.color = inactiveColor;

                _generatedPageIndicators.Add(indicator);
            }
        }

        /// <summary>
        /// 生成された項目間で循環する上下ナビゲーションリンクを設定します。
        /// </summary>
        private void LinkNavigation()
        {
            int count = _generatedItems.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                var current = _generatedItems[i];
                var nav = current.navigation;
                nav.mode = Navigation.Mode.Explicit;
                // ループするように上下を接続
                nav.selectOnUp = _generatedItems[(i - 1 + count) % count];
                nav.selectOnDown = _generatedItems[(i + 1) % count];
                current.navigation = nav;
            }
        }

        /// <summary>
        /// 生成済みの項目とインジケータをすべて削除します。
        /// Playモード・Editorモード両対応。Editor時はコンテナ配下も掃除します。
        /// </summary>
        [ContextMenu("Clear Menu")]
        public void ClearMenu()
        {
            foreach (var item in _generatedItems)
            {
                if (item != null)
                {
                    if (Application.isPlaying)
                        Destroy(item.gameObject);
                    else
                        DestroyImmediate(item.gameObject);
                }
            }
            _generatedItems.Clear();

            foreach (var indicator in _generatedPageIndicators)
            {
                if (indicator != null)
                {
                    if (Application.isPlaying)
                        Destroy(indicator);
                    else
                        DestroyImmediate(indicator);
                }
            }
            _generatedPageIndicators.Clear();

            if (horizontalLayoutGroupObj != null && !Application.isPlaying)
            {
                for (int i = horizontalLayoutGroupObj.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(horizontalLayoutGroupObj.transform.GetChild(i).gameObject);
                }
            }

            // コンテナの直接の子要素も念のため掃除（手動で追加されたものなど）
            if (container != null && !Application.isPlaying)
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(container.GetChild(i).gameObject);
                }
            }
        }
    }
}
