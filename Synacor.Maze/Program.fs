type NodeType =
    | Orb
    | L4
    | B4
    | T8
    | R18
    | M11
    | R1
    | B9

type Transition =
    | Add
    | Min
    | Mul

//type Node = {
//    NodeType: NodeType;
//    Transitions: seq<Transition * NodeType>;
//}

//type Graph = {
//    Nodes: seq<Node>;
//}

let graph =
    (seq {
        (Orb,
         [ (Add, L4)
           (Add, B4)
           (Min, B4)
           (Min, B9) ])

        (L4,
         [ (Mul, L4)
           (Add, L4)
           (Add, B4)
           (Mul, B4)
           (Mul, T8)
           (Mul, M11) ])

        (B4,
         [ (Mul, B4)
           (Add, B4)
           (Min, B4)
           (Mul, T8)
           (Mul, M11)
           (Add, L4)
           (Mul, L4)
           (Min, M11)
           (Min, R18)
           (Min, B9) ])

        (T8,
         [ (Mul, T8)
           (Min, T8)
           (Mul, L4)
           (Mul, B4)
           (Min, M11)
           (Mul, M11) ])

        (M11,
         [ (Min, M11)
           (Mul, M11)
           (Mul, T8)
           (Min, T8)
           (Min, R1)
           (Mul, R1)
           (Mul, R18)
           (Min, R18)
           (Mul, B4)
           (Min, B4)
           (Mul, L4)
           (Min, B9)])

        (R18,
         [ (Mul, R18)
           (Min, R18)
           (Mul, M11)
           (Min, M11)
           (Min, B4)
           (Mul, R1)
           (Min, B9)
           (Mul, B9)])

        (R1, [])

        (B9,
         [ (Min, B9)
           (Mul, B9)
           (Min, B4)
           (Min, R18)
           (Mul, R18) ])
     }
     |> Map)

let rec recurse transitions v (path: list<NodeType * Transition>) depth =
    match (depth, v) with
    | (6, _) -> () // Bail out after a while because graph is infinite
    | (_, x) when x < 0 -> () // Orb can't go negative
    | (_, x) when x > 100 -> () // Orb can't go too large (well it can but this prunes the graph)
    | _ ->
        transitions
        |> Seq.iter
            (fun (t, n) ->
                match (n, v) with
                | (R1, 30) -> printfn "%A" (path |> Seq.rev |> Seq.toList)
                | _ -> () |> ignore

                let nodeVal =
                    match n with
                    | L4 -> 4
                    | B4 -> 4
                    | T8 -> 8
                    | M11 -> 11
                    | R18 -> 18
                    | R1 -> 1
                    | Orb -> 22
                    | B9 -> 9

                let newVal =
                    match t with
                    | Add -> v + nodeVal
                    | Mul -> v * nodeVal
                    | Min -> v - nodeVal

                match (n, newVal) with
                | (R1, 30) -> printfn "%A" (path |> Seq.rev |> Seq.toList)
                | _ -> () |> ignore

                recurse (graph |> Map.find n) newVal ((n, t) :: path) (depth + 1))

let orb = (graph |> Map.find Orb)
recurse orb 22 [ (Orb, Add) ] 0 |> ignore
