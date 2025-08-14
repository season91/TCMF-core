using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWgMaterial : MonoBehaviour
{
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private Image imgIconBg;
    [SerializeField] private Image imgIcon;
    [SerializeField] private TextMeshProUGUI tmpCount;
    
    private void Reset()
    {
        rectTr = gameObject.GetComponent<RectTransform>();
        imgIconBg = transform.FindChildByName<Image>("Img_IconBG");
        imgIcon = transform.FindChildByName<Image>("Img_MaterialIcon");
        tmpCount = transform.FindChildByName<TextMeshProUGUI>("Tmp_MaterialCount");
    }

    public void Show(Sprite icon, int haveCount, int requireCount, bool isItem)
    {
        gameObject.SetActive(true);
        
        imgIconBg.enabled = isItem;
        imgIcon.sprite = icon;
        var colorCode = haveCount >= requireCount ? GameColorHexCode.Green : GameColorHexCode.Red;
        tmpCount.text = $"<color={colorCode}>{haveCount}</color>/{requireCount}";
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
    }
}
