using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationController
{
    void HitAnimation(CharacterBase target);
    void DeathAnimation(CharacterBase target);
    void EvasionAnimation(CharacterBase unit);
    void TriggerSkillAnimation(CharacterBase caster, string triggerName);
}

/// <summary>
/// 배틀매니저의 역할을 덜기 위해 만든 인터페이스
/// </summary>
public interface IBattleServices
{
    IAnimationController AnimationController { get; }
    IReadOnlyList<Unit> Units { get; }
    IReadOnlyList<Monster> Monsters { get; }
    bool CheckBattleEnd();
    void ApplyDamage(CharacterBase attacker, CharacterBase target);
    Coroutine StartCoroutine(IEnumerator routine);
    void ApplySplitDamage(CharacterBase attacker, CharacterBase target, bool isMainTarget);
    void UpdateHideDieUnitSkillButton(Unit unit);
    IBattleActionFacade Actions { get; }
    IBattleUIFacade UI { get; }
    IBattleTargetingFacade Targeting { get; }
    IBattleFlowFacade Flow { get; }
    IBattleInputFacade Input { get; }
    IBattleEffectFacade Effects { get; }

}