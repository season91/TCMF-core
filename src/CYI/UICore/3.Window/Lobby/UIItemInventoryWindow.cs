using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item Inventory의 Hub 스크립트
/// </summary>
public class UIItemInventoryWindow : UIBase
{
    protected override ContentType ContentType => ContentType.ItemInventory;
    
    [Header("====[아이템 인벤토리 박스]")]
    [SerializeField] private UIWcItemBox uiItemBox;
    
    [Header("====[아이템 정보 박스]")]
    [SerializeField] private UIBaseWcItemInfo uiBaseWcInfoBox;
    
    [Header("====[기본 구성]")]
    [SerializeField] private GUIContentTitle guiContentTitle;
    [SerializeField] private Button btnClose;
    // [SerializeField] private Button btnFilter;
    // [SerializeField] private Button btnSort;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        
        guiContentTitle = transform.FindChildByName<GUIContentTitle>("Group_ContentTitle");
        
        uiItemBox = GetComponentInChildren<UIWcItemBox>();
        uiBaseWcInfoBox = GetComponentInChildren<UIBaseWcItemInfo>();

        // btnFilter = transform.FindChildByName<Button>("Btn_Filter");
        // btnSort = transform.FindChildByName<Button>("Btn_Sort");
        btnClose = transform.FindChildByName<Button>("Btn_Close");
    }
        
    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        guiContentTitle.Initialize();
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(Close);
        // btnFilter.onClick.RemoveAllListeners();
        // btnFilter.AddListener(OnFilter);
        // btnSort.onClick.RemoveAllListeners();
        // btnSort.AddListener(OnSort);
        
        uiItemBox.Initialize();
        uiBaseWcInfoBox.Initialize();
    }
    
    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        guiContentTitle.SetTitle();
        uiItemBox.ShowItemBox<InventoryItem>(ShowItemInfo, false, uiBaseWcInfoBox.Hide);
    }
    
    /// <summary>
    /// UI 열기: 사운드 재생, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    public override void Open(OpenContext openContext = null)
    {
        SoundManager.Instance.PlaySfx(StringAdrAudioSfx.Open);
        UIManager.Instance.Open<UIWcUserInfo>();
        AnalyticsHelper.LogScreenView(AnalyticsMainScreen.Inventory, this.GetType().Name);
        ResetUI();
        base.Open(openContext);
    }
    
    /// <summary>
    /// UI 닫기: 부가 UI Close, Main Hub Open
    /// </summary>
    public override void Close(CloseContext closeContext)
    {
        base.Close(closeContext);
        UIManager.Instance.Close<UIWcUserInfo>();
        UIManager.Instance.Open<UILobbyWindow>();
    }

    /// <summary>
    /// UI Item Info 표시
    /// </summary>
    private void ShowItemInfo(InventoryItem item) => uiBaseWcInfoBox.ShowInfoByInventroy(item);
}
