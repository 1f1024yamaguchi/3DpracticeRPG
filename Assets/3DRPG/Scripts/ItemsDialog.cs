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

    // Start is called before the first frame update
    void Start()
    {
        //初期状態は非表示
        gameObject.SetActive(false);

        //アイテム欄を必要な分だけ複製する
        for (var i = 0; i < buttonNumber ; i++)
        {
            Instantiate(itemButton, transform);
        }
        //子要素のItemButtonを一括取得、保持しておく
        _itemButtons = GetComponentsInChildren<ItemButton>();
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
            SelectFirstInteractableButton();
        }
    }

    public void Refresh()
    {
        RefreshItems();
        
        if(EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            SelectFirstInteractableButton();
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
