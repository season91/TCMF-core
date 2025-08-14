using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item Inventory Box Widget Container 부모 클래스
/// </summary>
public abstract class UIBaseWcItemBox : MonoBehaviour
{
    [Header("====[카테고리 타입 버튼 POOL]")]
    private UIDynamicObjectPool<UIWgSelectBtn> dynamicItemTypePool;
    [SerializeField] private Transform itemTypeRoot;
    [SerializeField] private UIWgSelectBtn originItemType;
    
    private readonly Dictionary<ItemType, UIWgSelectBtn> typeDict = new ();
    protected ItemType ItemType;
    
    [Header("====[아이템 인벤토리 POOL]")]
    protected UIDynamicObjectPool<UIWgItem> DynamicItemPool;
    [SerializeField] private Transform itemRoot;
    [SerializeField] private ScrollRect scrollInventory;
    [SerializeField] private UIWgItem originItem;

    private Action<InventoryItem> itemClickEventByInven;
    private Action<ItemData> itemClickEventByData;
    private Action typeClickEvent;
    private bool isDuplicated;

    private readonly UIWgSelectBtnState selectedState = new() { IsSelectedObj = true };
    private readonly UIWgSelectBtnState unselectedState = new() { IsSelectedObj = false } ;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        itemTypeRoot = transform.FindChildByName<Transform>("Group_TypeButtons");
        originItemType = itemTypeRoot.FindChildByName<UIWgSelectBtn>("Btn_ItemType");
        
        itemRoot = transform.FindChildByName<Transform>("Layout_Items");
        scrollInventory = transform.FindChildByName<ScrollRect>("Scroll View_Items");
        originItem = itemRoot.GetComponentInChildren<UIWgItem>();
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize()
    {
        InitTypeBtns();
        InitItemList();
    }

    #region 아이템 타입 버튼

    /// <summary>
    /// 아이템 타입 버튼 GUI 및 이벤트 설정
    /// </summary>
    private void InitTypeBtns()
    {
        // 버튼 초기화
        dynamicItemTypePool = new UIDynamicObjectPool<UIWgSelectBtn>(originItemType, itemTypeRoot, 2);
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            if(itemType == ItemType.None) continue;
            
            var btnItemType = dynamicItemTypePool.Get();
            // 버튼 이벤트 바인딩
            switch (itemType)
            {
                case ItemType.Weapon:
                    btnItemType.SetEvent(OnWeaponType);
                    break;
                case ItemType.Armor:
                    btnItemType.SetEvent(OnArmorType);
                    break;
            }
            // 버튼 텍스트 설정
            btnItemType.SetText(itemType.ToKor());
            // 버튼 선택/취소 사이즈 변경
            btnItemType.SetVisualStates(selectedState, unselectedState);
            btnItemType.Unselect(); // 기본: 선택되지 않음
            typeDict[itemType] = btnItemType; // 캐싱 
        }
    }

    /// <summary>
    /// Item Box UI 표시: 클릭 이벤트 등록, 선택 가능 여부 설정, 타입 초기화
    /// </summary>
    /// <param name="duplicated">아이템 중복 선택 가능 여부</param>
    /// <typeparam name="T">ItemInventory/ItemData</typeparam>
    public void ShowItemBox<T>(Action<T> onItemClick, bool duplicated, Action onTypeClick = null)
    {
        isDuplicated = duplicated;
        typeClickEvent = onTypeClick;

        if (typeof(T) == typeof(InventoryItem))
            itemClickEventByInven = onItemClick as Action<InventoryItem>;
        else if (typeof(T) == typeof(ItemData))
            itemClickEventByData = onItemClick as Action<ItemData>;

        gameObject.SetActive(true);
        SetType(ItemType == ItemType.None ? ItemType.Weapon : ItemType);
    }
    
    /// <summary>
    /// Item Box UI 숨김
    /// </summary>
    public void Hide() => gameObject.SetActive(false);
    
    /// <summary>
    /// 타입 버튼 선택 시 호출
    /// 선택중이던 버튼 Unselected 메서드 호출 (사이즈 초기화 목적)
    /// 현재 선택된 Type 저장
    /// </summary>
    private void SetType(ItemType itemType)
    {
        if(ItemType != ItemType.None 
           && typeDict.TryGetValue(ItemType, out var prevCategory))
            prevCategory.Unselect();
        ItemType = itemType;
        typeDict[ItemType].Select();
        
        scrollInventory.verticalNormalizedPosition = 1f;
        typeClickEvent?.Invoke();
        UpdateGUI();
    }

    /// <summary>
    /// 무기 타입 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnWeaponType() => SetType(ItemType.Weapon);
    /// <summary>
    /// 방어구 타입 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnArmorType() => SetType(ItemType.Armor);

    #endregion

    #region 아이템 리스트

    /// <summary>
    /// Item List 초기화
    /// </summary>
    protected abstract void InitItemList();
    
    /// <summary>
    /// Item Pool 미리 생성
    /// </summary>
    protected void InitItemPool(int initSize)
    {
        DynamicItemPool = new UIDynamicObjectPool<UIWgItem>(originItem, itemRoot, initSize);
    }
    
    /// <summary>
    /// 인벤토리 GUI 업데이트
    /// </summary>
    public abstract void UpdateGUI();

    /// <summary>
    /// Item 클릭 시 파라미터(InventoryItem)에 따라 호출되는 이벤트 처리 메서드
    /// </summary>
    protected void OnItemClick(InventoryItem inventoryItem)
    {
        itemClickEventByInven.Invoke(inventoryItem);
        
        if(!isDuplicated)
            UpdateGUI();
    }
    
    /// <summary>
    /// Item 클릭 시 파라미터(ItemData)에 따라 호출되는 이벤트 처리 메서드
    /// </summary>
    protected void OnItemClick(ItemData itemData)
    {
        itemClickEventByData.Invoke(itemData);
        
        if(!isDuplicated)
            UpdateGUI();
    }
    
    #endregion
}
