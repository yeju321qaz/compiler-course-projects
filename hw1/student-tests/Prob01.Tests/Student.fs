module HW1.Prob01.StudentTests

open Xunit
open HW1.Prob01

[<Fact; Trait("TestSet", "student")>]
let ``celsiusToFahrenheit 0`` () =
    Assert.Equal(32.0, celsiusToFahrenheit 0.0)

[<Fact; Trait("TestSet", "student")>]
let ``celsiusToFahrenheit 10`` () =
    Assert.Equal(50.0, celsiusToFahrenheit 10.0)

[<Fact; Trait("TestSet", "student")>]
let ``celsiusToFahrenheit negative`` () =
    Assert.Equal(14.0, celsiusToFahrenheit -10.0)
