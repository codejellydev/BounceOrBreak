# CI/CD Workflow

## 개요

`.github/workflows/` 하위 yml 파일로 관리되는 GitHub Actions 기반 자동화 파이프라인.

---

## 워크플로우

| 파일 | 트리거 브랜치 | 역할 |
|---|---|---|
| `ci-test.yml` | develop, main | Unity Test Runner 실행 |
| `ci-build.yml` | main | Android 빌드(서명 AAB + universal APK + 네이티브 심볼) 및 Artifact 업로드. main 빌드 성공 시 `v{bundleVersion}` 태그 푸시 |

두 워크플로우 모두 아래 경로 변경 시에만 실행됨:
- `Assets/**`
- `Packages/**`
- `ProjectSettings/**`

그 외 브랜치의 빌드는 Actions 탭에서 수동 실행

---

## 버전

- versionName: main은 `bundleVersion` 그대로, 그 외는 `{bundleVersion}-dev.{실행 번호}+{SHA 7자리}` (예: `0.1.0-dev.23+d48e256`)
- versionCode: `bundleVersion`에서 유도. `major×10000 + minor×100 + patch` (예: 0.1.4 → 104)
- Play에는 main 빌드만 올린다. `-dev`가 붙은 빌드는 개발 기기 전용

---

## Secrets

GitHub 저장소 Settings → Secrets and variables → Actions에 등록.

| Secret | 용도 |
|---|---|
| `PAT_ASSET` | Private 에셋 저장소 접근 |
| `UNITY_LICENSE` | Unity 라이선스 인증 |
| `UNITY_EMAIL` | Unity 계정 이메일 |
| `UNITY_PASSWORD` | Unity 계정 비밀번호 |
| `ANDROID_KEYSTORE_BASE64` | 서명용 키스토어 파일의 Base64 |
| `ANDROID_KEYSTORE_PASS` | 키스토어 비밀번호 |
| `ANDROID_KEYALIAS_NAME` | 키 alias |
| `ANDROID_KEYALIAS_PASS` | 키 비밀번호 |

---

## Troubleshooting

### 테스트·빌드가 시작되지 않음

- 증상: Unity 로그에 `TimeStamp validation failed`
- 원인: game-ci가 v4.4.0에서 라이선스 활성화 방식을 바꿨고, `@v4`로 지정돼 있으면 자동으로 그 버전을 쓴다
- 조치: `unity-test-runner@v4.3.2`, `unity-builder@v5.0.1`로 고정한다 (2026-09-13)
