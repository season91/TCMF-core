using System;
using UnityEngine;

/// <summary>
/// 현재 유닛 인벤토리의 타입
/// </summary>
public enum UnitInventoryType
{
    Popup,
    Quick
}

/// <summary>
/// 유닛 인벤토리의 유닛 리스트 위젯
/// </summary>
public class UIWcUnitInventory : MonoBehaviour
{
    [Header("====[Unit Inventory]")]
    private UIDynamicObjectPool<UIWgUnitSlot> dynamicUnitPool;
    [SerializeField] private Transform originRoot;
    [SerializeField] private UIWgUnitSlot originUnitSlot;
    
    private UnitInventoryType curUnitInventoryType = UnitInventoryType.Popup;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        originRoot = transform.FindChildByName<Transform>("Layout_Units");
        originUnitSlot = transform.FindChildByName<UIWgUnitSlot>("GUI_UnitSlot");
    }
    
    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize(UnitInventoryType inventoryType)
    {
        curUnitInventoryType = inventoryType;
        dynamicUnitPool = new UIDynamicObjectPool<UIWgUnitSlot>(originUnitSlot, originRoot, 3);
    }

    /// <summary>
    /// UI 표시: 사운드 재생, 배경 설정, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    public void Show(Action<int> onSelected)
    {
        var inventoryUnitList = UserData.inventory.Units;
        var unitSlotList = dynamicUnitPool.GetActiveList();
        
        bool isReset = unitSlotList.Count != inventoryUnitList.Count;
        if (isReset)
        {
            dynamicUnitPool.OffAll();
        }
        
        for (int i = 0; i < inventoryUnitList.Count; i++)
        {
            var slot = isReset ? dynamicUnitPool.Get() : unitSlotList[i];
            var unit = inventoryUnitList[i];
            bool isSelected = curUnitInventoryType == UnitInventoryType.Quick &&
                              StageManager.Instance.IsSelectedUnit(unit);
        
            slot.Initialize();
            slot.Show(i, isSelected, onSelected);
        }
        
        AnalyticsHelper.LogScreenView(AnalyticsMainScreen.UnitInventory, GetType().Name);
    }
    
    /// <summary>
    /// 현재 Unit Index에 해당하는 Unit Slot의 Selected UI 설정
    /// </summary>
    public void SetSelectedUI(int unitIndex)
    {
        var unitSlotList = dynamicUnitPool.GetActiveList();
        if (unitIndex >= 0 && unitIndex < unitSlotList.Count)
            unitSlotList[unitIndex].SetSelectedUI();
        else
            MyDebug.LogWarning("Out Of Range => UnitSlotList");
    }
}
