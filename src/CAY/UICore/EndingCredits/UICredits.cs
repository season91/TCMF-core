using DG.Tweening;
using UnityEngine;

public class UICredits : UIBase
{
    [SerializeField] private CanvasGroup canvasGroupIntoduce;
    private Tween tween;

    protected override void Reset()
    {
        base.Reset();
        canvasGroupIntoduce = transform.FindChildByName<CanvasGroup>("Group_Credits");
    }

    private void OnEnable()
    {
        canvasGroupIntoduce.SetAlpha(0);
        Play();
    }
    
    private void OnDisable()
    {
        // 중간에 꺼질 때 누수 방지
        tween?.Kill();
        canvasGroup?.DOKill();
    }

    public void Play()
    {
        
        tween?.Kill(); // 이전 스크롤 중지
        canvasGroupIntoduce.SetAlpha(1);
        // 페이드인 후 스크롤 시작
        canvasGroup.DOFade(1f, 0.5f)
                   .OnComplete(() =>
                   {
                       // ScrollCreditsFromRoot 확장 메서드 호출
                       tween = canvasGroup.ScrollCreditsFromRoot(
                           speedPxPerSec: 200f, //px/sec 속도
                           topPadding: 180f,// 마지막 여유 높이
                           fastKey:  KeyCode.LeftShift, // 빨리 감기 입력키
                           fastMul: 3f, // 기존 1f -> 3f로 배속
                           skipKey: KeyCode.Escape, // ESC 스킵
                           onComplete: () =>
                           {
                               // 스크롤 끝나면 페이드아웃
                               canvasGroup.DOFade(0f, 0.5f)
                                          .OnComplete(() =>
                                          {
                                              _ = UIManager.Instance.EnterLoadingAsync(SceneType.Lobby);
                                          });
                           }
                       );
                   });
    }
}
