using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.MultiMedia
{
    /// <summary>
    /// カルーセル（ページめくり）の操作を受け持つメニューコントローラー。
    /// MenuManagerのサブメニューとして開き、左右キーでページ送りを処理します。
    /// </summary>
    public class CarouselMenuController : Selectable
    {
        [Header("Presenter Reference")]
        [SerializeField] private MultiMediaPresenter presenter;

        [Header("Page Indicator Settings")]
        [SerializeField] private GameObject horizontalLayoutGroupObj;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        private List<MediaPageData> _currentPages;
        private string _currentItemName;
        private int _currentPageIndex = 0;
        private List<GameObject> _generatedPageIndicators = new List<GameObject>();

        /// <summary>
        /// プレビュー表示（リストを上下移動している最中など）
        /// </summary>
        public void ShowPreview(string itemName, MediaPageData firstPage)
        {
            if (presenter != null)
            {
                presenter.DisplayPage(itemName, firstPage);
            }
        }

        public void ClearPreview()
        {
            if (presenter != null)
            {
                presenter.ClearAll();
            }
        }

        /// <summary>
        /// 決定ボタンを押してカルーセルモードに入った時の初期化
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

        public override void OnMove(AxisEventData eventData)
        {
            // 上下入力は無視するが、イベントは消費してフォーカスが外れないようにする
            if (eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down)
            {
                eventData.Use();
                return;
            }

            // 左右入力でページめくり
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

        private void ChangePage(int direction)
        {
            if (_currentPages == null || _currentPages.Count == 0) return;

            _currentPageIndex += direction;

            // ループ処理
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

        private void UpdatePresenter()
        {
            if (presenter != null && _currentPages != null && _currentPages.Count > 0)
            {
                int safeIndex = Mathf.Clamp(_currentPageIndex, 0, _currentPages.Count - 1);
                presenter.DisplayPage(_currentItemName, _currentPages[safeIndex]);
            }
            else if (presenter != null)
            {
                presenter.ClearAll();
            }
        }

        private void GeneratePageIndicators(int pageCount)
        {
            if (horizontalLayoutGroupObj == null || activeSprite == null || inactiveSprite == null) return;

            // 既存のインジケータをクリア
            foreach (var indicator in _generatedPageIndicators)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
            }
            _generatedPageIndicators.Clear();

            // 新しいインジケータを生成
            for (int i = 0; i < pageCount; i++)
            {
                GameObject indicator = new GameObject($"PageIndicator_{i}");
                indicator.transform.SetParent(horizontalLayoutGroupObj.transform, false);

                Image img = indicator.AddComponent<Image>();
                img.sprite = inactiveSprite;
                img.color = inactiveColor;

                _generatedPageIndicators.Add(indicator);
            }
        }

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

        private void PlayNavigationSound()
        {
            // if (Managers.SEManager.Instance != null)
            // {
            //     Managers.SEManager.Instance.PlaySE("Select");
            // }
        }
    }
}
