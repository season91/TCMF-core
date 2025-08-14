using UnityEngine;
using UnityEngine.UI;

public class UIPGameOption : UIBase
{
    [Header("====[Sound]")]
    [SerializeField] private Slider sliderBgm;
    [SerializeField] private Slider sliderSfx;

    [Header("====[Buttons]")]
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnSelect;
    [SerializeField] private Button btnLobby;

    protected override void Reset()
    {
        base.Reset();

        sliderBgm = transform.FindChildByName<Slider>("Slider_Bgm");
        sliderSfx = transform.FindChildByName<Slider>("Slider_Sfx");
        
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        btnSelect = transform.FindChildByName<Button>("Btn_Select");
        btnLobby = transform.FindChildByName<Button>("Btn_Lobby");
    }

    public override void Initialize()
    {
        base.Initialize();
        
        sliderBgm.onValueChanged.RemoveAllListeners();
        sliderBgm.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.Bgm, value));
        sliderSfx.onValueChanged.RemoveAllListeners();
        sliderSfx.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.Sfx, value));
        
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(OnClose);
        btnSelect.onClick.RemoveAllListeners();
        btnSelect.AddListener(OnStageSelect);
        btnLobby.onClick.RemoveAllListeners();
        btnLobby.AddListener(OnLobby);
    }

    public override void Open(OpenContext openContext = null)
    {
        UIManager.Instance.GetUI<UIBattleWindow>().BaseClose();
        GameManager.Instance.SetTimeScale(false);
        
        StageData data = StageManager.Instance.CurStageData;
        string stageBgAdr = data.IsBossStage
            ? StringAdrBg.ChapterBossBlurList[data.ChapterNumber]
            : StringAdrBg.ChapterBlurList[data.ChapterNumber];
        UIManager.Instance.ChangeBg(stageBgAdr);
        
        sliderBgm.value = SoundManager.Instance.GetVolume(SoundType.Bgm);
        sliderSfx.value = SoundManager.Instance.GetVolume(SoundType.Sfx);
        
        base.Open(openContext);
    }

    private void OnClose() => Close();
    
    public override void Close(CloseContext closeContext = null)
    {
        UIManager.Instance.GetUI<UIBattleWindow>().BaseOpen();
        StageData data = StageManager.Instance.CurStageData;
        string stageBgAdr = data.IsBossStage
            ? StringAdrBg.ChapterBossList[data.ChapterNumber]
            : StringAdrBg.ChapterList[data.ChapterNumber];
        UIManager.Instance.ChangeBg(stageBgAdr);
        
        GameManager.Instance.SetTimeScale(true);
        base.Close(closeContext);
    }

    private void OnStageSelect()
    {
        // BattleManager.Instance.CleanupBattle();
        // Close(CloseContext.WithCallback(UIManager.Instance.Open<UIChapterSelect>));
        Close();
        _ = UIManager.Instance.EnterLoadingAsync(SceneType.Battle);
    }
    private void OnLobby()
    {
        Close();
        _ = UIManager.Instance.EnterLoadingAsync(SceneType.Lobby);
    }
}
