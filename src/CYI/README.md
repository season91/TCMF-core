## 📄 `src/CYI/README.md` (개인)

# [CYI] 개인 작업 모음
이 폴더에는 제가 구현한 핵심 모듈들이 포함되어 있습니다.

## 📌 담당 기능
### 1) **ResourceCore**
   - Addressables 리소스 로드/릴리즈 일원화 (주소/라벨 단위)
   - 공통 비동기 API 제공 및 중복 로드/해제 방지
   - 리소스 키/라벨 상수화(StringNameSpace)로 오탈자·하드코딩 예방

### 2) **SceneCore**
   - 씬 전환 진입점(EnterSceneAsync) 및 로딩 Progress/Non‑Progress 지원
   - 전환 전 정리 루틴(DOTween Kill, 불필요 리소스 Unload) 적용
   - 예외 처리·복귀 로직과 씬별 초기화 훅 연동

### 3) **StageCore**
   - 스테이지 라이프사이클 총괄(준비→시작→클리어/실패→정산)
   - 웨이브/패턴 기반 스폰(EntitySpawner), 선택 유닛 검증·주입
   - 스테이지 데이터 로딩·파싱(StageDataLoader, reward/random/first “||” 규칙)

### 4) **UICore**
   - UIBase/UIPopupBase로 초기화·열기/닫기·애니메이션 표준화, 제네릭 Open<T>() 지원
   - UIManager 중앙집중 관리: 등록/조회/상태, 팝업 Queue, 로딩 진행도 갱신
   - 씬 전환 연계(SceneLoadController): 전환 전 정리 → Addressables 로드/언로드 → UI 재초기화
   - 구조화된 디렉토리(0.Core~6.Widget)와 Addressables 프리팹 캐싱으로 확장성·성능 확보
   - 동적 오브젝트 풀(UIDynamicObjectPool): 제네릭 기반 재사용 구조로 UI·게임 오브젝트 GC 최소화 및 프레임 안정성 향상

### 5) **GachaCore**
   - 가챠 전반 로직 관리: GachaManager에서 Draw → Pity → Transaction 전체 흐름 제어
   - 캐시 관리(**GachaCache**)
   - 뽑기 로직(**GachaDrawService**)
   - 천장 로직(**GachaPityService**)
   - 트랜잭션 처리(**GachaTransactionService**)

## 🛠 기술 스택
- Unity 2022.3 LTS
- C#
- Addressable
- DOTween
