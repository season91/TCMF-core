## 📄 `src/PJH/README.md` (개인)

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

<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="900">
  <style>
    .title { font-family: Inter, Arial, sans-serif; font-size: 40px; font-weight: 900; }
    .subtitle { font-family: Inter, Arial, sans-serif; font-size: 18px; fill: #444; }
    .panel { fill: #f8fafc; stroke: #1f2937; stroke-width: 2; rx: 14; }
    .label { font-family: Inter, Arial, sans-serif; font-size: 20px; font-weight: 800; }
    .note  { font-family: Inter, Arial, sans-serif; font-size: 16px; fill: #444; }
    .timeline { stroke: #111827; stroke-width: 4; }
    .bubble { fill: #fff; stroke: #111827; stroke-width: 2; rx: 10; }
    .good   { fill: #dcfce7; stroke: #166534; stroke-width: 2; rx: 10; }
    .warn   { fill: #fee2e2; stroke: #b91c1c; stroke-width: 2; rx: 10; }
    .mono  { font-family: "JetBrains Mono", Consolas, monospace; font-size: 16px; }
    .monoSmall { font-family: "JetBrains Mono", Consolas, monospace; font-size: 14px; }
    .badge { fill: #eef2ff; stroke: #3730a3; stroke-width: 2; rx: 12; }
  .link { stroke: #111827; stroke-width: 2; marker-end: url(#arrowhead); fill: none; }
</style>
  <defs>
    <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="10" refY="3.5" orient="auto">
      <polygon points="0 0, 10 3.5, 0 7" />
    </marker>
  </defs>


  <rect x="0" y="0" width="1600" height="900" fill="#ffffff"/>

  <text x="60" y="80" class="title">Effect — 수명/풀 관리 안전화</text>
  <text x="60" y="115" class="subtitle">Effect.Setup → Deactivate 규약 · EffectProvider 활성 집합 관리 · 전투 종료 시 ReturnAllEffectsToPool()</text>

  <!-- Timeline panel (back to original vertical spacing) -->
  <rect x="60" y="150" width="1480" height="280" class="panel"/>
  <text x="90" y="190" class="label">수명 타임라인</text>
  <line x1="120" y1="260" x2="1500" y2="260" class="timeline"/>

  <!-- stages (original-like positions) -->
  <rect x="150" y="220" width="170" height="80" class="bubble"/>
  <text x="165" y="255" class="note">Spawn</text>
  <text x="165" y="280" class="mono">Effect.Setup()</text>

  <rect x="410" y="220" width="170" height="80" class="bubble"/>
  <text x="425" y="255" class="note">Play</text>
  <text x="425" y="280" class="mono">Particle/VFX</text>

  <!-- Warn: OnParticleSystemStopped (two lines INSIDE, kept) -->
  <rect x="700" y="205" width="240" height="72" class="warn"/>
  <text x="715" y="235" class="note">OnParticleSystemStopped</text>
  <text x="715" y="260" class="note">미호출 가능(트레일)</text>

  <!-- Good: Timeout Guard (below warn, with (maxLifetime) on its own line) -->
  <rect x="700" y="295" width="240" height="68" class="good"/>
  <text x="715" y="320" class="note">Timeout Guard</text>
  <text x="715" y="345" class="monoSmall">AutoDeactivate (maxLifetime)</text>

  
<line x1="820" y1="277" x2="820" y2="295" class="link"/>
<line x1="940" y1="329" x2="980" y2="260" class="link"/>
<rect x="980" y="220" width="190" height="80" class="bubble"/>
  <text x="995" y="255" class="note">Deactivate</text>
  <text x="995" y="280" class="mono">Unregister + Return</text>

  <rect x="1260" y="220" width="170" height="80" class="bubble"/>
  <text x="1275" y="255" class="note">Pool</text>
  <text x="1275" y="280" class="mono">ObjectPool.Reuse</text>

  <!-- Badge (unchanged) -->
  <rect x="1080" y="160" width="430" height="46" class="badge"/>
  <text x="1100" y="190" class="note">트레일 콜백 미호출 → 타임아웃 가드로 100% 회수</text>

  <!-- Provider panel (keep v2 improvements for overflow) -->
  <rect x="60" y="460" width="740" height="360" class="panel"/>
  <text x="90" y="500" class="label">EffectProvider — 활성 집합 관리</text>
  <rect x="90" y="520" width="340" height="220" class="bubble"/>
  <text x="110" y="555" class="note">Register(Effect e)</text>
  <text x="110" y="585" class="note">Unregister(Effect e)</text>
  <text x="110" y="615" class="note">HashSet&lt;Effect&gt; _active</text>
  <text x="110" y="645" class="note">ReturnAllEffectsToPool()</text>
  <text x="110" y="675" class="monoSmall">foreach (var e in</text>
  <text x="110" y="695" class="monoSmall">  _active.ToList())</text>
  <text x="110" y="715" class="monoSmall">  e.Deactivate();</text>

  <rect x="460" y="540" width="320" height="160" class="good"/>
  <text x="480" y="580" class="note">안전 순회</text>
  <text x="480" y="605" class="mono">_active.ToList()로 복사 후</text>
  <text x="480" y="630" class="mono">Deactivate() → Unregister()</text>

  <!-- End-of-battle panel (restore original stacked result box) -->
  <rect x="840" y="460" width="720" height="360" class="panel"/>
  <text x="870" y="500" class="label">전투 종료 시 일괄 회수</text>
  <rect x="870" y="530" width="640" height="120" class="bubble"/>
  <text x="890" y="565" class="mono">BattleFlowFacade.OnBattleEnd</text>
  <text x="890" y="595" class="mono">→ EffectProvider.ReturnAllEffectsToPool()</text>

  <rect x="870" y="680" width="640" height="120" class="good"/>
  <text x="890" y="720" class="label">결과</text>
  <text x="890" y="750" class="note">· 장시간 플레이에도 활성 이펙트 누수 0</text>
  <text x="890" y="780" class="note">· 풀 재사용률 증가로 GC/Instantiate 스파이크 완화</text>
</svg>

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
