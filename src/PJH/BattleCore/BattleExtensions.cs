using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class BattleExtensions
{
    private static readonly Dictionary<string, float> SkillDelay = new()
    {
        { PlayerUnitCode.Ruru, BattleConfig.Instance.ruruSkillDelay },
        { MonsterCode.HappySeedling, BattleConfig.Instance.ruruSkillDelay },
        { BossMonsterCode.Boss1,BattleConfig.Instance.hallucinationEffectDuration },
        { BossMonsterCode.Boss3, BattleConfig.Instance.hallucinationEffectDuration },
        { MonsterCode.GuardianOfSilence, BattleConfig.Instance.ruruSkillDelay }
    };
    
    public static float GetSkillDelay(this CharacterBase caster)
    {
        string code = caster switch
        {
            Unit unit => unit.UnitData.Code,
            Monster monster => monster.MonsterData.Code,
            _ => throw new Exception("잘못된 코드")
        };
        
        return SkillDelay.TryGetValue(code, out float delay) ? delay : BattleConfig.Instance.skillDelay;
    }
    
    public static string GetCode(this CharacterBase caster)
    {
        return caster switch
        {
            Unit unit => unit.UnitData.Code,
            Monster monster => monster.MonsterData.Code,
            _ => throw new ArgumentException("지원하지 않는 캐스터 타입입니다.")
        };
    }
    
    public static string GetSkillSfxKey(this CharacterBase caster)
    {
        string code = caster.GetCode(); 

        // 유닛 개별
        if (caster is Unit)
        {
            return code switch
            {
                PlayerUnitCode.Usher => StringAdrAudioSfx.UsherSkill,
                PlayerUnitCode.Momo  => StringAdrAudioSfx.MomoSkill,
                PlayerUnitCode.Ruru  => StringAdrAudioSfx.RuruSkill,
                _ => null
            };
        }
        return code switch
        {
            BossMonsterCode.Boss1 => StringAdrAudioSfx.SkillMons0003,
            BossMonsterCode.Boss2 => StringAdrAudioSfx.SkillMons0007,
            BossMonsterCode.Boss3 => StringAdrAudioSfx.SkillMons0011,
            MonsterCode.PurpleMushroom => StringAdrAudioSfx.SkillMons0004,
            MonsterCode.GreenMushroom  => StringAdrAudioSfx.SkillMons0005,
            MonsterCode.HappySeedling  => StringAdrAudioSfx.SkillMons0006,
            MonsterCode.GuardianOfSilence => StringAdrAudioSfx.SkillMons0008,
            MonsterCode.DontCryingDeer => StringAdrAudioSfx.SkillMons0009,
            MonsterCode.SilentHummingbird => StringAdrAudioSfx.SkillMons0010,
            _ => null
        };
    }
    /// <summary>
    /// 스킬 사용 시 고양이 소리
    /// </summary>
    public static void PlayMeow(this Unit unit)
    {
        string code = unit.UnitData.Code switch
        {
            PlayerUnitCode.Usher => StringAdrAudioSfx.UsherMeow,
            PlayerUnitCode.Momo => StringAdrAudioSfx.MomoMeow,
            PlayerUnitCode.Ruru => StringAdrAudioSfx.RuruMeow,
            _ => throw new Exception("잘못된 코드입니다")
        };
            
        SoundManager.Instance.PlaySfx(code);
    }
    
    /// <summary>
    /// 기본공격에 특수효과 처리
    /// 현재는 몬스터 한종류만 적용 중
    /// </summary>
    public static void ProcessAttackEffects(this CharacterBase attacker, CharacterBase target)
    {
        if (attacker is Monster monster)
        {
            ProcessMonsterAttackEffects(monster, target);
        }
        //여기에 추가 예정
    }
    private static void ProcessMonsterAttackEffects(Monster monster, CharacterBase target)
    {
        switch (monster.MonsterData.Code)
        {
            case MonsterCode.PurpleMushroom:
                // 50% 확률로 2턴간 중독 (매턴 10 데미지)
                if (Random.Range(0, 100) < BattleConfig.Instance.poisonProbability)
                {
                    target.ApplyStatusEffect(StatusEffectType.Poison, 2, 10f);
                    MyDebug.Log($"{target.UnitName}이 독에 중독됨!");
                    SoundManager.Instance.PlaySfx(StringAdrAudioSfx.SkillMons0004);
                }
                break;
        }
    }
}
