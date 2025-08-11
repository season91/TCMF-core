using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// 데이터 저장 X
/// SO 
/// </summary>

public enum BossAttackPattern
{
    Sequential,  // 전열 -> 중열 -> 후열 순차
    MultiTarget, // 랜덤 2마리 동시
    SplitDamage
}

public class Monster : CharacterBase, IClickable
{
    public MonsterData MonsterData;
    public bool isBoss; // 보스 여부
    
    public BossAttackPattern bossAttackPattern;// 보스의 공격 패턴 타입
    private List<float> usedSkillTriggers = new List<float>(); // 보스 스킬 발동 트리거 기록

    private bool isSkillUsed;
    public bool IsSkillUsed => isSkillUsed;

    #region 일반, 보스몬스터 / 스킬 초기화
    
    public void InitializeFromStage(string monsterCode)
    {
        InitData(monsterCode);
        InitStats();
        InitBasicStatAndSkill();
        statusEffectController.BackupStats();
        InitVisual(MonsterData.Code);
        
        usedSkillTriggers.Clear();
    }
    private void InitData(string monsterCode)
    {
        if (MasterData.MonsterDataDict.TryGetValue(monsterCode, out MonsterData data))
        {
            MonsterData = data;
        }
        
        SkillData = MasterData.SkillDataDict.Values
                                  .FirstOrDefault(skill => skill.EntityCode == monsterCode);
    }

    private void InitBasicStatAndSkill()
    {
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        UnitName = MonsterData.Name;
        attackType = MonsterData.AtkType;
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            int value = GetStatFromUnitData(statType);
            baseStat[statType] = value;
        }
        
        statController.CopyBaseToCurrent();
        
        isBoss = MonsterData.IsBoss;

        if (SkillData != null)
        {
            if (isBoss)
                bossAttackPattern = BattleManager.Instance.GetBossAttackPattern(MonsterData.Code);
            
            skillName = SkillData.Name;
            skillCooldown = SkillData.Cooldown;
            skillDescription = SkillData.Description;
            currentSkillCooldown = 0;
            isSelectable = SkillData.IsSelectable;
            skillType = SkillData.SkillType;
        }
        
        MyDebug.Log("monster UnitName " + UnitName);
    }
    
    /// <summary>
    /// StatType value 조회
    /// </summary>
    private int GetStatFromUnitData(StatType type)
    {
        switch (type)
        {
            case StatType.Hp :
                return MonsterData.Hp;
            case StatType.Atk :
                return MonsterData.Atk;
            case StatType.Def :
                return MonsterData.Def;
            case StatType.Evasion :
                return 0;
        }

        return 0;
    }
    
    #endregion

    public override bool CanUseSkill()
    {
        if(SkillData == null) return false;

        if (isBoss)
        {
            float hpPercent = (float)currentStat[StatType.Hp] / baseStat[StatType.Hp] * 100f;
            return BattleConfig.Instance.bossSkillHealthTriggers.OrderBy(x=>x)
                .Any(trigger => hpPercent <= trigger && !usedSkillTriggers.Contains(trigger));
        }

        return (IsSkillReady && !HasStatusEffect(StatusEffectType.Silence));
    }

    /// <summary>
    /// 보스 스킬 체력 70퍼에 한번 발동 , 50퍼에 한번 발동, 30퍼에 한번 발동
    /// 한번에 30퍼로 깎일시 30퍼 한 번만 발동
    /// </summary>
    public override void ExecuteSkill(List<CharacterBase> targets = null)
    {
        isSkillUsed = false;
        if (!CanUseSkill()) return;
        if (SkillData == null)
        {
            MyDebug.LogWarning($"스킬 데이터를 찾을 수 없습니다: {MonsterData.Code}");
            return;
        }
        
        if (isBoss)
        {
            ExecuteBossSkill();
        }
        else
        {
            ExecuteMonsterSkill(targets);
        }
    }

    private void ExecuteBossSkill()
    {
        float hpPercent = (float)currentStat[StatType.Hp] / baseStat[StatType.Hp] * 100f;
        
         // 타겟 선택
         var monsters = battleServices.Monsters.Cast<CharacterBase>().ToList();
         var units = battleServices.Units.Cast<CharacterBase>().ToList();
         var targets = battleServices.Targeting.GetSkillTargets(
             SkillData, units, monsters);
         
        foreach (float trigger in BattleConfig.Instance.bossSkillHealthTriggers.OrderBy(x=>x))
        {
            if (hpPercent <= trigger && !usedSkillTriggers.Contains(trigger))
            {
                CharacterBase effectTarget = (targets != null && targets.Count > 0) ? targets[0] : this;
                usedSkillTriggers.Add(trigger);
                MyDebug.Log($"{UnitName} 체력 {hpPercent:F1}% 스킬 발동");
                
                Sequence skillSequence = DOTween.Sequence();
                isSkillUsed = true;

                if (MonsterData.Code == BossMonsterCode.Boss1 || MonsterData.Code == BossMonsterCode.Boss3)
                {
                    // 스킬 이펙트 먼저 스폰
                    skillSequence.AppendCallback(() =>
                    {
                        MyDebug.Log("보스 스킬 이펙트 스폰 시도");
                    
                        battleServices?.Effects.SpawnSkillEffect(this, effectTarget);
                    });
            
                    // 이펙트 지속시간만큼 대기
                    skillSequence.AppendInterval(BattleConfig.Instance.hallucinationEffectDuration);
                }
                
                // 스킬 실행
                skillSequence.AppendCallback(() =>
                {
                    skillExecutor.ExecuteSkill(MonsterData.Code, this, targets);
                    MyDebug.Log($"보스 스킬 적용 완료");
                });
                
                skillSequence.SetAutoKill(true);
                break;
            }
        }
    }
    private void ExecuteMonsterSkill(List<CharacterBase> targets)
    {
        MyDebug.Log($"{MonsterData.Name} 스킬 사용");
        isSkillUsed = true;
        skillExecutor.ExecuteSkill(MonsterData.Code, this, targets);
    }
    public void OnClick()
    {
        battleServices.Input.OnMonsterClicked(this);
    }
}

