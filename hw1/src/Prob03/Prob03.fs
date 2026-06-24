module HW1.Prob03

let rps (p1: string) (p2: string) : string =
  match (p1, p2) with
   | ("rock", "scissors") -> "p1"
   | ("scissors", "paper") -> "p1"
   | ("paper", "rock") -> "p1"
   | ("scissors", "rock") -> "p2"
   | ("paper", "scissors") -> "p2"
   | ("rock", "paper") -> "p2"
   | ("rock", "rock") -> "draw"
   | ("paper", "paper") -> "draw"
   | ("scissors", "scissors") -> "draw"
   | _ -> "invalid"
