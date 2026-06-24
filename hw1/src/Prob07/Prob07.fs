module HW1.Prob07

type MyList<'T> =
    | Empty
    | Cons of 'T * MyList<'T>

let rec length (lst: MyList<'T>) : int =
  match lst with
  | Empty -> 0
  | Cons(_, tail) -> 1+length tail

let rec toFSharpList (lst: MyList<'T>) : 'T list =
  match lst with
  | Empty -> []
  | Cons (head, tail) -> head :: toFSharpList tail
  
let rec fromFSharpList (lst: 'T list) : MyList<'T> =
  match lst with
  | [] -> Empty
  | head :: tail -> Cons (head, fromFSharpList tail)

let rec append (lst1: MyList<'T>) (lst2: MyList<'T>) : MyList<'T> =
  match lst1 with
  | Empty -> lst2
  | Cons (head, tail) -> Cons (head, append tail lst2)

let rec reverse (lst: MyList<'T>) : MyList<'T> =
  match lst with
  | Empty -> Empty
  | Cons (head, tail) -> append (reverse tail) (Cons (head, Empty))

let rec fold (f: 'S -> 'T -> 'S) (acc: 'S) (lst: MyList<'T>) : 'S =
  match lst with
  | Empty -> acc
  | Cons (head, tail) -> fold f (f acc head) tail
  

let map (f: 'T -> 'U) (lst: MyList<'T>) : MyList<'U> =
  lst
  |> fold (fun acc x-> Cons (f x, acc)) Empty
  |> reverse
