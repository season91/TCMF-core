using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIWgGlobalToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image imgHandle;
    private Action onStartCallback;
    private Action onCompleteCallback;

    private void Reset()
    {
        slider = GetComponent<Slider>();
        imgHandle = slider.handleRect.gameObject.GetComponent<Image>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlayButtonSound();
        ToggleAuto();
    }

    /// <summary>
    ///  초기 상태 설정 및 콜백 설정
    /// </summary>
    public void Initialize(bool isOn, Action startCallback = null, Action completeCallback = null)
    {
        slider.interactable = false;
        slider.value = 0;
        slider.transition = Selectable.Transition.None;
        onStartCallback = startCallback;
        onCompleteCallback = completeCallback;
        
        // 초기 토글 상태 설정
        SetState(isOn);
    }
    
    /// <summary>
    /// 명시적 상태 설정 
    /// </summary>
    private void SetState(bool isOn)
    {
        string spriteName;
        int sliderValue;

        if (isOn)
        {
            spriteName = StringAdrUI.AutoToggleOn;
            sliderValue = 1;
        }
        else
        {
            spriteName = StringAdrUI.AutoToggleOff;
            sliderValue = 0;
        }
        
        imgHandle.sprite = ResourceManager.Instance.GetResource<Sprite>(spriteName);
        slider.DOValue(sliderValue, 0.1f);
    }

    /// <summary>
    /// 현재 slider value에 따라 On/Off
    /// </summary>
    private void SetState()
    {
        bool nextState = slider.value < 0.5f;
        SetState(nextState);
    }
    
    /// <summary>
    /// 토글 클릭 시 호출
    /// 토글을 On/Off 해줌
    /// GUI, 애니메이션, 값 변경
    /// </summary>
    private void ToggleAuto()
    {
        onStartCallback?.Invoke();
        SetState();
        onCompleteCallback?.Invoke();
    }
}
