using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleOpenContext : IUIOpenContext
{
    public IReadOnlyList<Unit> UnitList;
    public bool IsBossStage;
    public string BossName;
}

public class UIBattleWindow : UIBase
{
    protected override ContentType ContentType => ContentType.Battle;
    
    [SerializeField] private Button btnPause;
    
    [Header("====[Skill]")]
    [SerializeField] private UIWgSkillBtn[] guiSkillButtons;
    [SerializeField] private TextMeshProUGUI tmpToggleAuto;
    [SerializeField] private TextMeshProUGUI tmpToggleX2;
    [SerializeField] private UIWgGlobalToggle uiToggleAuto;
    [SerializeField] private UIWgGlobalToggle uiToggleX2;
    [SerializeField] private ParticleSystem psSkillLine;
    private readonly Color colorToggleTxt = new (230 / 255f, 174 / 255f, 72 / 255f, 255 / 255f);
    private readonly Vector2 psSkillLinePos = new (150, 150);
    private readonly Vector2 skillZoomSize = new (300, 300);
    private readonly Vector2 skillOriginSize = new (230, 230);

    [Header("====[Round Info]")]
    [SerializeField] private CanvasGroup canvasGroupRoundInfo;
    [SerializeField] private Image imgRoundInfo;
    // [SerializeField] private TextMeshProUGUI tmpTurnInfo;

    [Header("====[Stage Info]")]
    [SerializeField] private GameObject objBossStageInfo;
    [SerializeField] private TextMeshProUGUI tmpBossName;

    [SerializeField] private RectTransform rectTrStage;
    [SerializeField] private TextMeshProUGUI tmpStage;
    [SerializeField] private TextMeshProUGUI tmpRound;
    
    [Header("====[Entity State]")]
    [SerializeField] private UIPDamage damage;
    [SerializeField] private Image imgCurTurnMark;
    
    private BattleOpenContext battleContext;
    private const int NormalInfoPosY = -80;
    private const int BossInfoPosY = -200;
    private const int MarkUpPos = 150;
    
    protected override void Reset()
    {
        base.Reset();

        btnPause = transform.FindChildByName<Button>("Btn_Pause");
        
        guiSkillButtons = transform.FindChildByName<Transform>("Layout_SkillButtons").GetComponentsInChildren<UIWgSkillBtn>();
        tmpToggleAuto = transform.FindChildByName<TextMeshProUGUI>("Tmp_SliderAuto");
        tmpToggleX2 = transform.FindChildByName<TextMeshProUGUI>("Tmp_SliderX2");
        uiToggleAuto = transform.FindChildByName<UIWgGlobalToggle>("ToggleSystem_Auto");
        uiToggleX2 = transform.FindChildByName<UIWgGlobalToggle>("ToggleSystem_X2");
        psSkillLine = transform.FindChildByName<ParticleSystem>("FX_UI_SkillLine");
        
        canvasGroupRoundInfo = transform.FindChildByName<CanvasGroup>("Group_RoundInfo");
        imgRoundInfo = transform.FindChildByName<Image>("Group_RoundInfo");
        // tmpTurnInfo = transform.FindChildByName<TextMeshProUGUI>("Tmp_TurnInfo");
        
        objBossStageInfo = transform.FindChildByName<Transform>("Group_BossStage").gameObject;
        tmpBossName = objBossStageInfo.transform.FindChildByName<TextMeshProUGUI>("Tmp_BossName");
        
        rectTrStage = transform.FindChildByName<RectTransform>("Group_NormalStage");
        tmpStage = transform.FindChildByName<TextMeshProUGUI>("Tmp_Stage");
        tmpRound = transform.FindChildByName<TextMeshProUGUI>("Tmp_Round");
        
        damage = transform.FindChildByName<UIPDamage>("Group_Damage");
        imgCurTurnMark = transform.FindChildByName<Image>("Img_CurTurnMark");
    }

    public override void Initialize()
    {
        base.Initialize();
        
        btnPause.onClick.RemoveAllListeners();
        btnPause.AddListener(OpenPause);
        
        int index = 0;
        foreach (var guiSkillButton in guiSkillButtons)
        {
            guiSkillButton.Initialize(index);
            index++;
        }
        psSkillLine.Stop();
        
        damage.Initialize();
        
        BattleManager.Instance.UI.OnShowCurrentRound -= ShowCurrentRound;
        BattleManager.Instance.UI.OnShowCurrentRound += ShowCurrentRound;
        BattleManager.Instance.UI.OnUpdateStageInfo -= UpdateStageInfo;
        BattleManager.Instance.UI.OnUpdateStageInfo += UpdateStageInfo;
        
        BattleManager.Instance.UI.OnMarkAsCurrentEntity -= SetCurTurnMark;
        BattleManager.Instance.UI.OnMarkAsCurrentEntity += SetCurTurnMark;
        
        BattleManager.Instance.UI.OnUpdateSkillCool -= UpdateAllSkillCool;
        BattleManager.Instance.UI.OnUpdateSkillCool += UpdateAllSkillCool;
        BattleManager.Instance.UI.OnHideDieUnitSkillButton -= HideSkillButton;
        BattleManager.Instance.UI.OnHideDieUnitSkillButton += HideSkillButton;
        
        BattleManager.Instance.UI.OnStartUseSkillWaitingGUI -= StartUseSkillWaitingGUI;
        BattleManager.Instance.UI.OnStartUseSkillWaitingGUI += StartUseSkillWaitingGUI;
        BattleManager.Instance.UI.OnEndUseSkillWaitingGUI -= EndUseSkillWaitingGUI;
        BattleManager.Instance.UI.OnEndUseSkillWaitingGUI += EndUseSkillWaitingGUI;
        BattleManager.Instance.UI.OnUseSkillWaitingCool -= UpdateWaitingGUI;
        BattleManager.Instance.UI.OnUseSkillWaitingCool += UpdateWaitingGUI;
    }

    public void BaseOpen()
    {
        base.Open();
        UIManager.Instance.Open<UIWgHpBar>();
    }

    public void BaseClose()
    {
        base.Close();
        UIManager.Instance.Close<UIWgHpBar>();
    }
    
    public override void Open(OpenContext openContext = null)
    {
        if (openContext == null)
        {
            if(battleContext == null) return;
        }
        else
        {
            if(openContext.Context is not BattleOpenContext castingContext) return;
            battleContext = castingContext;
        }
        
        canvasGroupRoundInfo.alpha = 0;
        
        SetSkillButton(battleContext.UnitList);
        SetStageInfo();
        SetToggle();
        
        base.Open(openContext);
        UIManager.Instance.Open<UIWgHpBar>();
        AnalyticsHelper.LogScreenView(tmpStage.text, this.GetType().Name);
    }

    public override void Close(CloseContext closeContext = null)
    {
        base.Close(closeContext);
        UIManager.Instance.Close<UIWgHpBar>();
    }

    /// <summary>
    /// 죽은 유닛의 스킬 비활성화 이벤트 바인딩 메서드
    /// </summary>
    private void HideSkillButton(int index)
    {
        guiSkillButtons[index].Hide();
    }

    /// <summary>
    /// Skill Button 초기 세팅
    /// </summary>
    private void SetSkillButton(IReadOnlyList<Unit> unitList)
    {
        for (int i = 0; i < guiSkillButtons.Length; i++)
        {
            if (i < unitList.Count)
            {
                string unitCode = unitList[i].UnitData.Code;
                Sprite skillIcon = MasterData.SkillDataDict[unitCode].Icon;
                Sprite skillShadeIcon =
                    ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.UnitSkillShade[unitCode]);
                guiSkillButtons[i].Show(skillIcon, skillShadeIcon, unitList[i].IsSkillReady, unitList[i].skillCooldown);
                guiSkillButtons[i].ChangeSizeSkillButton(skillOriginSize);
            }
            else
            {
                guiSkillButtons[i].Hide();
            }
        }
    }

    /// <summary>
    /// Toggle 초기 세팅
    /// </summary>
    private void SetToggle()
    {
        // Auto면 On
        bool isToggleAutoOn = BattleManager.Instance.Flow.CurrentMode == BattleMode.Auto;
        tmpToggleAuto.color = isToggleAutoOn ? colorToggleTxt : Color.white;
        uiToggleAuto.Initialize(isToggleAutoOn, ToggleAuto);
        bool isX2On = BattleManager.Instance.Flow.IsDoubleSpeed;
        uiToggleX2.Initialize(isX2On, ToggleX2);
        tmpToggleX2.color = isX2On ? colorToggleTxt : Color.white;
    }
    
    /// <summary>
    /// 초기 Stage Info 세팅
    /// </summary>
    private void SetStageInfo()
    {
        StageData stageData = StageManager.Instance.CurStageData;
        
        objBossStageInfo.SetActive(stageData.StageNumber == 3);
        if (battleContext.IsBossStage)
            tmpBossName.text = battleContext.BossName;
        
        rectTrStage.anchoredPosition = new Vector2(
            rectTrStage.anchoredPosition.x, 
            battleContext.IsBossStage? BossInfoPosY : NormalInfoPosY);
        tmpStage.text = $"Stage {stageData.ChapterNumber}-{stageData.StageNumber}";
        UpdateStageInfo(1);
    }

    /// <summary>
    /// 현재 Stage Info 세팅
    /// </summary>
    private void UpdateStageInfo(int roundCount)
    {
        String roundText = $"Round {roundCount}";
        tmpRound.text = roundText;
    }

    /// <summary>
    /// 현재 턴 유닛의 스킬 가시성: 현재 턴 유닛의 스킬 사용 차례시 호출
    /// - 사이즈 업, 테두리 이펙트 On
    /// </summary>
    /// <param name="unitIndex">현재 Unit Index</param>
    private void StartUseSkillWaitingGUI(int unitIndex)
    {
        guiSkillButtons[unitIndex].StartUseSkillWaiting(skillZoomSize);
        psSkillLine.transform.SetParent(guiSkillButtons[unitIndex].transform);
        psSkillLine.transform.localPosition = psSkillLinePos;
        psSkillLine.Play();
    }
    
    /// <summary>
    /// 현재 턴 유닛의 스킬 가시성: 현재 턴 유닛의 스킬 사용 차례 종료 시 호출
    /// - 사이즈 다운, 테두리 이펙트 Off, 스킬 사용 시 아이콘 Shade
    /// </summary>
    /// <param name="unitIndex">현재 Unit Index</param>
    /// <param name="isUseSkill">현재 Unit이 스킬을 사용하였는지?</param>
    private void EndUseSkillWaitingGUI(int unitIndex, bool isUseSkill)    
    {
        psSkillLine.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        guiSkillButtons[unitIndex].EndUseSkillWaiting(skillOriginSize, isUseSkill);
        imgCurTurnMark.enabled = false;
    }

    /// <summary>
    /// 현재 유닛의 스킬 버튼의 Fill Image 시간 비율
    /// -> 0: 시작, 1: 종료
    /// </summary>
    /// <param name="unitIndex">현재 유닛 인덱스</param>
    /// <param name="timeRatio">스킬 대기 시간 비율</param>
    private void UpdateWaitingGUI(int unitIndex, float timeRatio)
    {
        guiSkillButtons[unitIndex]?.UpdateWaitingGUI(timeRatio);
    }

    /// <summary>
    /// Delegate 함수
    /// Battle Manager => OnUpdateSkillCool에 바인딩
    /// 화면상 보이는 스킬 쿨에 대한 정보 업데이트
    /// </summary>
    private void UpdateAllSkillCool(IReadOnlyList<Unit> unitList)
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            guiSkillButtons[i].UpdateSkillCool(unitList[i].IsSkillReady, unitList[i].currentSkillCooldown);
        }
    }
    
    /// <summary>
    /// Delegate 함수
    /// Battle Manager => OnShowCurrentTurn => 8/8 Round로 수정
    /// 유닛 <=> 몬스터의 턴이 변경될 시, 화면에 잠깐 띄우는 Turn 표시 => 8/8 Unit 턴 시작시 Round 표시 
    /// </summary>
    private void ShowCurrentRound(Action onComplete = null)
    {
        // string turnInfoBgAdr = turnPhase == TurnPhase.UnitTurn ? StringAdrUI.TurnUser : StringAdrUI.TurnMonster;
        // imgTurnInfo.sprite = ResourceManager.Instance.GetResource<Sprite>(turnInfoBgAdr);
        // imgTurnInfo.SetNativeSize();
        // tmpTurnInfo.text = ((turnPhase == TurnPhase.UnitTurn ? "YOUR" : "MONSTER") + "\nTURN").ToUpper();
        canvasGroupRoundInfo.FadeInOut(1, 0, 0.2f, 0.5f, onComplete);
    }

    private void SetCurTurnMark(Vector2 worldPosition)
    {
        imgCurTurnMark.enabled = true;
        
        var screenPos = UIUtility.WorldToCanvasPosition(
            canvas,
            worldPosition, 
            UIManager.Instance.MainCamera
        );

        screenPos += Vector2.up * MarkUpPos;
        imgCurTurnMark.rectTransform.anchoredPosition = screenPos;
    }
    
    /// <summary>
    /// Auto 토글 클릭 기능
    /// </summary>
    private void ToggleAuto()
    {
        BattleManager.Instance.Flow.ToggleBattleMode();
        bool isToggleAutoOn = BattleManager.Instance.Flow.CurrentMode == BattleMode.Auto;
        tmpToggleAuto.color = isToggleAutoOn ? colorToggleTxt : Color.white;
    }

    /// <summary>
    /// X2 토글 클릭 기능
    /// </summary>
    private void ToggleX2()
    {
        BattleManager.Instance.Flow.ToggleSpeedMode();
        bool isDoubleSpeed = BattleManager.Instance.Flow.IsDoubleSpeed;
        tmpToggleX2.color = isDoubleSpeed ? colorToggleTxt : Color.white;
    }
    
    private void OpenPause() => UIManager.Instance.Open<UIPGameOption>();
}
