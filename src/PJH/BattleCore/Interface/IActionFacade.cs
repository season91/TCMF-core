/// <summary>
/// 전투 중 모든 액션 실행을 담당하는 핵심 Facade
/// 
/// 주요 역할:
/// - 기본공격 실행 (근접/원거리, 플레이어/몬스터)
/// - 스킬 사용 처리 (수동 타겟팅/자동 타겟팅)
/// - 보스 전용 액션 시퀀스 (공격 + 스킬 조합)
/// - 액션 진행 상태 관리 (동시 실행 방지)
/// - DOTween 기반 액션 시퀀스 제어
/// </summary>
public interface IBattleActionFacade
{
    void ExecuteBasicAttack(CharacterBase attacker, CharacterBase target);
    void ExecuteSkill(Unit caster, Monster monster);
    void ExecuteSkill(CharacterBase caster);
    void ExecuteBossAction(Monster monster);
    bool IsActionInProgress();
    void SetActionInProgress(bool value);
}

