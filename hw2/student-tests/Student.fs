(*
  ECC - Ewha C-- Compiler

  Copyright (c) SWEETS Lab. @ Ewha Womans University, since 2026

  Permission is hereby granted, free of charge, to any person obtaining a copy of
  this software and associated documentation files (the "Software"), to deal in
  the Software without restriction, including without limitation the rights to
  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
  of the Software, and to permit persons to whom the Software is furnished to do
  so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*)

module ECC.FrontEnd.Tests.Student

open Xunit
open ECC.Core
open ECC.FrontEnd

let tok value line column =
    { Value = value; Line = line; Column = column }

[<Fact; Trait("TestSet", "student")>]
let ``basic function`` () =
    let input = "int main(void) { return 0; }"
    let output = Ok [
        tok TokInt 1 0
        tok (TokID "main") 1 4
        tok TokLParen 1 8
        tok TokVoid 1 9
        tok TokRParen 1 13
        tok TokLBrace 1 15
        tok TokReturn 1 17
        tok (TokIntLit "0") 1 24
        tok TokSemicolon 1 25
        tok TokRBrace 1 27
    ]
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``control flow`` () =
    let input = "if(a!=0){return 1;}else{return 2;}"
    let output = Ok [
        tok TokIf 1 0
        tok TokLParen 1 2
        tok (TokID "a") 1 3
        tok TokNeq 1 4
        tok (TokIntLit "0") 1 6
        tok TokRParen 1 7
        tok TokLBrace 1 8
        tok TokReturn 1 9
        tok (TokIntLit "1") 1 16
        tok TokSemicolon 1 17
        tok TokRBrace 1 18
        tok TokElse 1 19
        tok TokLBrace 1 23
        tok TokReturn 1 24
        tok (TokIntLit "2") 1 31
        tok TokSemicolon 1 32
        tok TokRBrace 1 33
    ]
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``loops and assignment`` () =
    let input = "while(x<=10){x=x+1;}for(i=0;i<3;i=i+1){}"
    let output = Ok [
        tok TokWhile 1 0
        tok TokLParen 1 5
        tok (TokID "x") 1 6
        tok TokLeq 1 7
        tok (TokIntLit "10") 1 9
        tok TokRParen 1 11
        tok TokLBrace 1 12
        tok (TokID "x") 1 13
        tok TokAssign 1 14
        tok (TokID "x") 1 15
        tok TokPlus 1 16
        tok (TokIntLit "1") 1 17
        tok TokSemicolon 1 18
        tok TokRBrace 1 19
        tok TokFor 1 20
        tok TokLParen 1 23
        tok (TokID "i") 1 24
        tok TokAssign 1 25
        tok (TokIntLit "0") 1 26
        tok TokSemicolon 1 27
        tok (TokID "i") 1 28
        tok TokLt 1 29
        tok (TokIntLit "3") 1 30
        tok TokSemicolon 1 31
        tok (TokID "i") 1 32
        tok TokAssign 1 33
        tok (TokID "i") 1 34
        tok TokPlus 1 35
        tok (TokIntLit "1") 1 36
        tok TokRParen 1 37
        tok TokLBrace 1 38
        tok TokRBrace 1 39
    ]
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``operators and delimiters`` () =
    let input = "a&&b||!c,d[2]/e-f*g>=h"
    let output = Ok [
        tok (TokID "a") 1 0
        tok TokAnd 1 1
        tok (TokID "b") 1 3
        tok TokOr 1 4
        tok TokNot 1 6
        tok (TokID "c") 1 7
        tok TokComma 1 8
        tok (TokID "d") 1 9
        tok TokLBracket 1 10
        tok (TokIntLit "2") 1 11
        tok TokRBracket 1 12
        tok TokSlash 1 13
        tok (TokID "e") 1 14
        tok TokMinus 1 15
        tok (TokID "f") 1 16
        tok TokStar 1 17
        tok (TokID "g") 1 18
        tok TokGeq 1 19
        tok (TokID "h") 1 21
    ]
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``char and string literals`` () =
    let input = "char c='a'; char* s=\"hi\";"
    let output = Ok [
        tok TokChar 1 0
        tok (TokID "c") 1 5
        tok TokAssign 1 6
        tok (TokCharLit "'a'") 1 7
        tok TokSemicolon 1 10
        tok TokChar 1 12
        tok TokStar 1 16
        tok (TokID "s") 1 18
        tok TokAssign 1 19
        tok (TokStringLit "\"hi\"") 1 20
        tok TokSemicolon 1 24
    ]
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``comments are skipped`` () =
    let input = "// hello
int x; /* block comment */ return x;"
    let output = Ok [
        tok TokInt 2 0
        tok (TokID "x") 2 4
        tok TokSemicolon 2 5
        tok TokReturn 2 27
        tok (TokID "x") 2 34
        tok TokSemicolon 2 35
    ]
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``invalid character returns error`` () =
    let input = "@"
    let output = Error (1, 0)
    Assert.Equal(output, EntryPoints.lexString input)

[<Fact; Trait("TestSet", "student")>]
let ``unterminated block comment returns error`` () =
    let input = "/* hello"
    let output = Error (1, 8)
    Assert.Equal(output, EntryPoints.lexString input)
