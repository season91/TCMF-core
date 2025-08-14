using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputOpenContext : IUIOpenContext
{
    /// <summary>
    /// InputField 안내 Title
    /// </summary>
    public string Title;
    /// <summary>
    /// Input Field 입력 전, Input Field에 표시해놓을 Text
    /// </summary>
    public string PlaceholderText;
    /// <summary>
    /// 확인 버튼 텍스트
    /// 기본: 확인
    /// </summary>
    public string OkButtonText = "확인";
    /// <summary>
    /// 확인 버튼 연결 이벤트
    /// 입력한 텍스트를 반환해줌
    /// </summary>
    public Action<string> OkButtonAction;
}

public class UIPGlobalInputBox : UIBase
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private TextMeshProUGUI tmpPlaceholder;
    [SerializeField] private Button btnOk;
    [SerializeField] private TextMeshProUGUI tmpBtnOk;
    private Action<string> btnOkEvent;
    
    protected override void Reset()
    {
        base.Reset();
        
        inputField = GetComponentInChildren<TMP_InputField>();
        tmpTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_Title");
        tmpPlaceholder = inputField.transform.FindChildByName<TextMeshProUGUI>("Placeholder");
        btnOk = transform.FindChildByName<Button>("Btn_Ok");
        tmpBtnOk = btnOk.GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void Open(OpenContext openContext = null)
    {
        if(openContext?.Context is not InputOpenContext castingContext) return;

        tmpTitle.text = castingContext.Title;
        tmpPlaceholder.text = castingContext.PlaceholderText;
        tmpBtnOk.text = castingContext.OkButtonText;
        btnOkEvent = castingContext.OkButtonAction;
        btnOk.onClick.RemoveAllListeners();
        btnOk.AddListener(OnOk);
        
        base.Open(openContext);
    }
    
    private void OnOk()
    {
        Close();
        btnOkEvent.Invoke(inputField.text);
    }
}
