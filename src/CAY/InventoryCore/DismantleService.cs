using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 아이템 분해 로직만 담당하는 서비스 클래스
/// </summary>
public class DismantleService
{
    private readonly ItemService itemService;
    private readonly ResourceService resourceService;
    private InventoryItem dismantleItem;
    
    public DismantleService(ItemService itemService, ResourceService resourceService)
    {
        this.itemService = itemService;
        this.resourceService = resourceService;
    }
    
    private void ShowWarning(string message)
    {
        var context = new SlideOpenContext { Comment = message };
        UIManager.Instance.Open<UISlidePopup>(OpenContext.WithContext(context));
    }
    
    /// <summary>
    /// 분해 시도
    /// </summary>
    // 분해 시도
    public async Task<bool> TryDismantle(InventoryItem item)
    {
        dismantleItem = item;
        
        // 아이템 존재 여부
        if (item == null)
        {
            MyDebug.LogWarning("분해 실패: 아이템이 존재하지 않음");
            return false;
        }

        if (!TryGetDismantleData(dismantleItem, out var dismantleData))
        {
            MyDebug.LogError($"분해 실패: 마스터 데이터에 해당 희귀도({dismantleItem.Rarity}) 정보 없음");
            return false;
        }
        
        // 아이템 소모 처리
        await itemService.ConsumeItemAsync(dismantleItem);
        
        // 리소스 지급 처리
        await resourceService.AddAsync(dismantleData.ResourceType, dismantleData.Amount);
        MyDebug.Log($"[Dismantle] {dismantleItem.ItemCode} 분해 완료 → {dismantleData.ResourceType} x{dismantleData.Amount}");
        return true;
    }

    public bool TryGetDismantleData(InventoryItem item, out ItemDismantleData dismantleData)
    {
        // 마스터 데이터에서 분해 기준 정보 가져오기
        if (!MasterData.ItemDismantleDataDic.TryGetValue(item.Rarity, out var data))
        {
            MyDebug.LogError($"분해 실패: 마스터 데이터에 해당 희귀도({item.Rarity}) 정보 없음");
            
            dismantleData = null;
            return false;
        }
        
        dismantleData = data;
        return true;
    }

    /// <summary>
    /// 주어진 아이템 목록 중 강화 또는 돌파된 아이템이 하나라도 있는지 여부 반환
    /// </summary>
    public bool HasUpgradedItem(List<InventoryItem> items)
    {
        return items.Any(item => item.LimitBreakLevel > 0 || item.EnhancementLevel > 0);
    }
}