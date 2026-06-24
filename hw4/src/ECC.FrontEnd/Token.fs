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

namespace ECC.FrontEnd

type Token =
    /// Literals
    | TokIntLit of string
    | TokCharLit of string
    | TokStringLit of string
    | TokID of string
    /// Keywords - Types
    | TokInt
    | TokChar
    | TokVoid
    /// Keywords - Control Flow
    | TokIf
    | TokElse
    | TokWhile
    | TokFor
    | TokReturn
    /// Operators
    | TokPlus
    | TokMinus
    | TokStar
    | TokSlash
    | TokEq
    | TokNeq
    | TokLt
    | TokLeq
    | TokGt
    | TokGeq
    | TokAnd
    | TokOr
    | TokNot
    | TokAssign
    /// Delimiters
    | TokLParen
    | TokRParen
    | TokLBrace
    | TokRBrace
    | TokLBracket
    | TokRBracket
    | TokSemicolon
    | TokComma
    /// Special
    | TokEOF
with
    override this.ToString () =
        match this with
        | TokIntLit s -> sprintf "INTLIT(\"%s\")" s
        | TokCharLit s -> sprintf "CHARLIT(\"%s\")" s
        | TokStringLit s -> sprintf "STRINGLIT(\"%s\")" s
        | TokID s -> sprintf "ID(\"%s\")" s
        | TokInt -> "INT"
        | TokChar -> "CHAR"
        | TokVoid -> "VOID"
        | TokIf -> "IF"
        | TokElse -> "ELSE"
        | TokWhile -> "WHILE"
        | TokFor -> "FOR"
        | TokReturn -> "RETURN"
        | TokPlus -> "PLUS"
        | TokMinus -> "MINUS"
        | TokStar -> "STAR"
        | TokSlash -> "SLASH"
        | TokEq -> "EQ"
        | TokNeq -> "NEQ"
        | TokLt -> "LT"
        | TokLeq -> "LEQ"
        | TokGt -> "GT"
        | TokGeq -> "GEQ"
        | TokAnd -> "AND"
        | TokOr -> "OR"
        | TokNot -> "NOT"
        | TokAssign -> "ASSIGN"
        | TokLParen -> "LPAREN"
        | TokRParen -> "RPAREN"
        | TokLBrace -> "LBRACE"
        | TokRBrace -> "RBRACE"
        | TokLBracket -> "LBRACKET"
        | TokRBracket -> "RBRACKET"
        | TokSemicolon -> "SEMICOLON"
        | TokComma -> "COMMA"
        | TokEOF -> "EOF"