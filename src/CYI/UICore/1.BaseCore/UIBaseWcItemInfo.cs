using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Base Item Info Widget Container 부모 클래스
/// 아이템 등급에 따른 배경 설정, 아이템 아이콘 / 태그 / 스탯 세팅
/// </summary>
public class UIBaseWcItemInfo : MonoBehaviour
{
    [Header("====[아이템 정보]")]
    [SerializeField] private GameObject groupItemInfo;
    [SerializeField] private Image imgItemBg;
    [SerializeField] private Image imgItem;

    private UIDynamicObjectPool<UIWgItemTag> dynamicTagPool;
    [SerializeField] private RectTransform tagRoot;
    [SerializeField] private UIWgItemTag originItemTag;

    [SerializeField] private TextMeshProUGUI tmpName;
    [SerializeField] private TextMeshProUGUI tmpDescription;
    
    [Header("====[스탯 POOL]")]
    private UIDynamicObjectPool<UIWgStat> dynamicStatPool;
    [SerializeField] private Transform statRoot;
    [SerializeField] private UIWgStat originStat;
    private const int MaxStatCount = 4;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected virtual void Reset()
    {
        groupItemInfo = transform.FindChildByName<Transform>("Group_ItemInfo").gameObject;
        imgItemBg = transform.FindChildByName<Image>("Img_ItemBg");
        imgItem = transform.FindChildByName<Image>("Img_Item");

        tagRoot = transform.FindChildByName<RectTransform>("Layout_Tag");
        originItemTag = tagRoot.FindChildByName<UIWgItemTag>("GUI_Tag");

        tmpName = transform.FindChildByName<TextMeshProUGUI>("Tmp_Name");
        tmpDescription = transform.FindChildByName<TextMeshProUGUI>("Tmp_Description");
        
        statRoot = transform.FindChildByName<Transform>("Layout_Stats");
        originStat = transform.FindChildByName<UIWgStat>("GUI_Stat");
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public virtual void Initialize()
    {
        dynamicStatPool = new UIDynamicObjectPool<UIWgStat>(originStat, statRoot,MaxStatCount);
        dynamicTagPool = new UIDynamicObjectPool<UIWgItemTag>(originItemTag, tagRoot, 2);
    }

    /// <summary>
    /// 아이템 데이터에 따른 정보 표시
    /// 인벤토리 아이템 O
    /// </summary>
    public virtual void ShowInfoByInventroy(InventoryItem inventoryItem)
    {
        if (inventoryItem == null)
        {
            Hide();
            return;
        }
        groupItemInfo.SetActive(true);
        
        ItemData itemData = MasterData.ItemDataDict[inventoryItem.ItemCode];
        
        // 아이템 Show
        imgItemBg.color = itemData.Rarity.ToColor();
        imgItem.sprite = itemData.Icon;
        
        // 아이템 Info
        SetItemInfo(itemData);
        
        // 스탯 설정
        SetStats(inventoryItem);
    }
    
    /// <summary>
    /// 아이템 데이터에 따른 정보 표시
    /// 인벤토리 아이템 X
    /// </summary>
    public void ShowInfoByData(ItemData itemData)
    {
        if (itemData == null)
        {
            Hide();
            return;
        }
        groupItemInfo.SetActive(true);
        
        // 아이템 Show
        imgItemBg.color = itemData.Rarity.ToColor();
        imgItem.sprite = itemData.Icon;
        
        // 아이템 Info
        SetItemInfo(itemData);
        
        // 스탯 설정
        SetStats(itemData);
    }

    /// <summary>
    /// Item Data에 따른 info 설정 - Tag, 이름, 설명
    /// </summary>
    private void SetItemInfo(ItemData itemData)
    {
        dynamicTagPool.OffAll();
        dynamicTagPool.Get().Show(itemData.Rarity);
        dynamicTagPool.Get().Show(itemData.Type);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tagRoot);
        
        tmpName.text = itemData.Name;
        tmpDescription.text = itemData.Description;
    }
    
    /// <summary>
    /// 아이템 스탯에 따른 설정
    /// 인벤토리 아이템 O
    /// </summary>
    private void SetStats(InventoryItem item)
    {
        dynamicStatPool.OffAll();
        IReadOnlyDictionary<StatType, int> stats = item.GetStatsForUi();
        int index = 0;
        foreach (var stat in stats)
        {
            if(index >= MaxStatCount) break;
            dynamicStatPool.Get().Show(stat.Key, stat.Value);
            index++;
        }
    }

    /// <summary>
    /// 아이템 스탯에 따른 설정
    /// 인벤토리 아이템 X
    /// </summary>
    private void SetStats(ItemData item)
    {
        dynamicStatPool.OffAll();
        var stats = item.Stats;
        int index = 0;
        foreach (var stat in stats)
        {
            if(index >= MaxStatCount) break;
            dynamicStatPool.Get().Show(stat.Key, stat.Value);
            index++;
        }
    }

    /// <summary>
    /// Item Info 숨김
    /// </summary>
    public void Hide() => groupItemInfo.SetActive(false);
}
