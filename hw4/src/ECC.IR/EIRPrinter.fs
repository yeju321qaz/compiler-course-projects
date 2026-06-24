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

module ECC.IR.EIRPrinter

open System.Text

// This file is written by Claude Code, so it could have subtle bugs. If you
// find any, please report to the Front-Desk repository.

// Print one statement per line following the notation in spec §6. The body
// of each function is indented two spaces between `start f` and `end f`, while
// label declarations are pulled back to column 0 so they stand out as visual
// landmarks. Functions are separated by a blank line.

// ---------------------------------------------------------------------------
// Operator / label rendering
// ---------------------------------------------------------------------------

let private binOpStr = function
    | BAdd -> "+"  | BSub -> "-"  | BMul -> "*"  | BDiv -> "/"
    | BAnd -> "&&" | BOr  -> "||"

let private relOpStr = function
    | Req -> "==" | Rne -> "!="
    | Rlt -> "<"  | Rle -> "<="
    | Rgt -> ">"  | Rge -> ">="

let private unOpStr = function
    | UNeg -> "-"
    | UNot -> "!"

let private labelStr = function
    | LblFunc f  -> f
    | LblLocal l -> l

// Same char-escape rule used by ASTPrinter (matches the source forms listed
// in spec §2.3).
let private escapeChar = function
    | '\n'   -> "\\n"
    | '\000' -> "\\0"
    | '\t'   -> "\\t"
    | '\''   -> "\\'"
    | c      -> string c

// ---------------------------------------------------------------------------
// EIRExpr rendering
// IRTranslator only places leaves (constants or variables) at operator
// positions, but the EIRExpr type itself permits arbitrary nesting, so we
// recurse anyway. The output for a flat 3-address form is unambiguous, so we
// don't emit parentheses.
// ---------------------------------------------------------------------------

let rec private exprStr (e: EIRExpr) : string =
    match e with
    | ExprInt n            -> string n
    | ExprChar c           -> sprintf "'%s'" (escapeChar c)
    | ExprVar x            -> x
    | ExprUnOp (op, a)     -> sprintf "%s %s" (unOpStr op) (exprStr a)
    | ExprBinOp (op, a, b) ->
        sprintf "%s %s %s" (exprStr a) (binOpStr op) (exprStr b)
    | ExprRelOp (op, a, b) ->
        sprintf "%s %s %s" (exprStr a) (relOpStr op) (exprStr b)
    | ExprLoad (arr, idx)  -> sprintf "%s[%s]" arr (exprStr idx)

// ---------------------------------------------------------------------------
// Single-statement rendering (no indentation). Uses the notation from
// spec §6 verbatim.
// ---------------------------------------------------------------------------

let private stmtStr (s: EIRStmt) : string =
    match s with
    | StmtAssign (x, e)     -> sprintf "%s := %s" x (exprStr e)
    | StmtStore (x, idx, v) ->
        sprintf "%s[%s] := %s" x (exprStr idx) (exprStr v)
    | StmtLabel lbl         -> sprintf "label %s:" (labelStr lbl)
    | StmtJump lbl          -> sprintf "goto %s" (labelStr lbl)
    | StmtCJump (v, lT, lF) ->
        sprintf "if %s then goto %s else goto %s" v (labelStr lT) (labelStr lF)
    | StmtFuncStart lbl     -> sprintf "start %s" (labelStr lbl)
    | StmtFuncEnd lbl       -> sprintf "end %s" (labelStr lbl)
    | StmtParam x           -> sprintf "param %s" x
    | StmtCall (lbl, n)     -> sprintf "call %s, %d" (labelStr lbl) n
    | StmtRetrieve x        -> sprintf "retrieve %s" x
    | StmtReturn None       -> "return"
    | StmtReturn (Some x)   -> sprintf "return %s" x

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

/// Render a single expression as a string.
let prettyPrintExpr (e: EIRExpr) : string =
    exprStr e

/// Render a single statement as a single-line string with no indentation.
let prettyPrintStmt (s: EIRStmt) : string =
    stmtStr s

/// Render an entire EIR program in a human-readable form.
/// Layout conventions:
///   * `start f` / `end f` sit at column 0.
///   * Ordinary statements inside a function body are indented two spaces.
///   * Label declarations (`label L:`) are pulled back to column 0 so they
///     act as visual landmarks within a function.
///   * Functions are separated by a blank line.
let prettyPrint (prog: EIR) : string =
    if List.isEmpty prog then "(empty)"
    else
        let sb = StringBuilder ()
        // Carry (inFunc, firstFunc) through a fold so layout state stays
        // immutable. `inFunc` says whether we're currently between a
        // StmtFuncStart and its matching StmtFuncEnd; `firstFunc` suppresses
        // the blank line separator before the very first function.
        let appendStmt (inFunc, firstFunc) s =
            match s with
            | StmtFuncStart _ ->
                if not firstFunc then sb.AppendLine () |> ignore
                sb.AppendLine (stmtStr s) |> ignore
                true, false
            | StmtFuncEnd _ ->
                sb.AppendLine (stmtStr s) |> ignore
                false, firstFunc
            | StmtLabel _ ->
                // Labels stay at column 0 whether inside or outside a function.
                sb.AppendLine (stmtStr s) |> ignore
                inFunc, firstFunc
            | _ when inFunc ->
                sb.Append("  ").AppendLine (stmtStr s) |> ignore
                inFunc, firstFunc
            | _ ->
                // Statements outside any function are unusual but we render
                // them at column 0 as a fallback.
                sb.AppendLine (stmtStr s) |> ignore
                inFunc, firstFunc
        prog |> List.fold appendStmt (false, true) |> ignore
        sb.ToString().TrimEnd ()
