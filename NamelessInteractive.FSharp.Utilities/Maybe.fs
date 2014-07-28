module NamelessInteractive.FSharp.Utilities.Maybe

type MaybeBuilder() =
    
    member this.Bind(f,m) = Option.bind f m

    member this.Return(m) = Some(m)

let maybe = MaybeBuilder()
