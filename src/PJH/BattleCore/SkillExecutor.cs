using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 스킬 구현
/// </summary>
public class SkillExecutor
{
    private readonly IBattleServices battleServices;
    private readonly Dictionary<string, Action<CharacterBase, List<CharacterBase>, SkillData>> 
        skillActions = new();
    
    public SkillExecutor(IBattleServices services)
    {
        battleServices = services;
        Initialize();
    }
    
    private void Initialize()
    {
        // 플레이어 스킬 등록
        skillActions[PlayerUnitCode.Usher] = ExecuteScratch;
        skillActions[PlayerUnitCode.Momo] = ExecuteLickingWounds;
        skillActions[PlayerUnitCode.Ruru] = ExecuteCloneSlash;
        
        // 보스 스킬 등록
        skillActions[BossMonsterCode.Boss1] = ExecuteHallucination;
        skillActions[BossMonsterCode.Boss2] = ExecuteEntangle;
        skillActions[BossMonsterCode.Boss3] = ExecuteSilentHowl;

        //일반 몬스터 스킬 등록
        skillActions[MonsterCode.GreenMushroom] = ExecuteHealingSpore;
        skillActions[MonsterCode.HappySeedling] = ExecuteSummonWind;
        skillActions[MonsterCode.GuardianOfSilence] = ExecuteGuardianSkill;
        skillActions[MonsterCode.DontCryingDeer] = ExecuteBodySlam;
        skillActions[MonsterCode.SilentHummingbird] = ExecuteSmallFlutter;
    }

    public void ExecuteSkill(string entityCode, CharacterBase caster, List<CharacterBase> targets)
    {
        if (!MasterData.SkillDataDict.TryGetValue(entityCode, out var skillData))
        {
            MyDebug.LogWarning($"유닛 코드 {entityCode}를 찾을 수 없습니다.");
            return;
        }
        if (skillActions.TryGetValue(entityCode, out var action))
        {
            action.Invoke(caster,targets,skillData);
        }
    }

    private void ExecuteScratch(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        battleServices.Effects.SpawnSkillEffect(caster, target);
        int damage = Mathf.RoundToInt(caster.currentStat[StatType.Atk] * skillData.DamageMultiplier);
        for (int i = 0; i < skillData.HitCount; i++)
        {
            DOVirtual.DelayedCall(i * BattleConfig.Instance.hitInterval, () => target.TakeDamage(damage) , ignoreTimeScale: false);
        }
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    private void ExecuteLickingWounds(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        int healAmount = Mathf.RoundToInt(target.backupStat[StatType.Hp] * skillData.EffectValue / 100f);
        int healHp = Mathf.Min(target.currentStat[StatType.Hp] + healAmount, target.backupStat[StatType.Hp]);
        
        battleServices.Effects.SpawnSkillEffect(caster, target);
        
        target.currentStat[StatType.Hp] = healHp;
        float ratio = (float)target.currentStat[StatType.Hp] / target.baseStat[StatType.Hp];
        target.UpdateHpBar(ratio);
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }
    
    private void ExecuteCloneSlash(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        battleServices.Effects.SpawnSkillEffect(caster, target);
        int damage = Mathf.RoundToInt(caster.currentStat[StatType.Atk] * skillData.DamageMultiplier);
        for (int i = 0; i < skillData.HitCount; i++)
        {
            DOVirtual.DelayedCall(i * BattleConfig.Instance.hitInterval,
                () => target.TakePureDamage((damage - target.currentStat[StatType.Def]) / skillData.HitCount),ignoreTimeScale: false);
        }
        
        caster.ApplyStatusEffect(StatusEffectType.EvasionIncrease, skillData.Duration +1, skillData.EffectValue);
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    private void ExecuteHallucination(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        foreach (var target in targets)
        {
            target.ApplyStatusEffect(StatusEffectType.AttackDecrease, skillData.Duration, skillData.EffectValue);;
        }
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    private void ExecuteEntangle(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        battleServices.AnimationController.TriggerSkillAnimation(caster, "isSkill");
        var key = caster.GetSkillSfxKey();
        DOVirtual.DelayedCall(1f, () =>
        {
            battleServices.Effects.SpawnSkillEffect(caster, target);
            target.ApplyStatusEffect(StatusEffectType.Stun, skillData.Duration, skillData.EffectValue);
            SoundManager.Instance.PlaySfx(key);
            CameraShaker.Instance.Shake(0.2f, 0.2f);
        });
    }

    private void ExecuteSilentHowl(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
        // BattleManager.Instance.SpawnSkillEffect(caster, target);
        
        target.ApplyStatusEffect(StatusEffectType.Silence, skillData.Duration, skillData.EffectValue);
    }
    
    private void ExecuteGuardianSkill(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        battleServices.AnimationController.TriggerSkillAnimation(caster, "isSkill");
        
        DOVirtual.DelayedCall(1f, () =>
        {
            battleServices.Effects.SpawnSkillEffect(caster, target);
        
            target.ApplyStatusEffect(StatusEffectType.AttackDecrease, skillData.Duration, skillData.EffectValue);
        });
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }
    
    private void ExecuteHealingSpore(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        int healHp = Mathf.Min(target.currentStat[StatType.Hp] + (int)skillData.EffectValue, target.baseStat[StatType.Hp]);
        
        battleServices.Effects.SpawnSkillEffect(caster, target);
        
        target.currentStat[StatType.Hp] = healHp;
        float ratio = (float)target.currentStat[StatType.Hp] / target.baseStat[StatType.Hp];
        target.UpdateHpBar(ratio);
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    private void ExecuteSummonWind(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;

        battleServices.Effects.LaunchSkillProjectile(caster, target);
        int damage = (int)skillData.EffectValue;
        for (int i = 0; i < skillData.HitCount; i++)
        {
            DOVirtual.DelayedCall(i * 0.5f, () => target.TakePureDamage(damage));
        }
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }
    
    private void ExecuteBodySlam(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        battleServices.Effects.SpawnSkillEffect(caster, target);
        int damage = Mathf.RoundToInt(caster.currentStat[StatType.Atk] * skillData.DamageMultiplier);
        
        target.TakeDamage(damage);
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    private void ExecuteSmallFlutter(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        int damage = Mathf.RoundToInt(skillData.EffectValue);
        foreach (var target in targets)
        {
            battleServices.Effects.SpawnSkillEffect(caster, target);
            target.TakePureDamage(damage);
            // battleServices.UI.UpdateShowDamage(damage, target.GetTargetPoint());
        }
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }
}
