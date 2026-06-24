[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/QINmlfsy)
# HW4: ECC - Semantic Analyzer 및 IR Translator의 구현

## 과제 안내

이 과제는 ECC의 세 번째 파트인 Semantic Analyzer 및 IR Translator의 구현에 대한
과제입니다.

C-- 언어의 Specification은 사이버캠퍼스에서 확인할 수 있습니다.

여러분들의 목표는 두 가지입니다:
1. Type Checker 구현: AST를 입력으로 받아서, 각 노드의 타입을 체크하는 기능을 구현해야
   합니다. 타입 에러가 발생하는 경우, 에러 위치를 리턴할 수 있어야 합니다.
2. IR Translator 구현: 타입 체크가 완료된 AST를 입력으로 받아서, EIR로 변환하는 기능을
   구현해야 합니다. IR 변환 과정에서 에러는 발생하지 않는다고 가정합니다.

여러분들의 목표는 `src/ECC.FrontEnd/TypeChecker.fs`,
`src/ECC.FrontEnd/IRTranslator.fs` 파일을 완성하여, AST의 Semantic Analysis와 IR
Translation을 성공적으로 수행하는 것입니다.

* 주의: 반드시 `src/ECC.FrontEnd/TypeChecker.fs`와
  `src/ECC.FrontEnd/IRTranslator.fs` 파일만 수정해야 합니다.

### 참고 사항

- 타입 체크 수행 시 Undeclared Identifier 에러도 함께 체크할 수 있어야 합니니다.

- 타입 체크를 수행하면서 에러가 발생하는 경우, 해당 타입 에러와 직접적으로 관련된 AST 노드의
  위치 정보를 알려줄 수 있어야 합니다. 예를 들어, `42 + 'b'`와 같은 표현에서 integer
  literal `42`와 character literal `'b'`가 서로 더해질 수 없기 때문에 타입 에러가
  발생하는데, 이 때 `42 + 'b'` 표현식의 위치 정보를 에러 메시지에 포함시켜야 합니다.

### 참고 자료

- [AST 타입 정의](src/ECC.FrontEnd/AST.fs)
- [EIR 타입 정의](src/ECC.IR/EIR.fs)

## 프로젝트 구조

```
hw4-[Your GitHub ID]/
  HW4.slnx                       -- 솔루션 파일
  src/
    ECC.Core/                    -- 핵심 유틸리티 라이브러리
    ECC.Driver/                  -- 컴파일러 드라이버 (EntryPoint)
    ECC.FrontEnd/                -- 프론트엔드
      ECC.FrontEnd.fsproj
      AST.fs                     -- AST 타입 정의
      ASTPrinter.fs              -- AST를 사람이 읽을 수 있는 형태로 출력하는 유틸리티
      EntryPoints.fs             -- 프론트엔드 EntryPoint
      Error.fs                   -- 프론트엔드에서 발생하는 에러 타입 정의
      IRTranslator.fs            -- ★ IRTranslator 구현 (수정 대상)
      Lexer.fsl                  -- Lexer 규칙 정의
      Parser.fsy                 -- Parser 규칙 정의
      Token.fs                   -- 토큰 타입 정의
      TypeChecker.fs             -- ★ TypeChecker 구현 (수정 대상)
    ECC.IR/                      -- 중간 표현(IR) 관련 코드
      EIR.fs                     -- IR 타입 정의
      EIRPrinter.fs              -- IR을 사람이 읽을 수 있는 형태로 출력하는 유틸리티
  tests/
    ECC.FrontEnd.Tests/          -- FrontEnd 테스트
      ECC.FrontEnd.Tests.fsproj
      Public.fs                  -- 공개 테스트
      Student.fs                 -- 학생 작성 테스트
  examples/
    test.cmm                     -- 예제 입력 파일
```

## 빌드 및 테스트

```
# 전체 빌드
dotnet build

# 컴파일러 실행 방법
dotnet run --project src/ECC.Driver/

# 테스트 실행
dotnet test tests/ECC.FrontEnd.Tests

# 공개 테스트만 실행
dotnet test tests/ECC.FrontEnd.Tests --filter "TestSet=public"

# 학생 테스트만 실행
dotnet test tests/ECC.FrontEnd.Tests --filter "TestSet=student"
```

### ECC 컴파일러 실행 예시

구현하면서 실제로 실행해볼 수 있는 컴파일러 Driver도 함께 제공됩니다. `examples/`
디렉토리에 예제 입력 파일인 `test.cmm`이 포함되어 있습니다. 이 파일을 컴파일러에 입력으로
제공하여, 여러분이 구현한 TypeChecker와 IRTranslator가 제대로 작동하는지 확인할 수
있습니다. 현재는 구현이 완성되지 않았기 때문에, 실행 시 아래와 같이 빈 EIR이 출력될 것입니다.
```
Input: int main(void) { return 0; }

IR translation successful. The EIR will be printed below.
========
(empty)
```

하지만 `TypeChecker.fs`와 `IRTranslator.fs`를 완성한 후에는, `test.cmm` 파일이
성공적으로 IR로 변환되어 출력될 것입니다. 아래는 예상되는 출력입니다.
```
Input: int main(void) { return 0; }

IR translation successful. The EIR will be printed below.
========
start main
  t1 := 0
  return t1
end main
```

## 채점

채점 방식은 HW1과 동일하므로 추가적인 설명은 생략합니다. student 테스트 케이스를 만들 때,
반드시 [tests/ECC.FrontEnd.Tests/Public.fs](tests/ECC.FrontEnd.Tests/Public.fs)
파일처럼 테스트 케이스를 string type으로 작성하고,
ECC.FrontEnd.EntryPoints.parseString 함수를 이용하여 파싱한 결과를 검증하는 방식으로
작성해주세요.

* 주의: `src/ECC.FrontEnd/TypeChecker.fs`, `src/ECC.FrontEnd/IRTranslator.fs`
파일 외의 다른 파일을 수정시 감점