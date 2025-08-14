using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 로직만 수행 상태는 외부에서 주입  비즈니스 로직 단위
/// 강화 시도, 성공 여부 판단, 자원 소비, 결과 적용까지 담당
/// </summary>
public class EnhancementService
{
    private readonly ResourceService resourceService;
    private InventoryItem curItem;
    private EnhancementData curData;
    private const string MsgNoMaterial = "재료가 없습니다.";
    
    public EnhancementService(ResourceService resourceService)
    {
        this.resourceService = resourceService;
    }

    /// <summary>
    /// UI에 경고 메시지 출력
    /// </summary>
    private void ShowWarning(string message)
    {
        SlideOpenContext context = new SlideOpenContext { Comment = message };
        UIManager.Instance.Open<UISlidePopup>(OpenContext.WithContext(context));
    }
    
    /// <summary>
    /// 강화 가능 여부 검사 및 내부 상태 초기화
    /// </summary>
    public bool IsPossibleTryEnhancement(InventoryItem item)
    {
        curItem = null;
        curData = null;
        
        MyDebug.Log("돌파 대상 ItemUid ! "+item.ItemUid);
        if (!TryGetEnhancementData(item, out var enhancementData))
        {
            MyDebug.LogWarning("강화 실패: 데이터 없음");
            return false;
        }
        
        // 자원 충분한지 체크 (골드, 조각)
        if (!resourceService.HasEnough(ResourceType.Gold, enhancementData.RequiredGold) ||
            !resourceService.HasEnough(ResourceType.Piece, enhancementData.RequiredFragment))
        {
            MyDebug.LogWarning("강화에 필요한 자원이 부족함");
            ShowWarning(MsgNoMaterial);
            return false;
        }
        
        curItem = item;
        curData = enhancementData;
        
        return true;
    }
    
    /// <summary>
    /// 강화 시작
    /// </summary>
    public async Task<bool> StartEnhancementAsync()
    {
        if (curItem == null || curData == null)
        {
            MyDebug.LogError("강화 상태가 초기화되지 않았습니다.");
            return false;
        }
        
        // 자원 차감
        await resourceService.ConsumeAsync(ResourceType.Gold, curData.RequiredGold);
        await resourceService.ConsumeAsync(ResourceType.Piece, curData.RequiredFragment);

        // 성공 여부 판정
        bool isSuccess = Random.Range(0, 100) <= curData.SuccessRate;

        // 결과 반영
        if (isSuccess)
        {
            MyDebug.Log("강화 성공");
            await ApplyEnhancementSuccessAsyn();
        }
        else
        {
            // 실패 패널티는 없음으로 가정
            MyDebug.Log("강화 실패");
        }

        return isSuccess;
    }
    
    /// <summary>
    /// 강화 데이터 로드 및 유효성 검사
    /// </summary>
    public bool TryGetEnhancementData(InventoryItem item, out EnhancementData enhancementData)
    {
        // 강화 레시피 가져오기
        int toLevel = item.EnhancementLevel + 1;
        if (toLevel >= 6)
        {
            MyDebug.Log("강화치 max");
            enhancementData = null;
            return false;
        }
        
        // 강화 가능한지 확인
        if (!MasterData.EnhancementDataDict.TryGetValue(toLevel, out var data))
        {
            MyDebug.LogWarning($"No enhancement data found for level: {toLevel}");
            enhancementData = null;
            return false; // 강화 단계가 없음
        }

        enhancementData = data;
        return true;
    }

    /// <summary>
    /// 강화 성공 처리
    /// </summary>
    private async Task ApplyEnhancementSuccessAsyn()
    {
        curItem.enhancementLevel = curItem.EnhancementLevel + 1;
        curItem.BindingSumStat();
        await FirestoreUploader.SaveInventoryItemAsync(FirebaseManager.Instance.DbUser.UserId, curItem);
        MyDebug.Log($"강화 성공 → 레벨: {curItem.EnhancementLevel}");
    }
}
