using DG.Tweening;
using UnityEngine;

/// <summary>
/// 전투 관련 애니메이션 담당
/// </summary>
public class AnimationController : IAnimationController
{
    /// <summary>
    /// 피격시 애니메이션
    /// 현재 스케일만 조정 일시적으로 크기를 늘렸다가 원래대로 복귀
    /// </summary>
    public void HitAnimation(CharacterBase target)
    {
        Sequence hitSequence = DOTween.Sequence();
        
        hitSequence.Append(target.transform.DOPunchScale(
            BattleConfig.Instance.hitPunchScale,
            BattleConfig.Instance.hitAnimationDuration,
            BattleConfig.Instance.hitAnimationVibrato,
            BattleConfig.Instance.hitAnimationElasticity
            ));

        hitSequence.SetAutoKill(true);
    }
    
    /// <summary>
    /// 유닛, 몬스터 사망시 애니메이션
    /// 현재 크기 줄어들면서 회전
    /// </summary>
    public void DeathAnimation(CharacterBase target)
    {
        Sequence deathSequence = DOTween.Sequence();
        
        deathSequence.Append(target.transform.DORotate(
            BattleConfig.Instance.deathRotation,
            BattleConfig.Instance.deathAnimationDuration,
            RotateMode.Fast
            ).SetEase(Ease.OutBounce));
        
        deathSequence.Join(target.transform.DOScale(Vector3.zero, BattleConfig.Instance.deathAnimationDuration));
        
        deathSequence.SetAutoKill(true);
    }

    /// <summary>
    /// 스킬 사용 시 캐릭터의 Animator 트리거를 실행하는 메서드
    /// Live2D 애니메이션 있는 경우만 적용
    /// </summary>
    public void TriggerSkillAnimation(CharacterBase caster, string triggerName)
    {
        Animator animator = caster.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            MyDebug.LogWarning($"{caster.UnitName}에 Animator가 없습니다.");
        }
    }
}
