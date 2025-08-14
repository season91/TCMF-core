using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingWindow : UIBase
{
    [SerializeField] private Slider sliderProgress;
    private float curProgress;
    
    protected override void Reset()
    {
        base.Reset();
        
        sliderProgress = transform.FindChildByName<Slider>("Slider_Loading");
    }

    public override void Initialize()
    {
        base.Initialize();
        
        UIManager.OnProgressBarUpdated -= SetProgressBar;
        UIManager.OnProgressBarUpdated += SetProgressBar;
    }

    public override void Open(OpenContext openContext = null)
    {
        base.Open(openContext);

        UIManager.Instance.RemoveAllLoadingEvents();
        curProgress = 0;
        SetProgressBar(0f);
    }
    
    private void SetProgressBar(float normalizedValue, bool animate = false, float duration = 0.5f)
    {
        curProgress += normalizedValue;
        curProgress = Mathf.Clamp01(curProgress);

        if (!animate)
        {
            sliderProgress.value = curProgress;
        
            if (Mathf.Approximately(sliderProgress.value, 1f))
                Close();
        }
        else
        {
            sliderProgress.DOKill();
            sliderProgress
                .DOValue(curProgress, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    // 1초 딜레이 후 Close 호출
                    if (Mathf.Approximately(sliderProgress.value, 1f))
                    {
                        DOVirtual.DelayedCall(1f, CloseWrapping);
                    }
                });
        }
    }

    private void CloseCallback()
    {
        UIManager.Instance.ClosePopup();
        UIManager.Instance.DequeuePopup();
    }

    private void CloseWrapping()
    {
        MyDebug.Log("CloseWrapping");
        Close(CloseContext.WithCallback(CloseCallback));
    }
}