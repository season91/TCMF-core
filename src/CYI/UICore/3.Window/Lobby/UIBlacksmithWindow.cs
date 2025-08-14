using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대장장이에서의 현재 열린 UI 상태
/// </summary>
public enum BsOpenState
{
    Main,
    Contents,
    Process
}

/// <summary>
/// 대장장이의 Hub 스크립트
/// </summary>
public class UIBlacksmithWindow : UIBase
{
    protected override ContentType ContentType => curBsContentType;
    private ContentType curBsContentType;
    private BsOpenState curState;
    
    [Header("====[기본 구성]")]
    [SerializeField] private GUIContentTitle guiContentTitle;
    [SerializeField] private Button btnClose;
    [SerializeField] private GameObject bsNpc;
    
    [Header("====[대장장이 버튼]")]
    [SerializeField] private GameObject objBsBtns;
    [SerializeField] private Button btnEnhancement;
    [SerializeField] private Button btnLimitBreak;
    [SerializeField] private Button btnDismantle;
    
    [Header("====[강화/돌파]")]
    [SerializeField] private RectTransform uiItemBoxRectTr;
    [SerializeField] private UIWcBsItemBox uiItemBox;
    [SerializeField] private UIWcBsItemInfo uiItemInfo;
    private const float DefaultItemBoxHeight = 1100;
    private const float DismantleItemBoxHeight = 920;
    
    [Header("====[분해]")]
    [SerializeField] private UIWcBsDmResultBox uiWcDmResultBox;
    
    // =======================================================================
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();

        guiContentTitle = transform.FindChildByName<GUIContentTitle>("Group_ContentTitle");
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        
        objBsBtns = transform.FindChildByName<Transform>("Layout_BlacksmithButtons").gameObject;
        btnEnhancement = transform.FindChildByName<Button>("Btn_Enhancement");
        btnLimitBreak = transform.FindChildByName<Button>("Btn_LimitBreak");
        btnDismantle = transform.FindChildByName<Button>("Btn_Dismantle");

        uiItemBoxRectTr = transform.FindChildByName<RectTransform>("Group_InventoryBox");
        uiItemBox = uiItemBoxRectTr.GetComponent<UIWcBsItemBox>();
        uiItemInfo = transform.FindChildByName<UIWcBsItemInfo>("Group_InfoBox");
        uiWcDmResultBox = transform.FindChildByName<UIWcBsDmResultBox>("Group_ResultBox");
        
        bsNpc = transform.FindChildByName<Transform>("NPC_0001").gameObject;
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(OnClose);
        
        btnEnhancement.onClick.RemoveAllListeners();
        btnLimitBreak.onClick.RemoveAllListeners();
        btnDismantle.onClick.RemoveAllListeners();
        btnEnhancement.AddListener(OnEnhancement);
        btnLimitBreak.AddListener(OnLimitBreak);
        btnDismantle.AddListener(OnDismantle);
        
        guiContentTitle.Initialize();
        uiItemBox.Initialize();
        uiItemInfo.Initialize();
        uiWcDmResultBox.Initialize();
        
        bsNpc.SetActive(false);
    }

    /// <summary>
    /// Canvas의 Alpha와 인터랙션만 조절하기 위한 편의용
    /// </summary>
    public void BaseOpen() => base.Open();

    /// <summary>
    /// Canvas의 Alpha와 인터랙션만 조절하기 위한 편의용
    /// </summary>
    public void BaseClose()
    {
        canvasGroup.SetInteractable(false);
        canvasGroup.FadeAnimation(0);
    }
    
    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        curBsContentType = ContentType.Blacksmith;
        curState = BsOpenState.Main;
        UIManager.Instance.SetContentType(ContentType);
        
        guiContentTitle.SetTitle();
        bsNpc.SetActive(true);
        
        objBsBtns.SetActive(true);
        uiItemBox.Hide();
        uiItemInfo.Close();
        uiWcDmResultBox.Close();
    }

    /// <summary>
    /// NonUI 초기 상태 설정: 비활성화
    /// </summary>
    private void ResetNonUI()
    {
        bsNpc.SetActive(false);
    }
    
    /// <summary>
    /// UI 열기: BGM 재생, 배경 설정, 필요한 부가 UI Open, GA, 초기 상태 복원
    /// </summary>
    public override void Open(OpenContext openContext = null)
    {
        SoundManager.Instance.PlayBgm(StringAdrAudioBgm.BlackSmith);
        UIManager.Instance.ChangeBg(StringAdrBg.Blacksmith);
        UIManager.Instance.Open<UIWcUserInfo>();
        AnalyticsHelper.LogScreenView(AnalyticsBSScreen.BSMain, this.GetType().Name);

        ResetUI();
        base.Open(openContext);
    }

    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화
    /// </summary>
    public override void Close(CloseContext closeContext)
    {
        ResetNonUI();
        
        UIManager.Instance.Close<UIWcUserInfo>();
        base.Close(closeContext);
    }
    
    /// <summary>
    /// 콘텐츠 버튼 클릭을 통한 대장장이 Hub내 UI 콘텐츠 열기
    /// </summary>
    public void OpenContents(ContentType type)
    {
        uiItemInfo.Close();
        
        if (type == ContentType.Dismantle)
        {
            uiItemBoxRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, DismantleItemBoxHeight);
            uiItemBox.ShowItemBox<InventoryItem>(OnItemClick, true);
            uiWcDmResultBox.Open();
        }
        else
        {
            uiItemBoxRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, DefaultItemBoxHeight);
            uiItemBox.ShowItemBox<InventoryItem>(OnItemClick, false);
        }

        objBsBtns.SetActive(false);                     // 콘텐츠 버튼 비활성화
        curState = BsOpenState.Contents;                // 현재 Open State 설정
        curBsContentType = type;                        // 현재 게임 ContentType 설정
        UIManager.Instance.SetContentType(ContentType); // 현재 게임 ContentType UI Manager에 현행화
        guiContentTitle.SetTitle();                     // 현재 콘텐츠 Title 설정
        AnalyticsHelper.LogScreenView("BS"+type, GetType().Name); // GA 전송
    }

    /// <summary>
    /// 분해만 해당 - ResultBox 초기 상태 복원
    /// </summary>
    public void ResetResultBox()
    {
        if (curBsContentType == ContentType.Dismantle)
            uiWcDmResultBox.ResetGUI();
    }
    
    /// <summary>
    /// 인벤토리 GUI 업데이트
    /// </summary>
    public void UpdateInvenBoxGUI() => uiItemBox.UpdateGUI();

    /// <summary>
    /// Item 버튼 클릭 시 호출되는 추가 이벤트 처리 메서드
    /// </summary>
    private void OnItemClick(InventoryItem item)
    {
        if (curBsContentType == ContentType.Dismantle)
        {
            // Item 분해 결과 추가 시도
            uiWcDmResultBox.TryAddDmResult(item);
        }
        else
        {
            if (item == null 
                || item.ItemType == ItemType.None
                || (curBsContentType != ContentType.Enhancement 
                && curBsContentType != ContentType.LimitBreak))
            {
                return;
            }
            
            uiItemBox.Hide();
            uiItemInfo.Open(curBsContentType, item);
            curState = BsOpenState.Process;
        }
    }

    /// <summary>
    /// Close 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnClose()
    {
        switch (curState)
        {
            case BsOpenState.Main: // 메인 화면일 시, 로비 화면으로
                Close();
                UIManager.Instance.Open<UILobbyWindow>();
                return;
            case BsOpenState.Contents: // 콘텐츠 화면 일 시, 메인 화면으로
                ResetUI();
                break;
            case BsOpenState.Process: // 콘텐츠 내부 화면 일 시, 콘텐츠 화면으로
                uiItemInfo.Close();
                OpenContents(curBsContentType);
                break;
            default:
                MyDebug.LogError($"Is Not Blacksmith Content State => State: {curState}");
                return;
        }
    }
    
    /// <summary>
    /// 강화 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnEnhancement() => OpenContents(ContentType.Enhancement);
    /// <summary>
    /// 돌파 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnLimitBreak() => OpenContents(ContentType.LimitBreak);
    /// <summary>
    /// 분해 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnDismantle() => OpenContents(ContentType.Dismantle);
}
