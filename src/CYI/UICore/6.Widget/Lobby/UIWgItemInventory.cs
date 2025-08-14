using UnityEngine;

/// <summary>
/// UIWidgetItem을 인벤토리 전용으로 재정의
/// </summary>
public class UIWgItemInventory : UIWgItem
{
    [SerializeField] private UIWgItemBsState itemBsState;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        itemBsState = new(transform);
    }

    /// <summary>
    /// UI 표시 로직 + 재련 상태 UI 설정
    /// </summary>
    public override void ShowInventory(InventoryItem inventoryItem, int index = -1)
    {
        base.ShowInventory(inventoryItem, index);
        if (inventoryItem == null) return;

        itemBsState.ResetUI();
        // 장착한 유닛이 있다면, 유닛 얼굴 표시
        Sprite unitIcon = inventoryItem.GetUnitEquippedUnitIcon();
        itemBsState.ShowUnitIcon(unitIcon);
        // 강화 수치 설정
        itemBsState.ShowEnhancement(inventoryItem.EnhancementLevel);
        // 돌파 수치 설정
        itemBsState.ShowLimitBreak(inventoryItem.LimitBreakLevel);
    }
}
