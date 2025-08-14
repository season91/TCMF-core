using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Start Scene의 Hub 스크립트
/// </summary>
public class UIStartWindow : UIBase
{
    [SerializeField] private Button btnEnterLobby;
    [SerializeField] private GameObject groupEnterLobby;
    private TwoButtonOpenContext twoButtonContext;

    public const string SigninPopupTitle = "로그인";
    public const string SigninPopupComment = "로그인 방법을 선택하세요";
    public const string SigninPopupAccountBtn = "계정 로그인";
    public const string SigninPopupGuestBtn = "게스트";
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        
        btnEnterLobby = transform.FindChildByName<Button>("Btn_EnterLobby");
        groupEnterLobby = transform.FindChildByName<Transform>("Group_EnterLobby").gameObject;
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        btnEnterLobby.onClick.RemoveAllListeners();
        btnEnterLobby.AddListener(OnEnterLobbyAsync);
        
        twoButtonContext = new ()
        {
            Title = SigninPopupTitle,
            Comment = SigninPopupComment,
            Button1Text = SigninPopupAccountBtn,
            Button1Event = OnSignIn,
            Button2Text = SigninPopupGuestBtn,
            Button2Event = OnGuest
        };
    }
    
    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void ResetUI()
    {
        groupEnterLobby.SetActive(false);
        ShowHowtoSignPopup();
    }
    
    /// <summary>
    /// UI 열기: BGM 재생, 초기 상태 복원
    /// </summary>
    /// <param name="openContext"></param>
    public override void Open(OpenContext openContext = null)
    {
        SoundManager.Instance.PlayBgm(StringAdrAudioBgm.EntryScene);
        ResetUI();
        base.Open(openContext);
    }

    #region 로그인 방법

    /// <summary>
    /// UI 팝업 띄우기: 로그인 방법 팝업
    /// </summary>
    private void ShowHowtoSignPopup()
    {
        UIManager.Instance.Open<UIPGlobalTwoButton>(OpenContext.WithContext(twoButtonContext));
    }
    
    /// <summary>
    /// 계정 로그인 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// - 에디터: 안내 팝업 표시
    /// - 빌드: 구글 로그인 시도
    /// </summary>
    private void OnSignIn()
    {
#if UNITY_EDITOR
        MyDebug.LogWarning("게스트 로그인 눌러라.....");
        ShowHowtoSignPopup();
#else
        GameManager.Instance.StartManager.TrySignInWithGoogle();
#endif
    }
    
    /// <summary>
    /// 게스트 버튼 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnGuest()
    {
        _ = GameManager.Instance.StartManager.TrySignInAsGuestAsync();
    }

    #endregion

    #region 로비로

    /// <summary>
    /// UI 표시: 로비로
    /// </summary>
    public void ShowEnterLobbyUI()
    {
        groupEnterLobby.SetActive(true);
    }
    
    /// <summary>
    /// 씬 전환 - Lobby
    /// </summary>
    private async void OnEnterLobbyAsync()
    {
        Close();
        await UIManager.Instance.EnterLoadingAsync(SceneType.Lobby);
    }

    #endregion
    
}

