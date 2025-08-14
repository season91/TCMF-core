using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장착된 아이템, 스탯, 유닛 정보를 확인할 수 있는 작은 버블
/// </summary>
public class UIWcEquipBubble : MonoBehaviour
{
    [Header("====[Position]")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private RectTransform rectTr;
    private const float StartPosX = -150f;
    private const float FinalPosX = -345f;
    
    [Header("====[Unit Info]")]
    [SerializeField] private Image imgUnitIcon;
    [SerializeField] private TextMeshProUGUI tmpUnitName;
    
    [Header("====[Item]")]
    [SerializeField] private List<UIWgItem> guiItemList = new ();
    
    [Header("====[Stat]")]
    private UIDynamicObjectPool<UIWgStat> dynamicStatPool;

    [SerializeField] private Transform statRoot;
    [SerializeField] private UIWgStat originStat;
    private const int MaxStatCount = 6;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        cg = GetComponent<CanvasGroup>();
        rectTr = GetComponent<RectTransform>();
        imgUnitIcon = transform.FindChildByName<Image>("Img_BubbleUnitIcon");
        tmpUnitName = transform.FindChildByName<TextMeshProUGUI>("Tmp_UnitName");
        guiItemList = transform.FindChildByName<Transform>("Layout_Items").GetComponentsInChildren<UIWgItem>().ToList();
        statRoot = transform.FindChildByName<Transform>("Layout_Stats");
        originStat = statRoot.GetComponentInChildren<UIWgStat>();
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize()
    {
        dynamicStatPool = new UIDynamicObjectPool<UIWgStat>(originStat, statRoot, MaxStatCount);
    }
    
    /// <summary>
    /// UI 표시: 유닛 정보, 아이템, 스탯 세팅
    /// </summary>
    public void Show(InventoryUnit inventoryUnit)
    {
        cg.OpenPopupAnimation(rectTr, RectTransform.Axis.Horizontal, StartPosX, FinalPosX);
        
        // 유닛 정보 세팅
        UnitData unitData = MasterData.UnitDataDict[inventoryUnit.UnitCode];
        tmpUnitName.text = unitData.Name;
        imgUnitIcon.sprite = unitData.Icon;

        // 장착 중인 아이템 세팅
        foreach (var guiItem in guiItemList)
        {
            guiItem.Initialize();
        }
        List<InventoryItem> equippedItems = inventoryUnit.EquippedItems;
        for (int i = 0; i < equippedItems.Count; i++)
        {
            if(i < guiItemList.Count)
                guiItemList[i].ShowInventory(equippedItems[i]);
        }

        // 스탯 세팅
        SetStatInfo(inventoryUnit);
    }

    /// <summary>
    /// 선택된 유닛의 스탯 정보 설정
    /// </summary>
    private void SetStatInfo(InventoryUnit inventoryUnit)
    {
        dynamicStatPool.OffAll();
        Dictionary<StatType, int> stats = inventoryUnit.GetMergeStatsUi();
        
        int index = 0;
        foreach (var stat in stats)
        {
            if(index >= MaxStatCount) break;
            dynamicStatPool.Get().Show(stat.Key, stat.Value);
            index++;
        }
    }
    
    /// <summary>
    /// UI 숨김: 애니메이션 X
    /// </summary>
    public void Hide()
    {
        cg.SetAlpha(0);
    }
}
