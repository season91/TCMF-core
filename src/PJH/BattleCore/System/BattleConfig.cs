using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 관련 설정
/// 전투 관련 하드코딩 제거를 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "BattleConfig", menuName = "Config/Battle Config")]
public class BattleConfig : ScriptableObject
{
    [Header("=== 턴 설정 ===")]
    public float turnDelaySeconds = 0.5f;
    public float skillWaitTime = 3.0f;
    public float roundStartDelay = 1f;
    public float turnTransitionDelay = 1f;

    [Header("=== 유닛 설정 ===")]
    [Tooltip("캐릭터 이동 속도")]
    public float moveSpeed = 30f;
    
    [Tooltip("액션 간 간격")]
    public float actionInterval = 0.5f;
    [Tooltip("근접 공격 시 타겟과의 거리")]
    public float meleeAttackOffset = 2f;
    [Tooltip("기본 공격 후 대기 시간")]
    public float basicAttackDelay = 0.3f;
    [Tooltip("스킬 사용 후 대기 시간 (일반)")]
    public float skillDelay = 0.5f;
    [Tooltip("루루 스킬 사용 후 대기 시간")]
    public float ruruSkillDelay = 1.5f;
    [Tooltip("타격 간의 시간 조정")]
    public float hitInterval = 0.2f;
    
    [Header("=== 애니메이션 설정 ===")]
    [Tooltip("피격 애니메이션 펀치 스케일")]
    public Vector3 hitPunchScale = new Vector3(0.3f, 0, 0);
    [Tooltip("피격 애니메이션 지속 시간")]
    public float hitAnimationDuration = 0.3f;
    [Tooltip("피격 애니메이션 진동 횟수")]
    public int hitAnimationVibrato = 8;
    [Tooltip("피격 애니메이션 탄성")]
    public float hitAnimationElasticity = 0.3f;
    [Tooltip("색상 변경 지속 시간")]
    public float colorChangeDuration = 0.1f;
    [Tooltip("사망 애니메이션 지속 시간")]
    public float deathAnimationDuration = 0.5f;
    [Tooltip("사망 시 회전 각도")]
    public Vector3 deathRotation = new Vector3(0, 0, 90f);
    [Tooltip("회피 백스텝 거리")]
    public float evasionBackstepDistance = 1.5f;
    [Tooltip("회피 애니메이션 백스텝 시간")]
    public float evasionBackstepTime = 0.1f;
    [Tooltip("회피 애니메이션 복귀 시간")]
    public float evasionReturnTime = 0.25f;

    [Header("=== 수동 모드 설정 ===")]
    [Tooltip("자동 모드 체크 간격")]
    public float autoModeCheckInterval = 0.1f;
    [Tooltip("프레임 체크 간격")]
    public float frameCheckInterval = 0.05f;
    
    [Header("=== 확률 및 비율 상수 ===")]
    [Tooltip("독버섯의 중독 확률 (%)")]
    public float poisonProbability = 50f;
    [Tooltip("보스3 스플릿 데미지 비율")]
    public float splitDamageRatio = 0.1f;
    [Tooltip("메인 타겟 투사체 스케일")]
    public float mainTargetProjectileScale = 1.6f;
    [Tooltip("스플릿 타겟 투사체 스케일")]
    public float splitTargetProjectileScale = 1.0f;
    
    [Header("=== 이펙트 설정 ===")]
    [Tooltip("보스용 어셔 스킬 이펙트 오프셋")]
    public Vector3 usherSkillBossOffset = new Vector3(-0.5f, 2.5f, 0f);
    [Tooltip("보스용 어셔 스킬 스케일 배율")]
    public float usherSkillBossScaleMultiplier = 1.5f;
    [Tooltip("이펙트 기본 오프셋")]
    public Vector3 effectOffset = new Vector3(-0.75f, 1.5f, 0);
    [Tooltip("안보이는 이펙트를 위한 오프셋")]
    public Vector3 forwardOffset = new Vector3(0, 0, 0.1f);
    [Tooltip("투사체 이동 시간")]
    public float projectileMoveTime = 0.5f;
    [Tooltip("투사체 이동 후 데미지 적용 대기 시간")]
    public float projectileDamageDelay = 1f;
    [Tooltip("보스 몬스터 이펙트 오프셋")]
    public Vector3 bossEffectOffset = new Vector3(-2.3f, 2f, 0.1f);
    [Tooltip("이펙트 스케일 타겟 보스")]
    public float effectScaleTargetBoss = 1.7f;
    [Tooltip("어셔 스킬 이펙트 스케일")]
    public float usherSkillEffectScale = 0.7f;
    [Tooltip("루루 스킬 이펙트 스케일")]
    public float ruruSkillEffectScale = 0.6f;
    [Tooltip("보스 스킬 이펙트 높이 오프셋")]
    public float bossSkillEffectHeight = 4f;
    [Tooltip("보스3 스킬 이펙트 오프셋")]
    public Vector3 boss3SkillOffset = Vector3.left * 2f;
    [Tooltip("투사체 높이 오프셋")]
    public float projectileHeightOffset = 0.15f;
    [Tooltip("이펙트 최대 생존 시간")]
    public float effectMaxLifetime = 5f;
    [Tooltip("몬스터 공격 이펙트 높이 오프셋")]
    public Vector3 monsterAttackHeightOffset = Vector3.up * 2f;
    [Tooltip("묘목 스킬 이펙트 스케일")]
    public Vector3 happySeedlingSkillScale = new Vector3(0.8f, 1f, 1f);
    [Tooltip("사슴 스킬 이펙트 스케일")]
    public float dontCryingDeerSkillScale = 1.5f;
    [Tooltip("보스3 스킬 이펙트 스케일")]
    public float boss3SkillScale = 2.5f;

    [Header("=== 이펙트 각도 설정 ===")]
    [Tooltip("근접 공격 이펙트 회전")]
    public Vector3 meleeAttackEffectRotation = new Vector3(24, -98, 82);
    [Tooltip("어셔 스킬 이펙트 회전")]
    public Vector3 usherSkillEffectRotation = new Vector3(180, 193, -13);
    [Tooltip("루루 스킬 이펙트 회전")]
    public Vector3 ruruSkillEffectRotation = new Vector3(270, 60, 90);
    [Tooltip("보스 스킬 이펙트 회전")]
    public Vector3 bossSkillEffectRotation = new Vector3(0, 0, 90);
    [Tooltip("상태 효과 회전")]
    public Vector3 statusEffectEuler = new Vector3(10f, 0f, 0f);
    [Tooltip("보스3 스킬 이펙트 회전")]
    public Vector3 boss3SkillEuler = new Vector3(0, -15f, 90f);

    [Header("=== 보스 세팅 ===")]
    [Tooltip("보스 스킬 발동 체력 트리거")]
    public List<float> bossSkillHealthTriggers = new List<float> { 70f, 50f, 30f };
    [Tooltip("보스 스킬 이펙트 지속 시간")]
    public float hallucinationEffectDuration = 2.5f;

    // REFACTOR : SO로 쓸꺼면 addressable로 가져와서 써야대고
    // 진짜 싱글톤으로 쓸거면 SO를 싱글톤으로 쓰는건 좀 아닌것 같음!
    // 둘 중 하나로 선택해서 리팩토링 필요
    
    // 싱글턴 인스턴스
    private static BattleConfig _instance;
    public static BattleConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<BattleConfig>("BattleConfig"); // ResourceManager.GetResource로 바꿔야함
                if (_instance == null)
                {
                    MyDebug.LogError("BattleConfig를 Resources 폴더에서 찾을 수 없습니다!");
                }
            }
            return _instance;
        }
    }
}