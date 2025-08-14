using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 종료에 따른 보상 구성
/// </summary>
public class StageRewardService
{
    private readonly StageData curStageData;

    public StageRewardService(){ }
    public StageRewardService(StageData stageData)
    {
        this.curStageData = stageData;
    }

    /// <summary>
    /// 스테이지 승리 보상 구성
    /// </summary>
    public List<RewardData> ComposeVictoryRewards(StageProgress progress)
    {
        var rewards = new List<RewardData>();

        if (!progress.RewardClaimed)
        {
            rewards.AddRange(ParseFirstClearRewards());
        }
        
        rewards.AddRange(ParseFixedRewards());

        var random = ParseRandomRewards();
        if (random.Count > 0)
            rewards.AddRange(random);

        return rewards;
    }

    /// <summary>
    /// 레벨업 보상 구성
    /// </summary>
    public List<RewardData> ComposeLevelUpRewards()
    {
        var rewards = new List<RewardData>();

        rewards.Add(new RewardData
        {
            type = RewardType.Diamond,
            amount = 5
        });

        return rewards;
    }
    
    /// <summary>
    /// 도감 보상 구성
    /// </summary>
    public List<RewardData> ComposeCollectionRewards(CollectionData collectionData)
    {
        var rewards = new List<RewardData>();

        rewards.Add(new RewardData
        {
            type = collectionData.RewardType,
            amount = collectionData.Amount
        });

        return rewards;
    }
    
    /// <summary>
    /// 스테이지 실패 보상 구성
    /// </summary>
    public List<RewardData> ComposeFailureRewards()
    {
        var list = new List<RewardData>();

        if (curStageData.Rewards == null)
        {
            return list;
        }

        foreach (var pair in curStageData.Rewards)
        {
            list.Add(new RewardData
            {
                type = ParseRewardType(pair.Key),
                amount = Mathf.RoundToInt(pair.Value * 0.3f), // 실패 시 30%만 지급
                isFirstClearOnly = false,
            });
        }
        
        return list;
    }

    /// <summary>
    /// 스테이지 보상 타입 enum 파싱
    /// 보상 타입 문자열 → RewardType enum
    /// </summary>
    private RewardType ParseRewardType(string str)
    {
        return Enum.TryParse(str, true, out RewardType result) ? result : RewardType.Gold;
    }
    
    /// <summary>
    /// 확정 보상 (매 클리어 시 확정 지급) 목록 List로 파싱
    /// </summary>
    private List<RewardData> ParseFixedRewards()
    {
        var list = new List<RewardData>();

        if (curStageData.Rewards == null)
            return list;

        foreach (var pair in curStageData.Rewards)
        {
            list.Add(new RewardData
            {
                type = ParseRewardType(pair.Key),
                amount = pair.Value,
                isFirstClearOnly = false,
            });
        }

        return list;
    }
    
    /// <summary>
    /// 랜덤 보상 (매 클리어 시 확률 지급) 목록 List 파싱
    /// </summary>
    private List<RewardData> ParseRandomRewards()
    {
        var list = new List<RewardData>();

        if (curStageData.RandomRewards == null)
            return null;

        foreach (var pair in curStageData.RandomRewards)
        {
            float randomChance = UnityEngine.Random.value;  // 0.0f ~ 1.0f 사이의 랜덤 값
            // pair.Value (랜덤 확률) 이내로 랜덤 값이 들어오면 보상 추가
            if (randomChance <= pair.Value)
            {
                list.Add(new RewardData
                {
                    type = ParseRewardType(pair.Key),
                    amount = 1,
                    isFirstClearOnly = false,
                });
            }
        }

        return list;
    }
    
    /// <summary>
    /// 1회성 보상 목록 List 파싱
    /// </summary>
    private List<RewardData> ParseFirstClearRewards()
    {
        var list = new List<RewardData>();

        if (curStageData.FirstClearRewards == null)
            return list;

        foreach (var pair in curStageData.FirstClearRewards)
        {
            bool isInt = int.TryParse(pair.Value, out var amount);
            list.Add(new RewardData
            {
                type = pair.Key,
                rewardCode = isInt ? null : pair.Value,
                amount = isInt ? amount : 1,
                isFirstClearOnly = true
            });
        }

        return list;
    }
}