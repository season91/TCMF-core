using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultOpenContext : IUIOpenContext 
{
    public List<RewardData> RewardList;
    public bool IsVictory;
    public bool IsLastStage = false;
}

public class UIPBattleResult : UIBasePopup
{
    [SerializeField] private Image imgResultTitle;
    [SerializeField] private UIWgResult uiWgResult;
    [SerializeField] private Button btnNext;
    [SerializeField] private TextMeshProUGUI tmpBtnNext;
    [SerializeField] private Button btnRestart;
    [SerializeField] private TextMeshProUGUI tmpBtnRestart;

    private const string RestartComment = "다시하기";
    private const string SelectComment = "스테이지 선택";
    private const string NextComment = "다음 스테이지";
    
    protected override void Reset()
    {
        base.Reset();
        
        imgResultTitle = transform.FindChildByName<Image>("Img_ResultTitle");
        uiWgResult = GetComponentInChildren<UIWgResult>();
        btnNext = transform.FindChildByName<Button>("Btn_Next");
        tmpBtnNext = transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnNext");
        btnRestart = transform.FindChildByName<Button>("Btn_Restart");
        tmpBtnRestart = transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnRestart");
    }

    public override void Initialize()
    {
        base.Initialize();
        uiWgResult.Initialize();
    }

    public override void Open(OpenContext openContext = null)
    {
        if (OpenContext.Context is not ResultOpenContext castingContext) return;
        
        SettingGUI(castingContext.IsVictory, castingContext.IsLastStage);
        uiWgResult.Show(castingContext.RewardList);
        
        base.Open(OpenContext);
        
        if (castingContext.IsVictory)
        {
            AnalyticsHelper.LogScreenView(AnalyticsBattleScreen.Win, this.GetType().Name);
        }
        else
        {
            AnalyticsHelper.LogScreenView(AnalyticsBattleScreen.Lose, this.GetType().Name);
        }
    }

    /// <summary>
    /// 승리 여부에 따른 GUI 세팅
    /// 결과 이미지, 다음 버튼 텍스트 및 이벤트
    /// </summary>
    private void SettingGUI(bool isVictory, bool isLastStage)
    {
        string resultImageName = isVictory ? StringAdrUI.Victory : StringAdrUI.Defeat;
        imgResultTitle.sprite = ResourceManager.Instance.GetResource<Sprite>(resultImageName);
        imgResultTitle.SetNativeSize();
        
        bool isFinalWin = isLastStage && isVictory;
        if (!isFinalWin)
        {
            btnNext.onClick.RemoveAllListeners();
            btnNext.AddListener(isVictory ? OnNextStage : OnStageSelect);
            tmpBtnNext.text = isVictory ? NextComment : SelectComment;
        }
        else
        {
            btnNext.onClick.RemoveAllListeners();
            // btnNext.AddListener(OnStageSelect);
            btnNext.AddListener(() => _ = UIManager.Instance.EnterWithoutLoadingAsync(SceneType.Ending));
            tmpBtnNext.text = SelectComment;
        }
        btnRestart.onClick.RemoveAllListeners();
        btnRestart.AddListener(isVictory? OnTryAgain : OnCurStageSetting);
        tmpBtnRestart.text = RestartComment;
    }

    /// <summary>
    /// 게임 승리 시, 다음 스테이지로
    /// </summary>
    private void OnNextStage()
    {
        Close(CloseContext.WithCallback(StageManager.Instance.NextStage));
    }
    
    /// <summary>
    /// 현재 스테이지 다시하기
    /// </summary>
    private void OnTryAgain()
    {
        Close(CloseContext.WithCallback(() => StageManager.Instance.TryAgain(false)));
    }

    /// <summary>
    /// 현재 스테이지 세팅으로 가기
    /// </summary>
    private void OnCurStageSetting()
    {
        Close(CloseContext.WithCallback(() => StageManager.Instance.TryAgain(true)));
    }

    private void OnStageSelect()
    {
        Close(CloseContext.WithCallback(UIManager.Instance.Open<UIChapterSelectWindow>));
    }
}
