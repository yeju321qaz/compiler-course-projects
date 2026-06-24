module HW1.Prob03.StudentTests

open Xunit
open HW1.Prob03

[<Fact; Trait("TestSet", "student")>]
let ``rps p1 wins`` () =
    Assert.Equal("p1", rps "rock" "scissors")

[<Fact; Trait("TestSet", "student")>]
let ``rps p2 wins`` () =
    Assert.Equal("p2", rps "rock" "paper")

[<Fact; Trait("TestSet", "student")>]
let ``rps draw`` () =
    Assert.Equal("draw", rps "scissors" "scissors")

[<Fact; Trait("TestSet", "student")>]
let ``rps invalid`` () =
    Assert.Equal("invalid", rps "banana" "rock")
