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
![brochure_input_timeline_fixed_v4](https://github.com/user-attachments/assets/ba65a9f6-ca16-4694-9604-42228f066697)![brochure_input_timeline_fixed_v4](https://github.com/user-attachments/assets/2c96ce4d-5702-4ede-b5ec-309dd7d7c1f8)


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
