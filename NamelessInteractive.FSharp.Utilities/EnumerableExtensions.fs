module NamelessInteractive.FSharp.Utilities.EnumerableExtensions

open System.Collections.Generic

type IEnumerable<'T> with
    member this.IsEmpty() =
        this |> Seq.isEmpty
    
    member this.HeadOrDefault() =
        match this.IsEmpty() with
        | true -> None
        | false -> this |> Seq.head |> Some