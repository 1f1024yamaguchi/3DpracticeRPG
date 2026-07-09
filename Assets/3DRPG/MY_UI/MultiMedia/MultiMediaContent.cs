using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.MultiMedia
{
    /// <summary>
    /// 1つの選択項目に対応する複数のページデータを保持するコンポーネント。
    /// Selectableを継承し、EventSystemによるナビゲーションとローカルでのページ送り操作を処理する。
    /// </summary>
    public class MultiMediaContent : Selectable, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField] private string itemName;
        [SerializeField] private List<MediaPageData> pages = new List<MediaPageData>();
        [SerializeField] private GameObject selectionCursor;
        [SerializeField] private TMPro.TextMeshProUGUI labelText; // メニューボタン自体のテキスト表示用

        [Header("Input Settings")]
        // EventSystemによるOnMoveを利用するため、独自のクールダウン処理は削除

        // Generatorで設定した値をPlayモードでも保持させるためにシリアライズする
        [SerializeField, HideInInspector] private MultiMediaPresenter _presenter;
        private int _currentPageIndex = 0;

        public string ItemName => itemName;
        public int PageCount => pages.Count;

        /// <summary>
        /// 外部（Generator等）からデータを流し込むための初期化メソッド
        /// </summary>
        public void Initialize(string name, List<MediaPageData> pageData, MultiMediaPresenter presenter)
        {
            this.itemName = name;
            this.pages = new List<MediaPageData>(pageData);
            this._presenter = presenter;

            // Inspectorでアサインし忘れていた場合のために、自動的に子のテキストコンポーネントを探す
            if (labelText == null)
            {
                labelText = GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            }

            if (labelText != null)
            {
                labelText.text = name;
            }

            // 初期状態は非選択
            if (selectionCursor != null)
            {
                selectionCursor.SetActive(false);
            }
        }

        public override void OnMove(AxisEventData eventData)
        {
            // 上下の移動は通常通り（別の項目へ）
            if (eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down)
            {
                base.OnMove(eventData);
                return;
            }

            // 左右の移動でページ送りを処理
            if (pages != null && pages.Count > 1)
            {
                if (eventData.moveDir == MoveDirection.Left)
                {
                    ChangePage(-1);
                    eventData.Use(); // 入力を消費
                }
                else if (eventData.moveDir == MoveDirection.Right)
                {
                    ChangePage(1);
                    eventData.Use(); // 入力を消費
                }
            }
        }

        private void ChangePage(int direction)
        {
            if (pages == null || pages.Count == 0) return;

            _currentPageIndex += direction;

            // ループ処理
            if (_currentPageIndex >= pages.Count)
            {
                _currentPageIndex = 0;
            }
            else if (_currentPageIndex < 0)
            {
                _currentPageIndex = pages.Count - 1;
            }

            PlayNavigationSound();
            UpdatePresenter();

            OnIndicatorUpdate?.Invoke(pages?.Count ?? 0, _currentPageIndex);
        }

        public System.Action<int, int> OnIndicatorUpdate;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (selectionCursor != null) selectionCursor.SetActive(true);
            _currentPageIndex = 0; // 項目が選ばれたら最初のページに戻す

            PlayNavigationSound();
            UpdatePresenter();

            OnIndicatorUpdate?.Invoke(pages?.Count ?? 0, _currentPageIndex);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (selectionCursor != null) selectionCursor.SetActive(false);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            //if (SEManager.Instance != null)
            {
                // SEManager.Instance.PlaySE("NameSet");
            }
        }

        private void UpdatePresenter()
        {
            if (_presenter != null && pages != null && pages.Count > 0)
            {
                int safeIndex = Mathf.Clamp(_currentPageIndex, 0, pages.Count - 1);
                _presenter.DisplayPage(itemName, pages[safeIndex]);
            }
            else if (_presenter != null)
            {
                _presenter.ClearAll();
            }
        }

        private void PlayNavigationSound()
        {
            // if (SEManager.Instance != null)
            // {
            //     SEManager.Instance.PlaySE("Select");
            // }
        }
    }
}
