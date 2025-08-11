/// <summary>
/// 로비 흐름을 제어하는 일반 C# 클래스 (MonoBehaviour 아님)
/// </summary>
public class LobbyManager
{
    public void Initialize()
    {
        GachaManager.Instance.Initialize();
    }
    
    public void Setting()
    {
        // 아이템, 유닛 기본 정렬
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryByObtainedTimeDesc(ItemType.Weapon);
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryByObtainedTimeDesc(ItemType.Armor);
        InventoryManager.Instance.InventoryFilterSorter.SortInventoryUnitsByCodeAsc();
    }

    public void UISetting()
    {
        // 처음 오픈할 부분 
        UIManager.Instance.Open<UILobby>();
        UIManager.Instance.AllLoadingCompleteAction += UIManager.Instance.Open<UIUserInfo>;
        
        PoolRegister.RegisterUnitPrefabs();
    }

}