using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// UI 요청에 따른 장착 처리기
/// </summary>
public class EquipmentInteractor
{
    private InventoryItem equipItem;
    private InventoryItem unequipItem;
    private InventoryUnit curUnit;
    private ItemType curItemType;
    private Action completeAction;
    private readonly InventoryCache cache;

    public EquipmentInteractor(InventoryCache cache)
    {
        this.cache = cache;
    }

    /// <summary>
    /// 특정 슬롯의 아이템 장착 시도, UI에서 호출
    /// </summary>
    public async Task TryEquip(int slotIndex, InventoryUnit unit, ItemType itemType, Action onCompleted = null)
    {
        this.equipItem = cache.GetItemBySlotIndex(slotIndex, itemType);
        if (equipItem == null)
        {
            MyDebug.LogWarning($"해당 슬롯 {slotIndex}:{itemType} 아이템 존재하지 않음");
            return;
        }
        this.curUnit = unit;
        curItemType = itemType;
        completeAction = onCompleted;
        
        // 얘가 현재 누가 장착하고 있음? 판단 여부 필요
        if (equipItem.IsEquipped)
        {
            TwoButtonOpenContext context = new TwoButtonOpenContext
            {
                Title = "안내",
                Comment = "다른 고양이가 장착 중 입니다.\n해제 후 장착 하시겠어요?",
                Button1Text = "네",
                Button2Text = "아니오",
                Button1Event = null,
                Button2Event = null
            } ;
            context.Button1Event += OnUnEquipAndEquip;
            UIManager.Instance.Open<UITwoButtonPopup>(OpenContext.WithContext(context));
        }
        else
        {
            await EquipItemToUnitAsync();
        }

        SoundManager.Instance.PlaySfx(StringAdrAudioSfx.Equipment);
    }
    
    /// <summary>
    /// 특정 슬롯의 아이템 장착 해제 시도, UI에서 호출
    /// </summary>
    public async Task TryUnEquipAsync(int slotIndex, ItemType itemType)
    {
        this.equipItem = cache.GetItemBySlotIndex(slotIndex, itemType);
        if (equipItem == null)
        {
            MyDebug.LogWarning($"해당 슬롯 {slotIndex}:{itemType} 아이템 존재하지 않음");
            return;
        }
        this.curUnit = null;
        if (equipItem != null )
        {
            this.unequipItem = equipItem;
            await UnEquipItemAsync();
        }
    }

    /// <summary>
    /// 장착 해제 후 장착
    /// </summary>
    private async Task UnEquipAndEquipAsync()
    {
        // 다른 고양이가 끼고 있다면 제거 -> UI에서 기준 판단?
        if (!string.IsNullOrEmpty(equipItem.EquippedUnitUid))
        {
            this.unequipItem = equipItem;
            await UnEquipItemAsync();
        }

        await EquipItemToUnitAsync();
        
    }
    
    /// <summary>
    /// 특정 유닛에 아이템 장착
    /// </summary>
    private async Task EquipItemToUnitAsync()
    {
        // 유닛이 장착한 아이템 목록을 순회하며, 동일 타입의 기존 아이템 해제
        foreach (var equippedUid in curUnit.equippedItemUids.ToList()) // ToList()로 복사 후 안전한 반복
        {
            // 캐시에서 UID로 아이템 조회
            var equippedItem = cache.GetItemByUid(equippedUid);

            if (equippedItem == null)
            {
                MyDebug.LogWarning($"장착 목록에 존재하지만 캐시에 없는 아이템: {equippedUid}");
                continue;
            }

            // 타입이 같고, 다른 아이템이면 해제
            bool isSameType = equippedItem.ItemType == curItemType;
            bool isDifferentItem = equippedItem.ItemUid != equipItem.ItemUid;

            if (isSameType && isDifferentItem)
            {
                this.unequipItem = equippedItem;
                await UnEquipItemAsync();
            }
        }
        
        // 새 유닛에 장착
        equipItem.equippedUnitUid = curUnit.UnitUid;
        
        if (!curUnit.equippedItemUids.Contains(equipItem.ItemUid))
        {
            curUnit.equippedItemUids.Add(equipItem.ItemUid);
        }
        
        // 매핑 갱신
        cache.UpdateItemToUnitMapping(equipItem.ItemUid, curUnit);

        MyDebug.Log($"ItemCode: {equipItem.ItemUid}, 장착 유닛: {curUnit.UnitUid}");
        
        string uid = FirebaseManager.Instance.DbUser.UserId;
        await FirestoreUploader.SaveInventoryItemAsync(uid, equipItem);
        await FirestoreUploader.SaveInventoryUnitAsync(uid, curUnit);
        
        completeAction?.Invoke();
    }

    /// <summary>
    /// 장착 해제 처리
    /// </summary>
    private async Task UnEquipItemAsync()
    {
        string itemUid = unequipItem.ItemUid;
        string uid = FirebaseManager.Instance.DbUser.UserId;
        
        // 아이템에 장착된 유닛 조회
        var unit = cache.GetUnitByItemUid(itemUid);
        if (unit != null && unit.equippedItemUids.Remove(itemUid))
        {
            await FirestoreUploader.SaveInventoryUnitAsync(uid, unit);
            MyDebug.Log($"[UnEquip] {itemUid} → 유닛 {unit.UnitUid}에서 제거 완료");
        }

        // 슬롯 아이템 정보 초기화
        unequipItem.equippedUnitUid = null;

        // 매핑 갱신
        cache.UpdateItemToUnitMapping(itemUid, null);
        
        await FirestoreUploader.SaveInventoryItemAsync(uid, unequipItem);
        MyDebug.Log($"[UnEquip] {itemUid} 장착 해제 저장 완료");
    }
    
    /// <summary>
    /// 장착 해제 이벤트 등록할 함수
    /// </summary>
    private async void OnUnEquipAndEquip()
    {
        try
        {
            await UnEquipAndEquipAsync();
        }
        catch (Exception ex)
        {
            MyDebug.LogError($"UnEquipAndEquipAsync() 예외 발생: {ex}");
        }
    }
    
    /// <summary>
    /// 본인이 장착하고 있는 아이템
    /// 인벤토리 아이템 정보
    /// </summary>
    public InventoryItem GetEquippedItemByType(ItemType itemType, int unitIndex)
    {
        // 선택된 유닛을 가져옴
        InventoryUnit selectUnit = UserData.inventory.Units[unitIndex];

        // 장착한 아이템들 중에서 요청한 타입과 일치하는 것 반환
        foreach (var equippedItem in selectUnit.EquippedItems)
        {
            if (equippedItem.ItemType == itemType)
            {
                return equippedItem;
            }
        }

        // 해당 타입의 아이템을 찾지 못한 경우 null 반환
        return null;
    }
}