using System.Collections;

/// <summary>
/// 전투 중 사용자 입력 처리 및 수동 모드 상호작용을 담당하는 Facade
/// 
/// 주요 역할:
/// - 수동 모드에서 스킬 사용 입력 대기 및 처리
/// - 스킬 버튼 클릭 처리 (유효성 검증 포함)
/// - 몬스터 클릭 타겟 선택 처리
/// - 입력 제한시간 관리 및 자동 모드 전환 감지
/// - 스킬 사용 완료 상태 TurnManager와 연동
/// 
/// 동작 흐름:
/// 1. 플레이어 턴에서 기본공격 완료 후 스킬 입력 대기
/// 2. 제한시간 내 스킬 버튼 클릭 → 타겟 선택 UI 표시
/// 3. 몬스터 클릭 → 스킬 실행
/// 4. 시간 초과 또는 완료 시 턴 종료
/// </summary>
public interface IBattleInputFacade
{
    IEnumerator WaitForSkillInput(Unit unit, float waitTime);
    void OnSkillButtonClick(int unitIndex);
    void OnMonsterClicked(Monster clickedMonster);
    void IsSkillUsed(bool value);
}