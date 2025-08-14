using UnityEngine;

/// <summary>
/// 전투 중 모든 시각적 이펙트 생성 및 관리를 담당하는 Facade
/// 
/// 주요 역할:
/// - 공격/스킬/상태효과 이펙트 스폰 및 배치
/// - 투사체 발사 및 궤적 처리 (기본공격, 스킬, 보스 전용)
/// - 캐릭터 타입별 이펙트 위치/크기/회전 자동 조정
/// </summary>
public interface IBattleEffectFacade
{
    GameObject SpawnAttackEffect(CharacterBase attacker, CharacterBase target);
    GameObject SpawnSkillEffect(CharacterBase caster, CharacterBase target);
    GameObject SpawnStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType statusEffectType);
    void LaunchProjectile(CharacterBase attacker, CharacterBase target);
    void LaunchBossProjectile(CharacterBase attacker, CharacterBase target, bool isMainTarget);
    void LaunchSkillProjectile(CharacterBase caster, CharacterBase target);
    // 고수준 메서드들
    // 미구현 TODO : 공격 이펙트 생성 + 카메라 흔들림 효과 + 사운드 재생 + 크리티컬 시 연출 (예상 구현)
    void SpawnCompleteAttackSequence(CharacterBase attacker, CharacterBase target);
    void SpawnSkillWithStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType status);
}
