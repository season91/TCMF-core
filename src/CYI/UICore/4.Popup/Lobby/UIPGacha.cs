using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 가챠 결과 - OpenContext
/// </summary>
public class GachaResultOpenContext : IUIOpenContext 
{
    public ResourceType CurType;
    public List<ItemData> ItemDataList;
}

/// <summary>
/// 가챠 연출 팝업
/// </summary>
public class UIPGacha : UIBasePopup
{
    [SerializeField] private Button btnClose;
    
    [Header("====[Normal Gacha]")]
    [SerializeField] private GameObject groupGacha;
    [SerializeField] private GameObject groupResult;
    [SerializeField] private List<UIWgCard> guiCardList = new ();
    [SerializeField] private Button btnResult;
    [SerializeField] private SpriteRenderer spriteRdrResult;
    [SerializeField] private ParticleSystem psResultBack;
    [SerializeField] private ParticleSystem psBackSmallStar;
    [SerializeField] private ParticleSystem psBackStar;
    [SerializeField] private ParticleSystem psBackGlow;
    [SerializeField] private ParticleSystem psBackCard;
    [SerializeField] private ParticleSystem psFlashBack;
    
    [Header("====[Video]")]
    [SerializeField] private VideoPlayer videoPlayerGacha;
    [SerializeField] private RawImage rawImgVideoPlayer;

    [SerializeField]private Image imgWhite;
    
    private Coroutine coroutineResult;
    private GachaResultOpenContext gachaContext;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        btnClose = transform.FindChildByName<Button>("Btn_Close");

        groupGacha = transform.FindChildByName<Transform>("Group_Gacha").gameObject;
        groupResult = transform.FindChildByName<Transform>("Layout_Cards").gameObject;
        guiCardList = groupResult.GetComponentsInChildren<UIWgCard>().ToList();
        btnResult = transform.FindChildByName<Button>("Btn_Result");
        
        psResultBack = transform.FindChildByName<ParticleSystem>("FX_UI_CardBack");
        spriteRdrResult = psResultBack.transform.FindChildByName<SpriteRenderer>("SpriteCard");
        psBackSmallStar = psResultBack.transform.FindChildByName<ParticleSystem>("smallstar");
        psBackStar = psResultBack.transform.FindChildByName<ParticleSystem>("Star");
        psBackGlow = psResultBack.transform.FindChildByName<ParticleSystem>("glow");
        psBackCard = psResultBack.transform.FindChildByName<ParticleSystem>("card");
        psFlashBack = transform.FindChildByName<ParticleSystem>("FX_UI_Cardflash");
        
        videoPlayerGacha = transform.FindChildByName<VideoPlayer>("VideoPlayer_Gacha");
        rawImgVideoPlayer = transform.FindChildByName<RawImage>("RawImage_Gacha");
        
        imgWhite = transform.FindChildByName<Image>("Img_White");
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록, NonUI 오브젝트 비활성화
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(Close);
        btnResult.onClick.RemoveAllListeners();
        btnResult.AddListener(OnHideBigResult);
        
        spriteRdrResult.gameObject.SetActive(false);
        psResultBack.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        psFlashBack.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        videoPlayerGacha.loopPointReached += EndGachaVideo;
    }
    
    /// <summary>
    /// NonUI 초기 상태 설정: 비활성화
    /// </summary>
    private void ResetNonUI()
    {
        if (coroutineResult != null)
        {
            StopCoroutine(coroutineResult);
            coroutineResult = null;
        }
        
        // GUI Card 전부 비활성화
        foreach (var guiCard in guiCardList)
        {
            guiCard.Hide();
        }
    }
    
    /// <summary>
    /// UI 열기: 처음 진입 => 영상 재생 메서드 호출, 미리 연출 초기화 - 흰커버 Off
    /// </summary>
    /// <param name="openContext">GachaResultOpenContext 필수</param>
    public override void Open(OpenContext openContext = null)
    {
        if (OpenContext.Context is not GachaResultOpenContext castingContext) return;

        gachaContext = castingContext;
        imgWhite.color = new Color(1, 1, 1, 0);
        StartCoroutine(ShowVideo());
    }
    
    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화
    /// </summary>
    public override void Close()
    {
        ResetNonUI();
        base.Close(CloseContext.WithCallback(UIManager.Instance.Open<UIGachaWindow>));
    }

    private void EndGachaVideo(VideoPlayer videoPlayer)
    {
        videoPlayer.Stop();
        imgWhite.DOFade(1, 0.5f);
        Invoke(nameof(ShowBigResult), 1f);
    }

    /// <summary>
    /// 가챠 영상 재생 => 영상이 준비되면 Open 및 영상 재생, 가챠 결과 세팅
    /// </summary>
    private IEnumerator ShowVideo()
    {
        groupGacha.SetActive(false);
        videoPlayerGacha.Prepare();
        yield return new WaitUntil(() => videoPlayerGacha.isPrepared);
     
        base.Open(OpenContext);
        
        // 가챠 영상 클릭 시, 스킵 기능 추가하기!!
        videoPlayerGacha.Play();
        SetResult();
    }
    
    /// <summary>
    /// 가장 높은 단계의 희귀도를 결과를 보여주는 Big 결과를 표시
    /// </summary>
    private void ShowBigResult()
    {
        imgWhite.color = new Color(1, 1, 1, 0);
        rawImgVideoPlayer.enabled = false;
        btnResult.gameObject.SetActive(true);
        
        spriteRdrResult.gameObject.SetActive(true);
        if(gachaContext.CurType == ResourceType.Diamond)
            spriteRdrResult.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrCardBg.DiaFront);
        else if(gachaContext.CurType == ResourceType.Gold)
            spriteRdrResult.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrCardBg.GoldFront);
        
        ItemRarity highestRarity = gachaContext.ItemDataList.Count > 0
            ? gachaContext.ItemDataList.Max(item => item.Rarity)
            : ItemRarity.None;

        ParticleSystem.MainModule main;
        
        psBackSmallStar.gameObject.SetActive(false);
        psBackStar.gameObject.SetActive(false);
        
        if (highestRarity == ItemRarity.SuperRare)
        {
            psBackStar.gameObject.SetActive(true);
            main = psBackStar.main;
            main.startColor = highestRarity.ToColor();
        }
        else if (highestRarity == ItemRarity.Legendary)
        {
            psBackSmallStar.gameObject.SetActive(true);
            psBackStar.gameObject.SetActive(true);
            main = psBackStar.main;
            main.startColor = highestRarity.ToColor();
        }
        main = psBackCard.main;
        main.startColor = highestRarity.ToColor();
        main = psBackGlow.main;
        main.startColor = highestRarity.ToColor();
        
        psResultBack.Play();
    }
    
    /// <summary>
    /// 가장 높은 단계를 보여주는 Result 연출을 숨김
    /// </summary>
    private void OnHideBigResult()
    {
        spriteRdrResult.gameObject.SetActive(false);
        btnResult.gameObject.SetActive(false);
        psResultBack.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        psFlashBack.Play();
        coroutineResult = StartCoroutine(ShowResult());
    }

    /// <summary>
    /// 결과물 미리 설정
    /// </summary>
    private void SetResult()
    {
        rawImgVideoPlayer.enabled = true;
        groupGacha.SetActive(true);
        btnClose.gameObject.SetActive(false);
        ResetNonUI();
        
        for (int i = 0; i < gachaContext.ItemDataList.Count; i++)
        {
            var guiCard = gachaContext.ItemDataList[i];
            Sprite cardBg = gachaContext.CurType switch
            {
                ResourceType.Gold => ResourceManager.Instance.GetResource<Sprite>(
                    StringAdrCardBg.GoldCardBgDict[guiCard.Rarity]),
                ResourceType.Diamond => ResourceManager.Instance.GetResource<Sprite>(
                    StringAdrCardBg.DiaCardBgDict[guiCard.Rarity]),
                _ => null
            };

            guiCardList[i].Set(cardBg, guiCard.Icon, guiCard.Rarity);
        }
    }

    /// <summary>
    /// 가챠 결과 카드 연출 => 순차 표시
    /// </summary>
    private IEnumerator ShowResult()
    {
        try
        {
            btnClose.gameObject.SetActive(true);
            for (int i = 0; i < gachaContext.ItemDataList.Count; i++)
            {
                guiCardList[i].Show();
                yield return new WaitForSeconds(0.2f);
            }
        }
        finally
        {
            coroutineResult = null;
        }
    }
}
