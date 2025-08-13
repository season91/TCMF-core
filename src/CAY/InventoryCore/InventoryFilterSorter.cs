using System.Linq;
/// <summary>
/// 아이템 정렬 클래스
/// </summary>
public class InventoryFilterSorter
{
    private readonly InventoryCache cache;

    public InventoryFilterSorter(InventoryCache cache)
    {
        this.cache = cache;
    }
    /// <summary>
    /// 유저 인벤토리 전체를 itemType별로 정리하고 획득 시간순으로 정렬
    /// </summary>
    public void SortInventoryByObtainedTimeDesc(ItemType type)
    {
        if (!cache.TryGetMutableItemList(type, out var list))
            return;

        list.Sort((a, b) =>
        {
            // 장착 여부 우선 정렬
            bool aEquipped = !string.IsNullOrEmpty(a.EquippedUnitUid);
            bool bEquipped = !string.IsNullOrEmpty(b.EquippedUnitUid);

            if (aEquipped != bEquipped)
                return bEquipped.CompareTo(aEquipped); // true가 먼저 오도록

            // 그 외에는 획득시간 내림차순 정렬
            return b.ObtainedAt.CompareTo(a.ObtainedAt);
        });
        
        // 읽기 전용 딕셔너리도 다시 연결해줘야 UI 반영됨
        cache.UpdateReadOnlyView(type, list);
    }

    /// <summary>
    /// 무기 아이템을 공격력 내림차순으로 정렬 (동일 공격력일 경우 등급 높은 순)
    /// 내부 리스트 정렬 후 읽기 전용 뷰도 갱신 필요
    /// </summary>
    public void SortInventoryByAtkDesc()
    {
        if (!cache.TryGetMutableItemList(ItemType.Weapon, out var list))
            return;

        list.Sort((a, b) =>
        {
            int atkCompare = b.SumAtk.CompareTo(a.SumAtk); // 공격력 내림차순
            if (atkCompare != 0)
                return atkCompare;

            return b.Rarity.CompareTo(a.Rarity); // 동일 공격력일 경우 등급 높은 순
        });

        cache.UpdateReadOnlyView(ItemType.Weapon, list);
    }
    
    /// <summary>
    /// 무기 아이템을 공격력 오름차순으로 정렬 (동일 공격력일 경우 등급 높은 순)
    /// 내부 리스트 정렬 후 읽기 전용 뷰도 갱신 필요
    /// </summary>
    public void SortInventoryByAtkAsc()
    {
        if (!cache.TryGetMutableItemList(ItemType.Weapon, out var list))
            return;

        list.Sort((a, b) =>
        {
            int atkCompare = a.SumAtk.CompareTo(b.SumAtk); // 공격력 오름차순
            if (atkCompare != 0)
                return atkCompare;

            return b.Rarity.CompareTo(a.Rarity); // 동일 공격력일 경우 등급 높은 순
        });

        cache.UpdateReadOnlyView(ItemType.Weapon, list);
    }

    /// <summary>
    /// 방어구 아이템을 방어력 내림차순으로 정렬 (동일 방어력일 경우 등급 높은 순)
    /// 내부 리스트 정렬 후 읽기 전용 뷰도 갱신 필요
    /// </summary>
    public void SortInventoryByDefDesc()
    {
        if (!cache.TryGetMutableItemList(ItemType.Armor, out var list))
            return;

        list.Sort((a, b) =>
        {
            int defCompare = b.SumDef.CompareTo(a.SumDef); // 방어력 내림차순
            if (defCompare != 0)
                return defCompare;

            return b.Rarity.CompareTo(a.Rarity); // 동일 방어력일 경우 등급 높은 순
        });

        cache.UpdateReadOnlyView(ItemType.Armor, list);
    }

    /// <summary>
    /// 방어구 아이템을 방어력 오름차순으로 정렬 (동일 방어력일 경우 등급 높은 순)
    /// 내부 리스트 정렬 후 읽기 전용 뷰도 갱신 필요
    /// </summary>
    public void SortInventoryByDefAsc()
    {
        if (!cache.TryGetMutableItemList(ItemType.Armor, out var list))
            return;

        list.Sort((a, b) =>
        {
            int defCompare = a.SumDef.CompareTo(b.SumDef); // 방어력 오름차순
            if (defCompare != 0)
                return defCompare;

            return b.Rarity.CompareTo(a.Rarity); // 동일 방어력일 경우 등급 높은 순
        });

        cache.UpdateReadOnlyView(ItemType.Armor, list);
    }
    
    /// <summary>
    /// 인벤토리 유닛을 UnitCode 기준 오름차순 정렬
    /// </summary>
    public void SortInventoryUnitsByCodeAsc()
    {
        if (UserData.inventory.Units == null || UserData.inventory.Units.Count == 0)
            return;

        UserData.inventory.units = UserData.inventory.Units
                                           .OrderBy(unit => unit.UnitCode)
                                           .ToList();
    }
}