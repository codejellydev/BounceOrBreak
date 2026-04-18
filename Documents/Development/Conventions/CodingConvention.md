# Unity C# 네이밍 컨벤션

## 📋 Region 구조

### Region 순서 (필수)

```csharp
#region Nested Classes          // 필요시
#region Injection               // VContainer 의존성 주입 시
#region Events
#region Properties
#region Public Variables
#region Protected Variables
#region Private Variables
#region VContainer Lifecycle    // VContainer 생명주기 인터페이스 구현 시
#region Unity Lifecycle
#region Public Methods
#region Protected Methods
#region Private Methods
#region Input Action Callbacks  // 필요시
#region Debug                   // 필요시
```

### Region 내부 정렬 규칙

### 1. Region 간격

Region과 Region 사이에는 **두 줄 띄어쓰기**

### 2. 접근 제한자 순서

각 region 내에서는 **public → protected → private** 순으로 정렬

```csharp
#region Events
// public event들
public event Action OnSomething;
public event Action<int> OnValueChanged;

// protected event들 (있다면)
protected event Action OnProtectedEvent;

// private event들 (있다면)
private event Action OnPrivateEvent;
#endregion

#region Properties
// public 프로퍼티들
public int Health { get; set; }
public Transform Target => _target;

// protected 프로퍼티들 (있다면)
protected bool IsInitialized { get; set; }

// private 프로퍼티들 (있다면)
private float PrivateValue { get; set; }
#endregion
```

### 2. Variables에서 어트리뷰트 우선순위

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

### 3. 어트리뷰트 표기법

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
{}

public void Initialize()
{}

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
{}

private void Start()
{}

private void OnEnable()
{}

private void Update()
{}

private void FixedUpdate()
{}

private void LateUpdate()
{}

private void OnDisable()
{}

private void OnDestroy()
{}
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

## 📝 주석 및 문서화

### Class Summary (필수)

모든 클래스에 반드시 summary 작성

```csharp
/// <summary>
/// 플레이어의 이동을 담당하는 컨트롤러 클래스.
/// 입력에 따른 이동, 점프, 회전 등의 기능을 제공한다.
/// </summary>
public class PlayerController : MonoBehaviour
{
}

```

### Interface Summary

**인터페이스 정의:** 복잡하거나 중요한 인터페이스에만 작성 (선택)

```csharp
/// <summary>
/// 상호작용 가능한 객체들에 대한 인터페이스.
/// 플레이어와의 접촉 및 상호작용 로직을 정의한다.
/// </summary>
public interface IInteractable
{
    bool IsInteractable { get; }
    void OnPlayerContact(IPlayer player);
}

```

**인터페이스 구현:** 모든 인터페이스 메서드 구현체에 작성 (필수)
```csharp
/// <summary>
/// IInteractable.OnPlayerContact Implementation
/// </summary>
public void OnPlayerContact(IPlayer player)
{
    // 구현 로직
}
```

**예외: Lifecycle 성격의 인터페이스는 Summary 생략 가능**

Unity Lifecycle(`Awake`, `Start` 등)과 VContainer Lifecycle(`IStartable.Start`, `IAsyncStartable.StartAsync` 등)은 프레임워크가 정의한 잘 알려진 계약이므로 Summary를 생략 가능하다.

### Properties Summary (선택)

복잡한 로직이나 중요한 프로퍼티에만 작성

```csharp
#region Nested Classes
[System.Serializable]
public class InputProfile
{
    public InputContext Context;
    public bool Player;
    public bool UI;
    public bool System;
}
#endregion

#region Properties
/// <summary>
/// 현재 플레이어가 땅에 닿아있는지 여부.
/// GroundCheck를 통해 매 프레임 업데이트됨.
/// </summary>
public bool IsGrounded => _isGrounded;

// 간단한 프로퍼티는 summary 생략 가능
public float Speed { get; set; }
#endregion

```

### 주석 위치

제어 구문 주석은 구문 바로 위에 작성

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

## 📋 체크리스트

### 새 클래스 작성시

- [ ]  Class summary 작성
- [ ]  Region 순서 준수
- [ ]  접근 제한자별 정렬 (public → protected → private)
- [ ]  어트리뷰트 변수 상단 배치
- [ ]  빈 함수 한 줄 표기
- [ ]  Unity Lifecycle 순서 준수
- [ ]  인터페이스 구현 메서드에 Summary 작성 (I인터페이스명.메서드명 Implementation / Lifecycle 인터페이스 제외 가능)
- [ ]  비동기 메서드 명명 규칙 준수 (Async 접미사 여부)

### 코드 리뷰시

- [ ]  Region convention 준수 여부
- [ ]  변수 정렬 순서 확인
- [ ]  Class summary 존재 여부
- [ ]  인터페이스 구현 Summary 형식 확인 (I인터페이스명.메서드명 Implementation)
- [ ]  코드 스타일 일관성