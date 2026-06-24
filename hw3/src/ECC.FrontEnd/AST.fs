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

open ECC.Core

// Every AST node kind comes as a pair: a bare form `CmmX` and an abbreviation
// `LCmmX = Location<CmmX>`. Fields that reference another node always use the
// L-prefixed (Location-wrapped) form, so every node — including operator/type
// tags (`CmmBinOp`, `CmmType`, etc.) — carries its own source position.
// Payload scalars (int, char, string, identifier names) stay unwrapped; their
// position is covered by the enclosing Location<_>.
//
// Design note: this means a few unit productions end up with two positions
// that point at the same text — e.g. `CmmProgram` wraps each `CmmDecl` with a
// `Location`, and `CmmDeclVar` internally wraps its `CmmVarDecl` again. The
// redundancy is intentional: it keeps the rule "every node has its own
// Location" uniform and easy to teach, with no "except when wrapped by X"
// carve-outs. For such unit productions the parser must set the inner and
// outer positions consistently (normally both point at the full span of the
// production), and passes should prefer the outer Location when reporting
// errors about the whole declaration/statement/assignment.

/// <type> ::= <non-void-type> | 'void'
/// <non-void-type> ::= 'int' | 'char'
type CmmType =
    | CmmTyVoid
    | CmmTyInt
    | CmmTyChar

type LCmmType = Location<CmmType>

/// <unop> ::= '-' | '!'
type CmmUnOp =
    | CmmUNeg
    | CmmUNot

type LCmmUnOp = Location<CmmUnOp>

/// <binop> ::= '+' | '-' | '*' | '/' | '&&' | '||'
type CmmBinOp =
    | CmmBAdd
    | CmmBSub
    | CmmBMul
    | CmmBDiv
    | CmmBAnd
    | CmmBOr

type LCmmBinOp = Location<CmmBinOp>

/// <relop> ::= '==' | '!=' | '<' | '<=' | '>' | '>='
type CmmRelOp =
    | CmmREq
    | CmmRNeq
    | CmmRLt
    | CmmRLeq
    | CmmRGt
    | CmmRGeq

type LCmmRelOp = Location<CmmRelOp>

/// <expr>
type CmmExpr =
    | CmmExpIntLit of int
    | CmmExpCharLit of char
    | CmmExpStrLit of string
    | CmmExpVar of string
    | CmmExpArrAccess of string * LCmmExpr
    | CmmExpCall of string * LCmmExpr list
    | CmmExpUnOp of LCmmUnOp * LCmmExpr
    | CmmExpBinOp of LCmmBinOp * LCmmExpr * LCmmExpr
    | CmmExpRelOp of LCmmRelOp * LCmmExpr * LCmmExpr

and LCmmExpr = Location<CmmExpr>

/// <assign> ::= '[id]' '=' <expr>
///            | '[id]' '[' <expr> ']' '=' <expr>
type CmmAssign =
    | CmmAssignVar of string * LCmmExpr
    | CmmAssignArr of string * LCmmExpr * LCmmExpr

type LCmmAssign = Location<CmmAssign>

/// <var-name> ::= '[id]' | '[id]' '[' '[intlit]' ']'
type CmmVarName =
    | CmmVarScalar of string
    | CmmVarArray of string * int

type LCmmVarName = Location<CmmVarName>

/// <var-decl> ::= <non-void-type> <var-name-list> ';'
/// Type is restricted to non-void (int or char).
type CmmVarDecl = {
    Type: LCmmType
    Names: LCmmVarName list
}

type LCmmVarDecl = Location<CmmVarDecl>

/// <param-decl> ::= <type> '[id]' | <type> '[id]' '[' '[intlit]' ']'
type CmmParam =
    | CmmParamScalar of LCmmType * string
    | CmmParamArray of LCmmType * string * int

type LCmmParam = Location<CmmParam>

/// <block-stmt> ::= '{' <var-decl-list> <stmt-list> '}'
/// Reused as the body of a function and as a statement.
type CmmBlock = {
    Decls: LCmmVarDecl list
    Stmts: LCmmStmt list
}

and LCmmBlock = Location<CmmBlock>

/// <stmt>
and CmmStmt =
    | CmmStmtIf of LCmmExpr * LCmmStmt * LCmmStmt option
    | CmmStmtWhile of LCmmExpr * LCmmStmt
    | CmmStmtFor of LCmmAssign option
            * LCmmExpr option
            * LCmmAssign option
            * LCmmStmt
    | CmmStmtReturn of LCmmExpr option
    | CmmStmtAssign of LCmmAssign
    | CmmStmtCall of string * LCmmExpr list
    | CmmStmtBlock of LCmmBlock
    | CmmStmtEmpty

and LCmmStmt = Location<CmmStmt>

/// <func-decl> ::= <type> '[id]' '(' <param-decl-list> ')' <block-stmt>
/// An empty Params list represents the 'void' parameter list.
type CmmFuncDecl = {
    RetType: LCmmType
    Name: string
    Params: LCmmParam list
    Body: LCmmBlock
}

type LCmmFuncDecl = Location<CmmFuncDecl>

/// Top-level declaration: either a global variable or a function.
type CmmDecl =
    | CmmDeclVar of LCmmVarDecl
    | CmmDeclFunc of LCmmFuncDecl

type LCmmDecl = Location<CmmDecl>

/// <prog> is a sequence of global variable / function declarations.
type CmmProgram = LCmmDecl list