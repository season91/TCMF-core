using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIWcItemInvenQuick : MonoBehaviour
{
    private enum EquipButtonState
    {
        None,
        Equip,
        Unequip
    }
    
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private RectTransform rectTr;
    private const float StartPosX = 350;
    private const float FinalPosX = -30f;
    private Sequence popupSq;
    
    private UIDynamicObjectPool<UIWgItem> dynamicItemPool;
    [SerializeField] private Transform guiItemRoot;
    [SerializeField] private UIWgItem originItem;
    
    [SerializeField] private GameObject emptyInform;
    
    [SerializeField] private UIWcItemInfoEquip uiInfoEquipped;
    [SerializeField] private UIWcItemInfoEquip uiInfoSelected;
    
    [SerializeField] private Button btnToggleEquip;
    [SerializeField] private TextMeshProUGUI tmpToggleEquip;
    
    private ItemType curItemType;
    private int curUnitIndex;
    private Action equipAction;
    
    private const int InvalidIndex = -1;
    private int equippedItemIndex;
    private int selectedItemIndex;
    
    private readonly Vector2 itemInfoMovePos = new (-1675, 0);
    private readonly Vector2 itemInfoOriginPos = new (-905, 0);

    bool IsSelected => selectedItemIndex != InvalidIndex;
    private IReadOnlyList<InventoryItem> inventoryItems;
    private IReadOnlyList<UIWgItem> itemList;
    
    protected void Reset()
    {
        cg = GetComponent<CanvasGroup>();
        rectTr = GetComponent<RectTransform>();
        
        guiItemRoot = transform.FindChildByName<Transform>("Layout_Items");
        originItem = guiItemRoot.GetComponentInChildren<UIWgItem>();

        emptyInform = transform.FindChildByName<Transform>("Tmp_EmptyInform").gameObject;

        uiInfoEquipped = transform.FindChildByName<UIWcItemInfoEquip>("Group_EquipItemInfoBox");
        uiInfoSelected = transform.FindChildByName<UIWcItemInfoEquip>("Group_SelectedInfoBox");
        
        btnToggleEquip = transform.FindChildByName<Button>("Btn_ToggleEquip");
        tmpToggleEquip = btnToggleEquip.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize()
    {
        dynamicItemPool = new UIDynamicObjectPool<UIWgItem>(originItem, guiItemRoot, 105);
        
        uiInfoEquipped.Initialize();
        uiInfoSelected.Initialize();
    }

    public void Open(ItemType itemType, int unitIndex, Action callback = null)
    {
        if(popupSq != null)
            popupSq.Kill(true);
        popupSq = cg.OpenPopupAnimation(rectTr, RectTransform.Axis.Horizontal, StartPosX, FinalPosX);
        cg.SetInteractable(true);
        InitAllPopup();
        
        equipAction = callback;
        curItemType = itemType;
        curUnitIndex = unitIndex;
        
        UpdateGUI();
        
        selectedItemIndex = InvalidIndex;
        equippedItemIndex = InvalidIndex;
        inventoryItems = InventoryManager.Instance.Cache.InventoryDict[curItemType];
        InventoryItem item = InventoryManager.Instance.EquipmentInteractor.GetEquippedItemByType(curItemType, curUnitIndex);
        if (item != null)
        {
            equippedItemIndex = InventoryManager.Instance.Cache.GetItemIndexByItemUid(curItemType, item.ItemUid);
            EquipAfterUI();
        }
    }
    
    private void UpdateGUI()
    {
        if(!InventoryManager.Instance.Cache.InventoryDict.TryGetValue(curItemType, out var inventoryItemList) // Dict에 key가 없거나
           || inventoryItemList == null     // null일 때나
           || inventoryItemList.Count <= 0) // 0 이하일 때
        {
            emptyInform.SetActive(true); // 비었다는 안내 텍스트 표출
            guiItemRoot.gameObject.SetActive(false); // 인벤토리 레이아웃 inactive
            return;
        }
        
        emptyInform.SetActive(false);
        guiItemRoot.gameObject.SetActive(true);
        
        int itemCount = inventoryItemList.Count;
        
        // 인벤토리 표시 3의 배수로만 (초기 9개만 고정) => 나머지 Hide
        // 3의 배수로 맞춘 슬롯 수 (보일 총 슬롯 수)
        // Ceil To int => int형, 올림
        int visibleSlotCount = Mathf.Max(9, Mathf.CeilToInt(itemCount / 3f) * 3);
        itemList = dynamicItemPool.GetActiveList();
        int inventoryCount = Mathf.Max(itemList.Count, visibleSlotCount);
        for (int i = 0; i < inventoryCount; i++)
        {
            UIWgItem item = i < itemList.Count ? itemList[i] : dynamicItemPool.Get();

            if (i < itemCount)
            {
                item.Initialize();
                item.SetClickEvent(OnItemClick);
                item.ShowInventory(inventoryItemList[i], i);
            }
            else if(i < visibleSlotCount)
            {
                item.ShowInventory(null);
            }
            else
            {
                dynamicItemPool.Return(item);
            }
        }
    }
    
    private void OnItemClick(int selectItemIndex)
    {
        if (inventoryItems.Count <= selectItemIndex)
        {
            uiInfoEquipped.Close();
            UpdateGUI();
            return;
        }
        
        UpdateGUI();
        
        bool isSameAsEquipped = equippedItemIndex == selectItemIndex;
        bool isSameAsSelected = selectedItemIndex == selectItemIndex;
        
        // 장착된 아이템이 있다는 것은 별도의 bool이므로 판단하지 않음.
        // 그리고 각 메서드에서 열려있는지 판단하고 실행
        // 현재 선택된 것이 존재하는가?
        if (!IsSelected)
        {
            // 장착하고 있는 아이템 == 현재 선택한 아이템 
            if (isSameAsEquipped)
            {
                ToggleEquipUI();
                return;
            }
            // 현재 선택한 아이템 띄우기
            ShowSelectedUI(selectItemIndex);
        }
        else
        {
            // 이전에 선택한 item과 일치
            if (isSameAsSelected)
            {
                HideSelectedUI(); // 선택 취소
            }
            else
            {
                if (isSameAsEquipped)
                {
                    HideSelectedUI(); // 선택 취소
                }
                else
                    ReplaceSelectedUI(selectItemIndex); // 현재 선택한 아이템으로 변경
            }
        }
    }
    
    private void SetToggleEquipButton(EquipButtonState state)
    {
        string buttonText;
        UnityAction action = null;
        bool interactable = true;

        switch (state)
        {
            case EquipButtonState.None:
                buttonText = "장착";
                interactable = false;
                break;
            case EquipButtonState.Equip:
                buttonText = "장착";
                action = OnEquip;
                break;
            case EquipButtonState.Unequip:
                buttonText = "해제";
                action = OnUnequip;
                break;
            default:
                MyDebug.LogError($"EquipButtonState Enum => Not Exist {state}");
                return;
        }

        tmpToggleEquip.SetText(buttonText);
        btnToggleEquip.interactable = interactable;
        btnToggleEquip.onClick.RemoveAllListeners();
        if (action != null) btnToggleEquip.AddListener(action);
    }

    #region Info 팝업 관련
    
    /// <summary>
    /// 모든 Popup의 
    /// </summary>
    private void InitAllPopup()
    {
        uiInfoEquipped.cg.SetAlpha(0);
        uiInfoSelected.cg.SetAlpha(0);
        uiInfoEquipped.rectTr.anchoredPosition = itemInfoOriginPos;
    }

    private void OpenEquipUI()
    {
        InventoryItem item = inventoryItems[equippedItemIndex];
        uiInfoEquipped.Open(item);
    }
    
    private void ToggleEquipUI()
    {
        // 띄워져있지 않거나, 이동해야할 위치가 같을 때 Return
        if (uiInfoEquipped.cg.alpha <= 0.5f)
        {
            OpenEquipUI();
        }
        else
        {
            uiInfoEquipped.Close();
        }
    }

    /// <summary>
    /// Selected 팝업 띄우는 메서드,
    /// 아이템 선택 시 호출
    /// </summary>
    /// <param name="selectItemIndex">선택한 아이템 인덱스</param>
    private void ShowSelectedUI(int selectItemIndex)
    {
        InventoryItem item = inventoryItems[selectItemIndex];

        MoveEquippedUI(true); // Equip 팝업 옮겨주고
        uiInfoSelected.Open(item); // Selected 팝업 열어주고
        selectedItemIndex = selectItemIndex; // 선택된 아이템 인덱스 변수에 할당
        SetToggleEquipButton(EquipButtonState.Equip);
    }
    
    /// <summary>
    /// Select Popup 숨기는 메서드,
    /// 선택된 아이템을 다시 선택했을 시 취소 목적 호출
    /// </summary>
    private void HideSelectedUI()
    {
        uiInfoSelected.Close(); // Selected 팝업 닫아주고
        MoveEquippedUI(false); // Equip 팝업 원위치로
        
        selectedItemIndex = InvalidIndex; // 변수 초기화
        if (equippedItemIndex == InvalidIndex) // equipItemIndex가 
        {
            SetToggleEquipButton(EquipButtonState.None);
        }
        else
        {
            itemList[equippedItemIndex].SetSelectedObj(true);
            // selectedItemIndex = equippedItemIndex;
            SetToggleEquipButton(EquipButtonState.Unequip);
        }
    }
    
    /// <summary>
    /// Selected 팝업 교체 메서드,
    /// 선택된 아이템이 있으면 호출
    /// </summary>
    /// <param name="selectItemIndex">선택한 아이템 인덱스</param>
    private void ReplaceSelectedUI(int selectItemIndex)
    {
        InventoryItem item = inventoryItems[selectItemIndex];
        
        uiInfoSelected.Replace(item); // Select 팝업교체 해주고
        selectedItemIndex = selectItemIndex; // 선택된 아이템 인덱스 변수에 할당
        SetToggleEquipButton(EquipButtonState.Equip);
    }
    
    /// <summary>
    /// Equip Popup 움직이는 메서드,
    /// 현재 띄워져있거나 이동할 위치가 같을 경우 호출 무시
    /// </summary>
    /// <param name="isReplaceCondition">교체하는 상태일 경우</param>
    private void MoveEquippedUI(bool isReplaceCondition)
    {
        Vector2 targetPos = isReplaceCondition ? itemInfoMovePos : itemInfoOriginPos;
        var currentPos = uiInfoEquipped.rectTr.anchoredPosition;
        
        // 띄워져있지 않거나, 이동해야할 위치가 같을 때 Return
        if (equippedItemIndex == -1 || (currentPos - targetPos).sqrMagnitude < 0.0001f)
            return;

        if(uiInfoEquipped.cg.alpha <= 0.5f)
            OpenEquipUI();
        // 교체하려는 상태면 좌측으로 이동 시킴
        // 아니라면 원위치
        uiInfoEquipped.rectTr.DOAnchorPos(targetPos, 0.3f);
    }

    /// <summary>
    /// 장착이 끝난 후 UI 업데이트 메서드
    /// </summary>
    private void EquipAfterUI()
    {
        // 장착된 아이템 변수 세팅
        equippedItemIndex = selectedItemIndex != InvalidIndex ? selectedItemIndex : equippedItemIndex;
        selectedItemIndex = InvalidIndex;
        // 장착된 아이템 기준으로 아이템 세팅
        InventoryItem item = inventoryItems[equippedItemIndex];
        uiInfoEquipped.Open(item); // Equip 팝업 열어주고
        HideSelectedUI();
    }

    /// <summary>
    /// 장착 해제가 끝난 후 UI 업데이트 메서드
    /// </summary>
    private void UnequipAfterUI()
    {
        // 아이템 변수 초기화
        selectedItemIndex = InvalidIndex;
        equippedItemIndex = InvalidIndex; 
        // GUI 초기화
        uiInfoEquipped.Close(); // Equip 팝업 닫아주고
        SetToggleEquipButton(EquipButtonState.None); // 버튼 입력 없애주고
    }

    #endregion
    
    public void Close(bool isAnimation = true)
    {
        if (isAnimation)
        {
            cg.ClosePopupAnimation(rectTr, RectTransform.Axis.Horizontal, FinalPosX, StartPosX);
            cg.SetInteractable(false);
        }
        else
            cg.SetAlpha(0);
    }
    
    private async void OnEquip()
    {
        try
        {
            InventoryUnit inventoryUnit = UserData.inventory.Units[curUnitIndex];
            await InventoryManager.Instance.EquipmentInteractor.TryEquip(selectedItemIndex, inventoryUnit, curItemType, OnEquipCallback);
        }
        catch (Exception ex)
        {
            MyDebug.LogError($"OnEquip() 예외 발생: {ex}");
        }
    }
    
    private async void OnUnequip()
    {
        try
        {
            await InventoryManager.Instance.EquipmentInteractor.TryUnEquipAsync(equippedItemIndex, curItemType);
            OnUnequipCallback();
        }
        catch (Exception ex)
        {
            MyDebug.LogError($"OnUnequip() 예외 발생: {ex}");
        }
    }
    
    /// <summary>
    /// 장착/해제 시, 호출 메서드
    /// Item Inventory GUI 업데이트, 장착/해제 후 콜백 액션 실행, 장착/해제 팝업 Inactive
    /// </summary>
    private void OnEquipCallback()
    {
        UpdateGUI();
        equipAction?.Invoke();
        EquipAfterUI();
    }

    private void OnUnequipCallback()
    {
        UpdateGUI();
        equipAction?.Invoke();
        UnequipAfterUI();
    }
}
