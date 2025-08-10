# tcmf-core
**The Cat and Monster Forest** 프로젝트의 핵심 로직 저장소입니다.  
팀별/개인별 기여 코드를 모듈화하여 관리합니다.

## 📌 프로젝트 개요
- **장르**: 2D 수집형 RPG
- **엔진**: Unity 2022.3 LTS
- **언어**: C#
- **버전 관리**: Git + GitHub
- **협업 도구**: Notion, Figma
- **배포 환경**: Android

## 📂 구조
```
tcmf-core/
├─ src/
│ ├─ CAY/ # 팀원 조아영 (Firebase, Pool, Inventory)
│ │ ├─ FirebaseCore/
│ │ ├─ ObjectPoolCore/
│ │ ├─ InventoryCore/
│ │ └─ README.md # 개인 작업 설명
│ ├─ CYI/ # 팀원 최영임 (UI, Resource, Addressable)
│ │ ├─ UICore/
│ │ ├─ ....
│ │ └─ README.md # 개인 작업 설명
│ ├─ PJH/ # 팀원 박지환 (Battle, Skill)
│ │ ├─ BattleCore/
│ │ ├─ ....
│ │ └─ README.md # 개인 작업 설명
└─ README.md # 전체 프로젝트 개요
```

## 📄 팀원별 기여
| 팀원 | 담당 기능 | 주요 기술 |
|------|-----------|-----------|
| 조아영 | Firebase 연동, 오브젝트 풀링, 인벤토리 시스템 | Firebase SDK, Addressables, C# |
| 최영임 | 가챠 시스템, UI, Resource, Addressable.... | Addressables, C# |
| 박지환 | 전투 시스템, 사운드.... | Facade Pattern, C# |
