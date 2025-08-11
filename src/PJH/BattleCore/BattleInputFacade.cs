using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleInputFacade
{
    IEnumerator WaitForSkillInput(Unit unit, float waitTime);
    void OnSkillButtonClick(int unitIndex);
    void OnMonsterClicked(Monster clickedMonster);
    void IsSkillUsed(bool value);
}
public class BattleInputFacade : IBattleInputFacade
{
    private readonly ManualInputHandler inputHandler;
    private readonly TurnManager turnManager;
    
    public BattleInputFacade(ManualInputHandler inputHandler, TurnManager turnManager)
    {
        this.inputHandler = inputHandler;
        this.turnManager = turnManager;
    }
    
    public IEnumerator WaitForSkillInput(Unit unit, float waitTime)
        => inputHandler.WaitForSkillInput(unit, waitTime);
    
    public void OnSkillButtonClick(int unitIndex)
        => inputHandler.OnSkillButtonClick(unitIndex);
    
    public void OnMonsterClicked(Monster clickedMonster)
        => inputHandler.OnMonsterClicked(clickedMonster);
    
    public void IsSkillUsed(bool value)
        => turnManager.isSkillUsed = value;
}
