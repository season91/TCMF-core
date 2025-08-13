using System;
using UnityEngine;
/// <summary>
/// 상태 기억만 하는 역할
/// 씬 전환 상관없고 내부에 데이터 보존할 이유가 없음
/// </summary>
public static class PoolRegister
{
    // lobby, battle 씬 때 unit 등록
    private static bool isUnitPrefabsRegistered = false;
    // battle 씬때 monster 등록
    private static bool isMonsterPrefabsRegistered = false;
    private static bool isEffectPrefabsRegistered = false;
    
    /// <summary>
    /// 유닛 프리팹을 ObjectPool에 등록
    /// - MasterData의 프리팹 주소 기준으로 리소스를 가져오고
    /// - 해당 프리팹을 key로 등록하여 이후 Spawn/Despawn 시 재사용 가능하게 함
    /// </summary>
    public static void RegisterUnitPrefabs()
    {
        if (isUnitPrefabsRegistered)
        {
            MyDebug.Log("이미 유닛 프리팹 등록 완료");
            return;
        }

        // 유닛 프리팹 등록
        foreach (var unitData in MasterData.UnitDataDict)
        {
            // key: 유닛 프리팹 Addressables 주소 
            string key = unitData.Value.Prefab;

            // Addressables 리소스 매니저에서 해당 프리팹 불러오기
            GameObject prefab = ResourceManager.Instance.GetResource<GameObject>(key);

            // 풀 매니저에 등록 (카테고리: Entity)
            ObjectPoolManager.Instance.RegisterObjectPool(PoolCategory.Entity, key, prefab);
        }
    }

    /// <summary>
    /// 몬스터 프리팹을 ObjectPool에 등록
    /// </summary>
    public static void RegisterMonsterPrefabs()
    {
        if (isMonsterPrefabsRegistered)
        {
            MyDebug.Log("이미 몬스터 프리팹 등록 완료");
            return;
        }

        // isMonsterPrefabsRegistered = true;
        MyDebug.Log("몬스터 프리팹 최초 등록");

        // 몬스터 프리팹 등록
        foreach (var monsterData in MasterData.MonsterDataDict)
        {
            // key: 몬스터 프리팹 Addressables 주소 
            string key = monsterData.Value.Prefab;

            // Addressables 리소스 매니저에서 해당 프리팹 불러오기
            GameObject prefab = ResourceManager.Instance.GetResource<GameObject>(key);

            // 풀 매니저에 등록 (카테고리: Entity)
            ObjectPoolManager.Instance.RegisterObjectPool(PoolCategory.Entity, key, prefab);
        }
    }

    /// <summary>
    /// 어드레서블로 로드된 Effect 프리팹들을 ObjectPoolManager에 등록
    /// </summary>
    public static void RegisterEffectPrefabs()
    {
        if (isEffectPrefabsRegistered)
        {
            MyDebug.Log("이미 등록된 이펙트 프리펩이 있음");
            return;
        }
        
        MyDebug.Log("첫 이펙트 프리펩 등록");
        
        // 모든 Effect 타입별로 등록
        RegisterAllEffects();
    }

    /// <summary>
    /// 모든 Effect 프리팹을 ObjectPoolManager에 등록
    /// </summary>
    private static void RegisterAllEffects()
    {
        RegisterAttackEffects();
        RegisterSkillEffects();
        RegisterStatusEffects();
    }

    private static void RegisterAttackEffects()
    {
        // AttackEffectType enum의 모든 값을 순회
        foreach (AttackEffectType effectType in Enum.GetValues(typeof(AttackEffectType)))
        {
            string addressableKey = $"{StringAdrEffect.EffectAttack + effectType}"; // 어드레서블 주소
            GameObject prefab = ResourceManager.Instance.GetResource<GameObject>(addressableKey);
            
            if (prefab != null)
            {
                ObjectPoolManager.Instance.RegisterObjectPool(PoolCategory.Effect, addressableKey, prefab);
            }
        }
    }

    private static void RegisterSkillEffects()
    {
        // SkillEffectType enum의 모든 값을 순회
        foreach (SkillEffectType effectType in Enum.GetValues(typeof(SkillEffectType)))
        {
            string addressableKey = $"{StringAdrEffect.EffectSkill + effectType}"; // 어드레서블 주소
            GameObject prefab = ResourceManager.Instance.GetResource<GameObject>(addressableKey);
            
            if (prefab != null)
            {
                ObjectPoolManager.Instance.RegisterObjectPool(PoolCategory.Effect, addressableKey, prefab);
            }
        }
    }

    private static void RegisterStatusEffects()
    {
        // StatusEffectType enum의 모든 값을 순회
        foreach (StatusEffectType effectType in Enum.GetValues(typeof(StatusEffectType)))
        {
            string addressableKey = $"{StringAdrEffect.EffectStatus + effectType}"; // 어드레서블 주소
            GameObject prefab = ResourceManager.Instance.GetResource<GameObject>(addressableKey);
            
            if (prefab != null)
            {
                ObjectPoolManager.Instance.RegisterObjectPool(PoolCategory.Effect, addressableKey, prefab);
            }
        }
    }
}