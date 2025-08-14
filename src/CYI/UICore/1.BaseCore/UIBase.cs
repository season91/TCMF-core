using UnityEngine;

/// <summary>
/// UI Open 시, 전달할 데이터의 인터페이스
/// - UI마다 필요한 데이터가 다르므로 OpenContext 형태로 전달
/// - IUIOpenContext를 상속받아 각 UI 전용 Context 클래스 구현
/// </summary>
public interface IUIOpenContext { }

/// <summary>
/// 모든 UI의 기본이 되는 추상/베이스 클래스
/// - UI 열기/닫기, Fade 애니메이션, Canvas 설정, 인터랙션 제어 등 공통 기능 제공
/// - UI Manager와 연동되어 ContentType 관리
/// - Open 시 특정 데이터가 필요하면 OpenContext를 통해 전달
/// - Open/Close 시, 콜백이 필요하면 Open/CloseContext를 통해 전달
/// </summary>
public class UIBase : MonoBehaviour
{
    protected virtual ContentType ContentType => ContentType.None;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected Canvas canvas;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected virtual void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
    }
    
    /// <summary>
    /// 인스턴스화 시, UI Manager에 등록하는 메서드
    /// </summary>
    public void RegisterToManager()
    {
        UIManager.Instance.RegisterUI(this);
    }
    
    /// <summary>
    /// UI의 Awake단계: Canvas에 현재 씬의 메인 카메라 할당
    /// </summary>
    public virtual void Initialize()
    {
        canvasGroup.SetAlpha(0);
        
        if (canvas != null)
            canvas.worldCamera = UIManager.Instance.MainCamera;
    }
    
    /// <summary>
    /// UI 열기: 애니메이션 통일 - Fade In, Canvas 인터랙션 가능
    /// </summary>
    /// <param name="openContext"></param>
    public virtual void Open(OpenContext openContext = null)
    {
        if(ContentType != ContentType.None)
            UIManager.Instance.SetContentType(ContentType);
        canvasGroup.SetInteractable(true);
        canvasGroup.FadeAnimation(1, 0.2f, openContext?.OnComplete);
    }
    
    /// <summary>
    /// UI 닫기: 애니메이션 통일 - Fade Out, Canvas 인터랙션 불가능
    /// </summary>
    /// <param name="closeContext"></param>
    public virtual void Close(CloseContext closeContext)
    {
        if(ContentType != ContentType.None)
            UIManager.Instance.SetContentType(ContentType.None);
        canvasGroup.SetInteractable(false);
        canvasGroup.FadeAnimation(0, 0.2f, closeContext?.OnComplete);
        UIManager.Instance.DequeuePopup();
    }

    /// <summary>
    /// UI 닫기: 편의용 오버로드
    /// </summary>
    public virtual void Close() => Close(null);
}