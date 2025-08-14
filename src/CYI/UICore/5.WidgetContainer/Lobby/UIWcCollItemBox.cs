using System.Collections.Generic;

/// <summary>
/// UIBaseWcItemInven을 도감 아이템 목록에서 쓰일 부분 재정의
/// </summary>
public class UIWcCollItemBox : UIBaseWcItemBox
{
    /// <summary>
    /// Item Pool 미리 생성 및 초기화
    /// </summary>
    protected override void InitItemList()
    {
        int maxCount = 0;
        foreach (var list in InventoryManager.Instance.CollectionService.CollectedSlotDict.Values)
        {
            if (list.Count > maxCount)
                maxCount = list.Count;
        }

        InitItemPool(maxCount);
    }
    
    /// <summary>
    /// 인벤토리 GUI 업데이트 재정의
    /// </summary>
    public override void UpdateGUI()
    {
        IReadOnlyList<CollectionSlot> collectionList = InventoryManager.Instance.CollectionService.CollectedSlotDict[ItemType];
        var itemList = DynamicItemPool.GetActiveList();
        if (itemList.Count != collectionList.Count) // 기존이랑 다르면 생성 후 세팅
        {
            DynamicItemPool.OffAll();
            for (int i = 0; i < collectionList.Count; i++)
            {
                var item = DynamicItemPool.Get();
                item.Initialize();
                item.ShowCollection(collectionList[i], i);
                item.SetClickEvent(OnItemClick);
            }
        }
        else
        {
            for (int i = 0; i < itemList.Count; i++) // 기존이랑 같으면 생성 X, 기존 순서로 세팅
            {
                itemList[i].ShowCollection(collectionList[i], i);
                itemList[i].SetClickEvent(OnItemClick);
            }
        }
    }
    
    /// <summary>
    /// Item 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnItemClick(int itemIndex)
    {
        IReadOnlyList<CollectionSlot> collectionList = InventoryManager.Instance.CollectionService.CollectedSlotDict[ItemType];
        var collectionSlot = collectionList[itemIndex];
        ItemData data = collectionList.Count <= itemIndex 
                        || itemIndex < 0 
                        || !collectionSlot.IsCollected 
            ? null : collectionSlot.ItemData;

        if (collectionSlot.IsCollected && !collectionSlot.IsRewardClaimed)
        {
            _ = InventoryManager.Instance.CollectionService.TryRewardCollectionAsync(collectionSlot.Status.Code);
            var rewards = InventoryManager.Instance.CollectionService.GetComposeCollectionRewards(collectionSlot.Status.Code);
            RewardOpenContext context = new()
            {
                Title = "도감 완성 보상", 
                ButtonText = "확인", 
                ButtonEvent = null, 
                RewardList = rewards
            };
            UIManager.Instance.Open<UIPGlobalReward>(OpenContext.WithContext(context));
        }
        
        base.OnItemClick(data);
    }
}
