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

module ECC.FrontEnd.IRTranslator

open ECC.IR
open ECC.FrontEnd

let private toIRUnOp = function
    | CmmUNeg -> UNeg
    | CmmUNot -> UNot

let private toIRBinOp = function
    | CmmBAdd -> BAdd
    | CmmBSub -> BSub
    | CmmBMul -> BMul
    | CmmBDiv -> BDiv
    | CmmBAnd -> BAnd
    | CmmBOr -> BOr

let private toIRRelOp = function
    | CmmRLt -> Rlt
    | CmmRLeq -> Rle
    | CmmREq -> Req
    | CmmRNeq -> Rne
    | CmmRGeq -> Rge
    | CmmRGt -> Rgt

let private translateFunction (funcDecl: LCmmFuncDecl) : EIR =
    let tempCount = ref 0
    let labelCount = ref 0

    let freshTemp () =
        tempCount.Value <- tempCount.Value + 1
        sprintf "t%d" tempCount.Value

    let freshLabel () =
        labelCount.Value <- labelCount.Value + 1
        LblLocal (sprintf "L%d" labelCount.Value)

    let rec translateExpr (expr: LCmmExpr) : EIRStmt list * EIRVar =
        match expr.Value with
        | CmmExpIntLit n ->
            let t = freshTemp ()
            [ StmtAssign (t, ExprInt n) ], t

        | CmmExpCharLit c ->
            let t = freshTemp ()
            [ StmtAssign (t, ExprChar c) ], t

        | CmmExpStrLit s ->
            let t = freshTemp ()
            [ StmtAssign (t, ExprVar (sprintf "\"%s\"" s)) ], t

        | CmmExpVar name ->
            [], name

        | CmmExpArrAccess (name, idx) ->
            let idxCode, idxVar = translateExpr idx
            let t = freshTemp ()
            idxCode @ [ StmtAssign (t, ExprLoad (name, ExprVar idxVar)) ], t

        | CmmExpCall (name, args) ->
            let argResults = args |> List.map translateExpr
            let argCode = argResults |> List.collect fst
            let argVars = argResults |> List.map snd

            let t = freshTemp ()

            let paramCode =
                argVars |> List.map StmtParam

            argCode
            @ paramCode
            @ [
                StmtCall (LblFunc name, List.length args)
                StmtRetrieve t
            ], t

        | CmmExpUnOp (op, inner) ->
            let innerCode, innerVar = translateExpr inner
            let t = freshTemp ()

            innerCode
            @ [ StmtAssign (t, ExprUnOp (toIRUnOp op.Value, ExprVar innerVar)) ], t

        | CmmExpBinOp (op, lhs, rhs) ->
            let lhsCode, lhsVar = translateExpr lhs
            let rhsCode, rhsVar = translateExpr rhs
            let t = freshTemp ()

            lhsCode
            @ rhsCode
            @ [ StmtAssign (t, ExprBinOp (toIRBinOp op.Value, ExprVar lhsVar, ExprVar rhsVar)) ], t

        | CmmExpRelOp (op, lhs, rhs) ->
            let lhsCode, lhsVar = translateExpr lhs
            let rhsCode, rhsVar = translateExpr rhs
            let t = freshTemp ()

            lhsCode
            @ rhsCode
            @ [ StmtAssign (t, ExprRelOp (toIRRelOp op.Value, ExprVar lhsVar, ExprVar rhsVar)) ], t

    and translateAssign (assign: LCmmAssign) : EIRStmt list =
        match assign.Value with
        | CmmAssignVar (name, rhs) ->
            let rhsCode, rhsVar = translateExpr rhs
            rhsCode @ [ StmtAssign (name, ExprVar rhsVar) ]

        | CmmAssignArr (name, idx, rhs) ->
            let idxCode, idxVar = translateExpr idx
            let rhsCode, rhsVar = translateExpr rhs

            idxCode
            @ rhsCode
            @ [ StmtStore (name, ExprVar idxVar, ExprVar rhsVar) ]

    and translateStmt (stmt: LCmmStmt) : EIRStmt list =
        match stmt.Value with
        | CmmStmtIf (cond, thenStmt, elseStmt) ->
            let condCode, condVar = translateExpr cond
            let thenLabel = freshLabel ()
            let elseLabel = freshLabel ()
            let endLabel = freshLabel ()

            let thenCode = translateStmt thenStmt

            let elseCode =
                match elseStmt with
                | Some s -> translateStmt s
                | None -> []

            condCode
            @ [
                StmtCJump (condVar, thenLabel, elseLabel)
                StmtLabel thenLabel
            ]
            @ thenCode
            @ [
                StmtJump endLabel
                StmtLabel elseLabel
            ]
            @ elseCode
            @ [
                StmtLabel endLabel
            ]

        | CmmStmtWhile (cond, body) ->
            let condLabel = freshLabel ()
            let bodyLabel = freshLabel ()
            let endLabel = freshLabel ()

            let condCode, condVar = translateExpr cond
            let bodyCode = translateStmt body

            [
                StmtLabel condLabel
            ]
            @ condCode
            @ [
                StmtCJump (condVar, bodyLabel, endLabel)
                StmtLabel bodyLabel
            ]
            @ bodyCode
            @ [
                StmtJump condLabel
                StmtLabel endLabel
            ]

        | CmmStmtFor (initOpt, condOpt, updateOpt, body) ->
            let startLabel = freshLabel ()
            let bodyLabel = freshLabel ()
            let endLabel = freshLabel ()

            let initCode =
                match initOpt with
                | Some a -> translateAssign a
                | None -> []

            let condCode =
                match condOpt with
                | Some cond ->
                    let cCode, cVar = translateExpr cond
                    cCode @ [ StmtCJump (cVar, bodyLabel, endLabel) ]
                | None ->
                    []

            let updateCode =
                match updateOpt with
                | Some a -> translateAssign a
                | None -> []

            let bodyCode = translateStmt body

            initCode
            @ [
                StmtLabel startLabel
            ]
            @ condCode
            @ [
                StmtLabel bodyLabel
            ]
            @ bodyCode
            @ updateCode
            @ [
                StmtJump startLabel
                StmtLabel endLabel
            ]

        | CmmStmtReturn None ->
            [ StmtReturn None ]

        | CmmStmtReturn (Some expr) ->
            let exprCode, exprVar = translateExpr expr
            exprCode @ [ StmtReturn (Some exprVar) ]

        | CmmStmtAssign assign ->
            translateAssign assign

        | CmmStmtCall (name, args) ->
            let argResults = args |> List.map translateExpr
            let argCode = argResults |> List.collect fst
            let argVars = argResults |> List.map snd

            let paramCode =
                argVars |> List.map StmtParam

            argCode
            @ paramCode
            @ [ StmtCall (LblFunc name, List.length args) ]

        | CmmStmtBlock block ->
            translateBlock block

        | CmmStmtEmpty ->
            []

    and translateBlock (block: LCmmBlock) : EIRStmt list =
        block.Value.Stmts
        |> List.collect translateStmt

    let name = funcDecl.Value.Name
    let funcLabel = LblFunc name

    [
        StmtFuncStart funcLabel
    ]
    @ translateBlock funcDecl.Value.Body
    @ [
        StmtFuncEnd funcLabel
    ]

/// Translate a `CmmProgram` into an `EIR`.
let translate (prog: CmmProgram) : EIR =
    prog
    |> List.collect (fun decl ->
        match decl.Value with
        | CmmDeclFunc funcDecl -> translateFunction funcDecl
        | CmmDeclVar _ -> []
    )