using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
/// <summary>
/// 전체 흐름 관리, 매니저 관리
/// 다른 매니저들 캐싱하고 있다가 순차적으로 호출하게 관리하는 역할
/// 게임매니저가 곧 Entry씬에 매칭되는 매니저임
/// Addressable, 게임 엔진, DB 초기화
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
    
    private async Task Initialize()
    {
        // 초기화 순서
        // DB -> UI
        await FirebaseManager.Instance.InitializeFirebase();
        await MasterDataHandler.AddToMasterData();
        await ResourceManager.Instance.Initialize();
        DOTween.Init();
        UIManager.Instance.Initialize();
        StartManager = new StartManager();
        lobbyManager = new LobbyManager(); 
        SoundManager.Instance.Initialize();
        // await SceneLoadController.EnterSceneAsync(SceneType.Start);
        // 여기서 Intro 호출
        // InitializeManager(SceneType.Entry);
        
    }
    
    /// <summary>
    /// 각 씬에 맞게 초기화
    /// </summary>
    public void InitializeManager(SceneType type)
    {
        float progress = LoadType.Init.Weight();
        UIManager.Instance.UpdateProgressBar(progress, true);
    
        switch(type)
        {
            case SceneType.Entry:
                break;
            case SceneType.Start:
                break;
            case SceneType.Lobby:
                lobbyManager.Initialize();
                break;
            case SceneType.Battle:
                BattleManager.Instance.Initialize();
                break;
        }
    }

    /// <summary>
    /// 각 씬에 맞게 설정
    /// </summary>
    public void SceneSetting(SceneType type)
    {
        UIManager.Instance.UpdateProgressBar(1, true, 0.5f);
        switch(type)
        {
            case SceneType.Entry:
                UIManager.Instance.SettingUI();
                break;
            case SceneType.Start:
                UIManager.Instance.SettingUI();
                break;
            case SceneType.Lobby:
                lobbyManager.Setting();
                lobbyManager.UISetting();
                break;
            case SceneType.Battle:
                BattleManager.Instance.Setting();
                BattleManager.Instance.UISetting();
                break;
            case SceneType.Ending:
                UIManager.Instance.Open<UICredits>();
                break;
        }
    }

    public void SetTimeScale(bool resume)
    {
        if (!resume)
        {
            previousTimeScale = Time.timeScale; // 2배속이든 뭐든 저장
            Time.timeScale = 0f;             // 일시정지
        }
        else
        {
            Time.timeScale = previousTimeScale; // 저장한 값으로 복구
        }
        //Time.timeScale = Convert.ToInt32(isTime);   
    }  
    
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