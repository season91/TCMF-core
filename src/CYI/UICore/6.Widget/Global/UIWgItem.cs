using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 내 모든 Item UI의 Base를 
/// </summary>
public class UIWgItem : MonoBehaviour
{
    [SerializeField] private GameObject objItem;
    [SerializeField] private Image imgBg;
    [SerializeField] private Image imgFrame;
    [SerializeField] protected Image imgIcon;
    
    [SerializeField] protected GameObject objSelected;
    [SerializeField] private Button btn;
    
    private int itemIndex;
    private Action<int> clickEvent;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected virtual void Reset()
    {
        objItem = transform.FindChildByName<Transform>("Group_Item")?.gameObject;
        imgBg = GetComponent<Image>();
        imgFrame = transform.FindChildByName<Image>("Img_ItemFrame");
        imgIcon = transform.FindChildByName<Image>("Img_ItemIcon");
        
        objSelected = transform.FindChildByName<Transform>("Group_Select")?.gameObject;
        btn = GetComponent<Button>();
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize()
    {
        btn.onClick.RemoveAllListeners();
        btn.AddListener(OnClick);
    }
    
    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        objItem.SetActive(false);   
        objSelected.SetActive(false);
        imgBg.color = Color.white;
        imgFrame.enabled = false;
    }

    /// <summary>
    /// Item Data의 null 여부 판단 및 아이템 초기 상태 설정
    /// </summary>
    private bool TryShowItem(ItemData itemData, int index = -1)
    {
        gameObject.SetActive(true);
        itemIndex = index;
        ResetUI();
        
        return itemData != null; // Inventory Item이 null이면 빈 칸 표시
    }
    
    /// <summary>
    /// Item Data 기준의 GUI 표시
    /// 인벤토리 Item X
    /// </summary>
    public void ShowData(ItemData itemData, int index = -1)
    {
        if(!TryShowItem(itemData, index)) return;
        SetItemGUI(itemData);
    }
    
    /// <summary>
    /// Inventory Item 기준의 GUI 표시
    /// 인벤토리 Item O
    /// </summary>
    public virtual void ShowInventory(InventoryItem inventoryItem, int index = -1)
    {
        ItemData itemData = null;
        if (inventoryItem != null && MasterData.ItemDataDict.TryGetValue(inventoryItem.ItemCode, out var data))
        {
            itemData = data;
        }
        if(!TryShowItem(itemData, index)) return;
        SetItemGUI(itemData);
    }
    
    /// <summary>
    /// Item Data 기준의 GUI 표시
    /// 인벤토리 Item X
    /// </summary>
    public virtual void ShowCollection(CollectionSlot slot, int index = -1)
    {
        if(!TryShowItem(slot.ItemData, index)) return;
        SetItemGUI(slot.ItemData, slot.IsCollected);
    } 
    
    /// <summary>
    /// Item Data에 따른 아이템 GUI 세팅
    /// </summary>
    private void SetItemGUI(ItemData data, bool isCollected = true)
    {
        objItem.SetActive(true);
        imgIcon.sprite = data.Icon;
        if(!isCollected) return;
        
        string itemFrameAddress = StringAdrItemBg.ItemFrameDict[data.Rarity];
        imgBg.color = data.Rarity.ToColor();
        imgFrame.enabled = true;
        imgFrame.sprite = ResourceManager.Instance.GetResource<Sprite>(itemFrameAddress);
    }
    
    /// <summary>
    /// Item 클릭 시 호출되는 추가 이벤트 설정
    /// </summary>
    public void SetClickEvent(Action<int> onClick) => clickEvent = onClick;

    /// <summary>
    /// Item 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    protected virtual void OnClick()
    {
        bool isActive = !objSelected.activeSelf;
        
        if(clickEvent == null || itemIndex == -1) return;
        clickEvent.Invoke(itemIndex);
        SetSelectedObj(isActive);
    }

    /// <summary>
    /// 선택된 프레임 오브젝트 활성화/비활성화
    /// </summary>
    public void SetSelectedObj(bool isActive)
    {
        objSelected.SetActive(isActive);
    }
}
