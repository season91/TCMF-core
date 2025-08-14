using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 대장장이 - 선택 아이템에 대한 정보 컨테이너
/// </summary>
public class UIWcBsItemInfo : MonoBehaviour
{
    [Header("====[파티클]")]
    // 아이템 정보
    [SerializeField] private ParticleSystem psItemBg;
    // 강화
    [SerializeField] private ParticleSystem psEnhTry;
    [SerializeField] private ParticleSystem psEnhSuccess;
    [SerializeField] private ParticleSystem psEnhFailed;
    
    [Header("====[아이템 정보]")]
    [SerializeField] private GameObject obj;
    [SerializeField] private Image imgItemBg;
    [SerializeField] private Image imgItem;
    [SerializeField] private GameObject objImpossible;

    [Header("====[강화 단계]")] 
    [SerializeField] private GameObject groupEnhancement;
    [SerializeField] private TextMeshProUGUI tmpEnhValue;
    [SerializeField] private TextMeshProUGUI tmpEnhPlusValue;
    [SerializeField] private TextMeshProUGUI tmpEnhSuccessRate;

    [Header("====[돌파 단계]")] 
    [SerializeField] private GameObject groupLimitBreak;
    [SerializeField] private List<Image> imgLbIconList;
    [SerializeField] private List<Image> imgPlusLbIconList;

    [Header("====[강화/돌파 정보]")] 
    [SerializeField] private UIWgStat uiCurStat;
    [SerializeField] private UIWgStat uiPlusStat;
    private UIDynamicObjectPool<UIWgMaterial> dynamicMaterialPool;

    [Header("====[재료 정보]")] 
    [SerializeField] private RectTransform materialRoot;
    [SerializeField] private UIWgMaterial uiWgMaterial;
    
    [Header("====[기본 구성]")] 
    [SerializeField] private Button btnAccept;
    [SerializeField] private TextMeshProUGUI tmpBtnAccept;

    private InventoryItem curItem;
    private ContentType curType;
    private bool isProcessing;

    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        obj = transform.FindChildByName<Transform>("Group_Info").gameObject;
        psItemBg = transform.FindChildByName<ParticleSystem>("FX_UI_BsItemBg");
        psEnhTry = transform.FindChildByName<ParticleSystem>("FX_UI_BsEnhTry");
        psEnhSuccess = transform.FindChildByName<ParticleSystem>("FX_UI_BsEnhSuccess");
        psEnhFailed = transform.FindChildByName<ParticleSystem>("FX_UI_BsEnhFailed");
        
        imgItemBg = transform.FindChildByName<Image>("Img_ItemBg");
        imgItem = transform.FindChildByName<Image>("Img_Item");
        objImpossible = transform.FindChildByName<Transform>("Tmp_Impossible").gameObject;

        groupLimitBreak = transform.FindChildByName<Transform>("Group_LimitBreak").gameObject;
        imgLbIconList = groupLimitBreak.transform.FindChildByName<Transform>("Layout_LimitBreak")
            .GetComponentsInChildren<Image>().ToList();
        imgPlusLbIconList = groupLimitBreak.transform.FindChildByName<Transform>("Layout_PlusLimitBreak")
            .GetComponentsInChildren<Image>().ToList();

        groupEnhancement = transform.FindChildByName<Transform>("Group_Enhancement").gameObject;
        tmpEnhValue = transform.FindChildByName<TextMeshProUGUI>("Tmp_CurrentEnhLevel");
        tmpEnhPlusValue = transform.FindChildByName<TextMeshProUGUI>("Tmp_PlusEnhLevel");
        tmpEnhSuccessRate = transform.FindChildByName<TextMeshProUGUI>("Tmp_EnhSuccessRate");

        uiCurStat = transform.FindChildByName<Transform>("Group_CurrentStat").GetComponentInChildren<UIWgStat>();
        uiPlusStat = transform.FindChildByName<Transform>("Group_IncreaseStat").GetComponentInChildren<UIWgStat>();

        materialRoot = transform.FindChildByName<RectTransform>("Layout_Materials");
        uiWgMaterial = materialRoot.FindChildByName<UIWgMaterial>("GUI_Material");

        btnAccept = transform.FindChildByName<Button>("Btn_Accept");
        tmpBtnAccept = transform.FindChildByName<TextMeshProUGUI>("Tmp_BtnAccept");
    }
    
    /// <summary>
    /// UI의 Awake단계: UI 초기 설정, 버튼 이벤트 초기화 및 등록
    /// </summary>
    public void Initialize()
    {
        dynamicMaterialPool = new UIDynamicObjectPool<UIWgMaterial>(uiWgMaterial, materialRoot, 2);
        AllParticleStop();
        obj.SetActive(false);
    }

    /// <summary>
    /// UI 열기: BGM 재생, 배경 설정, 필요한 부가 UI 호출, GA, 초기 상태 복원
    /// </summary>
    public void Open(ContentType type, InventoryItem item)
    {
        gameObject.SetActive(true);
        curType = type;
        curItem = item;

        SetItem();
        SetGUIByType();
        UpdateGUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        psItemBg.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void SetItem()
    {
        ItemData itemData = MasterData.ItemDataDict[curItem.ItemCode];
        // 배경 이펙트 재생
        psItemBg.Play();
        // 희귀도에 맞는 아이템 배경색 설정
        imgItemBg.color = itemData.Rarity.ToColor(); 
        // 아이템 이미지 설정
        imgItem.sprite = itemData.Icon;
    }

    private void SetGUIByType()
    {
        switch (curType)
        {
            case ContentType.Enhancement:
                groupEnhancement.SetActive(true);
                groupLimitBreak.SetActive(false);
                SetButtonEvent("강화하기", OnTryEnhancement);
                break;
            case ContentType.LimitBreak:
                groupEnhancement.SetActive(false);
                groupLimitBreak.SetActive(true);
                SetButtonEvent("돌파하기", OnTryLimitBreak);
                break;
        }
    }

    private void SetButtonEvent(string text, UnityAction action)
    {
        btnAccept.onClick.RemoveAllListeners();
        tmpBtnAccept.text = text;
        btnAccept.AddListener(action);
    }

    private void UpdateGUI()
    {
        dynamicMaterialPool.OffAll();
        switch (curType)
        {
            case ContentType.Enhancement:
                SetGUIEnhancement();
                break;
            case ContentType.LimitBreak:
                SetGUILimitBreak();
                break;
            default:
                MyDebug.LogError($"This Blacksmith Type is Not Enh or Lb => {curType}");
                return;
        }
    }

    private void SetStats(int plusValue)
    {
        StatType statType;
        switch (curItem.ItemType)
        {
            case ItemType.Weapon:
                statType = StatType.Atk;
                break;
            case ItemType.Armor:
                statType = StatType.Def;
                break;
            case ItemType.None:
            default:
                MyDebug.LogError($"This Item Type is Not Weapon or Armor => {curItem.ItemType}");
                return;
        }
        
        uiCurStat.Show(statType, curItem.GetStatsForUi()[statType]);
        uiPlusStat.Show(statType, curItem.GetStatsForUi()[statType] + plusValue);
    }

    private void SetGUIEnhancement()
    {
        if (!InventoryManager.Instance.EnhancementService.TryGetEnhancementData(curItem, out var enhData))
        {
            obj.SetActive(false);
            objImpossible.SetActive(true);
            return;
        }
        obj.SetActive(true);
        objImpossible.SetActive(false);
        
        tmpEnhValue.text = $"Lv{curItem.EnhancementLevel}";
        tmpEnhPlusValue.text = $"Lv{curItem.EnhancementLevel + 1}";
        
        // 강화 성공률에 따른 색 변화
        Color enhColor = enhData.SuccessRate switch
        {
            >= 70 => GameColor.Safe,
            >= 31 => GameColor.Warning,
            _ => GameColor.Danger
        };
        tmpEnhSuccessRate.text = $"강화 성공률: {enhData.SuccessRate}%";
        tmpEnhSuccessRate.color = enhColor;
              
        SetStats(enhData.BonusStat);
        
        Sprite icon = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.ResourceDict[ResourceType.Gold]);
        dynamicMaterialPool.Get().Show(icon, UserData.inventory.Gold, enhData.RequiredGold, false);
        icon = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.ResourceDict[ResourceType.Piece]);
        dynamicMaterialPool.Get().Show(icon, UserData.inventory.Piece, enhData.RequiredFragment, false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(materialRoot);
    }
    
    private void SetGUILimitBreak()
    {
        if (!InventoryManager.Instance.LimitBreakService.TryGetLimitBreakData(curItem, out var lbData))
        {
            obj.SetActive(false);
            objImpossible.SetActive(true);
            return;
        }
        obj.SetActive(true);
        objImpossible.SetActive(false);
                
        for (int i = 0; i < imgLbIconList.Count; i++)
        {
            imgLbIconList[i].color = curItem.LimitBreakLevel <= i ? GameColor.Black : Color.white;
        }
        for (int i = 0; i < imgPlusLbIconList.Count; i++)
        {
            imgPlusLbIconList[i].color = lbData.LimitBreakLevel <= i ? GameColor.Black : Color.white;
        }

        int plusStat = lbData.PrintIncreaseStat(MasterData.ItemDataDict[curItem.ItemCode]);
        SetStats(plusStat);
        
        Sprite icon = ResourceManager.Instance.GetResource<Sprite>(StringAdrIcon.ResourceDict[ResourceType.Gold]);
        dynamicMaterialPool.Get().Show(icon, UserData.inventory.Gold, lbData.RequiredGold, false);
        icon = MasterData.ItemDataDict[curItem.ItemCode].Icon;
        int itemCount = InventoryManager.Instance.ItemService.GetMaterialItemCount(curItem.ItemType, curItem.ItemCode, curItem.ItemUid);
        dynamicMaterialPool.Get().Show(icon, itemCount, lbData.RequiredItemCount, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(materialRoot);
    }
    
    private async void OnTryLimitBreak()
    {
        if(isProcessing) return;
        isProcessing = true;
        bool isPossible = await InventoryManager.Instance.LimitBreakService.TryLimitBreakAsync(curItem);
        if (!isPossible)
        {
            isProcessing = false;
            return;
        }
        
        ItemData itemData = MasterData.ItemDataDict[curItem.ItemCode];
        BsLbOpenContext context = new() { LimitBreakLevel = curItem.LimitBreakLevel, ItemData = itemData };
        UIManager.Instance.EnqueuePopup<UIPBsLimitBreak>(OpenContext.WithContext(context));
        psItemBg.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        UIManager.Instance.GetUI<UIBlacksmithWindow>().BaseClose();
        UIManager.Instance.DequeuePopup(EndProcess);
    }

    private void EndProcess()
    {
        isProcessing = false;
        UIManager.Instance.GetUI<UIBlacksmithWindow>().BaseOpen();
        psItemBg.Play();
        UpdateGUI();
    }

    private async void OnTryEnhancement()
    {
        if(isProcessing) return;
        isProcessing = true;
        // 재료가 있을 때만 시도 가능
        // 판단 여부는 Service 자원이 부족한지 체크하는 부분 분리 후 판단
        bool isPossible = InventoryManager.Instance.EnhancementService.IsPossibleTryEnhancement(curItem);
        if (!isPossible)
        {
            isProcessing = false;
            return;
        }
        
        UpdateGUI();        
        bool isSuccess = await InventoryManager.Instance.EnhancementService.StartEnhancementAsync();
        
        PlayParticle(psEnhTry);
        await WaitForParticleSystem(psEnhTry);
        isProcessing = false;

        if (isSuccess) // 성공이면 성공 이펙트
        {
            PlayParticle(psEnhSuccess);
            SoundManager.Instance.PlaySfx(StringAdrAudioSfx.EnhancementSuccess);
            await WaitForParticleSystem(psEnhSuccess);
            UpdateGUI();
        }
        else // 실패면 실패 이펙트
        {
            PlayParticle(psEnhFailed);
            SoundManager.Instance.PlaySfx(StringAdrAudioSfx.EnhancementFail);
            await WaitForParticleSystem(psEnhFailed);
        }
    }
    
    private void AllParticleStop()
    {
        psItemBg.Stop();
        psEnhTry.Stop();
        psEnhSuccess.Stop();
        psEnhFailed.Stop();
    }
    
    private void PlayParticle(ParticleSystem ps)
    {
        if (ps.isPlaying)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ps.Play();
    }

    private async Task WaitForParticleSystem(ParticleSystem ps)
    {
        await Task.Delay((int)(ps.main.duration * 1000));
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        while (ps != null && ps.IsAlive())
        {
            await Task.Yield();
        }
    }
}
