#nullable enable
using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 전투에 배치될 선택 유닛 3명을 관리하는 클래스
/// </summary>
public class SelectedUnitManager
{
    private readonly InventoryUnit?[] selectedUnits = new InventoryUnit?[3];

    /// <summary>
    /// Catsite 개수
    /// </summary>
    public int UnitCount => selectedUnits.Length;

    /// <summary>
    ///  출정 준비 완료된 Unit 개수
    /// </summary>
    public int GetReadyUnitCount()
    {
        int count = 0;
        foreach (var unit in selectedUnits)
        {
            if (unit != null)
            {
                count++;
            }
        }

        return count;
    }
    
    /// <summary>
    /// 선택된 유닛 설정
    /// </summary>
    public void SetUnit(InventoryUnit unit, int index)
    {
        if (index >= 0 && index < 3) 
        {
            selectedUnits[index] = unit;
            return;
        }
        
        MyDebug.LogError($"Invalid index: {index}");
    }

    /// <summary>
    /// 선택된 유닛 반환
    /// </summary>
    public InventoryUnit? GetUnit(int index)
    {
        if (index >= 0 && index < 3) 
            return selectedUnits[index];
        
        MyDebug.LogError($"Invalid index: {index}");
        return null;
    }
    
    /// <summary>
    /// 특정 유닛이 선택되었는지 확인
    /// </summary>
    public bool IsSelected(InventoryUnit unit)
    {
        foreach (var inventoryUnit in selectedUnits)
        {
            if (inventoryUnit?.UnitUid == unit.UnitUid)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 해당 인덱스에 유닛이 존재하는지 확인
    /// </summary>
    public bool HasUnit(int index) => index >= 0 && index < 3 && selectedUnits[index] != null;

    /// <summary>
    /// 주어진 유닛이 어느 Catsite(index)에 있는지 반환
    /// </summary>
    public int GetCatsiteIndexByUnitIndex(int unitIndex)
    {
        var curUnitUid = UserData.inventory.Units[unitIndex].UnitUid;
        for (int i = 0; i < selectedUnits.Length; i++)
        {
            if (selectedUnits[i]?.UnitUid == curUnitUid)
                return i;
        }
        return -1; // 없음
    }
    
    /// <summary>
    /// 유닛 제거
    /// </summary>
    public void Clear(int index)
    {
        if (index >= 0 && index < 3) 
            selectedUnits[index] = null;
    }

    /// <summary>
    /// 전체 초기화
    /// </summary>
    public void ClearAll()
    {
        for (int i = 0; i < selectedUnits.Length; i++)
            selectedUnits[i] = null;
    }
}