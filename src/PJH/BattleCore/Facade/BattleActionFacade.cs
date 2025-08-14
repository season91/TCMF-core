/// <summary>
/// IBattleActionFacade 구현체
/// </summary>
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
    
    public void ExecuteBossAction(Monster monster)
        => actionManager.ExecuteBossAction(monster);
    
    public bool IsActionInProgress()
        => actionManager.IsActionInProgress;
    
    public void SetActionInProgress(bool value)
        => actionManager.SetActionInProgress(value);
}

