# Unity C# 네이밍 컨벤션

## 🔤 약어(Acronym) 표기 규칙

약어가 포함된 이름을 지을 때는 글자 수에 따라 대소문자를 구분하여 작성한다.

* **2글자 약어:** 모두 대문자로 표기한다.
  * **예시:** `UI`, `IO`, `DB`, `ID`, `OS`
  * **적용:** `UIPanel`, `DBConnection`, `PlayerID`
* **3글자 이상 약어:** 첫 글자만 대문자, 나머지는 소문자로 표기한다 
  * **예시:** `Api`, `Xml`, `Bgm`, `Sfx`, `Html`
  * **적용:** `BgmVolume` (O) / `BGMVolume` (X)
  * **적용:** `AppManager` (O) / `APPManager` (X)

## 🏷️ 필드 표기

* **private·protected·internal 필드:** `_camelCase`
* **상수(`const`, `static readonly`):** `UPPER_SNAKE_CASE`
* **public 필드, 프로퍼티:** `PascalCase`

## 📦 상수 위치

상수(`const`, `static readonly`)는 클래스 안에 두지 않고 컨테이너에 모은다. Core는 `AppConstants`, Game은 `GameConstants`.

* 상수 그룹은 기능 단위로 짓는다 (ex. `Audio`). 그 안에서 한 클래스 전용 값이 여럿이면 중첩 클래스로 둔다 (ex. `UI.Dialog`)
* 예외: 실행 시 계산해 채우는 `static readonly`는 캐시이므로 그 클래스에 둔다. Editor·Tests 어셈블리는 대상이 아니다

## 📋 Region 구조

### Region 순서 (필수)

```csharp
#region Nested Classes          // 필요시
#region Injection               // VContainer 의존성 주입 시
#region Constructors
#region Events
#region Properties
#region Public Variables
#region Internal Variables
#region Protected Variables
#region Private Variables
#region VContainer Lifecycle    // VContainer 생명주기 인터페이스 구현 시
#region Unity Lifecycle
#region Public Methods
#region Internal Methods
#region Protected Methods
#region Private Methods
#region Input Action Callbacks  // 필요시
#region Debug                   // 필요시
```

### Region 내부 정렬 규칙

#### 1. Region 간격

Region과 Region 사이에는 **두 줄 띄어쓰기**

#### 2. 접근 제한자 순서

각 region 내에서는 **public → internal → protected → private** 순으로 정렬

```csharp
#region Events
// public event들
public event Action OnStarted;
public event Action<int> OnCountChanged;

// internal event들 (있다면)
internal event Action OnLoaded;

// protected event들 (있다면)
protected event Action OnRefreshed;

// private event들 (있다면)
private event Action OnDisposed;
#endregion


#region Properties
// public 프로퍼티들
public int Count { get; set; }
public bool IsActive => _isActive;

// internal 프로퍼티들 (있다면)
internal bool IsReady { get; set; }

// protected 프로퍼티들 (있다면)
protected bool IsInitialized { get; set; }

// private 프로퍼티들 (있다면)
private float Ratio { get; set; }
#endregion
```

#### 3. Variables에서 어트리뷰트 우선순위

동일한 접근 제한자 내에서는 **어트리뷰트가 붙은 변수가 상단**에 위치

```csharp
#region Private Variables
// 어트리뷰트가 붙은 변수들 (상단)
[Header("Settings")]
[SerializeField] private float _speed;
[HideInInspector] [SerializeField] private bool _isDebug;

// 일반 private 변수들 (하단)
private Transform _target;
private float _timer;
#endregion
```

#### 4. 어트리뷰트 표기법

- `[SerializeField]`, `[HideInInspector]` 등 짧고 자주 사용되는 어트리뷰트는 **1줄로 표기**
- 가능한 한 **변수 상단에 표기**

```csharp
// ✅ 권장
[Header("Player Settings")]
[SerializeField] private float _moveSpeed;

// ❌ 지양
[Header("Player Settings")]
[SerializeField]
private float _moveSpeed;
```

## 🔧 코드 스타일

### 빈 함수 표기법

빈 함수는 **한 줄로 표기**

```csharp
// ✅ 권장
private void Awake()
{ }

public void Initialize()
{ }

// ❌ 지양
private void Awake()
{
}
```

### Unity Lifecycle 순서

일반적인 Unity 실행 순서를 따름

```csharp
#region Unity Lifecycle
private void Awake()
{ }

private void OnEnable()
{ }

private void Start()
{ }

private void FixedUpdate()
{ }

private void Update()
{ }

private void LateUpdate()
{ }

private void OnDisable()
{ }

private void OnDestroy()
{ }
#endregion
```

### Injection Region

VContainer 의존성 주입 시 사용. 주입 필드와 생성자 또는 Construct 메서드를 함께 배치.

- **non-MonoBehaviour**: 생성자 주입 사용 (`readonly` 보장, `[Inject]` 어트리뷰트 불필요)
- **MonoBehaviour**: 생성자를 Unity가 관리하므로 `[Inject]` Construct 메서드 주입 사용. 필드 주입(`[Inject]` 필드)은 지양.

```csharp
// non-MB
#region Injection
private readonly SomeService _someService;

public SomeClass(SomeService someService)
{
    _someService = someService;
}
#endregion

// MB
#region Injection
private SomeService _someService;

[Inject]
public void Construct(SomeService someService)
{
    _someService = someService;
}
#endregion
```

### Injection 파라미터 줄바꿈

Injection region의 생성자/Construct 메서드 파라미터가 **2개 이상**일 경우,
각 파라미터를 **한 줄씩** 배치한다.

의존성 목록이 세로로 정렬되어 각 의존성을 한눈에 파악하기 쉽고,
Git diff에서도 의존성 추가/제거가 명확히 드러난다.

```csharp
// ✅ 권장
public SomeClass(
    SomeService someService,
    AnotherService anotherService)
{
    _someService = someService;
    _anotherService = anotherService;
}

// ❌ 지양
public SomeClass(SomeService someService, AnotherService anotherService)
{
    ...
}
```

### bool 비교

`bool` 조건은 `== true` / `== false`로 명시 비교한다.

```csharp
// ✅ 권장
if (IsGrounded == true)
{
    Jump();
}

// ❌ 지양
if (IsGrounded)
{
    Jump();
}
```

### 제어 구문

모든 제어 구문은 반드시 중괄호 사용, 한 줄 표기 금지

```csharp
// ✅ 권장
if (condition == true)
{
    return;
}

// ❌ 지양
if (condition == true) return;
```

### 비동기 메서드 명명 규칙

**Awaitable 메서드 (`UniTask`, `Task`)**
- 반드시 이름 뒤에 **Async**를 붙인다.
- 호출하는 쪽에서 `await`가 가능함을 명시한다.

**Fire-and-Forget 메서드 (`UniTaskVoid`, `void`)**
- 이름 뒤에 **Async를 붙이지 않는다.**
- 이벤트 핸들러나 시작점(Root) 역할을 하므로, 일반 메서드처럼 명명한다.

```csharp
// ✅ 권장
// Await 가능
public async UniTask LoadDataAsync()
{
    await ...
}

// Await 불가능 (Fire-and-Forget)
public async UniTaskVoid StartGameSequence()
{
    await ...
}
```

## 📝 주석

주석은 한국어로 쓴다. 어휘는 [`Comments/vocabulary.json`](Comments/vocabulary.json)을 따른다.

### 작성 기준

summary는 자명하지 않을 때 쓴다. 이름과 시그니처만으로 역할을 알 수 있으면 자명하다.
아래는 자명해도 쓴다.

* 클래스 (상수 그룹 제외)
* 인터페이스 구현 메서드 (Lifecycle 인터페이스 제외): `I인터페이스.메서드 Implementation.`

```csharp
/// <summary>
/// IInteractable.OnPlayerContact Implementation.
/// </summary>
public void OnPlayerContact(IPlayer player)
{ }
```

필드에는 `//`가 아니라 `/// <summary>`를 쓴다.
backing field에 `[Tooltip]`이 있으면 프로퍼티 summary는 생략한다.

```csharp
// ✅ [Tooltip]이 있으므로 프로퍼티 summary 생략
#region Properties
public float Speed => _speed;
#endregion


#region Private Variables
[Tooltip("이동 속도 (m/s)")]
[SerializeField] private float _speed = 5f;
#endregion
```

### 문체

첫 문장은 역할을 쓴다. 이름과 시그니처로 알 수 없는 것은 보조 문장으로 덧붙인다.

```csharp
/// <summary>
/// 지정한 방향으로 이동합니다.
/// 땅에 닿아 있지 않으면 속도를 절반으로 줄입니다.
/// </summary>
public void Move(Vector2 direction)
{ }
```

| 형식 | 첫 문장 | 마침표 | 예시 |
|---|---|---|---|
| summary: 메서드 | 합니다체 | 찍는다 | `지정한 방향으로 이동합니다.` |
| summary: 타입·프로퍼티·필드 | 명사형 | 찍는다 | `플레이어의 이동을 처리하는 클래스.` |
| `//`, Tooltip | 명사형 | 안 찍는다 | `땅에 닿았을 때만 점프 허용` |

인라인 `//`는 실행문 바로 위에 쓰고, "무엇"이 아니라 "왜"를 쓴다.

```csharp
// ✅ 권장
// 땅에 닿았을 때만 점프 허용
if (IsGrounded == true)
{
    Jump();
}

// ❌ 지양
if (IsGrounded == true) // 땅에 닿았을 때만 점프 허용
{
    Jump();
}
```

### 어휘

* 코드를 가리킬 때는 그 이름을 그대로 쓴다 ("생성된 적들"이 아니라 "생성된 Enemy들")
* 서술어는 한자어를 권장한다. 구어체는 쓰지 않는다

## 🧪 테스트

### 명명

- 테스트 클래스는 `대상클래스Tests`로 명명한다
- 테스트 메서드는 `대상_조건_기대결과`로 명명한다. 조건이 없으면 `대상_기대결과`
- 실제 의존 객체를 대신하는 클래스(테스트 더블)는 역할에 맞는 접두사를 붙인다
  - `Fake` — 실제로 동작하는 간이 구현
  - `Stub` — 미리 정한 값을 반환
  - `Spy` — 호출을 기록
  - `Mock` — 상호작용을 검증

### 작성

- 테스트 메서드의 summary는 검증하는 규칙을 간결히 서술한다

## 📋 체크리스트

### 새 클래스 작성시

- [ ]  Class summary 작성
- [ ]  Region 순서 준수
- [ ]  접근 제한자별 정렬 (public → internal → protected → private)
- [ ]  어트리뷰트 변수 상단 배치
- [ ]  빈 함수 한 줄 표기
- [ ]  Unity Lifecycle 순서 준수
- [ ]  인터페이스 구현 메서드에 Summary 작성 (I인터페이스명.메서드명 Implementation. / Lifecycle 인터페이스 제외 가능)
- [ ]  비동기 메서드 명명 규칙 준수 (Async 접미사 여부)

### 코드 리뷰시

- [ ]  Region convention 준수 여부
- [ ]  변수 정렬 순서 확인
- [ ]  Class summary 존재 여부
- [ ]  인터페이스 구현 Summary 형식 확인 (I인터페이스명.메서드명 Implementation.)
- [ ]  주석 용어 표기 일관성 (한 용어 = 한 표기)
- [ ]  코드 스타일 일관성
