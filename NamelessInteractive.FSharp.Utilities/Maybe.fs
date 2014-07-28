module NamelessInteractive.FSharp.Utilities.Maybe

type MaybeBuilder() =
    
    member this.Bind(m,f) = Option.bind f m

    member this.Return(m) = Some(m)

let maybe = MaybeBuilder()
