using Cinemachine;
using UnityEngine;

/// <summary>
/// 카메라 흔들기 전용 싱글톤 클래스
/// - DOTween 기반으로 흔들기 연출을 제공함
/// </summary>
public class CameraShaker : Singleton<CameraShaker>
{
    public CinemachineImpulseSource impulseSource;

    protected override void Awake()
    {
        base.Awake();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    
    public void Shake(float strength = -1f, float duration = -1f)
    {
        float finalStrength = (strength > 0f) ? strength : 1f;
        float finalDuration = (duration > 0f) ? duration : 1f;

        // 방향과 강도 지정
        impulseSource.m_DefaultVelocity = Vector3.one * finalStrength;

        // 실제 발사
        impulseSource.GenerateImpulse();
    
        Debug.Log($"[CameraShaker] Impulse generated with strength {finalStrength}, duration {finalDuration}");
    }
}