using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item의 Type/Rarity에 따른 태그 UI
/// </summary>
public class UIWgItemTag : MonoBehaviour
{
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private Image imgBg;
    [SerializeField] private TextMeshProUGUI tmpName;
    private const float BgAlpha = 80 / 255f;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        rectTr = GetComponent<RectTransform>();
        imgBg = GetComponent<Image>();
        tmpName = transform.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    /// <summary>
    /// UI 표시: Item Type 태그
    /// </summary>
    public void Show(ItemType itemType)
    {
        gameObject.SetActive(true);
        Color bgColor = Color.black;
        bgColor.a = BgAlpha;
        imgBg.color = bgColor;
        tmpName.SetText(itemType.ToEng().ToUpper());
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
    }
        
    /// <summary>
    /// UI 표시: Item Rarity 태그
    /// </summary>
    public void Show(ItemRarity rarity)
    {
        gameObject.SetActive(true);
        Color bgColor = rarity.ToColor();
        bgColor.a = BgAlpha;
        imgBg.color = bgColor;
        tmpName.SetText(rarity.ToEng().ToUpper());
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
    }
}
