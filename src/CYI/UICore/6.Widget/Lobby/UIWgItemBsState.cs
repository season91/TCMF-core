using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대장장이 재련 상태에 대한 UI 클래스
/// </summary>
[System.Serializable]
public class UIWgItemBsState
{
    // Image 컴포넌트 - 장착 유닛 아이콘
    [SerializeField] private Image imgUnitIcon;
    // 강화 레벨
    [SerializeField] private TextMeshProUGUI tmpEnhLevel;
    // 돌파 Star 아이콘 Array
    [SerializeField] private Image[] lBIcons;
    
    private const string EnhancementLevelFront = "Lv.{0}";
    
    /// <summary>
    /// ***필수: Reset 메서드에서 생성 (비용이 큼)***
    /// 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    /// <param name="transform">부모 오브젝트의 Transform</param>
    public UIWgItemBsState(Transform transform)
    {
        imgUnitIcon = transform.FindChildByName<Image>("Img_UnitIcon");
        tmpEnhLevel = transform.FindChildByName<TextMeshProUGUI>("Tmp_EnhancementValue");
        lBIcons = transform.FindChildByName<Transform>("Layout_LimitBreak").GetComponentsInChildren<Image>();
    }
    
    /// <summary>
    /// UI 초기 상태 설정 - 장착중인 유닛 아이콘 Off, 돌파/강화 세팅 
    /// </summary>
    public void ResetUI()
    {
        // 아이콘 끄기
        imgUnitIcon.enabled = false;
        // 돌파 정보 끄기
        foreach (var icon in lBIcons)
        {
            icon.gameObject.SetActive(false);
        }
        // 강화 수치 비우기
        tmpEnhLevel.SetText(string.Empty);
    }
    
    /// <summary>
    /// 강화 레벨로 텍스트 설정
    /// </summary>
    public void ShowEnhancement(int level)
    {
        tmpEnhLevel.SetText(EnhancementLevelFront, level);
    }
    
    /// <summary>
    /// 돌파 레벨 기준 Start Icon 활성화
    /// </summary>
    public void ShowLimitBreak(int level)
    {
        for (int i = 0; i < lBIcons.Length; i++)
        {
            lBIcons[i].gameObject.SetActive(true);
            lBIcons[i].color = i < level ? Color.white : GameColor.Black;
        }
    }
    
    /// <summary>
    /// 유닛 아이콘으로 설정
    /// </summary>
    public void ShowUnitIcon(Sprite unitIcon)
    {
        if (unitIcon == null)
        {
            imgUnitIcon.enabled = false;
            return;
        }

        imgUnitIcon.enabled = true;
        imgUnitIcon.sprite = unitIcon;
    }
}