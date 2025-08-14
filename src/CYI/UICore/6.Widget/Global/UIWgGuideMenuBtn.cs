using System;
using TMPro;
using UnityEngine;

public class UIWgGuideMenuBtn : MonoBehaviour
{
    [SerializeField] private UIWgSelectBtn btn;
    [SerializeField] private TextMeshProUGUI tmpBtn;
    private Action<ContentType> clickEvent;
    private ContentType curContentType;

    private void Reset()
    {
        btn = GetComponent<UIWgSelectBtn>();
        tmpBtn = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(
        ContentType contentType, 
        Action<ContentType> onClick, 
        UIWgSelectBtnState selectState, 
        UIWgSelectBtnState unselectState)
    {
        curContentType = contentType;
        tmpBtn.SetText(contentType.ToKor());
        clickEvent = onClick;
        btn.SetEvent(SetGUI);
        btn.SetVisualStates(selectState, unselectState);
    }

    private void SetGUI() => clickEvent?.Invoke(curContentType);
    public void Select() => btn.Select();
    public void Unselect() => btn.Unselect();
}