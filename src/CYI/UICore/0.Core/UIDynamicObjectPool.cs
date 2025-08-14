using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 전용 동적 오브젝트 풀 클래스
/// 지정한 컴포넌트 타입(T)을 재사용 가능한 형태로 관리
/// </summary>
public class UIDynamicObjectPool<T> where T : Component
{
    private readonly T origin; // 생성 객체
    private readonly Transform parent; // 생성 위치
    private readonly Queue<T> inactiveQueue = new (); // 재사용 풀
    private readonly List<T> activeObjectList = new(); // 모든 오브젝트 캐싱

    /// <summary>
    /// 생성자 호출 시, 미리 정해놓은 개수만큼의 오브젝트 생성
    /// </summary>
    /// <param name="originT">복제할 오브젝트</param> 
    /// <param name="parentTr">생성될 오브젝트들의 부모 트랜스폼</param> 
    /// <param name="initSize">초기 생성할 오브젝트개수</param> 
    public UIDynamicObjectPool(T originT, Transform parentTr, int initSize)
    {
        origin = originT;
        parent = parentTr;

        for (int i = 0; i < initSize; i++)
        {
            T obj = Object.Instantiate(origin, parent);
            obj.gameObject.SetActive(false);
            inactiveQueue.Enqueue(obj);
        }
        
        originT.gameObject.SetActive(false);
    }

    public IReadOnlyList<T> GetActiveList() => activeObjectList;
    public int GetInactiveCount() => inactiveQueue.Count;
    
    /// <summary>
    /// 비활성화된 오브젝트가 있으면 재사용,
    /// 없으면 새로 생성
    /// </summary>
    /// <returns>생성한 객체의 타입으로 반환</returns>
    public T Get()
    {
        T obj;

        if (inactiveQueue.Count > 0)
        {
            obj = inactiveQueue.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(origin, parent);
        }

        obj.gameObject.SetActive(true);
        activeObjectList.Add(obj);
        obj.transform.SetAsLastSibling();
        return obj;
    }

    /// <summary>
    /// 부모 아래 모든 T 타입을 검사 후 재활용
    /// 오브젝트 비활성화 => 풀로 반환
    /// </summary>
    public void OffAll()
    {
        foreach (var obj in activeObjectList)
        {
            if (obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(false);
                inactiveQueue.Enqueue(obj);
            }
        }
        
        activeObjectList.Clear();
    }
    
    /// <summary>
    /// 개별 재활용
    /// 오브젝트 비활성화 => 풀로 반환
    /// </summary>
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        inactiveQueue.Enqueue(obj);
    }
}