using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitSelectWindow : UIBase
{
    protected override ContentType ContentType => ContentType.UnitSelect;
    [SerializeField] private GUIContentTitle guiContentTitle;
    
    [SerializeField] private List<UIWgCatsite> unitCatsiteList = new ();
    
    [SerializeField] private Button btnStartStage;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnAllClear;
    
    [Header("====[Unit Inventory]")]
    [SerializeField] private UIWcUnitInventory uiWcUnitInventory;

    [Header("====[Unit Equip Bubble]")]
    [SerializeField] private UIWcEquipBubble uiEquipBubble;
    [SerializeField] private Button btnCloseEquipBubble;
    private int curSelectedCatsiteIndex;
    
    protected override void Reset()
    {
        base.Reset();

        guiContentTitle = transform.FindChildByName<GUIContentTitle>("Group_ContentTitle");
        unitCatsiteList = GetComponentsInChildren<UIWgCatsite>().ToList();
        
        btnStartStage = transform.FindChildByName<Button>("Btn_StartStage");
        btnClose = transform.FindChildByName<Button>("Btn_Close");
        btnAllClear = transform.FindChildByName<Button>("Btn_AllClear");

        uiWcUnitInventory = GetComponentInChildren<UIWcUnitInventory>();
        
        uiEquipBubble = transform.FindChildByName<UIWcEquipBubble>("GUI_EquipBubble");
        btnCloseEquipBubble = transform.FindChildByName<Button>("Btn_CloseEquipBubble");
    }
    
    public override void Initialize()
    {
        base.Initialize();
        
        btnStartStage.onClick.RemoveAllListeners();
        btnStartStage.AddListener(OnStartStage);
        btnClose.onClick.RemoveAllListeners();
        btnClose.AddListener(OnClose);
        btnAllClear.onClick.RemoveAllListeners();
        btnAllClear.AddListener(OnAllClear);
        btnCloseEquipBubble.onClick.RemoveAllListeners();
        btnCloseEquipBubble.AddListener(uiEquipBubble.Hide);

        guiContentTitle.Initialize();
        uiEquipBubble.Initialize();
        uiWcUnitInventory.Initialize(UnitInventoryType.Quick);
    }
    
    public override void Open(OpenContext openContext = null)
    {
        UIManager.Instance.Open<UIWcUserInfo>();
        UIManager.Instance.ChangeBg(StringAdrBg.StageReady);
        uiEquipBubble.Hide();
        curSelectedCatsiteIndex = -1;
        
        InitCatsites();
        uiWcUnitInventory.Show(OnSelectUnit);
        
        base.Open(openContext);
        guiContentTitle.SetTitle();
        SoundManager.Instance.PlayBgm(StringAdrAudioBgm.BattleReady);
        AnalyticsHelper.LogScreenView(AnalyticsBattleScreen.BattleReady, this.GetType().Name);
    }

    public override void Close(CloseContext closeContext = null)
    {
        base.Close(closeContext);
        UIManager.Instance.Close<UIWcUserInfo>();
    }

    private void InitCatsites()
    {
        // 현재 클리어 스테이지 수 체크하고 개수만큼 활성화
        int unlockedCount = StageManager.Instance.UnitUnlockCount();
        
        for (int i = 0; i < unitCatsiteList.Count; i++)
        {
            var info = new CatsiteInfo
            {
                IsUnlock = i < unlockedCount,
                Index = i,
                Unit = StageManager.Instance.GetSelectedUnitByIndex(i)
            };
            unitCatsiteList[i].Show(info, OnSelectCatsite, OnShowEquipBubble);
        }
    }
    
    /// <summary>
    /// 콜백 메서드
    /// GUI Unit Slot: 유닛 선택 시 호출
    /// </summary>
    private void OnSelectUnit(int unitIndex)
    {
        uiEquipBubble.Hide();
        
        InventoryUnit inventoryUnit = UserData.inventory.Units[unitIndex];
        bool isSelected = StageManager.Instance.IsSelectedUnit(inventoryUnit);
        
        // 이미 선택된 유닛일 때
        if (isSelected) // Catsite에서 해제하기
        {
            if(curSelectedCatsiteIndex != -1)
                unitCatsiteList[curSelectedCatsiteIndex].SetUnit(null); // 현재 선택된 Catsite이 있다면 제거 
            int catsiteIndex = StageManager.Instance.GetCatsiteIndexByUnitIndex(unitIndex);
            unitCatsiteList[catsiteIndex].SetUnit(null); // Catsite GUI Update
            StageManager.Instance.ClearSelectedUnit(catsiteIndex);
        }
        else // Catsite에 등록해주기
        {
            // 현재 선택중인 Catsite가 없을 때
            if (curSelectedCatsiteIndex == -1) 
                return;
            
            StageManager.Instance.SetSelectedUnitInSlot(inventoryUnit, curSelectedCatsiteIndex);
            unitCatsiteList[curSelectedCatsiteIndex].SetUnit(inventoryUnit); // Catsite GUI Update
        }
        
        uiWcUnitInventory.SetSelectedUI(unitIndex);
        curSelectedCatsiteIndex = -1;
    }
    
    /// <summary>
    /// 콜백 메서드
    /// GUI Catsite: n초 이상 클릭 유지 시, 호출
    /// </summary>
    private void OnShowEquipBubble(int catsiteIndex)
    {
        InventoryUnit inventoryUnit = StageManager.Instance.GetSelectedUnitByIndex(catsiteIndex);
        uiEquipBubble.Show(inventoryUnit);

        if (curSelectedCatsiteIndex != -1)
        {
            unitCatsiteList[curSelectedCatsiteIndex].SetCatsite(false);
        }
        curSelectedCatsiteIndex = -1;
    }
    
    /// <summary>
    /// 콜백 메서드
    /// GUI Catsite: Catsite 선택 시 호출
    /// </summary>
    private void OnSelectCatsite(int catsiteIndex)
    {
        uiEquipBubble.Hide();
        
        if (curSelectedCatsiteIndex == catsiteIndex)
        {
            unitCatsiteList[curSelectedCatsiteIndex].SetCatsite(false);
            curSelectedCatsiteIndex = -1;
            return;
        }
        
        if(StageManager.Instance.HasUnit(catsiteIndex))
        {
            var curUnit = StageManager.Instance.GetSelectedUnitByIndex(catsiteIndex);
            int curUnitIndex = 0;
            var unitInventoryList = UserData.inventory.Units;
            for (int i = 0; i < UserData.inventory.Units.Count; i++)
            {
                if (curUnit.UnitUid == unitInventoryList[i].UnitUid)
                {
                    curUnitIndex = i;
                    break;
                }
            }
            
            Close();
            UnitInvenOpenContext context = new UnitInvenOpenContext { UnitIndex = curUnitIndex};
            UIManager.Instance.Open<UIUnitInventoryWindow>(OpenContext.WithContext(context)); // Open Setting Popup
            // 반환
            AllReturnUnit();
        }
        else
        {
            if(curSelectedCatsiteIndex != -1)
                unitCatsiteList[curSelectedCatsiteIndex].SetCatsite(false);
            curSelectedCatsiteIndex = catsiteIndex;
            unitCatsiteList[curSelectedCatsiteIndex].SetCatsite(true);
        }
    }
    
    /// <summary>
    /// 버튼 바인딩 이벤트: Start Stage
    /// </summary>
    private void OnStartStage()
    {
        if (StageManager.Instance.GetReadyUnitCount() <= 0)
        {
            UIManager.Instance.Open<UIPGlobalSlide>(
                OpenContext.WithContext(
                    new SlideOpenContext
                    {
                        Comment = "출전한 고양이가 없습니다!\n스테이지를 시작하려면 최소 한 마리를 배치해주세요."
                    }
                ));
            return;
        }
        Close();
        // 반환
        AllReturnUnit();
        
        StageManager.Instance.StartStage();
    }

    /// <summary>
    /// 버튼 바인딩 이벤트: Close
    /// </summary>
    private void OnClose()
    {
        Close();
        // 반환
        AllReturnUnit();
        UIManager.Instance.Open<UIChapterSelectWindow>();
    }

    /// <summary>
    /// 버튼 바인딩 이벤트: All Clear
    /// </summary>
    private void OnAllClear()
    {
        StageManager.Instance.ClearAllSelectedUnits();
        InitCatsites();
        uiWcUnitInventory.Show(OnSelectUnit);
        uiEquipBubble.Hide();
    }

    private void AllReturnUnit()
    {
        MyDebug.Log("obj AllReturnUnit 반환");
        for (int i = 0; i < unitCatsiteList.Count; i++)
        {
            unitCatsiteList[i].ReturnObjUnit();
        }
    }
}
