using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
/// <summary>
/// 턴 기반 전투에서 턴 순서와 턴 전환 로직을 관리
/// 턴 순서, 턴 상태, 턴 흐름 관리
/// 턴 순서 및 상태 전환을 제어함. 자동/수동 모드에 따라 다른 매니저 호출
/// </summary>

public enum TurnPhase
{
    UnitTurn,
    MonsterTurn,
}
public class TurnManager : MonoBehaviour
{
    private IBattleServices battleServices;
    
    public int currentUnitIndex = 0;
    private int currentMonsterIndex = 0;
    private int roundCount = 1;
    
    public TurnPhase currentPhase;
    
    private WaitUntil waitForActionComplete;
    private WaitForSeconds turnDelay;
    private WaitForSeconds roundStartDelay;
    public int Round => roundCount;
    
    public bool isSkillUsed = false;

    // 초기화
    public void Initialize(IBattleServices services)
    {
        battleServices = services;
        currentUnitIndex = 0;
        currentMonsterIndex = 0;
        roundCount = 1;
        waitForActionComplete = new WaitUntil(() => !battleServices.Actions.IsActionInProgress());
        turnDelay = new WaitForSeconds(BattleConfig.Instance.turnDelaySeconds);
        roundStartDelay = new WaitForSeconds(BattleConfig.Instance.roundStartDelay);
    }
    
    public void StartTurnLoop()
    {
        currentPhase = TurnPhase.UnitTurn;
        StartCoroutine(TurnRoutine());
    }
    
    private IEnumerator TurnRoutine()
    {
        // 배틀매니저가 전투 끝났는지 확인
        while (!battleServices.Flow.IsBattleOver())
        {
            yield return turnDelay;

            switch (currentPhase)
            {
                case TurnPhase.UnitTurn:
                    yield return HandleUnitTurn();
                    break;
                case TurnPhase.MonsterTurn:
                    yield return HandleMonsterTurn();
                    break;
            }
        }
    }

    private IEnumerator HandleUnitTurn()
    {
        IReadOnlyList<Unit> battleUnits = battleServices.Units;
        
        if (currentUnitIndex == 0)
        {
            MyDebug.Log($"{roundCount}번째 라운드 시작");
            battleServices.UI.UpdateStageInfo(roundCount);
            battleServices.UI.UpdateSkillCoolGUI(battleUnits);
            battleServices.UI.UpdateRound(null);
            yield return roundStartDelay;
        }
        // 현재 턴 유닛 인덱스 유효성 검사
        while (currentUnitIndex < battleUnits.Count && battleUnits[currentUnitIndex].currentStat[StatType.Hp] <= 0)
            currentUnitIndex++;

        // 턴 종료 조건 (모든 유닛 턴 끝)
        if (currentUnitIndex >= battleUnits.Count)
        {
            foreach (var unit in battleUnits)
            {
                if (unit.currentStat[StatType.Hp] > 0)
                {
                    unit.ReduceStatusEffectDuration();
                }
            }
            currentUnitIndex = 0;
            currentPhase = TurnPhase.MonsterTurn;
            yield break;
        }
        Unit battleUnit = battleUnits[currentUnitIndex];
        battleServices.UI.UpdateMarkAsCurrentEntity(battleUnit.GetUpperCenter()); // 턴 마크 표시
        
        yield return turnDelay;
        // 기절 상태면 턴 스킵
        if (battleUnit.HasStatusEffect(StatusEffectType.Stun))
        {
            MyDebug.Log($"{battleUnit.UnitName}은 기절 상태로 턴 스킵");
            battleUnit.ReduceSkillCooldown();
            currentUnitIndex++;
            yield break;
        }
        
        isSkillUsed = false;
        
        // <수동 모드> 기본 공격은 즉시 실행
        Monster defaultTarget = battleServices.Targeting.SelectMonsterTarget();
        SoundManager.Instance.PlayRandomUnitAttackSfx();
        battleServices.Actions.ExecuteBasicAttack(battleUnit, defaultTarget);

        yield return WaitForActionComplete();
        // 승리 체크
        if (battleServices.Flow.CurrentState == BattleState.Win
            || battleServices.Flow.CurrentState == BattleState.Interrupted) yield break;
        
        if (battleServices.Flow.CurrentMode == BattleMode.Auto)
        {
            if (battleUnit.CanUseSkill())
            {
                // battleServices.UI.UpdateMarkAsCurrentSkill(currentUnitIndex); // 스킬 사용 가능 표시
                battleServices.Actions.ExecuteSkill(battleUnit);
                battleServices.UI.EndUseSkillWaitingGUI(currentUnitIndex, true);
                isSkillUsed = true;
            }
            
            if (!isSkillUsed)
            {
                battleUnit.ReduceSkillCooldown();
            }
            
            yield return WaitForActionComplete();
            currentUnitIndex++;
            yield break;
        }
        
        if (battleUnit.CanUseSkill())
        {
            // battleServices.UI.UpdateMarkAsCurrentSkill(currentUnitIndex); // 스킬 사용 가능 표시
            yield return battleServices.Input.WaitForSkillInput(battleUnit, BattleConfig.Instance.skillWaitTime);
        }
        if (!isSkillUsed)
        {
            battleUnit.ReduceSkillCooldown();
        }
        
        yield return WaitForActionComplete();
        currentUnitIndex++;
    }
    
    /// <summary>
    /// 살아있는 몬스터 순서대로 기본 공격을 수행하고 턴 전환
    /// 몬스터 턴: 일반 or 보스에 따라 행동 다르게 처리
    /// </summary>
    private IEnumerator HandleMonsterTurn()
    {
        var monsters = battleServices.Monsters;
        
        if (currentMonsterIndex == 0)
        {
            MyDebug.Log("적 턴 시작");
            yield return roundStartDelay;
        }
        // 죽은 몬스터 스킵
        while (currentMonsterIndex < monsters.Count && monsters[currentMonsterIndex].currentStat[StatType.Hp] <= 0)
            currentMonsterIndex++;

        if (currentMonsterIndex >= monsters.Count)
        {
            MyDebug.Log($"{roundCount}번째 라운드 종료 - 플레이어 턴으로 전환");
            foreach (var mon in monsters)
            {
                if (mon.currentStat[StatType.Hp] > 0)
                {
                    // 일반 몬스터만 스킬 쿨타임 감소 (보스는 체력 기반이므로 제외)
                    if (!mon.isBoss)
                    {
                        mon.ReduceSkillCooldown();
                    }
                }
            }
            roundCount++;
            currentPhase = TurnPhase.UnitTurn;
            currentMonsterIndex = 0;
            yield break;
        }
        Monster monster = monsters[currentMonsterIndex];
        battleServices.UI.UpdateMarkAsCurrentEntity(monster.GetUpperCenter()); // 턴 마크 표시
        
        yield return turnDelay;
        
        if (monster.isBoss)
        {
            // 보스는 자신의 턴 직접 처리 (내부적으로 ActionManager 호출 가능)
            battleServices.Actions.ExecuteBossAttack(monster);
            yield return WaitForActionComplete();
        }
        else
        {
            Unit target = battleServices.Targeting.SelectUnitTarget();
            if (target != null)
                battleServices.Actions.ExecuteBasicAttack(monster, target);
            
            yield return WaitForActionComplete();
            
            if(battleServices.Flow.CurrentState == BattleState.Lose||
               battleServices.Flow.CurrentState == BattleState.Interrupted) yield break;

            if (monster.CanUseSkill())
            {
                battleServices.Actions.ExecuteSkill(monster);
                yield return WaitForActionComplete();
            }
        }

        currentMonsterIndex++;
    }
    
    private IEnumerator WaitForActionComplete()
    {
        yield return waitForActionComplete;
    }
}
