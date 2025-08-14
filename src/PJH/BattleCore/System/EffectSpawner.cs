using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 이펙트의 실제 배치와 시각적 설정을 담당하는 스포너
/// 
/// 주요 역할:
/// 1. 이펙트 타입 결정 및 매핑 (캐릭터 코드 → 이펙트 타입)
/// 2. 이펙트 위치, 회전, 크기 계산 및 적용
/// 3. 캐릭터별/상황별 특수 위치 조정 (보스, 근접/원거리 등)
/// 4. EffectProvider와 연동하여 실제 이펙트 생성
/// </summary>
public class EffectSpawner : MonoBehaviour
{
    // 스킬 이펙트의 배치 정보를 담는 구조체
    private struct SkillEffectData
    {
        public SkillEffectType EffectType { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }
    private EffectProvider effectProvider;

    //기본 위치 조정 값
    private Vector3 offset => BattleConfig.Instance.effectOffset;

    private static readonly Dictionary<string, SkillEffectType> SkillEffect = new() // 캐릭터 코드와 스킬 이펙트 타입을 매핑
    {
        { PlayerUnitCode.Usher, SkillEffectType.Usher },
        { PlayerUnitCode.Momo, SkillEffectType.Momo },
        { PlayerUnitCode.Ruru, SkillEffectType.Ruru },
        { MonsterCode.GreenMushroom, SkillEffectType.GreenMushroom },
        { MonsterCode.HappySeedling, SkillEffectType.HappySeedling },
        { MonsterCode.GuardianOfSilence, SkillEffectType.GuardianOfSilence },
        { MonsterCode.DontCryingDeer, SkillEffectType.DontCryingDeer },
        { MonsterCode.SilentHummingbird, SkillEffectType.SilentHummingbird },
        { BossMonsterCode.Boss1, SkillEffectType.Boss1 },
        { BossMonsterCode.Boss2, SkillEffectType.Boss2 },
        { BossMonsterCode.Boss3, SkillEffectType.Boss3 }
    };
    
    private static readonly Dictionary<string, AttackEffectType> AttackEffect = new() // 캐릭터 코드와 기본 공격 이펙트 타입을 매핑
    {
        { PlayerUnitCode.Usher, AttackEffectType.Melee },
        { PlayerUnitCode.Momo, AttackEffectType.Range },
        { PlayerUnitCode.Ruru, AttackEffectType.Melee },
        { MonsterCode.Cocoon, AttackEffectType.MonsterRange },
        { MonsterCode.WingedCocoon, AttackEffectType.MonsterRange },
        { MonsterCode.GreenMushroom, AttackEffectType.MonsterRange },
        { MonsterCode.PurpleMushroom, AttackEffectType.PurpleMushroom },
        { MonsterCode.HappySeedling, AttackEffectType.HappySeedling },
        { MonsterCode.GuardianOfSilence, AttackEffectType.MonsterRange },
        { MonsterCode.DontCryingDeer, AttackEffectType.DontCryingDeer },
        { MonsterCode.SilentHummingbird, AttackEffectType.MonsterRange },
        { BossMonsterCode.Boss1, AttackEffectType.Boss1 },
        { BossMonsterCode.Boss2, AttackEffectType.Boss2 },
        { BossMonsterCode.Boss3, AttackEffectType.Boss3 },
    };
    
    private void Start()
    {
        effectProvider = GetComponent<EffectProvider>();
    }
    
    /// <summary>
    /// 리팩토링 예정
    /// REFACTOR : 현재 하나의 메서드에 모든 로직이 집중됨, 공격 타입별 별도 메서드로 분리 필요
    /// </summary>
    public GameObject SpawnAttackEffect(CharacterBase attacker, CharacterBase target)
    {
        AttackEffectType effectType = GetAttackEffectType(attacker);
        Vector3 effectPosition;
        Quaternion effectRotation;
        Vector3 effectScale = Vector3.one;

        if (attacker.attackType == AttackType.Melee)
        {
            if (attacker is Unit)
            {
                // 플레이어 유닛의 근접 공격
                effectPosition = target.transform.position + offset + BattleConfig.Instance.forwardOffset;
                effectRotation = Quaternion.Euler(BattleConfig.Instance.meleeAttackEffectRotation);
            }
            else
            {
                // 몬스터의 근접 공격 
                effectPosition = target.transform.position + BattleConfig.Instance.monsterAttackHeightOffset
                                                           + BattleConfig.Instance.forwardOffset;
                effectRotation = Quaternion.Euler(BattleConfig.Instance.meleeAttackEffectRotation);
            }
            
            if (target is Monster monster)
            {
                //피격자가 보스인 경우
                if (monster.isBoss)
                {
                    effectPosition = target.transform.position + BattleConfig.Instance.bossEffectOffset;
                    effectRotation = Quaternion.Euler(BattleConfig.Instance.meleeAttackEffectRotation);
                    effectScale = Vector3.one * BattleConfig.Instance.effectScaleTargetBoss;
                }
            }
        }
        else
        {
            // === 원거리 공격 처리 ===
            // 일반적인 원거리 공격: 공격자 위치에서 시작
            effectPosition = attacker.GetTargetPoint() + Vector3.up * BattleConfig.Instance.projectileHeightOffset; 
            effectRotation = Quaternion.LookRotation(target.transform.position - attacker.transform.position);

            if (attacker is Monster monster && monster.MonsterData.Code == BossMonsterCode.Boss3)
            {
                effectPosition = attacker.GetTopPoint();
                effectRotation = Quaternion.LookRotation(target.transform.position - attacker.transform.position);
            }
        }

        GameObject effect = effectProvider.SpawnAttackEffect(effectType, effectPosition, effectRotation);

        effect.transform.localScale = effectScale;

        return effect;
    }

    /// <summary>
    /// 공격하는 캐릭터의 코드를 기반으로 공격 이펙트 타입을 반환
    /// </summary>
    private AttackEffectType GetAttackEffectType(CharacterBase character)
    {
        string code = character switch
        {
            Unit unit => unit.UnitData.Code,
            Monster monster => monster.MonsterData.Code,
            _ => throw new Exception("유닛 혹은 몬스터 코드가 없습니다")
        };
        
        if (AttackEffect.TryGetValue(code, out var attackEffectType))
            return attackEffectType;
        
        throw new Exception("기본공격 이펙트를 찾을 수 없습니다.");
    }

    /// <summary>
    /// 스킬 이펙트 생성 메서드
    /// </summary>
    public GameObject SpawnSkillEffect(CharacterBase caster, CharacterBase target)
    {
        MyDebug.Log($"=== SpawnSkillEffect 호출 ===");
        MyDebug.Log($"Caster: {caster.UnitName}, Target: {target.UnitName}");
        
        var effectData = GetSkillEffectData(caster, target);
        
        GameObject effect = effectProvider.SpawnSkillEffect(effectData.EffectType, effectData.Position, effectData.Rotation);
        effect.transform.localScale = effectData.Scale;
        return effect;
    }
    
    /// <summary>
    /// 캐릭터 타입에 따라 스킬 이펙트 데이터 계산
    /// </summary>
    private SkillEffectData GetSkillEffectData(CharacterBase caster, CharacterBase target)
    {
        var effectType = GetSkillEffectType(caster);
    
        return caster switch
        {
            Unit unit => GetUnitSkillEffectData(unit, target, effectType),
            Monster monster => GetMonsterSkillEffectData(monster, target, effectType),
            _ => GetDefaultSkillEffectData(target)
        };
    }
    
    /// <summary>
    /// 스킬을 사용하는 캐릭터의 코드를 기반으로 스킬 이펙트 타입을 반환
    /// </summary>
    private SkillEffectType GetSkillEffectType(CharacterBase character)
    {
        string code = character switch
        {
            Unit unit => unit.UnitData.Code,
            Monster monster => monster.MonsterData.Code,
            _ => throw new Exception("유닛 혹은 몬스터 코드가 없습니다")
        };
        
        if (SkillEffect.TryGetValue(code, out var skillType))
            return skillType;
        
        throw new Exception("스킬 이펙트를 찾을 수 없습니다.");
    }
    
    /// <summary>
    /// 플레이어 유닛의 스킬 이펙트 데이터를 계산하는 메서드
    /// </summary>
    private SkillEffectData GetUnitSkillEffectData(Unit unit, CharacterBase target, SkillEffectType effectType)
    {
        return unit.UnitData.Code switch
        {              
            PlayerUnitCode.Usher => new SkillEffectData
            {
                EffectType = effectType,
                // 보스 타겟 여부에 따른 위치 조정
                Position = target is Monster { isBoss: true }
                    ? target.transform.position + BattleConfig.Instance.usherSkillBossOffset  // 보스용 오프셋
                    : target.transform.position + offset,  // 일반 오프셋
                Rotation = Quaternion.Euler(BattleConfig.Instance.usherSkillEffectRotation),
                // 보스 타겟 여부에 따른 크기 조정
                Scale = Vector3.one * (BattleConfig.Instance.usherSkillEffectScale 
                                       * (target is Monster { isBoss: true } ? 
                    BattleConfig.Instance.usherSkillBossScaleMultiplier : 1f))
            },
            PlayerUnitCode.Ruru => new SkillEffectData
            {
                EffectType = effectType,
                Position = target.transform.position + offset + Vector3.up,
                Rotation = Quaternion.Euler(BattleConfig.Instance.ruruSkillEffectRotation),
                Scale = Vector3.one * BattleConfig.Instance.ruruSkillEffectScale
            },
            PlayerUnitCode.Momo => new SkillEffectData()
            {
                EffectType = effectType,
                Position = target.transform.position,
                Rotation = Quaternion.Euler(BattleConfig.Instance.statusEffectEuler),
                Scale = Vector3.one
            },
            _ => new SkillEffectData
            {
                EffectType = effectType,
                Position = target.transform.position + offset,
                Rotation = Quaternion.Euler(BattleConfig.Instance.ruruSkillEffectRotation),
                Scale = Vector3.one * BattleConfig.Instance.ruruSkillEffectScale
            }
        };
    }
    
    /// <summary>
    /// 몬스터의 스킬 이펙트 데이터를 계산하는 메서드
    /// </summary>
    private SkillEffectData GetMonsterSkillEffectData(Monster monster, CharacterBase target, SkillEffectType effectType)
    {
        if (monster.isBoss)
        {
            return monster.MonsterData.Code switch
            {
                BossMonsterCode.Boss1 => new SkillEffectData
                {
                    EffectType = effectType,
                    Position = monster.transform.position + Vector3.up * BattleConfig.Instance.bossSkillEffectHeight,
                    Rotation = Quaternion.Euler(BattleConfig.Instance.bossSkillEffectRotation),
                    Scale = Vector3.one
                },
                BossMonsterCode.Boss2 => new SkillEffectData
                {
                    EffectType = effectType,
                    Position = target.transform.position,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one
                },
                BossMonsterCode.Boss3 => new SkillEffectData
                {
                    EffectType = effectType,
                    Position = monster.transform.position + Vector3.up * BattleConfig.Instance.bossSkillEffectHeight
                                + BattleConfig.Instance.boss3SkillOffset,
                    Rotation = Quaternion.Euler(BattleConfig.Instance.boss3SkillEuler),
                    Scale = Vector3.one * BattleConfig.Instance.boss3SkillScale
                },
                _ => GetDefaultSkillEffectData(target)
            };
        }

        return monster.MonsterData.Code switch
        {
            MonsterCode.GreenMushroom 
                or MonsterCode.GuardianOfSilence => new SkillEffectData
            {
                EffectType = effectType,
                Position = monster.MonsterData.Code == MonsterCode.GreenMushroom
                ? target.GetTopPoint()
                : target.transform.position,
                Rotation = Quaternion.Euler(BattleConfig.Instance.statusEffectEuler),
                Scale = Vector3.one
            },
            MonsterCode.HappySeedling => new SkillEffectData
            {
                EffectType = effectType,
                Position = monster.transform.position,
                Rotation = Quaternion.identity,
                Scale = BattleConfig.Instance.happySeedlingSkillScale,
            },
            MonsterCode.DontCryingDeer or MonsterCode.SilentHummingbird => new SkillEffectData
            {
              EffectType = effectType,
              Position = target.GetTargetPoint(),
              Rotation = Quaternion.identity,
              Scale = monster.MonsterData.Code == MonsterCode.DontCryingDeer 
                  ? Vector3.one * BattleConfig.Instance.dontCryingDeerSkillScale
                  : Vector3.one
            },
            _ => GetDefaultSkillEffectData(target)
        };
    }

    /// <summary>
    /// 예외처리용 스킬이펙트 데이터
    /// </summary>
    private SkillEffectData GetDefaultSkillEffectData(CharacterBase target)
    {
        return new SkillEffectData
        {
            EffectType = SkillEffectType.Usher,
            Position = Vector3.zero,
            Rotation = Quaternion.identity,
            Scale = Vector3.one
        };
    }
    
    /// <summary>
    /// 상태효과 이펙트 생성
    /// </summary>
    public GameObject SpawnStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType statusEffectType)
    {
        Vector3 effectPosition = target.transform.position;
        Quaternion effectRotation = Quaternion.Euler(BattleConfig.Instance.statusEffectEuler);

        if (statusEffectType == StatusEffectType.Stun)
        {
            effectPosition =  target.GetTopPoint();
        }
        
        return effectProvider.SpawnStatusEffect(statusEffectType, effectPosition, effectRotation);
    }
}
