using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWgResult : MonoBehaviour
{
    [SerializeField] private Transform rewardRoot;
    [SerializeField] private UIWgReward originReward;
    
    private readonly List<UIWgReward> guiRewardList = new ();
    
    private void Reset()
    {
        rewardRoot = transform.FindChildByName<Transform>("Layout_Rewards");
        originReward = rewardRoot.FindChildByName<UIWgReward>("GUI_Reward");
    }

    public void Initialize()
    {
        originReward.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 획득 리워드 GUI 세팅
    /// </summary>
    public void Show(List<RewardData> rewardDataList)
    {
        ClearReward();
        
        // 리워드 세팅
        // 해당 리워드가 어떤 종류냐에 따라 결정 됨
        foreach (RewardData rewardData in rewardDataList)
        {
            UIWgReward uiWgReward = Instantiate(originReward, rewardRoot);
            Sprite icon = rewardData.type switch
            {
                RewardType.Gold or RewardType.Diamond or RewardType.Exp or RewardType.Piece
                    => ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.RewardDict[rewardData.type]),
                RewardType.Item
                    => MasterData.ItemDataDict[rewardData.rewardCode].Icon,
                RewardType.Unit 
                    => MasterData.UnitDataDict[rewardData.rewardCode].Icon,
                _ => null
            };

            uiWgReward.Show(icon, rewardData.amount, rewardData.type);
            guiRewardList.Add(uiWgReward);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(rewardRoot.GetComponent<RectTransform>());
    }
    
    private void ClearReward()
    {
        // 리워드 리스트 초기화
        foreach (var guiReward in guiRewardList)
        {
            Destroy(guiReward.gameObject);
        }
        guiRewardList.Clear();
    }
}
