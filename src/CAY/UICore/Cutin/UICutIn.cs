using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UICutIn : UIBase
{
    [Header("====[Skill Cutin Info]")]
    [SerializeField] private CanvasGroup canvasGroupSkillCutin;
    [SerializeField] private Image imgBg;
    [SerializeField] private Image imgShine;
    [SerializeField] private Image imgUnit;
    [SerializeField] private Image imgSlashBottom;
    [SerializeField] private Image imgSlashTop;

    private Vector2 initPosBg;
    private Vector2 initPosShine;
    private Vector2 initPosUnit;
    private Vector2 initPosSlashBottom;
    private Vector2 initPosSlashTop;

    private Sequence cutinSequence;
    
    protected override void Reset()
    {
        base.Reset();
        
        canvasGroupSkillCutin = transform.FindChildByName<CanvasGroup>("Group_SkillCutin");
        imgBg = transform.FindChildByName<Image>("Img_BG");
        imgShine = transform.FindChildByName<Image>("Img_Shine");
        imgUnit = transform.FindChildByName<Image>("Img_Unit");
        imgSlashBottom = transform.FindChildByName<Image>("Img_SlashBottom");
        imgSlashTop = transform.FindChildByName<Image>("Img_SlashTop");
    }

    public override void Initialize()
    {
        base.Initialize();
        initPosBg = imgBg.rectTransform.anchoredPosition;
        initPosShine = imgShine.rectTransform.anchoredPosition;
        initPosUnit = imgUnit.rectTransform.anchoredPosition;
        initPosSlashBottom = imgSlashBottom.rectTransform.anchoredPosition;
        initPosSlashTop = imgSlashTop.rectTransform.anchoredPosition;
    }

    /// <summary>
    /// 컷인 연출 재생
    /// </summary>
    public IEnumerator Play()
    {
        gameObject.SetActive(true);
        canvasGroup.SetAlpha(1);
        
        cutinSequence?.Kill();
        cutinSequence = DOTween.Sequence();

        // 초기 위치 세팅
        imgBg.rectTransform.anchoredPosition = initPosBg;
        imgShine.rectTransform.anchoredPosition = initPosShine;
        imgUnit.rectTransform.anchoredPosition = initPosUnit;
        imgSlashBottom.rectTransform.anchoredPosition = initPosSlashBottom;
        imgSlashTop.rectTransform.anchoredPosition = initPosSlashTop;

        // 배경 연출
        cutinSequence.Insert(0.01f,
            imgBg.rectTransform.MoveToAndBack(
                Vector2.zero, initPosBg, 0.05f, 0.5f, 0.01f, 0.1f));
        // 반짝이 연출
        cutinSequence.Insert(0.015f,
            imgShine.rectTransform.MoveToAndBack(
                new Vector2(450f, 0f), initPosShine, 0.05f, 0.3f, 0.01f, 0.1f));

        // 유닛 연출
        cutinSequence.Insert(0.02f,
            imgUnit.rectTransform.MoveToAndBack(
                Vector2.zero, initPosUnit, 0.05f, 0.45f, 0.3f, 0.1f));

        // 카메라 흔들기
        CameraShaker.Instance.Shake(0.2f,0.05f);
        
        Debug.Log($"[초기값 확인] Bottom: {initPosSlashBottom}, Top: {initPosSlashTop}");
        
        // 슬래시 아래 연출
        cutinSequence.Insert(0.01f,
            imgSlashBottom.rectTransform.FlySlash(
                new Vector2(293f, 0f), 0.2f, 0.2f,
                new Vector2(-1000f, -586f), 0.6f, 0.01f, 0.01f));

        // 슬래시 위 연출
        cutinSequence.Insert(0.01f,
            imgSlashTop.rectTransform.FlySlash(
                new Vector2(200f, 2200f), 0.2f, 0.2f,
                new Vector2(3340f, -1000f), 0.6f, 0.01f, 0.01f));
        
        // 종료 처리
        cutinSequence.AppendInterval(0.1f);
        cutinSequence.AppendCallback(Hide);
        // 컷인 끝날 때까지 대기
        yield return cutinSequence.WaitForCompletion();
    }

    /// <summary>
    /// 컷인 종료 처리
    /// </summary>
    public void Hide()
    {
        cutinSequence?.Kill(true);
        canvasGroup.SetAlpha(0);
    }

    /// <summary>
    /// 이미지 교체
    /// </summary>
    public void ReplaceImg(string unitCode)
    {
        MyDebug.Log("ReplaceImg "+ unitCode);
        if (StringAdrCutin.UnitSkillDict.TryGetValue(unitCode, out var cutInImageName))
        {
            imgBg.sprite = ResourceManager.Instance.GetResource<Sprite>(cutInImageName[0]);
            imgUnit.sprite = ResourceManager.Instance.GetResource<Sprite>(cutInImageName[1]);
            imgShine.sprite = ResourceManager.Instance.GetResource<Sprite>(cutInImageName[2]);
            imgSlashBottom.sprite = ResourceManager.Instance.GetResource<Sprite>(cutInImageName[3]);
            imgSlashTop.sprite = ResourceManager.Instance.GetResource<Sprite>(cutInImageName[4]);
            
            imgBg.SetNativeSize();
            imgUnit.SetNativeSize();
            imgShine.SetNativeSize();
            imgSlashBottom.SetNativeSize();
            imgSlashTop.SetNativeSize();
        }
    }
}