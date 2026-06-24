[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/bE87wbTO)
# HW2: ECC - Lexer의 구현

## 과제 안내

앞으로 여러분들은 여러 개의 과제에 걸쳐 ECC (Ewha C-- Compiler)라는 컴파일러를 개발하게
될 예정입니다. 이 과제는 ECC의 첫 번째 파트인 Lexer의 구현에 대한 과제입니다.

C-- 언어의 Specification은 사이버캠퍼스에서 확인할 수 있습니다.

여러분들의 목표는 `src/ECC.FrontEnd/Lexer.fsl` 파일의 `tokenize` 함수를 완성하여,
입력된 문자열을 토큰의 리스트로 변환하는 작업을 수행하는 것입니다.

* 주의: 반드시 `src/ECC.FrontEnd/Lexer.fsl` 파일만 수정해야 합니다.
* 주의: tokenize 함수의 시그니처를 변경하는 것도 허용되지 않습니다.

### 참고 자료

- [FsLex 공식 문서](https://fsprojects.github.io/FsLexYacc/content/fslex.html)
- [Token 타입 정의](src/ECC.FrontEnd/Token.fs)

## 프로젝트 구조

```
hw2-[Your GitHub ID]/
  HW2.slnx                       -- 솔루션 파일
  src/
    ECC.Core/                    -- 핵심 유틸리티 라이브러리
    ECC.Driver/                  -- 컴파일러 드라이버 (EntryPoint)
    ECC.FrontEnd/                -- 프론트엔드
      ECC.FrontEnd.fsproj
      EntryPoints.fs             -- 프론트엔드 EntryPoint
      Lexer.fsl                  -- ★ Lexer 규칙 정의 (수정 대상)
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

구현하면서 실제로 실행해볼 수 있는 컴파일러 Driver도 함께 제공됩니니다. `examples/`
디렉토리에 예제 입력 파일인 `test.cmm`이 포함되어 있습니다. 이 파일을 컴파일러에 입력으로
제공하여, 여러분이 구현한 Lexer가 제대로 작동하는지 확인할 수 있습니다. 현재는 `tokenize`
함수가 완성되지 않았기 때문에, 실행 시 아래와 같은 예외가 발생할 것입니다.
```
$ dotnet run --project src/ECC.Driver/ -- examples/test.cmm
Unhandled exception. System.Exception: TODO
... (stack trace 생략) ...
```

하지만 `tokenize` 함수를 완성한 후에는, `test.cmm` 파일이 성공적으로 토큰화되어 출력될
것입니다. 아래는 예상되는 출력입니다.
```
$ dotnet run --project src/ECC.Driver/ -- examples/test.cmm
Lexing successful. Tokens will be printed below, one per line.
========
INT at line 1, column 0
ID("main") at line 1, column 4
LPAREN at line 1, column 8
VOID at line 1, column 9
RPAREN at line 1, column 13
LBRACE at line 1, column 15
RETURN at line 1, column 17
INTLIT("0") at line 1, column 24
SEMICOLON at line 1, column 25
RBRACE at line 1, column 27
```

### Lexing 에러 출력

Front Desk의 [13번](https://github.com/Ewha-34811/2026s-Front-Desk/issues/13),
[14번](https://github.com/Ewha-34811/2026s-Front-Desk/issues/14) 이슈에서
논의된 바와 같이, Lexing에 실패하였을 때 출력해야 하는 에러의 위치는 ``Lexing에
실패한 위치``를 이야기합니다.

예: 아래와 같은 입력을 갖는 (단, 맨 마지막에 newline 없음) 파일을 입력으로
주었을 때 tokenize함수는 `Error (3, 37)`을 리턴해야 합니다.
```
/* this is comment
int hehe
file ended without ending the comment
```

## 채점

채점 방식은 HW1과 동일하므로 추가적인 설명은 생략합니다. student 테스트 케이스를 만들 때,
반드시 [tests/ECC.FrontEnd.Tests/Public.fs](tests/ECC.FrontEnd.Tests/Public.fs)
파일처럼 테스트 케이스를 string type으로 작성하고, ECC.FrontEnd.EntryPoints.lexString
함수를 이용하여 토큰화한 결과를 검증하는 방식으로 작성해주세요.

* 주의: `src/ECC.FrontEnd/Lexer.fsl` 파일 외의 다른 파일을 수정시 감점
* 주의: tokenize 함수의 시그니처를 변경시 감점
