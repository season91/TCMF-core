using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 스킬 시스템의 실행 클래스
/// 플레이어 유닛, 보스, 일반 몬스터의 모든 스킬 실행을 담당
/// 각 캐릭터 별로 고유한 스킬 로직을 구현하고 관리하는 역할
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
    
    /// <summary>
    /// 모든 캐릭터의 스킬 실행 함수를 딕셔너리에 등록하는 초기화 메서드
    /// 새로운 캐릭터나 스킬이 추가될 때 이 메서드에서 등록하면 됨
    /// </summary>
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

    /// <summary>
    /// 캐릭터 코드를 기반으로 적절한 스킬 실행 함수를 찾아 호출
    /// </summary>
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

    /// <summary>
    /// 어셔의 할퀴기 스킬 
    /// </summary>
    private void ExecuteScratch(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        battleServices.Effects.SpawnSkillEffect(caster, target);
        
        int damage = Mathf.RoundToInt(caster.currentStat[StatType.Atk] * skillData.DamageMultiplier);
        
        for (int i = 0; i < skillData.HitCount; i++)
        {
            DOVirtual.DelayedCall(i * BattleConfig.Instance.hitInterval,
                () => target.TakeDamage(damage) , ignoreTimeScale: false);
        }
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    /// <summary>
    /// 모모의 상처핥기 스킬 (힐) - 체력비례 30%
    /// </summary>
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
    
    /// <summary>
    /// 루루의 분신 베기
    /// 방어력을 무시하는 순수 데미지 다단 공격 + 시전자에게 회피율 증가 버프 적용
    /// </summary>
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

    /// <summary>
    /// 보스1의 환각
    /// 다중 대상에게 공격력 감소 디버프를 적용하는 광역 디버프 스킬
    /// </summary>
    private void ExecuteHallucination(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        foreach (var target in targets)
        {
            target.ApplyStatusEffect(StatusEffectType.AttackDecrease, skillData.Duration, skillData.EffectValue);;
        }
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }

    /// <summary>
    /// 보스2의 뿌리 휘감기
    /// 단일 대상을 스턴 상태
    /// </summary>
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

    /// <summary>
    /// 보스3의 침묵의 울음 스킬 실행
    /// 대상을 침묵 상태로 만들어 스킬 사용을 봉인
    /// </summary>
    private void ExecuteSilentHowl(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        var target = targets.FirstOrDefault();
        if (target == null) return;
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
        
        target.ApplyStatusEffect(StatusEffectType.Silence, skillData.Duration, skillData.EffectValue);
    }
    
    /// <summary>
    /// 침묵의 수호자 스킬 
    /// 대상의 공격력을 감소시키는 디버프
    /// </summary>
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
    
    /// <summary>
    /// 초록 버섯 스킬
    /// 고정된 수치로 대상의 HP를 회복시키는 단순한 힐링 스킬
    /// </summary>
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

    /// <summary>
    /// 행복한 묘목 스킬
    /// 투사체를 발사하여 다단 순수 데미지를 가하는 원거리 공격 스킬
    /// </summary>
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
    
    /// <summary>
    /// 울지 않는 사슴 스킬
    /// 강력한 단일 물리 공격을 가하는 근접 공격 스킬
    /// </summary>
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

    /// <summary>
    /// 침묵의 종달새 스킬
    /// 다중 대상에게 순수 데미지를 가하는 광역 공격 스킬
    /// </summary>
    private void ExecuteSmallFlutter(CharacterBase caster, List<CharacterBase> targets, SkillData skillData)
    {
        int damage = Mathf.RoundToInt(skillData.EffectValue);
        
        foreach (var target in targets)
        {
            battleServices.Effects.SpawnSkillEffect(caster, target);
            target.TakePureDamage(damage);
        }
        
        var key = caster.GetSkillSfxKey();
        SoundManager.Instance.PlaySfx(key);
    }
}
