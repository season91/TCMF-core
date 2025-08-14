using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum PityCountOperation
{
    Add,
    Decrease,
    Reset,
}

/// <summary>
/// 천장 로직 담당
/// </summary>
public class GachaPityService
{
    private readonly GachaCache cache;

    public GachaPityService(GachaCache cache)
    {
        this.cache = cache;
    }
    
    /// <summary>
    /// 반천장 or 천장 아이템 획득 시도 (없으면 null)
    /// </summary>
    public PityResultOpenContext TryGetPityItem(ResourceType type)
    {
        // 현재 피티 카운트와 천장 수치 조회
        int pityCount = InventoryManager.Instance.PityService.GetPityCount(type);
        int threshold = cache.GetPityThreshold(type);

        // 1. ApplyPityLogic에서 강제로 천장 도달했는데 전설 때문에 리셋된 경우
        if (cache.IsHardPityReachedBeforeLegend(type))
        {
            cache.SetHardPityReachedBeforeLegend(type, false); // 다시 초기화
            var item = GetHardPityItem(type);
            cache.SetHalfPityFlag(type, false);
            return new PityResultOpenContext { PityCount = threshold, ItemData = item };
        }
        
        // 2. 일반 하드 천장 조건
        if (cache.IsHardPity(type))
        {
            cache.ResetHardPityFlag(type); // 한 번만 지급
            var item = GetHardPityItem(type);
            cache.SetHalfPityFlag(type, false);
            return new PityResultOpenContext { PityCount = threshold, ItemData = item };
        }

        // 3. 반천장 조건: 아직 반천장 수령한 적 없고, 조건 도달했을 때
        if (!cache.IsHalfPity(type) && pityCount >= threshold / 2)
        {
            var item = GetHalfPityItem(type);
            cache.SetHalfPityFlag(type, true); // 한 번만 수령하도록 캐시 갱신
            return new PityResultOpenContext { PityCount = threshold / 2, ItemData = item };
        }

        MyDebug.Log("Not Pity Time!!!!");
        return null;
    }

    /// <summary>
    /// 천장 아이템 반환
    /// </summary>
    private ItemData GetHardPityItem(ResourceType type)
    {
        float chance = cache.GachaLegendaryChance[type];
        ItemData item;

        switch (type)
        {
            case ResourceType.Gold:
                float prob = Random.Range(0f, 100f);
                item = GetGuaranteedItem(type, prob < chance ? ItemRarity.Legendary : ItemRarity.SuperRare);
                break;
            case ResourceType.Diamond:
                item = GetGuaranteedItem(type, ItemRarity.Legendary);
                break;
            default:
                return null;
        }

        AnalyticsHelper.LogGachaPityEvent(type, "hard", InventoryManager.Instance.PityService.GetPityCount(type));
        return item;
    }

    /// <summary>
    /// 반천장 아이템 반환
    /// </summary>
    private ItemData GetHalfPityItem(ResourceType type)
    {
        float chance = cache.GachaLegendaryChance[type];
        float prob = Random.Range(0f, 100f);

        ItemData item = type switch
        {
            ResourceType.Gold => GetGuaranteedItem(type, ItemRarity.SuperRare),
            ResourceType.Diamond => GetGuaranteedItem(type, prob < chance ? ItemRarity.Legendary : ItemRarity.SuperRare),
            _ => null
        };

        AnalyticsHelper.LogGachaPityEvent(type, "half", InventoryManager.Instance.PityService.GetPityCount(type));
        return item;
    }

    /// <summary>
    /// 특정 희귀도 보장 아이템 반환
    /// </summary>
    private ItemData GetGuaranteedItem(ResourceType type, ItemRarity rarity)
    {
        var candidates = cache.GetItemsByRarity(type, rarity);
        return candidates[Random.Range(0, candidates.Count)];
    }
    
    /// <summary>
    /// 뽑은 아이템을 기준으로 피티 카운트 조정
    /// - 전설 등장 시 → 리셋
    /// - 천장 도달 시 → 감산(초과분 보존)
    /// - 그 외 → +1
    /// </summary>
    public async Task ApplyPityLogicAsync(ResourceType type, List<ItemData> items)
    {
        int pityCount = InventoryManager.Instance.PityService.GetPityCount(type);
        int threshold = cache.GetPityThreshold(type);
        
        for (int i = 0; i < items.Count; i++)
        {
            pityCount++;
            var item = items[i];

            // 1. CheckGetLegend 전설 → 피티 리셋 + 반천장 상태 초기화
            if (item.Rarity == ItemRarity.Legendary)
            {
                // 전설 등장 시 현재 pityCount 기준으로 천장 넘었는지 판단
                if (pityCount > threshold)
                {
                    await UpdatePityCountAsync(type, PityCountOperation.Decrease, -threshold);
                    cache.SetHardPityReachedBeforeLegend(type, true);
                    // 하드 천장 보상 수령은 TryGetPityItem()에서 수행됨
                }
                
                // 하드/반천장 캐시 초기화
                cache.ResetHardPityFlag(type);
                cache.ResetHalfPityFlag(type);
                
                // 전설 등장 순간에 리셋
                await UpdatePityCountAsync(type, PityCountOperation.Reset, 0);
                pityCount = 0;
                continue;
            }

            // 2. 하드 천장 도달 → threshold만큼 감산하여 초과분 보존
            if (pityCount == threshold)
            {
                await UpdatePityCountAsync(type, PityCountOperation.Decrease, -threshold);
                // 하드 천장 플래그 설정
                cache.SetHardPityFlag(type, pityCount);
                continue; // 이 줄 추가가 핵심
            }

            // 3. 일반 등급 → 피티 +1
            await UpdatePityCountAsync(type, PityCountOperation.Add, 1);
        }
    }
    
    /// <summary>
    /// Pity 카운트 업데이트
    /// </summary>
    public async Task UpdatePityCountAsync(ResourceType type, PityCountOperation op, int amount)
    {
        switch (op)
        {
            case PityCountOperation.Add:
                await AddPityCountAsync(type, amount);
                break;
            case PityCountOperation.Reset:
                await ResetPityCountAsync(type, amount);
                break;
            case PityCountOperation.Decrease:
                await DecreasePityCountAsync(type, amount);
                break;
        }
    }

    /// <summary>
    /// 천장 카운트 추가
    /// </summary>
    private async Task AddPityCountAsync(ResourceType type, int amount)
    {
        await InventoryManager.Instance.PityService.AddPityCountAsync(type, amount);
        if (type == ResourceType.Diamond)
        {
            int over = InventoryManager.Instance.PityService.GetPityCount(type) - cache.GetPityThreshold(type) / 2;
            cache.UpdateWeightAfterHalfPity(type, over);
        }
    }
    
    /// <summary>
    /// 천장 카운트 설정
    /// </summary>
    private async Task ResetPityCountAsync(ResourceType type, int amount)
    {
        await InventoryManager.Instance.PityService.SetPityCountAsync(type, amount);
        if (type == ResourceType.Diamond)
            cache.ResetWeight(type);
    }

    /// <summary>
    /// 천장 카운트 감소값으로 설정
    /// </summary>
    private async Task DecreasePityCountAsync(ResourceType type, int amount)
    {
        int newCount = Mathf.Max(0, InventoryManager.Instance.PityService.GetPityCount(type) + amount);
        await InventoryManager.Instance.PityService.SetPityCountAsync(type, newCount);
        if (type == ResourceType.Diamond)
            cache.ResetWeight(type);
    }
}
