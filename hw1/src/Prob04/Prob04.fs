module HW1.Prob04

let calculate (op: string) (a: int) (b: int) : int option =
  match op with
  | "+" -> Some (a+b)
  | "-" -> Some (a-b)
  | "*" -> Some (a*b)
  | "/" ->
    if b=0 then None
    else Some (a/b)
  | _ -> None
