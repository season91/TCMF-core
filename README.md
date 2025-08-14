# tcmf-core  
**The Cat and Monster Forest** 프로젝트의 **C# 코어 로직** 저장소입니다.  
팀별/개인별 기여 코드를 모듈화하여 관리합니다.

---

## 📌 프로젝트 개요
- **장르**: 모바일 2D 수집형 RPG  
- **목적**: 게임 엔진에 종속되지 않는 순수 C# 코어 로직 개발 및 모듈화  
- **언어**: C#  
- **엔진**: Unity 2022.3 LTS (실제 적용 시)  
- **버전 관리**: Git  
- **협업 도구**: Notion, Figma

---

## 📂 레포 구조
```
tcmf-core/
├─ src/
│ ├─ CAY/ # 팀원 조아영 담당 모듈
│ ├─ CYI/ # 팀원 최영임 담당 모듈
│ ├─ PJH/ # 팀원 박지환 담당 모듈
│ └─ ...
└─ README.md # 전체 개요(본 문서)
```
> 각 팀원별 세부 기능 및 구현 설명은 `src/<이니셜>/README.md` 참조

---

## ⚙️ 공통 개발 규칙
- **네이밍(C#)**: `public` → PascalCase / `private` → camelCase
- **폴더/네임스페이스**: 기능별 단위 분리
- **코드 원칙**: 단일 책임, 디버그 테스트 후 PR
- **리소스 관리**: GC 최소화, 컬렉션 재사용, 불필요한 동적 할당 지양  
- **의존성 관리**: 외부 SDK는 필요한 경우에만 모듈 내부에 국한하여 사용
- [UI 컨벤션 노션](https://www.notion.so/UI-Convention-218015673c55813bb0a2c2bb850b6845) / [코드 컨벤션 노션](https://www.notion.so/Code-Convention-218015673c5581be8677fd2ecc8e42a5) / [깃 컨벤션 노션](https://www.notion.so/Github-Rules-218015673c5581039c0ef9a46d977295)


---

## 🛠 공통 모듈
- **Global** – 공통 함수
- **Static** – 공통 Util

---

## 🗂 관리 방식
- 모든 변경 사항은 PR을 통해 검토 후 병합
- 공통 규칙 위반 시 수정 요청
