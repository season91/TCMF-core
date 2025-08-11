using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 기본 공격 및 스킬 사용하는 매니저
/// 공격, 스킬 등 실제 액션 실행 (데미지 처리, 이펙트 호출 포함 가능)
/// 유닛의 액션(기본 공격, 스킬 사용) 실행 담당
/// </summary>
public class ActionManager
{
    private Sequence currentAttackSequence;
    private Sequence currentSkillSequence;
    private bool isActionInProgress = false;

    private float moveSpeed => BattleConfig.Instance.moveSpeed;
    private float actionInterval => BattleConfig.Instance.actionInterval;
    private float offset => BattleConfig.Instance.meleeAttackOffset;
    public bool IsActionInProgress => isActionInProgress;
    
    private readonly IBattleServices battleServices;
    
    public ActionManager(IBattleServices services)
    {
        battleServices = services;
    }
    
    /// <summary>
    /// 유닛이 몬스터를 공격할 때  (수동 / 자동 모드 모두에서 사용)
    /// </summary>
    public void ExecuteBasicAttack(CharacterBase attacker, CharacterBase target)
    {
        if (attacker == null || target == null) return;
        
        // 기존 시퀀스 정리
        if (currentAttackSequence != null && currentAttackSequence.IsActive())
        {
            currentAttackSequence.Kill();
        }
        isActionInProgress = true;
        
        MyDebug.Log($"=== ExecuteBasicAttack 호출 ===");
        
        currentAttackSequence = DOTween.Sequence();
        
        if (attacker.attackType == AttackType.Melee)
        { 
            ExecuteMeleeAttack(currentAttackSequence, attacker, target);
        }
        else
        {
            ExecuteRangeAttack(currentAttackSequence, attacker, target);
        }

        FinalizeSequence(currentAttackSequence);
    }

    private void ExecuteMeleeAttack(Sequence sequence, CharacterBase attacker, CharacterBase target)
    {
        Vector3 originPos = attacker.transform.position;
        Vector3 targetPos = target.transform.position;
        
        Move(sequence, attacker.transform,originPos, targetPos, true);
            
        sequence.AppendCallback(() =>
        {
            battleServices.Effects.SpawnAttackEffect(attacker, target);
            battleServices.AnimationController.HitAnimation(target);
            ApplyDamage(attacker, target);
            SoundManager.Instance.PlaySfx(StringAdrAudioSfx.AttackMelee);
        });
        sequence.AppendInterval(BattleConfig.Instance.basicAttackDelay);
        
        DefaultDeathAnimation(sequence, target);
        
        Move(sequence, attacker.transform,targetPos, originPos, false);
    }

    private void ExecuteRangeAttack(Sequence sequence, CharacterBase attacker, CharacterBase target)
    {
        sequence.AppendCallback(() =>
        {
            battleServices.Effects.LaunchProjectile(attacker, target);
            PlayRangeAttackSound(attacker);
        });
        sequence.AppendInterval(actionInterval);
    }
    
    /// <summary>
    /// 유닛이 스킬을 사용할 때 -수동 모드
    /// </summary>
    public void ExecuteSkill(Unit caster, Monster target)
    {
        if (caster == null) return;
        
        isActionInProgress = true;
        
        // 스킬 데이터 기반으로 실제 타겟들 구하기
        if (MasterData.SkillDataDict.TryGetValue(caster.UnitData.Code, out var skillData))
        {
            var manualTarget = new List<CharacterBase>();
            
            if (skillData.TargetType == SkillTargetType.Single)
            {
                // 단일 타겟 스킬 - 선택한 타겟만 사용
                if (target != null)
                {
                    manualTarget.Add(target);
                }
            }
            
            ExecuteSkillWithCutin(caster, target, manualTarget);
        }
    }
    
    /// <summary>
    /// 스킬 데이터 기반 자동 타겟팅으로 스킬 실행 - 자동 모드
    /// </summary>
    public void ExecuteSkill(CharacterBase caster)
    {
        if (caster == null) return;
        
        isActionInProgress = true;

        string skillCode = caster.GetCode();
        
        // SkillData 가져오기
        if (!MasterData.SkillDataDict.TryGetValue(skillCode, out var skillData))
        {
            MyDebug.LogWarning($"스킬 코드 {skillCode}를 찾을 수 없습니다.");
            isActionInProgress = false;
            return;
        }
        
        var targets = battleServices.Targeting.GetSkillTargets(
            skillData, battleServices.Units.Cast<CharacterBase>().ToList(),
            battleServices.Monsters.Cast<CharacterBase>().ToList());
        
        // 타겟 선택이 필요 없는 스킬 
        if (!caster.isSelectable)
        {
            // 스킬 컷인 코루틴에 스킬 발동 로직을 콜백으로 전달
            ExecuteSkillWithCutin(caster, null, targets, () =>
            {
                MyDebug.Log($"{caster.UnitName} 스킬을 사용함 (타겟 없음)");
                isActionInProgress = false;
            });
            return;
        }
        
        if (targets == null || targets.Count == 0)
        {
            MyDebug.LogWarning("스킬 타겟을 찾을 수 없습니다.");
            isActionInProgress = false;
            return;
        }
        
        // 첫 번째 타겟으로 기존 애니메이션 로직 실행
        var primaryTarget = targets.FirstOrDefault();
        ExecuteSkillWithCutin(caster, primaryTarget, targets);
    }

    /// <summary>
    /// 컷인과 함께 스킬을 실행하는 통합 메서드
    /// </summary>
    private void ExecuteSkillWithCutin(CharacterBase caster, CharacterBase target, List<CharacterBase> allTargets, Action onComplete = null)
    {
        if (caster is Unit unit)
        {
            unit.PlayMeow();
            // 유닛인 경우 컷인 실행 후 스킬 발동
            battleServices.StartCoroutine(
                UIManager.Instance.StartCutin(unit.UnitData.Code, () => 
                {
                    ExecuteSkillSequence(caster, target, allTargets, onComplete);
                })
            );
        }
        else
        {
            // 유닛이 아닌 경우 바로 스킬 발동
            ExecuteSkillSequence(caster, target, allTargets, onComplete);
        }
    }
    
    /// <summary>
    /// 실제 스킬 시퀀스 실행
    /// </summary>
    private void ExecuteSkillSequence(CharacterBase caster, CharacterBase target, List<CharacterBase> allTargets, Action onComplete)
    {
        // 타겟이 없는 스킬의 경우 바로 실행하고 종료
        if (!caster.isSelectable)
        {
            caster.UseSkill(allTargets);
            onComplete?.Invoke();
            return;
        }

        currentSkillSequence = DOTween.Sequence();

        if (caster.SkillData.SkillType == SkillType.Melee)
        {
            ExecuteMeleeSkill(caster, target, allTargets);
        }
        else if (caster.SkillData.SkillType == SkillType.Range)
        {
            ExecuteRangeSkill(caster, allTargets);
        }

        FinalizeSequence(currentSkillSequence);
    }

    /// <summary>
    /// 근접 기본공격 / 스킬 사용 시 타겟으로 이동하는 로직
    /// </summary>
    private void Move(Sequence sequence, Transform character, Vector3 origin , Vector3 target, bool go)
    {
        Vector3 adjustedTarget = target;
        if (go)
        {
            Vector3 direction = (target - origin).normalized;
            adjustedTarget = target - direction * offset;
        }
        
        float distance = Vector3.Distance(origin, adjustedTarget);
        float moveTime = distance / moveSpeed;
        sequence.Append(character.DOMove(adjustedTarget, moveTime).SetEase(Ease.Linear));
    }

    private void ExecuteMeleeSkill(CharacterBase caster, CharacterBase target, List<CharacterBase> allTargets)
    {
        Vector3 originPos = caster.transform.position;
        Vector3 targetPos = target?.transform.position ?? originPos;
        
        Move(currentSkillSequence, caster.transform, originPos, targetPos, true);

        currentSkillSequence.AppendCallback(() =>
        {
            caster.UseSkill(allTargets);
            battleServices.AnimationController.HitAnimation(target);
            //if (caster is Unit) battleServices.UI.UpdateSkillCoolGUI(battleServices.Units);
        });

        float delay = caster.GetSkillDelay();
        currentSkillSequence.AppendInterval(delay);
        DefaultDeathAnimation(currentSkillSequence, allTargets);
        
        Move(currentSkillSequence, caster.transform, targetPos, originPos, false);
    }
    private void ExecuteRangeSkill(CharacterBase caster, List<CharacterBase> allTargets)
    {
        currentSkillSequence.AppendCallback(() =>
        {
            caster.UseSkill(allTargets);
            //if (caster is Unit) battleServices.UI.UpdateSkillCoolGUI(battleServices.Units);
        });
        
        float delay = caster.GetSkillDelay();
        currentSkillSequence.AppendInterval(delay);
    }
    /// <summary>
    /// 보스의 전반적인 액션 ( 기본공격 + 스킬 )
    /// </summary>
    public void ExecuteBossAction(Monster monster)
    {
        if (!monster.isBoss || monster.currentStat[StatType.Hp] <= 0 || battleServices.Flow.CurrentState == BattleState.Lose) return;
        
        List<Unit> targets = battleServices.Targeting.GetAttackTargets(battleServices.Units.ToList(), monster);
        
        SetActionInProgress(true);
        
        Sequence bossTurnSequence = DOTween.Sequence();
        Vector3 originPos = monster.transform.position;
        
        if (monster.attackType == AttackType.Range)
        {
            ExecuteRangeBossAttack(bossTurnSequence, monster, targets);
        }
        else if (monster.attackType == AttackType.Melee)
        {
            ExecuteMeleeBossAttack(bossTurnSequence, monster, targets, originPos);
        }

        bossTurnSequence.AppendCallback(() =>
        {
            if (battleServices.Flow.IsAllPlayersDead())
            {
                SetActionInProgress(false);
                return;
            }

            // 체력 조건에 따라 스킬 발동 시도 (중복 발동 방지 포함)
            monster.ExecuteSkill();
            // 스킬이 사용되었다면 추가 대기
            if (monster.IsSkillUsed)
            {
                float delay = monster.GetSkillDelay();
                
                // 추가 대기 시퀀스 생성
                DOVirtual.DelayedCall(delay,() =>
                {
                    SetActionInProgress(false);
                }).SetAutoKill(true);
            }
            else
            {
                MyDebug.Log("보스 스킬 사용되지 않음 - 즉시 턴 종료");
                SetActionInProgress(false);
            }
        });
        
        bossTurnSequence.SetAutoKill(true);
    }
    
    private void ExecuteRangeBossAttack(Sequence sequence, Monster monster, List<Unit> targets)
    {
        // 보스 3의 스플릿 데미지 처리
        if (monster.MonsterData.Code == BossMonsterCode.Boss3)
        {
            SoundManager.Instance.PlaySfx(StringAdrAudioSfx.AttackMons0011);
            if (targets.Count > 0)
            {
                // 메인 타겟 (첫 번째)
                sequence.AppendCallback(() =>
                {
                    battleServices.Effects.LaunchBossProjectile(
                        monster, targets[0], isMainTarget: true);
                });
            
                // 스플릿 타겟들 (나머지)
                for (int i = 1; i < targets.Count; i++)
                {
                    int index = i;
                    sequence.AppendCallback(() =>
                    {
                        battleServices.Effects.LaunchBossProjectile(
                            monster, targets[index], isMainTarget: false);
                    });
                }
            }
        }
        else
        {
            foreach (var unit in targets)
            {
                sequence.AppendCallback(() =>
                {
                    battleServices.Effects.LaunchProjectile(monster, unit);
                });
            }
        }

        sequence.AppendInterval(BattleConfig.Instance.projectileDamageDelay);

        //BossAttackDeathAnimation(sequence, targets);
    }
    
    private void ExecuteMeleeBossAttack(Sequence sequence, Monster monster, List<Unit> targets, Vector3 originPos)
    {
        if (targets.Count == 0) return;
        
        Unit closestTarget = battleServices.Targeting.GetClosestTarget(targets, originPos);
        Vector3 attackPosition = closestTarget.transform.position;
    
        //  가장 가까운 타겟으로 이동
        Move(sequence, monster.transform, originPos, attackPosition, true);
    
        //  모든 타겟에게 동시 공격
        sequence.AppendCallback(() =>
        {
            foreach (var target in targets)
            {
                // 동시에 이펙트 스폰
                battleServices.Effects.SpawnAttackEffect(monster, target);
                battleServices.AnimationController.HitAnimation(target);
                SoundManager.Instance.PlaySfx(StringAdrAudioSfx.AttackMons0007);
                // 동시에 데미지 적용
                ApplyDamage(monster, target);
            }
        });
    
        //  공격 후 대기
        sequence.AppendInterval(BattleConfig.Instance.basicAttackDelay);

        BossAttackDeathAnimation(sequence, targets);
        
        Move(sequence, monster.transform, attackPosition, originPos, false);
        
        sequence.AppendInterval(actionInterval);
        
        sequence.AppendCallback(() =>
        {
            battleServices.CheckBattleEnd();
        });
    }
    
    private void FinalizeSequence(Sequence sequence)
    {
        sequence.AppendInterval(actionInterval);
        sequence.AppendCallback(() => battleServices.CheckBattleEnd());
        sequence.OnComplete(() => SetActionInProgress(false));
        sequence.SetAutoKill(true);
    }

    public void SetActionInProgress(bool value)
    {
        isActionInProgress = value;
    }
    
    public void ApplyDamage(CharacterBase attacker, CharacterBase target)
    {
        int damage = attacker.DecreasedDamage();
        
        MyDebug.Log($"{attacker.UnitName} => {target.UnitName}에게 기본공격 ({damage} 데미지)");
        target.TakeDamage(damage);

        attacker.ProcessAttackEffects(target);
    }
    public void ApplySplitDamage(CharacterBase attacker, CharacterBase target, bool isMainTarget)
    {
        // 보스 3 스플릿 데미지인 경우만 특별 처리
        if (attacker is Monster monster && monster.MonsterData.Code == BossMonsterCode.Boss3 && !isMainTarget)
        {
            // 스플릿 데미지 (10%)
            int baseDamage = attacker.DecreasedDamage();
            int splitDamage = Mathf.RoundToInt(baseDamage * BattleConfig.Instance.splitDamageRatio);
        
            MyDebug.Log($"{attacker.UnitName} => {target.UnitName}에게 스플릿 공격 ({splitDamage} 데미지)");
            target.TakePureDamage(splitDamage);
        }
        else
        {
            // 기존 로직 사용 (메인 타겟이거나 다른 몬스터)
            ApplyDamage(attacker, target);
        }
    }
    
    //SoundManager로 이동해야할듯
    private void PlayRangeAttackSound(CharacterBase attacker)
    {
        string soundKey = attacker is Unit ? StringAdrAudioSfx.AttackRange : StringAdrAudioSfx.AttackMonster;
        SoundManager.Instance.PlaySfx(soundKey);
    }
    #region  옮겨야 할 듯
    private void BossAttackDeathAnimation(Sequence sequence, List<Unit> targets)
    {
        sequence.AppendCallback(() =>
        {
            foreach (var target in targets)
            {
                if (target.currentStat[StatType.Hp] <= 0)
                {
                    battleServices.AnimationController.DeathAnimation(target);
                    if(target is Unit unit)
                        battleServices.UpdateHideDieUnitSkillButton(unit);
                }
            }
        });
    }
    private void DefaultDeathAnimation(Sequence sequence, CharacterBase target)
    {
        sequence.AppendCallback(() =>
        {
            if (target.currentStat[StatType.Hp] <= 0)
            {
                battleServices.AnimationController.DeathAnimation(target);
                if(target is Unit unit)
                    battleServices.UpdateHideDieUnitSkillButton(unit);
            }
        });
    }
    private void DefaultDeathAnimation(Sequence sequence, List<CharacterBase> allTargets)
    {
        sequence.AppendCallback(() =>
        {
            foreach (var target in allTargets)
            {
                if (target.currentStat[StatType.Hp] <= 0)
                {
                    battleServices.AnimationController.DeathAnimation(target);
                    if(target is Unit unit)
                        battleServices.UpdateHideDieUnitSkillButton(unit);
                }
            }
        });
    }

    #endregion
}
