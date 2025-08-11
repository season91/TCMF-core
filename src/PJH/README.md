## 📄 `src/PJH/README.md` (개인)

# [PJH] 개인 작업 모음
이 폴더에는 제가 구현한 핵심 모듈들이 포함되어 있습니다.

## 📌 담당 기능
1. **BattleCore**

   -BattleManager: 전투 전체 흐름 제어 및 서비스 통합
   
   -ActionManager: 공격, 스킬 실행 및 시퀀스 관리
   
   -TurnManager: 턴제 전투 로직 및 순서 관리
   
   -AnimationController: 전투 애니메이션 제어
   
   -BattleConfig: 전투 관련 설정값 통합 관리
   
   -Facade 패턴: 복잡한 전투 시스템을 단순한 인터페이스로 추상화
   

  --> IBattleServices
   
      ├── IBattleActionFacade    // 액션 실행
   
      ├── IBattleUIFacade        // UI 이벤트
   
      ├── IBattleTargetingFacade // 타겟팅
   
      ├── IBattleFlowFacade      // 게임 흐름
   
      ├── IBattleInputFacade     // 입력 처리
   
      └── IBattleEffectFacade    // 이펙트 관리

3. **CharacterCore**
 
   -CharacterBase: 모든 캐릭터의 공통 기능 정의
   
   -Unit: 플레이어 유닛 전용 로직 및 데이터 처리
   
   -Monster: 몬스터 및 보스 전용 로직 및 AI 패턴
   
   -StatusEffectController: 버프/디버프 상태효과 관리 시스템

5. **EffectCore**
 
   -Effect: 이펙트 생명주기 및 풀 반환
   
   -EffectProvider: 이펙트 스폰 및 오브젝트 풀 연동
   
   -EffectSpawner: 전투 상황별 이펙트 배치 및 설정
   
   -ProjectileLauncher: 투사체 발사 및 궤적 처리

## 🛠 기술 스택
- Unity 2022.3 LTS
- C#
- Facade Pattern
- DOTween
