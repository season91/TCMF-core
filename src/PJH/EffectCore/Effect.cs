using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AttackEffectType
{
    Melee,
    Range,
    MonsterRange,
    PurpleMushroom,
    HappySeedling,
    DontCryingDeer,
    Boss1,
    Boss2,
    Boss3,
}

public enum SkillEffectType
{
    Usher,
    Momo,
    Ruru,
    GreenMushroom,
    HappySeedling,
    GuardianOfSilence,
    DontCryingDeer,
    SilentHummingbird,
    Boss1,
    Boss2,
    Boss3,
}

public class Effect : MonoBehaviour
{
    private ParticleSystem particle;
    private EffectProvider effectProvider;
    private Coroutine timeoutCoroutine;
    
    private bool isLoopParticle;
    private bool returned;
    private string prefabKey;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        
        var main = particle.main;
        isLoopParticle = main.loop;

        if (!isLoopParticle)
        {
            main.stopAction = ParticleSystemStopAction.Callback;
            main.loop = false;
        }
    }

    //타격 이펙트는 삭제하지 않고 메모리풀로 관리하기 때문에
    //Setup메서드에서 메모리풀 매개변수를 받아와 멤버변수 메모리풀에 저장
    public void Setup(string key, EffectProvider effect)
    {
        prefabKey = key;
        effectProvider = effect;
        returned = false;

        // Provider에 등록
        effectProvider?.Register(this);
        
        if (!isLoopParticle)
        {
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = StartCoroutine(AutoDeactivate());
        }
    }
    /// <summary>
    /// trail을 사용하는 원거리 이펙트들은 OnParticleSystemStopped이
    /// 호출 안되기 때문에 타이머 코루틴을 통하여 Deactivate시킴
    /// </summary>
    private IEnumerator AutoDeactivate()
    {
        yield return new WaitForSeconds(BattleConfig.Instance.effectMaxLifetime);
        Deactivate();
    }

    /// <summary>
    /// 파티클이 정지될때 호출되는 Unity 내장함수
    /// </summary>
    private void OnParticleSystemStopped()
    {
        MyDebug.Log("파티클 자동 정지됨 - 풀로 반환");

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
        
        Deactivate();
    }
    /// <summary>
    /// Effect 종료 시 풀로 반환
    /// </summary>
    public void Deactivate()
    {
        if (returned) return;
        returned = true;

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
        
        // 등록 해제 → 풀 반환
        effectProvider?.Unregister(this);
        effectProvider?.ReturnEffectToPool(prefabKey, gameObject);

        // 정리
        effectProvider = null;
        prefabKey = null;
    }
}
