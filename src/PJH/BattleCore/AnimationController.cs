using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 전투 관련 애니메이션 담당
/// </summary>
public class AnimationController : IAnimationController
{
    // private Dictionary<CharacterBase, Renderer[]> renderCache = new Dictionary<CharacterBase, Renderer[]>();
    // private Dictionary<CharacterBase, Color> originColor = new Dictionary<CharacterBase, Color>();
    //
    public void HitAnimation(CharacterBase target)
    {
        Sequence hitSequence = DOTween.Sequence();
        
        hitSequence.Append(target.transform.DOPunchScale(
            BattleConfig.Instance.hitPunchScale,
            BattleConfig.Instance.hitAnimationDuration,
            BattleConfig.Instance.hitAnimationVibrato,
            BattleConfig.Instance.hitAnimationElasticity
            ));
        // var renderers = GetRenderer(target);
        // if (renderers.Length > 0)
        // {
        //     Color originalColor = GetOriginColor(target);
        //     hitSequence.Join(renderers[0].material.DOColor(Color.red, BattleConfig.Instance.colorChangeDuration)); 
        //     hitSequence.Insert(BattleConfig.Instance.colorChangeDuration, renderers[0].material.DOColor(
        //         originalColor, BattleConfig.Instance.colorChangeDuration));
        // }

        hitSequence.SetAutoKill(true);
    }
    
    public void DeathAnimation(CharacterBase target)
    {
        Sequence deathSequence = DOTween.Sequence();
        
        deathSequence.Append(target.transform.DORotate(
            BattleConfig.Instance.deathRotation,
            BattleConfig.Instance.deathAnimationDuration,
            RotateMode.Fast
            ).SetEase(Ease.OutBounce));
        
        deathSequence.Join(target.transform.DOScale(Vector3.zero, BattleConfig.Instance.deathAnimationDuration));

        // deathSequence.OnComplete(() => ClearCache(target));
        
        deathSequence.SetAutoKill(true);
    }

    public void EvasionAnimation(CharacterBase unit)
    {
        Vector3 originPos = unit.transform.position;
        Vector3 backStepPos = originPos + Vector3.left * BattleConfig.Instance.evasionBackstepDistance; // 백스텝 거리
    
        Sequence backStepSequence = DOTween.Sequence();
        backStepSequence.Append(unit.transform.DOMove(backStepPos, BattleConfig.Instance.evasionBackstepTime).SetEase(Ease.OutQuart)) // 빠르게 뒤로
            .Append(unit.transform.DOMove(originPos, BattleConfig.Instance.evasionReturnTime).SetEase(Ease.InOutQuart));
        
        backStepSequence.SetAutoKill(true);
    }
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

    /// <summary>
    /// 렌더러 주소 캐싱
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    // private Renderer[] GetRenderer(CharacterBase target)
    // {
    //     if (!renderCache.TryGetValue(target, out Renderer[] renderers))
    //     {
    //         renderers = target.GetComponentsInChildren<Renderer>();
    //         renderCache[target] = renderers;
    //     }
    //
    //     return renderers;
    // }
    //
    // /// <summary>
    // /// 원본 색상 꺼내오기
    // /// originColor 캐시에 없으면 → 렌더러에서 다시 읽어와서 originColor를 보충
    // /// </summary>
    // /// <param name="target"></param>
    // /// <returns></returns>
    // private Color GetOriginColor(CharacterBase target)
    // {
    //     if (!originColor.TryGetValue(target, out Color originalColor))
    //     {
    //         var renderers = GetRenderer(target);
    //         if (renderers.Length > 0)
    //         {
    //             originalColor = renderers[0].material.color;
    //             originColor[target] = originalColor;
    //         }
    //         else
    //         {
    //             originalColor = Color.white; // 기본값
    //         }
    //     }
    //     return originalColor;
    // }
    //
    // public void ClearCache(CharacterBase target)
    // {
    //     renderCache.Remove(target);
    //     originColor.Remove(target);
    // }
    //
    // public void ClearAllCache()
    // {
    //     renderCache.Clear();
    //     originColor.Clear();
    // }
}
