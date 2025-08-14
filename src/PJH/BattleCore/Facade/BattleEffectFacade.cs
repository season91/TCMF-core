using UnityEngine;

/// <summary>
/// IBattleEffectFacade 구현체
/// </summary>
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
        //미구현 TODO : 카메라 흔들림 효과 + 사운드 재생 + 크리티컬 시 연출 (예상 구현)
    }
    
    public void SpawnSkillWithStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType status)
    {
        SpawnSkillEffect(caster, target);
        SpawnStatusEffect(caster, target, status);
    }
}