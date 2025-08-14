using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 출정석 정보 구조체
/// </summary>
public struct CatsiteInfo
{
    public bool IsUnlock;
    public int Index;
    public InventoryUnit Unit;
}

/// <summary>
/// Unit Select의 출정석 위젯
/// </summary>
public class UIWgCatsite : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image imgUnitCatsite;
    
    private CatsiteInfo catsiteInfo;
    
    private Action<int> onSelected;
    private Action<int> onEquipBubble;
    
    private readonly WaitForSeconds holdTime = new (1f);
    private Coroutine holdCoroutine;
    private bool isBubble;
    private SlideOpenContext slideContext = new();
    
    private GameObject selectedUnitPrefab;
    
    /// <summary>
    /// 에디터 메서드: 하위 오브젝트에서 컴포넌트를 찾아 직렬화된 변수에 참조 및 초기 할당
    /// </summary>
    private void Reset()
    {
        imgUnitCatsite = transform.FindChildByName<Image>("Img_UnitCatsite");
    }
    
    /// <summary>
    /// UI 표시: 출정석 정보 저장, 세팅
    /// </summary>
    public void Show(CatsiteInfo info, Action<int> selectedCallback, Action<int> bubbleCallback)
    {
        catsiteInfo = info;
        onSelected = selectedCallback;
        onEquipBubble = bubbleCallback;
        
        // Catsite 세팅
        if (catsiteInfo.IsUnlock)
            SetUnit(catsiteInfo.Unit);
        else
            imgUnitCatsite.sprite = ResourceManager.Instance.GetResource<Sprite>(StringAdrUI.CatsiteShade);
    }
    
    /// <summary>
    /// 유닛 세팅
    /// </summary>
    public void SetUnit(InventoryUnit unit)
    {
        catsiteInfo.Unit = unit;
        SetCatsite(false);
        
        if (catsiteInfo.Unit != null)
        {
            GameObject go = ObjectPoolManager.Instance.Get(PoolCategory.Entity, MasterData.UnitDataDict[catsiteInfo.Unit.UnitCode].Prefab, transform);
            // 자식으로 붙이고 위치/회전 초기화
            go.transform.localRotation = Quaternion.identity;

            // 원하는 위치/크기로 세팅
            go.transform.localPosition = new Vector3(-40f, -100f, 0f);
            go.transform.localScale = new Vector3(120f, 120f, 120f);
            
            selectedUnitPrefab = go;
        }
        else
        {
            if (selectedUnitPrefab != null)
            {
                ObjectPoolManager.Instance.Return(PoolCategory.Entity, MasterData.UnitDataDict[selectedUnitPrefab.name].Prefab, selectedUnitPrefab);
            }
                
        }
    }
    
    /// <summary>
    /// 유닛 슬롯 세팅
    /// </summary>
    public void SetCatsite(bool isUnitSelectState)
    {
        imgUnitCatsite.sprite = 
            ResourceManager.Instance.GetResource<Sprite>(isUnitSelectState ? 
                StringAdrUI.CatsiteSelected : StringAdrUI.Catsite);
    }

    /// <summary>
    /// 버튼을 누르고 있는 상태인지 체크
    /// </summary>
    private IEnumerator HoldingButton()
    {
        yield return holdTime;
        isBubble = true;

        // 버블 띄우기
        onEquipBubble?.Invoke(catsiteInfo.Index);
    }
    
    /// <summary>
    /// 마우스 클릭 시 호출
    /// 1초 미만 클릭 시, 유닛 설정 팝업 띄울 준비 완료
    /// 1초 이상 클릭 시, 유닛의 장비 장착 현황 버블 띄움
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        isBubble = false;
        if (!catsiteInfo.IsUnlock || catsiteInfo.Unit == null)
            return;

        holdCoroutine = StartCoroutine(HoldingButton());
    }

    private const string LockCommentBack = " 스테이지 클리어 시 개방됩니다.";
    
    /// <summary>
    /// 마우스 뗄 때 호출
    /// 버블이 띄워져 있을 시, 끄기
    /// 아닐 시, 유닛 설정 팝업 띄우기
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!catsiteInfo.IsUnlock)
        {
            string lockComment = catsiteInfo.Index switch
            {
                1 => MasterData.UnlockTable.StageTitles[0],
                2 => MasterData.UnlockTable.StageTitles[1],
                _ => string.Empty
            };
            slideContext.Comment = lockComment + LockCommentBack;
            UIManager.Instance.Open<UIPGlobalSlide>(OpenContext.WithContext(slideContext));
            return;
        }

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        if (!isBubble)
        {
            onSelected?.Invoke(catsiteInfo.Index);
        }
    }

    /// <summary>
    /// 유닛 오브젝트 Pool에 반환
    /// </summary>
    public void ReturnObjUnit()
    {
        if (selectedUnitPrefab == null) return;
        var child = transform.FindChildByName<Transform>(selectedUnitPrefab.name);
        if (child == null) return;
        GameObject go = child.gameObject;
        ObjectPoolManager.Instance.Return(PoolCategory.Entity, MasterData.UnitDataDict[selectedUnitPrefab.name].Prefab, go);
    }
}
