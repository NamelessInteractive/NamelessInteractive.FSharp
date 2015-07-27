namespace NamelessInteractive.FSharp.MongoDB.Conventions

open NamelessInteractive.FSharp.MongoDB
open MongoDB.Bson.Serialization.Conventions

type OptionConvention() =
    inherit ConventionBase("F# Option Type")

    interface IMemberMapConvention with
        member this.Apply(memberMap) =
            let objType = memberMap.MemberType
            if IsOption objType then
                memberMap.SetDefaultValue None |> ignore
                memberMap.SetIgnoreIfNull true |> ignore

type RecordConvention() =
    inherit ConventionBase("F# Record Type")

    interface IClassMapConvention with
        member this.Apply(classMap) =
            let objType = classMap.ClassType

            if IsRecord objType then
                classMap.SetIgnoreExtraElements(true)
                let fields = GetRecordFields objType
                let names = fields |> Array.map (fun x -> x.Name)
                let types = fields |> Array.map (fun x -> x.PropertyType)

                let ctor = objType.GetConstructor(types)
            
                classMap.MapConstructor(ctor, names) |> ignore
                fields |> Array.iter (fun x -> classMap.MapMember(x) |> ignore)

module ConventionsModule = 
    let mutable private isRegistered = false
    let Register() = 
        if not isRegistered then
            isRegistered <- true
            let pack = ConventionPack()
            pack.Add(RecordConvention())
            pack.Add(OptionConvention())
            ConventionRegistry.Register("F# Type Conventions", pack, (fun _ -> true))