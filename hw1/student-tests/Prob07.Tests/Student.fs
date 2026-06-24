module HW1.Prob07.StudentTests

open Xunit
open HW1.Prob07

[<Fact; Trait("TestSet", "student")>]
let ``length empty`` () =
    Assert.Equal(0, length Empty)

[<Fact; Trait("TestSet", "student")>]
let ``length nonempty`` () =
    let xs : MyList<int> = Cons (1, Cons (2, Cons (3, Empty)))
    Assert.Equal(3, length xs)

[<Fact; Trait("TestSet", "student")>]
let ``toFSharpList empty`` () =
    let expected : int list = []
    let actual : int list = toFSharpList Empty
    if expected <> actual then
        failwith "toFSharpList empty failed"

[<Fact; Trait("TestSet", "student")>]
let ``toFSharpList nonempty`` () =
    let xs : MyList<int> = Cons (1, Cons (2, Cons (3, Empty)))
    let expected : int list = [1; 2; 3]
    let actual : int list = toFSharpList xs
    if expected <> actual then
        failwith "toFSharpList nonempty failed"

[<Fact; Trait("TestSet", "student")>]
let ``fromFSharpList empty`` () =
    let expected : MyList<int> = Empty
    Assert.Equal(expected, fromFSharpList [])

[<Fact; Trait("TestSet", "student")>]
let ``fromFSharpList nonempty`` () =
    let expected : MyList<int> = Cons (1, Cons (2, Empty))
    Assert.Equal(expected, fromFSharpList [1; 2])

[<Fact; Trait("TestSet", "student")>]
let ``append two lists`` () =
    let expected : MyList<int> = Cons (1, Cons (2, Cons (3, Empty)))
    let actual = append (Cons (1, Cons (2, Empty))) (Cons (3, Empty))
    Assert.Equal(expected, actual)

[<Fact; Trait("TestSet", "student")>]
let ``reverse list`` () =
    let expected : MyList<int> = Cons (3, Cons (2, Cons (1, Empty)))
    let actual = reverse (Cons (1, Cons (2, Cons (3, Empty))))
    Assert.Equal(expected, actual)

[<Fact; Trait("TestSet", "student")>]
let ``fold sum`` () =
    let xs : MyList<int> = Cons (1, Cons (2, Cons (3, Empty)))
    Assert.Equal(6, fold (fun acc x -> acc + x) 0 xs)

[<Fact; Trait("TestSet", "student")>]
let ``map double`` () =
    let xs : MyList<int> = Cons (1, Cons (2, Cons (3, Empty)))
    let expected : MyList<int> = Cons (2, Cons (4, Cons (6, Empty)))
    Assert.Equal(expected, map (fun x -> x * 2) xs)
