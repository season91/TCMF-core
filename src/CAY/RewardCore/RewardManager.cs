using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum RewardType
{
    None,
    Gold, // 재화-골드
    Diamond, // 재화-다이아 1회
    Piece, // 재화-장비조각
    Exp, // 경험치
    Item, // 아이템 보상 1회
    Unit // 고양이 보상 1회
}

/// <summary>
/// 지급할 보상 정보를 담는 클래스
/// </summary>
[Serializable]
public class RewardData
{
    public RewardType type;
    public string rewardCode;
    public int amount;
    public bool isFirstClearOnly = false;
}

/// <summary>
/// 보상 전담 매니저
/// </summary>
public class RewardManager : Singleton<RewardManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    private StageRewardService stageRewardService;

    #region 스테이지 보상 지급 로직
    
    /// <summary>
    /// 지급할 보상 목록기준으로 실제 지급 절차 진행
    /// </summary>
    public async Task GrantRewards(List<RewardData> rewards)
    {
        foreach (var reward in rewards)
        {
            await ApplyReward(reward);
        }
    }

    /// <summary>
    /// 스테이지 보상 지급 DB 반영
    /// </summary>
    private async Task ApplyReward(RewardData reward)
    {
        switch (reward.type)
        {
            case RewardType.Gold:
                await InventoryManager.Instance.ResourceService.AddAsync(ResourceType.Gold, reward.amount);
                MyDebug.Log($"Gold x{reward.amount} 지급");
                break;
            case RewardType.Exp:
                bool isLevelUp = await UserDataHandler.ChangeExp(reward.amount);
                if (isLevelUp)
                {
                    // 레벨업 보상 구성
                    var levelUpRewards = ComposeLevelUpRewards();
                    await GrantRewards(levelUpRewards);

                    UIManager.Instance.EnqueuePopup<UILevelupPopup>(
                        OpenContext.WithContext(new LevelupOpenContext
                        {
                            Level = UserData.info.Level,
                            RewardList = levelUpRewards
                        }));
                    
                    MyDebug.Log($"레벨업! {UserData.info.Level} ");
                }
                MyDebug.Log($"Exp x{reward.amount} 지급");
                break;
            case RewardType.Piece:
                await InventoryManager.Instance.ResourceService.AddAsync(ResourceType.Piece, reward.amount);
                MyDebug.Log($"Piece x{reward.amount} 지급");
                break;
            case RewardType.Diamond:
                await InventoryManager.Instance.ResourceService.AddAsync(ResourceType.Diamond, reward.amount);
                MyDebug.Log($"Diamond x{reward.amount} 지급");
                break;
            case RewardType.Item:
                if (reward.isFirstClearOnly)
                {
                    await InventoryManager.Instance.ItemService.TryAddSingleItemAsync(reward.rewardCode);
                    MyDebug.Log($"Item x{reward.amount} 지급");
                }
                break;
            case RewardType.Unit:
                if (reward.isFirstClearOnly)
                {
                    await InventoryManager.Instance.UnitService.TryAddUnitAsync(reward.rewardCode);
                    MyDebug.Log($" Uni x{reward.amount} 지급");
                }
                break;
            default:
                MyDebug.LogWarning($"[보상] 알 수 없는 타입: {reward.type}");
                break;
        }
    }

    #endregion

    /// <summary>
    /// 스테이지 승리시 보상 목록 구성
    /// </summary>
    public List<RewardData> ComposeVictoryRewards()
    {
        var stageData = StageManager.Instance.GetCurrentStageData();
        var progress = StageManager.Instance.GetOrCreateStageProgress();

        var rewardService = new StageRewardService(stageData);
        return rewardService.ComposeVictoryRewards(progress);
    }
    
    /// <summary>
    /// 스테이지 실패시 보상 목록 구성
    /// </summary>
    public List<RewardData> ComposeFailureRewards()
    {
        var stageData = StageManager.Instance.GetCurrentStageData();

        var rewardService = new StageRewardService(stageData);
        return rewardService.ComposeFailureRewards();
    }

    /// <summary>
    /// 레벨업 보상 목록 구성
    /// </summary>
    public List<RewardData> ComposeLevelUpRewards()
    {
        var rewardService = new StageRewardService();
        return rewardService.ComposeLevelUpRewards();
    }
    
    /// <summary>
    /// 도감 보상 목록 구성
    /// </summary>
    public List<RewardData> ComposeCollectionRewards(CollectionData collectionData)
    {
        var rewardService = new StageRewardService();
        return rewardService.ComposeCollectionRewards(collectionData);
    }
}
