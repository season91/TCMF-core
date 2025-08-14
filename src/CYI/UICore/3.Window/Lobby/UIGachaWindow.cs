using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gacha의 Hub 스크립트
/// </summary>
public class UIGachaWindow : UIBase
{
    protected override ContentType ContentType
    {
        get
        {
            return currentType switch
            {
                ResourceType.Gold => ContentType.GoldGacha,
                ResourceType.Diamond => ContentType.DiaGacha,
                _ => ContentType.Gacha
            };
        } 
    }

    [Header("====[기본 구성]")]
    [SerializeField] private Button btnClose;
    [SerializeField] private GUIContentTitle guiContentTitle;
    [SerializeField] private GameObject gcNpc;

    [Header("====[Gacha Type]")]
    [SerializeField] private GameObject groupGachaTypes;
    [SerializeField] private Button btnGachaType1;
    [SerializeField] private Button btnGachaType2;
    [SerializeField] private TextMeshProUGUI tmpBtnGachaType1;
    [SerializeField] private TextMeshProUGUI tmpBtnGachaType2;

    [Header("====[Gacha GUI]")]
    [SerializeField] private GameObject groupGacha;
    [SerializeField] private Image imgGachaType;
    [SerializeField] private Button btnOne;
    [SerializeField] private Button btnTen;
    [SerializeField] private TextMeshProUGUI tmpBtnOne;
    [SerializeField] private TextMeshProUGUI tmpBtnTen;
    
    [Header("====[Pity Status]")]
    [SerializeField] private Slider sliderPityGauge;
    [SerializeField] private Image imgHalfHardPityCheckIcon;
    [SerializeField] private Image imgHardPityCheckIcon;
    [SerializeField] private TextMeshProUGUI tmpCurPityCount;

    private GachaOpenState gachaOpenState;
    private ResourceType currentType;
    private GachaResult curResult;
    private bool isProgress;
    
    /// <summary>
    /// 가챠에서의 현재 열린 UI 상태
    /// </summary>
    private enum GachaOpenState
    {
        Category,
        Gacha,
        GachaPopup
    }
    
    // =======================================================================
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();

        guiContentTitle = transform.FindChildByName<GUIContentTitle>("Group_ContentTitle");
        
        groupGachaTypes = transform.FindChildByName<Transform>("Layout_GachaTypes").gameObject;
        btnGachaType1 = transform.FindChildByName<Button>("Btn_GachaType (0)");
        btnGachaType2 = transform.FindChildByName<Button>("Btn_GachaType (1)");
        tmpBtnGachaType1 = btnGachaType1.transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnGachaType");
        tmpBtnGachaType2 = btnGachaType2.transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnGachaType");
        
        groupGacha = transform.FindChildByName<Transform>("Group_Gacha").gameObject;
        imgGachaType = groupGacha.transform.FindChildByName<Image>("Img_GachaType");
        btnOne = transform.FindChildByName<Button>("Btn_One");
        btnTen = transform.FindChildByName<Button>("Btn_Ten");
        tmpBtnOne = btnOne.transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnOne");
        tmpBtnTen = btnTen.transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnTen");
        
        sliderPityGauge = transform.FindChildByName<Slider>("Slider_PityGauge");
        imgHalfHardPityCheckIcon = transform.FindChildByName<Image>("Img_HalfHardCheckIcon");
        imgHardPityCheckIcon = transform.FindChildByName<Image>("Img_HardCheckIcon");
        
        tmpCurPityCount = transform.FindChildByName<TextMeshProUGUI>("Tmp_CurPityCount");
        
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        
        gcNpc = transform.FindChildByName<Transform>("NPC_0002").gameObject;
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        // Gold, Diamond 등 카테고리 버튼 텍스트 설정
        btnGachaType1.AddListener(OnGachaType1);
        btnGachaType2.AddListener(OnGachaType2);
        tmpBtnGachaType1.text = $"{RewardIconType.Gold.ToSpriteByTmp()} {ResourceType.Gold.ToKor()}뽑기";
        tmpBtnGachaType2.text = $"{RewardIconType.Diamond.ToSpriteByTmp()} {ResourceType.Diamond.ToKor()}뽑기";
        
        // 미리 리스너 제거 (중복 방지)
        btnOne.onClick.RemoveAllListeners();
        btnTen.onClick.RemoveAllListeners();
        btnOne.AddListener(OnGachaOne);
        btnTen.AddListener(OnGachaTen);

        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(Close);
        guiContentTitle.Initialize();
        gcNpc.SetActive(false);
    }
    
    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        isProgress = false;
        gachaOpenState = GachaOpenState.Category;
        currentType = ResourceType.None;
        UIManager.Instance.SetContentType(ContentType);
        guiContentTitle.SetTitle();
        
        groupGachaTypes.SetActive(true);
        groupGacha.SetActive(false);
    }
    
    /// <summary>
    /// NonUI 초기 상태 설정: 비활성화
    /// </summary>
    private void ResetNonUI()
    { 
        gcNpc.SetActive(false);
    }
    
    /// <summary>
    /// UI 열기: 사운드 재생, 배경 설정, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    public override void Open(OpenContext openContext = null)
    {
        UIManager.Instance.ChangeBg(StringAdrBg.Gacha);
        UIManager.Instance.Open<UIWcUserInfo>();
        gcNpc.SetActive(true);
        ResetUI();
        switch (gachaOpenState)
        {
            case GachaOpenState.Category:
                SoundManager.Instance.PlayBgm(StringAdrAudioBgm.Gacha);
                // 기본 세팅
                AnalyticsHelper.LogScreenView(AnalyticsGachaScreen.GachaMain, GetType().Name);
                break;
            case GachaOpenState.Gacha:
                break;
            case GachaOpenState.GachaPopup:
                ShowSelectedCategory(currentType);
                break;
            default:
                MyDebug.LogError($"This State is Not Exist => UI State: {gachaOpenState}");
                return;
        }
        
        base.Open(openContext);
    }
    
    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화, Main Hub Open
    /// </summary>
    public override void Close(CloseContext closeContext)
    {
        switch(gachaOpenState)
        {
            case GachaOpenState.Category:
                base.Close(closeContext);
                UIManager.Instance.Close<UIWcUserInfo>();
                UIManager.Instance.Open<UILobbyWindow>();
                ResetNonUI();
                break;
            case GachaOpenState.Gacha: // 카테고리 Open
                ResetUI();
                break;
            case GachaOpenState.GachaPopup:
                base.Close(closeContext);
                UIManager.Instance.Close<UIWcUserInfo>();
                ResetNonUI();
                break;
            default:
                MyDebug.LogError($"This State is Not Exist => UI State: {gachaOpenState}");
                return;
        }
    }

    /// <summary>
    /// 파라미터(ResourceType)에 해당하는 가챠 GUI Open 
    /// </summary>
    private void ShowSelectedCategory(ResourceType resourceType)
    {
        groupGachaTypes.SetActive(false);
        groupGacha.SetActive(true);
        gachaOpenState = GachaOpenState.Gacha;
        currentType = resourceType;
        UIManager.Instance.SetContentType(ContentType);
        guiContentTitle.SetTitle();
        
        SetGachaGUI();
        
        // Pity Item을 얻은 상태인지 세팅
        SetGetPityItems();
        SetCurPityCount();
        SetPityGauge();
    }

    /// <summary>
    /// 가챠 GUI 설정 - 버튼의 ResourceIcon 세팅, 버튼 텍스트 세팅, 천장 게이
    /// </summary>
    private void SetGachaGUI()
    {
        string resourceIcon = "";
        string resourceName = "";
        string gachaType = "";
        
        switch (currentType)
        {
            case ResourceType.Gold:
                AnalyticsHelper.LogScreenView(AnalyticsGachaScreen.GachaChooseGold, GetType().Name);
                gachaType = StringAdrCardBg.GoldFront;
                resourceIcon = RewardIconType.Gold.ToSpriteByTmp();
                resourceName = ResourceType.Gold.ToKor();
                
                break;
            case ResourceType.Diamond:
                AnalyticsHelper.LogScreenView(AnalyticsGachaScreen.GachaChooseDia, GetType().Name);
                gachaType = StringAdrCardBg.DiaFront;
                resourceIcon = RewardIconType.Diamond.ToSpriteByTmp();
                resourceName = ResourceType.Diamond.ToKor();
                break;
        }

        int cost = GachaManager.Instance.GetGachaCost(currentType);
        imgGachaType.sprite = ResourceManager.Instance.GetResource<Sprite>(gachaType);
        tmpBtnOne.text = $"{resourceIcon} {cost}{resourceName}";
        tmpBtnTen.text = $"{resourceIcon} {cost * 10}{resourceName}";
    }

    /// <summary>
    /// 천장 게이지 설정
    /// </summary>
    private void SetPityGauge()
    {
        float value = GachaManager.Instance.GetPityRatio(currentType);
        sliderPityGauge.value = value;
    }
    
    /// <summary>
    /// 현재 천장 카운트 텍스트 설정
    /// </summary>
    private void SetCurPityCount()
    {
        int pityCount = InventoryManager.Instance.PityService.GetPityCount(currentType);

        // 텍스트 갱신
        tmpCurPityCount.text = $"{pityCount}회";
    }

    /// <summary>
    /// 천장 넘을 시 호출
    /// </summary>
    private void SetGetPityItems()
    {
        imgHalfHardPityCheckIcon.enabled = GachaManager.Instance.IsGetHalfPity(currentType);
        imgHardPityCheckIcon.enabled = GachaManager.Instance.IsGetHardPity(currentType);
    }
    
    /// <summary>
    /// 가챠 시도
    /// </summary>
    private async Task Gacha(int count)
    {
        if (isProgress) return;
        isProgress = true;
        
        var gachaResult = await GachaManager.Instance.TryGacha(currentType, count);
        if (gachaResult == null)
        {
            isProgress = false;
            return;
        }
        gachaOpenState = GachaOpenState.GachaPopup;
        
        AnalyticsHelper.LogScreenView("Gacha"+currentType.ToString()+count+"Count", this.GetType().Name);
        
        // 받은 ItemData로 팝업 전부 등록
        UIManager.Instance.EnqueuePopup<UIPGacha>(OpenContext.With(gachaResult.GachaContext, ShowNormalGacha));
        if (gachaResult.PityContext != null)
        {
            UIManager.Instance.EnqueuePopup<UIPPity>(OpenContext.With(gachaResult.PityContext, ShowPityGacha));
        }
        
        UIManager.Instance.DequeuePopup();
    }
    
    /// <summary>
    /// 가챠 결과를 표시하는 콜백 메서드
    /// </summary>
    private void ShowNormalGacha()
    {
        SetCurPityCount();
        SetPityGauge();
    }

    /// <summary>
    /// 가챠 천장 결과를 표시하는 콜백 메서드
    /// </summary>
    private void ShowPityGacha()
    {
        Close();
        SetGetPityItems();
    }

    #region Button Events

    /// <summary>
    /// 가챠 Type1(Gold) 선택 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnGachaType1()
    {
        ShowSelectedCategory(ResourceType.Gold);
    }
    /// <summary>
    /// 가챠 Type2(Diamond) 선택 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnGachaType2()
    {
        ShowSelectedCategory(ResourceType.Diamond);
    }
    /// <summary>
    /// 1회 가챠 선택 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnGachaOne()
    {
        _ = Gacha(1);
    }
    /// <summary>
    /// 10회 가챠 선택 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnGachaTen()
    {
        _ = Gacha(10);
    }
    
    #endregion
}