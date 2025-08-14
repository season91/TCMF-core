using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWcBsDmResultBox : MonoBehaviour
{
    private UIDynamicObjectPool<UIWgReward> dynamicRewardPool;
    [SerializeField] private RectTransform rewardRoot;
    [SerializeField] private UIWgReward originReward;

    [SerializeField] private TextMeshProUGUI tmpBtnAccept;
    [SerializeField] private Button btnAccept;

    private readonly Dictionary<InventoryItem, UIWgReward> selectedResultDict = new();
    private readonly Dictionary<RewardType, UIWgReward> resultDict = new();
    
    private void Reset()
    {
        rewardRoot = transform.FindChildByName<RectTransform>("Layout_Reward");
        originReward = transform.FindChildByName<UIWgReward>("GUI_Reward");

        tmpBtnAccept = transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnAccept");
        btnAccept = transform.FindChildByName<Button>("Btn_Accept");
    }

    public void Initialize()
    {
        dynamicRewardPool = new UIDynamicObjectPool<UIWgReward>(originReward, rewardRoot, 1);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        tmpBtnAccept.text = "분해하기";
        btnAccept.onClick.RemoveAllListeners();
        btnAccept.onClick.AddListener(OnAccept);
        
        ResetGUI();
        UIManager.Instance.GetUI<UIBlacksmithWindow>().UpdateInvenBoxGUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnAccept()
    {
        if(selectedResultDict.Keys.Count == 0)
            return;

        bool hasUpgrade = InventoryManager.Instance.DismantleService.HasUpgradedItem(selectedResultDict.Keys.ToList());
        
        string comment = hasUpgrade
            ? "강화 또는 돌파된 아이템이 있습니다.\n장비를 분해하시겠습니까?"
            : "장비를 분해하시겠습니까?";
        
        TwoButtonOpenContext context = new() 
        { 
            Title = "안내",
            Comment = comment, 
            Button1Text = "네", 
            Button1Event = StartDismantle,
            Button2Text = "아니오",
            Button2Event = null
        };
        
        UIManager.Instance.Open<UIPGlobalTwoButton>(OpenContext.WithContext(context));
    }

    private async void StartDismantle()
    {
        UIManager.Instance.Open<UIWaitingWindow>();
        // 분해하기 
        foreach (var selectedItem in selectedResultDict.Keys.ToList())
        { 
            await InventoryManager.Instance.DismantleService.TryDismantleAsync(selectedItem);
        }

        await Task.Yield();
        ResetGUI();
        UIManager.Instance.GetUI<UIBlacksmithWindow>().UpdateInvenBoxGUI();
        UIManager.Instance.Close<UIWaitingWindow>();
        SoundManager.Instance.PlaySfx(StringAdrAudioSfx.Dismantle);
    }

    public void ResetGUI()
    {
        selectedResultDict.Clear();
        resultDict.Clear();
        dynamicRewardPool.OffAll();
    }
    
    public void TryAddDmResult(InventoryItem item)
    {
        if (!InventoryManager.Instance.DismantleService.TryGetDismantleData(item, out var data))
            return;

        RewardType rewardType;
        // 선택한 아이템 취소할 시, Remove만
        if (selectedResultDict.Remove(item))
        {
            rewardType = data.ResourceType.ToRewardType();
            resultDict[rewardType].Show(null, -data.Amount, rewardType);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rewardRoot);
            return;
        }
        
        // 현재, 무조건 장비 조각
        rewardType = data.ResourceType.ToRewardType();
        Sprite icon = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.RewardDict[rewardType]);
        if (resultDict.TryGetValue(rewardType, out var guiReward))
        {
            guiReward.Show(icon, data.Amount, rewardType);
            selectedResultDict[item] = guiReward;
        }
        else
        {
            var reward = dynamicRewardPool.Get();
            reward.Initialize();
            reward.Show(icon, data.Amount, rewardType);
            resultDict[rewardType] = reward;
            selectedResultDict[item] = reward;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rewardRoot);
    }
}
