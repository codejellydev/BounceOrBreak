# Git Workflow

## 브랜치 구조

```
main
└─ develop
   ├─ feat/gameplay
   ├─ feat/systems
   └─ feat/view
```

### main
- 출시 가능한 안정 버전
- 태그로 버전 관리 (v1.0.0, v1.1.0...)

### develop
- 개발 통합 브랜치
- 모든 feature 브랜치의 병합 대상

### hotfix
- main에서 분기
- 수정 후 main, develop 양쪽 머지

### Feature 브랜치

#### feat/gameplay
핵심 게임플레이 로직  
- Character, Physics

#### feat/systems
Core 및 게임 시스템 관리
- Core 레이어 (ResourceManager, SceneLoader), InputSystem

#### feat/view
UI 및 시청각 표현
- VFX, SFX, Animation, HUD

---

## 네이밍 규칙

- Feature: `feat/[기능명]`
- Hotfix: `hotfix/[이슈명]`