using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 수동 모드일 때 유저 입력을 기다리고, 선택된 타겟에 따라 스킬을 실행하도록 위임함
/// 수동 입력 대기
/// </summary>
public class ManualInputHandler : MonoBehaviour
{
    private IBattleServices battleServices;
    private WaitForSeconds frameWait => new WaitForSeconds(BattleConfig.Instance.frameCheckInterval);

    private bool isWaitingForTarget = false; // 스킬버튼 클릭 후 적을 클릭할 수 있는 상황인지 아닌지
    private bool isWatingForPlayerAction = false; // 스킬 턴인지 아닌지 구분
    private Unit currentUnit;
    private int currentUnitIndex;
    private Action<Monster> onTargetSelected;

    public void Initialize(IBattleServices services)
    {
        battleServices = services;
    }
    /// <summary>
    /// 턴 중 입력을 기다림 (TurnManager에서 호출됨)
    /// </summary>
    public IEnumerator WaitForSkillInput(Unit unit, float waitTime)
    {
        currentUnit = unit;
        isWatingForPlayerAction = true;
        
        float startTime = Time.time;
        float endTime = startTime + waitTime;
        
        float lastAutoModeCheckTime = startTime;
        currentUnitIndex = battleServices.Units.ToList().IndexOf(currentUnit);
        // battleServices.UI.UpdateTargetSelectPromptShown(true); // 스킬 선택 UI 표시 열기
        // <= 평타 이후 스킬 사용을 알리는 메서드인 StartUseSkillWaitingGUI(unitIndex);
        battleServices.UI.StartUseSkillWaitingGUI(currentUnitIndex);
        // 스킬 눌렀을 때, 적 선택하라는 안내용 => 이동 해야함
        
        while (Time.time < endTime && isWatingForPlayerAction)
        {
            if (Time.time - lastAutoModeCheckTime >= BattleConfig.Instance.autoModeCheckInterval)
            {
                if (battleServices.Flow.CurrentMode == BattleMode.Auto)
                {
                    UIManager.Instance.Close<UISlidePopup>();
                    MyDebug.Log($"자동 모드 전환 {unit.UnitName} 자동 스킬 실행");
                    battleServices.Actions.ExecuteSkill(currentUnit);
                    isWatingForPlayerAction = false;
                    battleServices.UI.EndUseSkillWaitingGUI(currentUnitIndex, true);
                    battleServices.Input.IsSkillUsed(true);
                    battleServices.UI.UpdateTargetSelectPromptShown(false); // 스킬 선택 UI 표시 닫기
                    yield break;
                }
                lastAutoModeCheckTime = Time.time;
            }
            float ratio = (Time.time - startTime) / waitTime;
            battleServices.UI.UpdateUseSkillWaitingCool(currentUnitIndex, ratio); 
            yield return frameWait;
        }

        // 시간 초과
        if (isWatingForPlayerAction)
        {
            MyDebug.Log("스킬 입력 시간 초과 → 턴 넘김");
            isWatingForPlayerAction = false;
            battleServices.UI.EndUseSkillWaitingGUI(currentUnitIndex, false);
            onTargetSelected?.Invoke(null); // null 전달하면 턴 넘어감
        }
        
        battleServices.UI.UpdateTargetSelectPromptShown(false);  // 스킬 선택 UI 표시 닫기
        
        currentUnit = null;
        isWaitingForTarget = false;
        onTargetSelected = null;
    }
    
    // 역할: 유저가 스킬 버튼을 눌렀다는 신호만 전달
    public void OnSkillButtonClick(int unitIndex)
    {
        // 플레이어 액션 대기 중이 아니면 무시
        if (!isWatingForPlayerAction || currentUnit == null) 
        {
            MyDebug.Log("현재 스킬을 사용할 수 있는 상태가 아닙니다.");
            return;
        }

        ShowTargetSelectionPopup();
        
        currentUnitIndex = battleServices.Units.ToList().IndexOf(currentUnit);
        if (unitIndex != currentUnitIndex)
        {
            MyDebug.Log($"현재 턴이 아닌 유닛의 스킬 버튼입니다. 현재 턴: {currentUnitIndex}, 클릭된 버튼: {unitIndex}");
            return;
        }
        //유닛 상태 재확인
        if (!currentUnit.CanUseSkill())
        {
            MyDebug.Log("스킬 사용 불가: 조건 불충분");
            return;
        }
        
        // 이미 타겟 대기 중이면 취소
        if (isWaitingForTarget)
        {
            CancelSkill();
            CloseTargetSelectionPopup();
            return;
        }
        ExecuteSkillAction(currentUnit, currentUnitIndex);
        battleServices.Input.IsSkillUsed(true);
    }

    private void ExecuteSkillAction(Unit unit, int index)
    {
        if (!MasterData.SkillDataDict.TryGetValue(unit.UnitData.Code, out var skillData))
        {
            MyDebug.LogWarning($"스킬 데이터를 찾을 수 없습니다: {unit.UnitData.Code}");
            return;
        }
        
        if (unit.isSelectable && skillData.TargetType == SkillTargetType.Single && skillData.TargetFilter == TargetFilter.Monster)
        {
            // 단일 몬스터 타겟이 필요한 스킬만 수동 선택
            MyDebug.Log($"{unit.UnitName} 스킬 준비. 적을 선택하세요. (스킬 버튼 다시 클릭하면 취소)");
            StartTargetSelection(index);
        }
        else
        {
            CloseTargetSelectionPopup();
            // 나머지는 자동 타겟팅 사용
            MyDebug.Log($"{unit.UnitName} 스킬 자동 실행");
            battleServices.Actions.ExecuteSkill(unit);
            battleServices.UI.EndUseSkillWaitingGUI(index, true);
            battleServices.Input.IsSkillUsed(true);
            
            // 스킬 사용 완료 - 턴 종료
            isWatingForPlayerAction = false;
            isWaitingForTarget = false;
        }
    }

    private void StartTargetSelection(int index)
    {
        isWaitingForTarget = true;
        onTargetSelected = (target) =>
        {
            if (target != null)
            {
                CloseTargetSelectionPopup();
                battleServices.Actions.ExecuteSkill(currentUnit, target);
                battleServices.UI.EndUseSkillWaitingGUI(index, true);
                battleServices.Input.IsSkillUsed(true);
            }
                
            // 스킬 사용 완료 - 턴 종료
            isWatingForPlayerAction = false;
            isWaitingForTarget = false;
            onTargetSelected = null;
        };
    }
    
    //스킬 사용 취소
     private void CancelSkill()
     {
         if (!isWaitingForTarget) return;
         MyDebug.Log("스킬 취소");
         
         isWaitingForTarget = false;
         onTargetSelected = null;
     }

    /// <summary>
    /// 적 클릭 시 호출됨
    /// </summary>
    public void OnMonsterClicked(Monster clickedMonster)
    {
        if (!isWaitingForTarget || clickedMonster == null || clickedMonster.currentStat[StatType.Hp] <= 0)
        {
            MyDebug.Log("적을 선택할 수 없는 상태거나 잘못된 대상");
            return;
        }

        MyDebug.Log($"{clickedMonster.UnitName} 선택됨");
        
        // 타겟 선택 완료 → 콜백 실행
        isWaitingForTarget = false;
        onTargetSelected?.Invoke(clickedMonster);
        onTargetSelected = null;
    }

    #region 리팩토링 필요
    private void ShowTargetSelectionPopup()
    {
        UIManager.Instance.Open<UISlidePopup>(
            OpenContext.WithContext(
                new SlideOpenContext
                {
                    Comment = "스킬을 사용할 적을 선택해주세요."
                }
            ));
    }
    private void CloseTargetSelectionPopup()
    {
        UIManager.Instance.Close<UISlidePopup>();
    }
    
    #endregion
}
