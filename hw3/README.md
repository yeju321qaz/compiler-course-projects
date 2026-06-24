[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/FhuoMomY)
# HW3: ECC - Parser의 구현

## 과제 안내

이 과제는 ECC의 두 번째 파트인 Parser의 구현에 대한 과제입니다.

C-- 언어의 Specification은 사이버캠퍼스에서 확인할 수 있습니다.

여러분들의 목표는 `src/ECC.FrontEnd/Parser.fsy` 파일을 완성하여, 토큰의 리스트를 AST로
변환하는 작업을 수행하는 것입니다.

* 주의: 반드시 `src/ECC.FrontEnd/Parser.fsy` 파일만 수정해야 합니다.

### 참고 사항

- C-- 언어의 문법은 if-else 문법에 대하여 모호한 부분이 존재합니다. 이 과제에서는 else가
가장 가까운 if에 붙는 것으로 해석하겠습니다. 예를 들어, 아래와 같은 코드가 있다고 할 때,
```c
if (cond1)
if (cond2) stmt1;
else stmt2;
```
이 코드는 아래와 같이 해석됩니다.
```c
if (cond1) {
  if (cond2) {
    stmt1;
  } else {
    stmt2;
  }
}
```

### 참고 자료

- [FsYacc 공식 문서](https://fsprojects.github.io/FsLexYacc/content/fsyacc.html)
- [AST 타입 정의](src/ECC.FrontEnd/AST.fs)

## 프로젝트 구조

```
hw3-[Your GitHub ID]/
  HW3.slnx                       -- 솔루션 파일
  src/
    ECC.Core/                    -- 핵심 유틸리티 라이브러리
    ECC.Driver/                  -- 컴파일러 드라이버 (EntryPoint)
    ECC.FrontEnd/                -- 프론트엔드
      ECC.FrontEnd.fsproj
      AST.fs                     -- AST 타입 정의
      ASTPrinter.fs              -- AST를 사람이 읽을 수 있는 형태로 출력하는 유틸리티
      EntryPoints.fs             -- 프론트엔드 EntryPoint
      Error.fs                   -- 프론트엔드에서 발생하는 에러 타입 정의
      Lexer.fsl                  -- Lexer 규칙 정의
      Parser.fsy                 -- ★ Parser 규칙 정의 (수정 대상)
      Token.fs                   -- 토큰 타입 정의
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
제공하여, 여러분이 구현한 Parser가 제대로 작동하는지 확인할 수 있습니다. 현재는 구현이
완성되지 않았기 때문에, 실행 시 아래와 같은 예외가 발생할 것입니다.
```
Input: int main(void) { return 0; }

Parsing failed at line 1, column 0.
```

하지만 `Parser.fsy`를 완성한 후에는, `test.cmm` 파일이 성공적으로 파싱되어 출력될
것입니다. 아래는 예상되는 출력입니다.
```
Input: int main(void) { return 0; }

Parsing successful. The AST will be printed below.
========
Program
  [1:0] DeclFunc
    [1:0] FuncDecl main : [1:0] int
      params: (none)
      body:
        [1:15] Block
          decls: (none)
          stmts:
            [1:17] StmtReturn
              [1:24] IntLit 0
```

## 채점

채점 방식은 HW1과 동일하므로 추가적인 설명은 생략합니다. student 테스트 케이스를 만들 때,
반드시 [tests/ECC.FrontEnd.Tests/Public.fs](tests/ECC.FrontEnd.Tests/Public.fs)
파일처럼 테스트 케이스를 string type으로 작성하고,
ECC.FrontEnd.EntryPoints.parseString 함수를 이용하여 파싱한 결과를 검증하는 방식으로
작성해주세요.

* 주의: `src/ECC.FrontEnd/Parser.fsy` 파일 외의 다른 파일을 수정시 감점