![brochure_input_timeline_fixed_v4](https://github.com/user-attachments/assets/ba65a9f6-ca16-4694-9604-42228f066697)![brochure_input_timeline_fixed_v4](https://github.com/user-attachments/assets/2c96ce4d-5702-4ede-b5ec-309dd7d7c1f8)## 📄 `src/PJH/README.md` (개인)

# [PJH] 개인 작업 모음
이 폴더에는 제가 구현한 핵심 모듈들이 포함되어 있습니다.

## 📌 담당 기능
1. **BattleCore**

   - BattleManager: 전투 전체 흐름 제어 및 서비스 통합
   
   - ActionManager: 공격, 스킬 실행 및 시퀀스 관리
   
   - TurnManager: 턴제 전투 로직 및 순서 관리
   
   - AnimationController: 전투 애니메이션 제어
   
   - BattleConfig: 전투 관련 설정값 통합 관리
   
   - Facade 패턴: 복잡한 전투 시스템을 단순한 인터페이스로 추상화
   

     IBattleServices
   
      ├── IBattleActionFacade    // 액션 실행
   
      ├── IBattleUIFacade        // UI 이벤트
   
      ├── IBattleTargetingFacade // 타겟팅
   
      ├── IBattleFlowFacade      // 게임 흐름
   
      ├── IBattleInputFacade     // 입력 처리
   
      └── IBattleEffectFacade    // 이펙트 관리

3. **CharacterCore**
 
   - CharacterBase: 모든 캐릭터의 공통 기능 정의
   
   - Unit: 플레이어 유닛 전용 로직 및 데이터 처리
   
   - Monster: 몬스터 및 보스 전용 로직 및 AI 패턴
   
   - StatusEffectController: 버프/디버프 상태효과 관리 시스템

5. **EffectCore**
 
   - Effect: 이펙트 생명주기 및 풀 반환
   
   - EffectProvider: 이펙트 스폰 및 오브젝트 풀 연동
   
   - EffectSpawner: 전투 상황별 이펙트 배치 및 설정
   
   - ProjectileLauncher: 투사체 발사 및 궤적 처리
  
⚙️ 핵심 기능

🎮 턴제 배틀 시스템
![Uploading brochure
<svg xmlns="http://www.w3.org/2000/svg" width="1100" height="560" viewBox="0 0 1100 560">
  <defs>
    <style>
      .title { font: 700 22px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Noto Sans KR", Arial, sans-serif; }
      .subtitle { font: 600 16px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Noto Sans KR", Arial, sans-serif; }
      .label { font: 500 13px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Noto Sans KR", Arial, sans-serif; }
      .small { font: 500 12px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Noto Sans KR", Arial, sans-serif; }
    </style>
    <marker id="arrowDark" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="8" markerHeight="8" orient="auto-start-reverse">
      <path d="M 0 0 L 10 5 L 0 10 z" fill="#222"/>
    </marker>
  </defs>

  <rect x="0" y="0" width="1100" height="560" fill="#fafafa"/>
  <text x="40" y="48" class="title" fill="#111">타임라인: 3초 대기, 0.05s 폴링, 전환 시 즉시 취소 → 오토 실행</text>

  <!-- Axis -->
  <line x1="120" y1="140" x2="1020" y2="140" stroke="#222" stroke-width="1.4"/>
  <!-- ticks every 0.5s -->
  <g fill="#111" class="small">
    <g transform="translate(120,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=0.0s</text></g>
    <g transform="translate(270,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=0.5s</text></g>
    <g transform="translate(420,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=1.0s</text></g>
    <g transform="translate(570,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=1.5s</text></g>
    <g transform="translate(720,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=2.0s</text></g>
    <g transform="translate(870,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=2.5s</text></g>
    <g transform="translate(1020,0)"><line x1="0" y1="134" x2="0" y2="146" stroke="#222"/><text x="0" y="122" text-anchor="middle">t=3.0s</text></g>
  </g>
  <text x="570" y="168" text-anchor="middle" class="small" fill="#444">폴링 Δt = 0.05s (내부 코루틴 체크)</text>

  <!-- Case A -->
  <text x="40" y="250" class="subtitle" fill="#111">Case A: 수동 대기 중 t=1.2s에 자동으로 토글</text>
  <!-- Manual wait bar 0 ~ 1.2s -->
  <rect x="120" y="268" width="360" height="20" fill="#eaf3ff" stroke="#4a90e2"/>
  <text x="300" y="283" text-anchor="middle" class="small" fill="#1b5dab">Manual Wait</text>
  <!-- Toggle marker at 1.2s -->
  <line x1="480" y1="258" x2="480" y2="302" stroke="#d00" stroke-dasharray="3 3"/>
  <!-- Auto execute 1.2 ~ 1.7s -->
  <rect x="480" y="268" width="150" height="20" fill="#eaffea" stroke="#4caf50"/>
  <text x="555" y="283" text-anchor="middle" class="small" fill="#2e7d32">Auto: Targeting + Execute</text>
  <!-- cancelToken note -->
  <line x1="480" y1="338" x2="480" y2="288" stroke="#222" stroke-width="1.2" marker-end="url(#arrowDark)"/>
  <text x="480" y="352" text-anchor="middle" class="small" fill="#222">토글</text>

  <!-- Case B -->
  <text x="40" y="370" class="subtitle" fill="#111">Case B: 토글 없음 → t=3.0s 타임아웃으로 턴 넘어감</text>
  <!-- Manual wait bar 0 ~ 3.0s -->
  <rect x="120" y="388" width="900" height="20" fill="#fff7e6" stroke="#ff9800"/>
  <text x="570" y="403" text-anchor="middle" class="small" fill="#e65100">Manual Wait (3s 타이머)</text>
  <!-- Auto execute after timeout -->
  <rect x="1020" y="388" width="60" height="20" fill="#ffecec" stroke="#e53935"/>
  <text x="1050" y="403" text-anchor="middle" class="small" fill="#b71c1c">턴 넘어감</text>


  <!-- Case C -->
  <text x="40" y="450" class="subtitle" fill="#111">Case C: 3초 내 적 선택 → 스킬 발동(수동)</text>
  <!-- Manual wait bar 0 ~ 1.8s -->
  <rect x="120" y="468" width="540" height="20" fill="#eaf3ff" stroke="#4a90e2"/>
  <text x="390" y="483" text-anchor="middle" class="small" fill="#1b5dab">Manual Wait</text>
  <!-- Select marker at 1.8s -->
  <line x1="660" y1="458" x2="660" y2="502" stroke="#222" stroke-dasharray="3 3"/>
  <text x="660" y="456" class="small" fill="#222" text-anchor="middle">적 선택</text>
  <!-- Manual execute 1.8 ~ 2.4s -->
  <rect x="660" y="468" width="180" height="20" fill="#eaffea" stroke="#2e7d32"/>
  <text x="750" y="483" text-anchor="middle" class="small" fill="#2e7d32">Manual: Execute</text>

  <text x="40" y="536" class="small" fill="#444">주: 0.05s 폴링은 내부 체크 주기이며, UI 반응은 이벤트 기반으로 즉시/가까운 프레임에 갱신.</text>
</svg>
_input_timeline_fixed_v4.svg…]()


- 수동/자동 모드 실시간 전환
- 입력 제한시간 관리 (스킬 선택)
- 상태효과 턴 단위 관리
- 2배속 지원

🤖 타겟팅 시스템

- 기본공격 - 확률 기반 타겟 선택 (전열 60%, 중열 30%, 후열 10%)
- 보스 패턴 다양화 (순차/멀티/스플릿)
- 스킬 조건부 타겟팅 (체력 비율 최소, 자기 자신 버프 등)

🎨 이펙트 시스템
![brochure_effect_lifecycle_v8](https://github.com/user-attachments/assets/44c19cad-44d0-4cd9-9d36-4b8cdc830089)

- 오브젝트 풀링 기반 성능 최적화
- 자동 생명주기 관리 (메모리 누수 방지)
- 캐릭터별 특화 이펙트 (위치/크기 자동 조정)

🏆 보스 시스템

- 체력 조건부 스킬 발동 (70%, 50%, 30%)
- 패턴별 차별화 공격 방식
- 복합 액션 (기본공격 + 스킬 조합)

## 🛠 기술 스택
- Unity 2022.3 LTS
- C#
- Facade Pattern - 복잡도 관리
- DOTween - 애니메이션 시퀀싱
- Object Polling - 성능 최적화
- ScriptableObject - 하드코딩 숫자 제거
