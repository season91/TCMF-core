using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 데이터 저장 X
/// SO
/// </summary>
public class Unit : CharacterBase
{
    public UnitData UnitData;
    public string UnitUid;
    /// <summary>
    /// 데이터 불러오기
    /// </summary>
    // 찾아야됨 어떤 유닛을 꺼낼건지
    // key가 UnitDataCode니까
    // key가 일치하면 해당 데이터를 넣어주는 방향으로 코드 수정필요
        
    // MasterData에서 가져온다. XXX 
    // ==> UnitInventory에서 유닛 가져오기
    // ==> 그곳에서 장착하고 있는 아이템에 따라 능력치도 가져오기
    // ==> 따라서 Unit Inventory에 있는 정보를 이 Unit.cs에 적용하는 느낌
    // ==> 이 Unit.cs는 배틀 Scene에만 있는 것이라 생각하면 되겠음.
    // ==> 유닛 선택 씬에서의 Unit.cs 또한, 마찬가지로 가져올 예정임
    // ==> 벌개의 cs라고 생각하면 됨.
    
    
    #region 유닛, 스킬 초기화
    
    public void InitializeFromInventory(InventoryUnit inventoryUnit)
    {
        InitData(inventoryUnit);
        InitStats();
        InitBasicStatAndSkill(inventoryUnit);
        statusEffectController.BackupStats();
        InitVisual(UnitData.Code);
    }
    private void InitData(InventoryUnit inventoryUnit)
    {
        if (inventoryUnit == null)
        {
            MyDebug.LogWarning("유닛 초기화에 InventoryUnit 누락됨");
            return;
        }

        // User Inventory Unit의 index로 설정
        string unitCode = inventoryUnit.UnitCode;
        UnitUid = inventoryUnit.UnitUid;
        
        MyDebug.Log("유닛 InitData !! UnitUid " + UnitUid);
        
        if (MasterData.UnitDataDict.TryGetValue(unitCode, out UnitData data))
        {
            UnitData = data;
        }
        
        SkillData = MasterData.SkillDataDict.Values
                              .FirstOrDefault(skill => skill.EntityCode == unitCode);
    }

    // InventoryUnit 인벤토리에 있는 정보대로 초기화 하는 방식 --> InitStats 이후에 호출되어야 함
    private void InitBasicStatAndSkill(InventoryUnit inventoryUnit)
    {
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        // 기본 data 기준 data 설정
        UnitName = UnitData.Name;
        
        // tobe 아이템-기본,강화,돌파,유닛 다 적용된 버전 unitSumStats
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            switch (statType)
            {
                case StatType.Atk:
                    baseStat[statType] = inventoryUnit.UnitSumStats[StatType.Atk];
                    break;
                case StatType.Def:
                    baseStat[statType] = inventoryUnit.UnitSumStats[StatType.Def];
                    break;
                case StatType.Hp:
                    baseStat[statType] = inventoryUnit.UnitSumStats[StatType.Hp];
                    break;
                case StatType.Evasion:
                    baseStat[statType] = inventoryUnit.UnitSumStats[StatType.Evasion];
                    break;
            }
        }
        statController.CopyBaseToCurrent();

        // 원복용 백업stat
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            backupStat[statType] = currentStat[statType];
        }
        
        attackType = UnitData.AtkType;
        // 스킬 기본 data 기준으로 data 설정
        skillName = SkillData.Name;
        skillCooldown = SkillData.Cooldown ; 
        skillDescription = SkillData.Description;
        currentSkillCooldown = 0;
        isSelectable = SkillData.IsSelectable;
        skillType = SkillData.SkillType;
        
        statController.PrintStat();
        
       RemoveAllStatusEffects();
    }

    #endregion

    #region 스킬 사용 및 정보
    
    public override bool CanUseSkill()
    {
        return (IsSkillReady && !HasStatusEffect(StatusEffectType.Silence));
    }

    public override void ExecuteSkill(List<CharacterBase> targets = null)
    {
        if (SkillData == null) return;

        skillExecutor.ExecuteSkill(UnitData.Code, this, targets);
    }
    
    #endregion
    

    public override int DecreasedDamage()
    {
        return currentStat[StatType.Atk];
    }
}
