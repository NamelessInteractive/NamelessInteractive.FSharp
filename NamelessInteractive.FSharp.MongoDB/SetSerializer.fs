namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type SetSerializer<'Type when 'Type: comparison>() =
    inherit SerializerBase<Set<'Type>>()

    let serializer = EnumerableInterfaceImplementerSerializer<List<'Type>>()

    override this.Serialize(context, args, value) =
        serializer.Serialize(context, args, new List<'Type>(value))

    override this.Deserialize(context, args) =
        let res = serializer.Deserialize(context, args)
        res |> unbox |> Set.ofSeq<'Type>

