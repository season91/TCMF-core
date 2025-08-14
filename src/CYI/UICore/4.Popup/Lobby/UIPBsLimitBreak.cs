using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 대장장이 돌파 - OpenContext
/// </summary>
public class BsLbOpenContext : IUIOpenContext
{
    public int LimitBreakLevel;
    public ItemData ItemData;
}

/// <summary>
/// 돌파 연출 팝업
/// </summary>
public class UIPBsLimitBreak : UIBasePopup
{
    [Header("====[기본 구성]")]
    [SerializeField] private Button btnClose;
    [SerializeField] private Image imgWhite;
    
    [Header("====[아이템 정보]")]
    [SerializeField] private Image imgItemBg;
    [SerializeField] private Image imgItem;
    [SerializeField] private ParticleSystem psItemBg;
    [SerializeField] private ParticleSystem psItemSuccess;
    
    [Header("====[아이템 돌파 정보]")]
    [SerializeField] private Image[] lbStars;
    [SerializeField] private ParticleSystem psEquip;
    
    [Header("====[Video]")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private VideoPlayer videoPlayerLb;
    [SerializeField] private RawImage rawImgVideoPlayer;
    
    private BsLbOpenContext bsLbOpenContext;
    private Sequence seq;
    private Image maxLevelStar;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();

        btnClose = transform.FindChildByName<Button>("Btn_Close");

        imgItemBg = transform.FindChildByName<Image>("Img_ItemBg");
        imgItem = transform.FindChildByName<Image>("Img_Item");
        psItemBg = transform.FindChildByName<ParticleSystem>("FX_UI_BsItemBg");
        psItemSuccess = transform.FindChildByName<ParticleSystem>("FX_UI_BsEnhSuccess");
        
        lbStars = transform.FindChildByName<Transform>("Layout_LimitBreak").GetComponentsInChildren<Image>();
        psEquip = transform.FindChildByName<ParticleSystem>("FX_UI_Flash_Yellow");
        
        videoPlayerLb = transform.FindChildByName<VideoPlayer>("VideoPlayer_Lb");
        audioSource = videoPlayerLb.gameObject.GetComponent<AudioSource>();
        rawImgVideoPlayer = transform.FindChildByName<RawImage>("RawImage_Lb");
        
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

        ResetNonUI();
        
        // 비디오 플레이어 -> 사운드 연결
        videoPlayerLb.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayerLb.SetTargetAudioSource(0, audioSource);
        
        videoPlayerLb.loopPointReached += OnEndLbVideo;
    }
    
    /// <summary>
    /// NonUI 초기 상태 설정: 비활성화
    /// </summary>
    private void ResetNonUI()
    {
        psEquip.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        psItemBg.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        psItemSuccess.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    
    /// <summary>
    /// UI 열기: 처음 진입 => 영상 재생 메서드 호출, 미리 연출 초기화 - 흰커버 Off
    /// </summary>
    /// <param name="openContext">BsLbOpenContext 필요</param>
    public override void Open(OpenContext openContext = null)
    {
        if (OpenContext.Context is not BsLbOpenContext castingContext) return;

        OffWhiteCover();
        bsLbOpenContext = castingContext;
        
        StartCoroutine(ShowLbVideo());
    }
    
    /// <summary>
    /// UI 닫기: 중간에 닫을 수 있으니 초기화 확실히, 시퀀스 제거, 모든 파티클 Stop
    /// </summary>
    public override void Close(CloseContext closeContext = null)
    {
        base.Close(closeContext);
        seq.Kill(true);
        ResetNonUI();
    }
    
    /// <summary>
    /// 영상 재생 => 영상이 준비되면 Open 및 영상 재생, 돌파 결과 세팅
    /// </summary>
    private IEnumerator ShowLbVideo()
    {
        videoPlayerLb.Prepare();
        yield return new WaitUntil(() => videoPlayerLb.isPrepared);
        base.Open(OpenContext);

        // 볼륨 제어 예시
        audioSource.volume = SoundManager.Instance.GetVolume(SoundType.Sfx);
        videoPlayerLb.Play();
        rawImgVideoPlayer.enabled = true;
        
        // 비디오 재생하는 동안 아이템 세팅
        SetStars();
        SetItem();
    }
    
    /// <summary>
    /// 돌파 Star 세팅 =>
    /// 돌파 이후 레벨에 따라 Star 활성화,
    /// 가장 높은 레벨의 Star는 꺼두기 (alpha = 0),
    /// Star 장착 Ps 부모 세팅 - 가장 높은 레벨의 Star 이미지
    /// </summary>
    private void SetStars()
    {
        for (int i = 0; i < lbStars.Length; i++)
        {
            lbStars[i].enabled = bsLbOpenContext.LimitBreakLevel > i;
        }
        
        maxLevelStar = lbStars[bsLbOpenContext.LimitBreakLevel - 1];
        maxLevelStar.color = new Color(1, 1, 1, 0);
        psEquip.transform.SetParent(maxLevelStar.transform);
        psEquip.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// 돌파 아이템 세팅 => 배경, 아이콘
    /// </summary>
    private void SetItem()
    {
        imgItemBg.color = bsLbOpenContext.ItemData.Rarity.ToColor();
        imgItem.sprite = bsLbOpenContext.ItemData.Icon;
    }
    
    /// <summary>
    /// 영상이 끝나면 자동으로 호출 => 영상을 멈추고(명시적), 돌파 연출 호출
    /// </summary>
    private void OnEndLbVideo(VideoPlayer videoPlayer)
    {
        videoPlayer.Stop();
        LimitBreakStarDirection();
    }
    
    /// <summary>
    /// 돌파 연출 시퀀스 =>
    /// 비디오 뷰어(RawImage) 끄고,
    /// 하얀 배경으로 덮은 뒤 다시 없애면서 (이때 명시적으로 Star Scale을 1.8까진 키워놓음),
    /// 아이템 강화 이펙트 출력하고,
    /// 가장 높은 레벨의 Star가 커지면서서 FadeIn,
    /// 그리고 원래 크기로 돌아오면서,
    /// Equip 이펙트 재생
    /// </summary>
    private void LimitBreakStarDirection()
    {
        seq = DOTween.Sequence().Append(imgWhite.DOFade(1, 1f))
            .Join(maxLevelStar.rectTransform.DOScale(Vector3.one * 2.8f, 0.1f))
            .AppendCallback(OffWhiteCover)
            .AppendCallback(psItemBg.Play)
            .JoinCallback(psItemSuccess.Play)
            .JoinCallback(OffRawImage)
            .AppendInterval(0.5f) // 이펙트 연출 시간 대기
            .Append(maxLevelStar.DOFade(1, 0.1f))
            .Join(maxLevelStar.rectTransform.DOScale(Vector3.one * 3f, 0.1f))
            .Append(maxLevelStar.rectTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.InQuad));
        seq.OnComplete(psEquip.Play);
    }
    
    /// <summary>
    /// 시퀀스 중, White Cover 숨김
    /// </summary>
    private void OffWhiteCover() => imgWhite.color = new Color(1, 1, 1, 0);
    /// <summary>
    /// 시퀀스 중, 비디오 출력 Raw Image
    /// </summary>
    private void OffRawImage() => rawImgVideoPlayer.enabled = false;
}
