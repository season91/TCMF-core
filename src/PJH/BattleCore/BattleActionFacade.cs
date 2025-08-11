using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleActionFacade
{
    void ExecuteBasicAttack(CharacterBase attacker, CharacterBase target);
    void ExecuteSkill(Unit caster, Monster monster);
    void ExecuteSkill(CharacterBase caster);
    void ExecuteBossAttack(Monster monster);
    bool IsActionInProgress();
    void SetActionInProgress(bool value);
}

public class BattleActionFacade : IBattleActionFacade
{
    private readonly ActionManager actionManager;
    
    public BattleActionFacade(ActionManager actionManager)
    {
        this.actionManager = actionManager;
    }
    
    public void ExecuteBasicAttack(CharacterBase attacker, CharacterBase target)
        => actionManager.ExecuteBasicAttack(attacker, target);
    
    public void ExecuteSkill(Unit caster, Monster monster)
        => actionManager.ExecuteSkill(caster, monster);
    
    public void ExecuteSkill(CharacterBase caster)
        => actionManager.ExecuteSkill(caster);
    
    public void ExecuteBossAttack(Monster monster)
        => actionManager.ExecuteBossAction(monster);
    
    public bool IsActionInProgress()
        => actionManager.IsActionInProgress;
    
    public void SetActionInProgress(bool value)
        => actionManager.SetActionInProgress(value);
}

