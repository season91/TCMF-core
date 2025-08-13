public enum ResourceType
{
    None,
    Gold,
    Diamond,
    Piece
}

/// <summary>
/// 인벤토리 관련 로직 담당 매니저
/// 해당 매니저는 인벤토리 아이템 관련 서비스와 캐시 초기화
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    public EquipmentInteractor EquipmentInteractor { get; private set; } // 장착
    public EnhancementService EnhancementService  { get; private set; } // 강화
    public InventoryFilterSorter InventoryFilterSorter { get; private set; } // 정렬
    public LimitBreakService LimitBreakService { get; private set; } // 돌파
    public DismantleService  DismantleService { get; private set; } // 분해
    public CollectionService CollectionService { get; private set; } // 도감
    
    public InventoryCache Cache { get; private set; } // 캐싱 데이터
    public ItemService ItemService { get; private set; } // 인벤토리 - 아이템 실제 변경 로직 서비스
    public UnitService UnitService { get; private set; } // 인벤토리 - 유닛 실제 변경 로직 서비스
    public ResourceService ResourceService { get; private set; } // 인벤토리 - 재화 실제 변경 로직 서비스
    public PityService PityService { get; private set; } // 인벤토리 - 천장 실제 변경 로직 서비스
    
    /// <summary>
    /// 매니저 공통 초기화 함수
    /// Firestore에서 인벤토리 아이템 서브컬렉션 캐싱
    /// User login 이후 호출해야함
    /// </summary>
    public void Initialize()
    {
        Cache = new InventoryCache();// 캐시 먼저 생성
        Cache.Initialize(); // 내부 데이터 초기화
        // 서비스에 의존성 주입
        ItemService = new ItemService(Cache); 
        UnitService = new UnitService(Cache);
        
        ItemService.Initialize(); // 아이템 바인딩 + 캐시 등록
        UnitService.Initialize(); // 유닛 바인딩 + 캐시 등록
        
        // 순서 조정 금지
        ResourceService = new ResourceService();
        PityService = new PityService();
        EquipmentInteractor = new EquipmentInteractor(Cache); 
        EnhancementService = new EnhancementService(ResourceService);
        InventoryFilterSorter = new InventoryFilterSorter(Cache);
        LimitBreakService = new LimitBreakService(ItemService, ResourceService);
        DismantleService =  new DismantleService(ItemService, ResourceService);
        CollectionService = new CollectionService();
        
        CollectionService.Initialize();
    }
}