module ECC.FrontEnd.Tests.Student

open Xunit
open ECC.IR
open ECC.FrontEnd

let private expectOk result =
    match result with
    | Ok v -> v
    | Error pos ->
        Assert.True(false, sprintf "Expected Ok, but got Error %A" pos)
        Unchecked.defaultof<_>

let private expectError result =
    match result with
    | Ok v ->
        Assert.True(false, sprintf "Expected Error, but got Ok %A" v)
    | Error _ ->
        Assert.True(true)

// ===== Basic public-style IR tests =====

[<Fact; Trait("TestSet", "student")>]
let ``student basic ir translation`` () =
    let input = """int main(void) { return 0; }"""

    let expected =
        Ok [
            StmtFuncStart (LblFunc "main")
            StmtAssign ("t1", ExprInt 0)
            StmtReturn (Some "t1")
            StmtFuncEnd (LblFunc "main")
        ]

    Assert.Equal(expected, EntryPoints.runString input)

[<Fact; Trait("TestSet", "student")>]
let ``student arithmetic return translation`` () =
    let input = """int main(void) { return 1 + 2; }"""

    let expected =
        Ok [
            StmtFuncStart (LblFunc "main")
            StmtAssign ("t1", ExprInt 1)
            StmtAssign ("t2", ExprInt 2)
            StmtAssign ("t3", ExprBinOp (BAdd, ExprVar "t1", ExprVar "t2"))
            StmtReturn (Some "t3")
            StmtFuncEnd (LblFunc "main")
        ]

    Assert.Equal(expected, EntryPoints.runString input)

[<Fact; Trait("TestSet", "student")>]
let ``student assignment translation`` () =
    let input = """int main(void) { int x; x = 3; return x; }"""

    let expected =
        Ok [
            StmtFuncStart (LblFunc "main")
            StmtAssign ("t1", ExprInt 3)
            StmtAssign ("x", ExprVar "t1")
            StmtReturn (Some "x")
            StmtFuncEnd (LblFunc "main")
        ]

    Assert.Equal(expected, EntryPoints.runString input)

// ===== IRTranslator statement/expression coverage =====

[<Fact; Trait("TestSet", "student")>]
let ``student relational if else translation`` () =
    let input = """int main(void) { if (1 == 1) return 1; else return 0; }"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtCJump ("t3", LblLocal "L1", LblLocal "L2"), ir)
    Assert.Contains(StmtLabel (LblLocal "L1"), ir)
    Assert.Contains(StmtLabel (LblLocal "L2"), ir)
    Assert.Contains(StmtLabel (LblLocal "L3"), ir)
    Assert.Contains(StmtReturn (Some "t4"), ir)
    Assert.Contains(StmtReturn (Some "t5"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student if without else ir translation`` () =
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

    let ir = EntryPoints.runString input |> expectOk
    let labels = ir |> List.choose (function StmtLabel l -> Some l | _ -> None)

    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)
    Assert.Equal(3, labels.Length)
    Assert.Contains(StmtReturn (Some "x"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student while translation`` () =
    let input = """
int main(void) {
    int x;
    x = 0;
    while (x < 3) {
        x = x + 1;
    }
    return x;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtLabel (LblLocal "L1"), ir)
    Assert.Contains(StmtCJump ("t3", LblLocal "L2", LblLocal "L3"), ir)
    Assert.Contains(StmtJump (LblLocal "L1"), ir)
    Assert.Contains(StmtLabel (LblLocal "L3"), ir)
    Assert.Contains(StmtReturn (Some "x"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student for loop ir translation`` () =
    let input = """
int main(void) {
    int x;
    for (x = 0; x < 3; x = x + 1) {
        ;
    }
    return x;
}
"""

    let ir = EntryPoints.runString input |> expectOk
    let labels = ir |> List.choose (function StmtLabel l -> Some l | _ -> None)

    Assert.True(labels.Length >= 3)
    Assert.Contains(StmtReturn (Some "x"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student for loop no init no cond no update ir translation`` () =
    let input = """
int main(void) {
    for (;;) {
        return 0;
    }
    return 1;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)
    Assert.True(ir |> List.exists (function StmtJump _ -> true | _ -> false))
    Assert.Contains(StmtFuncEnd (LblFunc "main"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student function call translation`` () =
    let input = """
int inc(int x) {
    return x + 1;
}

int main(void) {
    return inc(4);
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtFuncStart (LblFunc "inc"), ir)
    Assert.Contains(StmtFuncEnd (LblFunc "inc"), ir)
    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)
    Assert.Contains(StmtParam "t1", ir)
    Assert.Contains(StmtCall (LblFunc "inc", 1), ir)
    Assert.Contains(StmtRetrieve "t2", ir)
    Assert.Contains(StmtReturn (Some "t2"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student array store and load translation`` () =
    let input = """
int main(void) {
    int a[3];
    a[0] = 7;
    return a[0];
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtStore ("a", ExprVar "t1", ExprVar "t2"), ir)
    Assert.Contains(StmtAssign ("t4", ExprLoad ("a", ExprVar "t3")), ir)
    Assert.Contains(StmtReturn (Some "t4"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student void function return translation`` () =
    let input = """
void hello(void) {
    return;
}

int main(void) {
    hello();
    return 0;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtFuncStart (LblFunc "hello"), ir)
    Assert.Contains(StmtReturn None, ir)
    Assert.Contains(StmtCall (LblFunc "hello", 0), ir)
    Assert.Contains(StmtReturn (Some "t1"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student char literal ir translation`` () =
    let input = """
int main(void) {
    char c;
    c = 'a';
    return 0;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtAssign ("t1", ExprChar 'a'), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student string literal ir translation`` () =
    let input = """
void print(char s[4]) {
    return;
}

int main(void) {
    print("abc");
    return 0;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtFuncStart (LblFunc "print"), ir)
    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)
    Assert.Contains(StmtCall (LblFunc "print", 1), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student unary neg ir translation`` () =
    let input = """
int main(void) {
    int x;
    x = 1;
    return -x;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.True(ir |> List.exists (function
        | StmtAssign (_, ExprUnOp (UNeg, _)) -> true
        | _ -> false))

[<Fact; Trait("TestSet", "student")>]
let ``student unary not ir translation`` () =
    let input = """
int main(void) {
    if (!(1 == 1)) return 1;
    return 0;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.True(ir |> List.exists (function
        | StmtAssign (_, ExprUnOp (UNot, _)) -> true
        | _ -> false))

[<Fact; Trait("TestSet", "student")>]
let ``student nested block ir translation`` () =
    let input = """
int main(void) {
    int x;
    x = 1;
    {
        int y;
        y = 2;
        x = y;
    }
    return x;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)
    Assert.Contains(StmtReturn (Some "x"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student empty stmt ir translation`` () =
    let input = """
int main(void) {
    ;
    ;
    return 0;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtReturn (Some "t1"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student multiple relops ir translation`` () =
    let checkRelOp op =
        let input = sprintf "int main(void) { if (1 %s 1) return 1; return 0; }" op
        let ir = EntryPoints.runString input |> expectOk

        Assert.True(ir |> List.exists (function
            | StmtAssign (_, ExprRelOp _) -> true
            | _ -> false))

    checkRelOp "=="
    checkRelOp "!="
    checkRelOp "<"
    checkRelOp "<="
    checkRelOp ">"
    checkRelOp ">="

[<Fact; Trait("TestSet", "student")>]
let ``student multiple binops ir translation`` () =
    let checkBinOp op =
        let input = sprintf "int main(void) { return 1 %s 1; }" op
        let ir = EntryPoints.runString input |> expectOk

        Assert.True(ir |> List.exists (function
            | StmtAssign (_, ExprBinOp _) -> true
            | _ -> false))

    checkBinOp "+"
    checkBinOp "-"
    checkBinOp "*"
    checkBinOp "/"

[<Fact; Trait("TestSet", "student")>]
let ``student logical binops ir translation`` () =
    let checkLogicalOp op =
        let input = sprintf "int main(void) { if ((1 == 1) %s (2 == 2)) return 1; return 0; }" op
        let ir = EntryPoints.runString input |> expectOk

        Assert.True(ir |> List.exists (function
            | StmtAssign (_, ExprBinOp _) -> true
            | _ -> false))

    checkLogicalOp "&&"
    checkLogicalOp "||"

[<Fact; Trait("TestSet", "student")>]
let ``student multiple functions ir translation`` () =
    let input = """
int double(int x) {
    return x + x;
}

int triple(int x) {
    return x + x + x;
}

int main(void) {
    return double(3);
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.Contains(StmtFuncStart (LblFunc "double"), ir)
    Assert.Contains(StmtFuncStart (LblFunc "triple"), ir)
    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)

[<Fact; Trait("TestSet", "student")>]
let ``student global var decl is skipped in ir`` () =
    let input = """
int g;
char ch;

int main(void) {
    return 0;
}
"""

    let ir = EntryPoints.runString input |> expectOk

    Assert.False(ir |> List.exists (function StmtAssign ("g", _) -> true | _ -> false))
    Assert.False(ir |> List.exists (function StmtAssign ("ch", _) -> true | _ -> false))
    Assert.Contains(StmtFuncStart (LblFunc "main"), ir)

// ===== TypeChecker valid cases =====

[<Fact; Trait("TestSet", "student")>]
let ``student string literal type check ok`` () =
    let input = """
void print(char s[4]) {
    return;
}

int main(void) {
    print("abc");
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

[<Fact; Trait("TestSet", "student")>]
let ``student nested block scope ok`` () =
    let input = """
int main(void) {
    int x;
    x = 1;
    {
        int y;
        y = x;
        x = y;
    }
    return x;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

[<Fact; Trait("TestSet", "student")>]
let ``student nested block shadow variable ok`` () =
    let input = """
int main(void) {
    int x;
    x = 1;
    {
        int x;
        x = 2;
    }
    return x;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

[<Fact; Trait("TestSet", "student")>]
let ``student has value return in if both branches`` () =
    let input = """
int main(void) {
    int x;
    x = 0;
    if (x == 0) {
        return 1;
    } else {
        return 0;
    }
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

[<Fact; Trait("TestSet", "student")>]
let ``student has value return in nested block`` () =
    let input = """
int main(void) {
    {
        return 1;
    }
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

[<Fact; Trait("TestSet", "student")>]
let ``student value return inside while accepted`` () =
    let input = """
int main(void) {
    int x;
    x = 0;
    while (x < 1) {
        return x;
    }
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

[<Fact; Trait("TestSet", "student")>]
let ``student value return inside for accepted`` () =
    let input = """
int main(void) {
    int x;
    for (x = 0; x < 1; x = x + 1) {
        return x;
    }
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectOk
    |> ignore

// ===== TypeChecker error cases =====

[<Fact; Trait("TestSet", "student")>]
let ``student undeclared variable error`` () =
    let input = """int main(void) { return x; }"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student return type mismatch error`` () =
    let input = """int main(void) { return 'a'; }"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student assignment type mismatch error`` () =
    let input = """
int main(void) {
    int x;
    x = 'a';
    return x;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student duplicate local declaration error`` () =
    let input = """
int main(void) {
    int x;
    int x;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student duplicate global declaration error`` () =
    let input = """
int x;
char x;

int main(void) {
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student function argument count error`` () =
    let input = """
int add(int x, int y) {
    return x + y;
}

int main(void) {
    return add(1);
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student function argument type error`` () =
    let input = """
int id(int x) {
    return x;
}

int main(void) {
    return id('a');
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student array index type error`` () =
    let input = """
int main(void) {
    int a[3];
    return a['x'];
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student array assignment type error`` () =
    let input = """
int main(void) {
    int a[3];
    a[0] = 'x';
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student condition type error`` () =
    let input = """
int main(void) {
    if (1) return 1;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student while condition type error`` () =
    let input = """
int main(void) {
    int x;
    x = 0;
    while (x) {
        x = x + 1;
    }
    return x;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student for loop condition type error`` () =
    let input = """
int main(void) {
    int x;
    for (x = 0; x; x = x + 1) {
        ;
    }
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student char relational operator error`` () =
    let input = """
int main(void) {
    if ('a' == 'b') return 1;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student char arithmetic error`` () =
    let input = """
int main(void) {
    return 'a' + 'b';
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student neg operator on bool error`` () =
    let input = """
int main(void) {
    return -(1 == 1);
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student not operator on int error`` () =
    let input = """
int main(void) {
    if (!1) return 1;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student and operator on int error`` () =
    let input = """
int main(void) {
    if (1 && 0) return 1;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student assign to array variable error`` () =
    let input = """
int main(void) {
    int a[3];
    a = a;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student return void from int function error`` () =
    let input = """
int main(void) {
    return;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student return expr from void function error`` () =
    let input = """
void f(void) {
    return 1;
}

int main(void) {
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student void function used as expression error`` () =
    let input = """
void hello(int x) {
    return;
}

int main(void) {
    return hello(1);
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student non void function without value return error`` () =
    let input = """
int main(void) {
    int x;
    x = 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student function must be declared before call error`` () =
    let input = """
int main(void) {
    return foo(1);
}

int foo(int x) {
    return x;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student array parameter size mismatch error`` () =
    let input = """
int first(int a[4]) {
    return a[0];
}

int main(void) {
    int b[3];
    return first(b);
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student array assign index type error`` () =
    let input = """
int main(void) {
    int a[3];
    a[1 == 1] = 0;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student function name used as variable error`` () =
    let input = """
int f(void) {
    return 0;
}

int main(void) {
    return f;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student non array index access error`` () =
    let input = """
int main(void) {
    int x;
    x = 0;
    return x[0];
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student assign to undeclared variable error`` () =
    let input = """
int main(void) {
    x = 1;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student assign index to non array error`` () =
    let input = """
int main(void) {
    int x;
    x[0] = 1;
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student for init type mismatch error`` () =
    let input = """
int main(void) {
    int x;
    for (x = 'a'; x < 3; x = x + 1) {
        ;
    }
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError

[<Fact; Trait("TestSet", "student")>]
let ``student for update type mismatch error`` () =
    let input = """
int main(void) {
    int x;
    char c;
    c = 'a';
    for (x = 0; x < 3; x = c) {
        ;
    }
    return 0;
}
"""

    EntryPoints.analyzeSemanticsString input
    |> expectError