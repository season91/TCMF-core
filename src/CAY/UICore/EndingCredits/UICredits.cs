using DG.Tweening;
using UnityEngine;
public class UICredits : UIBase
{
    [SerializeField] private CanvasGroup canvasGroupCredits;
    
    private Tween tween;

    protected override void Reset()
    {
        base.Reset();
        canvasGroupCredits = transform.FindChildByName<CanvasGroup>("Group_Credits");
    }

    public override void Open(OpenContext openContext = null)
    {
        Play();
    }

    private void OnDisable()
    {
        // 중간에 꺼질 때 누수 방지
        tween?.Kill();
        canvasGroup?.DOKill();
    }

    /// <summary>
    /// 엔딩 크레딧 연출 재생
    /// </summary>
    public void Play()
    {
        tween?.Kill(); // 이전 스크롤 중지
        SoundManager.Instance.PlayBgm(StringAdrAudioBgm.EndingScene);
        canvasGroupCredits.ScrollCredits(canvasGroup, 150f, 500f);
    }
}
