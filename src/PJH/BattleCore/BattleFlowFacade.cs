using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    event Action<BattleMode> OnModeChanged;
    event Action<BattleState> OnStateChanged;
    event Action<bool> OnSpeedChanged;
}
public class BattleFlowFacade : IBattleFlowFacade
{
    private readonly IBattleServices battleServices;
    public BattleMode CurrentMode { get; private set; }
    public BattleState CurrentState { get; private set; }
    public bool IsDoubleSpeed { get; private set; } = false;
    private bool keepDoubleSpeed  = false;
    
    private readonly float originalTimeScale = 1f;
    
    public event Action<BattleMode> OnModeChanged;
    public event Action<BattleState> OnStateChanged;
    public event Action<bool> OnSpeedChanged;
    
    public BattleFlowFacade(IBattleServices battleServices)
    {
        this.battleServices = battleServices;

        CurrentState = BattleState.Start;
        CurrentMode = BattleMode.Manual;
        IsDoubleSpeed = false;
    }
    
    public void Initialize()
    {
        SetBattleState(BattleState.Start);
        SetSpeedMode(keepDoubleSpeed);
    }
    public bool CheckBattleEnd()
    {
        if (CurrentState == BattleState.Win || CurrentState == BattleState.Lose) 
            return true;
        
        if (IsAllPlayersDead())
        {
            SetBattleState(BattleState.Lose);
            return true;
        }
        
        if (IsAllMonstersDead())
        {
            SetBattleState(BattleState.Win);
            return true;
        }
        
        return false;
    }
    
    public bool IsBattleOver()
        => CurrentState == BattleState.Win ||
           CurrentState == BattleState.Lose ||
           CurrentState == BattleState.Interrupted;
    
    public bool IsAllPlayersDead()
        => battleServices.Units.All(unit => unit.currentStat[StatType.Hp] <= 0);
    
    public bool IsAllMonstersDead()
        => battleServices.Monsters.All(monster => monster.currentStat[StatType.Hp] <= 0);
    
    public void ToggleBattleMode()
    {
        BattleMode newMode = (CurrentMode == BattleMode.Auto) ? BattleMode.Manual : BattleMode.Auto;
        SetBattleMode(newMode);
    }
    
    public void ToggleSpeedMode()
    {
        SetSpeedMode(!IsDoubleSpeed);
        keepDoubleSpeed = IsDoubleSpeed;
    }
    
    public void ResetSpeedMode()
    {
        SetSpeedMode(false);
    }
    
    public void SetBattleMode(BattleMode newMode)
    {
        if (CurrentMode != newMode)
        {
            CurrentMode = newMode;
            OnModeChanged?.Invoke(CurrentMode);
            MyDebug.Log($"모드 변경됨: {CurrentMode}");
        }
    }
    
    public void SetBattleState(BattleState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
            MyDebug.Log($"배틀 상태 변경: {CurrentState}");
        }
    }
    
    private void SetSpeedMode(bool isDoubleSpeed)
    {
        if (IsDoubleSpeed != isDoubleSpeed)
        {
            IsDoubleSpeed = isDoubleSpeed;
            Time.timeScale = IsDoubleSpeed ? 2f : originalTimeScale;
            OnSpeedChanged?.Invoke(IsDoubleSpeed);
        }
    }
}
