using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 천장 자원(PityCount) 관련 처리 서비스
/// - 수치 증가/감소/설정
/// - Firestore 반영 포함
/// </summary>
public class PityService
{
    /// <summary>
    /// 천장 수치 조회
    /// </summary>
    public int GetPityCount(ResourceType type)
    {
        return UserData.inventory.PityCountDic.TryGetValue(type, out int count) ? count : 0;
    }

    /// <summary>
    /// 천장 수치 추가
    /// </summary>
    public async Task AddPityCountAsync(ResourceType type, int amount)
    {
        if (!UserData.inventory.PityCountDic.ContainsKey(type))
        {
            UserData.inventory.PityCountDic[type] = 0;
        }

        UserData.inventory.PityCountDic[type] += amount;

        await UpdatePityCountToServerAsync(type);
    }

    /// <summary>
    /// 천장 수치 차감
    /// </summary>
    public async Task SubtractPityCountAsync(ResourceType type, int amount)
    {
        if (!UserData.inventory.PityCountDic.ContainsKey(type))
        {
            UserData.inventory.PityCountDic[type] = 0;
        }

        UserData.inventory.PityCountDic[type] = Mathf.Max(0, UserData.inventory.PityCountDic[type] - amount);

        await UpdatePityCountToServerAsync(type);
    }

    /// <summary>
    /// 천장 수치 직접 설정
    /// </summary>
    public async Task SetPityCountAsync(ResourceType type, int value)
    {
        UserData.inventory.PityCountDic[type] = Mathf.Max(0, value);

        await UpdatePityCountToServerAsync(type);
    }

    /// <summary>
    /// 서버에 천장 수치 반영
    /// </summary>
    private async Task UpdatePityCountToServerAsync(ResourceType type)
    {
        await UserData.inventory.UpdateInventoryPityCount(type);
    }   
}