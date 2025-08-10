using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
///  아이템 지급, 소모, 조회만 담당
/// </summary>
public class ItemService
{
    private readonly InventoryCache cache;
    
    public ItemService(InventoryCache cache)
    {
        this.cache = cache;
    }
    /// <summary>
    /// UserData.inventory.Items를 초기화하는 메서드
    /// BindingItemData, BindingSumStat 호출 후 Cache에 등록함
    /// </summary>
    public void InitializeItems()
    {
        foreach (var item in UserData.inventory.Items)
        {
            if (TryGetItemData(item.ItemCode, out var itemData))
            {
                item.BindingItemData(itemData); // 마스터 데이터 바인딩
            }

            item.BindingSumStat(); // 스탯 합산

            cache.AddItem(item); // 캐시에 등록
        }
    }

    /// <summary>
    /// itemCode 기준으로 마스터 데이터에서 아이템 정보를 조회
    /// </summary>
    private bool TryGetItemData(string itemCode, out ItemData itemData)
    {
        return MasterData.ItemDataDict.TryGetValue(itemCode, out itemData);
    }
    
    /// <summary>
    /// 마스터데이터 기반으로 InventoryItem 생성
    /// </summary>
    private InventoryItem CreateInventoryItem(string itemCode, ItemData itemData)
    {
        var item = new InventoryItem
        {
            itemCode = itemCode,
            itemType = itemData.Type,
            limitBreakLevel = 0,
            enhancementLevel = 0
        };

        item.BindingItemData(itemData);
        item.BindingSumStat();

        return item;
    }
    
    /// <summary>
    /// 인벤토리에 아이템 1개 지급 시도
    /// </summary>
    public async Task<bool> TryAddSingleItemAsync(string itemCode)
    {
        if (!TryGetItemData(itemCode, out var itemData))
            return false;

        if (cache.IsInventoryFullForItemType(itemData.Type))
            return false;

        var item = CreateInventoryItem(itemCode, itemData);

        // 로컬에 반영
        UserData.inventory.AddItem(item);
        cache.AddItem(item);
        
        // 업로드
        await FirestoreUploader.SaveInventoryItem(FirebaseManager.Instance.DbUser.UserId, item);
        
        // 도감 등록 및 갱신
        await InventoryManager.Instance.CollectionService.TryCollect(itemData);
        
        return true;
    }

    /// <summary>
    /// 인벤토리 여러개 아이템 지급시
    /// </summary>
    public async Task<bool> TryAddMultipleItemsAsync(List<ItemData> drawResults)
    {
        List<InventoryItem> validItems = new();

        foreach (var itemData in drawResults)
        {
            //  인벤토리 수량 검사
            if (cache.IsInventoryFullForItemType(itemData.Type))
                return false;

            // 아이템 생성
            var item = CreateInventoryItem(itemData.Code, itemData);

            // 데이터 반영
            UserData.inventory.AddItem(item);
            cache.AddItem(item);
            validItems.Add(item);
        }

        // 업로드
        await FirestoreUploader.SaveInventoryItemsBatch(FirebaseManager.Instance.DbUser.UserId, validItems);
                
        // 도감 등록 및 갱신
        await InventoryManager.Instance.CollectionService.TryCollect(drawResults);
        
        return true;
    }
    
    /// <summary>
    /// 단일 아이템 소모 (로컬 제거 + 서버 반영)
    /// </summary>
    public async Task ConsumeItemAsync(InventoryItem item)
    {
        UserData.inventory.RemoveItem(item);
        cache.RemoveItem(item);
        await FirestoreUploader.DeleteInventoryItem(FirebaseManager.Instance.DbUser.UserId, item.ItemUid);
    }
    
    /// <summary>
    /// 복수 아이템 소모 (로컬 제거 + 서버 반영)
    /// </summary>
    public async Task ConsumeItemsAsync(List<InventoryItem> items)
    {
        foreach (var item in items)
        {
            UserData.inventory.RemoveItem(item);
            cache.RemoveItem(item);
        }
        await FirestoreUploader.DeleteInventoryItemsBatch(FirebaseManager.Instance.DbUser.UserId, items);
    }

    /// <summary>
    /// 전체 인벤토리에서 특정 itemCode를 가진 아이템 리스트 반환
    /// </summary>
    public List<InventoryItem> GetItemsByCode(string itemCode)
    {
    return cache.GetAllItems().Where(i => i.ItemCode == itemCode).ToList();
    }
    
    /// <summary>
    /// ItemType별로 N번째 아이템을 반환 (UI용 슬롯 인덱스 기준)
    /// </summary>
    public InventoryItem GetItemBySlotIndex(ItemType type, int index)
    {
        return cache.GetItemBySlotIndex(index, type);
    }
    
    /// <summary>
    /// 특정 itemCode의 보유 목록
    /// </summary>
    public List<InventoryItem> GetMaterialItem(ItemType type, string itemCode, string itemUid)
    {
        return cache.GetItemsByType(type).Where(i => i.ItemCode == itemCode && i.ItemUid != itemUid && i.LimitBreakLevel == 0 && i.EnhancementLevel == 0 && i.EquippedUnitUid == null).ToList();
    }
    
    /// <summary>
    /// 특정 itemCode의 보유 수량 반환
    /// </summary>
    public int GetMaterialItemCount(ItemType type, string itemCode, string itemUid)
    {
        return GetMaterialItem(type, itemCode, itemUid).Count;
    }
}