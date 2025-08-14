using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWgResource : MonoBehaviour
{
    [SerializeField] private Image imgIcon;
    [SerializeField] private TextMeshProUGUI tmpValue;
    private Tween resourceTween;
    
    private void Reset()
    {
        imgIcon = transform.FindChildByName<Image>("Img_ResourceIcon");
        tmpValue = transform.FindChildByName<TextMeshProUGUI>("Tmp_ResourceValue");
    }
    
    public void Initialize(Sprite icon, int value)
    {
        gameObject.SetActive(true);
        
        imgIcon.sprite = icon;
        UpdateResource(value);
    }
    
    private int endValue;
    
    public void UpdateResource(int value)
    {
        int currentValue = int.TryParse(tmpValue.text, out var result) ? result : 0;

        if(currentValue == value) return;
        
        endValue = value;
        // 변화량 비례 시간 설정 (0.2초 ~ 1.5초 제한)
        // Mathf.Abs(to - from) 현재 값과 목표 값의 절대 차이 = 변화량
        // * perUnitTime 단위당 시간(1 증가당 몇 초) → 전체 변화량에 시간 비례 계산
        // Mathf.Clamp(..., 0.2f, 1.5f) 계산된 시간(duration)을 최소 0.2초, 최대 1.5초 사이로 제한
        float duration = Mathf.Clamp(Mathf.Abs(value - currentValue) * 0.02f, 0.2f, 1.5f);
        
        if (resourceTween != null)
        {
            resourceTween.Kill(true);
        }
        resourceTween = DOVirtual.Int(
                currentValue,
                value,
                duration,
                x => tmpValue.text = x.ToString()
            )
            .SetEase(Ease.Linear)
            .OnComplete(SetResourceText);
    }
    
    private void SetResourceText() => tmpValue.text = endValue.ToString();
}
