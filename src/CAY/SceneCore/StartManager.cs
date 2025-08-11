using System.Threading.Tasks;

/// <summary>
/// 로그인 흐름을 제어하는 일반 C# 클래스 (MonoBehaviour 아님)
/// UIEntry → StartManager 호출 → FirebaseManager로 로그인 실행
/// 이후 닉네임 설정 및 로비 진입 흐름 처리
/// </summary>
public class StartManager
{
    /// <summary>
    /// 게스트 로그인 시도 및 후처리 분기
    /// </summary>
    public async Task TrySignInAsGuestAsync()
    {
        await FirebaseManager.Instance.SignInAsGuestAsync();

        if (FirebaseManager.Instance.IsNewUser)
            OpenInputNicNameUI();
        else
            await EnterLobbyAsync();
    }
    
    /// <summary>
    /// 구글 로그인 시도 및 후처리 분기
    /// </summary>
    public void TrySignInWithGoogle()
    {
        FirebaseManager.Instance.SignInWithGoogle(async () =>
        {
            MyDebug.Log($"[StartManager] FirebaseManager.Instance.IsNewUser = {FirebaseManager.Instance.IsNewUser}");

            if (FirebaseManager.Instance.IsNewUser)
            {
                MyDebug.Log("[StartManager] 신규 유저 - 닉네임 입력 UI 오픈");
                OpenInputNicNameUI();
            }
            else
            {
                MyDebug.Log("[StartManager] 기존 유저 - EnterLobbyAsync 호출");
                await EnterLobbyAsync();
            }
        });
    }
    
    private void OpenInputNicNameUI()
    {
        var context = new InputOpenContext
        {
            Title = "닉네임 설정",
            PlaceholderText = "8글자(한글, 영문, 숫자)",
            OkButtonText = "닉네임 결정",
            OkButtonAction = async nickName =>
            {
                await FirebaseManager.Instance.UploadFirstUserData(nickName);
                await EnterLobbyAsync();
            }
        };

        UIManager.Instance.Open<UIInputBoxPopup>(OpenContext.WithContext(context));
    }
    
    private async Task EnterLobbyAsync()
    {
        await FirebaseManager.Instance.LoadUserDataAsync();
        UIManager.Instance.GetUI<UIEntry>().ReadyEnterLobby();
    }
    
}