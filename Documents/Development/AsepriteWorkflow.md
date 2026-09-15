# Aseprite Workflow

픽셀 아트 제작을 AI 에이전트와 함께 진행하기 위한 구성.

Aseprite에 MCP 서버를 붙여 사람과 에이전트가 같은 문서를 함께 편집한다.
에이전트의 편집은 화면에 즉시 반영되며, 사람이 직접 고친 내용도 에이전트가 읽을 수 있다.

## 개요

아래 구성 요소는 모두 [`oaktreegames/aseprite-live-mcp`](https://github.com/oaktreegames/aseprite-live-mcp) (MIT)가 제공하며, 이 저장소에는 포함하지 않는다.

채택 배경과 리스크는 [ADR-9](/Documents/Development/ADR.md)를 참조한다.

---

## 구성

| 요소 | 설치 위치 | 역할 |
|---|---|---|
| `aseprite-live-mcp` | `uvx`가 실행 시 내려받음 | MCP 서버. Claude가 stdio로 실행 |
| `bridge/relay_server.py` | 저장소를 clone한 경로 | HTTP(9878)를 WebSocket(9877)으로 중계 |
| `aseprite_client/claude-bridge.lua` | `%APPDATA%/Aseprite/scripts/` | Aseprite 안에 상주하며 전달받은 Lua를 실행 |

```
Claude ──stdio──▶ MCP 서버 ──HTTP──▶ 릴레이 ──WS──▶ bridge.lua ──▶ Aseprite
```

릴레이가 분리되어 있는 것은 MCP 서버(세션마다 재시작)와 Aseprite 연결(계속 유지)의 수명이 다르기 때문이다. 세션을 재시작해도 Aseprite 쪽 연결은 끊기지 않는다.

Aseprite의 WebSocket은 클라이언트 전용이라 외부에서 Aseprite로 들어갈 수 없다.
따라서 작업을 시작할 때 `File > Scripts > claude-bridge`를 사람이 직접 실행해야 한다.

---

## 설정

설치는 위 저장소의 README를 따른다.

다음 항목은 README에 없으므로 별도로 처리한다.

| 항목 | 내용 |
|---|---|
| `mcp` 버전 고정 | 패키지가 `mcp>=1.10.0`으로만 의존성을 걸어 두어 2.x가 설치되면 import가 실패한다. 실행 인자에 `--with "mcp<2"`를 넣는다 |

---

## 향후 계획

유사한 구조의 Aseprite MCP 서버를 직접 구현할 예정이다.
