using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 전체 흐름 관리, 매니저 관리 담당
/// 다른 매니저들 캐싱하고 있다가 순차적으로 호출하게 관리하는 역할
/// </summary>
public class GameManager: Singleton<GameManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    
    public StartManager StartManager;
    private LobbyManager lobbyManager;
    
    private float previousTimeScale;
    private void Start()
    {
        _ = Initialize();
    }
    
    /// <summary>
    /// 매니저 공통 초기화 함수
    /// </summary>
    private async Task Initialize()
    {
        // 해당 순서는 흐름에 맞게 호출한 것이니 절대 조정 하면 안됨
        await FirebaseManager.Instance.Initialize();
        await MasterDataHandler.AddToMasterData();
        await ResourceManager.Instance.Initialize();
        DOTween.Init();
        UIManager.Instance.Initialize();
        StartManager = new StartManager();
        lobbyManager = new LobbyManager(); 
        SoundManager.Instance.Initialize();
    }
    
    /// <summary>
    /// 각 씬에 필요한 초기화 작업을 씬 매니저에 위임한 것을 호출
    /// </summary>
    public void InitializeManager(SceneType type)
    {
        // 진행 상태 초기화 (모든 씬에 공통)
        float progress = LoadType.Init.Weight();
        UIManager.Instance.UpdateProgressBar(progress, true);
    
        switch(type)
        {
            case SceneType.Entry: // Entry 씬은 초기화 작업 없음
                break;
            case SceneType.Start: // Start 씬 초기화가 필요하다면 추가
                break;
            case SceneType.Lobby: // Lobby 씬에 대한 초기화 작업
                lobbyManager.Initialize();
                break;
            case SceneType.Battle: // Battle 씬에 대한 초기화 작업
                BattleManager.Instance.Initialize();
                break;
        }
    }

    /// <summary>
    ///  각 씬에 맞는 UI 및 설정을 적용
    /// </summary>
    public void SceneSetting(SceneType type)
    {
        // 모든 씬에 공통적으로 진행되는 UI 진행 표시
        UIManager.Instance.UpdateProgressBar(1, true, 0.5f);
        
        switch(type)
        {
            case SceneType.Entry: // Entry 씬에서 UI 설정만 필요
            case SceneType.Start: // Start 씬에서 UI 설정만 필요
                UIManager.Instance.SettingUI();
                break;
            case SceneType.Lobby: // Lobby 씬에 대한 설정과 UI 설정
                lobbyManager.Setting();
                lobbyManager.UISetting();
                break;
            case SceneType.Battle: // Battle 씬에 대한 설정과 UI 설정
                BattleManager.Instance.Setting();
                BattleManager.Instance.UISetting();
                break;
            case SceneType.Ending: // Ending 씬에서는 UI 변경만 필요
                UIManager.Instance.Open<UICredits>();
                break;
        }
    }

    
    /// <summary>
    /// 타임 스케일 공통 함수
    /// </summary>
    public void SetTimeScale(bool resume)
    {
        if (!resume)
        {
            previousTimeScale = Time.timeScale; // 2배속이든 뭐든 저장
            Time.timeScale = 0f; // 일시정지
        }
        else
        {
            Time.timeScale = previousTimeScale; // 저장한 값으로 복구
        }
    }  
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    public void ExitGame()
    {
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}