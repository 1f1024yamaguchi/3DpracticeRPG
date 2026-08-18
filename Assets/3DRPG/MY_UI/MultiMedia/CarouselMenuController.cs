using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.MultiMedia
{
    /// <summary>
    /// カルーセル（ページめくり）の操作を受け持つメニューコントローラー。
    /// MenuManagerのサブメニューとして開き、左右キーでページ送りを処理します。
    ///
    /// 動作の流れ:
    ///   1. GenericMenuItem(Carousel) を選択中 → ShowPreview() で1ページ目を表示
    ///   2. 決定を押す → Initialize() でページデータを受け取り閲覧モード開始
    ///   3. 左右キー → OnMove() 経由で ChangePage() が呼ばれページ送り
    ///   4. キャンセル → MenuManager.CloseMenu() で親メニューに戻る
    ///
    /// 表示自体は MultiMediaPresenter に委譲し、このクラスは
    /// 「ページ番号の管理」と「ページインジケータ（●○）の生成・更新」を担当します。
    /// </summary>
    public class CarouselMenuController : Selectable
    {
        [Header("Presenter Reference")]
        [SerializeField] private MultiMediaPresenter presenter; // 実際にテキスト/画像/動画を表示する担当

        [Header("Page Indicator Settings")]
        [SerializeField] private GameObject horizontalLayoutGroupObj; // インジケータを並べる親（HorizontalLayoutGroup付き）
        [SerializeField] private Sprite activeSprite;    // 現在ページのインジケータ画像
        [SerializeField] private Sprite inactiveSprite;  // 非現在ページのインジケータ画像
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("Page Indicator Size (自在に調整可能)")]
        [Tooltip("各ページインジケータ(●)のサイズ(px)。ここを変えれば現在ページ表示の大きさを自在に変えられます。")]
        [SerializeField] private Vector2 indicatorSize = new Vector2(24f, 24f);
        [Tooltip("インジケータ同士の間隔(px)。0以上ならHorizontalLayoutGroupのspacingをこの値で上書き。負値なら既存設定のまま。")]
        [SerializeField] private float indicatorSpacing = -1f;

        private List<MediaPageData> _currentPages;  // 現在閲覧中のページデータ一覧
        private string _currentItemName;            // 現在の項目名（タイトル表示用）
        private int _currentPageIndex = 0;          // 現在表示中のページ番号
        private List<GameObject> _generatedPageIndicators = new List<GameObject>(); // 生成したインジケータ

        /// <summary>
        /// プレビュー表示（リストを上下移動している最中など）。
        /// 決定前でも1ページ目の内容を Presenter に表示させます。
        /// </summary>
        public void ShowPreview(string itemName, MediaPageData firstPage)
        {
            if (presenter != null)
            {
                presenter.DisplayPage(itemName, firstPage);
            }
        }

        /// <summary>
        /// プレビュー表示をすべて消去します（選択が外れた時に呼ばれる）。
        /// </summary>
        public void ClearPreview()
        {
            if (presenter != null)
            {
                presenter.ClearAll();
            }
        }

        /// <summary>
        /// 決定ボタンを押してカルーセルモードに入った時の初期化。
        /// ページデータを受け取り、1ページ目を表示してインジケータを生成します。
        /// </summary>
        public void Initialize(string itemName, List<MediaPageData> pages)
        {
            _currentItemName = itemName;
            _currentPages = pages;
            _currentPageIndex = 0;

            GeneratePageIndicators(pages != null ? pages.Count : 0);
            UpdatePresenter();
            UpdateIndicator();
        }

        /// <summary>
        /// EventSystem からの移動入力。上下は無効化し、左右をページ送りに割り当てます。
        /// </summary>
        public override void OnMove(AxisEventData eventData)
        {
            // 上下入力は無視するが、イベントは消費してフォーカスが外れないようにする
            if (eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down)
            {
                eventData.Use();
                return;
            }

            // 左右入力でページめくり（ページが2枚以上ある時のみ）
            if (_currentPages != null && _currentPages.Count > 1)
            {
                if (eventData.moveDir == MoveDirection.Left)
                {
                    ChangePage(-1);
                    eventData.Use();
                }
                else if (eventData.moveDir == MoveDirection.Right)
                {
                    ChangePage(1);
                    eventData.Use();
                }
            }
        }

        /// <summary>
        /// ページ番号を direction(±1) だけ進め、端まで来たらループさせます。
        /// </summary>
        private void ChangePage(int direction)
        {
            if (_currentPages == null || _currentPages.Count == 0) return;

            _currentPageIndex += direction;

            // ループ処理（最後の次は最初へ、最初の前は最後へ）
            if (_currentPageIndex >= _currentPages.Count)
            {
                _currentPageIndex = 0;
            }
            else if (_currentPageIndex < 0)
            {
                _currentPageIndex = _currentPages.Count - 1;
            }

            PlayNavigationSound();
            UpdatePresenter();
            UpdateIndicator();
        }

        /// <summary>
        /// 現在のページ内容を Presenter に表示させます。ページがなければ全消去。
        /// </summary>
        private void UpdatePresenter()
        {
            if (presenter != null && _currentPages != null && _currentPages.Count > 0)
            {
                // 範囲外アクセスを防ぐため念のため Clamp する
                int safeIndex = Mathf.Clamp(_currentPageIndex, 0, _currentPages.Count - 1);
                presenter.DisplayPage(_currentItemName, _currentPages[safeIndex]);
            }
            else if (presenter != null)
            {
                presenter.ClearAll();
            }
        }

        /// <summary>
        /// ページ数分のインジケータ（●）を HorizontalLayoutGroup の子として生成します。
        /// 既存のものは一度破棄してから作り直します。
        /// </summary>
private void GeneratePageIndicators(int pageCount)
        {
            // 必要な参照が揃っていなければ何もしない（インジケータなしでも動作可能）
            if (horizontalLayoutGroupObj == null || activeSprite == null || inactiveSprite == null) return;

            // 間隔をコード側から上書き（indicatorSpacing が 0 以上のときのみ）
            if (indicatorSpacing >= 0f)
            {
                var hlg = horizontalLayoutGroupObj.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null) hlg.spacing = indicatorSpacing;
            }

            // 既存のインジケータをクリア
            foreach (var indicator in _generatedPageIndicators)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
            }
            _generatedPageIndicators.Clear();

            // 新しいインジケータを生成（サイズは indicatorSize で指定）
            for (int i = 0; i < pageCount; i++)
            {
                GameObject indicator = new GameObject($"PageIndicator_{i}", typeof(RectTransform));
                indicator.transform.SetParent(horizontalLayoutGroupObj.transform, false);

                Image img = indicator.AddComponent<Image>();
                img.sprite = inactiveSprite;
                img.color = inactiveColor;

                ApplyIndicatorSize(indicator);

                _generatedPageIndicators.Add(indicator);
            }
        }

        /// <summary>
        /// 現在ページのインジケータだけをアクティブ表示に切り替えます。
        /// </summary>
        private void UpdateIndicator()
        {
            if (_generatedPageIndicators == null) return;

            for (int i = 0; i < _generatedPageIndicators.Count; i++)
            {
                if (_generatedPageIndicators[i] != null)
                {
                    Image img = _generatedPageIndicators[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = (i == _currentPageIndex) ? activeSprite : inactiveSprite;
                        img.color = (i == _currentPageIndex) ? activeColor : inactiveColor;
                    }
                }
            }
        }

/// <summary>
        /// 実行中でもインジケータのサイズを変更したい場合に呼びます。
        /// indicatorSize を更新し、生成済みインジケータへ即座に反映します。
        /// </summary>
        public void SetIndicatorSize(Vector2 size)
        {
            indicatorSize = size;
            foreach (var indicator in _generatedPageIndicators)
            {
                if (indicator != null) ApplyIndicatorSize(indicator);
            }
        }

        /// <summary>
        /// 1つのインジケータへ現在の indicatorSize を適用します。
        /// RectTransform と LayoutElement の両方へ設定するため、
        /// HorizontalLayoutGroup の ChildControlSize 設定に関わらずサイズが反映されます。
        /// </summary>
        private void ApplyIndicatorSize(GameObject indicator)
        {
            if (indicator == null) return;

            var rt = indicator.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = indicatorSize;

            var le = indicator.GetComponent<LayoutElement>();
            if (le == null) le = indicator.AddComponent<LayoutElement>();
            le.preferredWidth = indicatorSize.x;
            le.preferredHeight = indicatorSize.y;
            le.minWidth = indicatorSize.x;
            le.minHeight = indicatorSize.y;
        }


        /// <summary>
        /// ページ送り時の効果音（SEManager 連携は現在コメントアウト中）。
        /// </summary>
        private void PlayNavigationSound()
        {
            // if (Managers.SEManager.Instance != null)
            // {
            //     Managers.SEManager.Instance.PlaySE("Select");
            // }
        }
    }
}
