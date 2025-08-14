using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 애니메이션 제어를 담당하는 인터페이스
/// TODO : Facade 추가하는게 좋을 듯
/// </summary>
public interface IAnimationController
{
    void HitAnimation(CharacterBase target);
    void DeathAnimation(CharacterBase target);
    void TriggerSkillAnimation(CharacterBase caster, string triggerName);
}

/// <summary>
/// 전투 시스템의 모든 서비스에 접근할 수 있는 통합 인터페이스
/// 설계 목적:
/// 1. BattleManager의 역할 분산 - 거대한 매니저 클래스 분해
/// 2. 의존성 관리 단순화 - 하나의 인터페이스로 모든 서비스 접근
/// 3. 순환 참조 방지 - 각 컴포넌트 간 직접 참조 대신 서비스 경유
/// 4. 확장성 제공 - 새로운 서비스 추가 시 기존 코드 영향 최소화
/// </summary>
public interface IBattleServices
{
    IAnimationController AnimationController { get; }
    IReadOnlyList<Unit> Units { get; }
    IReadOnlyList<Monster> Monsters { get; }
    bool CheckBattleEnd();
    void ApplyDamage(CharacterBase attacker, CharacterBase target);
    void ApplySplitDamage(CharacterBase attacker, CharacterBase target, bool isMainTarget);
    Coroutine StartCoroutine(IEnumerator routine);
    void UpdateHideDieUnitSkillButton(Unit unit);
    IBattleActionFacade Actions { get; }
    IBattleUIFacade UI { get; }
    IBattleTargetingFacade Targeting { get; }
    IBattleFlowFacade Flow { get; }
    IBattleInputFacade Input { get; }
    IBattleEffectFacade Effects { get; }

}