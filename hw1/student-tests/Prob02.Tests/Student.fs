module HW1.Prob02.StudentTests

open Xunit
open HW1.Prob02

[<Fact; Trait("TestSet", "student")>]
let ``abs positive`` () =
    Assert.Equal(5, abs 5)

[<Fact; Trait("TestSet", "student")>]
let ``abs negative`` () =
    Assert.Equal(5, abs -5)

[<Fact; Trait("TestSet", "student")>]
let ``abs zero`` () =
    Assert.Equal(0, abs 0)
