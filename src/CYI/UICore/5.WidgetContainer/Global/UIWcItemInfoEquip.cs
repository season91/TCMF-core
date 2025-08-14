using UnityEngine;

/// <summary>
/// UIBaseWcItemInfo를 장착한 아이템 정보에 맞게 재정의 
/// </summary>
public class UIWcItemInfoEquip : UIBaseWcItemInfo
{
    [SerializeField] private UIWgItemBsState itemBsState;
    
    public RectTransform rectTr;
    public CanvasGroup cg;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        itemBsState = new UIWgItemBsState(transform);
        
        rectTr = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// 아이템 데이터에 따른 정보 표시 + 재련 정보 표시
    /// </summary>
    public override void ShowInfoByInventroy(InventoryItem inventoryItem)
    {
        base.ShowInfoByInventroy(inventoryItem);
        
        itemBsState.ResetUI();
        itemBsState.ShowUnitIcon(inventoryItem.GetUnitEquippedUnitIcon());
        itemBsState.ShowEnhancement(inventoryItem.EnhancementLevel);
        itemBsState.ShowLimitBreak(inventoryItem.LimitBreakLevel);
    }

    /// <summary>
    /// UI 열기: Fade In, 인벤토리 아이템으로 정보 표시
    /// </summary>
    public void Open(InventoryItem item)
    {
        cg.FadeAnimation(1);
        ShowInfoByInventroy(item);
    }

    /// <summary>
    /// 다른 아이템으로 UI 교체(갱신)
    /// </summary>
    public void Replace(InventoryItem item)
    {
        rectTr.BounceAnimation();
        ShowInfoByInventroy(item);
    } 
    
    /// <summary>
    /// UI 닫기: Fade Out
    /// </summary>
    public void Close() => cg.FadeAnimation(0);
} 
