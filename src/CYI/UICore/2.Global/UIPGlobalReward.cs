using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardOpenContext : IUIOpenContext
{
    public string Title;
    public List<RewardData> RewardList;
    public string ButtonText;
    public Action ButtonEvent;
}

public class UIPGlobalReward : UIBase
{
    private UIDynamicObjectPool<UIWgReward> dynamicRewardPool;
    [SerializeField] private Transform rewardRoot;
    [SerializeField] private UIWgReward originReward;

    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private Button btnAccept;
    [SerializeField] private TextMeshProUGUI tmpBtnAccept;
    private Action acceptAction;

    protected override void Reset()
    {
        base.Reset();

        rewardRoot = transform.FindChildByName<Transform>("Layout_Rewards");
        originReward = rewardRoot.GetComponentInChildren<UIWgReward>();
        
        tmpTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_Title");
        btnAccept = transform.FindChildByName<Button>("Btn_Accept");
        tmpBtnAccept = btnAccept.transform.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public override void Initialize()
    {
        base.Initialize();

        dynamicRewardPool = new UIDynamicObjectPool<UIWgReward>(originReward, rewardRoot, 1);
        
        btnAccept.onClick.RemoveAllListeners();
        btnAccept.AddListener(OnAccept);
    }

    public override void Open(OpenContext openContext = null)
    {
        if(openContext?.Context is not RewardOpenContext castingContext) return;

        tmpTitle.text = castingContext.Title;
        tmpBtnAccept.text = castingContext.ButtonText;
        acceptAction = castingContext.ButtonEvent;
        dynamicRewardPool.OffAll();
        foreach (var data in castingContext.RewardList)
        {
            UIWgReward reward = dynamicRewardPool.Get();
            reward.Initialize();
            Sprite icon = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.RewardDict[data.type]);
            reward.Show(icon, data.amount, data.type);
        }
        
        base.Open(openContext);
    }
    
    private void OnAccept()
    {
        acceptAction?.Invoke();
        Close();
    }
}
