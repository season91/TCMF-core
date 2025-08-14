using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit Inventory - Open Context
/// </summary>
public class UnitInvenOpenContext : IUIOpenContext
{
    public int UnitIndex;
}

/// <summary>
/// 유닛 인벤토리의 Hub 스크립트
/// </summary>
public class UIUnitInventoryWindow : UIBase
{
    protected override ContentType ContentType => ContentType.UnitInventory;
    [Header("====[기본 구성]")]
    [SerializeField] private GUIContentTitle guiContentTitle;
    [SerializeField] private Button btnClose;
    
    [Header("====[장비 세팅 버튼]")]
    [SerializeField] private Button btnWeapon;
    [SerializeField] private Button btnArmor;
    [SerializeField] private Image imgBtnWeaponBg;
    [SerializeField] private Image imgBtnArmorBg;
    [SerializeField] private Image imgBtnWeapon;
    [SerializeField] private Image imgBtnArmor;
    
    [Header("====[유닛 인벤토리]")]
    [SerializeField] private UIWcUnitInventory uiWcUnitInventory;
    
    [Header("====[유닛 정보]")]
    [SerializeField] private GameObject objUnit;
    [SerializeField] private UIWcUnitInfo uiWcUnitInfo;
    
    [Header("====[아이템 인벤토리]")]
    [SerializeField] private UIWcItemInvenQuick uiWcItemInvenQuick;

    private InventoryUnit selectedUnit;
    private int curUnitIndex;
    private bool isSetItem;
    private GameObject selectedUnitPrefab;
    
    // ============================================================================
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        
        guiContentTitle = transform.FindChildByName<GUIContentTitle>("Group_ContentTitle");
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        
        btnWeapon = transform.FindChildByName<Button>("Btn_Weapon");
        btnArmor = transform.FindChildByName<Button>("Btn_Armor");
        imgBtnWeaponBg = transform.FindChildByName<Image>("Img_BtnWeaponBG");
        imgBtnArmorBg = transform.FindChildByName<Image>("Img_BtnArmorBG");
        imgBtnWeapon = transform.FindChildByName<Image>("Img_BtnWeapon");
        imgBtnArmor = transform.FindChildByName<Image>("Img_BtnArmor");
        
        uiWcUnitInventory = GetComponentInChildren<UIWcUnitInventory>();
        
        uiWcUnitInfo = transform.GetComponentInChildren<UIWcUnitInfo>();
        objUnit = transform.FindChildByName<Transform>("Unit").gameObject;
        
        uiWcItemInvenQuick = GetComponentInChildren<UIWcItemInvenQuick>(true);
    }

    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        
        guiContentTitle.Initialize();
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(Close);
        
        btnWeapon.onClick.RemoveAllListeners();
        btnArmor.onClick.RemoveAllListeners();
        btnWeapon.AddListener(OnWeapon);
        btnArmor.AddListener(OnArmor);
        
        uiWcItemInvenQuick.Initialize();
        uiWcUnitInfo.Initialize();
        uiWcUnitInventory.Initialize(UnitInventoryType.Popup);
    }
    
    /// <summary>
    /// UI 열기: 필요한 부가 UI 호출, 초기 상태 복원
    /// </summary>
    /// <param name="openContext">UnitInvenOpenContext 필수</param>
    public override void Open(OpenContext openContext = null)
    {
        if(openContext?.Context is not UnitInvenOpenContext castingContext) return;

        UIManager.Instance.Open<UIWcUserInfo>();
        curUnitIndex = castingContext.UnitIndex;
        selectedUnit = UserData.inventory.Units[curUnitIndex];
        uiWcUnitInventory.Show(OnSelectUnit);
        uiWcUnitInventory.SetSelectedUI(curUnitIndex);
        UpdateGUI();
        guiContentTitle.SetTitle();
        base.Open(openContext);
    }
    
    /// <summary>
    /// UI 닫기: 부가 UI Close, 연계된 NonUI 오브젝트 비활성화
    /// </summary>
    public override void Close()
    {
        if (isSetItem)
        {
            uiWcItemInvenQuick.Close();
            uiWcUnitInfo.Show(selectedUnit);
            isSetItem = false;
        }
        else
        {
            base.Close();
            UIManager.Instance.Close<UIWcUserInfo>();
            if (UIManager.Instance.CurSceneType == SceneType.Battle)
                UIManager.Instance.Open<UIUnitSelectWindow>();
            else
                UIManager.Instance.Open<UILobbyWindow>(); // Lobby Main Hub Open
            ObjectPoolManager.Instance.Return(
                PoolCategory.Entity, 
                MasterData.UnitDataDict[selectedUnit.UnitCode].Prefab, 
                selectedUnitPrefab);
        }
        
    }

    /// <summary>
    /// 유닛 설정 팝업의 GUI를 재설정
    /// </summary>
    private void UpdateGUI()
    {
        uiWcItemInvenQuick.Close(false);
        uiWcUnitInfo.Show(selectedUnit);
        
        GameObject go = ObjectPoolManager.Instance.Get(PoolCategory.Entity, MasterData.UnitDataDict[selectedUnit.UnitCode].Prefab, objUnit.transform);
        go.transform.localRotation = Quaternion.identity; // 자식으로 붙이고 위치/회전 초기화
        go.transform.localPosition = new Vector3(-50f, -570f, 0f); // 원하는 위치/크기로 세팅
        go.transform.localScale = new Vector3(150f, 150f, 150f);
        selectedUnitPrefab = go;
        
        UpdateItemButtons();
    }

    /// <summary>
    /// 아이템 버튼의 아이템 이미지를 변경
    /// </summary>
    private void UpdateItemButtons()
    {
        ResetItemButtons();
        
        // 장착한 아이템이 있으면 세팅
        List<InventoryItem> equippedItems = selectedUnit.EquippedItems;
        foreach (var equippedItem in equippedItems)
        {
            ItemRarity itemRarity = MasterData.ItemDataDict[equippedItem.ItemCode].Rarity;
            Sprite itemBgSprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrCardBg.GoldCardBgDict[itemRarity]);
            Sprite itemSprite = MasterData.ItemDataDict[equippedItem.ItemCode].Icon;
            
            switch (equippedItem.ItemType)
            {
                case ItemType.Armor:
                    imgBtnArmorBg.sprite = itemBgSprite;
                    imgBtnArmor.enabled = true;
                    imgBtnArmor.sprite = itemSprite;
                    break;
                case ItemType.Weapon:
                    imgBtnWeaponBg.sprite = itemBgSprite;
                    imgBtnWeapon.enabled = true;
                    imgBtnWeapon.sprite = itemSprite;
                    break;
            }
        }
    }
    
    /// <summary>
    /// 장비 버튼 이미지 세팅
    /// </summary>
    private void ResetItemButtons()
    {
        imgBtnWeapon.enabled = false;
        imgBtnArmor.enabled = false;
        imgBtnWeaponBg.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrCardBg.GoldCardBgDict[ItemRarity.Common]);
        imgBtnArmorBg.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrCardBg.GoldCardBgDict[ItemRarity.Common]);
    }
    
    /// <summary>
    /// 유닛 슬롯 클릭 시 호출되는 이벤트 처리 메서드
    /// </summary>
    private void OnSelectUnit(int unitIndex)
    {
        ObjectPoolManager.Instance.Return(PoolCategory.Entity, MasterData.UnitDataDict[selectedUnit.UnitCode].Prefab, selectedUnitPrefab);
        uiWcUnitInventory.SetSelectedUI(curUnitIndex);
        uiWcUnitInventory.SetSelectedUI(unitIndex);
        curUnitIndex = unitIndex;
        selectedUnit = UserData.inventory.Units[curUnitIndex];
        UpdateGUI();
        
    }

    /// <summary>
    /// 무기 클릭 시 호출되는 이벤트 처리 메서드
    /// - 무기 퀵 인벤토리 Open
    /// </summary>
    private void OnWeapon()
    {
        isSetItem = true;
        uiWcItemInvenQuick.Open(ItemType.Weapon, curUnitIndex, UpdateItemButtons);
        uiWcUnitInfo.Hide();
    }
    /// <summary>
    /// 방어구 클릭 시 호출되는 이벤트 처리 메서드
    /// - 무기 퀵 인벤토리 Open
    /// </summary>
    private void OnArmor()
    {
        isSetItem = true;
        uiWcItemInvenQuick.Open(ItemType.Armor, curUnitIndex, UpdateItemButtons);
        uiWcUnitInfo.Hide();
    }
}
