using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 얻은 천장 아이템 안내 팝업
/// </summary>
public class UIPPity : UIBasePopup
{
    [SerializeField] private GameObject groupPity;
    [SerializeField] private TextMeshProUGUI tmpPityTitle;
    [SerializeField] private UIWgItem guiPityItem;
    [SerializeField] private Button btnAccept;
    private PityResultOpenContext pityResultOpenContext;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        
        groupPity = transform.FindChildByName<Transform>("Group_Pity").gameObject;
        tmpPityTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_PityPopupTitle");
        guiPityItem = transform.FindChildByName<UIWgItem>("GUI_Item");
        btnAccept = transform.FindChildByName<Button>("Btn_Accept");
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        btnAccept.onClick.RemoveAllListeners();
        btnAccept.AddListener(Close);
    }
    
    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        tmpPityTitle.text = $"{pityResultOpenContext.PityCount}뽑기 달성 보상";
        groupPity.SetActive(true);
        guiPityItem.ShowData(pityResultOpenContext.ItemData);
    }
    
    /// <summary>
    /// UI 열기: 사운드 재생, 배경 설정, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    /// <param name="openContext">PityResultOpenContext 필수</param>
    public override void Open(OpenContext openContext = null)
    {
        if (OpenContext.Context is not PityResultOpenContext castingContext) return;
        pityResultOpenContext = castingContext;
        
        UIManager.Instance.ChangeBg(StringAdrBg.GachaBlur);
        AnalyticsHelper.LogScreenView(AnalyticsGachaScreen.GachaCeilingPopup, GetType().Name);
        ResetUI();
        base.Open(OpenContext);
    }
    
    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화, Main Hub Open
    /// </summary>
    public override void Close()
    {
        base.Close();
        UIManager.Instance.Open<UIGachaWindow>();
    }
}
