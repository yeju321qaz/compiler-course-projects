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

module ECC.FrontEnd.TypeChecker

open ECC.Core
open ECC.FrontEnd

/// Type representation used by the type checker.
type TcType =
    | TInt
    | TChar
    | TVoid
    | TBool
    | TArr of TcType * int   // Element type is always TInt or TChar.

/// Identifier information stored in the symbol table. All type information is
/// expressed through `TcType`.
/// - `SymVar`:  Variable.
/// - `SymFunc`: Function — return type (`TVoid` allowed) plus the list of
///              parameter types.
type Symbol =
    | SymVar  of TcType
    | SymFunc of TcType * TcType list

/// Maps identifier names to symbols within a single scope.
type SymbolTable = Map<string, Symbol>

/// Scope stack: head is the innermost (current) scope; the last entry is the
/// outermost (global) scope.
type Scope = SymbolTable list

exception TypeError of int * int

let private failAt (loc: Location<'T>) : 'U =
    raise (TypeError (loc.Line, loc.Column))

let private failAtPos (line: int) (col: int) : 'U =
    raise (TypeError (line, col))

let private toTcType (ty: LCmmType) : TcType =
    match ty.Value with
    | CmmTyInt -> TInt
    | CmmTyChar -> TChar
    | CmmTyVoid -> TVoid

let rec private sameType t1 t2 =
    match t1, t2 with
    | TArr (elem1, size1), TArr (elem2, size2) ->
        size1 = size2 && sameType elem1 elem2
    | _ ->
        t1 = t2

let private lookup (name: string) (scope: Scope) : Symbol option =
    scope
    |> List.tryPick (fun table -> Map.tryFind name table)

let private addToCurrent (loc: Location<'T>) (name: string) (sym: Symbol) (scope: Scope) : Scope =
    match scope with
    | current :: rest ->
        if Map.containsKey name current then
            failAt loc
        else
            Map.add name sym current :: rest
    | [] ->
        [ Map.add name sym Map.empty ]

let private varSymbolFromName (baseTy: TcType) (nameLoc: LCmmVarName) : string * Symbol =
    match nameLoc.Value with
    | CmmVarScalar name ->
        name, SymVar baseTy

    | CmmVarArray (name, size) ->
        if size <= 0 then
            failAt nameLoc

        name, SymVar (TArr (baseTy, size))

let private paramType (param: LCmmParam) : TcType =
    match param.Value with
    | CmmParamScalar (ty, _) ->
        let t = toTcType ty

        if t = TVoid then
            failAt ty

        t

    | CmmParamArray (ty, _, size) ->
        let t = toTcType ty

        if t = TVoid then
            failAt ty

        if size <= 0 then
            failAt param

        TArr (t, size)

let private paramName (param: LCmmParam) : string =
    match param.Value with
    | CmmParamScalar (_, name) -> name
    | CmmParamArray (_, name, _) -> name

let private addParam (scope: Scope) (param: LCmmParam) : Scope =
    let name = paramName param
    let ty = paramType param
    addToCurrent param name (SymVar ty) scope

let private addVarDeclToScope (scope: Scope) (decl: LCmmVarDecl) : Scope =
    let baseTy = toTcType decl.Value.Type

    if baseTy = TVoid then
        failAt decl.Value.Type

    decl.Value.Names
    |> List.fold (fun sc nameLoc ->
        let name, sym = varSymbolFromName baseTy nameLoc
        addToCurrent nameLoc name sym sc
    ) scope

let private functionSignature (funcDecl: LCmmFuncDecl) : Symbol =
    let retTy = toTcType funcDecl.Value.RetType
    let paramTys = funcDecl.Value.Params |> List.map paramType
    SymFunc (retTy, paramTys)

let private addGlobalVarDecl (table: SymbolTable) (varDecl: LCmmVarDecl) : SymbolTable =
    let baseTy = toTcType varDecl.Value.Type

    if baseTy = TVoid then
        failAt varDecl.Value.Type

    varDecl.Value.Names
    |> List.fold (fun tbl nameLoc ->
        let name, sym = varSymbolFromName baseTy nameLoc

        if Map.containsKey name tbl then
            failAt nameLoc

        Map.add name sym tbl
    ) table

let private addGlobalFuncDecl (table: SymbolTable) (funcDecl: LCmmFuncDecl) : SymbolTable =
    let name = funcDecl.Value.Name

    if Map.containsKey name table then
        failAt funcDecl

    Map.add name (functionSignature funcDecl) table

let rec private checkExpr (scope: Scope) (expr: LCmmExpr) : TcType =
    match expr.Value with
    | CmmExpIntLit _ ->
        TInt

    | CmmExpCharLit _ ->
        TChar

    | CmmExpStrLit s ->
        TArr (TChar, s.Length + 1)

    | CmmExpVar name ->
        match lookup name scope with
        | Some (SymVar ty) -> ty
        | _ -> failAt expr

    | CmmExpArrAccess (name, idx) ->
        let idxTy = checkExpr scope idx

        if idxTy <> TInt then
            failAt idx

        match lookup name scope with
        | Some (SymVar (TArr (elemTy, _))) -> elemTy
        | _ -> failAt expr

    | CmmExpCall (name, args) ->
        checkCall scope expr.Line expr.Column name args true

    | CmmExpUnOp (op, inner) ->
        let innerTy = checkExpr scope inner

        match op.Value with
        | CmmUNeg ->
            if innerTy <> TInt then
                failAt expr

            TInt

        | CmmUNot ->
            if innerTy <> TBool then
                failAt expr

            TBool

    | CmmExpBinOp (op, lhs, rhs) ->
        let lt = checkExpr scope lhs
        let rt = checkExpr scope rhs

        match op.Value with
        | CmmBAdd
        | CmmBSub
        | CmmBMul
        | CmmBDiv ->
            if lt <> TInt || rt <> TInt then
                failAt expr

            TInt

        | CmmBAnd
        | CmmBOr ->
            if lt <> TBool || rt <> TBool then
                failAt expr

            TBool

    | CmmExpRelOp (_, lhs, rhs) ->
        let lt = checkExpr scope lhs
        let rt = checkExpr scope rhs

        if lt <> TInt || rt <> TInt then
            failAt expr

        TBool

and private checkCall
    (scope: Scope)
    (line: int)
    (col: int)
    (name: string)
    (args: LCmmExpr list)
    (usedAsExpr: bool)
    : TcType =

    match lookup name scope with
    | Some (SymFunc (retTy, paramTys)) ->
        if usedAsExpr && retTy = TVoid then
            failAtPos line col

        if List.length args <> List.length paramTys then
            failAtPos line col

        List.zip args paramTys
        |> List.iter (fun (arg, expectedTy) ->
            let actualTy = checkExpr scope arg

            if not (sameType actualTy expectedTy) then
                failAt arg
        )

        retTy

    | _ ->
        failAtPos line col

let private checkAssign (scope: Scope) (assign: LCmmAssign) : unit =
    match assign.Value with
    | CmmAssignVar (name, rhs) ->
        let rhsTy = checkExpr scope rhs

        match lookup name scope with
        | Some (SymVar (TArr _)) ->
            failAt assign

        | Some (SymVar lhsTy) ->
            if not (sameType lhsTy rhsTy) then
                failAt assign

        | _ ->
            failAt assign

    | CmmAssignArr (name, idx, rhs) ->
        let idxTy = checkExpr scope idx

        if idxTy <> TInt then
            failAt idx

        let rhsTy = checkExpr scope rhs

        match lookup name scope with
        | Some (SymVar (TArr (elemTy, _))) ->
            if not (sameType elemTy rhsTy) then
                failAt assign

        | _ ->
            failAt assign

let rec private hasValueReturnInStmt (stmt: LCmmStmt) : bool =
    match stmt.Value with
    | CmmStmtReturn (Some _) ->
        true

    | CmmStmtIf (_, thenStmt, elseStmt) ->
        hasValueReturnInStmt thenStmt
        ||
        match elseStmt with
        | Some s -> hasValueReturnInStmt s
        | None -> false

    | CmmStmtWhile (_, body) ->
        hasValueReturnInStmt body

    | CmmStmtFor (_, _, _, body) ->
        hasValueReturnInStmt body

    | CmmStmtBlock block ->
        hasValueReturnInBlock block

    | _ ->
        false

and private hasValueReturnInBlock (block: LCmmBlock) : bool =
    block.Value.Stmts
    |> List.exists hasValueReturnInStmt

let rec private checkStmt (scope: Scope) (retTy: TcType) (stmt: LCmmStmt) : unit =
    match stmt.Value with
    | CmmStmtIf (cond, thenStmt, elseStmt) ->
        let condTy = checkExpr scope cond

        if condTy <> TBool then
            failAt cond

        checkStmt scope retTy thenStmt

        match elseStmt with
        | Some s -> checkStmt scope retTy s
        | None -> ()

    | CmmStmtWhile (cond, body) ->
        let condTy = checkExpr scope cond

        if condTy <> TBool then
            failAt cond

        checkStmt scope retTy body

    | CmmStmtFor (initOpt, condOpt, updateOpt, body) ->
        match initOpt with
        | Some a -> checkAssign scope a
        | None -> ()

        match condOpt with
        | Some cond ->
            let condTy = checkExpr scope cond

            if condTy <> TBool then
                failAt cond

        | None -> ()

        match updateOpt with
        | Some a -> checkAssign scope a
        | None -> ()

        checkStmt scope retTy body

    | CmmStmtReturn None ->
        if retTy <> TVoid then
            failAt stmt

    | CmmStmtReturn (Some expr) ->
        let exprTy = checkExpr scope expr

        if retTy = TVoid then
            failAt stmt

        if not (sameType retTy exprTy) then
            failAt expr

    | CmmStmtAssign assign ->
        checkAssign scope assign

    | CmmStmtCall (name, args) ->
        checkCall scope stmt.Line stmt.Column name args false |> ignore

    | CmmStmtBlock block ->
        checkBlock (Map.empty :: scope) retTy block

    | CmmStmtEmpty ->
        ()

and private checkBlock (scope: Scope) (retTy: TcType) (block: LCmmBlock) : unit =
    let scopeWithDecls =
        block.Value.Decls
        |> List.fold addVarDeclToScope scope

    block.Value.Stmts
    |> List.iter (checkStmt scopeWithDecls retTy)

let private checkFunction (globalScope: SymbolTable) (funcDecl: LCmmFuncDecl) : unit =
    let retTy = toTcType funcDecl.Value.RetType

    let scopeWithParams =
        funcDecl.Value.Params
        |> List.fold addParam [ Map.empty; globalScope ]

    checkBlock scopeWithParams retTy funcDecl.Value.Body

    if retTy <> TVoid && not (hasValueReturnInBlock funcDecl.Value.Body) then
        failAt funcDecl

let private checkDeclSequentially (globalScope: SymbolTable) (decl: LCmmDecl) : SymbolTable =
    match decl.Value with
    | CmmDeclVar varDecl ->
        addGlobalVarDecl globalScope varDecl

    | CmmDeclFunc funcDecl ->
        let newGlobalScope = addGlobalFuncDecl globalScope funcDecl
        checkFunction newGlobalScope funcDecl
        newGlobalScope

/// Perform type checking on a `CmmProgram`.
let check (prog: CmmProgram) : Result<CmmProgram, int * int> =
    try
        prog
        |> List.fold checkDeclSequentially Map.empty
        |> ignore

        Ok prog

    with
    | TypeError (line, col) ->
        Error (line, col)