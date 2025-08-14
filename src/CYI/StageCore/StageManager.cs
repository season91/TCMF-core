using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EntityType
{
    Unit,
    Monster
}

/// <summary>
/// 스테이지 초기화 ,시작, 종료
/// </summary>
public class StageManager : Singleton<StageManager>
{
    private readonly StageDataLoader stageDataLoader = new();
    private readonly SelectedUnitManager selectedUnitManager = new();
    private EntitySpawner entitySpawner;
    private StageProgressService progressService;
    
    public Dictionary<int, List<StageData>> StageDataListByChapter => stageDataLoader.StageDataListByChapter;
    public StageData CurStageData { get; private set; }
   
    #region SelctedUnit

    public int GetCatsiteCount() 
        => selectedUnitManager.UnitCount;

    public int GetReadyUnitCount()
        => selectedUnitManager.GetReadyUnitCount();
    public int GetCatsiteIndexByUnitIndex(int unitIndex)
        => selectedUnitManager.GetCatsiteIndexByUnitIndex(unitIndex);
    public bool HasUnit(int catsiteIndex) 
        => selectedUnitManager.HasUnit(catsiteIndex);
    public void SetSelectedUnitInSlot(InventoryUnit unit, int index) 
        => selectedUnitManager.SetUnit(unit, index);
    public bool IsSelectedUnit(InventoryUnit unit) 
        => selectedUnitManager.IsSelected(unit);
    public InventoryUnit GetSelectedUnitByIndex(int catsiteIndex) 
        => selectedUnitManager.GetUnit(catsiteIndex);
    public void ClearSelectedUnit(int index) 
        => selectedUnitManager.Clear(index);
    public void ClearAllSelectedUnits() 
        => selectedUnitManager.ClearAll();
    
    #endregion

    [SerializeField] private List<Unit> unitObjectList = new ();
    [SerializeField] private List<Monster> monsterObjectList = new ();
    public IReadOnlyList<Unit> GetActiveUnitList() => entitySpawner.GetActiveUnits();
    public IReadOnlyList<Monster> GetActiveMonsterList() => entitySpawner.GetActiveMonsters();
    
    private void Reset()
    {
        unitObjectList = FindObjectsOfType<Unit>().ToList();
        // entitiesDict[EntityType.Unit] = unitList.Cast<CharacterBase>().ToList();
        monsterObjectList = FindObjectsOfType<Monster>().ToList();
        // entitiesDict[EntityType.Monster] = monsterList.Cast<CharacterBase>().ToList();
    }
    
    public void Initialize()
    {
        stageDataLoader.Initialize();
        entitySpawner = new EntitySpawner(unitObjectList, monsterObjectList);
        progressService = new  StageProgressService();
        
        InitStage();
    }
    
    public void InitStage()
    {
        // Entity 전부 Off
        entitySpawner.DeactivateAll();
        UIManager.Instance.GetUI<UIWgHpBar>()?.InitHpBar();
    }
    
    /// <summary>
    /// Stage 선택 시, Stage Data 세팅
    /// </summary>
    public void SetCurStageData(int chapterNum, int stageNum)
    {
        // 스테이지 정보 셋팅
        int chapterCode = chapterNum * 1000;
        string curStageCode = CodeType.Stage.GetFullCode(chapterCode + stageNum);
        CurStageData = MasterData.StageDataDict[curStageCode];
        MyDebug.Log("스테이지 정보 셋팅 !! " + CurStageData.ChapterNumber + "-" + CurStageData.StageNumber);
    }

    public bool IsLastStage()
    {
        int lastChapterNum = MasterData.StageDataDict.Count / 3;
        return CurStageData.ChapterNumber == lastChapterNum && CurStageData.StageNumber == 3;
    }
    
    /// <summary>
    /// 다음 스테이지로
    /// 스테이지 데이터 설정해주고 Unit Select
    /// </summary>
    public void NextStage()
    {
        if (CurStageData == null)
        {
            MyDebug.LogError("CurStageData is null. You must call => SetCurStageData");
            return;
        }
        
        int chapterNum = CurStageData.StageNumber == 3 ? CurStageData.ChapterNumber + 1 : CurStageData.ChapterNumber;
        int stageNum = CurStageData.StageNumber == 3 ? 1 : CurStageData.StageNumber + 1;

        SetCurStageData(chapterNum, stageNum);
        UIManager.Instance.Open<UIUnitSelectWindow>();
    }

    public void TryAgain(bool needSetting)
    {
        if (needSetting)
        {
            UIManager.Instance.Open<UIUnitSelectWindow>();
        }
        else
        {
            StartStage();
        }
    }
    
    /// <summary>
    /// Stage 시작 시
    /// Stage 구성 (맵 생성, 유닛 생성, 적 생성 등)
    /// </summary>
    public void StartStage()
    {
        if (CurStageData == null)
        {
            MyDebug.LogError("CurStageData is null. You must call => SetCurStageData");
            return;
        }
        
        if (!InventoryManager.Instance.Cache.CanAddItems(CurStageData.GetRewardItemCount()))
        {
            MyDebug.LogWarning("인벤토리 부족으로 스테이지 시작 불가");
            return;
        }
        
        // 스테이지 구조 및 오브젝트 생성 로직
        // 예: 맵 셋업, 스폰, 오브젝트 배치 등

        // Entity 세팅
        // entitySpawner.Spawn(EntityType.Unit, GetCatsiteCount()); // 3마리 틀만 만들어줌
        // entitySpawner.Spawn(EntityType.Monster, CurStageData.MonsterSpawn.Count);
        
        // 변경 로직
        entitySpawner.UnitSpawn(GetCatsiteCount()); // 3마리 틀만 만들어줌
        entitySpawner.MonsterSpawn(CurStageData.MonsterSpawn, CurStageData.IsBossStage);

        // UI 세팅
        BattleOpenContext context = new BattleOpenContext
        {
            UnitList = GetActiveUnitList(),
            IsBossStage = CurStageData.IsBossStage,
            BossName = entitySpawner.GetActiveMonsters()[0].UnitName
        };

        string stageBgAdr = CurStageData.IsBossStage
            ? StringAdrBg.ChapterBossList[CurStageData.ChapterNumber]
            : StringAdrBg.ChapterList[CurStageData.ChapterNumber];
        UIManager.Instance.ChangeBg(stageBgAdr);
        
        // 배틀 셋업
        BattleManager.Instance.SetupBattle();
        UIManager.Instance.Open<UIBattleWindow>(OpenContext.WithContext(context));
        
        if(CurStageData.StageNumber == 3)
        {
            SoundManager.Instance.PlayBgm(StringAdrAudioBgm.BattleBoss);
        }
        else
        {
            SoundManager.Instance.PlayBgm(StringAdrAudioBgm.BattleScene);
        }
    }
 
    public StageProgress GetOrCreateStageProgress()
    {
        string stageCode = CurStageData.Code;
        return progressService.GetProgressCreate(stageCode);
    }
    
    /// <summary>
    /// 해당 챕터 전체 클리어 여부 확인
    /// </summary>
    public int UnitUnlockCount()
    {
        int unlockCount = 1;
        
        if (UserData.stage.IsChapterAllCleared(1))
        {
            unlockCount++;
        }
        
        if (UserData.stage.IsChapterAllCleared(2))
        {
            unlockCount++;
        }
        
        return unlockCount;   
    }
    
    public StageData GetCurrentStageData() => CurStageData;

    public void EntityAllDespawn()
    {
        entitySpawner.EntityDespawn();
    }
}
