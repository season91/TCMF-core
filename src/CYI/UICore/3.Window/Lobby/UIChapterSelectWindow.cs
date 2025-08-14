using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChapterSelectWindow : UIBase
{
    [SerializeField] private Image imgBg;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnPrev;
    [SerializeField] private Button btnNext;
    
    [SerializeField] private List<UIWgStageBtn> stageBtnList = new ();
    
    [SerializeField] private TextMeshProUGUI tmpChapterTitle;

    private int curChapter;
    private readonly SlideOpenContext slideContext = new();
    private const string NoUnlockComment = "이전 스테이지를 먼저 클리어 하세요.";
    
    protected override void Reset()
    {
        base.Reset();

        imgBg = transform.FindChildByName<Image>("Group_SelectedChapter");
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        btnPrev = transform.FindChildByName<Button>("Btn_PreviousChapter");
        btnNext = transform.FindChildByName<Button>("Btn_NextChapter");
        
        stageBtnList = transform.FindChildByName<Transform>("Layout_StageButtons")
            .GetComponentsInChildren<UIWgStageBtn>().ToList();
        
        tmpChapterTitle = transform.FindChildByName<TextMeshProUGUI>("Tmp_ChapterTitle");
    }

    public override void Initialize()
    {
        base.Initialize();
        
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(OnClose);
        
        btnPrev.onClick.RemoveAllListeners();
        btnNext.onClick.RemoveAllListeners();
        btnPrev.AddListener(OnPrev);
        btnNext.AddListener(OnNext);

        for (int i = 0; i < stageBtnList.Count; i++)
        {
            int stageNum = i + 1;
            
            stageBtnList[i].Initialize(OnStageSelect, stageNum);
        }
    }

    public override void Open(OpenContext openContext = null)
    {
        UIManager.Instance.Open<UIWcUserInfo>();
        base.Open(openContext);
        // 현재 챕터에 따라 세팅
        // 챕터1 로 시작 고정
        curChapter = 1;
        UpdateStageGUI();
        UIManager.Instance.ChangeBg(StringAdrBg.LobbyBlur);
        SoundManager.Instance.PlayBgm(StringAdrAudioBgm.BattleReady);
        AnalyticsHelper.LogScreenView(AnalyticsBattleScreen.SelectStage, this.GetType().Name);
    }
    
    public override void Close(CloseContext closeContext = null)
    {
        base.Close(closeContext);
        UIManager.Instance.Close<UIWcUserInfo>();
    }
    
    private void UpdateStageGUI()
    {
        imgBg.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrBg.ChapterSelectList[curChapter]);
        
        // 챕터에 따른 스테이지 Data 불러오기
        List<StageData> curStageDatas = StageManager.Instance.StageDataListByChapter[curChapter];

        tmpChapterTitle.text = curStageDatas[0].ChapterTitle;
        
        for (int i = 0; i < stageBtnList.Count; i++)
        {
            bool isUnlocked = UserData.stage.IsStageUnlocked(curChapter, i + 1);
            string monsterName = isUnlocked ? StringAdrEntity.EntityDict[curStageDatas[i].ChapterSelectMonsterCode]
                : StringAdrEntity.EntityShadeDict[curStageDatas[i].ChapterSelectMonsterCode];
            Sprite monsterImage = ResourceManager.Instance.GetResource<Sprite>(monsterName);
            StageData stageData = curStageDatas[i];
            string stageText = stageData.StageNumber == 3 ? " Boss" : $"-{stageData.StageNumber}";
            string stageTitle = $"Stage {stageData.ChapterNumber}{stageText}";
            stageBtnList[i].Show(monsterImage, stageTitle, isUnlocked);
        }
    }
    
    private void ChangeChapter(int changeNum)
    {
        int chapterCount = StageManager.Instance.StageDataListByChapter.Count;
        int changeChapter = curChapter + changeNum;

        // 무한 루프, 1 ~ Chapter Count 범위 유지
        if (changeChapter > chapterCount)
            changeChapter = 1;
        else if (changeChapter <= 0)
            changeChapter = chapterCount;

        curChapter = changeChapter;

        UpdateStageGUI();
    }
    
    private void OnStageSelect(int stageNum, bool isUnlocked)
    {
        if (!isUnlocked)
        {
            slideContext.Comment = NoUnlockComment;
            UIManager.Instance.Open<UIPGlobalSlide>(OpenContext.WithContext(slideContext));
            return;
        }
        
        Close();
        UIManager.Instance.Open<UIUnitSelectWindow>();
        StageManager.Instance.SetCurStageData(curChapter, stageNum);
    }
    
    private void OnPrev() => ChangeChapter(-1);
    private void OnNext() => ChangeChapter(1);
    private void OnClose()
    {
        Close();
        _ = UIManager.Instance.EnterLoadingAsync(SceneType.Lobby);
    }
}
