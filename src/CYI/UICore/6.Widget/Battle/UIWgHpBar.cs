using UnityEngine;

public class UIWgHpBar : UIBase
{
    private UIDynamicObjectPool<UIWgEntityHpBar> dynamicHpBarPool;
    [SerializeField] private Transform hpBarRoot;
    [SerializeField] private UIWgEntityHpBar originHpBar;
    [SerializeField] private UIWgBossHpBar bossHpBar;

    protected override void Reset()
    {
        base.Reset();
        
        hpBarRoot = transform.FindChildByName<Transform>("Group_HpBar");
        originHpBar = transform.FindChildByName<UIWgEntityHpBar>("GUI_EntityHpBar");
        bossHpBar = transform.FindChildByName<UIWgBossHpBar>("GUI_BossHpBar");
    }

    public override void Initialize()
    {
        base.Initialize();
        
        dynamicHpBarPool = new UIDynamicObjectPool<UIWgEntityHpBar>(originHpBar, hpBarRoot, 6);
    }

    public override void Open(OpenContext openContext = null)
    {
        base.Open(openContext);
        
        bossHpBar.gameObject.SetActive(StageManager.Instance.CurStageData.IsBossStage);
    }
    
    public void InitHpBar()
    {
        dynamicHpBarPool.OffAll();
    }

    public void BindingHpBar(CharacterBase character)
    {
        character.RemoveAllListenerHpAction();
        
        if (character is Monster monster && monster.isBoss)
        {
            bossHpBar.gameObject.SetActive(true);
            bossHpBar.Initialize();
            character.OnHpChanged += bossHpBar.UpdateHpBar;
        }
        else
        {
            var hpBar = dynamicHpBarPool.Get();
            var pos = UIUtility.WorldToCanvasPosition(
                canvas,
                character.GetUpperCenter(0.2f), 
                UIManager.Instance.MainCamera
                );
            hpBar.Initialize();
            hpBar.SetHpBar(pos);
            character.OnHpChanged += hpBar.UpdateHpBar;
        }
    }
}
