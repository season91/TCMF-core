using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 엔티티(Unit, Monster 등)를 스폰하고 관리하는 클래스
/// </summary>
public class EntitySpawner
{
    private readonly List<Unit> unitObjectList;
    private readonly List<Monster> monsterObjectList;

    private readonly List<Unit> activeUnitList = new();
    private readonly List<Monster> activeMonsterList = new();

    public EntitySpawner(List<Unit> unitsObject, List<Monster> monstersObject)
    {
        unitObjectList = unitsObject;
        monsterObjectList = monstersObject;
    }

    /// <summary>
    /// 전체 비활성화
    /// </summary>
    public void DeactivateAll()
    {
        foreach (var unit in unitObjectList)
            unit.gameObject.SetActive(false);

        foreach (var monster in monsterObjectList)
            monster.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 엔티티 유닛 스폰
    /// </summary>
    public void UnitSpawn(int count)
    {
        activeUnitList.Clear();
        for (int i = 0; i < count &&  i < unitObjectList.Count; i++)
        {
            InventoryUnit inventoryUnit = StageManager.Instance.GetSelectedUnitByIndex(i);
            if (inventoryUnit != null)
            {
                var unit = unitObjectList[i];
                unit.gameObject.SetActive(true);
                unit.InitializeFromInventory(inventoryUnit);
                
                // 자식 1개에 붙이기 (예: MeshRoot, ModelHolder 등)
                Transform mountPoint = unit.transform.GetChild(0); // 첫 번째 자식

                // pool 꺼내기
                GameObject go = ObjectPoolManager.Instance.Get(PoolCategory.Entity, unit.UnitData.Prefab, mountPoint);
                
                // 자식으로 붙이고 위치/회전 초기화
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = new Vector3(1f, 1f, 1f);

                // 스폰된 prefab 캐싱
                unit.SetSpawnPrefab(go);
                
                UIManager.Instance.GetUI<UIWgHpBar>().BindingHpBar(unit);
                activeUnitList.Add(unit);
            }
        }
    }
    
    /// <summary>
    /// 엔티티 몬스터 스폰
    /// </summary>
    public void MonsterSpawn(Dictionary<string, int> monsterSpawn, bool isBossStage)
    {
        activeMonsterList.Clear();
        // spawn count 초기화
        int count = 0;
        foreach (var monsterKvp in monsterSpawn)
        {
            for (int i = 0; i < monsterKvp.Value && i < monsterObjectList.Count; i++)
            {
                if (isBossStage)
                {
                    count = 1; // 보스라면 2번째칸 고정
                }
                
                var monster = monsterObjectList[count];
                monster.gameObject.SetActive(true);
                monster.InitializeFromStage(monsterKvp.Key);
                
                // 자식 1개에 붙이기
                Transform mountPoint = monster.transform.GetChild(0); // 첫 번째 자식
                
                // pool 꺼내기
                GameObject go = ObjectPoolManager.Instance.Get(PoolCategory.Entity, monster.MonsterData.Prefab, mountPoint);
                
                // 자식으로 붙이고 위치/회전 초기화
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                
                // 스폰된 prefab 캐싱
                monster.SetSpawnPrefab(go);
                
                activeMonsterList.Add(monster);
                UIManager.Instance.GetUI<UIWgHpBar>().BindingHpBar(monster);
                count++;
            }
        }
    }

    /// <summary>
    /// pool 매니저 반환
    /// </summary>
    public void EntityDespawn()
    {
        foreach (var unit in activeUnitList)
        {
            unit.DespawnPrefab(unit.UnitData.Prefab);
        }

        foreach (var monster in activeMonsterList)
        {
            monster.DespawnPrefab(monster.MonsterData.Prefab);
        }
    }

    public IReadOnlyList<Unit> GetActiveUnits() => activeUnitList.AsReadOnly();
    public IReadOnlyList<Monster> GetActiveMonsters() => activeMonsterList.AsReadOnly();
}