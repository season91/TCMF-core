using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 모든 캐릭터 공통으로 가지는 기능 추상 클래스 (Unit, Monster)
/// 역할을 함수 단위로 분리하고, 필요 시 확장할 수 있도록 구조 개선
/// 체력, 스킬, 초기화 흐름 등의 공통 로직을 포함하며 자식 클래스에서 필요한 부분만 구현하도록 구성
/// </summary>
public abstract class CharacterBase : MonoBehaviour
{
    public IBattleServices battleServices;
    protected SkillExecutor skillExecutor;
    public CharacterStatController statController { get; private set; }
    public CommonStat currentStat => statController.CurrentStat;
    public CommonStat baseStat => statController.BaseStat;
    public string UnitName { get; protected set; }
    public Dictionary<StatType, int> backupStat = new Dictionary<StatType, int>();
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector3 targetPoint;
    public Canvas canvas;
    private Action<float> onHpChanged;
    public event Action<float> OnHpChanged
    {
        add => onHpChanged += value;
        remove => onHpChanged -= value;
    }
    
    public void UpdateHpBar(float ratio) => onHpChanged?.Invoke(ratio);
    public void RemoveAllListenerHpAction() => onHpChanged = null;

    public SkillData SkillData;
    public AttackType attackType;
    
    [Header("스킬 정보")]
    public string skillName;
    public int skillCooldown;
    public string skillDescription;
    public int currentSkillCooldown;
    public bool isSelectable;
    public SkillType skillType;
    public bool IsSkillReady => currentSkillCooldown <= 0;
    
    protected StatusEffectController statusEffectController;
    
    // 이 캐릭터에 붙은 프리팹 인스턴스 (풀매니저를 통해 붙은)
    protected GameObject spawnedPrefab;

    public virtual void Initialize(IBattleServices services)
    {
        battleServices = services ?? throw new ArgumentNullException(nameof(services));
        skillExecutor = new SkillExecutor(battleServices);
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        canvas = GetComponentInChildren<Canvas>();
    }

    protected virtual void InitStats()
    {
        statController = new CharacterStatController();
        statusEffectController = new StatusEffectController();
        statusEffectController.Initialize(this);
    }
    
    protected virtual void InitVisual(string entityCode)
    {
        string entitySprite = StringAdrEntity.EntityDict[entityCode];
        spriteRenderer.sprite = ResourceManager.Instance.GetResource<Sprite>(entitySprite);
        targetPoint = spriteRenderer.bounds.center;
    }
    /// <summary>
    /// 데미지를 받은 경우 스탯과 HP UI를 갱신함
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        int finalDamage = statController.CalculateDamageTaken(amount);
        statController.ApplyDamage(finalDamage);
        MyDebug.Log($"받은데미지 : {finalDamage}");
        if (this is Unit) // 플레이어 유닛이 피격당했을 때만
        {
            SoundManager.Instance.PlayRandomUnitHitSfx();
            MyDebug.Log("Vibration");
            VibrationManager.Instance.VibrateOnHit();
        }
        else if (this is Monster)
        {
            SoundManager.Instance.PlaySfx(StringAdrAudioSfx.MonsterHit);
        }
        battleServices.UI.UpdateShowDamage(finalDamage, GetTargetPoint()); // 데미지 UI 표시
        
        if (currentStat[StatType.Hp] >= 0)
        {
            float ratio = statController.HpRatio;
            onHpChanged?.Invoke(ratio);
        }
        // if (currentStat[StatType.Hp] <= 0)
        // {
        //     battleServices?.AnimationController.DeathAnimation(this);
        // }
    }
    // 순수 데미지 (방어력 무시)
    public virtual void TakePureDamage(int amount)
    {
        statController.ApplyDamage(amount); // 방어력 계산 없이 바로 적용
        
        if (this is Unit) // 플레이어 유닛이 피격당했을 때만
        {
            SoundManager.Instance.PlayRandomUnitHitSfx();
            VibrationManager.Instance.VibrateOnHit();
        }
        else if (this is Monster)
        {
            SoundManager.Instance.PlaySfx(StringAdrAudioSfx.MonsterHit);
        }
        MyDebug.Log($"순수 데미지: {amount}");
        
        battleServices.UI.UpdateShowDamage(amount, GetTargetPoint()); // 데미지 UI 표시
        
        if (currentStat[StatType.Hp] >= 0)
        {
            float ratio = statController.HpRatio;
            onHpChanged?.Invoke(ratio);
        }
        
        // if (currentStat[StatType.Hp] <= 0)
        // {
        //     battleServices?.AnimationController.DeathAnimation(this);
        // }
    }
    
    /// <summary>
    /// 스킬 사용 진입
    /// 현재 쿨타임 조건 만족 여부 확인 후 ExecuteSkill 호출
    /// </summary>
    public virtual void UseSkill(List<CharacterBase> targets = null)
    {
        if (!CanUseSkill()) return;  // 자식 클래스에서 구현

        ExecuteSkill(targets); // 자식 클래스에서 구현
        currentSkillCooldown = skillCooldown;
    }
    public abstract bool CanUseSkill(); // 스킬 사용 가능 조건 (예: 쿨타임, 침묵 등)
    public abstract void ExecuteSkill(List<CharacterBase> targets = null); // 스킬 실제 실행 로직
    
    /// <summary>
    /// 디버프 포함 데미지 계산 등 확장용 함수
    /// </summary>
    public virtual int DecreasedDamage()
    {
        return currentStat[StatType.Atk];
    }

    public Vector3 GetTargetPoint()
    {
        return targetPoint;
    }
    
    public Vector3 GetTopPoint()
    {
        return targetPoint + Vector3.up * (spriteRenderer.bounds.size.y * 0.5f);
    }
    
    public Vector3 GetUpperCenter(float height = 0)
    {
        return transform.position + Vector3.up * (spriteRenderer.bounds.size.y + height);
    }
    
    public virtual void ReduceSkillCooldown()
    {
        if (currentSkillCooldown > 0)
        {
            currentSkillCooldown--;
        }
    }
    #region 상태효과 관련 메서드들
    public void ApplyStatusEffect(StatusEffectType statusEffectType, int duration, float value)
    {
        statusEffectController.ApplyEffect(statusEffectType, duration, value);
    }

    public bool HasStatusEffect(StatusEffectType statusEffectType)
    {
        return statusEffectController.HasEffect(statusEffectType);
    }

    public void ReduceStatusEffectDuration()
    {
        statusEffectController.ReduceEffectDuration();
    }

    public void RemoveAllStatusEffects()
    {
        statusEffectController.RemoveAllEffects();
    }
    #endregion
    
    /// <summary>
    /// 풀에서 꺼낸 후 호출
    /// </summary>
    public void SetSpawnPrefab(GameObject go)
    {
        spawnedPrefab = go;
    }

    /// <summary>
    /// 풀에 반환함
    /// </summary>
    public void DespawnPrefab(string prefabKey)
    {
        if (spawnedPrefab != null)
        {
            ObjectPoolManager.Instance.Return(PoolCategory.Entity, prefabKey, spawnedPrefab);
            spawnedPrefab = null;
        }
    }
}
