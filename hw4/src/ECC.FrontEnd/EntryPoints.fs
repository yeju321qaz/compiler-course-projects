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

module ECC.FrontEnd.EntryPoints

open FSharp.Text.Lexing
open System.IO

// smk: Fix for HW3
let rec lexAll acc lexbuf =
    try
        match Lexer.tokenize lexbuf with
        | TokEOF -> Ok (List.rev acc)
        | token -> lexAll (token :: acc) lexbuf
    with
        | LexError (line, col) -> Error (line, col)

let lexString src =
    printfn "Input: %s" src
    let lexbuf = LexBuffer<char>.FromString src
    lexbuf.EndPos <- { pos_bol = 0; pos_fname = "<input>"; pos_cnum = 0; pos_lnum = 1; pos_orig_lnum = 1 }
    lexAll [] lexbuf

let lexFile filePath =
    let src = File.ReadAllText filePath
    printfn "Input: %s" src
    let lexbuf = LexBuffer<char>.FromString src
    lexbuf.EndPos <- { pos_bol = 0; pos_fname = filePath; pos_cnum = 0; pos_lnum = 1; pos_orig_lnum = 1 }
    lexAll [] lexbuf

// Maps our hand-written Token DU (see Token.fs) to the FsYacc-generated
// Parser.token DU (see the %token declarations in Parser.fsy). The two
// DUs are kept in 1:1 correspondence, so this is a pure relabelling.
let toParserToken = function
    // Literals
    // Parse the intlit lexeme into an int at the lex/parse boundary. The
    // lexer's INTLIT regex already guarantees a valid base-10 form, so the
    // only failure mode here is overflow — which surfaces as a runtime
    // exception to the driver, rather than silently wrapping.
    | TokIntLit s     -> Parser.INTLIT (int s)
    | TokCharLit s    -> Parser.CHARLIT s
    | TokStringLit s  -> Parser.STRINGLIT s
    | TokID s         -> Parser.ID s
    // Type keywords
    | TokInt          -> Parser.INT
    | TokChar         -> Parser.CHAR
    | TokVoid         -> Parser.VOID
    // Control flow
    | TokIf           -> Parser.IF
    | TokElse         -> Parser.ELSE
    | TokWhile        -> Parser.WHILE
    | TokFor          -> Parser.FOR
    | TokReturn       -> Parser.RETURN
    // Operators
    | TokPlus         -> Parser.PLUS
    | TokMinus        -> Parser.MINUS
    | TokStar         -> Parser.STAR
    | TokSlash        -> Parser.SLASH
    | TokEq           -> Parser.EQ
    | TokNeq          -> Parser.NEQ
    | TokLt           -> Parser.LT
    | TokLeq          -> Parser.LEQ
    | TokGt           -> Parser.GT
    | TokGeq          -> Parser.GEQ
    | TokAnd          -> Parser.AND
    | TokOr           -> Parser.OR
    | TokNot          -> Parser.NOT
    | TokAssign       -> Parser.ASSIGN
    // Delimiters
    | TokLParen       -> Parser.LPAREN
    | TokRParen       -> Parser.RPAREN
    | TokLBrace       -> Parser.LBRACE
    | TokRBrace       -> Parser.RBRACE
    | TokLBracket     -> Parser.LBRACKET
    | TokRBracket     -> Parser.RBRACKET
    | TokSemicolon    -> Parser.SEMICOLON
    | TokComma        -> Parser.COMMA
    // End of input
    | TokEOF          -> Parser.EOF

let parseString src =
    let lexbuf = LexBuffer<char>.FromString src
    lexbuf.EndPos <- { pos_bol = 0; pos_fname = "<input>"; pos_cnum = 0; pos_lnum = 1; pos_orig_lnum = 1 }
    try
        let ast = Parser.start (Lexer.tokenize >> toParserToken) lexbuf
        Ok ast
    with
    | LexError (line, col) -> Error (line, col)
    | ParseError ->
        let p = lexbuf.StartPos
        Error (p.pos_lnum, p.pos_cnum - p.pos_bol)

let parseFile filePath =
    let src = File.ReadAllText filePath
    printfn "Input: %s" src
    let lexbuf = LexBuffer<char>.FromString src
    lexbuf.EndPos <- { pos_bol = 0; pos_fname = filePath; pos_cnum = 0; pos_lnum = 1; pos_orig_lnum = 1 }
    try
        let ast = Parser.start (Lexer.tokenize >> toParserToken) lexbuf
        Ok ast
    with
    | LexError (line, col) -> Error (line, col)
    | ParseError ->
        let p = lexbuf.StartPos
        Error (p.pos_lnum, p.pos_cnum - p.pos_bol)

let analyzeSemantics ast =
    Result.bind TypeChecker.check ast

let analyzeSemanticsString src =
    parseString src
    |> analyzeSemantics

let analyzeSemanticsFile filePath =
    parseFile filePath
    |> analyzeSemantics

let translateToIR ast =
    Result.map IRTranslator.translate ast

let runString src =
    parseString src
    |> analyzeSemantics
    |> translateToIR

let runFile filePath =
    parseFile filePath
    |> analyzeSemantics
    |> translateToIR
