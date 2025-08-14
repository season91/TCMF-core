## 📄 `src/CAY/README.md` (개인)

# [CAY] 개인 작업 모음
이 폴더에는 제가 구현한 핵심 모듈들이 포함되어 있습니다.

## 📌 담당 기능

### 1) **FirebaseCore**
- Firebase Authentication / Firestore 연동
- 사용자 데이터 **저장·로드 구조** 설계 (문서 ID 규칙, 경로, 타입 매핑)
- 최초 사용자 생성/닉네임 흐름, 중복 실행 방지(비동기 보호) 적용

### 2) **GachaCore**
- 가챠 전반 로직 관리: GachaManager에서 Draw → Pity → Transaction 전체 흐름 제어
- 캐시 관리(**GachaCache**)
- 뽑기 로직(**GachaDrawService**)
- 천장 로직(**GachaPityService**)
- 트랜잭션 처리(**GachaTransactionService**)
- GA 로그 전송

### 3) **InventoryCore**
- 아이템 **지급/소모 로직** 및 재화 관리(**ResourceService**)
- 장착/해제 시스템(**EquipmentInteractor**)
- 정렬/필터 모듈(**InventoryFilterSorter**)
- **강화/돌파/분해** 시스템(**EnhancementService**, **LimitBreakService**, **DismantleService**)
- 도감(**CollectionService**)
- **인벤토리 캐싱/읽기 전용 뷰**(**InventoryCache**)로 GC 최소화
- 유닛 관련 유틸(**UnitService**)

### 4) **ObjectPoolCore**
- **재사용 가능한 오브젝트 풀** 시스템 설계·구현
- Addressables 기반 **프리팹 로드 & 풀 초기화** 파이프라인

### 5) **RewardCore**
- 스테이지/가챠/퀘스트 보상 등 **보상 지급 규칙** 모듈화

### 6) **SceneCore**
- **씬 별 초기화 Manager** : 각 씬 별로 Setting할 부분 작성
- **LobbyManager 리팩토링**: 로비 전담 책임만 유지, 가챠 관련 의존성 제거
- (연계) **GachaManager & Services**: Draw/Pity/Transaction/Cache 분리로 **코드 가시성↑**, 버그 격리, 단위 테스트 용이

### 7) **UICore (일부)**
- **카메라 흔들림**: Cinemachine **Impulse** + `CameraShaker`(싱글톤)로 전역 트리거
- **컷인 연출**: **DOTween** `Sequence` 기반 타이밍/병렬 조합
- **엔딩크레딧**: DOTween 기반 스크롤 & 페이드, 스킵 처리, 씬 전환 연동

### 8) **URPCore**
- **URP 전환**(에셋 호환성 & 커스텀 렌더링 확장 대비)
- **ForceClearFeature**(ScriptableRendererFeature): `BeforeRendering` 단계에서 **Color/Depth 버퍼 강제 클리어**
- 설정은 **URP.asset**에 등록하여 **전역 적용** (모든 카메라 공통)

---

## 🎨 연출·렌더링 품질 개선

### 1. Cinemachine Impulse 흔들림
- **도전**: 전투 타격감 강화 필요  
- **과정**: Impulse Source/Listener + `CameraShaker`로 전역 호출 구조 설계  
- **결과**: 몰입도↑, 한 줄 호출로 재사용, Transform 직접 제어 대비 충돌 최소화

### 2. URP + ForceClearFeature
- **도전**: 화면 잔상, UI 뒤 배경 비침 이슈  
- **과정**: ScriptableRendererFeature로 파이프라인 개입 → `BeforeRendering`에 버퍼 강제 클리어, **URP.asset 전역 등록**  
- **결과**: 전환 시 잔상 해소, **빌트인 대비 문제 대응·예방 용이**(파이프라인 개입점 확보)

