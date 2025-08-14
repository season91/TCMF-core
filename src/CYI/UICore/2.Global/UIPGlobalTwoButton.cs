using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwoButtonOpenContext : IUIOpenContext
{
    public string Title;
    public string Comment;
    public string Button1Text;
    public string Button2Text;
    public Action Button1Event;
    public Action Button2Event;
}

public class UIPGlobalTwoButton : UIBase
{
    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private TextMeshProUGUI tmpComment;
    [SerializeField] private Button btn1;
    [SerializeField] private TextMeshProUGUI tmpBtn1;
    [SerializeField] private Button btn2;
    [SerializeField] private TextMeshProUGUI tmpBtn2;

    private Action btn1Action;
    private Action btn2Action;
    
    protected override void Reset()
    {
        base.Reset();

        tmpTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_Title");
        tmpComment = transform.FindChildByName<TextMeshProUGUI>("Tmp_Comment");
        btn1 = transform.FindChildByName<Button>("Btn_1");
        tmpBtn1 = btn1.transform.GetComponentInChildren<TextMeshProUGUI>();
        btn2 = transform.FindChildByName<Button>("Btn_2");
        tmpBtn2 = btn2.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void Initialize()
    {
        base.Initialize();
        
        btn1.onClick.RemoveAllListeners();
        btn1.AddListener(OnBtn1Click);
        btn2.onClick.RemoveAllListeners();
        btn2.AddListener(OnBtn2Click);
    }

    public override void Open(OpenContext openContext = null)
    {
        if(openContext?.Context is not TwoButtonOpenContext castingContext) return;
        
        tmpTitle.text = castingContext.Title;
        tmpComment.text = castingContext.Comment;
        tmpBtn1.text = castingContext.Button1Text;
        tmpBtn2.text = castingContext.Button2Text;
        
        btn1Action = castingContext.Button1Event;
        btn2Action = castingContext.Button2Event;
        
        base.Open(openContext);
    }

    private void OnBtn1Click()
    {
        Close();
        btn1Action?.Invoke();
    }

    private void OnBtn2Click()
    {
        Close();
        btn2Action?.Invoke();
    }
}
