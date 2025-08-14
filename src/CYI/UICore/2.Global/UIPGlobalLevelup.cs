using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelupOpenContext : IUIOpenContext
{
    public List<RewardData> RewardList;
    public int Level;
}

public class UIPGlobalLevelup : UIBasePopup
{
    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private UIWgResult uiWgResult;
    [SerializeField] private Button btnAccept;
    
    protected override void Reset()
    {
        base.Reset();
        
        tmpTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_LevelupTitle");
        btnAccept = transform.FindChildByName<Button>("Btn_Accept");
        uiWgResult = GetComponentInChildren<UIWgResult>();
    }

    public override void Initialize()
    {
        base.Initialize();
        uiWgResult.Initialize();
        
        btnAccept.onClick.RemoveAllListeners();
        btnAccept.AddListener(OnClose);
    }
    
    public override void Open(OpenContext openContext = null)
    {
        if(OpenContext.Context is not LevelupOpenContext castingContext) return;
        
        tmpTitle.text = $"{castingContext.Level}레벨 달성!";
        
        // Reward 보상 목록 
        uiWgResult.Show(castingContext.RewardList);
        
        base.Open(OpenContext);
    }

    private void OnClose()
    {
        Close();
    }
}
