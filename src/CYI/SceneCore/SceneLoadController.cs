using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum LoadType
{
    Scene,
    DataParse,
    Resources,
    Init,
    Setting
}

public enum SceneType
{
    None,
    Entry,
    Start,
    Lobby,
    Battle,
    Ending
}

public static class SceneLoadController
{
    // Loading 항목 및 가중치
    // 씬 로딩(20%), 리소스 프리팹 로딩(50%), 설정값 세팅(30%)
    public static float Weight(this LoadType type)
    {
        return type switch
        {
            LoadType.Scene => 0.2f,
            LoadType.Setting => 0.3f,
            LoadType.Resources => 0.5f,
            _ => 0
        };
    }
    
    /// <summary>
    /// Scene Type에 따른 Addressable에 등록된 Scene 로드 메서드,
    /// Loading UI와 함께 진행
    /// </summary>
    /// <param name="sceneType">로드하려는 Scene Type</param>
    public static async Task EnterSceneAsync(SceneType sceneType)
    {
        // 1. 로딩 선행 작업
        // 모든 Tween Kill, 모든 Resource Release
        DOTween.KillAll();
        await ResourceManager.Instance.UnloadResourcesByLabel();
        
        // 2. Scene 타입에 따른 => 주소, 라벨 설정
        string sceneAdr;
        string sceneLabelFront;
        switch (sceneType)
        {
            case SceneType.Start:
                sceneAdr = StringAdrScene.StartScene;
                sceneLabelFront = StringAdrLabelFront.StartScene;
                break;
            case SceneType.Lobby:
                sceneAdr = StringAdrScene.LobbyScene;
                sceneLabelFront = StringAdrLabelFront.LobbyScene;
                break;
            case SceneType.Battle:
                sceneAdr = StringAdrScene.GameScene;
                sceneLabelFront = StringAdrLabelFront.GameScene;
                break;
            case SceneType.Ending:
                sceneAdr = StringAdrScene.EndingScene;
                sceneLabelFront = StringAdrLabelFront.EndingScene;
                break;
            default:
                MyDebug.LogError($"Is Not Addressable Scene => SceneType: {sceneType}");
                return;
        }
        
        // 3. Scene Load와 그에 따른 초기 작업 진행
        // 주소에 따라 어드레서블에 등록된 Scene 로드
        if (sceneAdr == StringAdrScene.EndingScene)
        {
            await ResourceManager.Instance.LoadAdrSceneWithoutProgressBarAsync(sceneAdr);
        }
        else
        {
            await ResourceManager.Instance.LoadAdrSceneAsync(sceneAdr);
        }
        // 해당 Scene에 대한 매니저 초기화 작업
        GameManager.Instance.InitializeManager(sceneType);
        // 해당 Scene에 대한 모든 라벨의 에셋 어드레서블 등록
        await ResourceManager.Instance.LoadAssets(sceneLabelFront);
        // 해당 Scene에 대한 UI 초기화 작업
        UIManager.Instance.InitializeByLoadScene(sceneType);

        // 4. 해당 Scene Setting 작업 진행
        if (sceneAdr != StringAdrScene.EndingScene)
        {
            float progress = LoadType.Setting.Weight();
            UIManager.Instance.UpdateProgressBar(progress, true);
        }
        GameManager.Instance.SceneSetting(sceneType);
    }
}
