using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일반 엔티티들의 Hp 게이지
/// </summary>
public class UIWgEntityHpBar : MonoBehaviour
{
    [SerializeField] protected Image imgHpGauge;
    [SerializeField] private RectTransform rectTrHpGauge;
    private float offsetY = 0;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected virtual void Reset()
    {
        imgHpGauge = transform.FindChildByName<Image>("Img_Hp");
        rectTrHpGauge = GetComponent<RectTransform>();
    }

    /// <summary>
    /// UI의 Awake단계: Full로 미리 초기화
    /// </summary>
    public void Initialize()
    {
        UpdateHpBar(1);
    }
    
    /// <summary>
    /// 엔티티 세팅 시, 실행
    /// 엔티티 키에 맞게 HP Bar 위치 설정
    /// </summary>
    public virtual void SetHpBar(Vector2 pos)
    {
        // Position 지정
        rectTrHpGauge.anchoredPosition = pos + new Vector2(0, offsetY);
    }
    
    /// <summary>
    /// 해당 Entity의 Hp Bar GUI 업데이트 => CharacterBase의 GetHpRatio 매개변수로 넣어주삼
    /// </summary>
    public virtual void UpdateHpBar(float normalizedValue)
    {
        if (imgHpGauge != null)
        {
            imgHpGauge.fillAmount = normalizedValue;
            if(normalizedValue == 0)
                gameObject.SetActive(false);
        }
    }
}
