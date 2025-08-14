using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit Inventory의 Unit Slot 위젯
/// </summary>
public class UIWgUnitSlot : MonoBehaviour
{
    [SerializeField] private Button btnSelectUnit;
    [SerializeField] private Image imgUnitSlotBg;
    [SerializeField] private Image imgIcon;
    private int curUnitIndex;
    private Action<int> selectedAction;
    private bool isSelected;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        btnSelectUnit = GetComponent<Button>();
        imgIcon = transform.FindChildByName<Image>("Img_UnitIcon");
        imgUnitSlotBg = GetComponent<Image>();
    }
    
    /// <summary>
    /// UI의 Awake단계: 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize()
    {
        btnSelectUnit.onClick.RemoveAllListeners();
        btnSelectUnit.AddListener(OnSelected);
    }
    
    /// <summary>
    /// UI 표시: 아이콘 설정, 선택된 프레임 설정, 선택(클릭) 이벤트 바인딩
    /// </summary>
    public void Show(int unitIndex, bool isSelect, Action<int> onSelected)
    {
        gameObject.SetActive(true);
        
        curUnitIndex = unitIndex;
        selectedAction = onSelected;
        
        // isSelect로 설정해줘야 하므로 한 번 더 반전
        isSelected = !isSelect;
        SetSelectedUI();
        
        string unitCode = UserData.inventory.Units[curUnitIndex].UnitCode;
        imgIcon.sprite = MasterData.UnitDataDict[unitCode].Icon;
    }

    /// <summary>
    /// 유닛 선택된 프레임 토글
    /// </summary>
    public void SetSelectedUI()
    {
        // Bg 이미지 스프라이트를 변경
        isSelected = !isSelected;
        string bgAdr = isSelected ? StringAdrUI.UnitSlotSelected : StringAdrUI.UnitSlot;
        imgUnitSlotBg.sprite = ResourceManager.Instance.GetResource<Sprite>(bgAdr);
    }

    /// <summary>
    /// 유닛 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnSelected()
    {
        selectedAction.Invoke(curUnitIndex);
    }
}
