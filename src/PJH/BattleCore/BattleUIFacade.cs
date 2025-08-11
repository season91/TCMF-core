using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI 관련 Facade
public interface IBattleUIFacade
{
    event Action<IReadOnlyList<Unit>> OnUpdateSkillCool;
    event Action<Action> OnShowCurrentRound;
    event Action<int> OnUpdateStageInfo;
    event Action<int, Vector2> OnShowDamage;
    event Action<int> OnStartUseSkillWaitingGUI;
    event Action<int, bool> OnEndUseSkillWaitingGUI;
    event Action<Vector2> OnMarkAsCurrentEntity;
    event Action<int> OnHideDieUnitSkillButton;
    event Action<bool> OnTargetSelectPromptShown;
    event Action<int, float> OnUseSkillWaitingCool;
    
    void UpdateSkillCoolGUI(IReadOnlyList<Unit> units);
    void UpdateRound(Action action);
    void UpdateStageInfo(int roundNumber);
    void UpdateShowDamage(int damage, Vector2 worldPosition);
    void StartUseSkillWaitingGUI(int index);
    void EndUseSkillWaitingGUI(int index, bool isUseSkill);
    void UpdateMarkAsCurrentEntity(Vector2 worldPosition);
    void UpdateHideDieUnitSkillButton(int index);
    void UpdateTargetSelectPromptShown(bool isShown);
    void UpdateUseSkillWaitingCool(int unitIndex, float timeRatio);
}

public class BattleUIFacade : IBattleUIFacade
{
    public event Action<IReadOnlyList<Unit>> OnUpdateSkillCool;
    public event Action<Action> OnShowCurrentRound;
    public event Action<int> OnUpdateStageInfo;
    public event Action<int, Vector2> OnShowDamage;
    public event Action<int> OnStartUseSkillWaitingGUI;
    public event Action<int, bool> OnEndUseSkillWaitingGUI;
    public event Action<Vector2> OnMarkAsCurrentEntity;
    public event Action<int> OnHideDieUnitSkillButton;
    public event Action<bool> OnTargetSelectPromptShown;
    public event Action<int, float> OnUseSkillWaitingCool;
    
    // =======================================================================
    
    public void UpdateSkillCoolGUI(IReadOnlyList<Unit> units)
        => OnUpdateSkillCool?.Invoke(units);
    
    public void UpdateRound(Action action)
        => OnShowCurrentRound?.Invoke(action);
    
    public void UpdateStageInfo(int roundNumber)
        => OnUpdateStageInfo?.Invoke(roundNumber);
    
    /// <summary>
    /// 데미지 폰트 띄우는 메서드
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="worldPosition">띄울려는 Unit의 Center 월드 좌표</param>
    public void UpdateShowDamage(int damage, Vector2 worldPosition)
        => OnShowDamage?.Invoke(damage, worldPosition);
    
    /// <summary>
    /// 현재 턴 유닛의 스킬 가시성: 현재 턴 유닛의 스킬 사용 차례시 호출
    /// - 사이즈 업, 테두리 이펙트 On
    /// </summary>
    /// <param name="index">현재 Unit Index</param>
    public void StartUseSkillWaitingGUI(int index)
        => OnStartUseSkillWaitingGUI?.Invoke(index);
    
    /// <summary>
    /// 현재 턴 유닛의 스킬 가시성: 현재 턴 유닛의 스킬 사용 차례 종료 시 호출
    /// - 사이즈 다운, 테두리 이펙트 Off, 스킬 사용 시 아이콘 Shade
    /// </summary>
    /// <param name="index">현재 Unit Index</param>
    /// <param name="isUseSkill">현재 Unit이 스킬을 사용하였는지?</param>
    public void EndUseSkillWaitingGUI(int index, bool isUseSkill)
        => OnEndUseSkillWaitingGUI?.Invoke(index, isUseSkill);
    
    /// <summary>
    /// 현재 Entity 턴 가시성: 현재 턴 유닛의 마크를 유닛의 위로 옮김
    /// - 어떤 Entity 공격 턴인지 알려주는 표시 이벤트로 vector는 공격 차례 Entity의 머리 위 월드 좌표
    /// </summary>
    /// <param name="worldPosition">현재 Unit의 Upper Center 월드 좌표</param>
    public void UpdateMarkAsCurrentEntity(Vector2 worldPosition)
        => OnMarkAsCurrentEntity?.Invoke(worldPosition);
    
    /// <summary>
    /// 유닛 사망 시, 스킬 버튼 제거
    /// </summary>
    /// <param name="index">현재 죽은 Unit의 index</param>
    public void UpdateHideDieUnitSkillButton(int index)
        => OnHideDieUnitSkillButton?.Invoke(index);
    
    public void UpdateTargetSelectPromptShown(bool isShown)
        => OnTargetSelectPromptShown?.Invoke(isShown);

    /// <summary>
    /// 스킬 사용 대기 시간 비율 업데이트 메서드
    /// </summary>
    /// <param name="timeRatio">Waiting Time 비율</param>
    public void UpdateUseSkillWaitingCool(int unitIndex, float timeRatio)
        => OnUseSkillWaitingCool?.Invoke(unitIndex, timeRatio);
}
