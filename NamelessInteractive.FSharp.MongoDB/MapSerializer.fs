namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open NamelessInteractive.FSharp.MongoDB
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type MapSerializer<'Key, 'Value when 'Key: comparison>() =
    inherit SerializerBase<Map<'Key, 'Value>>()

    let serializer = DictionaryInterfaceImplementerSerializer<Dictionary<'Key, 'Value>>()

    override this.Serialize(context, args, value) =
        let dictValue =
            value
            |> Map.toSeq<'Key, 'Value>
            |> dict 
        serializer.Serialize(context, args, dictValue :?> Dictionary<'Key, 'Value>)

    override this.Deserialize(context, args) =
        serializer.Deserialize(context, args)
        |> Seq.map(|KeyValue|)
        |> Map.ofSeq<'Key,'Value>