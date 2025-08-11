using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public enum BattleState
{
    Start,
    Win,
    Lose,
    Interrupted
}
public enum BattleMode
{
    Auto,
    Manual
}
/// <summary>
/// 전투 시작, 끝 흐름만 담당
/// </summary>
public class BattleManager : Singleton<BattleManager>, IBattleServices
{
    private ActionManager actionManager;
    private TurnManager turnManager;
    private ManualInputHandler manualInputHandler;
    private TargetingSystem targetingSystem;
    private AnimationController animationController;
    private EffectSpawner effectSpawner;
    private EffectProvider effectProvider;
    private ProjectileLauncher projectileLauncher;
    
    public IBattleActionFacade Actions { get; private set; }
    public IBattleTargetingFacade Targeting { get; private set; }
    public IBattleEffectFacade Effects { get; private set; }
    public IBattleUIFacade UI { get; private set; }
    public IBattleFlowFacade Flow { get; private set; }
    public IBattleInputFacade Input { get; private set; }

    public IReadOnlyList<Unit> Units;
    public IReadOnlyList<Monster> Monsters;
    IReadOnlyList<Unit> IBattleServices.Units => Units;
    IReadOnlyList<Monster> IBattleServices.Monsters => Monsters;
    
    public IAnimationController AnimationController => animationController;
    
    private bool isEndBattle = false;

    /// <summary>
    /// BattleScene 초기화
    /// </summary>
    public void Setting()
    {
        // battle 인벤토리, 유닛 기본 정렬
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryByAtkDesc();
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryByDefDesc();
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryUnitsByCodeAsc();
        
        // pool 등록
        PoolRegister.RegisterUnitPrefabs();
        PoolRegister.RegisterMonsterPrefabs();
        PoolRegister.RegisterEffectPrefabs();
    }

    public void UISetting()
    {
        // 처음 오픈할 부분 
        UIManager.Instance.Open<UIChapterSelect>();
    }
    
    /// <summary>
    /// BattleManager 초기화 함수
    /// 단순히 유닛 리스트 초기화 및 전투 상태만 리셋함
    /// 턴 정보 및 상태 변수는 다른 매니저에서 담당
    /// </summary>
    public void Initialize()
    {
        StageManager.Instance.Initialize();
        
        actionManager = new ActionManager(this);
        targetingSystem = new TargetingSystem();
        animationController = new AnimationController();
        manualInputHandler = GetComponent<ManualInputHandler>() ?? gameObject.AddComponent<ManualInputHandler>();
        turnManager = GetComponent<TurnManager>() ?? gameObject.AddComponent<TurnManager>();
        effectSpawner = GetComponent<EffectSpawner>() ?? gameObject.AddComponent<EffectSpawner>();
        effectProvider = GetComponent<EffectProvider>() ?? gameObject.AddComponent<EffectProvider>();
        projectileLauncher = new ProjectileLauncher(this);
        manualInputHandler.Initialize(this);
        
        Actions = new BattleActionFacade(actionManager);
        Targeting = new BattleTargetingFacade(targetingSystem, this);
        Effects = new BattleEffectFacade(effectSpawner, projectileLauncher);
        UI = new BattleUIFacade();
        Flow = new BattleFlowFacade(this);
        Input = new BattleInputFacade(manualInputHandler, turnManager);

        Flow.Initialize();
    }
    
    /// <summary>
    /// 전투 전 유닛 및 몬스터 초기화, 보스 여부 설정, 턴 루프 시작
    /// </summary>
    public void SetupBattle()
    {
        isEndBattle = false;
        MyDebug.Log("SetUpBattle");

        // 유닛/몬스터 정보 초기화
        Units = StageManager.Instance.GetActiveUnitList();
        Monsters = StageManager.Instance.GetActiveMonsterList();

        InitCharacterWithServices();
        
        // 전투 상태 초기화
        Flow.Initialize();

        // 각 매니저 초기화
        turnManager.Initialize(this);
        turnManager.StartTurnLoop();
        
        MyDebug.Log($"플레이어 유닛 수 : {Units.Count} 몬스터 수 : {Monsters.Count} 초기화 완료");
    }

    private void InitCharacterWithServices()
    {
        foreach (var unit in Units)
        {
            unit.Initialize(this);
        }

        foreach (var monster in Monsters)
        {
            monster.Initialize(this);
        }
    }

    /// <summary>
    /// 전투 종료 조건을 확인하고, 끝났다면 EndBattle 호출
    /// 몬스터나 유닛 중 하나가 모두 죽었는지 확인후 전투 종료 처리
    /// - 유닛 모두 사망하면 패배 (BattleState.Lose)
    /// - 몬스터가 모두 사망하면 승리 (BattleState.Win)
    ///  이 메소드는 턴 종료 시점 또는 공격 직후 반드시 호출 (ActionManager, TurnManager)
    /// true : 전투가 종료되어 후속 로직이 필요 없음
    /// false : 전투가 아직 계속 중임
    /// </summary>
    public bool CheckBattleEnd()
    {
        bool battleEnded = Flow.CheckBattleEnd();
        if (battleEnded && !isEndBattle)
        {
            isEndBattle = true;
            _ = EndBattle();
        }
        return battleEnded;
    }

    // 전투 종료 후 보상 지급
    private async Task EndBattle()
    {
        switch (Flow.CurrentState)
        {
            case BattleState.Win:
                MyDebug.Log("전투 승리");
                SoundManager.Instance.PlaySfx(StringAdrAudioSfx.BattleWin);
                await HandleBattleVictoryAsync();
                break;

            case BattleState.Lose:
                MyDebug.Log("전투 패배");
                SoundManager.Instance.PlaySfx(StringAdrAudioSfx.BattleLose);
                await HandleBattleDefeatAsync();
                break;
        }
        SoundManager.Instance.PlaySfx(StringAdrAudioSfx.GetReward);
        CleanupBattle();
    }

    public void CleanupBattle()
    {
        Flow.SetBattleState(BattleState.Interrupted);
        Flow.ResetSpeedMode();
        //ClearAllCache();
        effectProvider.ReturnAllEffectsToPool();
        StageManager.Instance.InitStage();
        UIManager.Instance.Close<UIBattle>();
        StageManager.Instance.EntityAllDespawn();
    }
    
    // 전투 승리 후 흐름 처리 (보상, 진척도 저장, UI 등)
    private async Task HandleBattleVictoryAsync()
    {
        // 진척도 및 스테이지 정보 가져오기
        var progress = StageManager.Instance.GetOrCreateStageProgress();

        // 스테이지 보상 구성
        var rewards = RewardManager.Instance.ComposeVictoryRewards();
        
        // 진척도 갱신
        progress.isCleared = true;
        progress.rewardClaimed = true;
        await UserDataHandler.SaveStageProgress(progress);
        
        // 전투 결과 팝업 표시
        ShowBattleResultPopup(rewards, true, StageManager.Instance.IsLastStage());
        
        // 스테이지 보상 지급
        await RewardManager.Instance.GrantRewards(rewards);
        
        await Task.Yield();
        UIManager.Instance.DequeuePopup();
    }

    // 전투 패배 후 처리 (보상, UI)
    private async Task HandleBattleDefeatAsync()
    {
        var rewards = RewardManager.Instance.ComposeFailureRewards();
        
        ShowBattleResultPopup(rewards, false, StageManager.Instance.IsLastStage());
        
        await RewardManager.Instance.GrantRewards(rewards);
       
        await Task.Yield();
        UIManager.Instance.DequeuePopup();
    }

    private void ShowBattleResultPopup(List<RewardData> rewards, bool isVictory, bool isLastStage)
    {
        UIManager.Instance.EnqueuePopup<UIBattleResultPopup>(
            OpenContext.WithContext(new ResultOpenContext
            {
                RewardList = rewards,
                IsVictory = isVictory,
                IsLastStage = isLastStage
            }));
    }
  
    // 사망 스킬 버튼 제거
    public void UpdateHideDieUnitSkillButton(Unit unit)
    {
        int index = -1;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i] == unit)
            {
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            MyDebug.Log("죽은 유닛 !!" + index);
            UI.UpdateHideDieUnitSkillButton(index);
        }
        else
        {
            MyDebug.LogWarning("Unit 인덱스를 찾을 수 없습니다.");
        }
    }
    public void ApplyDamage(CharacterBase attacker, CharacterBase target)
        =>actionManager.ApplyDamage(attacker, target);
    
    public void ApplySplitDamage(CharacterBase character, CharacterBase target, bool isMainTarget)
        =>actionManager.ApplySplitDamage(character, target, isMainTarget);
    
    public GameObject SpawnStatusEffect(CharacterBase caster, CharacterBase target, StatusEffectType statusEffectType)
    => Effects.SpawnStatusEffect(caster, target, statusEffectType);
    
    public BossAttackPattern GetBossAttackPattern(string monsterCodeNumber)
        => Targeting.GetBossAttackPattern(monsterCodeNumber);
    
    public new Coroutine StartCoroutine(IEnumerator routine)
    {
        return base.StartCoroutine(routine);
    }
}
