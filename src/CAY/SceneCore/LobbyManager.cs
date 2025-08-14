/// <summary>
/// Lobby Scene에 필요한 초기화만 담당하는 일반 C# 클래스 (MonoBehaviour 아님)
/// </summary>
public class LobbyManager
{
    /// <summary>
    /// 매니저 공통 초기화 함수
    /// </summary>
    public void Initialize()
    {
        GachaManager.Instance.Initialize();
    }
    
    /// <summary>
    /// 씬 셋팅 함수
    /// </summary>
    public void Setting()
    {
        // 아이템, 유닛 기본 정렬
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryByObtainedTimeDesc(ItemType.Weapon);
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryByObtainedTimeDesc(ItemType.Armor);
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryUnitsByCodeAsc();
    }

    /// <summary>
    /// 씬 집입시 UI 셋팅 함수
    /// </summary>
    public void UISetting()
    {
        // 처음 오픈할 부분 
        UIManager.Instance.Open<UILobby>();
        UIManager.Instance.AllLoadingCompleteAction += UIManager.Instance.Open<UIUserInfo>;
        
        PoolRegister.RegisterUnitPrefabs();
    }

}