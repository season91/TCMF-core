using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 도감 슬롯 클래스
/// </summary>
public class CollectionSlot
{
    public CollectionStatus Status { get; private set; }
    public ItemData ItemData { get; }

    public CollectionSlot(CollectionStatus status, ItemData itemData)
    {
        Status = status;
        ItemData = itemData;
    }
    
    /// <summary>
    /// 도감 상태 갱신 함수. TryCollect에서 수집 처리 시 호출됨
    /// </summary>
    public void UpdateStatus(CollectionStatus newStatus)
    {
        Status = newStatus;
    }
    
    // 수집 여부
    public bool IsCollected => Status?.State == CollectionState.Collected || Status?.State == CollectionState.RewardClaimed;
    
    // 보상 여부
    public bool IsRewardClaimed => Status?.State == CollectionState.RewardClaimed;
}
/// <summary>
/// 도감 조회, 등록, 보상
/// </summary>
public class CollectionService
{
    // 도감 타입별 캐시 뷰모델 딕셔너리 (내부 수정용, GC 없는 리스트 재사용)
    private Dictionary<ItemType, List<CollectionSlot>> collectedSlotDict = new();

    // 도감 타입별 읽기 전용 뷰모델 딕셔너리 (UI 등 외부 노출용)
    private Dictionary<ItemType, IReadOnlyList<CollectionSlot>> collectedSlotReadOnlyDict = new();

    // 외부에서 접근 가능한 도감 뷰모델 캐시 (ItemType 기준 그룹)
    public IReadOnlyDictionary<ItemType, IReadOnlyList<CollectionSlot>> CollectedSlotDict => collectedSlotReadOnlyDict;

    // itemCode로 해당 도감의 collectionCode를 빠르게 찾기 위한 맵
    private Dictionary<string, string> itemCodeToCollectionCode = new();

    // 이미 수집한 도감인지 빠르게 확인하기 위한 Set 탐색
    private HashSet<string> collectedCodeSet = new();

    /// <summary>
    /// 유저가 실제로 수집한 도감만 반환
    /// ItemType 기준으로 그룹핑된 도감 상태 뷰모델 딕셔너리
    /// </summary>
    public static Dictionary<ItemType, IReadOnlyList<CollectionSlot>> GetUserCollectedViewModelDict()
    {
        var result = new Dictionary<ItemType, List<CollectionSlot>>();

        foreach (var status in UserData.collected.Collects)
        {
            // 도감 코드 기준으로 마스터 도감 정보 조회
            if (!MasterData.CollectionDataDict.TryGetValue(status.Code, out var collectionData)) continue;

            // 도감에 연결된 아이템 정보 조회
            if (!MasterData.ItemDataDict.TryGetValue(collectionData.ItemCode, out var itemData)) continue;

            var itemType = itemData.Type;
            var viewModel = new CollectionSlot(status, itemData);

            if (!result.ContainsKey(itemType))
                result[itemType] = new List<CollectionSlot>();

            result[itemType].Add(viewModel);
        }

        // IReadOnlyList 형태로 변환
        return result.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<CollectionSlot>)pair.Value
        );
    }

    /// <summary>
    /// 도감 수집 정보 초기화 및 디폴트 정렬
    /// </summary>
    public void Initialize()
    {
        collectedSlotDict.Clear();
        collectedSlotReadOnlyDict.Clear();
        itemCodeToCollectionCode.Clear();
        collectedCodeSet.Clear();
        
        var userCollectionDict = UserData.collected.Collects.ToDictionary(x => x.Code);
        collectedCodeSet = new HashSet<string>(userCollectionDict.Keys); 

        foreach (var pair in MasterData.CollectionDataDict)
        {
            var code = pair.Key;
            var collectionData = pair.Value;

            userCollectionDict.TryGetValue(code, out var userStatus);

            if (!MasterData.ItemDataDict.TryGetValue(collectionData.ItemCode, out var itemData))
            {
                MyDebug.Log($"해당 컬렉션 {collectionData.ItemCode} 에 아이템 데이터 존재하지 않음");
                continue; // itemData 없으면 처리 불가
            }
            // 맵핑 딕셔너리 채우기
            itemCodeToCollectionCode[collectionData.ItemCode] = collectionData.Code;

            // ViewModel 생성
            var viewModel = new CollectionSlot(userStatus, itemData);

            // 타입별로 리스트에 추가
            if (!collectedSlotDict.TryGetValue(itemData.Type, out var list))
            {
                list = new List<CollectionSlot>();
                collectedSlotDict[itemData.Type] = list;
                collectedSlotReadOnlyDict[itemData.Type] = list;
            }
            list.Add(viewModel);
            
            // 정렬: 등급 기준
            list.Sort((a, b) => a.ItemData.Rarity.CompareTo(b.ItemData.Rarity));
        }
    }

    /// <summary>
    /// 가챠 후 도감 등록 (단건)
    /// </summary>
    public async Task TryCollectAsync(ItemData itemData)
    {
        string itemCode = itemData.Code;
        // 마스터 도감데이터에 등록된 아이템인지 확인
        if (!itemCodeToCollectionCode.TryGetValue(itemCode, out var collectionCode))
        {
            MyDebug.LogWarning($"도감 등록 실패 - 등록되지 않은 itemCode: {itemCode}");
            return;
        }

        // 이미 수집한 도감인지 확인 (HashSet 기반 O(1))
        if (collectedCodeSet.Contains(collectionCode))
        {
            MyDebug.Log($"이미 등록된 아이템 코두?: {itemCode}");
            return;
        }
        
        // 새로운 수집 상태 추가
        var newStatus = new CollectionStatus
        {
            code = collectionCode,
            state = CollectionState.Collected,
            isRead = false
        };
            
        collectedCodeSet.Add(collectionCode);
            
        // CollectionSlot 수집 정보 반영
        var slotList = collectedSlotDict[itemData.Type];
        var slot = slotList.FirstOrDefault(s => s.ItemData.Code == itemCode);

        if (slot != null)
        {
            slot.UpdateStatus(newStatus);
        }
        
        await FirestoreUploader.SaveUserCollectedAsync(FirebaseManager.Instance.DbUser.UserId, newStatus);
        
        // 도감 갱신
        UpdateReadOnlyDic();
        return;
    }
    
    /// <summary>
    /// 가챠 후 도감 등록 (여러건)
    /// </summary>
    public async Task TryCollectAsync(List<ItemData> itemDataList)
    {
        List<CollectionStatus> statusList = new List<CollectionStatus>();
        
        foreach (var itemData in itemDataList)
        {
            string itemCode = itemData.Code;
            // 마스터 도감데이터에 등록된 아이템인지 확인
            if (!itemCodeToCollectionCode.TryGetValue(itemCode, out var collectionCode))
            {
                MyDebug.LogWarning($"도감 등록 실패 - 등록되지 않은 itemCode: {itemCode}");
                continue;
            }

            // 이미 수집한 도감인지 확인 (HashSet 기반 O(1))
            if (collectedCodeSet.Contains(collectionCode))
            {
                // MyDebug.Log($"이미 등록된 아이템 코두?: {itemCode}");
                continue;
            }
            // 새로운 수집 상태 추가
            var newStatus = new CollectionStatus
            {
                code = collectionCode,
                state = CollectionState.Collected,
                isRead = false
            };
            
            collectedCodeSet.Add(collectionCode);
            
            // CollectionSlot 수집 정보 반영
            var slotList = collectedSlotDict[itemData.Type];
            var slot = slotList.FirstOrDefault(s => s.ItemData.Code == itemCode);

            if (slot != null)
            {
                slot.UpdateStatus(newStatus);
            }
            statusList.Add(newStatus);
        }
        
        await FirestoreUploader.SaveUserCollectedBatchAsync(FirebaseManager.Instance.DbUser.UserId, statusList);
        MyDebug.Log("도감 등록 성공!");
        
        // 도감 갱신
        UpdateReadOnlyDic();
    }

    /// <summary>
    /// 도감 보상 지급 처리
    /// </summary>
    public async Task<bool> TryRewardCollectionAsync(string collectionCode)
    {
        CollectionSlot targetViewModel = null;

        // 1. Slot 딕셔너리에서 찾아오기
        foreach (var kv in collectedSlotReadOnlyDict)
        {
            targetViewModel = kv.Value.FirstOrDefault(vm => vm.Status?.Code == collectionCode);
            if (targetViewModel != null)
                break;
        }

        if (targetViewModel == null)
        {
            MyDebug.LogWarning($"도감 보상 실패 - 유저가 수집하지 않은 도감: {collectionCode}");
            return false;
        }

        // 2. 이미 보상을 수령했는지 확인
        if (targetViewModel.IsRewardClaimed)
        {
            MyDebug.Log($"도감 보상 실패 - 이미 보상 수령 완료된 도감: {collectionCode}");
            return false;
        }

        // 3. 마스터 데이터에서 도감 정보 조회
        if (!MasterData.CollectionDataDict.TryGetValue(collectionCode, out var collectionData))
        {
            MyDebug.LogWarning($"도감 보상 실패 - 존재하지 않는 도감 코드: {collectionCode}");
            return false;
        }

        // 4. 보상 지급
        targetViewModel.Status.state = CollectionState.RewardClaimed;
        targetViewModel.Status.isRead = true;
        
        var rewards = RewardManager.Instance.ComposeCollectionRewards(collectionData);
        
        await RewardManager.Instance.GrantRewards(rewards);
        MyDebug.Log($"도감 보상 지급 완료: {collectionCode}");
        
        // 5. 상태 저장
        await FirestoreUploader.SaveUserCollectedAsync(FirebaseManager.Instance.DbUser.UserId, targetViewModel.Status);
        MyDebug.Log($"도감 보상 상태 저장 완료: {collectionCode}");

        // 6. 유저 데이터 현행화
        await UserDataHandler.LoadToUserData(FirebaseManager.Instance.DbUser.UserId);
        return true;
    }

    /// <summary>
    /// UI 지급할 보상 목록 확인
    /// </summary>
    public List<RewardData> GetComposeCollectionRewards(string collectionCode)
    {
        if (!MasterData.CollectionDataDict.TryGetValue(collectionCode, out var collectionData))
        {
            MyDebug.LogWarning($"도감 보상 실패 - 존재하지 않는 도감 코드: {collectionCode}");
        }

        var rewards = RewardManager.Instance.ComposeCollectionRewards(collectionData);
        return rewards;
    }
    
    /// <summary>
    /// 도감 타입별 캐시에서 읽기 전용 딕셔너리를 갱신하는 함수
    /// </summary>
    private void UpdateReadOnlyDic(ItemType? targetType = null)
    {
        if (targetType.HasValue)
        {
            // 특정 타입만 갱신
            if (collectedSlotDict.TryGetValue(targetType.Value, out var list))
            {
                collectedSlotReadOnlyDict[targetType.Value] = list.AsReadOnly();
            }
        }
        else
        {
            // 전체 갱신
            foreach (var pair in collectedSlotDict)
            {
                collectedSlotReadOnlyDict[pair.Key] = pair.Value.AsReadOnly();
            }
        }
    }
}