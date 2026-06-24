module HW1.Prob04.StudentTests

open Xunit
open HW1.Prob04

[<Fact; Trait("TestSet", "student")>]
let ``calculate plus`` () =
    Assert.Equal(Some 8, calculate "+" 3 5)

[<Fact; Trait("TestSet", "student")>]
let ``calculate minus`` () =
    Assert.Equal(Some 2, calculate "-" 5 3)

[<Fact; Trait("TestSet", "student")>]
let ``calculate multiply`` () =
    Assert.Equal(Some 15, calculate "*" 3 5)

[<Fact; Trait("TestSet", "student")>]
let ``calculate divide`` () =
    Assert.Equal(Some 2, calculate "/" 6 3)

[<Fact; Trait("TestSet", "student")>]
let ``calculate divide by zero`` () =
    Assert.Equal(None, calculate "/" 6 0)

[<Fact; Trait("TestSet", "student")>]
let ``calculate invalid operator`` () =
    Assert.Equal(None, calculate "%" 6 3)
