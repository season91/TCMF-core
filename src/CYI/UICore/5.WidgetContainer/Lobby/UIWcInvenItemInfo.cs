using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIBaseWcItemInfo를 Inventory에 맞게 재정의 
/// </summary>
public class UIWcInvenItemInfo : UIBaseWcItemInfo
{
    [Header("====[강화/돌파 정보]")]
    [SerializeField] private UIWgItemBsState itemBsState;
    
    [Header("====[대장장이 버튼]")]
    [SerializeField] private Button btnEnhancement;
    [SerializeField] private Button btnDismantle;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        itemBsState = new(transform);
        
        btnEnhancement = transform.FindChildByName<Button>("Btn_Enhancement");
        btnDismantle = transform.FindChildByName<Button>("Btn_Dismantle");
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        btnEnhancement.onClick.RemoveAllListeners();
        btnEnhancement.AddListener(OnEnhancement);
        btnDismantle.onClick.RemoveAllListeners();
        btnDismantle.AddListener(OnDismantle);
    }

    /// <summary>
    /// 아이템 데이터에 따른 정보 표시 + 대장장이 재련 정보 표시
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
    /// 강화 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// 대장장이 - 강화로 이동
    /// </summary>
    private void OnEnhancement()
    {
        // 대장장이의 강화탭으로 이동
        UIManager.Instance.Close<UIItemInventoryWindow>();
        UIManager.Instance.GetUI<UILobbyWindow>().OpenContents(ContentType.Blacksmith);
        UIManager.Instance.GetUI<UIBlacksmithWindow>().OpenContents(ContentType.Enhancement);
    }
    
    /// <summary>
    /// 분해 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// 대장장이 - 분해 이동
    /// </summary>
    private void OnDismantle()
    {
        // 대장장이의 분해 탭으로 이동
        UIManager.Instance.Close<UIItemInventoryWindow>();
        UIManager.Instance.GetUI<UILobbyWindow>().OpenContents(ContentType.Blacksmith);
        UIManager.Instance.GetUI<UIBlacksmithWindow>().OpenContents(ContentType.Dismantle);
    }
}
