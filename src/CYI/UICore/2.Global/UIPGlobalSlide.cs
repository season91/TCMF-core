using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlideOpenContext : IUIOpenContext
{
    public string Comment;
}

public class UIPGlobalSlide : UIBase
{
    [SerializeField] private TextMeshProUGUI tmpComment;

    private Tween tweenCloseDelay;
    private const float DelayTime = 1f;

    protected override void Reset()
    {
        base.Reset();
        
        tmpComment = transform.FindChildByName<TextMeshProUGUI>("Tmp_Comment");
    }

    private Action completeAction;
    
    public override void Open(OpenContext openContext = null)
    {
        if(openContext?.Context is not SlideOpenContext castingContext) return;

        DOTween.Kill(canvasGroup, true);
        tweenCloseDelay.Kill();
        
        tmpComment.text = castingContext.Comment;
        LayoutRebuilder.ForceRebuildLayoutImmediate(tmpComment.rectTransform);
        
        completeAction = openContext.OnComplete;
        
        base.Open(OpenContext.WithCallback(CloseDelay));
    }
    
    // GC도 거의 없음 (위치 고정됨 → delegate reuse 가능)
    // Unity + DOTween 사용 시, 이런 람다는 GC allocation 0B 또는 매우 적음
    // (DOVirtual.DelayedCall이 내부적으로 캐시함)
    // 익명 함수 방지용 Wrapping
    private void CloseDelay()
    {
        tweenCloseDelay = DOVirtual.DelayedCall(DelayTime, () =>
        {
            if (completeAction != null)
                Close(CloseContext.WithCallback(completeAction));
            else
                Close();
        });
    }
}
