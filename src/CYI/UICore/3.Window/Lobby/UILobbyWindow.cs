using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lobby Scene의 Main Hub 스크립트
/// </summary>
public class UILobbyWindow : UIBase
{
    protected override ContentType ContentType => ContentType.Lobby;
    [Header("====[Side Buttons]")] 
    [SerializeField] private GUIContentTitle guiContentTitle;
    [SerializeField] private Button btnOption;
    [SerializeField] private GameObject bsNpc;
    [SerializeField] private GameObject gcNpc;
    [SerializeField] private GameObject stNpc;
    
    [Header("====[Contents Buttons]")]
    [SerializeField] private Button btnGacha;
    [SerializeField] private Button btnStage;
    [SerializeField] private Button btnBlacksmith;

    [Header("====[Side Buttons]")] 
    [SerializeField] private Button btnUnitInventory;
    [SerializeField] private Button btnItemInventory;
    [SerializeField] private Button btnCollection;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        
        guiContentTitle = transform.FindChildByName<GUIContentTitle>("Group_ContentTitle");
        
        btnGacha = transform.FindChildByName<Button>("Btn_Gacha");
        btnStage = transform.FindChildByName<Button>("Btn_Stage");
        btnBlacksmith = transform.FindChildByName<Button>("Btn_Blacksmith");
        
        btnUnitInventory = transform.FindChildByName<Button>("Btn_UnitInventory");
        btnItemInventory = transform.FindChildByName<Button>("Btn_ItemInventory");
        btnCollection = transform.FindChildByName<Button>("Btn_Collection");
        
        btnOption = transform.FindChildByName<Button>("Btn_Option");
        
        bsNpc = transform.FindChildByName<Transform>("NPC_0001").gameObject;
        gcNpc = transform.FindChildByName<Transform>("NPC_0002").gameObject;
        stNpc = transform.FindChildByName<Transform>("NPC_0003").gameObject;
    }
    
    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>

    public override void Initialize()
    {
        base.Initialize();
        
        btnOption.onClick.RemoveAllListeners();
        btnOption.AddListener(OnOption);
                
        btnGacha.onClick.RemoveAllListeners();
        btnStage.onClick.RemoveAllListeners();
        btnBlacksmith.onClick.RemoveAllListeners();
        btnGacha.AddListener(OnGacha);
        btnStage.AddListener(OnStage);
        btnBlacksmith.AddListener(OnBlacksmith);
        
        btnUnitInventory.onClick.RemoveAllListeners();
        btnUnitInventory.AddListener(OnUnitInventory);
        btnItemInventory.onClick.RemoveAllListeners();
        btnItemInventory.AddListener(OnItemInventory);
        btnCollection.onClick.RemoveAllListeners();
        btnCollection.AddListener(OnCollection);
        
        guiContentTitle.Initialize();

        ResetNonUI();
    }

    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        guiContentTitle.SetTitle();
    }
    
    /// <summary>
    /// NonUI 초기 상태 설정: 비활성화
    /// </summary>
    private void ResetNonUI()
    {
        bsNpc.SetActive(false);
        gcNpc.SetActive(false);
        stNpc.SetActive(false);
    }    
    
    /// <summary>
    /// UI 열기: 사운드 재생, 배경 설정, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    public override void Open(OpenContext openContext = null)
    {
        SoundManager.Instance.PlayBgm(StringAdrAudioBgm.LobbyScene);
        UIManager.Instance.ChangeBg(StringAdrBg.Lobby);
        UIManager.Instance.Open<UIWcUserInfo>();
        AnalyticsHelper.LogScreenView(AnalyticsMainScreen.Main, this.GetType().Name);

        bsNpc.SetActive(true);
        gcNpc.SetActive(true);
        stNpc.SetActive(true);
        ResetUI();
        base.Open(openContext);
    }

    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화
    /// </summary>
    public override void Close(CloseContext closeContext)
    {
        base.Close(closeContext);
        UIManager.Instance.Close<UIWcUserInfo>();
        ResetNonUI();
    }
    
    /// <summary>
    /// 콘텐츠 버튼 클릭을 통한 Lobby Hub내 UI 콘텐츠 열기
    /// </summary>
    public void OpenContents(ContentType contentType)
    {
        Close();
        switch (contentType)
        {
            case ContentType.Gacha:
                UIManager.Instance.Open<UIGachaWindow>();
                break;
            case ContentType.Blacksmith:
                UIManager.Instance.Open<UIBlacksmithWindow>();
                break;
            case ContentType.Battle:
                _ = UIManager.Instance.EnterLoadingAsync(SceneType.Battle);
                break;
            default:
                MyDebug.LogError($"Is Not ContentType about Lobby => {contentType}");
                return;
        }
    }

    /// <summary>
    /// 로비의 옵션 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 로비 옵션 Open
    /// </summary>
    private void OnOption()
    {
        Close();
        UIManager.Instance.Open<UIPLobbyOption>();
    }
    /// <summary>
    /// 가챠 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 가챠 Open
    /// </summary>
    private void OnGacha()
    {
        OpenContents(ContentType.Gacha);
    }
    /// <summary>
    /// 스테이지 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 배틀 Open
    /// </summary>
    private void OnStage()
    {
        OpenContents(ContentType.Battle);
    }
    /// <summary>
    /// 대장장이 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 대장장이 Open
    /// </summary>
    private void OnBlacksmith()
    {
        OpenContents(ContentType.Blacksmith);
    }
    /// <summary>
    /// 유닛 인벤토리 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 유닛 인벤토리 Open
    /// </summary>
    private void OnUnitInventory()
    {
        Close();
        UnitInvenOpenContext context = new UnitInvenOpenContext { UnitIndex = 0 };
        UIManager.Instance.Open<UIUnitInventoryWindow>(OpenContext.WithContext(context));
    }
    /// <summary>
    /// 아이템 인벤토리 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 아이템 인벤토리 Open
    /// </summary>
    private void OnItemInventory()
    {
        Close();
        UIManager.Instance.Open<UIItemInventoryWindow>();
    }
    /// <summary>
    /// 도감 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 도감 Open
    /// </summary>
    private void OnCollection()
    {
        Close();
        UIManager.Instance.Open<UICollectionWindow>();
    }
}
