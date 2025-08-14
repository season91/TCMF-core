using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 UI 업데이트 및 이벤트 전달을 담당하는 Facade
/// 
/// 주요 역할:
/// - 전투 진행 상황의 실시간 UI 반영
/// - 스킬 쿨타임 및 사용 가능 상태 시각화
/// - 턴 진행 및 현재 행동 주체 표시
/// - 데미지 폰트 및 시각적 피드백 처리
/// - 라운드 정보 및 스테이지 진행도 관리
/// - 플레이어 상호작용 UI 가이드 (타겟 선택 등)
/// </summary>
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
    event Action<int, float> OnUseSkillWaitingCool;
    
    void UpdateSkillCoolGUI(IReadOnlyList<Unit> units);
    void UpdateRound(Action action);
    void UpdateStageInfo(int roundNumber);
    void UpdateShowDamage(int damage, Vector2 worldPosition);
    void StartUseSkillWaitingGUI(int index);
    void EndUseSkillWaitingGUI(int index, bool isUseSkill);
    void UpdateMarkAsCurrentEntity(Vector2 worldPosition);
    void UpdateHideDieUnitSkillButton(int index);
    void UpdateUseSkillWaitingCool(int unitIndex, float timeRatio);
}
