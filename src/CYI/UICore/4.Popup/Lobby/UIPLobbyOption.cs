using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비에서의 옵션 팝업
/// </summary>
public class UIPLobbyOption : UIBase
{
    [Header("====[Sound]")]
    [SerializeField] private Slider sliderBgm;
    [SerializeField] private Slider sliderSfx;

    [Header("====[Buttons]")]
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnSignOut;
    [SerializeField] private Button btnGameQuit;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();

        sliderBgm = transform.FindChildByName<Slider>("Slider_Bgm");
        sliderSfx = transform.FindChildByName<Slider>("Slider_Sfx");
        
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        btnSignOut = transform.FindChildByName<Button>("Btn_SignOut");
        btnGameQuit = transform.FindChildByName<Button>("Btn_GameQuit");
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        sliderBgm.onValueChanged.RemoveAllListeners();
        sliderBgm.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.Bgm, value));
        sliderSfx.onValueChanged.RemoveAllListeners();
        sliderSfx.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.Sfx, value));
        
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(Close);
        btnSignOut.onClick.RemoveAllListeners();
        btnSignOut.AddListener(OnSignOut);
        btnGameQuit.onClick.RemoveAllListeners();
        btnGameQuit.AddListener(OnGameQuit);
    }

    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        GameManager.Instance.SetTimeScale(false);
        sliderBgm.value = SoundManager.Instance.GetVolume(SoundType.Bgm);
        sliderSfx.value = SoundManager.Instance.GetVolume(SoundType.Sfx);
    }
    
    /// <summary>
    /// UI 열기: 사운드 재생, 배경 설정, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    public override void Open(OpenContext openContext = null)
    {
        UIManager.Instance.ChangeBg(StringAdrBg.LobbyBlur);
        ResetUI();
        base.Open(openContext);
    }

    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화, Main Hub Open
    /// </summary>
    public override void Close()
    {
        UIManager.Instance.Open<UILobbyWindow>();
        UIManager.Instance.ChangeBg(StringAdrBg.Lobby);
        GameManager.Instance.SetTimeScale(true);
        base.Close();
    }

    /// <summary>
    /// 로그아웃 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnSignOut()
    {
        Close();
        FirebaseManager.Instance.SignOut();
        _ = UIManager.Instance.EnterLoadingAsync(SceneType.Start);
    }
    
    /// <summary>
    /// 게임 종료 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnGameQuit()
    {
        Close();
        GameManager.Instance.ExitGame();
    }
}
