using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 가챠 시스템의 진입점 클래스. 외부에서 TryGacha를 호출하면 내부 서비스들을 조합하여 가챠를 실행함.
/// 마스터데이터 기반 캐시 관리
/// </summary>
public class PityResultOpenContext : IUIOpenContext
{
    public int PityCount;
    public ItemData ItemData;
}

[Serializable]
public class GachaItem
{
    public GachaItem(ItemData itemData, float weight)
    {
        ItemData = itemData;
        Weight = weight;
    }

    public ItemData ItemData { get; }
    public float Weight { get; set; }
}

/// <summary>
/// UI 가챠 결과를 담을 클래스
/// </summary>
public class GachaResult
{
    public GachaResultOpenContext GachaContext = new ();
    public PityResultOpenContext PityContext = new ();
}

/// <summary>
/// 가챠 캐싱할 데이터 목록
/// </summary>
public class GachaCache
{
    // 마스터데이터 기반 테이블 (초기화 시 입력됨)
    private IReadOnlyDictionary<ResourceType, Dictionary<ItemRarity, float>> gachaTableByType;
    private IReadOnlyDictionary<ResourceType, float> gachaPityIncreases;
    private IReadOnlyDictionary<ResourceType, float> gachaLegendaryChance;
    private IReadOnlyDictionary<ResourceType, int> gachaPityThresholds;
    private IReadOnlyDictionary<ResourceType, int> gachaCosts;
    private IReadOnlyDictionary<string, ItemData> itemDataByItemCode;

    // 캐싱
    private Dictionary<ResourceType, List<GachaItem>> gachaItemsByType;
    private Dictionary<ItemRarity, int> countsOfRarityByDia;
    private Dictionary<ResourceType, bool> isHalfPityReached;
    private Dictionary<ResourceType, bool> isHardPityReached;
    private Dictionary<ResourceType, bool> wasHardPityReachedBeforeLegend;

    // 외부 접근용 프로퍼티
    public IReadOnlyDictionary<ResourceType, int> GachaCosts => gachaCosts;
    public IReadOnlyDictionary<ResourceType, float> GachaLegendaryChance => gachaLegendaryChance;
    

    /// <summary>
    /// 특정 타입의 천장 카운트 기준 반환
    /// </summary>
    public int GetPityThreshold(ResourceType type) => gachaPityThresholds[type];
 
    /// <summary>
    /// 초기화: 마스터데이터 기반으로 가챠 데이터 생성
    /// </summary>
    public void Initialize()
    {
        // Data 초기화
        gachaTableByType = MasterData.GachaTableByType;
        gachaPityIncreases = MasterData.GachaPityIncreases;
        gachaLegendaryChance = MasterData.GachaPityLegendChances;
        gachaPityThresholds = MasterData.GachaPityThresholds;
        gachaCosts = MasterData.GachaCosts;
        itemDataByItemCode = MasterData.GachaItemDataDict;

        // 자료구조 초기화
        gachaItemsByType = new();
        countsOfRarityByDia = new();
        isHalfPityReached = new();
        isHardPityReached = new();
        wasHardPityReachedBeforeLegend = new();

        InitWeights();         // 가중치 초기화
        InitRarityCounts();    // 다이아 가챠 희귀도별 개수 계산
        InitPityFlags();   // 반천장, 천장 여부 초기화
    }

    /// <summary>
    /// 리소스 타입에 따라 가챠 아이템 리스트 반환
    /// </summary>
    public List<GachaItem> GetItems(ResourceType type) => gachaItemsByType[type];

    /// <summary>
    /// 특정 희귀도에 해당하는 아이템 목록 반환
    /// </summary>
    public List<ItemData> GetItemsByRarity(ResourceType type, ItemRarity rarity)
    {
        return gachaItemsByType[type]
            .Where(g => g.ItemData.Rarity == rarity)
            .Select(g => g.ItemData)
            .ToList();
    }
    
    /// <summary>
    /// 반 천장 초기화
    /// </summary>
    public void ResetHalfPityFlag(ResourceType type)
    {
        isHalfPityReached[type] = false;
    }
    
    /// <summary>
    /// 하드 천장 조건 달성 여부 캐싱
    /// </summary>
    public void SetHardPityFlag(ResourceType type, int pityCount)
    {
        int threshold = gachaPityThresholds[type];
        isHardPityReached[type] = pityCount >= threshold;
    }

    /// <summary>
    /// 하드 천장 초기화
    /// </summary>
    public void ResetHardPityFlag(ResourceType type)
    {
        isHardPityReached[type] = false;
    }

    /// <summary>
    /// 반천장 여부 확인
    /// </summary>
    public bool IsHalfPity(ResourceType type)
    {
        return isHalfPityReached.TryGetValue(type, out bool flag) && flag;
    }
    
    /// <summary>
    /// 반천장 상태를 설정
    /// </summary>
    public void SetHalfPityFlag(ResourceType type, bool value)
    {
        isHalfPityReached[type] = value;
    }

    /// <summary>
    /// 천장 여부 확인
    /// </summary>
    public bool IsHardPity(ResourceType type)
    {
        return isHardPityReached.TryGetValue(type, out bool flag) && flag;
    }
    
    /// <summary>
    /// 천장 특이케이스 상태 확인
    /// </summary>
    public bool IsHardPityReachedBeforeLegend(ResourceType type)
    {
        return wasHardPityReachedBeforeLegend.TryGetValue(type, out bool flag) && flag;
    }
    
    /// <summary>
    /// 천장 특이케이스 상태를 설정
    /// </summary>
    public void SetHardPityReachedBeforeLegend(ResourceType type, bool value)
    {
        wasHardPityReachedBeforeLegend[type] = value;
    }

    /// <summary>
    /// 다이아 가챠 반천장 이후 전설 아이템 확률 증가
    /// </summary>
    public void UpdateWeightAfterHalfPity(ResourceType type, int overCount)
    {
        if (type != ResourceType.Diamond || overCount <= 0)
            return;

        float legendaryIncrease = gachaPityIncreases[type];
        float legendaryDecrease = legendaryIncrease / 4f;

        foreach (var item in gachaItemsByType[type])
        {
            var rarity = item.ItemData.Rarity;
            if (rarity == ItemRarity.Legendary)
            {
                item.Weight += legendaryIncrease / countsOfRarityByDia[rarity];
            }
            else
            {
                item.Weight = Mathf.Max(0, item.Weight - legendaryDecrease / countsOfRarityByDia[rarity]);
            }
        }
    }

    /// <summary>
    /// 다이아 가챠 가중치 초기화
    /// </summary>
    public void ResetWeight(ResourceType type)
    {
        foreach (var item in gachaItemsByType[type])
        {
            float rarityWeight = gachaTableByType[type][item.ItemData.Rarity];
            int rarityCount = gachaItemsByType[type].Count(g => g.ItemData.Rarity == item.ItemData.Rarity);
            item.Weight = rarityWeight / rarityCount;
        }
    }

    /// <summary>
    /// MasterData를 기준으로 아이템별 가중치를 초기화함
    /// </summary>
    private void InitWeights()
    {
        foreach (var resourceType in gachaTableByType.Keys)
        {
            gachaItemsByType[resourceType] = new List<GachaItem>();
            foreach (var item in itemDataByItemCode.Values)
            {
                float rarityWeight = gachaTableByType[resourceType][item.Rarity];
                int rarityCount = itemDataByItemCode.Values.Count(i => i.Rarity == item.Rarity);
                float weight = rarityWeight / rarityCount;
                gachaItemsByType[resourceType].Add(new GachaItem(item, weight));
            }
        }
    }

    /// <summary>
    /// 다이아 가챠에 대한 희귀도별 아이템 개수 계산
    /// </summary>
    private void InitRarityCounts()
    {
        // 다이아 전용: 희귀도별 아이템 개수 미리 계산 (Pity 로직에서 사용)
        var diaItems = gachaItemsByType[ResourceType.Diamond];
        foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            int count = diaItems.Count(g => g.ItemData.Rarity == rarity);
            countsOfRarityByDia[rarity] = count;
        }
    }

    /// <summary>
    /// 반천장, 천장 조건 달성 여부를 캐싱하여 플래그 설정
    /// </summary>
    private void InitPityFlags()
    {
        foreach (var type in gachaPityThresholds.Keys)
        {
            int count = InventoryManager.Instance.PityService.GetPityCount(type);
            int threshold = gachaPityThresholds[type];
            isHalfPityReached[type] = count >= threshold / 2;
            isHardPityReached[type] = count >= threshold;
            wasHardPityReachedBeforeLegend[type] = false;
        }
    }
}