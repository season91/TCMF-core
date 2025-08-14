using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
/// 가챠 전반 로직 담당 매니저
/// </summary>
public class GachaManager : Singleton<GachaManager>
{
    private GachaCache cache; // 캐싱 데이터
    private GachaDrawService drawService; // 뽑기
    private GachaPityService pityService; // 천장
    private GachaTransactionService transactionService; // 로직처리

    // UI
    private SlideOpenContext slideContext = new();
    private const string NoCostComment = "재화가 부족합니다.";
    private const string NoInventoryComment = "인벤토리 공간이 충분치 않습니다.\n<size=50>무기, 방어구를 11칸씩 비워주세요</size>";
    
    // 개발모드인 경우 Firebase에 가챠 결과 적재되지 않음
    private bool isDeveloperMode;

    /// <summary>
    /// 매니저 공통 초기화 함수
    /// 캐싱 데이터 생성, 서비스 구성
    /// </summary>
    public void Initialize(bool isDevMode = false)
    {
        isDeveloperMode = isDevMode;
        
        // 캐시 초기화 (MasterData 기반)
        cache = new GachaCache();
        cache.Initialize();

        // 각 서비스에 캐시를 주입하여 구성
        drawService = new GachaDrawService(cache);
        pityService = new GachaPityService(cache);
        transactionService = new GachaTransactionService(cache);
    }

    /// <summary>
    /// UI 호출 가챠 진입점 (Draw → 피티 처리 → 보상 판단 및 지급)
    /// </summary>
    public async Task<GachaResult> TryGacha(ResourceType type, int count)
    {
        // 0. 인벤토리 여유 확인
        int expectedCount = count + 1; // 최대 1개 pity 보상 발생 가능성
        if (!InventoryManager.Instance.Cache.CanAddItems(expectedCount))
        {
            slideContext.Comment = NoInventoryComment;
            UIManager.Instance.Open<UIPGlobalSlide>(OpenContext.WithContext(slideContext));
            MyDebug.LogWarning("인벤토리 공간 부족으로 가챠 실행 불가");
            return null;
        }

        // 1. 재화 차감
        bool success = await transactionService.TryConsumeResourceAsync(type, count);
        if (!success)
        {
            slideContext.Comment = NoCostComment;
            UIManager.Instance.Open<UIPGlobalSlide>(OpenContext.WithContext(slideContext));
            MyDebug.LogWarning("재화 부족으로 가챠 실행 불가");
            return null;
        }
        // 2. GA 로그
        AnalyticsHelper.LogGachaStartEvent(type, (cache.GachaCosts[type] * count));
        
        // 3. 아이템 뽑기, GA 결과 로그
        var items = drawService.DrawItems(type, count);
        
        // 4. 피티 로직 반영 (카운트 증가, 리셋 등)
        await pityService.ApplyPityLogicAsync(type, items);
        
        // 5. 뽑은 아이템, 천장 보상 아이템 있으면 지급
        await transactionService.GiveItemsToInventory(items,isDeveloperMode);

        // 6. 반천장 or 천장 보상 판단
        var pityContext = pityService.TryGetPityItem(type);
        if (pityContext != null)
        {
            await transactionService.GiveItemsToInventory(new List<ItemData> {pityContext.ItemData}, isDeveloperMode);
        }
        
        // 7. 결과 통합
        return new GachaResult
        {
            GachaContext = new GachaResultOpenContext
            {
                CurType = type,
                ItemDataList = items
            },
            PityContext = pityContext
        };
    }
    
    /// <summary>
    /// UI 호출 현재 가챠 타입의 천장 비율(0~1)을 반환함
    /// </summary>
    public float GetPityRatio(ResourceType type)
    {
        // 현재까지 누적된 가챠 횟수를 PityService에서 가져옴
        int currentCount = InventoryManager.Instance.PityService.GetPityCount(type);
    
        // 해당 타입의 천장 임계값을 GachaCache에서 가져옴
        int threshold = cache.GetPityThreshold(type);
    
        // 비율 계산 (0~1 사이의 값 반환)
        return (float)currentCount / threshold;
    }
    
    /// <summary>
    /// UI 호출 현재 타입에 대한 가챠 비용을 반환
    /// </summary>
    public int GetGachaCost(ResourceType type)
    {
        return cache.GachaCosts[type];
    }

    /// <summary>
    /// UI 호출 반천장 조건 달성 여부 반환
    /// </summary>
    public bool IsGetHalfPity(ResourceType type)
    {
        return cache.IsHalfPity(type);
    }

    /// <summary>
    /// UI 호출 천장 조건 달성 여부 반환
    /// </summary>
    public bool IsGetHardPity(ResourceType type)
    {
        return cache.IsHardPity(type);
    }

}