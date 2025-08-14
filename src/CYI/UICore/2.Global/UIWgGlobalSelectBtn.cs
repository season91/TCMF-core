using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 시각 상태 설정 클래스
/// </summary>
public class UIWgSelectBtnState
{
    /// <summary>
    /// 변경할 RectTransform W/H Size
    /// </summary>
    public Vector2? Size;
    /// <summary>
    /// 변경할 RectTransform Position
    /// </summary>
    public Vector2? AnchoredPosition;
    /// <summary>
    /// 변경할 배경 색
    /// </summary>
    public Color? BackgroundColor;
    /// <summary>
    /// 변경할 배경 Sprite
    /// </summary>
    [CanBeNull] public Sprite BackgroundSprite;
    /// <summary>
    /// Selected 오브젝트 활성 여부
    /// </summary>
    public bool IsSelectedObj = false;
    /// <summary>
    /// 변경할 Text Color
    /// </summary>
    public Color? TextColor;
}

/// <summary>
/// 선택/해제의 상태 변경이 필요한 버튼 오브젝트를 위한 클래스
/// </summary>
public class UIWgGlobalSelectBtn : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [SerializeField] private Button btn;
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Image imgBg;
    [SerializeField] private GameObject objSelected;

    [Header("상태 설정")]
    private UIWgSelectBtnState selectedState;
    private UIWgSelectBtnState unselectedState;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        btn = GetComponent<Button>();
        rectTr = GetComponent<RectTransform>();
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        imgBg = GetComponent<Image>();
        objSelected = transform.FindChildByName<Transform>("Group_Selected")?.gameObject;
    }

    /// <summary>
    /// 버튼 이벤트 초기 바인딩
    /// </summary>
    public void SetEvent(Action action)
    {
        btn.onClick.RemoveAllListeners();
        btn.AddListener(action.Invoke);
        btn.AddListener(Select);
    }
    
    /// <summary>
    /// 버튼 이벤트 추가 바인딩
    /// </summary>
    public void AddEvent(Action action)
    {
        btn.AddListener(action.Invoke);
    }

    /// <summary>
    /// 버튼 텍스트 설정
    /// </summary>
    public void SetText(string text) => tmp.text = text;
    
    /// <summary>
    /// 선택 시 상태 적용
    /// </summary>
    public void Select()
    {
        ApplyState(selectedState);
    }
    
    /// <summary>
    /// 선택 해제 시 상태 적용
    /// </summary>
    public void Unselect()
    {
        ApplyState(unselectedState);
    }

    /// <summary>
    /// 버튼 상태 적용 - 해당되는 부분에 대한 상태 변경
    /// </summary>
    private void ApplyState(UIWgSelectBtnState state)
    {
        if (state == null) return;

        if (state.Size.HasValue)
        {
            rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, state.Size.Value.x);
            rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, state.Size.Value.y);
        }

        if (state.AnchoredPosition.HasValue)
        {
            rectTr.anchoredPosition = state.AnchoredPosition.Value;
        }

        if (imgBg != null)
        {
            if (state.BackgroundColor.HasValue)
                imgBg.color = state.BackgroundColor.Value;

            if (state.BackgroundSprite != null)
                imgBg.sprite = state.BackgroundSprite;
        }

        if (objSelected != null)
        {
            objSelected.SetActive(state.IsSelectedObj);
        }

        if (tmp != null && state.TextColor.HasValue)
        {
            tmp.color = state.TextColor.Value;
        }
    }

    /// <summary>
    /// 선택/비선택 상태를 한 번에 설정
    /// </summary>
    public void SetVisualStates(UIWgSelectBtnState selected, UIWgSelectBtnState unselected)
    {
        selectedState = selected;
        unselectedState = unselected;
    }
}
