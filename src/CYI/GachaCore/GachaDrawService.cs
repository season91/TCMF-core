using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
/// <summary>
/// 순수한 아이템 뽑기 로직을 담당함.
/// 상태 변경 없음. 가중치 기반 추출 및 보정 처리만 수행.
/// </summary>
public class GachaDrawService
{
    private readonly GachaCache cache;

    public GachaDrawService(GachaCache cache)
    {
        this.cache = cache;
    }
    
    /// <summary>
    /// 확률 기반으로 N개의 아이템을 동기적으로 뽑음
    /// - 각 아이템은 하나씩 DrawItem() 호출
    /// - 10회 뽑기 시 소프트천장(Rare 이상 보장) 적용
    /// </summary>
    public List<ItemData> DrawItems(ResourceType type, int count)
    {
        List<ItemData> resultList = new List<ItemData>();

        // 1. Count만큼 반복하여 아이템 뽑기
        for (int i = 0; i < count; i++)
        {
            resultList.Add(DrawSingleItem(type));
            AnalyticsHelper.LogGachaResultEvent(type, count, resultList[i].Rarity, resultList[i].Code);
        }

        // 2. 다이아 10연차 시 소프트 천장 로직
        if (count >= 10 && type == ResourceType.Diamond)
        {
            bool hasRareOrHigher = resultList.Any(i => i.Rarity >= ItemRarity.Rare);
            if (!hasRareOrHigher)
            {
                resultList[9] = GetItemByRarity(type, ItemRarity.Rare);
            }
        }

        return resultList;
    }

    /// <summary>
    /// 실제 확률 기반으로 1개 아이템 동기 추출
    /// </summary>
    private ItemData DrawSingleItem(ResourceType type)
    {
        // 1. 해당 타입의 가챠 아이템 목록 가져오기
        List<GachaItem> gachaItems = cache.GetItems(type);

        // 2. 총 가중치 계산
        // float형이라 소숫점의 오차가 있을 수 있기 때문에 매번 totalWeight을 계산해줘야 오차 없음
        float totalWeight = gachaItems.Sum(item => item.Weight);
        float selectNum = Random.Range(0f, totalWeight);

        // 3. 랜덤 숫자에 해당하는 아이템 선택
        foreach (var item in gachaItems)
        {
            //아이템의 가중치가 랜덤 숫자보다 높다면 아이템 뽑기 성공
            if (item.Weight >= selectNum)
            {
                return item.ItemData;
            }
            selectNum -= item.Weight;
        }
        
        throw new Exception("Gacha Error Item Not Found");
    }
    
    /// <summary>
    /// 소프트 천장 보상 아이템 추출 (희귀도 보정용)
    /// </summary>
    private ItemData GetItemByRarity(ResourceType type, ItemRarity rarity)
    {
        var items = cache.GetItemsByRarity(type, rarity);
        return items[Random.Range(0, items.Count)];
    }
    
}