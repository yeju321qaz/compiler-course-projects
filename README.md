# Compiler Course Projects

컴파일러 수업에서 수행한 프로젝트를 정리한 저장소입니다.

F# 기반으로 **Ewha C-- Compiler (ECC)** 의 핵심 구성 요소를 단계적으로 구현하며,
프로그래밍 언어가 소스 코드를 분석하고 중간 표현(Intermediate Representation)으로 변환하는 전체 컴파일 과정에 대해 학습했습니다.

---

## Project Overview

본 프로젝트에서는 간단한 C 계열 언어인 **C--** 를 대상으로 컴파일러의 주요 단계를 구현했습니다.

구현 과정에서 다음과 같은 컴파일러 파이프라인을 다뤘습니다.

- Lexical Analysis (Lexer)
- Syntax Analysis (Parser)
- Semantic Analysis (Type Checking)
- Intermediate Representation Translation (IR Translation)

이를 통해 source code가 내부 표현으로 변환되는 과정을 직접 구현하고 디버깅했습니다.

---

## Assignments

### HW1 — F# Functional Programming Foundations
F# 문법 및 함수형 프로그래밍 패턴에 익숙해지기 위한 기초 과제입니다.

주요 내용:
- Recursive function 구현
- Custom data structure 처리
- Pattern matching 활용
- Immutable data manipulation

---

### HW2 — Lexer Implementation
입력 문자열을 토큰(Token) 시퀀스로 변환하는 Lexer를 구현했습니다.

구현 내용:
- Keyword / Identifier 인식
- Integer literal 처리
- Operator 및 delimiter tokenization
- Invalid token detection
- Lexing error location reporting

사용 기술:
- FsLex

---

### HW3 — Parser & AST Construction
토큰 리스트를 Abstract Syntax Tree(AST)로 변환하는 Parser를 구현했습니다.

구현 내용:
- Grammar rule 설계
- Operator precedence 처리
- Ambiguous grammar 해결 (dangling else)
- AST generation

사용 기술:
- FsYacc

---

### HW4 — Semantic Analysis & IR Translation
AST를 기반으로 semantic validation 및 IR 변환을 구현했습니다.

구현 내용:
- Type checking
- Undeclared identifier detection
- Semantic error localization
- AST → EIR translation
- Intermediate representation generation

핵심 구현 파일:
- `TypeChecker.fs`
- `IRTranslator.fs`

---

## Tech Stack

- F#
- .NET
- FsLex
- FsYacc

---

## Key Learnings

이 프로젝트를 통해 다음 역량을 강화했습니다.

- Formal grammar 이해
- Compiler architecture 이해
- Lexer / Parser 동작 원리 이해
- Static semantic validation
- Intermediate representation design
- Debugging language-processing pipelines

---

## Notes

본 저장소는 수업에서 제공된 skeleton code를 기반으로, 과제에서 요구된 컴파일러 핵심 구성 요소를 구현한 내용을 정리한 것입니다.
