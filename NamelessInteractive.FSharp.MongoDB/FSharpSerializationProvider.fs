namespace NamelessInteractive.FSharp.MongoDB

open System
open MongoDB.Bson.Serialization
open NamelessInteractive.FSharp.MongoDB.Serializers

type FSharpTypeSerializationProvider() =
    let CreateInstance (objType:Type) = 
        Activator.CreateInstance(objType)
    let AsBsonSerializer (value: obj) =
        value :?> IBsonSerializer
    interface IBsonSerializationProvider with
        member this.GetSerializer(objType) =
            if IsOption objType then
                typedefof<OptionSerializer<_>>.MakeGenericType (objType.GetGenericArguments())
                |> CreateInstance
                |> AsBsonSerializer
            elif IsList objType then
                typedefof<ListSerializer<_>>.MakeGenericType (objType.GetGenericArguments())
                |> CreateInstance
                |> AsBsonSerializer
            elif IsMap objType then
                typedefof<MapSerializer<_,_>>.MakeGenericType(objType.GetGenericArguments())
                |> CreateInstance
                |> AsBsonSerializer
            elif IsSet objType then
                typedefof<SetSerializer<_>>.MakeGenericType(objType.GetGenericArguments())
                |> CreateInstance
                |> AsBsonSerializer
            elif IsUnion objType then
                typedefof<DiscriminatedUnionSerializer<_>>.MakeGenericType(objType)
                |> CreateInstance
                |> AsBsonSerializer
            elif IsRecord objType then
                typedefof<RecordSerializer<_>>.MakeGenericType(objType)
                |> CreateInstance
                |> AsBsonSerializer
            else
                null

module SerializationProviderModule = 
    let mutable private isRegistered = false

    let Register() = 
        if not isRegistered then
            isRegistered <- true
            BsonSerializer.RegisterSerializationProvider(FSharpTypeSerializationProvider())
        
