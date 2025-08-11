using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ProjectileLauncher
{
    private IBattleServices battleServices;

    public ProjectileLauncher(IBattleServices services)
    {
        battleServices = services;
    }
    
    /// <summary>
    /// 원거리 기본공격 투사체 발사용
    /// </summary>
    public void LaunchProjectile(CharacterBase attacker, CharacterBase target)
    {
        if (target.currentStat[StatType.Hp] <= 0) return;
        
        GameObject projectile = battleServices.Effects.SpawnAttackEffect(attacker, target);

        Vector3 endPos = target.GetTargetPoint() + Vector3.up * BattleConfig.Instance.projectileHeightOffset;

        projectile.transform.DOMove(endPos, BattleConfig.Instance.projectileMoveTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                battleServices.AnimationController.HitAnimation(target);
                battleServices.ApplyDamage(attacker, target);

                if (target.currentStat[StatType.Hp] <= 0)
                {
                    battleServices.AnimationController.DeathAnimation(target);
                    if(target is Unit unit)
                        battleServices.UpdateHideDieUnitSkillButton(unit);
                }
                
                DOVirtual.DelayedCall(BattleConfig.Instance.turnTransitionDelay, () =>
                {
                    battleServices.CheckBattleEnd();
                });
            });
    }
    /// <summary>
    /// 보스의 멀티 타겟용 투사체 발사 메서드
    /// </summary>
    public void LaunchBossProjectile(CharacterBase attacker, CharacterBase target, bool isMainTarget = true)
    {
        if (target.currentStat[StatType.Hp] <= 0) return;
    
        GameObject projectile = battleServices.Effects.SpawnAttackEffect(attacker, target);

        // 보스 3 스플릿 데미지인 경우 크기 조정
        if (attacker is Monster monster && monster.MonsterData.Code == BossMonsterCode.Boss3)
        {
            float scale = isMainTarget ? BattleConfig.Instance.mainTargetProjectileScale : 
                BattleConfig.Instance.splitTargetProjectileScale;
            projectile.transform.localScale = Vector3.one * scale;
        }

        Vector3 endPos = target.GetTargetPoint() + Vector3.up * BattleConfig.Instance.projectileHeightOffset;

        projectile.transform.DOMove(endPos, BattleConfig.Instance.projectileMoveTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                battleServices.AnimationController.HitAnimation(target);
            
                // 스플릿 데미지 적용
                battleServices.ApplySplitDamage(attacker, target, isMainTarget);
            
                if (target.currentStat[StatType.Hp] <= 0)
                {
                    battleServices.AnimationController.DeathAnimation(target);
                    if(target is Unit unit)
                        battleServices.UpdateHideDieUnitSkillButton(unit);
                }
                DOVirtual.DelayedCall(BattleConfig.Instance.turnTransitionDelay, () =>
                {
                    battleServices.CheckBattleEnd();
                });
            });
    }
    
    /// <summary>
    /// 원거리 스킬 투사체 발사용
    /// </summary>
    public void LaunchSkillProjectile(CharacterBase caster, CharacterBase target)
    {
        if (target.currentStat[StatType.Hp] <= 0) return;
    
        // 스킬 이펙트로 투사체 생성
        GameObject projectile = battleServices.Effects.SpawnSkillEffect(caster, target);

        Vector3 endPos = target.transform.position + Vector3.up;

        projectile.transform.DOMove(endPos, BattleConfig.Instance.projectileMoveTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                battleServices.AnimationController.HitAnimation(target);

                if (target.currentStat[StatType.Hp] <= 0)
                {
                    battleServices.AnimationController.DeathAnimation(target);
                    if(target is Unit unit)
                        battleServices.UpdateHideDieUnitSkillButton(unit);
                }
                
                DOVirtual.DelayedCall(BattleConfig.Instance.turnTransitionDelay, () =>
                {
                    battleServices.CheckBattleEnd();
                });
            });
    }
}

