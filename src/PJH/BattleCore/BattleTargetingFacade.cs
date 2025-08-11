using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleTargetingFacade
{
    Monster SelectMonsterTarget();
    Unit SelectUnitTarget();
    List<Unit> GetAttackTargets(List<Unit> allUnits, Monster monster);
    public Unit GetClosestTarget(List<Unit> targets, Vector3 originPos);
    BossAttackPattern GetBossAttackPattern(string monsterCodeNumber);
    List<CharacterBase> GetSkillTargets(SkillData skillData, List<CharacterBase> units, List<CharacterBase> monsters);
}

public class BattleTargetingFacade : IBattleTargetingFacade
{
    private readonly TargetingSystem targetingSystem;
    private readonly IBattleServices battleServices;
    
    public BattleTargetingFacade(TargetingSystem targetingSystem, IBattleServices battleServices)
    {
        this.targetingSystem = targetingSystem;
        this.battleServices = battleServices;
    }
    
    public Monster SelectMonsterTarget()
        => targetingSystem.SelectTarget(battleServices.Monsters);
    public Unit SelectUnitTarget()
        => targetingSystem.SelectTarget(battleServices.Units);
    public List<Unit> GetAttackTargets(List<Unit> allUnits, Monster monster)
        => targetingSystem.GetAttackTargets(allUnits, monster);
    public Unit GetClosestTarget(List<Unit> targets, Vector3 originPos)
        => targetingSystem.GetClosestTarget(targets, originPos);
    public BossAttackPattern GetBossAttackPattern(string monsterCodeNumber)
        => targetingSystem.GetBossAttackPattern(monsterCodeNumber);
    public List<CharacterBase> GetSkillTargets(SkillData skillData, List<CharacterBase> units, List<CharacterBase> monsters)
        => targetingSystem.SelectSkillTargets(skillData.TargetType, skillData.TargetFilter, units, monsters);
}
