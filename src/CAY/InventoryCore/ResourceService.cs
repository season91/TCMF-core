using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 인벤토리 재화 처리 전담 서비스
/// - 자원 보유 확인
/// - 자원 소모 및 추가
/// - Firestore 반영 처리
/// </summary>
public class ResourceService
{
    private readonly InventoryCache cache;
    public ResourceService(InventoryCache cache)
    {
        this.cache = cache;
    }
        
    /// <summary>
    /// 재화 보유량이 충분한지 확인
    /// </summary>
    public bool HasEnough(ResourceType type, int amount)
    {
        return UserData.inventory.CurrencyResource.TryGetValue(type, out int current) && current >= amount;
    }

    /// <summary>
    /// 자원 소모 처리 (Firestore 업데이트 포함)
    /// </summary>
    public async Task ConsumeAsync(ResourceType type, int amount)
    {
        if (!HasEnough(type, amount))
        {
            MyDebug.LogWarning($"[{type}] 재화 부족: {amount}만큼 필요");
            return;
        }

        UserData.inventory.CurrencyResource[type] -= amount;

        await UpdateResourceAsync(type);
    }

    /// <summary>
    /// 자원 추가 처리 (Firestore 업데이트 포함)
    /// </summary>
    public async Task AddAsync(ResourceType type, int amount)
    {
        if (!UserData.inventory.CurrencyResource.ContainsKey(type))
        {
            UserData.inventory.CurrencyResource[type] = 0;
        }

        UserData.inventory.CurrencyResource[type] += amount;

        await UpdateResourceAsync(type);
    }

    /// <summary>
    /// UI 에 자원 반영
    /// Firestore에 해당 자원 반영
    /// </summary>
    private async Task UpdateResourceAsync(ResourceType type)
    {
        UIManager.Instance.UpdateUserResource(type);
        await UserData.inventory.UpdateInventoryResource(type);
    }
}