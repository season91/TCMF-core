using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPGuide : UIBase
{
    [SerializeField] private Button btnClose;
    
    [Header("====[Menu]")]
    private UIDynamicObjectPool<UIWgGuideMenuBtn> dynamicMenuPool;
    [SerializeField] private Transform menuRoot;
    [SerializeField] UIWgGuideMenuBtn uiWgGuideMenu;
    private readonly UIWgSelectBtnState menuSelectedState = new() { IsSelectedObj = true };
    private readonly UIWgSelectBtnState menuUnselectedState = new() { IsSelectedObj = false };

    [Header("====[Content]")]
    // Content
    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private RectTransform commentRectTr;
    [SerializeField] private TextMeshProUGUI tmpComment;
    private const int CommentOriginHeightY = 190;
    private const int CommentIncreaseHeightY = 510;
    [SerializeField] private Image imgGuide;
    // Content Page Info
    [SerializeField] private GameObject groupProgress;
    [SerializeField] private Button btnPrev;
    [SerializeField] private Button btnNext;
    [SerializeField] private TextMeshProUGUI tmpPageProgress;
    private ContentType curMenuContentType;
    private int curPage;
    private GuideData curGuideData;
    private const string EmptyFileName = "None";
    private readonly Dictionary<ContentType, UIWgGuideMenuBtn> menuBtnDict = new();
    
    protected override void Reset()
    {
        base.Reset();

        btnClose = transform.FindChildByName<Button>("Btn_Close");
        
        menuRoot = transform.FindChildByName<Transform>("Layout_GuideMenu");
        uiWgGuideMenu = menuRoot.FindChildByName<UIWgGuideMenuBtn>("GUI_GuideMenu");

        Transform contentRoot = transform.FindChildByName<Transform>("Group_GuideContent");
        tmpTitle = contentRoot.FindChildByName<TextMeshProUGUI>("Tmp_Title");
        commentRectTr = contentRoot.FindChildByName<RectTransform>("Tmp_Comment");
        tmpComment = contentRoot.FindChildByName<TextMeshProUGUI>("Tmp_Comment");
        imgGuide = contentRoot.FindChildByName<Image>("Img_Guide");

        groupProgress = transform.FindChildByName<Transform>("Group_PageInfo").gameObject;
        btnPrev = groupProgress.transform.FindChildByName<Button>("Btn_Prev");
        btnNext = groupProgress.transform.FindChildByName<Button>("Btn_Next");
        tmpPageProgress = groupProgress.transform.FindChildByName<TextMeshProUGUI>("Tmp_PageProgress");
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // InitSize => enum ContentType 크기
        var contentTypes = Enum.GetValues(typeof(ContentType));
        dynamicMenuPool = new UIDynamicObjectPool<UIWgGuideMenuBtn>(uiWgGuideMenu, menuRoot, contentTypes.Length);
        
        foreach (ContentType contentType in contentTypes)
        {
            // None과 큰 틀의 타이틀은 제외
            if (contentType is ContentType.None or ContentType.Gacha or ContentType.Battle or ContentType.Blacksmith or ContentType.Lobby) continue;
            var menuBtn = dynamicMenuPool.Get();
            menuBtn.Initialize(contentType, SetGUI, menuSelectedState, menuUnselectedState);
            menuBtnDict[contentType] = menuBtn;
        }
        
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(OnClose);
        btnPrev.onClick.RemoveAllListeners();
        btnPrev.AddListener(OnPrev);
        btnNext.onClick.RemoveAllListeners();
        btnNext.AddListener(OnNext);
    }

    public override void Open(OpenContext openContext = null)
    {
        foreach (var menuBtn in menuBtnDict)
        {
            menuBtn.Value.Unselect();
        }
        
        curMenuContentType = UIManager.Instance.CurContentType;
        curPage = 1;
        SetGUI(curMenuContentType);
        base.Open(openContext);
    }

    private void OnClose() => Close();

    /// <summary>
    /// Content Type에 맞는 가이드 GUI 세팅
    /// </summary>
    /// <param name="contentType">세팅하려는 가이드 enum Content Type</param>
    private void SetGUI(ContentType contentType)
    {
        if (menuBtnDict.TryGetValue(curMenuContentType, out var menuBtn))
        {
            menuBtn.Select();
        }
        
        if (curMenuContentType != contentType)
        {
            menuBtn?.Unselect();
            curMenuContentType = contentType;
            curPage = 1;
        }
        
        curGuideData = MasterData.GuideDataDict[curMenuContentType];
        
        SetContent();
        SetPageProgress();
    }
    private void SetContent()
    {
        tmpTitle.SetText(curMenuContentType == ContentType.Lobby ? string.Empty : curGuideData.SubTitle);
        string prompt = curGuideData.Prompts[curPage - 1];
        tmpComment.SetText(prompt);
        tmpComment.alignment = prompt.Length > 30 ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Top;
        string guideImgAdr = curGuideData.FileNames[curPage - 1];
        if (guideImgAdr == EmptyFileName)
        {
            imgGuide.enabled = false;
            commentRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CommentIncreaseHeightY);
        }
        else
        {
            imgGuide.enabled = true;
            Sprite guide = ResourceManager.Instance.GetResource<Sprite>(guideImgAdr);
            imgGuide.sprite = guide;
            imgGuide.SetNativeSize();
            commentRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CommentOriginHeightY);
        }
    }
    private void SetPageProgress()
    {
        if (curMenuContentType != ContentType.Lobby || curGuideData.Prompts.Count > 1)
        {
            groupProgress.SetActive(true);
            tmpPageProgress.SetText("{0}/{1}", curPage, curGuideData.Prompts.Count);
        }
        else
        {
            groupProgress.SetActive(false);
        }
    }

    private void ChangePage(int changeNum)
    {
        int pageCount = curGuideData.Prompts.Count;
        int changePage = curPage + changeNum;

        // 무한 루프, 1 ~ Page Count 범위 유지
        if (changePage > pageCount)
            changePage = 1;
        else if (changePage <= 0)
            changePage = pageCount;

        // GUI 업데이트
        curPage = changePage;
        SetGUI(curMenuContentType);
    }
    
    private void OnPrev() => ChangePage(-1);
    private void OnNext() => ChangePage(+1);
}
