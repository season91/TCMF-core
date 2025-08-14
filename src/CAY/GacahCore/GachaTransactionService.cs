using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 리소스 차감, 인벤토리 지급 상태 변경 로직 전담
/// </summary>
public class GachaTransactionService
{
    private readonly GachaCache cache;

    public GachaTransactionService(GachaCache cache)
    {
        this.cache = cache;
    }
    
    /// <summary>
    /// 해당 리소스 타입에 대한 가챠 비용만큼 재화 차감 시도
    /// </summary>
    public async Task<bool> TryConsumeResourceAsync(ResourceType type, int count)
    {
        int totalCost = cache.GachaCosts[type] * count;

        // 재화 부족 시 차감 불가
        if (!InventoryManager.Instance.ResourceService.HasEnough(type, totalCost))
        {
            MyDebug.LogWarning($"재화 부족: {type} {totalCost} 필요");
            return false;
        }

        await InventoryManager.Instance.ResourceService.ConsumeAsync(type, totalCost);
        return true;
    }
    
    /// <summary>
    /// 인벤토리에 아이템 지급 (단일 or 복수)
    /// </summary>
    public async Task GiveItemsToInventory(List<ItemData> items, bool isDevMode)
    {
        int count = items.Count;
        foreach (var item in items)
        {
            
#if UNITY_EDITOR
            if (isDevMode)
            {
                MyDebug.Log($"[DevMode] 지급 스킵: {item.Name}");
                continue;
            }
#endif
            if (count == 1)
                await InventoryManager.Instance.ItemService.TryAddSingleItemAsync(item.Code);
        }

        if (count >= 10)
            await InventoryManager.Instance.ItemService.TryAddMultipleItemsAsync(items);
    }

}