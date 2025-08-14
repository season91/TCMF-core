using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리의 필터 시스템 UI
/// </summary>
public class UIPInvenFilter : MonoBehaviour
{
    private UIDynamicObjectPool<UIWgSelectBtn> dynamicRarityPool;
    [SerializeField] private Transform btnRarityRoot;
    [SerializeField] private UIWgSelectBtn originRarityBtn;
    
    private UIDynamicObjectPool<UIWgSelectBtn> dynamicEnhPool;
    [SerializeField] private Transform btnEnhRoot;
    [SerializeField] private UIWgSelectBtn originEnhBtn;
    
    private UIDynamicObjectPool<UIWgSelectBtn> dynamicLbPool;
    [SerializeField] private Transform btnLbRoot;
    [SerializeField] private UIWgSelectBtn originLbBtn;
    
    // 상호작용 버튼들 눌렀는지
    private HashSet<ItemRarity> selectRaritiesSet = new();
    private HashSet<int> selectEnhLevelSet = new();
    private HashSet<int> selectLbLevelSet = new();

    private UIWgSelectBtnState selectedState = new ();
    private UIWgSelectBtnState unselectedState = new ();
    
    private void Reset()
    {
        btnRarityRoot = transform.FindChildByName<Transform>("Layout_Rarity");
        originRarityBtn = btnRarityRoot.FindChildByName<UIWgSelectBtn>("GUI_SelectButton");
        btnEnhRoot = transform.FindChildByName<Transform>("Layout_Enhancement");
        originEnhBtn = btnEnhRoot.FindChildByName<UIWgSelectBtn>("GUI_SelectButton");
        btnLbRoot = transform.FindChildByName<Transform>("Layout_LimitBreak");
        originLbBtn = btnLbRoot.FindChildByName<UIWgSelectBtn>("GUI_SelectButton");
    }

    public void Initialize()
    {
        var rarities = Enum.GetValues(typeof(ItemRarity));
        dynamicRarityPool = new UIDynamicObjectPool<UIWgSelectBtn>(originRarityBtn, btnRarityRoot, rarities.Length);
        foreach (ItemRarity rarity in rarities)
        {
            var selectBtn = dynamicRarityPool.Get();
            selectBtn.SetEvent(() => OnRarity(rarity)); // 초기화: 람다 사용 허용
            selectBtn.SetVisualStates(selectedState, unselectedState);
            selectBtn.Unselect();
        }

        int enhCount = MasterData.EnhancementDataDict.Count;
        dynamicEnhPool = new UIDynamicObjectPool<UIWgSelectBtn>(originEnhBtn, btnEnhRoot, enhCount);
        for (int i = 0; i < enhCount; i++)
        {
            var enhBtn = dynamicEnhPool.Get();
            var level = i;
            enhBtn.SetEvent(() => OnEnh(level)); // 초기화: 람다 사용 허용
            enhBtn.SetVisualStates(selectedState, unselectedState);
            enhBtn.Unselect();
        }
        
        int lbCount = MasterData.LimitBreakData.Count / rarities.Length - 1; // Rarity에서 None은 제외해야해서 -1
        dynamicLbPool = new UIDynamicObjectPool<UIWgSelectBtn>(originLbBtn, btnLbRoot, lbCount);
        for (int i = 0; i < lbCount; i++)
        {
            var lbBtn = dynamicLbPool.Get();
            var level = i;
            lbBtn.SetEvent(() => OnLb(level)); // 초기화: 람다 사용 허용
            lbBtn.SetVisualStates(selectedState, unselectedState);
            lbBtn.Unselect();
        }
    }
    
    public void Open()
    {
        
    }

    // private void 
    
    private void OnRarity(ItemRarity rarity)
    {
        if (!selectRaritiesSet.Remove(rarity))
        {
            selectRaritiesSet.Add(rarity);
        }
    }
    private void OnEnh(int level)
    {
        if (!selectEnhLevelSet.Remove(level))
        {
            selectEnhLevelSet.Add(level);
        }
    }
    private void OnLb(int level)
    {
        if (!selectLbLevelSet.Remove(level))
        {
            selectLbLevelSet.Add(level);
        }
    }
}
