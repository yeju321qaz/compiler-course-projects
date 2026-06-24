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

module ECC.FrontEnd.ASTPrinter

open System.Text
open ECC.Core

// This file is written by Claude Code, so it could have subtle bugs. If you
// find any, please report to the Front-Desk repository.

// ---------------------------------------------------------------------------
// Small helpers
// ---------------------------------------------------------------------------

let private loc (l: Location<_>) = sprintf "[%d:%d]" l.Line l.Column

let private typeStr = function
    | CmmTyVoid -> "void"
    | CmmTyInt  -> "int"
    | CmmTyChar -> "char"

let private unopStr = function
    | CmmUNeg -> "-"
    | CmmUNot -> "!"

let private binopStr = function
    | CmmBAdd -> "+"  | CmmBSub -> "-"  | CmmBMul -> "*" | CmmBDiv -> "/"
    | CmmBAnd -> "&&" | CmmBOr  -> "||"

let private relopStr = function
    | CmmREq  -> "==" | CmmRNeq -> "!="
    | CmmRLt  -> "<"  | CmmRLeq -> "<="
    | CmmRGt  -> ">"  | CmmRGeq -> ">="

// Render char/string literals so the output is unambiguous and (roughly)
// matches the C-- source form that produced them.
let private escapeChar = function
    | '\n'   -> "\\n"
    | '\000' -> "\\0"
    | '\t'   -> "\\t"
    | '\''   -> "\\'"
    | c      -> string c

let private escapeStringContent (s: string) =
    let sb = StringBuilder ()
    for c in s do
        match c with
        | '\n'   -> sb.Append "\\n"  |> ignore
        | '\000' -> sb.Append "\\0"  |> ignore
        | '\t'   -> sb.Append "\\t"  |> ignore
        | '"'    -> sb.Append "\\\"" |> ignore
        | c      -> sb.Append c      |> ignore
    sb.ToString ()

// ---------------------------------------------------------------------------
// Core writer: everything builds up a single StringBuilder, indented by depth.
// ---------------------------------------------------------------------------

let private write (sb: StringBuilder) (depth: int) (line: string) =
    sb.Append(String.replicate depth "  ").AppendLine(line) |> ignore

let private writeOpt sb depth label printer opt =
    write sb depth (label + ":")
    match opt with
    | Some v -> printer sb (depth + 1) v
    | None   -> write sb (depth + 1) "(none)"

let private writeList sb depth label printer (items: _ list) =
    if List.isEmpty items then
        write sb depth (label + ": (none)")
    else
        write sb depth (label + ":")
        items |> List.iter (printer sb (depth + 1))

// ---------------------------------------------------------------------------
// Expressions
// ---------------------------------------------------------------------------

let rec private pExpr sb depth (e: LCmmExpr) =
    let head s = write sb depth (sprintf "%s %s" (loc e) s)
    match e.Value with
    | CmmExpIntLit n ->
        head (sprintf "IntLit %d" n)
    | CmmExpCharLit c ->
        head (sprintf "CharLit '%s'" (escapeChar c))
    | CmmExpStrLit s ->
        head (sprintf "StrLit \"%s\"" (escapeStringContent s))
    | CmmExpVar name ->
        head (sprintf "Var %s" name)
    | CmmExpArrAccess (name, idx) ->
        head (sprintf "ArrAccess %s" name)
        write sb (depth + 1) "index:"
        pExpr sb (depth + 2) idx
    | CmmExpCall (name, args) ->
        head (sprintf "Call %s" name)
        writeList sb (depth + 1) "args" pExpr args
    | CmmExpUnOp (op, arg) ->
        head (sprintf "UnOp %s %s" (loc op) (unopStr op.Value))
        pExpr sb (depth + 1) arg
    | CmmExpBinOp (op, l, r) ->
        head (sprintf "BinOp %s %s" (loc op) (binopStr op.Value))
        pExpr sb (depth + 1) l
        pExpr sb (depth + 1) r
    | CmmExpRelOp (op, l, r) ->
        head (sprintf "RelOp %s %s" (loc op) (relopStr op.Value))
        pExpr sb (depth + 1) l
        pExpr sb (depth + 1) r

// ---------------------------------------------------------------------------
// Assignments
// ---------------------------------------------------------------------------

let private pAssign sb depth (a: LCmmAssign) =
    let head s = write sb depth (sprintf "%s %s" (loc a) s)
    match a.Value with
    | CmmAssignVar (name, e) ->
        head (sprintf "AssignVar %s" name)
        pExpr sb (depth + 1) e
    | CmmAssignArr (name, idx, e) ->
        head (sprintf "AssignArr %s" name)
        write sb (depth + 1) "index:"
        pExpr sb (depth + 2) idx
        write sb (depth + 1) "value:"
        pExpr sb (depth + 2) e

// ---------------------------------------------------------------------------
// Declarations: var-name, var-decl, param
// ---------------------------------------------------------------------------

let private pVarName sb depth (vn: LCmmVarName) =
    match vn.Value with
    | CmmVarScalar name ->
        write sb depth (sprintf "%s %s (scalar)" (loc vn) name)
    | CmmVarArray (name, size) ->
        write sb depth (sprintf "%s %s[%d] (array)" (loc vn) name size)

let private pVarDecl sb depth (vd: LCmmVarDecl) =
    let ty = vd.Value.Type
    write sb depth
        (sprintf "%s VarDecl : %s %s" (loc vd) (loc ty) (typeStr ty.Value))
    writeList sb (depth + 1) "names" pVarName vd.Value.Names

let private pParam sb depth (p: LCmmParam) =
    match p.Value with
    | CmmParamScalar (ty, name) ->
        write sb depth
            (sprintf "%s Param %s : %s %s"
                     (loc p) name (loc ty) (typeStr ty.Value))
    | CmmParamArray (ty, name, size) ->
        write sb depth
            (sprintf "%s Param %s[%d] : %s %s"
                     (loc p) name size (loc ty) (typeStr ty.Value))

// ---------------------------------------------------------------------------
// Statements (mutually recursive with blocks)
// ---------------------------------------------------------------------------

let rec private pStmt sb depth (s: LCmmStmt) =
    let head ss = write sb depth (sprintf "%s %s" (loc s) ss)
    match s.Value with
    | CmmStmtEmpty ->
        head "StmtEmpty"
    | CmmStmtReturn None ->
        head "StmtReturn (void)"
    | CmmStmtReturn (Some e) ->
        head "StmtReturn"
        pExpr sb (depth + 1) e
    | CmmStmtAssign a ->
        head "StmtAssign"
        pAssign sb (depth + 1) a
    | CmmStmtCall (name, args) ->
        head (sprintf "StmtCall %s" name)
        writeList sb (depth + 1) "args" pExpr args
    | CmmStmtIf (cond, thn, els) ->
        head "StmtIf"
        write sb (depth + 1) "cond:"
        pExpr sb (depth + 2) cond
        write sb (depth + 1) "then:"
        pStmt sb (depth + 2) thn
        writeOpt sb (depth + 1) "else" pStmt els
    | CmmStmtWhile (cond, body) ->
        head "StmtWhile"
        write sb (depth + 1) "cond:"
        pExpr sb (depth + 2) cond
        write sb (depth + 1) "body:"
        pStmt sb (depth + 2) body
    | CmmStmtFor (init, cond, upd, body) ->
        head "StmtFor"
        writeOpt sb (depth + 1) "init"   pAssign init
        writeOpt sb (depth + 1) "cond"   pExpr   cond
        writeOpt sb (depth + 1) "update" pAssign upd
        write sb (depth + 1) "body:"
        pStmt sb (depth + 2) body
    | CmmStmtBlock b ->
        head "StmtBlock"
        pBlock sb (depth + 1) b

and private pBlock sb depth (b: LCmmBlock) =
    write sb depth (sprintf "%s Block" (loc b))
    writeList sb (depth + 1) "decls" pVarDecl b.Value.Decls
    writeList sb (depth + 1) "stmts" pStmt    b.Value.Stmts

// ---------------------------------------------------------------------------
// Function decl / top-level decl / program
// ---------------------------------------------------------------------------

let private pFuncDecl sb depth (fd: LCmmFuncDecl) =
    let v = fd.Value
    write sb depth
        (sprintf "%s FuncDecl %s : %s %s"
                 (loc fd) v.Name (loc v.RetType) (typeStr v.RetType.Value))
    writeList sb (depth + 1) "params" pParam v.Params
    write sb (depth + 1) "body:"
    pBlock sb (depth + 2) v.Body

let private pDecl sb depth (d: LCmmDecl) =
    match d.Value with
    | CmmDeclVar vd ->
        write sb depth (sprintf "%s DeclVar" (loc d))
        pVarDecl sb (depth + 1) vd
    | CmmDeclFunc fd ->
        write sb depth (sprintf "%s DeclFunc" (loc d))
        pFuncDecl sb (depth + 1) fd

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

/// Pretty-print a full program. Top-level node is always "Program"; each
/// declaration underneath carries a `[line:col]` prefix.
let prettyPrint (prog: CmmProgram) : string =
    let sb = StringBuilder ()
    if List.isEmpty prog then
        sb.AppendLine "Program (empty)" |> ignore
    else
        sb.AppendLine "Program" |> ignore
        prog |> List.iter (pDecl sb 1)
    sb.ToString().TrimEnd ()

/// Pretty-print a single expression subtree. Handy for unit-testing parser
/// actions in isolation.
let prettyPrintExpr (e: LCmmExpr) : string =
    let sb = StringBuilder ()
    pExpr sb 0 e
    sb.ToString().TrimEnd ()

/// Pretty-print a single statement subtree.
let prettyPrintStmt (s: LCmmStmt) : string =
    let sb = StringBuilder ()
    pStmt sb 0 s
    sb.ToString().TrimEnd ()