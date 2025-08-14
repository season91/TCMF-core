using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWcUnitInfo : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private RectTransform rectTr;
    private const float StartPosX = 500f;
    private const float FinalPosX = -30f;
    
    [Header("====[Unit Info]")]
    [SerializeField] private Image imgUnitIcon;
    [SerializeField] private TextMeshProUGUI tmpUnitName;
    [SerializeField] private TextMeshProUGUI tmpUnitDescription;
    
    [Header("====[Stat Info]")]
    private UIDynamicObjectPool<UIWgStat> dynamicStatPool;
    [SerializeField] private Transform statRoot;
    [SerializeField] private UIWgStat uiStat;
    
    [Header("====[Skill Info]")]
    [SerializeField] private Image imgSkillIcon;
    [SerializeField] private TextMeshProUGUI tmpSkillName;
    [SerializeField] private TextMeshProUGUI tmpSkillDescription;

    // ============================================================================
    
    private void Reset()
    {
        cg = GetComponent<CanvasGroup>();
        rectTr = GetComponent<RectTransform>();
        imgUnitIcon = transform.FindChildByName<Image>("Img_UnitIcon");
        tmpUnitName = transform.FindChildByName<TextMeshProUGUI>("Tmp_UnitName");
        tmpUnitDescription = transform.FindChildByName<TextMeshProUGUI>("Tmp_UnitDescription");

        statRoot = transform.FindChildByName<Transform>("Layout_Stats");
        uiStat = statRoot.GetComponentInChildren<UIWgStat>();
        
        imgSkillIcon = transform.FindChildByName<Image>("Img_SkillIcon");
        tmpSkillName = transform.FindChildByName<TextMeshProUGUI>("Tmp_SkillName");
        tmpSkillDescription = transform.FindChildByName<TextMeshProUGUI>("Tmp_SkillDescription");
    }

    public void Initialize()
    {
        dynamicStatPool = new UIDynamicObjectPool<UIWgStat>(uiStat, statRoot, 6);
    }
    
    public void Show(InventoryUnit inventoryUnit)
    {
        cg.OpenPopupAnimation(rectTr, RectTransform.Axis.Horizontal, StartPosX, FinalPosX);
        cg.SetInteractable(true);
        
        UnitData unitData = MasterData.UnitDataDict[inventoryUnit.UnitCode];

        imgUnitIcon.sprite = unitData.Icon;
        tmpUnitName.text = unitData.Name;
        tmpUnitDescription.text = unitData.Description;

        UpdateStatInfo(inventoryUnit);
        
        SkillData skillData = MasterData.SkillDataDict[unitData.Code];
        imgSkillIcon.sprite = skillData.Icon;
        tmpSkillName.text = skillData.Name;
        tmpSkillDescription.text = skillData.Description;
    }

    public void Hide()
    {
        cg.ClosePopupAnimation(rectTr, RectTransform.Axis.Horizontal, FinalPosX, StartPosX);
        cg.SetInteractable(false);
    }
    
    // 스탯 항목이 수백 개 이상이 아니면 사실상 단점 없음
    // "스탯 계산은 항상 처음부터 다시 하는 방식"이 유지보수와 안정성 면에서 훨씬 좋음
    // 따라서, 장비 변경 시 이 메서드를 호출할 것
    private void UpdateStatInfo(InventoryUnit inventoryUnit)
    {
        Dictionary<StatType, int> stats = inventoryUnit.GetMergeStatsUi();
        
        // 결과 => GUI Stat에 세팅
        dynamicStatPool.OffAll();
        foreach (var stat in stats)
        {
            dynamicStatPool.Get().Show(stat.Key, stat.Value);
        }
    }
}
