/// <summary>
/// UIWcItemInven을 대장장이 인벤토리에서 쓰일 부분 재정의
/// </summary>
public class UIWcBsItemBox : UIWcItemBox
{
    /// <summary>
    /// UI 업데이트 이후 호출할 콜백
    /// </summary>
    protected override void CallbackUpdatedUI()
    {
        UIManager.Instance.GetUI<UIBlacksmithWindow>().ResetResultBox();
    }
}
