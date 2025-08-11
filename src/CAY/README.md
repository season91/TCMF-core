## 📄 `src/CAY/README.md` (개인)

# [CAY] 개인 작업 모음
이 폴더에는 제가 구현한 핵심 모듈들이 포함되어 있습니다.

## 📌 담당 기능
1. **FirebaseCore**
   - Firebase Authentication / Firestore 연동
   - 사용자 데이터 저장/로드 구조 설계

2. **InventoryCore**
   - 아이템 지급/소모 로직
   - 장착/해제 시스템 (EquipmentInteractor)
   - 정렬/필터 모듈 (InventoryFilterSorter)
   - 강화/돌파/분해 시스템 (EnhancementService, EnhancementService, DismantleService)
   - 재화 리소스 (ResourceService, EnhancementService)
   - 도감 시스템 (CollectionService)
   - 인벤토리 캐싱 데이터 (InventoryCache)
   - 유닛 (UnitService)

3. **ObjectPoolCore**
   - 재사용 가능한 오브젝트 풀 시스템 구현
   - Addressables 기반 프리팹 로드 & 풀 초기화
  
4. **RewardCore**
  - 보상 지급 로직

5. **UICore 일부**
  - 시네머신을 통한 카메리 흔들림 효과 구현 (CameraShaker)
  - 두트윈을 활용한 컷인 효과 구현 (Cutin)
  - 두트윈을 활용한 엔딩 크레딧 구현 (EndingCredits)
