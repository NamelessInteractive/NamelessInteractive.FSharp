namespace NamelessInteractive.FSharp.MongoDB

open Microsoft.FSharp.Reflection
open System

[<AutoOpen>]
module internal Helpers = 
    let IsUnion objType = FSharpType.IsUnion objType

    let IsOption objType = IsUnion objType && 
                           objType.IsGenericType &&
                           objType.GetGenericTypeDefinition() = typedefof<_ option>

    let IsList objType = IsUnion objType &&
                         objType.IsGenericType &&
                         objType.GetGenericTypeDefinition() = typedefof<_ list>

    let IsMap (objType: Type) =
        objType.IsGenericType &&
        objType.GetGenericTypeDefinition() = typedefof<Map<_,_>>

    let IsSet (objType: Type) =
        objType.IsGenericType &&
        objType.GetGenericTypeDefinition() = typedefof<Set<_>>

    let GetUnionCases objType = 
        FSharpType.GetUnionCases(objType) 
        |> Seq.map(fun x -> (x.Name, x)) 
        |> dict

    let IsRecord (objType) =
        FSharpType.IsRecord(objType)

    let GetRecordFields objType = 
        FSharpType.GetRecordFields(objType)