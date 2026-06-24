module HW1.Prob06.StudentTests

open Xunit
open HW1.Prob06

[<Fact; Trait("TestSet", "student")>]
let ``depth leaf`` () =
    Assert.Equal(0, depth Leaf)

[<Fact; Trait("TestSet", "student")>]
let ``depth single node`` () =
    Assert.Equal(1, depth (Node (1, Leaf, Leaf)))

[<Fact; Trait("TestSet", "student")>]
let ``depth deeper left`` () =
    Assert.Equal(2, depth (Node (1, Node (2, Leaf, Leaf), Leaf)))

[<Fact; Trait("TestSet", "student")>]
let ``depth deeper right`` () =
    Assert.Equal(3, depth (Node (1, Leaf, Node (2, Leaf, Node (3, Leaf, Leaf)))))
