using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// 데미지 텍스트 위젯
/// </summary>
public class UIWgDamage : MonoBehaviour
{
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private TextMeshProUGUI tmp;
    private UIDynamicObjectPool<UIWgDamage> pool;
    private Sequence sequence;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        rectTr = GetComponent<RectTransform>();
        tmp = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 풀 등록
    /// </summary>
    public void RegisterPool(UIDynamicObjectPool<UIWgDamage> damagePool)
    {
        pool = damagePool;
    }
    
    /// <summary>
    /// UI 표시: DOTween 시퀀스 실행, 해당 위치에서 생성
    /// </summary>
    public void Show(string damageText, Vector2 anchoredPos)
    {
        gameObject.SetActive(true);
        tmp.text = damageText;
        
        rectTr.anchoredPosition = anchoredPos;
        rectTr.localScale = Vector3.one;
        tmp.color = GameColor.White;

        if (sequence != null && sequence.IsActive()) 
            sequence.Kill();
        sequence = DOTween.Sequence()
            .Append(tmp.transform.DOScale(1.5f, 0.05f))
            .Append(tmp.transform.DOScale(1f, 0.05f))
            .AppendInterval(0.5f)
            .Append(tmp.DOFade(0, 0.1f));
        sequence.OnComplete(Hide);
    }
    
    /// <summary>
    /// UI 숨김
    /// </summary>
    private void Hide()
    {
        gameObject.SetActive(false);
        sequence.Kill();
        pool?.Return(this);
    }
}
