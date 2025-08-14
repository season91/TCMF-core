
public class UIBasePopup : UIBase
{
    protected OpenContext OpenContext;
    
    public void SetOpen(OpenContext openContext)
    {
        OpenContext = openContext;
    }

    public override void Close(CloseContext closeContext = null)
    {
        canvasGroup.SetInteractable(false);
        canvasGroup.FadeAnimation(0);
        UIManager.Instance.ClosePopup();
        UIManager.Instance.DequeuePopup(closeContext?.OnComplete);
    }
}
