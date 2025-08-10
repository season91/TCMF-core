## 📄 `src/CAY/README.md` (개인)

# [CAY] 개인 작업 모음
이 폴더에는 제가 구현한 핵심 모듈들이 포함되어 있습니다.

## 📌 담당 기능
1. **FirebaseCore**
   - Firebase Authentication / Firestore 연동
   - 사용자 데이터 저장/로드 구조 설계
   - 비동기 데이터 요청 시 중복 호출 방지 (SemaphoreSlim 적용)

2. **ObjectPoolCore**
   - 재사용 가능한 오브젝트 풀 시스템 구현
   - Addressables 기반 프리팹 로드 & 풀 초기화
   - 풀 관리 UI 디버깅 기능 추가

3. **InventoryCore**
   - 아이템 지급/소모 로직
   - 장착/해제 시스템 (EquipmentInteractor)
   - 정렬/필터 모듈 (InventoryFilterSorter)


## 🛠 기술 스택
- Unity 2022.3 LTS
- C#
- Firebase Authentication, Firestore
- DOTween
