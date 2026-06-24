module ECC.FrontEnd.Tests.Student

open Xunit
open ECC.Core
open ECC.FrontEnd

let private mustParse input =
    match EntryPoints.parseString input with
    | Ok ast -> ast
    | Error (line, col) ->
        Assert.Fail(sprintf "Parsing failed at line %d, column %d" line col)
        []

let private mustFailParse input =
    match EntryPoints.parseString input with
    | Error _ -> ()
    | Ok _ -> Assert.Fail(sprintf "Expected parse failure but succeeded for input: %s" input)

[<Fact; Trait("TestSet", "student")>]
let ``Student Comprehensive Parser Test`` () =
    let input = """
int g, arr[5];
char ch;

int add(int a, int b) {
  int result;
  result = a + b;
  return result;
}

void touch(int a, char c, int xs[3]) {
  a = a + 1;
  xs[0] = a;
  return;
}

int main(void) {
  int x;
  int y;
  int local[3];
  char c;

  x = 1 + 2 * 3;
  y = add(x, 4);
  c = 'a';
  local[0] = y;
  local[1] = -x + 10;
  local[2] = !(x < y == 1) || 0;

  touch(y, c, local);
  add(1, 2);

  if (local[0] >= 10) {
    y = y - 1;
  } else {
    y = y + 1;
  }

  while (x < y) {
    x = x + 1;
  }

  for (x = 0; x < 3; x = x + 1) {
    local[x] = x;
  }

  for (; x < 5;) {
    x = x + 1;
  }

  for (;;) {
    ;
    return local[0] + y;
  }

  return "done";
}
"""
    let ast = mustParse input

    match ast with
    | [
        { Value = CmmDeclVar _ }
        { Value = CmmDeclVar _ }
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc mainFunc }
      ] ->
        Assert.Equal("main", mainFunc.Value.Name)
        Assert.Equal(4, mainFunc.Value.Body.Value.Decls.Length)
        Assert.True(mainFunc.Value.Body.Value.Stmts.Length >= 10)
    | _ ->
        Assert.Fail("Unexpected top-level AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Dangling Else Test`` () =
    let input = """
int main(void) {
  int x;
  int y;
  x = 0;
  y = 0;

  if (x)
    if (y)
      x = 1;
    else
      x = 2;

  return x;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let stmts = f.Value.Body.Value.Stmts
        match stmts with
        | _ :: _ :: outerIf :: _ ->
            match outerIf.Value with
            | CmmStmtIf (_, innerStmt, None) ->
                match innerStmt.Value with
                | CmmStmtIf (_, _, Some _) ->
                    Assert.True(true)
                | _ ->
                    Assert.Fail("The inner statement should be an if-else statement")
            | _ ->
                Assert.Fail("The outer if should not have an else branch")
        | _ ->
            Assert.Fail("Unexpected statement list")
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Empty Program Test`` () =
    let input = ""
    let ast = mustParse input
    Assert.True(List.isEmpty ast)

[<Fact; Trait("TestSet", "student")>]
let ``Student Expression Precedence Test`` () =
    let input = """
int main(void) {
  return 1 + 2 * 3 < 10 == 1 && 0 || 1;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | [{ Value = CmmStmtReturn (Some expr) }] ->
            match expr.Value with
            | CmmExpBinOp (op, _, _) ->
                Assert.Equal(CmmBOr, op.Value)
            | _ ->
                Assert.Fail("Top-level expression should be ||")
        | _ ->
            Assert.Fail("Unexpected statement shape")
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Function Call And Literals Test`` () =
    let input = """
void print(int x, char c, int arr[2]) {
  return;
}

int main(void) {
  int a[2];
  char c;
  a[0] = 1;
  c = '\n';
  print(a[0], c, a);
  return 0;
}
"""
    let ast = mustParse input

    match ast with
    | [
        { Value = CmmDeclFunc printFunc }
        { Value = CmmDeclFunc mainFunc }
      ] ->
        Assert.Equal("print", printFunc.Value.Name)
        Assert.Equal(3, printFunc.Value.Params.Length)
        Assert.Equal("main", mainFunc.Value.Name)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Pretty Printer Coverage Test`` () =
    let input = """
int g, arr[5];
char ch;

int add(int a, int b) {
  int result;
  result = a + b;
  return result;
}

void touch(int a, char c, int xs[3]) {
  a = a + 1;
  xs[0] = a;
  return;
}

int main(void) {
  int x;
  int y;
  int local[3];
  char c;

  ;
  x = 1 + 2 * 3 / 4 - 5;
  y = add(x, 4);
  c = '\n';
  local[0] = y;
  local[1] = -x + 10;
  local[2] = !(x < y == 1) || 0;

  touch(y, c, local);
  add(1, 2);

  if (local[0] >= 10) {
    y = y - 1;
  } else {
    y = y + 1;
  }

  if (x <= y) {
    x = x + 1;
  }

  while (x != y) {
    x = x + 1;
  }

  for (x = 0; x < 3; x = x + 1) {
    local[x] = x;
  }

  for (; x > 0;) {
    x = x - 1;
  }

  for (;;) {
    ;
    return local[0] + y;
  }

  return "hello\nworld";
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("Program", printed)
    Assert.Contains("DeclVar", printed)
    Assert.Contains("FuncDecl main", printed)
    Assert.Contains("StmtIf", printed)
    Assert.Contains("StmtWhile", printed)
    Assert.Contains("StmtFor", printed)
    Assert.Contains("StmtEmpty", printed)
    Assert.Contains("StmtReturn", printed)
    Assert.Contains("StmtCall", printed)
    Assert.Contains("AssignArr", printed)
    Assert.Contains("ArrAccess", printed)
    Assert.Contains("CharLit", printed)
    Assert.Contains("StrLit", printed)
    Assert.Contains("UnOp", printed)
    Assert.Contains("BinOp", printed)
    Assert.Contains("RelOp", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Comment And Escaped Literal Parse Test`` () =
    let input = """
// line comment
/* block
   comment */
int main(void) {
  char c;
  c = '\n';
  c = '\0';
  c = '\t';
  c = '\'';
  return "hello\n\0\t\"world";
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("CharLit", printed)
    Assert.Contains("StrLit", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Syntax Error Coverage Test`` () =
    let badInputs =
        [
            "int main(void) { return 0 "
            "int main(void) { int x x = 1; return x; }"
            "int main(void) { return 1 + ; }"
            "int main(void) { if (1) else return 0; }"
            "int main(void) { for (i = 0 i < 10; i = i + 1) return i; }"
        ]

    for input in badInputs do
        mustFailParse input

[<Fact; Trait("TestSet", "student")>]
let ``Student All Operators Coverage Test`` () =
    let input = """
int main(void) {
  int a;
  int b;
  int c;
  a = 1 + 2;
  a = 1 - 2;
  a = 1 * 2;
  a = 1 / 2;
  a = 1 && 0;
  a = 1 || 0;
  a = 1 == 2;
  a = 1 != 2;
  a = 1 < 2;
  a = 1 <= 2;
  a = 1 > 2;
  a = 1 >= 2;
  a = -a;
  a = !a;
  return a;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("BinOp", printed)
    Assert.Contains("RelOp", printed)
    Assert.Contains("UnOp", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student All Relop Nodes Via Parse Test`` () =
    let checkRelOp src expectedOp =
        let input = sprintf "int main(void) { return 1 %s 2; }" src
        match EntryPoints.parseString input with
        | Ok [{ Value = CmmDeclFunc f }] ->
            match f.Value.Body.Value.Stmts with
            | [{ Value = CmmStmtReturn (Some { Value = CmmExpRelOp (op, _, _) }) }] ->
                Assert.Equal(expectedOp, op.Value)
            | _ -> Assert.Fail(sprintf "Expected relop for %s" src)
        | _ -> Assert.Fail(sprintf "Parse failed for %s" src)

    checkRelOp "==" CmmREq
    checkRelOp "!=" CmmRNeq
    checkRelOp "<"  CmmRLt
    checkRelOp "<=" CmmRLeq
    checkRelOp ">"  CmmRGt
    checkRelOp ">=" CmmRGeq

[<Fact; Trait("TestSet", "student")>]
let ``Student All Binop Nodes Via Parse Test`` () =
    let checkBinOp src expectedOp =
        let input = sprintf "int main(void) { return 1 %s 2; }" src
        match EntryPoints.parseString input with
        | Ok [{ Value = CmmDeclFunc f }] ->
            match f.Value.Body.Value.Stmts with
            | [{ Value = CmmStmtReturn (Some { Value = CmmExpBinOp (op, _, _) }) }] ->
                Assert.Equal(expectedOp, op.Value)
            | _ -> Assert.Fail(sprintf "Expected binop for %s" src)
        | _ -> Assert.Fail(sprintf "Parse failed for %s" src)

    checkBinOp "+"  CmmBAdd
    checkBinOp "-"  CmmBSub
    checkBinOp "*"  CmmBMul
    checkBinOp "/"  CmmBDiv
    checkBinOp "&&" CmmBAnd
    checkBinOp "||" CmmBOr

[<Fact; Trait("TestSet", "student")>]
let ``Student Unary Operators Via Parse Test`` () =
    let negInput = "int main(void) { int x; x = -x; return x; }"
    match EntryPoints.parseString negInput with
    | Ok [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | { Value = CmmStmtAssign { Value = CmmAssignVar (_, { Value = CmmExpUnOp (op, _) }) } } :: _ ->
            Assert.Equal(CmmUNeg, op.Value)
        | _ -> Assert.Fail("Expected unary neg assignment")
    | _ -> Assert.Fail("Parse failed for unary neg")

    let notInput = "int main(void) { int x; x = !x; return x; }"
    match EntryPoints.parseString notInput with
    | Ok [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | { Value = CmmStmtAssign { Value = CmmAssignVar (_, { Value = CmmExpUnOp (op, _) }) } } :: _ ->
            Assert.Equal(CmmUNot, op.Value)
        | _ -> Assert.Fail("Expected unary not assignment")
    | _ -> Assert.Fail("Parse failed for unary not")

[<Fact; Trait("TestSet", "student")>]
let ``Student Char Literals Via Parse Test`` () =
    let input = """
int main(void) {
  char c;
  c = '\n';
  c = '\0';
  c = '\t';
  c = '\'';
  c = 'a';
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("CharLit", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student String Literal Via Parse Test`` () =
    let input = """
int main(void) {
  return "hello\nworld\0\t\"end";
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("StrLit", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Var And Array Decl Via Parse Test`` () =
    let input = """
int g1, g2;
int arr1[10];
char ch1, ch2;
char carr[5];

int main(void) {
  int x, y;
  int local[3];
  char c;
  char ca[2];
  return 0;
}
"""
    let ast = mustParse input
    match ast with
    | { Value = CmmDeclVar v1 } :: { Value = CmmDeclVar _ } :: { Value = CmmDeclVar _ } :: { Value = CmmDeclVar _ } :: _ ->
        Assert.True(v1.Value.Names.Length >= 1)
    | _ ->
        Assert.Fail("Unexpected top-level shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Param Types Via Parse Test`` () =
    let input = """
int f1(void) { return 0; }
int f2(int a, char b) { return a; }
int f3(int xs[3], char cs[2]) { return 0; }
int main(void) { return 0; }
"""
    let ast = mustParse input
    match ast with
    | [
        { Value = CmmDeclFunc f1 }
        { Value = CmmDeclFunc f2 }
        { Value = CmmDeclFunc f3 }
        { Value = CmmDeclFunc _ }
      ] ->
        Assert.Equal(0, f1.Value.Params.Length)
        Assert.Equal(2, f2.Value.Params.Length)
        Assert.Equal(2, f3.Value.Params.Length)
        match f3.Value.Params.[0].Value with
        | CmmParamArray _ -> Assert.True(true)
        | _ -> Assert.Fail("Expected array param")
    | _ ->
        Assert.Fail("Unexpected AST shape for param types test")

[<Fact; Trait("TestSet", "student")>]
let ``Student For Loop Variants Via Parse Test`` () =
    let input = """
int main(void) {
  int i;
  for (i = 0; i < 10; i = i + 1) {
    ;
  }
  for (; i < 5;) {
    i = i + 1;
  }
  for (;;) {
    return i;
  }
  return 0;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let forStmts =
            f.Value.Body.Value.Stmts
            |> List.choose (fun s ->
                match s.Value with
                | CmmStmtFor (init, cond, update, _) -> Some (init, cond, update)
                | _ -> None)

        Assert.Equal(3, forStmts.Length)

        let (i1, c1, u1) = forStmts.[0]
        Assert.True(i1.IsSome)
        Assert.True(c1.IsSome)
        Assert.True(u1.IsSome)

        let (i2, c2, u2) = forStmts.[1]
        Assert.True(i2.IsNone)
        Assert.True(c2.IsSome)
        Assert.True(u2.IsNone)

        let (i3, c3, u3) = forStmts.[2]
        Assert.True(i3.IsNone)
        Assert.True(c3.IsNone)
        Assert.True(u3.IsNone)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Block Stmt Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  {
    int y;
    y = 1;
    x = y;
  }
  return x;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("StmtBlock", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Array Access And Assignment Via Parse Test`` () =
    let input = """
int main(void) {
  int arr[5];
  arr[0] = 1;
  arr[1] = arr[0] + 2;
  return arr[1];
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("AssignArr", printed)
    Assert.Contains("ArrAccess", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Function Call As Expression Via Parse Test`` () =
    let input = """
int add(int a, int b) { return a + b; }
int main(void) {
  int x;
  x = add(1, 2);
  return add(x, add(1, 2));
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("Call", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Stmt Call No Return Via Parse Test`` () =
    let input = """
void doNothing(void) { return; }
int main(void) {
  doNothing();
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("StmtCall", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Return Void Via Parse Test`` () =
    let input = """
void f(void) {
  return;
}
int main(void) { return 0; }
"""
    let ast = mustParse input
    match ast with
    | { Value = CmmDeclFunc f } :: _ ->
        let stmts = f.Value.Body.Value.Stmts
        match stmts with
        | [{ Value = CmmStmtReturn None }] -> Assert.True(true)
        | _ -> Assert.Fail("Expected void return")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Parenthesized Expr Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = (1 + 2) * 3;
  return x;
}
"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | { Value = CmmStmtAssign { Value = CmmAssignVar (_, { Value = CmmExpBinOp (op, lhs, _) }) } } :: _ ->
            Assert.Equal(CmmBMul, op.Value)
            match lhs.Value with
            | CmmExpBinOp (innerOp, _, _) -> Assert.Equal(CmmBAdd, innerOp.Value)
            | _ -> Assert.Fail("LHS should be BinOp Add")
        | _ -> Assert.Fail("Unexpected stmt shape")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student While Stmt Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  while (x < 10) {
    x = x + 1;
  }
  return x;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("StmtWhile", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student Void Return Type Func Via Parse Test`` () =
    let input = """
void greet(void) {
  return;
}
int main(void) { return 0; }
"""
    let ast = mustParse input
    match ast with
    | { Value = CmmDeclFunc f } :: _ ->
        match f.Value.RetType.Value with
        | CmmTyVoid -> Assert.True(true)
        | _ -> Assert.Fail("Expected void return type")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Char Return Type Func Via Parse Test`` () =
    let input = """
char getChar(void) {
  return 'a';
}
int main(void) { return 0; }
"""
    let ast = mustParse input
    match ast with
    | { Value = CmmDeclFunc f } :: _ ->
        match f.Value.RetType.Value with
        | CmmTyChar -> Assert.True(true)
        | _ -> Assert.Fail("Expected char return type")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Multi Var Same Line Via Parse Test`` () =
    let input = """
int a, b, c;
char x, y;
int main(void) { return 0; }
"""
    let ast = mustParse input
    match ast with
    | { Value = CmmDeclVar v1 } :: { Value = CmmDeclVar v2 } :: _ ->
        Assert.Equal(3, v1.Value.Names.Length)
        Assert.Equal(2, v2.Value.Names.Length)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student AST Printer prettyPrintExpr And prettyPrintStmt Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = 1 + 2;
  return x;
}
"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let stmts = f.Value.Body.Value.Stmts
        let assignStmt = stmts.[0]
        let assignText = ASTPrinter.prettyPrintStmt assignStmt
        Assert.Contains("StmtAssign", assignText)

        let retStmt = stmts.[1]
        let retText = ASTPrinter.prettyPrintStmt retStmt
        Assert.Contains("StmtReturn", retText)

        match assignStmt.Value with
        | CmmStmtAssign { Value = CmmAssignVar (_, expr) } ->
            let exprText = ASTPrinter.prettyPrintExpr expr
            Assert.Contains("BinOp", exprText)
        | _ -> Assert.Fail("Expected assign stmt")
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student For With Array Assign Via Parse Test`` () =
    let input = """
int main(void) {
  int arr[5];
  int i;
  for (arr[0] = 0; i < 5; arr[0] = arr[0] + 1) {
    i = i + 1;
  }
  return arr[0];
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("StmtFor", printed)
    Assert.Contains("AssignArr", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student ASTPrinter Empty Program And Void Func Via Parse Test`` () =
    let emptyAst = mustParse ""
    let emptyText = ASTPrinter.prettyPrint emptyAst
    Assert.Contains("Program", emptyText)

    let input = """
void noop(void) {
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("void", printed)
    Assert.Contains("params: (none)", printed)
    Assert.Contains("decls: (none)", printed)
    Assert.Contains("stmts: (none)", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student AssignVar And Nested Expr Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  int y;
  x = 1;
  y = x;
  x = x + y * 2 - 1 / 1;
  return x;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast
    Assert.Contains("AssignVar", printed)
    Assert.Contains("BinOp", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student If Without Else Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  if (x == 0) {
    x = 1;
  }
  return x;
}
"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let ifStmts =
            f.Value.Body.Value.Stmts
            |> List.choose (fun s ->
                match s.Value with
                | CmmStmtIf (_, _, elseOpt) -> Some elseOpt
                | _ -> None)
        Assert.Equal(1, ifStmts.Length)
        Assert.True(ifStmts.[0].IsNone)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student If With Else Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  if (x == 0) {
    x = 1;
  } else {
    x = 2;
  }
  return x;
}
"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let ifStmts =
            f.Value.Body.Value.Stmts
            |> List.choose (fun s ->
                match s.Value with
                | CmmStmtIf (_, _, elseOpt) -> Some elseOpt
                | _ -> None)
        Assert.Equal(1, ifStmts.Length)
        Assert.True(ifStmts.[0].IsSome)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Var Scalar And Array Name Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  int arr[5];
  return 0;
}
"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let decls = f.Value.Body.Value.Decls
        Assert.Equal(2, decls.Length)
        match decls.[0].Value.Names.[0].Value with
        | CmmVarScalar "x" -> Assert.True(true)
        | _ -> Assert.Fail("Expected CmmVarScalar x")
        match decls.[1].Value.Names.[0].Value with
        | CmmVarArray ("arr", 5) -> Assert.True(true)
        | _ -> Assert.Fail("Expected CmmVarArray arr[5]")
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Param Scalar And Array Via Parse Test`` () =
    let input = """
int f(int a, int xs[3]) { return a; }
int main(void) { return 0; }
"""
    let ast = mustParse input
    match ast with
    | { Value = CmmDeclFunc f } :: _ ->
        Assert.Equal(2, f.Value.Params.Length)
        match f.Value.Params.[0].Value with
        | CmmParamScalar (_, "a") -> Assert.True(true)
        | _ -> Assert.Fail("Expected CmmParamScalar a")
        match f.Value.Params.[1].Value with
        | CmmParamArray (_, "xs", 3) -> Assert.True(true)
        | _ -> Assert.Fail("Expected CmmParamArray xs[3]")
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Var Expr Via Parse Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  return x;
}
"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | _ :: { Value = CmmStmtReturn (Some { Value = CmmExpVar "x" }) } :: _ ->
            Assert.True(true)
        | _ -> Assert.Fail("Expected return x")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Int Lit Expr Via Parse Test`` () =
    let input = "int main(void) { return 42; }"
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | [{ Value = CmmStmtReturn (Some { Value = CmmExpIntLit 42 }) }] ->
            Assert.True(true)
        | _ -> Assert.Fail("Expected return 42")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Char Lit Expr Via Parse Test`` () =
    let input = "int main(void) { char c; c = 'z'; return 0; }"
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | { Value = CmmStmtAssign { Value = CmmAssignVar (_, { Value = CmmExpCharLit 'z' }) } } :: _ ->
            Assert.True(true)
        | _ -> Assert.Fail("Expected char lit 'z'")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student Str Lit Expr Via Parse Test`` () =
    let input = """int main(void) { return "abc"; }"""
    let ast = mustParse input
    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | [{ Value = CmmStmtReturn (Some { Value = CmmExpStrLit "abc" }) }] ->
            Assert.True(true)
        | _ -> Assert.Fail("Expected StrLit abc")
    | _ -> Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrint Empty Program Branch Test`` () =
    let emptyAst = mustParse ""
    let text = ASTPrinter.prettyPrint emptyAst

    Assert.Contains("Program", text)

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintStmt If No Else Branch Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  if (x == 0) {
    x = 1;
  }
  return x;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let ifStmt =
            f.Value.Body.Value.Stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtIf _ -> true
                | _ -> false)

        match ifStmt.Value with
        | CmmStmtIf (_, _, elseOpt) ->
            Assert.True(elseOpt.IsNone)
        | _ ->
            Assert.Fail("Expected if statement")

        let text = ASTPrinter.prettyPrintStmt ifStmt
        Assert.Contains("StmtIf", text)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintStmt If With Else Branch Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  if (x == 0) {
    x = 1;
  } else {
    x = 2;
  }
  return x;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let ifStmt =
            f.Value.Body.Value.Stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtIf _ -> true
                | _ -> false)

        match ifStmt.Value with
        | CmmStmtIf (_, _, elseOpt) ->
            Assert.True(elseOpt.IsSome)
        | _ ->
            Assert.Fail("Expected if-else statement")

        let text = ASTPrinter.prettyPrintStmt ifStmt
        Assert.Contains("StmtIf", text)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintStmt While And For Branch Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  while (x < 10) {
    x = x + 1;
  }
  for (x = 0; x < 3; x = x + 1) {
    ;
  }
  return x;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let stmts = f.Value.Body.Value.Stmts

        let whileStmt =
            stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtWhile _ -> true
                | _ -> false)

        let whileText = ASTPrinter.prettyPrintStmt whileStmt
        Assert.Contains("StmtWhile", whileText)

        let forStmt =
            stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtFor _ -> true
                | _ -> false)

        let forText = ASTPrinter.prettyPrintStmt forStmt
        Assert.Contains("StmtFor", forText)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintStmt Block Branch Test`` () =
    let input = """
int main(void) {
  int x;
  x = 0;
  {
    x = 1;
  }
  return x;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let blockStmt =
            f.Value.Body.Value.Stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtBlock _ -> true
                | _ -> false)

        let text = ASTPrinter.prettyPrintStmt blockStmt
        Assert.Contains("StmtBlock", text)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student VarDecl Int And Char Type String Branch Test`` () =
    let input = """
int main(void) {
  int x;
  char c;
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("VarDecl", printed)
    Assert.Contains("int", printed)
    Assert.Contains("char", printed)

[<Fact; Trait("TestSet", "student")>]
let ``Student StmtCall And ExpCall No Args Branch Test`` () =
    let input = """
void f(void) { return; }
int g(void) { return 0; }
int main(void) {
  int x;
  f();
  x = g();
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("StmtCall", printed)
    Assert.Contains("Call g", printed)

    match ast with
    | [
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc mainFunc }
      ] ->
        let callStmt =
            mainFunc.Value.Body.Value.Stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtCall _ -> true
                | _ -> false)

        let stmtText = ASTPrinter.prettyPrintStmt callStmt
        Assert.Contains("StmtCall", stmtText)
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintExpr All Leaf Nodes Branch Test`` () =
    let input = """
int f(void) { return 0; }
int main(void) {
  int x;
  int arr[3];
  x = 0;
  arr[0] = x;
  return f();
}
"""
    let ast = mustParse input

    match ast with
    | [
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc mainFunc }
      ] ->
        let stmts = mainFunc.Value.Body.Value.Stmts

        match stmts.[0].Value with
        | CmmStmtAssign { Value = CmmAssignVar (_, e) } ->
            let text = ASTPrinter.prettyPrintExpr e
            Assert.Contains("IntLit", text)
        | _ ->
            Assert.Fail("Expected assignment x = 0")

        match stmts.[1].Value with
        | CmmStmtAssign { Value = CmmAssignArr (_, idx, rhs) } ->
            let idxText = ASTPrinter.prettyPrintExpr idx
            Assert.Contains("IntLit", idxText)

            let rhsText = ASTPrinter.prettyPrintExpr rhs
            Assert.Contains("Var", rhsText)
        | _ ->
            Assert.Fail("Expected assignment arr[0] = x")

        match stmts.[2].Value with
        | CmmStmtReturn (Some e) ->
            let text = ASTPrinter.prettyPrintExpr e
            Assert.Contains("Call", text)
        | _ ->
            Assert.Fail("Expected return f()")
    | _ ->
        Assert.Fail("Unexpected AST shape")

[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrint Empty Program Exact Branch Test`` () =
    let emptyAst = mustParse ""
    let text = ASTPrinter.prettyPrint emptyAst

    Assert.Contains("Program", text)


[<Fact; Trait("TestSet", "student")>]
let ``Student escapeChar Branches Via Parsed AST Test`` () =
    let input = """
int main(void) {
  char c;
  c = '\n';
  c = '\0';
  c = '\t';
  c = '\'';
  c = 'a';
  return 0;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let stmts = f.Value.Body.Value.Stmts

        for i in 0 .. 4 do
            match stmts.[i].Value with
            | CmmStmtAssign { Value = CmmAssignVar (_, e) } ->
                let text = ASTPrinter.prettyPrintExpr e
                Assert.Contains("CharLit", text)
            | _ ->
                Assert.Fail(sprintf "Expected char assignment at statement %d" i)
    | _ ->
        Assert.Fail("Unexpected AST shape")


[<Fact; Trait("TestSet", "student")>]
let ``Student escapeStringContent Branches Via Parsed AST Test`` () =
    let input = """int main(void) { return "a\n\0\t\"b"; }"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        match f.Value.Body.Value.Stmts with
        | [{ Value = CmmStmtReturn (Some e) }] ->
            let text = ASTPrinter.prettyPrintExpr e
            Assert.Contains("StrLit", text)
            Assert.Contains("a", text)
            Assert.Contains("b", text)
        | _ ->
            Assert.Fail("Expected return with string literal")
    | _ ->
        Assert.Fail("Unexpected AST shape")


[<Fact; Trait("TestSet", "student")>]
let ``Student pVarName Array Branch Via prettyPrint Test`` () =
    let input = """
int main(void) {
  int arr[5];
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("arr[5]", printed)
    Assert.Contains("array", printed)


[<Fact; Trait("TestSet", "student")>]
let ``Student pVarName Scalar Branch Via prettyPrint Test`` () =
    let input = """
int main(void) {
  int x;
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("x", printed)
    Assert.Contains("scalar", printed)


[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintStmt For None Branches Via prettyPrintStmt Test`` () =
    let input = """
int main(void) {
  for (;;) {
    return 0;
  }
  return 0;
}
"""
    let ast = mustParse input

    match ast with
    | [{ Value = CmmDeclFunc f }] ->
        let forStmt =
            f.Value.Body.Value.Stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtFor _ -> true
                | _ -> false)

        let text = ASTPrinter.prettyPrintStmt forStmt
        Assert.Contains("StmtFor", text)
        Assert.Contains("(none)", text)
    | _ ->
        Assert.Fail("Unexpected AST shape")


[<Fact; Trait("TestSet", "student")>]
let ``Student pParam Array Branch Via prettyPrint Test`` () =
    let input = """
int f(int xs[3]) {
  return 0;
}

int main(void) {
  return 0;
}
"""
    let ast = mustParse input
    let printed = ASTPrinter.prettyPrint ast

    Assert.Contains("xs[3]", printed)


[<Fact; Trait("TestSet", "student")>]
let ``Student writeList None Branch Via StmtCall Test`` () =
    let input = """
void f(void) {
  return;
}

int main(void) {
  f();
  return 0;
}
"""
    let ast = mustParse input

    match ast with
    | [
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc mainFunc }
      ] ->
        let callStmt =
            mainFunc.Value.Body.Value.Stmts
            |> List.find (fun s ->
                match s.Value with
                | CmmStmtCall _ -> true
                | _ -> false)

        let text = ASTPrinter.prettyPrintStmt callStmt
        Assert.Contains("StmtCall", text)
        Assert.Contains("args", text)
    | _ ->
        Assert.Fail("Unexpected AST shape")


[<Fact; Trait("TestSet", "student")>]
let ``Student prettyPrintExpr Leaf Nodes Extra Branch Test`` () =
    let input = """
int f(void) {
  return 0;
}

int main(void) {
  int x;
  int arr[3];
  x = 0;
  arr[0] = x;
  return f();
}
"""
    let ast = mustParse input

    match ast with
    | [
        { Value = CmmDeclFunc _ }
        { Value = CmmDeclFunc mainFunc }
      ] ->
        let stmts = mainFunc.Value.Body.Value.Stmts

        match stmts.[0].Value with
        | CmmStmtAssign { Value = CmmAssignVar (_, e) } ->
            let text = ASTPrinter.prettyPrintExpr e
            Assert.Contains("IntLit", text)
        | _ ->
            Assert.Fail("Expected x = 0")

        match stmts.[1].Value with
        | CmmStmtAssign { Value = CmmAssignArr (_, idx, rhs) } ->
            let idxText = ASTPrinter.prettyPrintExpr idx
            Assert.Contains("IntLit", idxText)

            let rhsText = ASTPrinter.prettyPrintExpr rhs
            Assert.Contains("Var", rhsText)
        | _ ->
            Assert.Fail("Expected arr[0] = x")

        match stmts.[2].Value with
        | CmmStmtReturn (Some e) ->
            let text = ASTPrinter.prettyPrintExpr e
            Assert.Contains("Call", text)
        | _ ->
            Assert.Fail("Expected return f()")
    | _ ->
        Assert.Fail("Unexpected AST shape")