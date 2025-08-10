using UnityEngine;
/// <summary>
/// 아이템 관련 서비스와 캐시 초기화만 담당
/// </summary>
public enum ResourceType
{
    None,
    Gold,
    Diamond,
    Piece
}
public class InventoryManager : Singleton<InventoryManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    public EquipmentInteractor EquipmentInteractor { get; private set; }
    public EnhancementService EnhancementService  { get; private set; }
    public InventoryFilterSorter InventoryFilterSorter { get; private set; }
    public LimitBreakService LimitBreakService { get; private set; }
    public DismantleService  DismantleService { get; private set; }
    public CollectionService CollectionService { get; private set; }
    
    public InventoryCache Cache { get; private set; }
    public ItemService ItemService { get; private set; }
    public UnitService UnitService { get; private set; }
    public ResourceService ResourceService { get; private set; }
    public PityService PityService { get; private set; }
    
    /// <summary>
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
        
        ItemService.InitializeItems(); // 아이템 바인딩 + 캐시 등록
        UnitService.InitializeUnits(); // 유닛 바인딩 + 캐시 등록
        
        ResourceService = new ResourceService(Cache);
        PityService = new PityService();
        EquipmentInteractor = new EquipmentInteractor(Cache); 
        EnhancementService = new EnhancementService(ResourceService);
        InventoryFilterSorter = new InventoryFilterSorter(Cache);
        LimitBreakService = new LimitBreakService(ItemService, ResourceService);
        DismantleService =  new DismantleService(ItemService, ResourceService);
        CollectionService = new CollectionService();
        
        CollectionService.InitializeCollectedDic();
    }
}