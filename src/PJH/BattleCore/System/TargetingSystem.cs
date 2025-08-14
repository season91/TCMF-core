using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 전투 중 확률 기반으로 대상 선택하는 매니저
/// 일반 유닛 및 보스 전용 패턴을 포함
/// </summary>
public class TargetingSystem
{
    private int sequentialAttackIndex = 0; // 순차 공격용 인덱스
    
    /// <summary>
    /// 배치 별 확률 적용
    /// 위 확률 기반으로 단일 타겟 선택
    /// </summary>
    private static readonly Dictionary<int, float[]> probabilityCache = new()
    {
        { 1, new float[] { 100f } },
        { 2, new float[] { 60f, 40f } },
        { 3, new float[] { 60f, 30f, 10f } }
    };
    
    /// <summary>
    /// 보스 공격 패턴에 따라 공격 대상 리스트 반환
    /// </summary>
    public List<Unit> GetAttackTargets(List<Unit> allUnits, Monster monster)
    {
        List<Unit> aliveUnits = allUnits.FindAll(unit => unit.currentStat[StatType.Hp] > 0);
        List<Unit> targets = new List<Unit>();

        if (!monster.isBoss)
        {
            // 일반 몬스터 - 확률 기반 단일 대상
            targets.Add(SelectTarget(aliveUnits));
            return targets;
        }

        return GetBossTargets(aliveUnits, monster);
    }
    
    /// <summary>
    /// 보스몬스터 코드에 따른 공격 패턴 설정
    /// </summary>
    public BossAttackPattern GetBossAttackPattern(string monsterCodeNumber)
    {
        // monsterCode에 따라 보스 패턴 결정
        switch (monsterCodeNumber)
        {
            case BossMonsterCode.Boss1:
                return BossAttackPattern.Sequential;    // 나비여왕
            case BossMonsterCode.Boss2:
                return BossAttackPattern.MultiTarget;    // 덤불뿌리
            case BossMonsterCode.Boss3:
                return BossAttackPattern.SplitDamage;      // 울음꽃
            default:
                throw new Exception("잘못된 보스몬스터 코드입니다.");
        }
    }
    
    /// <summary>
    /// 보스몬스터 공격 패턴에 따른 타겟 설정
    /// </summary>
    private List<Unit> GetBossTargets(List<Unit> aliveUnits, Monster monster)
    {
        return monster.bossAttackPattern switch
        {
            BossAttackPattern.Sequential => GetSequentialTarget(aliveUnits)
                is Unit target ? new List<Unit> { target } : new List<Unit>(),
            BossAttackPattern.MultiTarget => GetMultiTargets(aliveUnits, 2),
            BossAttackPattern.SplitDamage => GetSplitDamageTargets(aliveUnits),
            _ => throw new Exception("잘못된 공격타입")
        };
    }
    
    /// <summary>
    /// 전열(0) → 중열(1) → 후열(2) 순서로 공격
    /// BattleManager.units의 인덱스 순서를 따름
    /// </summary>
    private Unit GetSequentialTarget(List<Unit> units)
    {
        // 현재 인덱스부터 시작해서 살아있는 플레이어 찾기
        for (int i = 0; i < units.Count; i++)
        {
            // i = 0 일때 처음에 sequentialAttackIndex가 0이므로 targetIndex =0
            
            //units[0]이 살아 있는 상태라면 units[0] 반환 및 For문 종료
            //반환 하기 전 sequentialAttackIndex +1 --> 다음 턴엔 units[1]부터 시작
            
            //만약 units[0]이 죽은 상태라면  i = 1, targetIndex = 1 --> units[1]
            
            //units[1]이 살아있으면 반환 후 sequentialAttackIndex +1 --> 반복
            int targetIndex = (sequentialAttackIndex + i) % units.Count;
            
            if (units[targetIndex].currentStat[StatType.Hp] > 0)
            {
                sequentialAttackIndex = (targetIndex + 1) % units.Count; // 다음 턴용
                MyDebug.Log($"{units[targetIndex].name} 공격");
                return units[targetIndex];
            }
        }

        return units.LastOrDefault();
    }

    /// <summary>
    /// 다중 타겟 설정
    /// </summary>
    private List<T> GetMultiTargets<T>(List<T> aliveTargets, int count) where T : CharacterBase
    {
        List<T> targets = new List<T>();
    
        if (aliveTargets == null || aliveTargets.Count == 0)
            return targets;
    
        // 후보보다 많이 선택하려고 하면 모든 후보 반환
        if (count >= aliveTargets.Count)
        {
            return new List<T>(aliveTargets);
        }
    
        // 중복 선택을 피하기 위해 후보 리스트 복사
        List<T> availableTargets = new List<T>(aliveTargets);
    
        for (int i = 0; i < count && availableTargets.Count > 0; i++) 
        {
            int randomIndex = Random.Range(0, availableTargets.Count);
            targets.Add(availableTargets[randomIndex]);
            availableTargets.RemoveAt(randomIndex); // 선택된 타겟 제거
        }
    
        return targets;
    }
    
    /// <summary>
    /// 스플릿 데미지 패턴을 위한 타겟 선택 
    /// 메인 타겟 1명에게는 높은 데미지, 나머지 모든 대상에게는 감소된 데미지 적용
    /// </summary>
    private List<Unit> GetSplitDamageTargets(List<Unit> aliveUnits)
    {
        List<Unit> targets = new List<Unit>();
        
        if (aliveUnits.Count == 0) return targets;
        
        // 첫 번째는 주 대상 
        Unit mainTarget = GetSequentialTarget(aliveUnits);
        targets.Add(mainTarget);
        MyDebug.Log($"주 대상: {mainTarget.name}");
        
        // 나머지는 스플릿 대상 
        foreach (Unit unit in aliveUnits)
        {
            if (unit != mainTarget)
            {
                targets.Add(unit);
                MyDebug.Log($"스플릿 대상: {unit.name}");
            }
        }
        
        return targets;
    } 
    
    /// <summary>
    /// 다중 타겟일 경우 가까운 타겟을 찾기 위한 메서드
    /// </summary>
    public Unit GetClosestTarget(List<Unit> targets, Vector3 originPos)
    {
        Unit closestTarget = targets[0];
        float closestDistance = Vector3.Distance(originPos, closestTarget.transform.position);
    
        foreach (var target in targets)
        {
            float distance = Vector3.Distance(originPos, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
    
        return closestTarget;
    }
    
    /// <summary>
    /// 배틀매니저의 IReadOnlyList unit을 받기 위해
    /// </summary>
    public T SelectTarget<T>(IReadOnlyList<T> unitList) where T : CharacterBase
    {
        if (unitList == null || unitList.Count == 0) return null;

        var aliveUnitList = new List<T>();
        for (int i = 0; i < unitList.Count; i++)
        {
            if (unitList[i].currentStat[StatType.Hp] > 0)
                aliveUnitList.Add(unitList[i]);
        }
        if (aliveUnitList.Count == 0) return null;
        
        float[] probabilities = GetProbabilities(aliveUnitList.Count);
        float random = Random.Range(0f, 100f);
        float sum = 0;

        for (int i = 0; i < aliveUnitList.Count; i++)
        {
            sum += probabilities[i];
            if (random < sum)
            {
                return aliveUnitList[i];
            }
        }

        return aliveUnitList.LastOrDefault();
    }
    
    /// <summary>
    /// List 타입을 받는 타겟 선택 메서드 (오버로드)
    /// 내부적으로 IReadOnlyList 버전으로 캐스팅하여 동일한 로직 사용
    /// </summary>
    public T SelectTarget<T>(List<T> unitList) where T : CharacterBase
    {
        return SelectTarget((IReadOnlyList<T>) unitList);
    }
    /// <summary>
    /// 유닛 수에 따른 확률 반환
    /// 3마리 시 60 30 10, 2마리시 60 40, 1마리시 100
    /// </summary>
    private float[] GetProbabilities(int count)
    {
        return probabilityCache.TryGetValue(count, out var probabilities)
            ? probabilities : throw new Exception("잘못된 인덱스입니다.");
    }
    
    /// <summary>
    /// 스킬 시스템과 연동하여 스킬 타입과 필터에 따른 대상 선택
    /// 다양한 스킬의 타겟팅 요구사항을 통합적으로 처리
    /// </summary>
    public List<CharacterBase> SelectSkillTargets(SkillTargetType targetType, TargetFilter targetFilter,
        List<CharacterBase> units, List<CharacterBase> monsters)
    {
        var aliveUnits = units.FindAll(u => u.currentStat[StatType.Hp] > 0);
        var aliveMonsters = monsters.FindAll(m => m.currentStat[StatType.Hp] > 0);
        
        List<CharacterBase> candidates = targetFilter switch
        {
            TargetFilter.Monster => aliveMonsters,
            TargetFilter.Unit => aliveUnits,
            TargetFilter.LowestHp => aliveUnits
                .OrderBy(t => (float)t.currentStat[StatType.Hp] / t.baseStat[StatType.Hp])
                .Take(1).ToList(),
            TargetFilter.None => new List<CharacterBase>(),
            _ => new List<CharacterBase>()
        };
        
        return targetType switch
        {
            SkillTargetType.Single =>  targetFilter == TargetFilter.LowestHp ? candidates 
                 : SelectTarget(candidates) is CharacterBase selected ?
                 new List<CharacterBase> { selected } 
                 : new List<CharacterBase>(),
            SkillTargetType.MultiTarget => GetMultiTargets(candidates, 2),
            SkillTargetType.All => candidates,
            SkillTargetType.None => new List<CharacterBase>(),
            _ => new List<CharacterBase>()
        };
    }
}
