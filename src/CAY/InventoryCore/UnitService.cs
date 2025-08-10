using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 유닛 관련 로직을 담당하는 서비스 클래스
/// - 유닛 생성
/// - 장비 연결 캐시 갱신
/// </summary>
public class UnitService
{
    private readonly InventoryCache cache;

    public UnitService(InventoryCache cache)
    {
        this.cache = cache;
    }
    
    /// <summary>
    /// UserData.inventory.Units를 초기화하는 메서드
    /// 스탯 바인딩 및 itemUid → unit 캐시 등록
    /// </summary>
    public void InitializeUnits()
    {
        // 유닛 캐시 초기화
        foreach (var unit in UserData.inventory.Units)
        {
            unit.BindingOriginStatSum();
            unit.BindingItemSumStats();

            foreach (var uid in unit.equippedItemUids)
            {
                cache.UpdateItemToUnitMapping(uid, unit);
            }
        }
    }
    
    /// <summary>
    /// 유닛 추가 (Firestore 업로드 포함)
    /// </summary>
    public async Task TryAddUnitAsync(string unitCode)
    {
        InventoryUnit unit = new InventoryUnit
        {
            unitCode = unitCode
        };

        string uid = FirebaseManager.Instance.DbUser.UserId;

        await FirestoreUploader.SaveInventoryUnit(uid, unit);

        UserData.inventory.AddUnit(unit);

        // 초기 바인딩도 여기서 할 수 있음
        unit.BindingOriginStatSum();
        unit.BindingItemSumStats();

        // 장착된 아이템 UID → 유닛 바인딩
        foreach (var itemUid in unit.equippedItemUids)
        {
            cache.UpdateItemToUnit(itemUid, unit);
        }
    }

    /// <summary>
    /// 아이템 UID에 유닛 바인딩 (장비 장착 시 사용)
    /// </summary>
    public void BindItemToUnit(string itemUid, InventoryUnit unit)
    {
        cache.UpdateItemToUnit(itemUid, unit);
    }
}