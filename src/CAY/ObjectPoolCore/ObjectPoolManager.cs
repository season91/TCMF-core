using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PoolCategory
{
    Entity,
    Effect,
}
/// <summary>
/// 오브젝트 풀링 전용 매니저 (리소스는 외부에서 미리 로드됨)
/// </summary>
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    
    // key string은 프리팹 Addressables 주소명
    private Dictionary<PoolCategory, Dictionary<string, GameObject>> prefabDict = new();
    private Dictionary<PoolCategory, Dictionary<string, Queue<GameObject>>> poolDict = new();
    
    // 루트 캐시
    private readonly Dictionary<PoolCategory, Transform> categoryRoots = new();
    private Transform poolReturnRoot;
    
    /// <summary>
    /// 오브젝트 풀 등록 (프리팹 기준)
    /// - PoolCategory와 key를 기준으로 프리팹과 큐를 등록
    /// - 중복 등록은 무시됨
    /// </summary>
    public void RegisterObjectPool(PoolCategory category, string key, GameObject prefab)
    {
        // prefabDict의 카테고리 딕셔너리 가져오기 (없으면 생성)
        if (!prefabDict.TryGetValue(category, out var prefabCategoryDict))
        {
            prefabCategoryDict = new Dictionary<string, GameObject>();
            prefabDict[category] = prefabCategoryDict;
        }

        // poolDict의 카테고리 딕셔너리 가져오기 (없으면 생성)
        if (!poolDict.TryGetValue(category, out var poolCategoryDict))
        {
            poolCategoryDict = new Dictionary<string, Queue<GameObject>>();
            poolDict[category] = poolCategoryDict;
        }

        // 프리팹과 빈 큐 등록
        prefabCategoryDict[key] = prefab;
        poolDict[category][key] = new Queue<GameObject>();
        
        if (prefabCategoryDict.TryGetValue(key, out var pf))
        {
            MyDebug.Log($"[등록됨] {pf.name}");
        }
        else
        {
            MyDebug.LogError($"[접근 실패] {category}/{key} 등록 안 됨");
        }
    }

    /// <summary>
    /// 오브젝트 풀에서 객체 꺼내기
    /// - 해당 category/key로 등록된 프리팹이 있어야 사용 가능
    /// - 재사용 가능한 오브젝트가 없으면 Instantiate
    /// </summary>
    public GameObject Get(PoolCategory category, string key, Transform parent = null)
    {
        // 프리팹 등록 여부 확인 (등록 안 되어있으면 에러)
        if (!prefabDict.TryGetValue(category, out var prefabCategoryDict) || 
            !prefabCategoryDict.TryGetValue(key, out var prefab))
        {
            MyDebug.LogError($"[ObjectPoolManager] '{category}/{key}' 프리팹이 등록되지 않았음");
            return null;
        }

        // 풀 큐 조회 (카테고리와 키 기반)
        if (!poolDict.TryGetValue(category, out var poolCategoryDict) || 
            !poolCategoryDict.TryGetValue(key, out var objectQueue))
        {
            MyDebug.LogError($"[ObjectPoolManager] '{category}/{key}' 풀 큐가 존재하지 않음");
            return null;
        }
        
        // 큐에 오브젝트가 있으면 꺼내고, 없으면 새로 생성
        GameObject obj;
        
        if (objectQueue.Count > 0)
        {
            obj = poolDict[category][key].Dequeue();
        }
        else
        {
            obj = Instantiate(prefabDict[category][key]);
        }

        // 부모가 지정되어 있으면 계층 설정
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }

        // 오브젝트 활성화 후 반환
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 오브젝트 풀에 오브젝트 반환
    /// - 등록되지 않은 풀이라면 Destroy 처리
    /// </summary>
    public void Return(PoolCategory category, string key, GameObject obj)
    {
        if (obj == null)
            return;
        
        // 풀 존재 여부 확인 (카테고리 → 키 → 큐)
        if (!poolDict.TryGetValue(category, out var poolCategoryDict) || !poolCategoryDict.TryGetValue(key, out var objectQueue))
        {
            MyDebug.LogWarning($"[ObjectPoolManager] '{category}/{key}' 풀 없음 → Destroy 처리");
            Destroy(obj);
            return;
        }
        
        // ObjectPoolReturn이 없으면 생성
        if (poolReturnRoot == null)
        {
            var go = new GameObject("ObjectPoolReturn");
            poolReturnRoot = go.transform;
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        }
        
        // 해당 카테고리 루트가 없으면 생성
        if (!categoryRoots.TryGetValue(category, out var categoryRoot))
        {
            string rootName = category.ToString() + "PoolRoot";
            var go = new GameObject(rootName);
            categoryRoot = go.transform;
            categoryRoot.SetParent(poolReturnRoot, false);
            categoryRoots[category] = categoryRoot;
        }
        
        // 다시 풀매니저 아래로 이동시켜둠 (씬 종속 방지)
        obj.transform.SetParent(categoryRoot, false);
        
        // 오브젝트 비활성화 후 큐에 재삽입
        obj.SetActive(false);
        objectQueue.Enqueue(obj);
    }
}