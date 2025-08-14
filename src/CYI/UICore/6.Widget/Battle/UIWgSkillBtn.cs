using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배틀 Hub에서의 유닛의 스킬 버튼
/// </summary>
public class UIWgSkillBtn : MonoBehaviour
{
    [SerializeField] private RectTransform rectTr;
    [SerializeField] private Button btnSkill;
    [SerializeField] private Image imgSkill;
    [SerializeField] private GameObject objCool;
    [SerializeField] private TextMeshProUGUI tmpCool;
    [SerializeField] private Image imgWaiting;
    private Sprite activeIcon;
    private Sprite inactiveIcon;
    private int curIndex;

    private void Reset()
    {
        rectTr = GetComponent<RectTransform>();
        btnSkill = GetComponent<Button>();
        imgSkill = btnSkill.GetComponent<Image>();
        objCool = transform.FindChildByName<Transform>("Group_Cool").gameObject;
        tmpCool = transform.FindChildByName<TextMeshProUGUI>("Tmp_Cool");
        imgWaiting = transform.FindChildByName<Image>("Img_SkillWaiting");
    }

    public void Initialize(int unitIndex)
    {
        curIndex = unitIndex;
        btnSkill.onClick.RemoveAllListeners();
        btnSkill.AddListener(OnSkillButton);
    }

    public void Show(Sprite activeSkillIcon, Sprite inactiveSkillIcon, bool isAvailable, int cool)
    {
        gameObject.SetActive(true);
        activeIcon = activeSkillIcon;
        inactiveIcon = inactiveSkillIcon;
        imgSkill.sprite = activeIcon;
        imgWaiting.enabled = false;

        UpdateSkillCool(isAvailable, cool);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnSkillButton()
    {
        BattleManager.Instance.Input.OnSkillButtonClick(curIndex);
    }

    /// <summary>
    /// 유닛 턴 시작or종료 시 마다 호출해줘야 함.
    /// </summary>
    public void UpdateSkillCool(bool isAvailable, int cool)
    {
        btnSkill.interactable = isAvailable;
        imgSkill.sprite = isAvailable ? activeIcon : inactiveIcon;
        objCool.SetActive(!isAvailable);
        if (objCool.activeSelf)
            tmpCool.text = cool == -1 ? string.Empty : cool.ToString("D0");
    }

    public void ChangeSizeSkillButton(Vector2 targetSize)
    {
        // 현재 사이즈
        float currentWidth = rectTr.rect.width;
        float currentHeight = rectTr.rect.height;
        
        DOTween.To(() => 
                currentWidth, 
            x => rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x), 
            targetSize.x, 
            0.2f).SetEase(Ease.OutExpo);
        DOTween.To(() => 
            currentHeight, 
            y => rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y), 
            targetSize.y, 
            0.2f).SetEase(Ease.OutExpo);
    }

    public void StartUseSkillWaiting(Vector2 targetSize)
    {
        imgWaiting.enabled = true;
        ChangeSizeSkillButton(targetSize);
    }

    public void EndUseSkillWaiting(Vector2 targetSize, bool isUseSkill)
    {
        imgWaiting.enabled = false;
        ChangeSizeSkillButton(targetSize);
        UpdateSkillCool(!isUseSkill, -1);
    }
    
    /// <summary>
    /// Waiting Fill Image가 시계방향으로 점점 차는 메서드
    /// -> 0 = 시작, 1 = 종료
    /// </summary>
    /// <param name="timeRatio">흐른 시간 비율</param>
    public void UpdateWaitingGUI(float timeRatio)
    {
        imgWaiting.fillAmount = timeRatio;
    }
}