using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 인벤토리 캐시 전담 클래스
/// - 아이템 타입별 딕셔너리 캐시
/// - ItemUid 기반 아이템/유닛 조회용 딕셔너리 캐시
/// - 최대 수량 제한 관리
/// </summary>
public class InventoryCache
{
    // 최대 아이템 수 제한 (탭별)
    private const int MaxItemCount = 105;

    // 내부 캐시용 딕셔너리 (수정 가능)
    private Dictionary<ItemType, List<InventoryItem>> inventoryDict = new();
    private Dictionary<ItemType, IReadOnlyList<InventoryItem>> inventoryReadOnlyDict = new();

    // 아이템 UID로 아이템 조회
    private Dictionary<string, InventoryItem> itemUidToItemDic = new();

    // 아이템 UID로 장착된 유닛 조회
    private Dictionary<string, InventoryUnit> itemUidToUnitDic = new();

    // 외부에 노출할 읽기 전용 프로퍼티
    public IReadOnlyDictionary<ItemType, IReadOnlyList<InventoryItem>> InventoryDict => inventoryReadOnlyDict;
    public IReadOnlyDictionary<string, InventoryItem> ItemUidToItemDic => itemUidToItemDic;
    public IReadOnlyDictionary<string, InventoryUnit> ItemUidToUnitDic => itemUidToUnitDic;
    
    /// <summary>
    /// UserData에서 아이템/유닛 정보를 읽어 초기화
    /// 반드시 로그인 이후 호출되어야 함
    /// </summary>
    public void Initialize()
    {
        inventoryDict.Clear();
        inventoryReadOnlyDict[ItemType.Weapon] = new List<InventoryItem>();
        inventoryReadOnlyDict[ItemType.Armor] = new List<InventoryItem>();
        itemUidToItemDic.Clear();
        itemUidToUnitDic.Clear();
    }

    /// <summary>
    /// 새 아이템을 캐시에 추가 (지급 시 호출)
    /// </summary>
    public void AddItem(InventoryItem item)
    {
        itemUidToItemDic[item.ItemUid] = item;

        if (!inventoryDict.TryGetValue(item.ItemType, out var list))
        {
            list = new List<InventoryItem>();
            inventoryDict[item.ItemType] = list;
            inventoryReadOnlyDict[item.ItemType] = list;
        }

        list.Add(item);
    }

    /// <summary>
    /// 아이템 제거 (소모 시 호출)
    /// </summary>
    public void RemoveItem(InventoryItem item)
    {
        itemUidToItemDic.Remove(item.ItemUid);

        if (inventoryDict.TryGetValue(item.ItemType, out var list))
        {
            list.Remove(item);
            if (list.Count == 0)
            {
                inventoryDict.Remove(item.ItemType);
                inventoryReadOnlyDict.Remove(item.ItemType);
            }
        }

        itemUidToUnitDic.Remove(item.ItemUid);
    }

    /// <summary>
    /// 인벤토리 특정 타입의 아이템 리스트 반환
    /// </summary>
    public List<InventoryItem> GetItemsByType(ItemType type)
    {
        return inventoryDict.TryGetValue(type, out var list) ? list : new List<InventoryItem>();
    }

    /// <summary>
    /// 인벤토리에 등록된 모든 아이템 반환 (flat하게)
    /// </summary>
    public List<InventoryItem> GetAllItems()
    {
        return inventoryDict.Values.SelectMany(x => x).ToList();
    }

    /// <summary>
    /// 해당 타입의 아이템이 최대 수량 도달했는지 검사
    /// </summary>
    public bool IsInventoryFullForItemType(ItemType type)
    {
        return inventoryDict.TryGetValue(type, out var list) && list.Count >= MaxItemCount;
    }

    /// <summary>
    /// 특정 UID의 유닛 연결 정보 갱신 (장착 시 사용)
    /// </summary>
    public void UpdateItemToUnit(string itemUid, InventoryUnit unit)
    {
        if (unit == null)
            itemUidToUnitDic.Remove(itemUid);
        else
            itemUidToUnitDic[itemUid] = unit;
    }
    
    /// <summary>
    /// 현재 인벤토리 소지 개수
    /// </summary>
    public int GetItemCount(ItemType type)
    {
        return inventoryDict.TryGetValue(type, out var list) ? list.Count : 0;
    }

    /// <summary>
    /// ItemType별로 N번째 아이템을 가져오는 함수
    /// UI에서 ItemType + index 조합으로 접근 시 사용
    /// </summary>
    public InventoryItem GetItemBySlotIndex(int index, ItemType type)
    {
        if (inventoryDict.TryGetValue(type, out var list) && index >= 0 && index < list.Count)
            return list[index];

        return null;
    }
    
    /// <summary>
    /// 해당 수량을 추가할 경우, 인벤토리 최대치를 초과하는지 확인
    /// </summary>
    public bool CanAddItems(int itemCountToAdd)
    {
        int weaponCount = GetItemCount(ItemType.Weapon) + itemCountToAdd;
        int armorCount = GetItemCount(ItemType.Armor) + itemCountToAdd;

        if (weaponCount > MaxItemCount || armorCount > MaxItemCount)
        {
            MyDebug.LogWarning($"둘 중 하나 탭 가득 찰 것 같음.. sumArmorCount: {armorCount}, sumWeaponCount: {weaponCount}");
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// 특정 ItemType에 해당하는 내부 수정 가능한 인벤토리 리스트를 반환함
    /// GC 없이 기존 리스트 참조를 유지하고 싶은 경우 사용
    /// 실패 시 list는 null
    /// </summary>
    public bool TryGetMutableItemList(ItemType type, out List<InventoryItem> list)
    {
        return inventoryDict.TryGetValue(type, out list);
    }
    
    /// <summary>
    /// 읽기 전용 뷰 딕셔너리 갱신 (정렬 이후 필수)
    /// </summary>
    public void UpdateReadOnlyView(ItemType type, List<InventoryItem> updatedList)
    {
        inventoryReadOnlyDict[type] = updatedList;
    }
    
    /// <summary>
    /// itemUid를 기준으로 인벤토리 아이템을 조회하는 함수
    /// 분해, 장착 등 특정 아이템을 직접 접근해야 할 때 사용
    /// </summary>
    public InventoryItem GetItemByUid(string itemUid)
    {
        return itemUidToItemDic.GetValueOrDefault(itemUid);
    }
    
    /// <summary>
    /// 특정 아이템이 어떤 유닛에 장착되었는지 매핑 정보를 갱신함
    /// unit이 null이면 장착 해제로 간주하고 매핑에서 제거
    /// </summary>
    public void UpdateItemToUnitMapping(string itemUid, InventoryUnit unit)
    {
        if (unit == null)
            itemUidToUnitDic.Remove(itemUid);
        else
            itemUidToUnitDic[itemUid] = unit;
    }

    /// <summary>
    /// 특정 itemUid에 장착된 유닛 정보를 반환함
    /// 장착된 상태가 아닐 경우 null 반환
    /// </summary>
    public InventoryUnit GetUnitByItemUid(string itemUid)
    {
        return itemUidToUnitDic.GetValueOrDefault(itemUid);
    }
    
    /// <summary>
    ///  특정 itemUid의 인벤토리 index 반환
    /// </summary>
    public int GetItemIndexByItemUid(ItemType type, string itemUid)
    {
        if(!inventoryDict.TryGetValue(type, out var list))
        {
            MyDebug.Log($"해당 {itemUid} 는 인벤토리에 없음");
            return -1;
        }
        
        InventoryItem item = ItemUidToItemDic[itemUid];
        return list.IndexOf(item);
    }
    
}