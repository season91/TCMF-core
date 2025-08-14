using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase
{
    UnitTurn,
    MonsterTurn,
}

/// <summary>
/// 턴 기반 전투에서 턴 순서와 턴 전환 로직을 관리
/// 턴 순서, 턴 상태, 턴 흐름 관리
/// 턴 순서 및 상태 전환을 제어함. 자동/수동 모드에 따른 다른 처리
/// </summary>
public class TurnManager : MonoBehaviour
{
    private IBattleServices battleServices;
    
    public int currentUnitIndex = 0;
    private int currentMonsterIndex = 0;
    private int roundCount = 1;
    
    public TurnPhase currentPhase;
    
    private WaitUntil waitForActionComplete; // 액션 완료 대기
    private WaitForSeconds turnDelay;
    private WaitForSeconds roundStartDelay;
    
    public bool isSkillUsed = false; //현재 턴에서 스킬 사용 여부

    /// <summary>
    /// 배틀 서비스 연결 및 초기 상태 설정
    /// </summary>
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
    
    /// <summary>
    /// 턴 루프 시작
    /// 전투가 시작될 때 호출되어 턴 기반 전투를 개시
    /// </summary>
    public void StartTurnLoop()
    {
        currentPhase = TurnPhase.UnitTurn;
        StartCoroutine(TurnRoutine());
    }
    
    /// <summary>
    /// 전투가 끝날 때까지 유닛 턴과 몬스터 턴을 반복 실행
    /// </summary>
    private IEnumerator TurnRoutine()
    {
        // 전투가 끝날 때까지 무한 루프
        while (!battleServices.Flow.IsBattleOver())
        {
            yield return turnDelay;

            switch (currentPhase)
            {
                case TurnPhase.UnitTurn:
                    if(IsEndBattle()) yield break;
                    yield return HandleUnitTurn();
                    break;
                case TurnPhase.MonsterTurn:
                    if(IsEndBattle()) yield break;
                    yield return HandleMonsterTurn();
                    break;
            }
        }
    }

    /// <summary>
    /// 유닛턴 전체 관리
    /// </summary>
    private IEnumerator HandleUnitTurn()
    {
        IReadOnlyList<Unit> battleUnits = battleServices.Units;
        
        if (IsNewRound())
        {
            yield return StartNewRound(battleUnits);
        }
        if (IsEndUnitPhase(battleUnits))
        {
            yield return EndUnitPhase(battleUnits);
            yield break;
        }
        
        Unit battleUnit = GetCurrentUnit(battleUnits);
        yield return ProcessUnitTurn(battleUnit); // 여기서 공격 + 스킬
    }

    #region HandleUnitTurn

    private bool IsNewRound() => currentUnitIndex == 0; // 새 라운드 시작

    /// <summary>
    /// UI 업데이트 및 스킬 쿨다운 감소
    /// </summary>
    private IEnumerator StartNewRound(IReadOnlyList<Unit> battleUnits)
    {
        MyDebug.Log($"{roundCount}번째 라운드 시작");
        battleServices.UI.UpdateStageInfo(roundCount); // 라운드 숫자 업데이트
        battleServices.UI.UpdateSkillCoolGUI(battleUnits); //스킬 쿨 감소
        battleServices.UI.UpdateRound(null); // 라운드 이미지 출력
        yield return roundStartDelay;
    }
    
    /// <summary>
    /// 유닛 페이즈 종료 조건 확인
    /// 죽은 유닛들을 스킵하고 모든 유닛이 행동했는지 확인
    /// </summary>
    private bool IsEndUnitPhase(IReadOnlyList<Unit> battleUnits)
    {
        // 죽은 유닛들 스킵
        while (currentUnitIndex < battleUnits.Count && 
               battleUnits[currentUnitIndex].currentStat[StatType.Hp] <= 0)
            currentUnitIndex++;
        
        return currentUnitIndex >= battleUnits.Count;
    }
    
    /// <summary>
    /// 유닛 페이즈 종료 처리
    /// 상태효과 지속시간 감소 및 몬스터 턴으로 전환
    /// </summary>
    private IEnumerator EndUnitPhase(IReadOnlyList<Unit> battleUnits)
    {
        ReduceAliveUnitsEffects(battleUnits);
        currentUnitIndex = 0;
        currentPhase = TurnPhase.MonsterTurn;
        yield break;
    }

    /// <summary>
    /// /// 살아있는 유닛들의 상태효과 지속시간 감소
    /// </summary>
    private void ReduceAliveUnitsEffects(IReadOnlyList<Unit> battleUnits)
    {
        foreach (var unit in battleUnits)
        {
            if (unit.currentStat[StatType.Hp] > 0)
            {
                unit.ReduceStatusEffectDuration();
            }
        }
    }
    
    /// <summary>
    /// 현재 행동 유닛 반환
    /// </summary>
    private Unit GetCurrentUnit(IReadOnlyList<Unit> battleUnits)
    {
        return battleUnits[currentUnitIndex];
    }

    #endregion
    
    /// <summary>
    /// 기본 공격 -> 스킬 사용 순서로 진행
    /// </summary>
    private IEnumerator ProcessUnitTurn(Unit unit)
    {
        PrepareUnitTurn(unit);
        
        yield return turnDelay;
        
        if (IsUnitStunned(unit))
        {
            yield return HandleStunnedUnit(unit);
            yield break;
        }
    
        yield return ExecuteUnitBasicAttack(unit);
    
        if (IsEndBattle()) yield break;
    
        yield return HandleSkillPhase(unit);
    
        currentUnitIndex++;
    }

    #region ProcessUnitTurn

    /// <summary>
    /// 유닛 턴 준비
    /// UI 마커 업데이트 및 스킬 사용 플래그 초기화
    /// </summary>
    private void PrepareUnitTurn(Unit unit)
    {
        battleServices.UI.UpdateMarkAsCurrentEntity(unit.GetUpperCenter());
        isSkillUsed = false;
    }
    
    private bool IsUnitStunned(Unit unit)
    {
        return unit.HasStatusEffect(StatusEffectType.Stun); // 스턴상태 확인
    }
    
    /// <summary>
    /// 스턴된 유닛 처리
    /// 턴 스킵 및 스킬 쿨다운 감소
    /// </summary>
    private IEnumerator HandleStunnedUnit(Unit unit)
    {
        MyDebug.Log($"{unit.UnitName}은 기절 상태로 턴 스킵");
        unit.ReduceSkillCooldown();
        currentUnitIndex++;
        yield return null;
    }
    
    /// <summary>
    /// 기본 공격 실행
    /// </summary>
    private IEnumerator ExecuteUnitBasicAttack(Unit unit)
    {
        Monster target = battleServices.Targeting.SelectMonsterTarget();
        SoundManager.Instance.PlayRandomUnitAttackSfx();
        battleServices.Actions.ExecuteBasicAttack(unit, target);
        yield return WaitForActionComplete();
    }
    
    /// <summary>
    /// 전투 종료 조건 확인
    /// </summary>
    private bool IsEndBattle()
    {
        return battleServices.Flow.CurrentState == BattleState.Win ||
               battleServices.Flow.CurrentState == BattleState.Lose ||
               battleServices.Flow.CurrentState == BattleState.Interrupted;
    }
    
    /// <summary>
    /// 스킬 페이즈 처리
    /// 자동/수동 모드에 따라 다른 스킬 사용 로직 적용
    /// </summary>
    private IEnumerator HandleSkillPhase(Unit unit)
    {
        if (battleServices.Flow.CurrentMode == BattleMode.Auto)
            yield return HandleAutoModeSkill(unit);
        else
            yield return HandleManualModeSkill(unit);
        
        yield return WaitForActionComplete();
    }
    
    /// <summary>
    /// 자동 모드 스킬 처리
    /// 스킬 사용 가능 시 즉시 사용
    /// </summary>
    private IEnumerator HandleAutoModeSkill(Unit unit)
    {
        if (unit.CanUseSkill())
        {
            battleServices.Actions.ExecuteSkill(unit);
            battleServices.UI.EndUseSkillWaitingGUI(currentUnitIndex, true);
            isSkillUsed = true;
        }
    
        if (!isSkillUsed)
            unit.ReduceSkillCooldown();
        
        yield return null;
    }
    
    /// <summary>
    /// 수동 모드 스킬 처리
    /// 플레이어 입력 대기 및 스킬 사용
    /// </summary>
    private IEnumerator HandleManualModeSkill(Unit unit)
    {
        if (unit.CanUseSkill())
        {
            yield return battleServices.Input.WaitForSkillInput(unit, BattleConfig.Instance.skillWaitTime);
        }
    
        if (!isSkillUsed)
            unit.ReduceSkillCooldown();
    }

    #endregion
    /// <summary>
    /// 살아있는 몬스터 순서대로 기본 공격을 수행하고 턴 전환
    /// 몬스터 턴: 일반 or 보스에 따라 행동 다르게 처리
    /// </summary>
    private IEnumerator HandleMonsterTurn()
    {
        var monsters = battleServices.Monsters;
        
        if (IsNewMonsterRound())
            yield return StartMonsterRound();
    
        if (IsEndMonsterPhase(monsters))
        {
            yield return EndMonsterPhase(monsters);
            yield break;
        }

        Monster monster = GetCurrentMonster(monsters);
        yield return ProcessMonsterTurn(monster);
    }
    #region HandleMonsterTurn
    private bool IsNewMonsterRound() => currentMonsterIndex == 0; // 적 턴 시작

    /// <summary>
    /// 몬스터 턴 시작시 처리
    /// </summary>
    private IEnumerator StartMonsterRound()
    {
        MyDebug.Log("적 턴 시작");
        yield return roundStartDelay;
    }

    /// <summary>
    /// 몬스터 페이즈 종료 조건 확인
    /// 죽은 몬스터들을 스킵하고 모든 몬스터가 행동했는지 확인
    /// </summary>
    private bool IsEndMonsterPhase(IReadOnlyList<Monster> monsters)
    {
        // 죽은 몬스터들 스킵
        while (currentMonsterIndex < monsters.Count && 
               monsters[currentMonsterIndex].currentStat[StatType.Hp] <= 0)
            currentMonsterIndex++;
        
        return currentMonsterIndex >= monsters.Count;
    }
    
    /// <summary>
    /// 몬스터 페이즈 종료 처리
    /// 일반 몬스터들의 스킬 쿨다운 감소 및 유닛 턴으로 전환
    /// </summary>
    private IEnumerator EndMonsterPhase(IReadOnlyList<Monster> monsters)
    {
        MyDebug.Log($"{roundCount}번째 라운드 종료 - 플레이어 턴으로 전환");
    
        ReduceAliveMonsterEffects(monsters);
        ResetMonsterPhase();
    
        yield break;
    }
    
    /// <summary>
    /// 살아있는 일반 몬스터들의 스킬 쿨다운 감소
    /// 보스는 별도 관리되므로 제외
    /// </summary>
    private void ReduceAliveMonsterEffects(IReadOnlyList<Monster> monsters)
    {
        foreach (var monster in monsters)
        {
            if (monster.currentStat[StatType.Hp] > 0 && !monster.isBoss)
            {
                monster.ReduceSkillCooldown();
            }
        }
    }

    /// <summary>
    /// 몬스터 페이즈 리셋
    /// 라운드 카운트 증가 및 유닛 턴으로 전환
    /// </summary>
    private void ResetMonsterPhase()
    {
        roundCount++;
        currentPhase = TurnPhase.UnitTurn;
        currentMonsterIndex = 0;
    }
    
    /// <summary>
    /// 현재 행동 몬스터 반환
    /// </summary>
    private Monster GetCurrentMonster(IReadOnlyList<Monster> monsters)
    {
        return monsters[currentMonsterIndex];
    }

    #endregion

    /// <summary>
    /// 개별 몬스터의 턴 처리
    /// 보스와 일반 몬스터를 구분하여 처리
    /// </summary>
    private IEnumerator ProcessMonsterTurn(Monster monster)
    {
        PrepareMonsterTurn(monster);
    
        if (monster.isBoss)
            yield return ProcessBossTurn(monster);
        else
            yield return ProcessRegularMonsterTurn(monster);

        currentMonsterIndex++;
    }
    #region ProcessMonsterTurn
    
    /// <summary>
    /// 몬스터 턴 준비, UI 마커 업데이트
    /// </summary>
    private void PrepareMonsterTurn(Monster monster)
    {
        battleServices.UI.UpdateMarkAsCurrentEntity(monster.GetUpperCenter());
    }

    /// <summary>
    /// 보스 턴 처리
    /// ActionManager의 ExecuteBossAction을 통해 보스 패턴 실행
    /// </summary>
    private IEnumerator ProcessBossTurn(Monster boss)
    {
        battleServices.Actions.ExecuteBossAction(boss);
        yield return WaitForActionComplete();
    }
    
    /// <summary>
    /// 일반 몬스터 턴 처리
    /// 기본 공격 -> 스킬 사용 순서로 진행
    /// </summary>
    private IEnumerator ProcessRegularMonsterTurn(Monster monster)
    {
        yield return ExecuteMonsterBasicAttack(monster);
    
        if (IsEndBattle()) 
            yield break;
        
        yield return ExecuteMonsterSkillIfPossible(monster);
    }

    /// <summary>
    /// 몬스터 기본 공격 실행
    /// </summary>
    private IEnumerator ExecuteMonsterBasicAttack(Monster monster)
    {
        Unit target = battleServices.Targeting.SelectUnitTarget();
        if (target != null)
        {
            battleServices.Actions.ExecuteBasicAttack(monster, target);
            yield return WaitForActionComplete();
        }
    }

    /// <summary>
    /// 몬스터 스킬 사용 가능 시 실행
    /// </summary>
    private IEnumerator ExecuteMonsterSkillIfPossible(Monster monster)
    {
        if (monster.CanUseSkill())
        {
            battleServices.Actions.ExecuteSkill(monster);
            yield return WaitForActionComplete();
        }
    }

    #endregion
    
    /// <summary>
    /// 액션 완료까지 대기하는 유틸리티 메서드
    /// </summary>
    private IEnumerator WaitForActionComplete()
    {
        yield return waitForActionComplete;
    }
}
