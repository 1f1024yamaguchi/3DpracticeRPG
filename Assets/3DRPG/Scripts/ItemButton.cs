using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Button))]
public class ItemButton : MonoBehaviour
{
    public OwnedItemsData.OwnedItem OwnedItem
    {
        get { return _ownedItem; }
        set
        {
            _ownedItem = value;
            //アイテムが割り当てられたかどうかでアイテム画像や所持個数の表示を切り替える
            var isEmpty = null == _ownedItem;
            image.gameObject.SetActive(!isEmpty);
            number.gameObject.SetActive(!isEmpty);
            _button.interactable = !isEmpty;
            if (!isEmpty)
            {
                image.sprite = itemSprits.First(x => x.itemType == _ownedItem.Type).sprite;
                number.text = ""+ _ownedItem.Number;
            }
        }
    }

    [SerializeField] private ItemTypeSpriteMap[] itemSprits;
    [SerializeField] private Image image;
    [SerializeField] private Text number;

    private Button _button;
    private OwnedItemsData.OwnedItem _ownedItem;
    private PlayerEffectManager _playerEffectManager;
    private ItemsDialog _itemsDialog;

    private InputSystem_Actions _controls;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);

        //プレイヤーとUI管理クラスを見つけて変数に保存
        _playerEffectManager = FindObjectOfType<PlayerEffectManager>();
        _itemsDialog = GetComponentInParent<ItemsDialog>();

        _controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _controls.UI.UseItem.Enable();
        _controls.UI.UseItem.performed += OnUseItem; 
    }

    private void OnDisable()
    {
        _controls.UI.UseItem.performed -= OnUseItem;
        _controls.UI.UseItem.Disable();
    }

    private void OnUseItem(InputAction.CallbackContext context)
    {
        if(_button.interactable)
        {
            OnClick();
        }

    }

  
    private void OnClick()
    {
        //アイテムが0だったら使えないようにする
        if (_ownedItem == null || _ownedItem.Number <=0)
        {
            Debug.Log("アイテムなし");
            return;

        }
        Debug.Log(_ownedItem.Type +"ボタンがクリックされました！");

        //プレイヤーに効果を適用
        if(_playerEffectManager != null)
        {
            _playerEffectManager.ApplyItemEffect(_ownedItem.Type);
        }

        //アイテムデータを1つ消費
        OwnedItemsData.Instance.Use(_ownedItem.Type,1);
        OwnedItemsData.Instance.Save(); 

        //アイテムダイアログの表示更新
        if (_itemsDialog != null)
        {
            _itemsDialog.Refresh();
        }



    }

    //アイテムの種類とSpriteをインスペクタで紐づけされるようにするためのクラス
    [Serializable]
    public class ItemTypeSpriteMap
    {
        public Item.ItemType itemType;
        public Sprite sprite;
    }

}
