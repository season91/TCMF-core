using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스만 사용하는 체력 게이지
/// </summary>
public class UIWgBossHpBar : UIWgEntityHpBar
{
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        imgHpGauge = transform.FindChildByName<Image>("Img_HpGauge");
    }

    /// <summary>
    /// Hp의 비율에 따른 체력 바 색 변경 및 게이지 갱신
    /// </summary>
    /// <param name="normalizedValue">0~1 Hp Ratio</param>
    public override void UpdateHpBar(float normalizedValue)
    {
        base.UpdateHpBar(normalizedValue);

        int index = normalizedValue switch
        {
            >= 0.7f => 0,
            >= 0.5f => 1,
            >= 0.3f => 2,
            >= 0  => 3,
            _ => 0
        };
        
        Sprite gauge = ResourceManager.Instance.GetResource<Sprite>(StringAdrUI.BossHpGauge[index]);
        imgHpGauge.sprite = gauge;
    }
}
