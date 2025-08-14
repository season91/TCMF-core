using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    EvasionIncrease,
    AttackDecrease, //공격력 감소
    Poison,
    Stun,    //행동 불가
    Silence, //침묵 스킬사용 불가
}

[Serializable]
public class StatusEffect
{
    public StatusEffectType statusEffectType;
    public int duration;  //지속 시간
    public float value;   // 효과 수치

    public StatusEffect(StatusEffectType statusEffectType, int duration, float value)
    {
        this.statusEffectType = statusEffectType;
        this.duration = duration;
        this.value = value;
    }
    public bool IsExpired => duration <= 0;
}

/// <summary>
/// 1. 상태효과 적용 및 해제
/// 2. 지속시간 관리 및 매턴 감소
/// 3. 스탯 변경 및 복원
/// 4. 시각적 이펙트 생성 및 제거
/// 5. 중독 등 매턴 발동 효과 처리
/// </summary>
public class StatusEffectController
{
    private Dictionary<StatusEffectType, StatusEffect> statusEffects = new(); //캐릭터별 상태효과
    private Dictionary<StatusEffectType, GameObject> activeEffectObjects = new(); //캐릭터별 이펙트 오브젝트
    private CharacterBase character;
    private Dictionary<StatType, int> backupStats = new();
    /// <summary>
    /// StatusEffectController 초기화
    /// CharacterBase와 연결하고 백업 스탯 생성
    /// </summary>
    public void Initialize(CharacterBase character)
    {
        this.character = character;
    }
    
    /// <summary>
    /// 원본 스탯 백업 저장
    /// 버프/디버프 해제 시 이 값으로 복원
    /// </summary>
    public void BackupStats()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            backupStats[statType] = character.currentStat[statType];
        }
    }
    
    public bool HasEffect(StatusEffectType statusEffectType)
    {
        return statusEffects.TryGetValue(statusEffectType, out StatusEffect effect) && !effect.IsExpired;
    }
    
    /// <summary>
    /// 1. 기존 동일 효과가 있다면 제거 (시각효과, 스탯효과)
    /// 2. 새로운 효과 정보 저장 또는 기존 효과 갱신
    /// 3. 새로운 시각 효과 생성
    /// 4. 모든 스탯 재계산 및 적용
    /// </summary>
    public void ApplyEffect(StatusEffectType statusEffectType, int duration, float value)
    {
        if (statusEffects.TryGetValue(statusEffectType, out StatusEffect effect))
        {
            // 기존 시각 효과 제거
            RemoveEffectVisual(statusEffectType);
            // 기존 스탯 효과 제거
            RemoveEffectStat(statusEffectType);
            effect.duration = duration;
            effect.value = value;
        }
        else
        {
            var newEffect = new StatusEffect(statusEffectType, duration, value);
            statusEffects[statusEffectType] = newEffect;
        }
        CreateEffectVisual(statusEffectType);
        RecalculateAllStats();
    }

    /// <summary>
    /// 매턴 호출되어 상태효과 지속시간을 감소시키고 관련 처리를 수행
    /// 처리 순서:
    /// 1. 모든 상태효과의 지속시간 1턴 감소
    /// 2. 중독 등 매턴 발동 효과 처리
    /// 3. 만료된 상태효과들 제거
    /// </summary>
    public void ReduceEffectDuration()
    {
        if(statusEffects.Count == 0) return;
        List<StatusEffectType> expiredEffects = new List<StatusEffectType>();

        foreach (var kvp in statusEffects)
        {
            var effect = kvp.Value;

            if (effect.statusEffectType == StatusEffectType.Poison)
            {
                int poisonDamage = Mathf.RoundToInt(effect.value);
                character.TakePureDamage(poisonDamage);
                MyDebug.Log($"{character.UnitName}이 중독으로 {poisonDamage} 데미지 (남은 턴: {effect.duration})");
            }
            effect.duration--;
            if (effect.IsExpired)
            {
                expiredEffects.Add(effect.statusEffectType);
            }
        }

        foreach (var effectType in expiredEffects)
        {
            RemoveEffect(effectType);
        }
    }

    /// <summary>
    /// 특정 상태효과를 완전히 제거
    /// 시각 효과와 스탯 효과를 모두 제거
    /// </summary>
    private void RemoveEffect(StatusEffectType statusEffectType)
    {
        if (statusEffects.Remove(statusEffectType))
        {
            RemoveEffectVisual(statusEffectType);
            RemoveEffectStat(statusEffectType);
        }
    }
    
    /// <summary>
    /// 상태효과의 시각적 이펙트를 제거
    /// 이펙트 오브젝트를 비활성화하고 오브젝트 풀로 반환
    /// </summary>
    private void RemoveEffectVisual(StatusEffectType statusEffectType)
    {
        if (activeEffectObjects.TryGetValue(statusEffectType, out GameObject effectObject))
        {
            effectObject.GetComponent<Effect>().Deactivate();
            activeEffectObjects.Remove(statusEffectType);
        }
    }
    
    /// <summary>
    /// 특정 상태효과로 변경된 스탯을 원래 값으로 복원
    /// 백업된 스탯 값을 사용하여 복원
    /// </summary>
    private void RemoveEffectStat(StatusEffectType statusEffectType)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.AttackDecrease :
                character.currentStat[StatType.Atk] = backupStats[StatType.Atk];
                break;
            case StatusEffectType.EvasionIncrease:
                character.currentStat[StatType.Evasion] = backupStats[StatType.Evasion];
                break;
        }
    }
    
    /// <summary>
    /// 캐릭터의 모든 상태효과를 즉시 제거
    /// 전투 종료 시
    /// </summary>
    public void RemoveAllEffects()
    {
        // 현재 활성화된 모든 상태효과 타입을 복사 (반복 중 수정 방지)
        var effectTypes = new List<StatusEffectType>(statusEffects.Keys);
        foreach (var effectType in effectTypes)
        {
            RemoveEffect(effectType);
        }
    }
    
    /// <summary>
    /// 특정 상태효과가 스탯에 미치는 영향을 적용
    /// 백업된 원본 스탯을 기준으로 계산
    /// </summary>
    private void ApplyEffectStat(StatusEffectType statusEffectType, float value)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.AttackDecrease:
                character.currentStat[StatType.Atk] = Mathf.RoundToInt(backupStats[StatType.Atk] * (100 - value)/100f);
                break;
            case StatusEffectType.EvasionIncrease:
                character.currentStat[StatType.Evasion] = Mathf.RoundToInt(backupStats[StatType.Evasion] + value);
                break;
        }
    }
    
    /// <summary>
    /// 모든 활성 상태효과를 고려하여 캐릭터의 스탯을 재계산
    /// 1. 상태효과 관련 스탯들을 백업 값으로 초기화
    /// 2. 모든 활성 상태효과를 순차적으로 적용
    /// </summary>
    private void RecalculateAllStats()
    {
        // 상태효과 관련 스탯만 초기화
        var affectedStats = GetAffectedStatTypes();
        foreach (var statType in affectedStats)
        {
            if (backupStats.ContainsKey(statType))
            {
                character.currentStat[statType] = backupStats[statType];
            }
        }
        
        // 2. 모든 활성 효과를 적용
        foreach (var effect in statusEffects.Values)
        {
            ApplyEffectStat(effect.statusEffectType, effect.value);
        }
    }
    
    /// <summary>
    /// 현재 활성화된 상태효과들이 영향을 미치는 스탯 타입들을 반환
    /// 중복 제거를 위해 HashSet 사용
    /// </summary>
    private HashSet<StatType> GetAffectedStatTypes()
    {
        var affectedStats = new HashSet<StatType>();
    
        foreach (var effect in statusEffects.Values)
        {
            switch (effect.statusEffectType)
            {
                case StatusEffectType.AttackDecrease:
                    affectedStats.Add(StatType.Atk);
                    break;
                case StatusEffectType.EvasionIncrease:
                    affectedStats.Add(StatType.Evasion);
                    break;
                // 새로운 상태효과 추가시 여기에 추가
            }
        }
    
        return affectedStats;
    }
    
    /// <summary>
    /// 1. 기존 동일 타입 이펙트가 있다면 제거
    /// 2. BattleManager를 통해 새 이펙트 생성
    /// 3. 이펙트를 캐릭터에 부착
    /// 4. 활성 이펙트 목록에 등록
    /// </summary>
    private void CreateEffectVisual(StatusEffectType statusEffectType)
    {
        if (activeEffectObjects.ContainsKey(statusEffectType))
        {
            RemoveEffectVisual(statusEffectType);
        }
    
        GameObject effect = BattleManager.Instance.SpawnStatusEffect(character, character, statusEffectType);
        effect.transform.localScale = Vector3.one;
        effect.transform.SetParent(character.transform);
        
        activeEffectObjects[statusEffectType] = effect;
    }
}
