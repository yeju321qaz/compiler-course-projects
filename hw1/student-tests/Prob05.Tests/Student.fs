module HW1.Prob05.StudentTests

open Xunit
open HW1.Prob05

[<Fact; Trait("TestSet", "student")>]
let ``comb n 0`` () =
    Assert.Equal(1, comb 5 0)

[<Fact; Trait("TestSet", "student")>]
let ``comb n n`` () =
    Assert.Equal(1, comb 5 5)

[<Fact; Trait("TestSet", "student")>]
let ``comb 5 2`` () =
    Assert.Equal(10, comb 5 2)

[<Fact; Trait("TestSet", "student")>]
let ``comb 6 3`` () =
    Assert.Equal(20, comb 6 3)
