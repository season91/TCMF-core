using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
/// 버프/디버프 상태효과 관리
/// 지속시간 관리, 스탯 적용/해제, 시각 효과 관리
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
    /// 기존 효과가 있으면 갱신, 없으면 새로 추가
    /// 스탯 변경 및 시각 효과 생성
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
    /// 상태효과 제거 => 이펙트 제거 + 스탯 초기화
    /// </summary>
    /// <param name="statusEffectType"></param>
    private void RemoveEffect(StatusEffectType statusEffectType)
    {
        if (statusEffects.Remove(statusEffectType))
        {
            RemoveEffectVisual(statusEffectType);
            RemoveEffectStat(statusEffectType);
        }
    }
    private void RemoveEffectVisual(StatusEffectType statusEffectType)
    {
        if (activeEffectObjects.TryGetValue(statusEffectType, out GameObject effectObject))
        {
            effectObject.GetComponent<Effect>().Deactivate();
            activeEffectObjects.Remove(statusEffectType);
        }
    }
    /// <summary>
    /// 스탯 초기화
    /// </summary>
    /// <param name="statusEffectType"></param>
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

    public void RemoveAllEffects()
    {
        var effectTypes = new List<StatusEffectType>(statusEffects.Keys);
        foreach (var effectType in effectTypes)
        {
            RemoveEffect(effectType);
        }
    }
    
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
    
    private void RecalculateAllStats()
    {
        // 1. 모든 스탯을 기본값으로 초기화
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
    /// 중복되지 않도록
    /// </summary>
    /// <returns></returns>
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
