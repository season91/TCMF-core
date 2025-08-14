using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛의 스탯을 표시하는 위젯 (스탯명, 수치)
/// </summary>
public class UIWgStat : MonoBehaviour
{
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private TextMeshProUGUI tmpName;
    [SerializeField] private TextMeshProUGUI tmpValue;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        rectTr = gameObject.GetComponent<RectTransform>();
        tmpName = transform.FindChildByName<TextMeshProUGUI>("Tmp_StatName");
        tmpValue = transform.FindChildByName<TextMeshProUGUI>("Tmp_StatValue");
    }

    /// <summary>
    /// UI 표시: Stat명, Stat수치
    /// </summary>
    public void Show(StatType statType, int value)
    {
        if (value == 0)
        {
            gameObject.SetActive(false);
            return;
        }
   
        gameObject.SetActive(true);
        
        tmpName.text = statType.ToKor();
        tmpValue.text = value.ToString();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
    }
}
