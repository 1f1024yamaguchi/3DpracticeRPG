using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class ItemsDialog : MonoBehaviour
{
    [SerializeField] private int buttonNumber = 15;
    [SerializeField] private ItemButton itemButton;

    private ItemButton[] _itemButtons;
    private bool _isInitialized = false;
    private int _currentIndex = 0;



    

    // Start is called before the first frame update
    private void Awake()
    {
        if (_isInitialized) return;

        // 既存の子要素（プレハブなど）があれば削除
        foreach (Transform child in transform) Destroy(child.gameObject);
        //初期状態は非表示
        //gameObject.SetActive(false);

        //配列を必要なサイズで初期化
        _itemButtons = new ItemButton[buttonNumber];
 
        //アイテム欄を必要な分だけ複製する
        for (var i = 0; i < buttonNumber ; i++)
        {
            //Instantiate(itemButton, transform);
            _itemButtons[i] = Instantiate(itemButton, transform);
        }
        //子要素のItemButtonを一括取得、保持しておく
        // _itemButtons = GetComponentsInChildren<ItemButton>();
        _isInitialized = true;
    }
    
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
        {
            // //表示された場合はアイテム欄をリフレッシュする
            // for (var i =0; i < buttonNumber; i++)
            // {
            //     //各アイテムボタンに所持アイテム情報をセット
            //     _itemButtons[i].OwnedItem  = OwnedItemsData.Instance.OwnedItems.Length > i
            //     ? OwnedItemsData.Instance.OwnedItems[i]
            //     : null;
            // }
            RefreshItems();
            StartCoroutine(SelectFirstButtonDelayed());
        }
    }

    private IEnumerator SelectFirstButtonDelayed()
    {
        yield return null; //1フレーム待つ
        
        SelectFirstInteractableButton();
    }

    public void Refresh()
    {
        RefreshItems();
        
        if(EventSystem.current.currentSelectedGameObject == null )
        {
            StartCoroutine(SelectFirstButtonDelayed());
        }
        // for (var i = 0; i < _itemButtons.Length; i++)
        // {
        //     _itemButtons[i].OwnedItem = OwnedItemsData.Instance.OwnedItems.Length > i ? OwnedItemsData.Instance.OwnedItems[i] : null;

        // }
    }

    private void RefreshItems()
    {
        for ( var i =0; i < buttonNumber; i++)
        {
            _itemButtons[i].OwnedItem = OwnedItemsData.Instance.OwnedItems.Length > i
            ? OwnedItemsData.Instance.OwnedItems[i] : null;
        }
    }

    private void SelectFirstInteractableButton()
    {
        var firstButton = _itemButtons.FirstOrDefault(b => b.GetComponent<UnityEngine.UI.Button>().interactable);
        if (firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }
}
