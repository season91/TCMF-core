using System.Collections;
using UnityEngine;
/// <summary>
/// 기본 공격 이펙트 타입 열거형
/// 공격 방식과 캐릭터별로 분류된 이펙트 종류
/// </summary>
public enum AttackEffectType
{
    Melee,          // 근접 공격 (어셔, 루루 등)
    Range,          // 일반 원거리 공격 (모모 등)
    MonsterRange,   // 몬스터 원거리 공격 (코쿤, 초록버섯 등)
    PurpleMushroom, // 독버섯
    HappySeedling,  // 행복한 묘목
    DontCryingDeer, // 울지 않는 사슴
    Boss1,          // 나비여왕
    Boss2,          // 덤불뿌리
    Boss3,          // 울음꽃
}

/// <summary>
/// 스킬 이펙트 타입 열거형
/// 캐릭터별 고유 스킬 이펙트 종류
/// </summary>
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

/// <summary>
/// 전투 이펙트의 생명주기를 관리하는 컴포넌트
/// 파티클 재생 및 자동 관리
/// 완료 시 자동으로 오브젝트 풀 반환
/// </summary>
public class Effect : MonoBehaviour
{
    private const float EFFECT_LIFETIME = 5.0f; //트레일 이펙트의 OnParticleSystemStopped이 호출되지 않는 경우를 위한 안전 장치
    private static readonly WaitForSeconds LifetimeWait = // 성능 최적화를 위해 캐싱
        new WaitForSeconds(EFFECT_LIFETIME);
    
    private ParticleSystem particle;
    private EffectProvider effectProvider;
    private Coroutine timeoutCoroutine;
    
    private bool isLoopParticle;
    private bool isReturned; //오브젝트 풀 반환 여부 체크 , 중복 반환 방지
    private string prefabKey; 

    /// <summary>
    /// 파티클 시스템 설정 및 초기화
    /// 비루프 파티클의 경우 정지 시 콜백
    /// </summary>
    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        
        var main = particle.main;
        isLoopParticle = main.loop;

        if (!isLoopParticle)
        {
            main.stopAction = ParticleSystemStopAction.Callback; //파티클 정지 시 OnParticleSystemStopped() 자동 호출
            main.loop = false;
        }
    }
    
    /// <summary>
    /// 반환 상태 초기화
    /// EffectProvider에 자신을 등록 (활성 이펙트 추적용)
    /// 비루프 파티클의 경우 타임아웃 코루틴 시작
    /// </summary>
    public void ResetEffect(string key, EffectProvider effect)
    {
        prefabKey = key;
        effectProvider = effect;
        isReturned = false;

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
        yield return LifetimeWait;
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
        if (isReturned) return;
        isReturned = true;

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
