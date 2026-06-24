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

namespace ECC.IR

type EIRVar = string

type EIRLabel =
    | LblFunc of string
    | LblLocal of string

type EIRUnOp =
    | UNeg
    | UNot

type EIRBinOp =
    | BAdd
    | BSub
    | BMul
    | BDiv
    | BAnd
    | BOr

type EIRRelOp =
    | Rlt
    | Rle
    | Req
    | Rne
    | Rge
    | Rgt

type EIRExpr =
    | ExprInt of int
    | ExprChar of char
    | ExprVar of EIRVar
    | ExprUnOp of EIRUnOp * EIRExpr
    | ExprBinOp of EIRBinOp * EIRExpr * EIRExpr
    | ExprRelOp of EIRRelOp * EIRExpr * EIRExpr
    | ExprLoad of EIRVar * EIRExpr

type EIRStmt =
    | StmtAssign of EIRVar * EIRExpr
    | StmtStore of EIRVar * EIRExpr * EIRExpr
    | StmtLabel of EIRLabel
    | StmtJump of EIRLabel
    | StmtCJump of EIRVar * EIRLabel * EIRLabel
    | StmtFuncStart of EIRLabel
    | StmtFuncEnd of EIRLabel
    | StmtParam of EIRVar
    | StmtCall of EIRLabel * int
    | StmtRetrieve of EIRVar
    | StmtReturn of EIRVar option

type EIR = EIRStmt list
