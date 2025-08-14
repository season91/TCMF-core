using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 이펙트의 생성과 관리를 담당
/// 주요 역할:
/// 1. Object Pool에서 이펙트 오브젝트 획득 및 초기화
/// 2. 다양한 이펙트 타입(공격/스킬/상태효과)의 통합 관리
/// 3. 활성 이펙트 추적 및 일괄 정리 기능
/// 4. EffectSpawner와 ObjectPoolManager 사이의 중간 계층 역할
/// </summary>
public class EffectProvider : MonoBehaviour
{
    //활성화된 모든 이펙트 추적, 전투종료시 일괄 정리
    private readonly HashSet<Effect> activeEffects = new HashSet<Effect>();
    /// <summary>
    /// AttackEffect 스폰 - enum 타입으로 프리팹 키 생성
    /// </summary>
    public GameObject SpawnAttackEffect(AttackEffectType type, Vector3 position, Quaternion rotation)
    {
        // 명명 규칙: "Effect_Attack{타입명}" 
        string prefabKey = $"{StringAdrEffect.EffectAttack + type}";
        return SpawnEffect(prefabKey, position, rotation);
    }

    /// <summary>
    /// SkillEffect 스폰 - enum 타입으로 프리팹 키 생성
    /// </summary>
    public GameObject SpawnSkillEffect(SkillEffectType type, Vector3 position, Quaternion rotation)
    {
        // 명명 규칙: "Effect_Skill{타입명}"
        string prefabKey = $"{StringAdrEffect.EffectSkill + type}";
        return SpawnEffect(prefabKey, position, rotation);
    }

    /// <summary>
    /// StatusEffect 스폰 - enum 타입으로 프리팹 키 생성
    /// </summary>
    public GameObject SpawnStatusEffect(StatusEffectType type, Vector3 position, Quaternion rotation)
    {
        // 명명 규칙: "Effect_Status{타입명}"
        string prefabKey = $"{StringAdrEffect.EffectStatus + type}";
        return SpawnEffect(prefabKey, position, rotation);
    }

    /// <summary>
    /// 공통 Effect 스폰 로직
    /// </summary>
    private GameObject SpawnEffect(string prefabKey, Vector3 position, Quaternion rotation)
    {
        GameObject obj = ObjectPoolManager.Instance.Get(PoolCategory.Effect, prefabKey);
        
        if (obj == null)
        {
            MyDebug.LogError($"Effect 프리팹을 찾을 수 없음: {prefabKey}");
            return null;
        }

        ResetEffectObject(obj, position, rotation, prefabKey);
        return obj;
    }

    /// <summary>
    /// Effect 오브젝트 설정 (위치, 회전, 활성화)
    /// </summary>
    private void ResetEffectObject(GameObject obj, Vector3 position, Quaternion rotation, string prefabKey)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // Effect 컴포넌트가 있다면 설정
        Effect effect = obj.GetComponent<Effect>();
        if (effect != null)
        {
            effect.ResetEffect(prefabKey,this); // prefabKey를 넘겨서 반환 시 사용
        }
    }

    /// <summary>
    /// 활성 이펙트를 추적 목록에 등록
    /// </summary>
    /// <param name="effect"></param>
    public void Register(Effect effect)
    {
        activeEffects.Add(effect);
    }
    /// <summary>
    /// 활성 이펙트를 추적 목록에서 제거
    /// 제거 시점:
    /// 1. 파티클 자연 종료 시
    /// 2. 타임아웃으로 강제 종료 시
    /// 3. 일괄 정리 시
    /// </summary>
    /// <param name="effect"></param>
    public void Unregister(Effect effect)
    {
        activeEffects.Remove(effect);
    }
    /// <summary>
    /// Effect를 풀로 반환
    /// </summary>
    public void ReturnEffectToPool(string prefabKey, GameObject obj)
    {
        ObjectPoolManager.Instance.Return(PoolCategory.Effect, prefabKey, obj);
    }
    
    /// <summary>
    /// /// 모든 활성 이펙트를 일괄적으로 풀로 반환하는 메서드
    /// </summary>
    public void ReturnAllEffectsToPool()
    {
        if (activeEffects.Count == 0)
        {
            MyDebug.Log("활성 이펙트 없음");
            return;
        }
        
        // activeAll을 직접 순회하면서 각 원소에 대해 effect.Deactivate를 부르면
        // Deactivate() 내부에서 provider.Unregister가 실행되면,
        // 지금 순회 중인 컬렉션(activeAll)이 변경돼서 에러 남
        // 따라서 복사본을 생성해서 에러 방지 복사본에서 Deactivate 호출해도
        // effectProvider?.Unregister(this);를 통해 원본에서 삭제
        var copyList = new List<Effect>(activeEffects);
        int count = copyList.Count;

        for (int i = 0; i < count; i++)
        {
            var effect = copyList[i];
            if (effect != null && effect.gameObject.activeInHierarchy)
            {
                effect.Deactivate(); // 내부에서 Unregister + 풀반환
            }
        }

        MyDebug.Log($"모든 활성 Effect 반환 완료: {count}개");
    }
}
