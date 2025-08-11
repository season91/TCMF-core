using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleEffectFacade
{
    GameObject SpawnAttackEffect(CharacterBase attacker, CharacterBase target);
    GameObject SpawnSkillEffect(CharacterBase caster, CharacterBase target);
    GameObject SpawnStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType statusEffectType);
    void LaunchProjectile(CharacterBase attacker, CharacterBase target);
    void LaunchBossProjectile(CharacterBase attacker, CharacterBase target, bool isMainTarget);
    void LaunchSkillProjectile(CharacterBase caster, CharacterBase target);
    // 고수준 메서드들
    void SpawnCompleteAttackSequence(CharacterBase attacker, CharacterBase target);
    void SpawnSkillWithStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType status);
}

public class BattleEffectFacade : IBattleEffectFacade
{
    private readonly EffectSpawner effectSpawner;
    private readonly ProjectileLauncher projectileLauncher;
    
    public BattleEffectFacade(EffectSpawner effectSpawner, ProjectileLauncher projectileLauncher)
    {
        this.effectSpawner = effectSpawner;
        this.projectileLauncher = projectileLauncher;
    }
    
    public GameObject SpawnAttackEffect(CharacterBase attacker, CharacterBase target)
        => effectSpawner.SpawnAttackEffect(attacker, target);
    public GameObject SpawnSkillEffect(CharacterBase caster, CharacterBase target)
        => effectSpawner.SpawnSkillEffect(caster, target);
    public GameObject SpawnStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType statusEffectType)
        => effectSpawner.SpawnStatusEffect(caster, target, statusEffectType);
    public void LaunchProjectile(CharacterBase attacker, CharacterBase target)
         => projectileLauncher.LaunchProjectile(attacker, target);
    public void LaunchBossProjectile(CharacterBase attacker, CharacterBase target, bool isMainTarget)
        => projectileLauncher.LaunchBossProjectile(attacker, target, isMainTarget);
    public void LaunchSkillProjectile(CharacterBase caster, CharacterBase target)
        => projectileLauncher.LaunchSkillProjectile(caster, target);
    
    public void SpawnCompleteAttackSequence(CharacterBase attacker, CharacterBase target)
    {
        effectSpawner.SpawnAttackEffect(attacker, target);
    }
    
    public void SpawnSkillWithStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType status)
    {
        SpawnSkillEffect(caster, target);
        SpawnStatusEffect(caster, target, status);
    }
}