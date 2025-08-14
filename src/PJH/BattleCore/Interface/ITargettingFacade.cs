using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 타겟 선택 및 공격 패턴 관리를 담당하는 Facade
/// 
/// 주요 역할:
/// - 확률 기반 단일 타겟 선택 (전열 60%, 중열 30%, 후열 10%)
/// - 보스별 공격 패턴 처리 (순차공격, 멀티타겟, 스플릿데미지)
/// - 스킬 타겟팅 시스템 (단일, 다중, 전체, 조건부)
/// - 거리 기반 최적 타겟 선택
/// </summary>
public interface IBattleTargetingFacade
{
    Monster SelectMonsterTarget();
    Unit SelectUnitTarget();
    List<Unit> GetAttackTargets(List<Unit> allUnits, Monster monster);
    public Unit GetClosestTarget(List<Unit> targets, Vector3 originPos);
    BossAttackPattern GetBossAttackPattern(string monsterCodeNumber);
    List<CharacterBase> GetSkillTargets(SkillData skillData, List<CharacterBase> units, List<CharacterBase> monsters);
}
