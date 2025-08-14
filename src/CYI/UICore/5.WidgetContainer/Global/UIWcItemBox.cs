using System.Collections.Generic;

/// <summary>
/// UIBaseWcItemInven을 아이템 인벤토리에서 쓰일 부분 재정의 
/// </summary>
public class UIWcItemBox : UIBaseWcItemBox
{
    private int lastItemCount;
    
    /// <summary>
    /// Item Pool 미리 생성 및 초기화
    /// </summary>
    protected override void InitItemList()
    {
        InitItemPool(105);
        int poolCount = DynamicItemPool.GetInactiveCount();
        
        for (int i = 0; i < poolCount; i++)
        {
            var item = DynamicItemPool.Get();
            item.Initialize();
            item.ShowInventory(null);
            item.SetClickEvent(OnItemClick);
        }
    }

    /// <summary>
    /// Item 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnItemClick(int itemIndex)
    {
        IReadOnlyList<InventoryItem> inventoryItems = InventoryManager.Instance.Cache.InventoryDict[ItemType];
        var item = inventoryItems.Count <= itemIndex || itemIndex < 0 ? null : inventoryItems[itemIndex];
        
        base.OnItemClick(item);
    }

    /// <summary>
    /// 인벤토리 GUI 업데이트 재정의
    /// </summary>
    public override void UpdateGUI()
    {
        if(ItemType == ItemType.None) return;
        
        IReadOnlyList<InventoryItem> itemList = InventoryManager.Instance.Cache.InventoryDict[ItemType];
        var itemSlotList = DynamicItemPool.GetActiveList();
        
        for (int i = 0; i < itemSlotList.Count; i++)
        {
            if (i < itemList.Count)
            {
                itemSlotList[i].ShowInventory(itemList[i], i);
                continue;
            }
            if(itemList.Count == lastItemCount)
                break;
            itemSlotList[i].ShowInventory(null);
        }

        lastItemCount = itemList.Count;

        CallbackUpdatedUI();
    }
    
    /// <summary>
    /// UI 업데이트 이후 호출할 콜백
    /// </summary>
    protected virtual void CallbackUpdatedUI() { }
}
