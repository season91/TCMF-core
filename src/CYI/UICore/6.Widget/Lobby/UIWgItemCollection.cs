using UnityEngine;

/// <summary>
/// UIWidgetItem을 도감 전용으로 재정의
/// </summary>
public class UIWgItemCollection : UIWgItem
{
    [SerializeField] private GameObject objRedDot;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        objRedDot = transform.FindChildByName<Transform>("Img_RedDot").gameObject;
    }

    /// <summary>
    /// UI 표시 로직 + 수집하지 못한 아이템 실루엣 UI 설정
    /// </summary>
    public override void ShowCollection(CollectionSlot slot, int index = -1)
    {
        base.ShowCollection(slot, index);
        SetItemSilhouette(slot.IsCollected);
        
        objRedDot.SetActive(slot.IsCollected && !slot.IsRewardClaimed);
    }

    private void SetItemSilhouette(bool isCollected)
    {
        imgIcon.color = isCollected ? Color.white : Color.black;
    }

    protected override void OnClick()
    {
        base.OnClick();
        objSelected.SetActive(false);
    }
}