module HW1.Prob05

let rec comb (n: int) (k: int) : int =
  if k=0 || k=n then 1
  else comb (n-1) (k-1) + comb (n-1) k
