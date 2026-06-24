[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/ROqbLUgi)
# HW1: F#과 친해지기

## 과제 안내

각 `src/ProbXX/ProbXX.fs` 파일에서 `failwith "Not implemented"` 부분을 올바른
구현으로 교체하세요. 함수 시그니처는 변경하면 안됩니다.

---

<details>
<summary>### 문제 1: 온도 변환 (`src/Prob01/Prob01.fs`)</summary>

섭씨 온도를 화씨 온도로 변환하는 함수 `celsiusToFahrenheit`를 작성하세요.

변환 공식: **F = C * 9.0 / 5.0 + 32.0**

| 입력 | 출력 |
|------|------|
| `0.0` | `32.0` |

**추가:** 입력은 항상 올바른 범위 안에서 주어집니다 (C >= -273.15).

</details>

---

<details>
<summary>### 문제 2: 절대값 함수 (`src/Prob02/Prob02.fs`)</summary>

정수를 받아 절대값을 반환하는 함수 `abs`를 작성하세요. `System.Math.Abs`를 사용하면
안 됩니다.

| 입력 | 출력 |
|------|------|
| `5` | `5` |

</details>

---

<details>
<summary>### 문제 3: 가위바위보 (`src/Prob03/Prob03.fs`)</summary>

두 플레이어의 선택을 받아 결과를 반환하는 함수를 작성하세요. 튜플 패턴 매칭을 활용합니다.

- 이기면 `"p1"`, 지면 `"p2"`, 비기면 `"draw"`를 반환합니다.
- 유효하지 않은 입력이면 `"invalid"`를 반환합니다.

| `p1` | `p2` | 출력 |
|------|------|------|
| `"rock"` | `"scissors"` | `"p1"` |

</details>

---

<details>
<summary>### 문제 4: 간단한 계산기 (`src/Prob04/Prob04.fs`)</summary>

두 수와 연산자 문자열을 받아 계산 결과를 반환하는 함수를 작성하세요.

- 지원 연산자: `"+"`, `"-"`, `"*"`, `"/"`
- 0으로 나누는 경우 또는 지원하지 않는 연산자인 경우 `None`을 반환합니다.

| `op` | `a` | `b` | 출력 |
|------|-----|-----|------|
| `"+"` | `3` | `5` | `Some 8` |

</details>

---

<details>
<summary>### 문제 5: 이항 계수 (`src/Prob05/Prob05.fs`)</summary>

재귀를 사용하여 이항 계수 C(n, k)를 구하는 함수를 작성하세요. 다음 점화식을 활용합니다.

- C(n, 0) = 1
- C(n, n) = 1
- C(n, k) = C(n-1, k-1) + C(n-1, k)

| `n` | `k` | 출력 |
|-----|-----|------|
| `5` | `2` | `10` |

**추가:** 입력은 항상 올바른 범위 안에서 주어집니다 (n >= 0, 0 <= k <= n).

</details>

---

<details>
<summary>### 문제 6: 이진 트리 (`src/Prob06/Prob06.fs`)</summary>

다음 이진 트리 타입에 대해 트리의 깊이(depth)를 구하는 함수를 작성하세요. 빈 트리의 깊이는
0입니다.

```fsharp
type Tree<'T> =
    | Leaf
    | Node of 'T * Tree<'T> * Tree<'T>
```

| 입력 | 출력 |
|------|------|
| `Node (1, Leaf, Leaf)` | `1` |

</details>

---

<details>
<summary>### 문제 7: 나만의 리스트 만들기 (`src/Prob07/Prob07.fs`)</summary>

F#의 `list` 타입을 사용하지 않고, Discriminated Union으로 직접 리스트를 구현합니다.
아래의 타입 정의를 공통으로 사용합니다.

```fsharp
type MyList<'T> =
    | Empty
    | Cons of 'T * MyList<'T>

// 예시: [1; 2; 3] 은 다음과 같이 표현한다
// Cons (1, Cons (2, Cons (3, Empty)))
```

#### 기본 연산

`MyList`에 대해 길이를 구하는 함수와 F#의 리스트로 변환하는 함수를 작성하세요.

| 식 | 결과 |
|----|------|
| `length Empty` | `0` |
| `toFSharpList Empty` | `[]` |
| `fromFSharpList []` | `Empty` |

#### append와 reverse

두 `MyList`를 이어붙이는 함수와 뒤집는 함수를 작성하세요.

| 식 | 결과 |
|----|------|
| `append Empty Empty` | `Empty` |
| `reverse Empty` | `Empty` |

#### fold

`MyList`에 대한 `fold` 함수를 작성하세요. F#의 `List.fold`와 동일하게 동작합니다.

| 식 | 결과 |
|----|------|
| `fold (fun acc x -> acc + x) 0 (Cons (1, Empty))` | `1` |

#### fold를 활용한 map

문제 7c에서 작성한 `fold`를 사용하여 `map` 함수를 작성하세요.
**직접 재귀를 사용하면 안 됩니다.**

| 식 | 결과 |
|----|------|
| `map (fun x -> x * 2) (Cons (1, Empty))` | `(Cons (2, Empty))` |

**힌트:** `fold`와 `reverse`를 조합해 보세요.

**추가:** 필요하다면 Prob07.fs 내에서 함수의 정의 순서를 바꿔도 됩니다.

</details>

## 프로젝트 구조

```
src/
  Prob01/Prob01.fs     -- 문제 1
  Prob02/Prob02.fs     -- 문제 2
  Prob03/Prob03.fs     -- 문제 3
  Prob04/Prob04.fs     -- 문제 4
  Prob05/Prob05.fs     -- 문제 5
  Prob06/Prob06.fs     -- 문제 6
  Prob07/Prob07.fs     -- 문제 7
  Launcher/Program.fs  -- 직접 실행시켜 보는 용도의 코드. 채점에 영향 X
tests/
  ProbXX.Tests/
    Public.fs          -- 공개 테스트
    Student.fs         -- 학생 테스트
```

## 빌드 및 테스트

```bash
# 전체 빌드
dotnet build

# 런쳐 실행 방법
dotnet run --project src/Launcher

# 특정 문제 테스트 실행
dotnet test tests/Prob01.Tests

# 공개 테스트만 실행
dotnet test tests/Prob01.Tests --filter "TestSet=public"

# 학생 테스트만 실행
dotnet test tests/Prob01.Tests --filter "TestSet=student"
```

## 채점

### 채점 기준

채점은 총 3가지 종류의 테스트 케이스 셋을 이용하여 진행됩니다.

(1) public: 과제와 함께 제공되는 테스트 케이스입니다. 아주 간단한 테스트 케이스로만
    구성되어 있습니다.

(2) student: 여러분들이 직접 만들어야 하는 테스트 케이스입니다. 과제 제출 시에 student
    테스트 케이스의 코드 커버리지가 75%를 넘겨야 점수를 받을 수 있습니다. 모든 학생들의
    student 테스트 케이스를 모아서 채점에 사용할 예정입니다.

(3) private: 채점용 비공개 테스트 케이스입니다. 과제 claim 시에 통과하지 못한 test
    case를 확인할 수 있으나, 공개되지 않습니다.

### 채점 환경/스크립트

채점은 자동화된 스크립트를 통해 이루어집니다. 채점 스크립트와 동일한 스크립트가 GitHub
Actions CI에서도 등록되어 있기 때문에, 본인의 점수를 대략적으로 예측할 수 있습니다.

#### 채점 스크립트 파일

```
grading/
  run-ci.sh       -- GitHub Actions CI에서 실행되는 채점 스크립트
  run-local.sh    -- 로컬에서 채점 스크립트를 실행하는 스크립트 (Linux/macOS/WSL)
  run-local.cmd   -- 로컬에서 채점 스크립트를 실행하는 스크립트 (Windows cmd)
  ...
```

#### 채점 스크립트 실행 방법

* 로컬 실행

  (1) Linux/macOS/WSL: 터미널에서 과제 루트 디렉토리 (hw1-[github id])로 이동한 뒤,
      `grading/run-local.sh` 스크립트를 실행합니다.

  (2) Windows: 파워쉘에서 과제 루트 디렉토리 (hw1-[github id])로 이동한 뒤,
      `grading/run-local.cmd` 스크립트를 실행합니다.

  (3) 스크립트가 실행되면 화면에 테스트 실행 결과가 출력되며 `TestResults` 디렉토리가
      생성되고, `TestResults/CoverageReport` 디렉토리에는 코드 커버리지에 대한 정보가
      생성되게 됩니다.

* GitHub에서 실행

  (1) main 브랜치에 푸시를 하거나, 다른 브랜치에서 main 브랜치에 pull request를 생성하면
      GitHub Actions CI가 자동으로 채점 스크립트를 실행합니다.

  (2) 조금 기다리면 Actions 탭에서 실행 결과를 확인할 수 있습니다.

  (3) GitHub에서 실행할 때에는 코드 커버리지에 대한 리포트가 생성되지 않습니다.

  (4) **주의** GitHub Actions CI의 사용량은 한정적이기 때문에, 채점 스크립트를
      GitHub에서 너무 자주 실행하지 않도록 주의하세요.

* 권장 개발 방법

  (1) 새로운 브랜치를 만들어서 작업한다.

  (2) 로컬에서 테스트를 돌려보며 확인한다.

  (3) 개발이 완료되면, main 브랜치에 머지한다.
