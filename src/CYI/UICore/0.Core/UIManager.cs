using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OpenContext
{
    public IUIOpenContext Context { get; set; }
    public Action OnComplete { get; set; }

    public static OpenContext WithContext(IUIOpenContext context) 
        => new() { Context = context };
    public static OpenContext WithCallback(Action callback) 
        => new () { OnComplete = callback };
    public static OpenContext With(IUIOpenContext context, Action callback) 
        => new() { Context = context, OnComplete = callback };
}

public class CloseContext
{
    public Action OnComplete { get; set; }
    public static CloseContext WithCallback(Action callback) 
        => new() { OnComplete = callback };
}

public enum ContentType
{
    None,
    Lobby,
    UnitInventory,
    ItemInventory,
    Collection,
    Gacha,
    GoldGacha,
    DiaGacha,
    Blacksmith,
    Enhancement,
    LimitBreak,
    Dismantle,
    Battle,
    UnitSelect,
}

public class UIManager : Singleton<UIManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    public Camera MainCamera => Camera.main;
    public SceneType CurSceneType { get; private set; } = SceneType.Entry;
    public ContentType CurContentType { get; private set; } = ContentType.None;
    private readonly Dictionary<Type, UIBase> instanceUIs = new ();

    // ===== [순차 실행 팝업]
    private readonly Queue<UIBasePopup> popupQueue = new();
    private bool isPopup;
    public void ClosePopup() => isPopup = false;
    
    // ===== [모든 팝업/로딩 이후 콜백 액션]
    private Action allPopupCompleteAction;
    public event Action AllLoadingCompleteAction
    {
        add => allPopupCompleteAction += value;
        remove => allPopupCompleteAction -= value;
    }
    public void RemoveAllLoadingEvents() => allPopupCompleteAction = null;
    
    // ===== [로딩 프로그레스 업데이트]
    public static event Action<float, bool, float> OnProgressBarUpdated;
    
    // ===== [재화 및 경험치 업데이트]
    public void UpdateProgressBar(float value, bool animate = false, float duration = 0.3f) 
        => OnProgressBarUpdated?.Invoke(value, animate, duration);
    public void UpdateUserResource(ResourceType resourceType) 
        => GetUI<UIWcUserInfo>().UpdateResource(resourceType);
    public void UpdateUserExpGauge() 
        => GetUI<UIWcUserInfo>().UpdateUserExp();

    public UIWgCutIn wgCutIn;
    
    #region 최초 초기화
    
    // Overlay Canvas - Loading
    [SerializeField] private UILoadingWindow uiLoadingWindow;
    // No Canvas - Bg
    [SerializeField] private UIBackgroundWindow uiBackgroundWindow;
    
    private void Reset()
    {
        uiLoadingWindow = GetComponentInChildren<UILoadingWindow>();
        uiBackgroundWindow = GetComponentInChildren<UIBackgroundWindow>();
    }
    public void Initialize()
    {
        var uiStartLoading = transform.GetComponentInChildren<UIStartLoadingWindow>();
        uiStartLoading.PlayIntro();
        uiLoadingWindow.Initialize();
        uiLoadingWindow.Open();
        UpdateProgressBar(0.5f, true, 0.5f);
        InitializeByLoadScene(SceneType.Entry);
    }
    public void SettingUI()
    {
        ChangeBg(StringAdrBg.Loading);
        Open<UISceneStart>();
    }
    
    #endregion
    
    #region 순차실행 Queue

    public void EnqueuePopup<T>(OpenContext openContext) where T : UIBasePopup
    {
        var popup = GetUI<T>();
        popup.SetOpen(openContext);
        popupQueue.Enqueue(popup);
    }
    
    public void DequeuePopup(Action onAllComplete = null)
    {
        if(isPopup) return;
        if(onAllComplete != null)
            allPopupCompleteAction += onAllComplete;
        
        if (popupQueue.Count == 0)
        {
            allPopupCompleteAction?.Invoke();
            allPopupCompleteAction = null;
            return;
        }
        
        isPopup = true;
        var popup = popupQueue.Dequeue();
        popup.Open();
    }

    #endregion

    #region UI Base

    public void SetContentType(ContentType type) => CurContentType = type;
    
    public void RegisterUI(UIBase ui)
    {
        var type = ui.GetType();
        if (!instanceUIs.TryAdd(type, ui))
        {
            MyDebug.LogWarning($"UI {type} already registered.");
        }
    }
    
    public T GetUI<T>() where T : UIBase
    {
        var type = typeof(T);
        if (instanceUIs.TryGetValue(type, out var ui))
            return (T)ui;
        MyDebug.Log($"UI {type} not found.");
        return null;
    }

    public void Initialize<T>() where T : UIBase
    {
        GetUI<T>()?.Initialize();
    }

    public void Open<T>(OpenContext openContext) where T : UIBase
    {
        var ui = GetUI<T>();
        ui.Open(openContext);
    }
    
    public void Open<T>() where T : UIBase
    {
        var ui = GetUI<T>();
        ui.Open();
    }

    public void Close<T>(CloseContext closeContext) where T : UIBase
    {
        GetUI<T>()?.Close(closeContext);
    }
    
    public void Close<T>() where T : UIBase
    {
        GetUI<T>()?.Close();
    }

    #endregion
    
    /// <summary>
    ///  씬 전환 시 불러오기
    /// </summary>
    public async Task EnterLoadingAsync(SceneType sceneType)
    {
        uiLoadingWindow.Open();
        await Task.Delay(1000); // Loading Open Delay
        
        await SceneLoadController.EnterSceneAsync(sceneType);
    }
    
    /// <summary>
    ///  씬 전환 시 불러오기
    /// </summary>
    public async Task EnterWithoutLoadingAsync(SceneType sceneType)
    {
        await SceneLoadController.EnterSceneAsync(sceneType);
    }
    
    /// <summary>
    ///  씬 로드 시, 초기화할 것들
    ///  => 현재 씬 타입 설정, 모든 활성화된 UI 초기화
    /// </summary>
    /// <param name="sceneType"></param>
    public void InitializeByLoadScene(SceneType sceneType)
    {
        // Scene 타입 설정 
        CurSceneType = sceneType;
        
        // 활성화된 UI 전부 초기화
        foreach (var activeUI in instanceUIs)
        {
            activeUI.Value.Initialize();
        }
    }
    
    /// <summary>
    ///  인스턴스 UI List 비우기
    /// </summary>
    public void ClearInstanceUIList() => instanceUIs.Clear();

    /// <summary>
    /// 배경 화면 변경 메서드
    /// </summary>
    public void ChangeBg(string bgAdr)
    {
        uiBackgroundWindow.ChangeBg(bgAdr);
    }
    public IEnumerator StartCutin(string unitName, System.Action onCutinFinished)
    {
        if (wgCutIn == null)
        {
            wgCutIn = GetComponentInChildren<UIWgCutIn>();
        }
        wgCutIn.ReplaceImg(unitName);
        // 컷인 연출이 끝날 때까지 대기
        yield return StartCoroutine(wgCutIn.Play());
        
        // 컷인이 완전히 끝나면 콜백을 호출
        onCutinFinished?.Invoke();
    }
}
