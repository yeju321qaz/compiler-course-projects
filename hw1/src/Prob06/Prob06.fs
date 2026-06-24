module HW1.Prob06

type Tree<'T> =
    | Leaf
    | Node of 'T * Tree<'T> * Tree<'T>

let rec depth (tree: Tree<'T>) : int =
  match tree with
  | Leaf ->0
  | Node (_, left, right) -> 1+ max (depth left) (depth right)
