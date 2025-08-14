/// <summary>
/// 전투의 전체적인 흐름과 상태를 관리하는 Facade
/// 
/// 주요 역할:
/// - 배틀 모드 관리 (수동/자동 전환)
/// - 배틀 상태 추적 (시작/진행/승리/패배/중단)
/// - 게임 속도 제어 (1배속/2배속)
/// - 전투 종료 조건 체크 및 승부 판정
/// 
/// 상태 관리 구조:
/// - BattleMode: Auto(자동) ↔ Manual(수동)
/// - BattleState: Start → Win/Lose/Interrupted  
/// - SpeedMode: 1x ↔ 2x 
/// </summary>
public interface IBattleFlowFacade
{
    BattleMode CurrentMode { get; }
    BattleState CurrentState { get; }
    bool IsDoubleSpeed { get; }
    
    void Initialize();
    bool CheckBattleEnd();
    bool IsBattleOver();
    bool IsAllPlayersDead();
    bool IsAllMonstersDead();
    void ToggleBattleMode();
    void ToggleSpeedMode();
    void ResetSpeedMode();
    void SetBattleState(BattleState newState);
}
