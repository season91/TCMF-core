using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/// <summary>
/// 돌파 시도, 자원 소비, 조건 검사, 결과 적용까지 담당
/// 상태는 외부에서 주입
/// </summary>
public class LimitBreakService
{
    private readonly ItemService itemService;
    private readonly ResourceService resourceService;
    
    private const string MsgNoMaterial = "재료가 없습니다.";
    private const string MsgNoItem = "아이템이 없습니다.";
    
    public LimitBreakService(ItemService itemService, ResourceService resourceService)
    {
        this.itemService = itemService;
        this.resourceService = resourceService;
    }
    
    /// <summary>
    /// UI에 경고 메시지 출력
    /// </summary>
    private void ShowWarning(string message)
    {
        var context = new SlideOpenContext { Comment = message };
        UIManager.Instance.Open<UISlidePopup>(OpenContext.WithContext(context));
    }
    
    /// <summary>
    /// 돌파 시도
    /// </summary>
    public async Task<bool> TryLimitBreakAsync(InventoryItem item)
    {
        // 돌파 레시피 가져오기 
        if (!TryGetLimitBreakData(item, out var limitBreakData))
        {
            MyDebug.LogWarning("돌파 실패: 데이터 없음");
            return false;
        }
        
        // 자원 체크
        if (!resourceService.HasEnough(ResourceType.Gold, limitBreakData.RequiredGold))
        {
            MyDebug.LogWarning("돌파에 필요한 리소스 자원이 부족함");
            ShowWarning(MsgNoMaterial);
            return false;
        }

        // 재료 아이템 확인
        if (!CanItemsToConsume(item, limitBreakData, out var itemsToConsume))
        {
            MyDebug.LogWarning("필요한 아이템 수량이 부족합니다.");
            ShowWarning(MsgNoItem);
            return false;
        }

        /// 자원 소모
        await resourceService.ConsumeAsync(ResourceType.Gold, limitBreakData.RequiredGold);
        await itemService.ConsumeItemsAsync(itemsToConsume);
        
        // 성공 처리
        await ApplyLimitBreakSuccessAsync(item);

        MyDebug.Log($"아이템 돌파 완료! 새로운 레벨: {limitBreakData.LimitBreakLevel}");
        
        return true;
    }

    /// <summary>
    /// 조건에 맞는 레시피가 있는지 판단 (재료는 강화/돌파 없는 상태만 허용)
    /// </summary>
    public bool TryGetLimitBreakData(InventoryItem item, out LimitBreakData limitBreakData)
    {
        int toLevel = item.LimitBreakLevel + 1;
        ItemRarity rarity = item.Rarity;
        
        // 조건: 다음 레벨, 현재 희귀도에 맞는 레시피를 찾음
        var data = MasterData.LimitBreakData
            .FirstOrDefault(data => data.LimitBreakLevel == toLevel && data.Rarity == rarity);
        
        // 돌파 가능한지 확인
        if (data == null)
        {
            MyDebug.Log($"돌파 레시피 존재하지 않음! toLevel : {toLevel}");
            limitBreakData = null;
            return false;
        }

        limitBreakData = data;
        return true;
    }

    /// <summary>
    /// 소모할 재료 아이템 확인
    /// </summary>
    private bool CanItemsToConsume(InventoryItem item, LimitBreakData data, out List<InventoryItem> itemsToConsume)
    {
        // GetItemsByCode 한 번만 호출
        var materialItems = itemService.GetMaterialItem(item.ItemType, item.ItemCode, item.ItemUid);

        // 재료 아이템이 충분한지 체크
        if (materialItems.Count < data.RequiredItemCount)
        {
            MyDebug.LogWarning("재료 아이템 부족");
            itemsToConsume = null;
            return false;
        }

        // 충분하면 필요한 만큼 아이템 선택
        itemsToConsume = materialItems.Take(data.RequiredItemCount).ToList();
        return true;
    }

    /// <summary>
    /// 돌파 성공 처리
    /// </summary>
    private async Task ApplyLimitBreakSuccessAsync(InventoryItem item)
    {
        item.limitBreakLevel = item.LimitBreakLevel + 1;
        item.BindingSumStat();
        await FirestoreUploader.SaveInventoryItemAsync(FirebaseManager.Instance.DbUser.UserId, item);
        MyDebug.Log($"돌파 성공 → 레벨: {item.LimitBreakLevel}");
    }
    
}