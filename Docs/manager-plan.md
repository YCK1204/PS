# 매니저 계층 계획

*2026-08-26*

## 왜 지금인가

94파일 5,979줄. 코어(격자 → 단어 → 전투)가 한 바퀴 돌았고, 여기서 살을 더 붙이기 전에 흩어진 것을 모은다.

측정한 실제 부채:

| 증상 | 숫자 |
|---|---|
| `Instantiate`/`Destroy` 산재 | 6곳 — 잔상(대시당 7) · 회오리(타격당) · 데미지숫자(피격당) · 얼음(결빙당) |
| `Keyboard.current` 직접 읽기 | 5개 파일 |
| 어셈블리 | 1개(`Assembly-CSharp`) — ThirdParty 포함 전체 재컴파일 |
| `Resources.Load` | 2곳(테이블만) |

---

## 0. asmdef 분리 — 선행 작업

나머지 전부의 토대. 매니저를 만들면서 어셈블리를 나누는 게 나중에 나누는 것보다 훨씬 싸다.

```
PS.Core      GameSettings · GameAction · SceneRouter · 매니저 뼈대
PS.Game      Inventory 모델 · Combat · Actors · Words   (PS.Core 참조)
PS.Data      SO · 테이블                                 (PS.Core 참조)
PS.UI        UIPanel · UIStack · 위젯 · 패널             (PS.Core, PS.Game 참조)
PS.Editor    에디터 전용                                  (전부 참조, 빌드 제외)
```

**의존은 한 방향** — `PS.Game`은 `PS.UI`를 모른다. 지금 지키고 있는 규칙을 어셈블리로 강제한다.

### 걸림돌

asmdef를 만든 어셈블리는 **`Assembly-CSharp`을 참조할 수 없다.** ThirdParty 중 asmdef가 없는 것을 쓰면 컴파일이 깨진다.

| 쓰는 것 | 상태 | 처리 |
|---|---|---|
| `DamageNumbersPro` | asmdef 없음 | asmdef 추가 필요 |
| `TextMeshPro` · `InputSystem` · `URP` | 패키지(asmdef 있음) | 참조만 추가 |
| `Feel` · `DOTween` | 아직 안 씀 | 쓸 때 처리 |

### 확인 방법
컴파일 클린 + 플레이 모드에서 격자·전투 동작.

---

## 1. PoolManager

근거가 가장 확실하다. 짧은 수명 + 고빈도 객체가 이미 넷.

### 대상

| 객체 | 발생 빈도 | 현재 |
|---|---|---|
| `SpriteGhost` | 대시당 4~8개 | `new GameObject` → `Destroy` |
| `Projectile`(회오리) | 타격당 1개 | `Instantiate` → `Destroy` |
| `DamageNumber` | 피격당 1개 | DNP 자체 풀 있음 (제외) |
| `FreezeVisual` | 결빙당 1개 | `Instantiate` → `Destroy` |

### 모양

```csharp
PoolManager.Get<T>(T prefab, Vector3 pos, Quaternion rot) → T
PoolManager.Release(Component instance)
PoolManager.Prewarm<T>(T prefab, int count)
```

- 프리팹 인스턴스 ID를 키로 풀을 나눈다
- 반납은 `SetActive(false)` + 풀 루트로 부모 이동
- `IPoolable { void OnGet(); void OnRelease(); }` — 상태 초기화가 필요한 것만 구현

### 호출부 변화
`SpriteGhost.Spawn` 내부만 고치면 `DashTrail`은 안 건드린다. `Projectile`·`FreezeVisual`도 생성 지점 한 줄씩.

### 주의
- 반납 시 **구독 해제**를 빼먹으면 이벤트가 중복 발화한다
- `Destroy` 대신 `Release`로 바꿀 때 **이미 반납된 것을 또 반납**하지 않게 플래그
- 씬 전환 시 풀 비우기

### 확인 방법
대시 20회 후 `SpriteGhost` 총 인스턴스 수가 최대 동시 수를 안 넘는지.

---

## 2. InputManager

기획에 컨트롤러 지원이 있는데 지금 구조로는 못 붙인다. 키보드를 5곳이 각자 읽는다.

### 지금

| 파일 | 읽는 것 |
|---|---|
| `PlayerController` | 이동 · 점프 · 대시 · 공격 |
| `GameMenu` | 인벤토리 키 |
| `UIStack` | ESC |
| `KeyBindRow` | 재설정 캡처 |
| `GameSettings` | 표시 이름 |

### 모양

```csharp
InputManager.Move            // -1 ~ 1
InputManager.JumpPressed     // 이번 프레임
InputManager.AttackPressed
InputManager.DashPressed
InputManager.InventoryPressed
InputManager.CancelPressed   // ESC
```

- `GameSettings`의 바인딩을 읽어 **행동 단위**로 노출한다
- 소스는 키보드 하나로 시작. 컨트롤러는 나중에 소스만 추가
- `KeyBindRow`의 캡처는 그대로 둔다(재설정은 예외)

### 주의
- 지금 `UIStack`이 ESC를 독점하는 구조를 깨지 말 것. `CancelPressed`는 `UIStack`이 먼저 먹고 남으면 게임플레이로
- UI가 열려 있을 때 게임플레이 입력 차단은 이미 `PlayerController.Blocked`로 됨 — 이걸 `InputManager`가 알게 할지 결정 필요

### 확인 방법
키 재설정 후 즉시 반영 · UI 열림 중 조작 차단.

---

## 3. ResourceManager

근거는 약하다(`Resources.Load` 2곳). 다만 **경로 문자열이 흩어지기 전에** 모아두면 나중에 Addressables 전환이 싸다.

### 모양

```csharp
ResourceManager.Load<T>(string key) → T      // 캐시
ResourceManager.LoadAll<T>(string folder) → T[]
ResourceManager.Clear()
```

- 경로 상수를 한 곳에 (`Data/Words`, `Data/Letters`, `Prefabs/...`)
- 내부는 지금은 `Resources`, 나중에 Addressables로 갈아끼움

### 주의
`Resources/` 폴더는 **빌드에 전부 포함되고 언로드가 안 된다.** 지금 크기면 문제없지만 아트가 들어오면 옮겨야 한다.

---

## 4. UIManager

**기존과 겹친다.** `UIStack`(스택·ESC) + `GameMenu`(여닫기) + `UIPanel`(개별 화면)이 이미 그 일을 한다.

만든다면 하는 일은 하나 — **패널을 키로 열기.**

```csharp
UIManager.Open(UIKey.Inventory)
UIManager.Close(UIKey.Settings)
UIManager.IsOpen(UIKey.Inventory)
```

지금은 `GameMenu`가 인스펙터로 패널 참조를 들고 있다. 화면이 늘면 그 참조가 늘어난다. 레지스트리로 바꾸면 씬마다 다시 꽂지 않아도 된다.

- `UIStack`은 **그대로 둔다** — 스택·ESC 규칙은 이미 검증됨
- `UIManager`는 그 위에 등록/조회만 얹는다
- 안 하면 `GameMenu`가 계속 늘어나는 것뿐이라 급하지 않다

---

## 5. SceneManager (SceneRouter 확장)

`SceneRouter`가 이미 있고 타이틀 ↔ 전투 왕복이 확인됐다.

확장이 필요해지는 시점은 **③ 보상 루프**다. 그때 필요한 것:

- 씬 전환 중 **런 데이터 유지** (지금은 씬과 함께 사라짐)
- 비동기 로딩 + 전환 연출
- 전환 전후 훅(`OnSceneWillChange` / `OnSceneReady`)

지금 만들면 쓸 데가 없다. **GameManager와 같이 하는 게 맞다.**

---

## 6. GameManager

`RunState`가 런 1회분의 소유자다. 그 위 계층이 필요해지는 건:

- **게임 상태** — 타이틀 / 전투 / 정비 / 결과
- **런 데이터 보존** — 씬을 넘어도 격자·글자·글리프가 유지
- **일시정지** — 기획상 전투 중엔 인벤토리를 못 켠다. 그 규칙을 강제할 주체

지금은 씬이 둘뿐이고 유지할 데이터가 없어서 **빈 껍데기가 된다.**

---

## 순서

```
0. asmdef 분리          토대. 나머지 전부가 여기 얹힘
1. PoolManager          근거 확실. 호출부 변화 최소
2. InputManager         컨트롤러 대비 + 5곳 분산 해소
3. ResourceManager      경로만 모으는 얇은 층
4. UIManager            GameMenu 참조가 늘어나기 시작하면
5. SceneManager + GameManager   ③ 보상 루프와 함께
```

4·5는 **쓸 데가 생겼을 때** 만든다. 지금 만들면 기존 코드를 감싸기만 하는 층이 생기고, 이름은 있는데 하는 일이 없는 상태가 제일 나쁘다.

---

## 원칙

- 매니저는 **싱글턴을 강제하지 않는다.** `PoolManager`·`ResourceManager`는 정적 유틸로 충분하고, 씬 오브젝트가 필요한 것만 인스턴스로
- 만들 때마다 **기존 호출부를 실제로 옮긴다.** 새 API만 만들고 옛 코드를 남기면 두 경로가 공존해서 더 나빠짐
- 각 단계마다 **컴파일 + 플레이 모드 확인**. 매니저 도입은 조용히 깨지는 종류의 작업임
