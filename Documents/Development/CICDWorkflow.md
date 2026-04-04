# CI/CD Workflow

## 개요

`.github/workflows/` 하위 yml 파일로 관리되는 GitHub Actions 기반 자동화 파이프라인.

---

## 워크플로우

| 파일 | 트리거 브랜치 | 역할 |
|---|---|---|
| `ci-test.yml` | develop, main | Unity Test Runner 실행 |
| `ci-build.yml` | main | Android 빌드 및 Artifact 업로드 |

두 워크플로우 모두 아래 경로 변경 시에만 실행됨:
- `Assets/**`
- `Packages/**`
- `ProjectSettings/**`

---

## Secrets

GitHub 저장소 Settings → Secrets and variables → Actions에 등록.

| Secret | 용도 |
|---|---|
| `PAT_ASSET` | Private 에셋 저장소 접근 |
| `UNITY_LICENSE` | Unity 라이선스 인증 |
| `UNITY_EMAIL` | Unity 계정 이메일 |
| `UNITY_PASSWORD` | Unity 계정 비밀번호 |